using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 军议平衡配置（GDD_008 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围。能力对缺口发现的权重等来自配置，<b>不</b>写入建议生成方法。
    /// </summary>
    public sealed class CouncilConfig
    {
        /// <summary>能力对发现缺口/矛盾的权重 w_gap（≥0）。</summary>
        public FixedPoint GapDetectionWeight { get; }

        public CouncilConfig(FixedPoint gapDetectionWeight)
        {
            if (gapDetectionWeight < FixedPoint.Zero)
                throw new ArgumentOutOfRangeException(nameof(gapDetectionWeight), "缺口发现权重不可为负。");
            GapDetectionWeight = gapDetectionWeight;
        }
    }
}
