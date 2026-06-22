using System;
using ThreeKingdom.Domain.Cohesion;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 部队凝聚力展示模型（P6 多维不合并 / ADR-0002）。
    /// 士气/疲劳/军纪<b>三维各为独立字段</b>，本类型<b>刻意不含</b>任何单一综合/总分值
    /// （负向不变量由 PresentationLockTests 反射断言）。展示用 decimal 为非权威值（ADR-0004）。不可变。
    /// </summary>
    public sealed class CohesionView
    {
        /// <summary>单位标签。</summary>
        public string UnitLabel { get; }

        /// <summary>人数。</summary>
        public int Headcount { get; }

        /// <summary>士气 unit_morale 展示值 [0,1]（独立维度）。</summary>
        public decimal Morale { get; }

        /// <summary>疲劳展示值 [0,1]（独立维度）。</summary>
        public decimal Fatigue { get; }

        /// <summary>军纪展示值 [0,1]（独立维度）。</summary>
        public decimal Discipline { get; }

        private CohesionView(string unitLabel, int headcount, decimal morale, decimal fatigue, decimal discipline)
        {
            UnitLabel = unitLabel;
            Headcount = headcount;
            Morale = morale;
            Fatigue = fatigue;
            Discipline = discipline;
        }

        /// <summary>从凝聚力状态构造（三维分列，不合并）。</summary>
        public static CohesionView FromState(CohesionState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new CohesionView(
                state.Unit.ToString(),
                state.Headcount,
                Display.ToDecimal(state.Morale),
                Display.ToDecimal(state.Fatigue),
                Display.ToDecimal(state.Discipline));
        }
    }
}
