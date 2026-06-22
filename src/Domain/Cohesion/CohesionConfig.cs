using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Cohesion
{
    /// <summary>
    /// 凝聚力平衡配置（GDD_011 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围。阈值/曲线/影响上限/断粮惩罚来自配置，不硬编码。定点（ADR-0004）。
    /// </summary>
    public sealed class CohesionConfig
    {
        /// <summary>士气下限 morale_floor（[0,1]）。</summary>
        public FixedPoint MoraleFloor { get; }

        /// <summary>维持阈值 hold_threshold（[0,1]）：discipline×command×route 低于此值则动摇。</summary>
        public FixedPoint HoldThreshold { get; }

        /// <summary>溃散概率 rout_prob（[0,1]，检查点随机流比较）。</summary>
        public FixedPoint RoutProbability { get; }

        /// <summary>指挥影响上限 cmd_cap（≥0）。</summary>
        public FixedPoint CommandInfluenceCap { get; }

        /// <summary>断粮每累计时段的士气惩罚（≥0；011 唯一施加）。</summary>
        public FixedPoint SupplyMoralePenaltyPerSegment { get; }

        /// <summary>断粮每累计时段的疲劳增加（≥0；011 唯一施加）。</summary>
        public FixedPoint SupplyFatiguePerSegment { get; }

        public CohesionConfig(
            FixedPoint moraleFloor, FixedPoint holdThreshold, FixedPoint routProbability,
            FixedPoint commandInfluenceCap, FixedPoint supplyMoralePenaltyPerSegment, FixedPoint supplyFatiguePerSegment)
        {
            RequireUnit(moraleFloor, nameof(moraleFloor));
            RequireUnit(holdThreshold, nameof(holdThreshold));
            RequireUnit(routProbability, nameof(routProbability));
            RequireNonNegative(commandInfluenceCap, nameof(commandInfluenceCap));
            RequireNonNegative(supplyMoralePenaltyPerSegment, nameof(supplyMoralePenaltyPerSegment));
            RequireNonNegative(supplyFatiguePerSegment, nameof(supplyFatiguePerSegment));

            MoraleFloor = moraleFloor; HoldThreshold = holdThreshold; RoutProbability = routProbability;
            CommandInfluenceCap = commandInfluenceCap;
            SupplyMoralePenaltyPerSegment = supplyMoralePenaltyPerSegment;
            SupplyFatiguePerSegment = supplyFatiguePerSegment;
        }

        private static void RequireUnit(FixedPoint v, string n)
        { if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(n, "须在 [0,1]。"); }
        private static void RequireNonNegative(FixedPoint v, string n)
        { if (v < FixedPoint.Zero) throw new ArgumentOutOfRangeException(n, "不可为负。"); }
    }

    /// <summary>阈值检查综合结果（GDD_011 §Formula 4：多因素，非单一士气决定）。</summary>
    public enum CohesionStatus
    {
        /// <summary>稳定。</summary>
        Steady = 0,

        /// <summary>动摇（军纪/退路/士气任一恶化触发，非单一士气）。</summary>
        Wavering = 1,

        /// <summary>溃散（士气低 + 维持不足 + 随机检查同时成立）。</summary>
        Routed = 2,
    }
}
