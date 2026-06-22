using System;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 战役单位权威状态（GDD_010 §Data Model：BattleUnitState / ADR-0004）。
    /// 不可变。兵力为权威整数；士气/疲劳/军纪/修正为定点（[0,1] 或乘数），权威路径无 float。
    /// <para>
    /// 士气/疲劳/军纪是 GDD_011 已结算值的<b>只读副本</b>（GDD_010 §8：本系统只读不再施加，
    /// 杜绝重复扣减）。<see cref="CombatMath.CombatPower"/> 据此派生有效战斗力。
    /// </para>
    /// </summary>
    public sealed class BattleUnitState
    {
        /// <summary>单位 ID。</summary>
        public BattleUnitId Id { get; }

        /// <summary>所属阵营。</summary>
        public FactionId Faction { get; }

        /// <summary>所在区域（GDD_003）。</summary>
        public RegionId Region { get; }

        /// <summary>当前兵力（权威整数，≥0）。</summary>
        public int Force { get; }

        /// <summary>士气（[0,1]，读自 GDD_011）。</summary>
        public FixedPoint Morale { get; }

        /// <summary>疲劳（[0,1]，读自 GDD_011）。</summary>
        public FixedPoint Fatigue { get; }

        /// <summary>军纪（[0,1]，读自 GDD_011）。</summary>
        public FixedPoint Discipline { get; }

        /// <summary>地形修正（乘数，≥0）。</summary>
        public FixedPoint TerrainMod { get; }

        /// <summary>姿态修正（乘数，≥0）。</summary>
        public FixedPoint PostureMod { get; }

        /// <summary>友邻支援系数（≥0）。</summary>
        public FixedPoint Support { get; }

        public BattleUnitState(
            BattleUnitId id, FactionId faction, RegionId region, int force,
            FixedPoint morale, FixedPoint fatigue, FixedPoint discipline,
            FixedPoint terrainMod, FixedPoint postureMod, FixedPoint support)
        {
            if (force < 0) throw new ArgumentOutOfRangeException(nameof(force), "兵力不可为负。");
            RequireUnit(morale, nameof(morale));
            RequireUnit(fatigue, nameof(fatigue));
            RequireUnit(discipline, nameof(discipline));
            RequireNonNegative(terrainMod, nameof(terrainMod));
            RequireNonNegative(postureMod, nameof(postureMod));
            RequireNonNegative(support, nameof(support));

            Id = id; Faction = faction; Region = region; Force = force;
            Morale = morale; Fatigue = fatigue; Discipline = discipline;
            TerrainMod = terrainMod; PostureMod = postureMod; Support = support;
        }

        /// <summary>替换兵力后的新实例。</summary>
        public BattleUnitState WithForce(int force)
            => new BattleUnitState(Id, Faction, Region, force, Morale, Fatigue, Discipline, TerrainMod, PostureMod, Support);

        /// <summary>替换区域后的新实例。</summary>
        public BattleUnitState WithRegion(RegionId region)
            => new BattleUnitState(Id, Faction, region, Force, Morale, Fatigue, Discipline, TerrainMod, PostureMod, Support);

        private static void RequireUnit(FixedPoint v, string n)
        { if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(n, "须在 [0,1]。"); }
        private static void RequireNonNegative(FixedPoint v, string n)
        { if (v < FixedPoint.Zero) throw new ArgumentOutOfRangeException(n, "不可为负。"); }
    }
}
