using System.Collections.Generic;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 日界结算阶段（GDD_001 / systems-index §跨系统结算顺序）。
    /// 时间推进后，各阶段按 <see cref="DayBoundaryStages.CanonicalOrder"/> 固定全局顺序结算：
    /// 环境 → 补给 → 城市 → 状态事件。枚举序数即该固定顺序。
    /// </summary>
    public enum DayBoundaryStage
    {
        /// <summary>环境（天气/风向派生修正）。</summary>
        Environment = 0,

        /// <summary>补给（后勤消耗/产出结算）。</summary>
        Supply = 1,

        /// <summary>城市（经济/生产结算）。</summary>
        City = 2,

        /// <summary>状态事件（士气/疲劳等状态派生事件）。</summary>
        StateEvents = 3,
    }

    /// <summary>日界结算阶段的权威固定顺序。</summary>
    public static class DayBoundaryStages
    {
        /// <summary>规范结算顺序：环境 → 补给 → 城市 → 状态事件（不可变，跨日复用同一实例）。</summary>
        public static readonly IReadOnlyList<DayBoundaryStage> CanonicalOrder = new[]
        {
            DayBoundaryStage.Environment,
            DayBoundaryStage.Supply,
            DayBoundaryStage.City,
            DayBoundaryStage.StateEvents,
        };
    }
}
