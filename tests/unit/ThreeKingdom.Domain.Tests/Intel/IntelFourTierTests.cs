using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Intel
{
    /// <summary>
    /// epic-005 story-001：情报四层分离与只读投影。
    /// 治理 ADR：ADR-0002（架构分层；显示层只读投影，不触真值）。GDD_007 / TR-intel-001。
    /// 覆盖 AC-1 投影无真值泄露（含报告/真值冲突时投影只含报告）、AC-2 四层独立
    /// （更新观察不改真值）、投影只读快照。
    /// </summary>
    [TestFixture]
    public class IntelFourTierTests
    {
        private static readonly IntelService Service = new IntelService();
        private static readonly IntelSubjectId Enemy = new IntelSubjectId("enemy-main-host");
        private static readonly FactionId Wei = new FactionId("wei");
        private static readonly FactionId Shu = new FactionId("shu");
        private static readonly WorldTime T0 = new WorldTime(3, DaySegment.Day);

        private static WorldTruthLedger TruthWith(int strength)
        {
            var truth = new WorldTruthLedger();
            truth.Set(new TruthRecord(Enemy, strength, Shu));
            return truth;
        }

        // ---- AC-1: 投影无真值泄露 ----

        [Test]
        public void test_projection_does_not_contain_unobserved_truth()
        {
            // Arrange：真值含未被观察的敌情
            var truth = TruthWith(80);
            var intel = new FactionIntel(Wei);

            // Act：未侦察 → 阵营知识为空
            var projection = intel.Project();

            // Assert：投影不含该主题（真值未泄露）
            Assert.That(projection.Knows(Enemy), Is.False);
            Assert.That(projection.Count, Is.EqualTo(0));
            Assert.That(truth.Get(Enemy).ActualStrength, Is.EqualTo(80), "真值仍在真值层。");
        }

        [Test]
        public void test_projection_reflects_report_not_live_truth_on_conflict()
        {
            // Arrange：侦察得真值 80 → 报告 → 知识
            var truth = TruthWith(80);
            var intel = new FactionIntel(Wei);
            var obs = Service.Observe(truth, Enemy, Wei, T0);
            intel.ApplyReport(Service.ToReport(obs, Wei, IntelSource.Scouting));

            // Act：真值此后变为 50（投影不应跟随真值）
            truth.SetStrength(Enemy, 50);
            var projection = intel.Project();

            // Assert：投影只含报告值 80，与当前真值 50 冲突——证明投影非真值背书
            Assert.That(projection.TryGet(Enemy, out var entry), Is.True);
            Assert.That(entry.KnownStrength, Is.EqualTo(80));
            Assert.That(truth.Get(Enemy).ActualStrength, Is.EqualTo(50));
        }

        // ---- AC-2: 四层独立 ----

        [Test]
        public void test_observation_snapshot_does_not_mutate_truth()
        {
            // Arrange
            var truth = TruthWith(80);

            // Act：侦察产生观察 + 报告
            var obs = Service.Observe(truth, Enemy, Wei, T0);
            var report = Service.ToReport(obs, Wei, IntelSource.DirectObservation);

            // Assert：观察/报告携带快照值，真值未被结算改动
            Assert.That(obs.ObservedStrength, Is.EqualTo(80));
            Assert.That(report.ReportedStrength, Is.EqualTo(80));
            Assert.That(truth.Get(Enemy).ActualStrength, Is.EqualTo(80));
        }

        [Test]
        public void test_truth_change_after_observation_leaves_prior_observation_intact()
        {
            // Arrange：先观察（真值 80）
            var truth = TruthWith(80);
            var obs = Service.Observe(truth, Enemy, Wei, T0);

            // Act：真值变 120，再观察一次
            truth.SetStrength(Enemy, 120);
            var obs2 = Service.Observe(truth, Enemy, Wei, T0.Advance(1));

            // Assert：旧观察不变（历史快照），新观察反映新真值
            Assert.That(obs.ObservedStrength, Is.EqualTo(80));
            Assert.That(obs2.ObservedStrength, Is.EqualTo(120));
        }

        // ---- 投影只读快照 ----

        [Test]
        public void test_projection_is_immutable_snapshot()
        {
            // Arrange
            var truth = TruthWith(80);
            var intel = new FactionIntel(Wei);
            intel.ApplyReport(Service.ToReport(Service.Observe(truth, Enemy, Wei, T0), Wei, IntelSource.Scouting));
            var snapshot = intel.Project();

            // Act：投影后再加新主题
            var other = new IntelSubjectId("enemy-convoy");
            truth.Set(new TruthRecord(other, 30, Shu));
            intel.ApplyReport(Service.ToReport(Service.Observe(truth, other, Wei, T0), Wei, IntelSource.Rumor));

            // Assert：先前快照不随后续变化（只读快照）
            Assert.That(snapshot.Count, Is.EqualTo(1));
            Assert.That(snapshot.Knows(other), Is.False);
            Assert.That(intel.Project().Count, Is.EqualTo(2));
        }

        // ---- 构造/归属不变量 ----

        [Test]
        public void test_apply_report_rejects_foreign_faction()
        {
            var truth = TruthWith(80);
            var intel = new FactionIntel(Wei);
            var shuReport = Service.ToReport(Service.Observe(truth, Enemy, Shu, T0), Shu, IntelSource.Scouting);

            Assert.Throws<InvalidOperationException>(() => intel.ApplyReport(shuReport));
        }

        [Test]
        public void test_truth_rejects_negative_strength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TruthRecord(Enemy, -1, Shu));
        }

        [Test]
        public void test_observe_unknown_subject_throws()
        {
            var truth = new WorldTruthLedger();
            Assert.Throws<KeyNotFoundException>(() => Service.Observe(truth, Enemy, Wei, T0));
        }
    }
}
