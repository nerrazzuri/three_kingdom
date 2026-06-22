using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 一次军议产出的建议集（GDD_008 §Data Model：CouncilSession 输出 / TR-council-001）。
    /// 绑定召开时的知识快照 ID；建议<b>并列</b>呈现，<b>无</b>全局排名或「最佳方案」标记（§Formula 3）。
    /// 当前知识快照变化后经 <see cref="IsStaleAgainst"/> 判定过时，<b>不</b>静默重算（§Formula 4）。不可变。
    /// </summary>
    public sealed class CouncilAdviceSet
    {
        /// <summary>召开时冻结的知识快照 ID。</summary>
        public KnowledgeSnapshotId SnapshotId { get; }

        /// <summary>建议（并列，无优劣排序）。</summary>
        public IReadOnlyList<AdviceStatement> Advice { get; }

        public CouncilAdviceSet(KnowledgeSnapshotId snapshotId, IReadOnlyList<AdviceStatement> advice)
        {
            SnapshotId = snapshotId;
            Advice = advice ?? throw new ArgumentNullException(nameof(advice));
        }

        /// <summary>
        /// 是否相对当前知识快照已过时（GDD_008 §Formula 4）：快照 ID 不一致即过时。
        /// 过时仅标记、不静默更新——玩家须重新召开军议。
        /// </summary>
        public bool IsStaleAgainst(KnowledgeSnapshotId currentSnapshotId) => SnapshotId != currentSnapshotId;
    }
}
