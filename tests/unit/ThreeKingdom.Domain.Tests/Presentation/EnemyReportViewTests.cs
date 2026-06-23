using NUnit.Framework;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// EPIC_010 竖切续：敌情探报展示视图（BLOCKING）。治理 ADR：ADR-0002 + P10 不完全信息设计锁。
    /// 覆盖无情报提示、时效（刚侦察/已过 N 时段）、只呈现估计值（无真值字段来源）。
    /// </summary>
    [TestFixture]
    public class EnemyReportViewTests
    {
        private static readonly FactionId Player = new FactionId("玩家势力");
        private static readonly IntelSubjectId Subject = new IntelSubjectId("敌前锋");

        private static IntelProjection ProjectionWith(int strength, WorldTime observedAt)
        {
            var intel = new FactionIntel(Player);
            intel.ApplyReport(new IntelReport(Subject, Player, strength, IntelSource.Scouting, observedAt));
            return intel.Project();
        }

        [Test]
        public void test_no_intel_shows_actionable_empty_hint()
        {
            var view = new EnemyReportView(new FactionIntel(Player).Project(), new WorldTime(0, DaySegment.Dawn));

            Assert.That(view.HasIntel, Is.False);
            Assert.That(view.Lines, Is.Empty);
            Assert.That(view.EmptyLabel, Does.Contain("侦察"));
        }

        [Test]
        public void test_fresh_intel_marked_just_scouted()
        {
            var now = new WorldTime(0, DaySegment.Dawn);
            var view = new EnemyReportView(ProjectionWith(1000, now), now);

            Assert.That(view.HasIntel, Is.True);
            Assert.That(view.Lines, Has.Count.EqualTo(1));
            Assert.That(view.Lines[0], Does.Contain("敌前锋"));
            Assert.That(view.Lines[0], Does.Contain("估计 1000 兵"));
            Assert.That(view.Lines[0], Does.Contain("刚侦察"));
        }

        [Test]
        public void test_stale_intel_reports_segment_gap()
        {
            var observedAt = new WorldTime(0, DaySegment.Dawn);
            var now = new WorldTime(1, DaySegment.Day); // 绝对索引相差 5 个时段（4/日 + 1）
            long expectedGap = now.AbsoluteIndex - observedAt.AbsoluteIndex;

            var view = new EnemyReportView(ProjectionWith(1000, observedAt), now);

            Assert.That(view.Lines[0], Does.Contain("已过 " + expectedGap + " 个时段"));
        }
    }
}
