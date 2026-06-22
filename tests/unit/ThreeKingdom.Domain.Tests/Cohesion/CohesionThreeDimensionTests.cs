using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Cohesion;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;

namespace ThreeKingdom.Domain.Tests.Cohesion
{
    /// <summary>
    /// epic-007 story-003：士气/疲劳/军纪三维与阈值检查。
    /// 治理 ADR：ADR-0004。GDD_011 / TR-cohesion-001/002。
    /// 覆盖 AC-1 三维独立 + 士气事件幂等聚合、AC-2 阈值多输入（非单一士气）、
    /// 拆分合并人数加权（非取最大）、AC-5 011 唯一施加 morale/fatigue（消费断粮事件）。
    /// </summary>
    [TestFixture]
    public class CohesionThreeDimensionTests
    {
        private static readonly CohesionService Service = new CohesionService();
        private static readonly UnitId U = new UnitId("unit-vanguard");

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CohesionState State(int n, int moraleN, int fatN, int discN)
            => new CohesionState(U, n, F(moraleN, 100), F(fatN, 100), F(discN, 100));

        private static CohesionConfig Config()
            => new CohesionConfig(
                moraleFloor: F(30, 100), holdThreshold: F(40, 100), routProbability: F(50, 100),
                commandInfluenceCap: F(10, 100), supplyMoralePenaltyPerSegment: F(5, 100), supplyFatiguePerSegment: F(4, 100));

        private sealed class FakeRandom : IDeterministicRandom
        {
            private readonly FixedPoint _unit;
            public int CallCount { get; private set; }
            public FakeRandom(FixedPoint unit) => _unit = unit;
            public ulong Position => (ulong)CallCount;
            public ulong NextBits() { CallCount++; return 0UL; }
            public FixedPoint NextUnit() { CallCount++; return _unit; }
            public int NextInt(int a, int b) { CallCount++; return a; }
        }

        // ---- AC-1: 三维独立 + 士气事件幂等 ----

        [Test]
        public void test_morale_event_does_not_change_fatigue_or_discipline()
        {
            var state = State(1000, moraleN: 60, fatN: 20, discN: 90);
            var evt = new MoraleEvent(new MoraleEventId("casualty"), F(-20, 100), FixedPoint.One);

            var after = Service.ApplyMoraleEvents(state, new[] { evt }, FixedPoint.Zero, Config());

            Assert.That(after.Fatigue, Is.EqualTo(state.Fatigue), "士气事件不改疲劳（三维独立）。");
            Assert.That(after.Discipline, Is.EqualTo(state.Discipline), "士气事件不改军纪（三维独立）。");
            Assert.That(after.Morale, Is.LessThan(state.Morale));
        }

        [Test]
        public void test_same_morale_event_is_idempotent()
        {
            var state = State(1000, moraleN: 60, fatN: 20, discN: 90);
            var evt = new MoraleEvent(new MoraleEventId("casualty"), F(-20, 100), FixedPoint.One);

            // 同一事件出现两次 → 仅结算一次（否则 morale 会降到约 0.2）
            var after = Service.ApplyMoraleEvents(state, new[] { evt, evt }, FixedPoint.Zero, Config());

            Assert.That(after.Morale, Is.GreaterThan(F(35, 100)), "幂等：同事件只算一次（≈0.4 而非 0.2）。");
            Assert.That(after.Morale, Is.LessThan(F(45, 100)));
        }

        [Test]
        public void test_morale_events_aggregate_by_audience_weight()
        {
            var state = State(1000, moraleN: 60, fatN: 20, discN: 90);
            var casualty = new MoraleEvent(new MoraleEventId("casualty"), F(-20, 100), FixedPoint.One);
            var relief = new MoraleEvent(new MoraleEventId("relief-news"), F(10, 100), F(80, 100));

            // 0.6 + (1.0×−0.2) + (0.8×+0.1) = 0.6 − 0.2 + 0.08 = 0.48
            var after = Service.ApplyMoraleEvents(state, new[] { casualty, relief }, FixedPoint.Zero, Config());

            Assert.That(after.Morale, Is.GreaterThan(F(46, 100)));
            Assert.That(after.Morale, Is.LessThan(F(50, 100)));
        }

        // ---- AC-2: 阈值多输入（非单一士气决定）----

        [Test]
        public void test_high_morale_still_wavers_when_discipline_collapses()
        {
            // 高士气 0.8，但军纪崩 0.1 → holdScore 0.1 < 0.4 → 动摇（证明非单一士气）
            var state = State(1000, moraleN: 80, fatN: 20, discN: 10);
            var status = Service.EvaluateThreshold(state, commandQuality: FixedPoint.One, routeClear: FixedPoint.One, new FakeRandom(F(99, 100)), Config());

            Assert.That(status, Is.EqualTo(CohesionStatus.Wavering));
        }

        [Test]
        public void test_steady_when_morale_and_hold_both_strong()
        {
            var state = State(1000, moraleN: 80, fatN: 20, discN: 90);
            var status = Service.EvaluateThreshold(state, FixedPoint.One, FixedPoint.One, new FakeRandom(F(1, 100)), Config());

            Assert.That(status, Is.EqualTo(CohesionStatus.Steady));
        }

        [Test]
        public void test_rout_requires_low_morale_broken_hold_and_random_check()
        {
            // 低士气 0.1 + 维持不足（军纪0.1）+ r=0.0 < 0.5 → 溃散（多条件，非单一）
            var state = State(1000, moraleN: 10, fatN: 50, discN: 10);
            var routed = Service.EvaluateThreshold(state, FixedPoint.One, FixedPoint.One, new FakeRandom(FixedPoint.Zero), Config());
            Assert.That(routed, Is.EqualTo(CohesionStatus.Routed));

            // 低士气但维持充分（军纪0.9）→ 不溃散，仅动摇（退路/军纪救场）
            var holding = State(1000, moraleN: 10, fatN: 50, discN: 90);
            var status = Service.EvaluateThreshold(holding, FixedPoint.One, FixedPoint.One, new FakeRandom(FixedPoint.Zero), Config());
            Assert.That(status, Is.EqualTo(CohesionStatus.Wavering));
        }

        // ---- 拆分/合并按人数加权（非取最大）----

        [Test]
        public void test_merge_is_headcount_weighted_not_max()
        {
            var a = new CohesionState(U, 1000, F(70, 100), F(20, 100), F(80, 100));
            var b = new CohesionState(new UnitId("unit-b"), 500, F(40, 100), F(50, 100), F(60, 100));

            var merged = Service.Merge(new UnitId("unit-merged"), new[] { a, b });

            // (1000×0.7 + 500×0.4)/1500 = 0.6（非 max 0.7）
            Assert.That(merged.Headcount, Is.EqualTo(1500));
            Assert.That(merged.Morale, Is.GreaterThan(F(59, 100)));
            Assert.That(merged.Morale, Is.LessThan(F(61, 100)));
            Assert.That(merged.Morale, Is.LessThan(a.Morale), "加权平均低于较高者，非取最大。");
        }

        // ---- AC-5: 011 唯一施加 morale/fatigue（消费断粮事件）----

        [Test]
        public void test_supply_cutoff_applies_morale_and_fatigue_only()
        {
            var state = State(1000, moraleN: 60, fatN: 20, discN: 90);
            var cutoff = new SupplyCutoffEvent(U, shortageSegments: 3, shortage: 100);

            var after = Service.ApplySupplyCutoff(state, cutoff, Config());

            // 士气 0.6 − 0.05×3 = 0.45；疲劳 0.2 + 0.04×3 = 0.32；军纪不变
            Assert.That(after.Morale, Is.LessThan(state.Morale));
            Assert.That(after.Fatigue, Is.GreaterThan(state.Fatigue));
            Assert.That(after.Discipline, Is.EqualTo(state.Discipline), "断粮不改军纪（三维独立）。");
        }

        // ---- 构造不变量 ----

        [Test]
        public void test_state_rejects_morale_out_of_unit_range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CohesionState(U, 100, FixedPoint.FromInt(2), FixedPoint.Zero, FixedPoint.Zero));
        }
    }
}
