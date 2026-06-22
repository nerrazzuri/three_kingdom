using System.Collections.Generic;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市日界结算阶段（GDD_004 §Main Rules 稳定顺序 / TR-city-002）。
    /// 枚举序数即权威固定顺序：承诺 → 产入 → 消耗 → 短缺后果 → 工事/治安。
    /// 该顺序确定性可复盘，账本逐阶段记录（ADR-0004）。
    /// </summary>
    public enum CityDaySettlementStage
    {
        /// <summary>承诺：执行已保留的军粮征用，移交后勤（所有权转移，守恒）。</summary>
        Commit = 0,

        /// <summary>产入：基础日产入入库。</summary>
        Yield = 1,

        /// <summary>消耗：民用消耗（夹至库存下限）。</summary>
        Consume = 2,

        /// <summary>短缺后果：未满足的民用需求转化为民心损耗与骚乱风险。</summary>
        ShortageConsequence = 3,

        /// <summary>工事/治安：工事修复与治安结算（不触动粮食库存）。</summary>
        FortificationSecurity = 4,
    }

    /// <summary>城市日界结算阶段的权威固定顺序。</summary>
    public static class CityDaySettlementStages
    {
        /// <summary>规范结算顺序：承诺 → 产入 → 消耗 → 短缺后果 → 工事/治安（不可变）。</summary>
        public static readonly IReadOnlyList<CityDaySettlementStage> CanonicalOrder = new[]
        {
            CityDaySettlementStage.Commit,
            CityDaySettlementStage.Yield,
            CityDaySettlementStage.Consume,
            CityDaySettlementStage.ShortageConsequence,
            CityDaySettlementStage.FortificationSecurity,
        };
    }
}
