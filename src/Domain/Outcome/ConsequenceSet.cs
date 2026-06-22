using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 一次战果的跨系统变更集合（gdd-010 §后果 / systems-index「后果结算」契约）。
    /// 不可变：携带 <see cref="OutcomeBranch"/> 分支与有序的变更意图列表（人物/关系/城市/名声）。
    /// 本身<b>不</b>写回任何状态——由 <see cref="OutcomeWritebackService"/> 全量校验通过后统一原子写回。
    /// <para>
    /// 确定性：变更按列表顺序参与校验与哈希；同一战果应产出同一变更集（由结算上游保证）。
    /// </para>
    /// </summary>
    public sealed class ConsequenceSet
    {
        /// <summary>战果分支。</summary>
        public OutcomeBranch Branch { get; }

        /// <summary>有序变更意图（只读）。</summary>
        public IReadOnlyList<OutcomeChange> Changes { get; }

        public ConsequenceSet(OutcomeBranch branch, IEnumerable<OutcomeChange> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            var list = new List<OutcomeChange>();
            foreach (var c in changes)
            {
                if (c == null) throw new ArgumentException("变更集不可含 null 项。", nameof(changes));
                list.Add(c);
            }
            Branch = branch;
            Changes = list;
        }
    }
}
