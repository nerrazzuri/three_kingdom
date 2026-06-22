using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 一次战果结算的延续结果（gdd-010 §后果/失败延续）。携带分支、写回后的世界快照、
    /// 用于产生该分支的变更集，以及至少一条<b>合法可继续命令</b>。
    /// <para>
    /// 强制设计锁：失败<b>必须</b>产生可继续状态——<see cref="Options"/> 对任何分支均<b>非空</b>，
    /// 由 <see cref="FailureContinuationService"/> 在构造时断言保证（败局不切到空白/死局）。
    /// </para>
    /// </summary>
    public sealed class OutcomeContinuation
    {
        /// <summary>战果分支。</summary>
        public OutcomeBranch Branch { get; }

        /// <summary>本分支的跨系统变更集（经 Story 001 写回路径产生）。</summary>
        public ConsequenceSet Consequences { get; }

        /// <summary>写回结果（含写回后世界快照与确定性哈希）。</summary>
        public OutcomeWritebackResult Writeback { get; }

        /// <summary>合法可继续命令（保证非空）。</summary>
        public IReadOnlyList<ContinuationOption> Options { get; }

        internal OutcomeContinuation(
            OutcomeBranch branch,
            ConsequenceSet consequences,
            OutcomeWritebackResult writeback,
            IReadOnlyList<ContinuationOption> options)
        {
            if (options == null || options.Count == 0)
                throw new InvalidOperationException("强制设计锁违反：结算未产生任何可继续命令（失败必须可玩）。");

            Branch = branch;
            Consequences = consequences;
            Writeback = writeback;
            Options = options;
        }

        /// <summary>世界状态是否完整可继续（写回成功且存在 ≥1 合法命令）。</summary>
        public bool IsPlayable => Writeback.Committed && Options.Count > 0;

        /// <summary>是否提供某种可继续命令。</summary>
        public bool HasOption(ContinuationCommandKind kind)
        {
            foreach (var o in Options) if (o.Kind == kind) return true;
            return false;
        }
    }
}
