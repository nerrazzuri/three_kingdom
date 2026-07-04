using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>治理事务类别（GDD_004；太守派人处理，需时完成）。</summary>
    public enum GovernanceActionKind
    {
        /// <summary>征用军粮。</summary>
        Requisition = 0,
        /// <summary>修缮城防。</summary>
        RepairFortification = 1,
        /// <summary>安抚民心。</summary>
        Appease = 2,
    }

    /// <summary>
    /// 一件在办的治理事务（GDD_004，<b>非即时</b>）。派出时记录类别/参数/预计完成时刻；
    /// 推进到 <see cref="CompletionTime"/> 后由 <see cref="CampaignSessionService"/> 应用其效果——
    /// 太守下令 → 派人办理 → 需时见效。不可变。
    /// </summary>
    public sealed class PendingGovernanceTask
    {
        /// <summary>事务类别。</summary>
        public GovernanceActionKind Kind { get; }

        /// <summary>参数（征用量；其余类别为 0）。</summary>
        public long Amount { get; }

        /// <summary>预计完成时刻（下令时刻 + 办理时段）。</summary>
        public WorldTime CompletionTime { get; }

        public PendingGovernanceTask(GovernanceActionKind kind, long amount, WorldTime completionTime)
        {
            Kind = kind;
            Amount = amount;
            CompletionTime = completionTime;
        }
    }
}
