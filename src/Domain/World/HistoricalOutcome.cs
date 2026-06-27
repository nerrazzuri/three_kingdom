using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 历史事件结局（GDD_015 / ADR-0007：四元组之正常结局 / 分叉结局）。不可变值。
    /// 本骨架结局以稳定 <see cref="Label"/> 标识触发了哪条分支；态势/归属变更经抽象结算器（story-005）
    /// 与 GDD_004 控制权变更（story-004），不在本结局直接写（ADR-0008 城池归属只读）。
    /// </summary>
    public sealed class HistoricalOutcome
    {
        /// <summary>结局稳定标签（非空，用于回放/测试与后续结算分发）。</summary>
        public string Label { get; }

        public HistoricalOutcome(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("结局标签不可为空或空白。", nameof(label));
            Label = label;
        }

        public override string ToString() => Label;
    }
}
