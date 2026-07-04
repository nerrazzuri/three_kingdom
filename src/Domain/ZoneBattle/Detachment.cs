using System;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 支队（GDD_021 R2：可部署单位 = 将 + 兵种 + 兵力 + 姿态，所在某区）。不可变；回合/命令产出新实例。
    /// 调动中（<see cref="InTransit"/>）本回合不参与任一区结算（失位代价，ADR-0012 D3）。
    /// </summary>
    public sealed class Detachment
    {
        /// <summary>支队 id。</summary>
        public DetachmentId Id { get; }
        /// <summary>所属阵营。</summary>
        public BattleSide Side { get; }
        /// <summary>统领将领（可空——无将支队）。</summary>
        public OffensiveGeneral? General { get; }
        /// <summary>兵种编成（份额=条件门/契合输入；总数 ≤ 兵力）。</summary>
        public TroopComposition Composition { get; }
        /// <summary>兵力（战中减员递减）。</summary>
        public int Strength { get; }
        /// <summary>士气（GDD_011，定点 [0,1]）。</summary>
        public FixedPoint Morale { get; }
        /// <summary>疲劳（GDD_011，定点 [0,1]）。</summary>
        public FixedPoint Fatigue { get; }
        /// <summary>姿态。</summary>
        public Posture Posture { get; }
        /// <summary>当前所在区。</summary>
        public ZoneId Location { get; }
        /// <summary>调动目标区（在途时非空）。</summary>
        public ZoneId? TransitTarget { get; }
        /// <summary>调动剩余在途回合（0=已到位）。</summary>
        public int TransitRemaining { get; }

        public Detachment(
            DetachmentId id, BattleSide side, OffensiveGeneral? general, TroopComposition composition,
            int strength, FixedPoint morale, FixedPoint fatigue, Posture posture, ZoneId location,
            ZoneId? transitTarget = null, int transitRemaining = 0)
        {
            if (id.Value is null) throw new ArgumentException("Detachment.Id 不可为空。", nameof(id));
            if (strength < 0) throw new ArgumentOutOfRangeException(nameof(strength), "兵力不可为负。");
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            if (composition.Total > strength) throw new ArgumentException("兵种编成总数不可超过兵力。", nameof(composition));
            if (location.Value is null) throw new ArgumentException("Detachment.Location 不可为空。", nameof(location));
            if (transitRemaining < 0) throw new ArgumentOutOfRangeException(nameof(transitRemaining));
            Require01(morale, nameof(morale));
            Require01(fatigue, nameof(fatigue));

            Id = id;
            Side = side;
            General = general;
            Composition = composition;
            Strength = strength;
            Morale = morale;
            Fatigue = fatigue;
            Posture = posture;
            Location = location;
            TransitTarget = transitTarget;
            TransitRemaining = transitRemaining;
        }

        /// <summary>是否调动在途（本回合失位，不参与结算）。</summary>
        public bool InTransit => TransitTarget != null && TransitRemaining > 0;

        /// <summary>是否已被打散（兵力 0）。</summary>
        public bool IsBroken => Strength <= 0;

        private Detachment With(
            int? strength = null, FixedPoint? morale = null, FixedPoint? fatigue = null, Posture? posture = null,
            ZoneId? location = null, bool clearTransit = false, ZoneId? transitTarget = null, int? transitRemaining = null)
            => new Detachment(
                Id, Side, General, Composition,
                strength ?? Strength, morale ?? Morale, fatigue ?? Fatigue, posture ?? Posture,
                location ?? Location,
                clearTransit ? (ZoneId?)null : (transitTarget ?? TransitTarget),
                clearTransit ? 0 : (transitRemaining ?? TransitRemaining));

        /// <summary>发起调往某区（在途 <paramref name="transit"/> 回合；到位由回合推进递减）。</summary>
        public Detachment MoveTo(ZoneId target, int transit)
            => With(transitTarget: target, transitRemaining: transit);

        /// <summary>回合推进：在途递减；归零则落位目标区、清在途。</summary>
        public Detachment AdvanceTransit()
        {
            if (!InTransit) return this;
            int rem = TransitRemaining - 1;
            if (rem <= 0) return With(location: TransitTarget, clearTransit: true);
            return With(transitRemaining: rem);
        }

        /// <summary>改姿态。</summary>
        public Detachment WithPosture(Posture posture) => With(posture: posture);

        /// <summary>减员 + 士气/疲劳更新（结算写回）；兵种编成按新兵力比例缩放（份额近似保持）。</summary>
        public Detachment WithCombat(int strength, FixedPoint morale, FixedPoint fatigue)
        {
            int s = Math.Max(0, strength);
            return new Detachment(
                Id, Side, General, Composition.ScaledTo(s), s,
                morale.Clamp(FixedPoint.Zero, FixedPoint.One), fatigue.Clamp(FixedPoint.Zero, FixedPoint.One),
                Posture, Location, TransitTarget, TransitRemaining);
        }

        internal void AppendTo(StateHasher hasher)
        {
            ZoneHashing.AppendString(hasher, Id.Value);
            hasher.Append((int)Side);
            hasher.Append(General != null);
            if (General != null)
            {
                ZoneHashing.AppendString(hasher, General.Character.Value);
                hasher.Append(General.Command).Append(General.Valor).Append(General.Guile).Append((int)General.Specialty);
            }
            // 兵种编成（按枚举序遍历四类，缺省 0）——规范确定性。
            hasher.Append(Composition.Count(TroopType.Infantry));
            hasher.Append(Composition.Count(TroopType.Cavalry));
            hasher.Append(Composition.Count(TroopType.Archer));
            hasher.Append(Composition.Count(TroopType.Marine));
            hasher.Append(Strength).Append(Morale).Append(Fatigue).Append((int)Posture);
            ZoneHashing.AppendString(hasher, Location.Value);
            hasher.Append(TransitTarget != null);
            if (TransitTarget != null) ZoneHashing.AppendString(hasher, TransitTarget.Value.Value);
            hasher.Append(TransitRemaining);
        }

        private static void Require01(FixedPoint v, string name)
        {
            if (v < FixedPoint.Zero || v > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "须在 [0,1]。");
        }
    }
}
