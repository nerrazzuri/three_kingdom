using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Talent
{
    /// <summary>人才稳定 ID（GDD_020）。序数比较，非空。</summary>
    public readonly struct TalentId : IEquatable<TalentId>
    {
        public string Value { get; }
        public TalentId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("TalentId 不可为空。", nameof(value));
            Value = value;
        }
        public bool Equals(TalentId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TalentId o && Equals(o);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(TalentId a, TalentId b) => a.Equals(b);
        public static bool operator !=(TalentId a, TalentId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>知晓渠道（GDD_020 R2 反全知：经此才进入玩家视野）。</summary>
    public enum TalentChannel
    {
        Scouting = 0,          // 侦察
        Council = 1,           // 军师举荐
        RetinueNetwork = 2,    // 部曲人脉
        HistoricalEvent = 3,   // 历史事件带出
    }

    /// <summary>招揽判定（GDD_020 R3）。</summary>
    public enum RecruitmentVerdict
    {
        Joined = 0,     // 出仕
        Declined = 1,   // 婉拒（可再请）
    }

    /// <summary>
    /// 人才档案（GDD_020 §6）：为将属性（复用出征/区域战斗 <see cref="OffensiveGeneral"/>）+ 出仕志向/阻力 + 登场时间窗。不可变。
    /// 属性 Q16.16 ∈[0,1]（ADR-0004）。
    /// </summary>
    public sealed class TalentProfile
    {
        public TalentId Id { get; }
        public CharacterId Character { get; }
        public FixedPoint Command { get; }
        public FixedPoint Valor { get; }
        public FixedPoint Guile { get; }
        public GeneralSpecialty Specialty { get; }
        /// <summary>出仕志向（越高越易招）。</summary>
        public FixedPoint Willingness { get; }
        /// <summary>招揽阻力（越高越难招，如择主而事、隐居之志）。</summary>
        public FixedPoint Reluctance { get; }
        /// <summary>登场起始世界时间（未到不存在）。</summary>
        public WorldTime AppearFrom { get; }
        /// <summary>登场终止（可空=不设上限）。</summary>
        public WorldTime? AppearUntil { get; }

        public TalentProfile(
            TalentId id, CharacterId character, FixedPoint command, FixedPoint valor, FixedPoint guile,
            GeneralSpecialty specialty, FixedPoint willingness, FixedPoint reluctance,
            WorldTime appearFrom, WorldTime? appearUntil = null)
        {
            Require01(command, nameof(command));
            Require01(valor, nameof(valor));
            Require01(guile, nameof(guile));
            Require01(willingness, nameof(willingness));
            Require01(reluctance, nameof(reluctance));
            if (appearUntil.HasValue && appearUntil.Value.AbsoluteIndex < appearFrom.AbsoluteIndex)
                throw new ArgumentException("登场终止早于起始。", nameof(appearUntil));
            Id = id;
            Character = character;
            Command = command;
            Valor = valor;
            Guile = guile;
            Specialty = specialty;
            Willingness = willingness;
            Reluctance = reluctance;
            AppearFrom = appearFrom;
            AppearUntil = appearUntil;
        }

        /// <summary>某世界时间是否已登场（F1 登场门）。</summary>
        public bool AppearedAt(WorldTime t)
            => t.AbsoluteIndex >= AppearFrom.AbsoluteIndex
               && (!AppearUntil.HasValue || t.AbsoluteIndex <= AppearUntil.Value.AbsoluteIndex);

        /// <summary>为将（复用出征/区域战斗将领属性）。</summary>
        public OffensiveGeneral ToGeneral()
            => new OffensiveGeneral(Character, Command, Valor, Guile, Specialty);

        private static void Require01(FixedPoint v, string name)
        {
            if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(name, "须在 [0,1]。");
        }
    }

    /// <summary>人才目录（GDD_020，数据驱动）：全体人才档案 + 按 id 查 + 按世界时间取已登场者。不可变。</summary>
    public sealed class TalentRoster
    {
        private readonly List<TalentProfile> _all;
        private readonly Dictionary<string, TalentProfile> _byId;

        public IReadOnlyList<TalentProfile> All => _all;

        public TalentRoster(IReadOnlyList<TalentProfile> talents)
        {
            _all = new List<TalentProfile>(talents ?? Array.Empty<TalentProfile>());
            _all.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));
            _byId = new Dictionary<string, TalentProfile>(StringComparer.Ordinal);
            foreach (TalentProfile t in _all)
            {
                if (_byId.ContainsKey(t.Id.Value)) throw new ArgumentException($"人才 id 重复：{t.Id.Value}");
                _byId[t.Id.Value] = t;
            }
        }

        public TalentProfile? Find(TalentId id) => _byId.TryGetValue(id.Value ?? "", out TalentProfile? t) ? t : null;

        /// <summary>某世界时间已登场的人才（F1 时间窗门，规范序）。</summary>
        public IReadOnlyList<TalentProfile> Appeared(WorldTime t)
        {
            var list = new List<TalentProfile>();
            foreach (TalentProfile p in _all) if (p.AppearedAt(t)) list.Add(p);
            return list;
        }
    }
}
