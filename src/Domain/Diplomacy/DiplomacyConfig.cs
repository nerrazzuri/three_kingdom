using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 受控外交的版本化平衡配置（GDD_012 §8 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围，非法即抛、无部分写入。响应权重/阈值/宽限/背约曲线均来自配置，
    /// 逻辑不硬编码。判定全程定点（[0,1] 域），确定性可重放（ADR-0004）。
    /// </summary>
    public sealed class DiplomacyConfig
    {
        /// <summary>外势力基础响应倾向 base_grant（[0,1]）。</summary>
        public FixedPoint BaseGrant { get; }

        /// <summary>声望权重 w_s（≥0）。</summary>
        public FixedPoint WeightStanding { get; }

        /// <summary>代价权重 w_c（≥0），作用于边际递减 f(pledge_cost)。</summary>
        public FixedPoint WeightCost { get; }

        /// <summary>反向压力权重 w_p（≥0）。</summary>
        public FixedPoint WeightPressure { get; }

        /// <summary>接受阈值 accept_threshold（[0,1]）。</summary>
        public FixedPoint AcceptThreshold { get; }

        /// <summary>附条件阈值 cond_threshold（[0,1]，≤ 接受阈值）。</summary>
        public FixedPoint ConditionalThreshold { get; }

        /// <summary>代价边际递减归一化常数（&gt;0）：f(cost)=cost/(cost+norm)，加码收益放缓。</summary>
        public long CostNormalizer { get; }

        /// <summary>交付时段数 commit_lead（≥1；外援不即时，GDD_012 §8.2）。</summary>
        public int CommitLeadSegments { get; }

        /// <summary>背约基础风险 betray_risk 基线（[0,1]），按声望折减、按压力放大。</summary>
        public FixedPoint BetrayRiskBase { get; }

        /// <summary>背约风险的压力权重（≥0）。</summary>
        public FixedPoint BetrayPressureWeight { get; }

        /// <summary>背约/违约的声誉惩罚量（≥0；写回 GDD_006）。</summary>
        public int BetrayalStandingPenalty { get; }

        public DiplomacyConfig(
            FixedPoint baseGrant,
            FixedPoint weightStanding,
            FixedPoint weightCost,
            FixedPoint weightPressure,
            FixedPoint acceptThreshold,
            FixedPoint conditionalThreshold,
            long costNormalizer,
            int commitLeadSegments,
            FixedPoint betrayRiskBase,
            FixedPoint betrayPressureWeight,
            int betrayalStandingPenalty)
        {
            RequireUnit(baseGrant, nameof(baseGrant));
            RequireNonNegative(weightStanding, nameof(weightStanding));
            RequireNonNegative(weightCost, nameof(weightCost));
            RequireNonNegative(weightPressure, nameof(weightPressure));
            RequireUnit(acceptThreshold, nameof(acceptThreshold));
            RequireUnit(conditionalThreshold, nameof(conditionalThreshold));
            if (conditionalThreshold > acceptThreshold)
                throw new ArgumentOutOfRangeException(nameof(conditionalThreshold), "附条件阈值不可高于接受阈值。");
            if (costNormalizer <= 0) throw new ArgumentOutOfRangeException(nameof(costNormalizer), "代价归一化常数须为正。");
            if (commitLeadSegments < 1) throw new ArgumentOutOfRangeException(nameof(commitLeadSegments), "交付时段数须≥1（外援不即时）。");
            RequireUnit(betrayRiskBase, nameof(betrayRiskBase));
            RequireNonNegative(betrayPressureWeight, nameof(betrayPressureWeight));
            if (betrayalStandingPenalty < 0) throw new ArgumentOutOfRangeException(nameof(betrayalStandingPenalty), "声誉惩罚不可为负。");

            BaseGrant = baseGrant;
            WeightStanding = weightStanding;
            WeightCost = weightCost;
            WeightPressure = weightPressure;
            AcceptThreshold = acceptThreshold;
            ConditionalThreshold = conditionalThreshold;
            CostNormalizer = costNormalizer;
            CommitLeadSegments = commitLeadSegments;
            BetrayRiskBase = betrayRiskBase;
            BetrayPressureWeight = betrayPressureWeight;
            BetrayalStandingPenalty = betrayalStandingPenalty;
        }

        private static void RequireUnit(FixedPoint value, string name)
        {
            if (value < FixedPoint.Zero || value > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "须在 [0,1]。");
        }

        private static void RequireNonNegative(FixedPoint value, string name)
        {
            if (value < FixedPoint.Zero)
                throw new ArgumentOutOfRangeException(name, "不可为负。");
        }
    }
}
