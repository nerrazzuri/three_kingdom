using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 分叉传播配置（GDD_015 / ADR-0007 §3）。脱稿涟漪深度数据驱动，不硬编码。
    /// <see cref="SpreadDepth"/>=0（默认）只重评估被分叉事件的<b>直接下游</b>；每 +1 多扩一跳涟漪。
    /// 远方超出深度的事件不重评估（按历史/抽象推进），避免全图涟漪。
    /// </summary>
    public sealed class DivergencePropagationConfig
    {
        /// <summary>脱稿涟漪深度（额外跳数，≥0）。</summary>
        public int SpreadDepth { get; }

        public DivergencePropagationConfig(int spreadDepth)
        {
            if (spreadDepth < 0) throw new ArgumentOutOfRangeException(nameof(spreadDepth), "脱稿深度不可为负。");
            SpreadDepth = spreadDepth;
        }

        /// <summary>默认：仅玩家圈直接下游（深度 0）。</summary>
        public static DivergencePropagationConfig Default { get; } = new DivergencePropagationConfig(0);
    }
}
