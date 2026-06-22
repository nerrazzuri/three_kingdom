using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 后勤补给的版本化平衡配置（GDD_012 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造时校验范围，非法即抛、无部分写入。短缺宽限/损耗率等来自配置，
    /// 结算逻辑不硬编码。补给量为权威整数，损耗率用定点（权威路径无 float，ADR-0004）。
    /// </summary>
    public sealed class SupplyConfig
    {
        /// <summary>短缺宽限时段 grace_period（≥0）：短缺连续累计达此值才发断粮后果事件（GDD_012 §Formula 5）。</summary>
        public int GracePeriodSegments { get; }

        /// <summary>在途基础损耗率 loss_rate（[0,1]，定点，GDD_012 §Formula 4）。</summary>
        public FixedPoint TransitLossRate { get; }

        public SupplyConfig(int gracePeriodSegments, FixedPoint transitLossRate)
        {
            if (gracePeriodSegments < 0) throw new ArgumentOutOfRangeException(nameof(gracePeriodSegments), "宽限时段不可为负。");
            if (transitLossRate < FixedPoint.Zero || transitLossRate > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(transitLossRate), "在途损耗率须在 [0,1]。");

            GracePeriodSegments = gracePeriodSegments;
            TransitLossRate = transitLossRate;
        }
    }
}
