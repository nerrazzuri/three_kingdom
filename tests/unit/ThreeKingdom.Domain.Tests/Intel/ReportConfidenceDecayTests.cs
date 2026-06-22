using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Intel
{
    /// <summary>
    /// epic-005 story-002：报告置信/时效/区间与确定性暴露。
    /// 治理 ADR：ADR-0004（确定性随机流；定点）。GDD_007 / TR-intel-002。
    /// 覆盖 AC-1 三信号（来源可靠性/时效/区间，非单一百分比）、AC-2 时效衰减权威归 007、
    /// AC-3 暴露确定性（同流位置→同结果）、AC-4 区间随时效变宽。
    /// </summary>
    [TestFixture]
    public class ReportConfidenceDecayTests
    {
        private static readonly IntelAssessmentService Assessor = new IntelAssessmentService();
        private static readonly ScoutingExposureService Exposure = new ScoutingExposureService();
        private static readonly IntelSubjectId Enemy = new IntelSubjectId("enemy-host");
        private static readonly FactionId Wei = new FactionId("wei");

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static IntelConfig Config(int baseError = 4000, int ttl = 10)
            => new IntelConfig(
                new Dictionary<IntelSource, FixedPoint>
                {
                    [IntelSource.DirectObservation] = F(9, 10),
                    [IntelSource.Scouting] = F(7, 10),
                    [IntelSource.Rumor] = F(4, 10),
                    [IntelSource.Captured] = F(6, 10),
                },
                baseError, ttl,
                baseExposure: F(2, 10), exposureAlertWeight: F(1, 2), exposureSkillWeight: F(3, 10));

        private static IntelReport Report(int strength, WorldTime observedAt, IntelSource source = IntelSource.Scouting)
            => new IntelReport(Enemy, Wei, strength, source, observedAt);

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

        // ---- AC-1: 三信号（非单一百分比）----

        [Test]
        public void test_assessment_carries_three_distinct_signals()
        {
            var report = Report(5000, new WorldTime(0, DaySegment.Dawn));
            var assessment = Assessor.Assess(report, new WorldTime(1, DaySegment.Dawn), Config());

            // 信号 1：来源可靠性（非真实概率）
            Assert.That(assessment.Signals.SourceReliability, Is.EqualTo(F(7, 10)));
            // 信号 2：时效（age + freshness）
            Assert.That(assessment.Signals.Age, Is.GreaterThan(0));
            Assert.That(assessment.Signals.Freshness, Is.LessThan(FixedPoint.One));
            // 信号 3：估计区间（范围而非点值）
            Assert.That(assessment.Interval.Center, Is.EqualTo(5000));
            Assert.That(assessment.Interval.Upper, Is.GreaterThan(assessment.Interval.Lower));
        }

        // ---- AC-2: 时效衰减（权威归 007）----

        [Test]
        public void test_freshness_is_full_at_observation_time()
        {
            var t = new WorldTime(2, DaySegment.Day);
            var assessment = Assessor.Assess(Report(5000, t), t, Config());

            Assert.That(assessment.Signals.Age, Is.EqualTo(0));
            Assert.That(assessment.Signals.Freshness, Is.EqualTo(FixedPoint.One));
            Assert.That(assessment.Signals.EffectiveConfidence, Is.EqualTo(F(7, 10)));
        }

        [Test]
        public void test_freshness_decays_with_age_and_lowers_effective_confidence()
        {
            var observed = new WorldTime(0, DaySegment.Dawn);
            var early = Assessor.Assess(Report(5000, observed), observed.Advance(2), Config(ttl: 10));
            var late = Assessor.Assess(Report(5000, observed), observed.Advance(6), Config(ttl: 10));

            Assert.That(late.Signals.Freshness, Is.LessThan(early.Signals.Freshness));
            Assert.That(late.Signals.EffectiveConfidence, Is.LessThan(early.Signals.EffectiveConfidence));
        }

        [Test]
        public void test_report_past_ttl_is_marked_expired_not_deleted()
        {
            var observed = new WorldTime(0, DaySegment.Dawn);
            var assessment = Assessor.Assess(Report(5000, observed), observed.Advance(12), Config(ttl: 10));

            Assert.That(assessment.Signals.Freshness, Is.EqualTo(FixedPoint.Zero));
            Assert.That(assessment.Signals.Expired, Is.True);
            Assert.That(assessment.Signals.EffectiveConfidence, Is.EqualTo(FixedPoint.Zero));
            // 报告仍可评估（未删除），区间退化到最宽
            Assert.That(assessment.Interval.Center, Is.EqualTo(5000));
        }

        // ---- AC-4: 区间随时效变宽（确定性）----

        [Test]
        public void test_estimate_interval_widens_as_freshness_decays()
        {
            var observed = new WorldTime(0, DaySegment.Dawn);
            var fresh = Assessor.Assess(Report(5000, observed), observed, Config(ttl: 10));
            var stale = Assessor.Assess(Report(5000, observed), observed.Advance(6), Config(ttl: 10));

            Assert.That(stale.Interval.HalfWidth, Is.GreaterThan(fresh.Interval.HalfWidth));
        }

        [Test]
        public void test_assessment_is_deterministic()
        {
            var observed = new WorldTime(1, DaySegment.Dusk);
            var now = observed.Advance(3);
            var a = Assessor.Assess(Report(5000, observed), now, Config());
            var b = Assessor.Assess(Report(5000, observed), now, Config());

            Assert.That(b.Signals.EffectiveConfidence, Is.EqualTo(a.Signals.EffectiveConfidence));
            Assert.That(b.Interval.HalfWidth, Is.EqualTo(a.Interval.HalfWidth));
        }

        // ---- AC-3: 暴露由确定性随机流判定 ----

        [Test]
        public void test_exposure_triggers_when_rng_below_probability()
        {
            // P = 0.2 + 0.5×0.4 − 0.3×0.8 = 0.16；r=0.1 < 0.16 → 暴露
            var result = Exposure.Resolve(new FakeRandom(F(1, 10)), alert: F(4, 10), executorCapability: F(8, 10), Config());

            Assert.That(result.Exposed, Is.True);
        }

        [Test]
        public void test_exposure_avoided_when_rng_at_or_above_probability()
        {
            var result = Exposure.Resolve(new FakeRandom(F(5, 10)), alert: F(4, 10), executorCapability: F(8, 10), Config());

            Assert.That(result.Exposed, Is.False);
        }

        [Test]
        public void test_exposure_is_deterministic_for_same_stream_position()
        {
            var rngA = new DeterministicRandom(seed: 12345UL);
            var rngB = new DeterministicRandom(seed: 12345UL);

            var a = Exposure.Resolve(rngA, F(4, 10), F(8, 10), Config());
            var b = Exposure.Resolve(rngB, F(4, 10), F(8, 10), Config());

            Assert.That(b.Probability, Is.EqualTo(a.Probability));
            Assert.That(b.Exposed, Is.EqualTo(a.Exposed));
        }

        // ---- 构造不变量 ----

        [Test]
        public void test_config_rejects_zero_ttl()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config(ttl: 0));
        }

        [Test]
        public void test_assess_rejects_read_time_before_observation()
        {
            var observed = new WorldTime(5, DaySegment.Day);
            Assert.Throws<ArgumentException>(
                () => Assessor.Assess(Report(5000, observed), new WorldTime(4, DaySegment.Day), Config()));
        }
    }
}
