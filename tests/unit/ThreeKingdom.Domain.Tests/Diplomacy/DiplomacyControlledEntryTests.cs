using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Diplomacy
{
    /// <summary>
    /// epic-004 story-003：外交受控入口（求援/求粮/求时限，§8）。
    /// 治理 ADR：ADR-0002（单一受控入口经 Command）+ ADR-0004（确定性）。GDD_012 §8。
    /// 覆盖 AC-1 三选一入口、AC-2 grant_score+延迟交付确定性、AC-3 可背约失败、
    /// AC-4 代价兑付、AC-5 交付守恒、AC-6 静态背景（无 AI，响应纯由配置+声望+随机流）。
    /// </summary>
    [TestFixture]
    public class DiplomacyControlledEntryTests
    {
        private static readonly DiplomacyService Service = new DiplomacyService();
        private static readonly ForeignPowerId Power = new ForeignPowerId("power-jingzhou");
        private static readonly WorldTime Now = new WorldTime(5, DaySegment.Day);

        private static DiplomacyConfig Config()
            => new DiplomacyConfig(
                baseGrant: F(3, 10),
                weightStanding: F(2, 5),
                weightCost: F(3, 10),
                weightPressure: F(1, 5),
                acceptThreshold: F(11, 20),
                conditionalThreshold: F(3, 10),
                costNormalizer: 100,
                commitLeadSegments: 3,
                betrayRiskBase: F(1, 5),
                betrayPressureWeight: F(3, 10),
                betrayalStandingPenalty: 10);

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static DiplomaticRequest Request(
            DiplomaticRequestType type, long cost = 100, long amount = 500,
            int standingN = 3, int standingD = 5, int pressureN = 2, int pressureD = 5)
            => new DiplomaticRequest(type, Power, cost, amount, F(standingN, standingD), F(pressureN, pressureD));

        /// <summary>测试用确定性随机替身：NextUnit 返回固定值并计消费次数（验证「仅兑现检查点消费」）。</summary>
        private sealed class FakeRandom : IDeterministicRandom
        {
            private readonly FixedPoint _unit;
            public int CallCount { get; private set; }
            public FakeRandom(FixedPoint unit) => _unit = unit;
            public ulong Position => (ulong)CallCount;
            public ulong NextBits() { CallCount++; return 0UL; }
            public FixedPoint NextUnit() { CallCount++; return _unit; }
            public int NextInt(int minInclusive, int maxExclusive) { CallCount++; return minInclusive; }
        }

        // ---- AC-2 + AC-1: 接受 → 延迟交付（非即到）----

        [Test]
        public void test_accepted_request_schedules_delayed_delivery_not_immediate()
        {
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply), Now, Config());

            Assert.That(pledge.Response, Is.EqualTo(DiplomaticResponse.Accepted));
            Assert.That(pledge.ArrivalTime, Is.Not.Null);
            Assert.That(pledge.ArrivalTime!.Value, Is.EqualTo(Now.Advance(3)));
            Assert.That(pledge.ArrivalTime.Value, Is.GreaterThan(Now), "外援不即时——到达时间须晚于当下。");
            Assert.That(pledge.DeliveredAmount, Is.EqualTo(500));
        }

        [Test]
        public void test_low_grant_score_is_rejected_with_no_delivery()
        {
            // 声望 0 + 高反向压力 → grant 低于附条件阈值
            var req = Request(DiplomaticRequestType.Supply, cost: 0, standingN: 0, standingD: 1, pressureN: 4, pressureD: 5);
            var pledge = Service.Evaluate(req, Now, Config());

            Assert.That(pledge.Response, Is.EqualTo(DiplomaticResponse.Rejected));
            Assert.That(pledge.ArrivalTime, Is.Null);
            Assert.That(pledge.DeliveredAmount, Is.EqualTo(0));
        }

        [Test]
        public void test_conditional_response_between_thresholds()
        {
            // base 0.3、无声望/代价/压力 → grant=0.3 ∈ [cond 0.3, accept 0.55) → 附条件
            var req = Request(DiplomaticRequestType.Supply, cost: 0, standingN: 0, standingD: 1, pressureN: 0, pressureD: 1);
            var pledge = Service.Evaluate(req, Now, Config());

            Assert.That(pledge.Response, Is.EqualTo(DiplomaticResponse.Conditional));
            Assert.That(pledge.ArrivalTime, Is.Null);
        }

        [Test]
        public void test_three_request_types_each_carry_their_slice_payload()
        {
            foreach (var type in new[] { DiplomaticRequestType.Reinforcement, DiplomaticRequestType.Supply, DiplomaticRequestType.Deadline })
            {
                var pledge = Service.Evaluate(Request(type), Now, Config());
                Assert.That(pledge.Request.Type, Is.EqualTo(type));
                Assert.That(pledge.Response, Is.EqualTo(DiplomaticResponse.Accepted));
                Assert.That(pledge.DeliveredAmount, Is.EqualTo(500));
            }
        }

        // ---- AC-6: 静态背景——确定性，同输入同输出 ----

        [Test]
        public void test_grant_score_and_arrival_are_deterministic()
        {
            var req = Request(DiplomaticRequestType.Supply);
            var a = Service.Evaluate(req, Now, Config());
            var b = Service.Evaluate(req, Now, Config());

            Assert.That(b.GrantScore, Is.EqualTo(a.GrantScore));
            Assert.That(b.BetrayRisk, Is.EqualTo(a.BetrayRisk));
            Assert.That(b.ArrivalTime, Is.EqualTo(a.ArrivalTime));
            Assert.That(b.Response, Is.EqualTo(a.Response));
        }

        // ---- AC-3 + AC-4: 兑现/背约 + 代价兑付 ----

        [Test]
        public void test_fulfillment_when_rng_at_or_above_betray_risk()
        {
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply), Now, Config());
            var rng = new FakeRandom(F(9, 10)); // r=0.9 ≥ betray_risk

            var outcome = Service.Resolve(pledge, rng, playerHonored: true, routePermanentlyCut: false, Config());

            Assert.That(outcome.Fulfilled, Is.True);
            Assert.That(outcome.Reason, Is.EqualTo(DiplomaticOutcomeReason.Fulfilled));
            Assert.That(outcome.CostPaid, Is.EqualTo(100));
            Assert.That(outcome.ReputationPenalty, Is.EqualTo(0));
            Assert.That(rng.CallCount, Is.EqualTo(1), "兑现检查点消费一次随机流。");
        }

        [Test]
        public void test_betrayal_when_rng_below_betray_risk_keeps_cost_and_penalizes_standing()
        {
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply), Now, Config());
            var rng = new FakeRandom(F(1, 100)); // r=0.01 < betray_risk

            var outcome = Service.Resolve(pledge, rng, playerHonored: true, routePermanentlyCut: false, Config());

            Assert.That(outcome.Fulfilled, Is.False);
            Assert.That(outcome.Reason, Is.EqualTo(DiplomaticOutcomeReason.BetrayedByForeignPower));
            Assert.That(outcome.CostPaid, Is.EqualTo(100), "背约不凭空返还代价。");
            Assert.That(outcome.ReputationPenalty, Is.EqualTo(10));
        }

        [Test]
        public void test_player_breach_cancels_without_consuming_rng()
        {
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply), Now, Config());
            var rng = new FakeRandom(F(9, 10));

            var outcome = Service.Resolve(pledge, rng, playerHonored: false, routePermanentlyCut: false, Config());

            Assert.That(outcome.Reason, Is.EqualTo(DiplomaticOutcomeReason.PlayerBreached));
            Assert.That(outcome.Fulfilled, Is.False);
            Assert.That(outcome.CostPaid, Is.EqualTo(100));
            Assert.That(outcome.ReputationPenalty, Is.EqualTo(10));
            Assert.That(rng.CallCount, Is.EqualTo(0), "未到兑现检查点不消费随机流。");
        }

        [Test]
        public void test_route_permanently_cut_fails_as_transport_not_betrayal()
        {
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply), Now, Config());
            var rng = new FakeRandom(F(9, 10));

            var outcome = Service.Resolve(pledge, rng, playerHonored: true, routePermanentlyCut: true, Config());

            Assert.That(outcome.Reason, Is.EqualTo(DiplomaticOutcomeReason.RoutePermanentlyCut));
            Assert.That(outcome.Fulfilled, Is.False);
            Assert.That(outcome.ReputationPenalty, Is.EqualTo(0), "运输失败非外势力背约，无声誉惩罚。");
            Assert.That(rng.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void test_rejected_request_pays_no_cost_and_consumes_no_rng()
        {
            var req = Request(DiplomaticRequestType.Supply, cost: 0, standingN: 0, standingD: 1, pressureN: 4, pressureD: 5);
            var pledge = Service.Evaluate(req, Now, Config());
            Assume.That(pledge.Response, Is.EqualTo(DiplomaticResponse.Rejected));
            var rng = new FakeRandom(F(9, 10));

            var outcome = Service.Resolve(pledge, rng, playerHonored: true, routePermanentlyCut: false, Config());

            Assert.That(outcome.Reason, Is.EqualTo(DiplomaticOutcomeReason.NotAccepted));
            Assert.That(outcome.CostPaid, Is.EqualTo(0));
            Assert.That(rng.CallCount, Is.EqualTo(0));
        }

        // ---- AC-5: 交付守恒，外部补给单一计入 ----

        [Test]
        public void test_fulfilled_supply_delivery_is_conserved_and_counted_once()
        {
            var state = new SupplyChainState(cityStock: 0, convoyLoad: 0, unitCarried: 0, consumed: 0, lost: 0, shortageSegments: 0);
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Supply, amount: 500), Now, Config());
            var outcome = Service.Resolve(pledge, new FakeRandom(F(9, 10)), true, false, Config());
            Assume.That(outcome.Fulfilled, Is.True);

            var after = Service.ApplyFulfilledSupply(state, outcome, pledge);

            Assert.That(after.ConvoyLoad, Is.EqualTo(500), "外部补给单一计入在途。");
            Assert.That(after.GrandTotal, Is.EqualTo(state.GrandTotal + 500), "外援合法外部来源，总量增加恰为交付量。");
        }

        [Test]
        public void test_reinforcement_does_not_apply_to_supply_stock()
        {
            var state = new SupplyChainState(0, 0, 0, 0, 0, 0);
            var pledge = Service.Evaluate(Request(DiplomaticRequestType.Reinforcement, amount: 500), Now, Config());
            var outcome = Service.Resolve(pledge, new FakeRandom(F(9, 10)), true, false, Config());

            var after = Service.ApplyFulfilledSupply(state, outcome, pledge);

            Assert.That(after.GrandTotal, Is.EqualTo(state.GrandTotal), "求援不入后勤库存（喂 GDD_010 兵力）。");
        }

        // ---- 构造不变量 ----

        [Test]
        public void test_config_rejects_conditional_threshold_above_accept()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DiplomacyConfig(
                F(3, 10), F(2, 5), F(3, 10), F(1, 5),
                acceptThreshold: F(3, 10), conditionalThreshold: F(11, 20),
                costNormalizer: 100, commitLeadSegments: 3,
                betrayRiskBase: F(1, 5), betrayPressureWeight: F(3, 10), betrayalStandingPenalty: 10));
        }

        [Test]
        public void test_request_rejects_standing_out_of_unit_range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DiplomaticRequest(DiplomaticRequestType.Supply, Power, 0, 0, FixedPoint.FromInt(2), FixedPoint.Zero));
        }
    }
}
