using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Relationships
{
    /// <summary>协作响应（GDD_006 §Formula 3）：结构化结果，非单一数值。</summary>
    public enum CooperationResponse
    {
        /// <summary>接受。</summary>
        Accept = 0,

        /// <summary>要求保证。</summary>
        RequireGuarantee = 1,

        /// <summary>反对。</summary>
        Oppose = 2,

        /// <summary>拒绝。</summary>
        Reject = 3,
    }

    /// <summary>协作响应阈值（accept ≥ guarantee ≥ reject）。</summary>
    public readonly struct CooperationThresholds
    {
        public FixedPoint Accept { get; }
        public FixedPoint Guarantee { get; }
        public FixedPoint Reject { get; }

        public CooperationThresholds(FixedPoint accept, FixedPoint guarantee, FixedPoint reject)
        {
            if (accept < guarantee || guarantee < reject)
                throw new ArgumentException("阈值须满足 accept ≥ guarantee ≥ reject。");
            Accept = accept;
            Guarantee = guarantee;
            Reject = reject;
        }
    }

    /// <summary>
    /// 协作评估（GDD_006 §Formula 3 / TR-relationship-001）：
    /// <c>coop_score = Σ_dim w_coop[dim]·rel[executor→requester][dim] − risk_pen</c>，再映射为结构化响应。
    /// 关系<b>只</b>经此产出协作意愿（coop_score 供 GDD_005 willingness 的 relation_term 消费，破环在前），
    /// <b>不</b>凭空授予法律权限（授权见 <see cref="AuthorityGrant"/>，AC-3）。定点确定性。
    /// </summary>
    public static class CooperationEvaluator
    {
        /// <summary>
        /// 计算 executor 对 requester 的协作分（GDD §Formula 3）。
        /// </summary>
        public static FixedPoint ComputeCoopScore(
            RelationshipState relationships,
            CharacterId executor,
            CharacterId requester,
            IReadOnlyDictionary<RelationshipDimension, FixedPoint> dimensionWeights,
            FixedPoint riskPenalty)
        {
            if (relationships == null) throw new ArgumentNullException(nameof(relationships));
            if (dimensionWeights == null) throw new ArgumentNullException(nameof(dimensionWeights));

            FixedPoint sum = FixedPoint.Zero;
            foreach (var kv in dimensionWeights)
            {
                int rel = relationships.Get(executor, requester, kv.Key);
                sum += kv.Value * FixedPoint.FromInt(rel);
            }
            return sum - riskPenalty;
        }

        /// <summary>将协作分映射为结构化响应（GDD §Formula 3 阈值阶梯）。</summary>
        public static CooperationResponse Classify(FixedPoint coopScore, CooperationThresholds thresholds)
        {
            if (coopScore >= thresholds.Accept) return CooperationResponse.Accept;
            if (coopScore >= thresholds.Guarantee) return CooperationResponse.RequireGuarantee;
            if (coopScore >= thresholds.Reject) return CooperationResponse.Oppose;
            return CooperationResponse.Reject;
        }
    }
}
