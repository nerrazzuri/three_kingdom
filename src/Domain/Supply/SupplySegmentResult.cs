namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 单时段补给结算结果（GDD_012 §Formula 2/5）。
    /// 含结算后补给链状态、本时段消耗/短缺、派生补给状态等级，以及（达宽限期时）
    /// 发出的断粮后果事件。事件为意图通知，士气/疲劳由 GDD_011 消费（单一权威）。
    /// </summary>
    public sealed class SupplySegmentResult
    {
        /// <summary>结算后补给链状态。</summary>
        public SupplyChainState State { get; }

        /// <summary>本时段实际消耗量。</summary>
        public long Consumed { get; }

        /// <summary>本时段短缺量（需求 − 实际可得，≥0）。</summary>
        public long Shortage { get; }

        /// <summary>派生补给状态等级（充足/紧张/断粮）。</summary>
        public SupplyStatusLevel Status { get; }

        /// <summary>断粮后果事件；仅当短缺累计达宽限期时非空，否则为 null（渐进，不立即触发）。</summary>
        public SupplyCutoffEvent? CutoffEvent { get; }

        public SupplySegmentResult(SupplyChainState state, long consumed, long shortage, SupplyStatusLevel status, SupplyCutoffEvent? cutoffEvent)
        {
            State = state;
            Consumed = consumed;
            Shortage = shortage;
            Status = status;
            CutoffEvent = cutoffEvent;
        }
    }
}
