namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 外交兑现结果（GDD_012 §8.3 / §8.5 / §Failure Cases）。
    /// <see cref="DiplomacyService.Resolve"/> 的确定性输出：是否兑现、可解释原因、
    /// 已兑付代价（接受即扣，不凭空返还）、背约/违约的声誉惩罚（写回 GDD_006）。不可变。
    /// </summary>
    public sealed class DiplomaticOutcome
    {
        /// <summary>是否兑现交付。</summary>
        public bool Fulfilled { get; }

        /// <summary>结果原因（可解释，非黑箱）。</summary>
        public DiplomaticOutcomeReason Reason { get; }

        /// <summary>已兑付代价（接受即扣，背约也不凭空返还，GDD_012 §8.5）。</summary>
        public long CostPaid { get; }

        /// <summary>声誉惩罚量（背约或玩家违约时 &gt;0，写回 GDD_006 声望/问责）。</summary>
        public int ReputationPenalty { get; }

        public DiplomaticOutcome(bool fulfilled, DiplomaticOutcomeReason reason, long costPaid, int reputationPenalty)
        {
            Fulfilled = fulfilled;
            Reason = reason;
            CostPaid = costPaid;
            ReputationPenalty = reputationPenalty;
        }
    }
}
