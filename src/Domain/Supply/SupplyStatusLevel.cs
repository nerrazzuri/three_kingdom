namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 单位补给状态等级（GDD_012 §Data Model：SupplyStatus 充足/紧张/短缺）。
    /// 由短缺累计时段派生，逐时段恶化（渐进，非即时崩溃，TR-supply-002）。
    /// </summary>
    public enum SupplyStatusLevel
    {
        /// <summary>充足：本时段需求被满足，无短缺累计。</summary>
        Sufficient = 0,

        /// <summary>紧张：出现短缺但未达宽限期，尚未发断粮后果事件。</summary>
        Strained = 1,

        /// <summary>断粮：短缺累计达宽限期，发出断粮后果事件（交 GDD_011 结算）。</summary>
        Cutoff = 2,
    }
}
