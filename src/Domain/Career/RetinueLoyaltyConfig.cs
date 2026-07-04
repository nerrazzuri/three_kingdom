using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 忠诚经营配置（GDD_014 忠诚维持 + 被挖角，数据驱动，权威定点 ADR-0004）。不可变。
    /// 赏赐升忠诚、久疏则衰减、低忠诚可被敌挖角（与 GDD_024 人心杠杆对玩家守将的策反对称）。
    /// </summary>
    public sealed class RetinueLoyaltyConfig
    {
        /// <summary>每单位赏赐强度提升的好感。</summary>
        public FixedPoint RewardPerIntensity { get; }
        /// <summary>每次衰减推进的好感降幅（久疏忠诚自然消退）。</summary>
        public FixedPoint DecayPerTick { get; }
        /// <summary>忠诚衰减下限（衰减不越此，避免无操作即归零）。</summary>
        public FixedPoint LoyaltyFloor { get; }
        /// <summary>可被挖角的忠诚上界（好感 ≥ 此则不可挖角——忠者不叛）。</summary>
        public FixedPoint PoachThreshold { get; }
        /// <summary>挖角基础概率。</summary>
        public FixedPoint PoachBase { get; }
        /// <summary>敌拉拢力权重（对方名望/许诺投影）。</summary>
        public FixedPoint PoachPullWeight { get; }
        /// <summary>脆弱度权重（作用于 阈值−好感，越不忠越易挖）。</summary>
        public FixedPoint PoachVulnerabilityWeight { get; }

        public RetinueLoyaltyConfig(
            FixedPoint rewardPerIntensity, FixedPoint decayPerTick, FixedPoint loyaltyFloor,
            FixedPoint poachThreshold, FixedPoint poachBase, FixedPoint poachPullWeight, FixedPoint poachVulnerabilityWeight)
        {
            RewardPerIntensity = rewardPerIntensity;
            DecayPerTick = decayPerTick;
            LoyaltyFloor = loyaltyFloor;
            PoachThreshold = poachThreshold;
            PoachBase = poachBase;
            PoachPullWeight = poachPullWeight;
            PoachVulnerabilityWeight = poachVulnerabilityWeight;
        }

        /// <summary>默认（待打磨）：赏赐 +0.2、衰减 -0.03（下限 0.1）、忠诚 &lt;0.4 可挖角。</summary>
        public static RetinueLoyaltyConfig Default { get; } = new RetinueLoyaltyConfig(
            rewardPerIntensity: FixedPoint.FromFraction(2, 10),
            decayPerTick: FixedPoint.FromFraction(3, 100),
            loyaltyFloor: FixedPoint.FromFraction(1, 10),
            poachThreshold: FixedPoint.FromFraction(4, 10),
            poachBase: FixedPoint.FromFraction(1, 10),
            poachPullWeight: FixedPoint.FromFraction(4, 10),
            poachVulnerabilityWeight: FixedPoint.FromFraction(6, 10));
    }
}
