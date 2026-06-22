using System;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 战果延续命令种类（gdd-010 §后果/失败延续）。失败<b>为分支非终局</b>——
    /// 任一败局至少给出一条可继续路径（强制设计锁），胜局亦延续战役循环。
    /// </summary>
    public enum ContinuationCommandKind
    {
        /// <summary>追击扩大战果（胜利后）。</summary>
        Pursue = 0,

        /// <summary>巩固据点（胜利后）。</summary>
        Consolidate = 1,

        /// <summary>重整旗鼓（败后通用兜底，始终可用）。</summary>
        Regroup = 2,

        /// <summary>问责追责（败后通用兜底，始终可用）。</summary>
        Accountability = 3,

        /// <summary>且战且退/撤往后方（撤退分支）。</summary>
        Retreat = 4,

        /// <summary>遣使求和（失城分支可用）。</summary>
        SueForPeace = 5,
    }

    /// <summary>
    /// 一条合法的可继续命令（不可变）。携带种类与人类可读理由，供 main-menu/pause「继续」契约与 hud.md §2.5。
    /// </summary>
    public sealed class ContinuationOption
    {
        /// <summary>命令种类。</summary>
        public ContinuationCommandKind Kind { get; }

        /// <summary>可读理由/说明（非空）。</summary>
        public string Reason { get; }

        public ContinuationOption(ContinuationCommandKind kind, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("理由不可为空。", nameof(reason));
            Kind = kind;
            Reason = reason;
        }
    }
}
