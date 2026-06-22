using System;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 受控外交服务（GDD_012 §8 / TR 外交受控入口 / ADR-0002 单一受控入口 + ADR-0004 确定性）。
    /// 外势力为<b>静态背景</b>（无 AI）：响应仅由场景配置 + 声望 + 代价 + 反向压力 + 注入随机流决定。
    /// 设计锁：<b>非点击即到</b>——接受也须经延迟交付（arrival_T）+ 兑现/背约判定；代价兑付不凭空返还；
    /// 同种子+同请求+同前态 → 同 grant_score 与同 fulfilled（可重放）。
    /// 判定全程定点（[0,1] 域），权威路径无 float。
    /// </summary>
    public sealed class DiplomacyService
    {
        /// <summary>
        /// 响应判定 + 延迟交付时间 + 背约风险（GDD_012 §8.1/§8.2/§8.3，确定性，<b>不</b>消费随机流）。
        /// </summary>
        public DiplomaticPledge Evaluate(DiplomaticRequest request, WorldTime now, DiplomacyConfig config)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // f(pledge_cost)：边际递减 cost/(cost+norm) ∈ [0,1)（加码代价收益放缓）。
            long denom = checked(request.PledgeCost + config.CostNormalizer);
            FixedPoint fCost = FixedPoint.FromFraction(checked((int)request.PledgeCost), checked((int)denom));

            // grant_score = clamp(base + w_s·standing + w_c·f(cost) − w_p·pressure, 0, 1)。
            FixedPoint grantRaw =
                config.BaseGrant
                + config.WeightStanding * request.Standing
                + config.WeightCost * fCost
                - config.WeightPressure * request.DiplomaticPressure;
            FixedPoint grantScore = grantRaw.Clamp(FixedPoint.Zero, FixedPoint.One);

            DiplomaticResponse response =
                grantScore >= config.AcceptThreshold ? DiplomaticResponse.Accepted
                : grantScore >= config.ConditionalThreshold ? DiplomaticResponse.Conditional
                : DiplomaticResponse.Rejected;

            // betray_risk = clamp(base·(1−standing) + w_pressure·pressure, 0, 1)：声望降风险、压力升风险。
            FixedPoint betrayRisk =
                (config.BetrayRiskBase * (FixedPoint.One - request.Standing)
                 + config.BetrayPressureWeight * request.DiplomaticPressure)
                .Clamp(FixedPoint.Zero, FixedPoint.One);

            bool accepted = response == DiplomaticResponse.Accepted;
            WorldTime? arrival = accepted ? now.Advance(config.CommitLeadSegments) : (WorldTime?)null;
            long delivered = accepted ? request.RequestedAmount : 0;

            return new DiplomaticPledge(request, response, grantScore, config.CommitLeadSegments, arrival, betrayRisk, delivered);
        }

        /// <summary>
        /// 兑现/背约判定（GDD_012 §8.3/§8.5）：fulfilled = (r ≥ betray_risk) ∧ 玩家未违约 ∧ 路线未永久切断。
        /// 随机流<b>仅在兑现检查点消费</b>（玩家守约且路线通时才比较 r）。代价接受即兑付，背约不凭空返还。
        /// </summary>
        public DiplomaticOutcome Resolve(
            DiplomaticPledge pledge,
            IDeterministicRandom random,
            bool playerHonored,
            bool routePermanentlyCut,
            DiplomacyConfig config)
        {
            if (pledge == null) throw new ArgumentNullException(nameof(pledge));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 未接受：不兑付代价、不交付。
            if (pledge.Response != DiplomaticResponse.Accepted)
                return new DiplomaticOutcome(false, DiplomaticOutcomeReason.NotAccepted, 0, 0);

            long costPaid = pledge.Request.PledgeCost; // 接受即兑付（§8.5），后续失败也不返还。

            // 玩家违反承诺前提：外援取消 + 声誉后果。
            if (!playerHonored)
                return new DiplomaticOutcome(false, DiplomaticOutcomeReason.PlayerBreached, costPaid, config.BetrayalStandingPenalty);

            // 交付路线被永久切断：转运输失败（非外势力背约，无声誉惩罚）。
            if (routePermanentlyCut)
                return new DiplomaticOutcome(false, DiplomaticOutcomeReason.RoutePermanentlyCut, costPaid, 0);

            // 兑现检查点：消费随机流比较背约风险。
            FixedPoint r = random.NextUnit();
            if (r < pledge.BetrayRisk)
                return new DiplomaticOutcome(false, DiplomaticOutcomeReason.BetrayedByForeignPower, costPaid, config.BetrayalStandingPenalty);

            return new DiplomaticOutcome(true, DiplomaticOutcomeReason.Fulfilled, costPaid, 0);
        }

        /// <summary>
        /// 将兑现的求粮外援交付计入后勤权威库存（GDD_012 §8.4/§8.5：补给→后勤，守恒）。
        /// 外部补给作为合法外部来源单一计入在途载量，总量增加恰为交付量（不凭空双计）。
        /// 仅对已兑现的 <see cref="DiplomaticRequestType.Supply"/> 生效，其余原样返回。
        /// </summary>
        public SupplyChainState ApplyFulfilledSupply(SupplyChainState state, DiplomaticOutcome outcome, DiplomaticPledge pledge)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            if (pledge == null) throw new ArgumentNullException(nameof(pledge));

            if (!outcome.Fulfilled || pledge.Request.Type != DiplomaticRequestType.Supply)
                return state;

            return state.With(convoyLoad: checked(state.ConvoyLoad + pledge.DeliveredAmount));
        }
    }
}
