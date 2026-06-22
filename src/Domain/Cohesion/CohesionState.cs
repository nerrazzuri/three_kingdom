using System;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;

namespace ThreeKingdom.Domain.Cohesion
{
    /// <summary>
    /// 部队凝聚力状态（GDD_011 §Data Model：CohesionState / TR-cohesion-001 / ADR-0004）。
    /// <b>士气、疲劳、军纪三维独立保存、不得合并为单一数值</b>（P6）。各为定点 [0,1]，权威路径无 float。
    /// 不可变。<see cref="Headcount"/> 用于拆分/合并的人数加权（GDD_011 §Formula 5，非取最大）。
    /// <para>本系统 morale 指<b>部队士气 unit_morale</b>，与 GDD_004 城市民心 civ_morale 是不同状态（C-W1）。</para>
    /// </summary>
    public sealed class CohesionState
    {
        /// <summary>单位 ID。</summary>
        public UnitId Unit { get; }

        /// <summary>人数（≥0，拆分/合并加权用）。</summary>
        public int Headcount { get; }

        /// <summary>士气 unit_morale（继续战斗意愿，[0,1]）。</summary>
        public FixedPoint Morale { get; }

        /// <summary>疲劳（身体/组织负荷，[0,1]）。</summary>
        public FixedPoint Fatigue { get; }

        /// <summary>军纪（压力下维持队形能力，[0,1]）。</summary>
        public FixedPoint Discipline { get; }

        public CohesionState(UnitId unit, int headcount, FixedPoint morale, FixedPoint fatigue, FixedPoint discipline)
        {
            if (headcount < 0) throw new ArgumentOutOfRangeException(nameof(headcount), "人数不可为负。");
            RequireUnit(morale, nameof(morale));
            RequireUnit(fatigue, nameof(fatigue));
            RequireUnit(discipline, nameof(discipline));
            Unit = unit; Headcount = headcount; Morale = morale; Fatigue = fatigue; Discipline = discipline;
        }

        /// <summary>替换士气后的新实例（疲劳/军纪不变——三维独立）。</summary>
        public CohesionState WithMorale(FixedPoint morale) => new CohesionState(Unit, Headcount, morale, Fatigue, Discipline);

        /// <summary>替换疲劳后的新实例（士气/军纪不变——三维独立）。</summary>
        public CohesionState WithFatigue(FixedPoint fatigue) => new CohesionState(Unit, Headcount, Morale, fatigue, Discipline);

        /// <summary>替换军纪后的新实例（士气/疲劳不变——三维独立）。</summary>
        public CohesionState WithDiscipline(FixedPoint discipline) => new CohesionState(Unit, Headcount, Morale, Fatigue, discipline);

        private static void RequireUnit(FixedPoint v, string n)
        { if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(n, "须在 [0,1]。"); }
    }
}
