using System;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 战前准备校验的版本化配置（GDD_009 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围。错误/风险阈值来自配置，不硬编码。
    /// </summary>
    public sealed class PreparationConfig
    {
        /// <summary>
        /// 资源紧张风险边际（≥0）：可承诺量 − 需求 ∈ [0, 此值] 且有需求时，标软风险（非错误，P7）。
        /// 「资源恰好够」（差额 0）即落入此区间，列为风险而非硬冲突。
        /// </summary>
        public long TightResourceMargin { get; }

        public PreparationConfig(long tightResourceMargin)
        {
            if (tightResourceMargin < 0) throw new ArgumentOutOfRangeException(nameof(tightResourceMargin), "紧张边际不可为负。");
            TightResourceMargin = tightResourceMargin;
        }
    }
}
