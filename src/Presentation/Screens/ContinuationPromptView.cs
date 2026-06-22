using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Outcome;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>一条可继续命令的展示项。</summary>
    public sealed class ContinuationOptionView
    {
        /// <summary>命令种类标识（展示文本）。</summary>
        public string KindLabel { get; }
        /// <summary>可读理由。</summary>
        public string Reason { get; }

        internal ContinuationOptionView(string kindLabel, string reason)
        {
            KindLabel = kindLabel;
            Reason = reason;
        }
    }

    /// <summary>
    /// 失败延续「继续」契约的展示模型（pause-menu/main-menu 续局入口 / 强制设计锁 / ADR-0002）。
    /// 消费 epic-008 <see cref="OutcomeContinuation"/>：即便败局也<b>保证</b>至少一条合法可继续命令
    /// （<see cref="IsPlayable"/>）。本视图只读不写。不可变。
    /// </summary>
    public sealed class ContinuationPromptView
    {
        /// <summary>战果分支标识。</summary>
        public string BranchLabel { get; }

        /// <summary>可继续命令项（保证非空）。</summary>
        public IReadOnlyList<ContinuationOptionView> Options { get; }

        /// <summary>世界是否可继续（写回成功且存在 ≥1 合法命令）。</summary>
        public bool IsPlayable { get; }

        private ContinuationPromptView(string branchLabel, IReadOnlyList<ContinuationOptionView> options, bool isPlayable)
        {
            BranchLabel = branchLabel;
            Options = options;
            IsPlayable = isPlayable;
        }

        /// <summary>从结算延续结果构造续局入口。</summary>
        public static ContinuationPromptView From(OutcomeContinuation continuation)
        {
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            var views = new List<ContinuationOptionView>();
            foreach (var o in continuation.Options)
                views.Add(new ContinuationOptionView(o.Kind.ToString(), o.Reason));
            return new ContinuationPromptView(continuation.Branch.ToString(), views, continuation.IsPlayable);
        }
    }
}
