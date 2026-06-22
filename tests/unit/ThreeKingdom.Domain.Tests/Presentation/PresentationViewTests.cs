using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Cohesion;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Relationships;
using ThreeKingdom.Domain.Supply;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation;
using ThreeKingdom.Presentation.Projections;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-001：投影→展示模型（确定性 + 忠实映射）。
    /// 治理 ADR：ADR-0002（只读投影→展示模型）。覆盖 AC-1/AC-6 确定性、敌方探报映射（报告值非真值）、
    /// 三维/四维分列忠实映射。
    /// </summary>
    [TestFixture]
    public class PresentationViewTests
    {
        private static readonly IntelSubjectId Enemy = new IntelSubjectId("enemy-main-host");
        private static readonly FactionId Wei = new FactionId("wei");
        private static readonly WorldTime T0 = new WorldTime(3, DaySegment.Day);

        private static IntelProjection ProjectionWithReport(int reportedStrength)
        {
            var intel = new FactionIntel(Wei);
            intel.ApplyReport(new IntelReport(Enemy, Wei, reportedStrength, IntelSource.Scouting, T0));
            return intel.Project();
        }

        // ---- 敌方探报映射：展示估计值=报告值（非真值）----

        [Test]
        public void test_enemy_view_shows_reported_estimate_from_projection()
        {
            var panel = EnemyIntelPanelView.FromProjection(ProjectionWithReport(1000));

            Assert.That(panel.Entries.Count, Is.EqualTo(1));
            Assert.That(panel.Entries[0].EstimatedStrength, Is.EqualTo(1000), "展示的是报告估计值。");
            Assert.That(panel.Entries[0].SubjectLabel, Is.EqualTo(Enemy.ToString()));
        }

        // ---- AC-1/AC-6: 确定性（同投影 → 同展示模型）----

        [Test]
        public void test_enemy_panel_is_deterministic_for_same_projection()
        {
            var a = EnemyIntelPanelView.FromProjection(ProjectionWithReport(1000));
            var b = EnemyIntelPanelView.FromProjection(ProjectionWithReport(1000));

            Assert.That(b.Entries.Count, Is.EqualTo(a.Entries.Count));
            for (int i = 0; i < a.Entries.Count; i++)
            {
                Assert.That(b.Entries[i].SubjectLabel, Is.EqualTo(a.Entries[i].SubjectLabel));
                Assert.That(b.Entries[i].EstimatedStrength, Is.EqualTo(a.Entries[i].EstimatedStrength));
            }
        }

        // ---- 凝聚力三维分列忠实映射 ----

        [Test]
        public void test_cohesion_view_maps_three_independent_dimensions()
        {
            var state = new CohesionState(new UnitId("u-1"), 800,
                FixedPoint.FromFraction(7, 10), FixedPoint.FromFraction(3, 10), FixedPoint.FromFraction(6, 10));

            var view = CohesionView.FromState(state);

            Assert.That(view.Headcount, Is.EqualTo(800));
            Assert.That(view.Morale, Is.EqualTo(Display.ToDecimal(state.Morale)));
            Assert.That(view.Fatigue, Is.EqualTo(Display.ToDecimal(state.Fatigue)));
            Assert.That(view.Discipline, Is.EqualTo(Display.ToDecimal(state.Discipline)));
            // 三维各异，证明未被折叠成单值。
            Assert.That(view.Morale, Is.Not.EqualTo(view.Fatigue));
        }

        // ---- 关系四维分列 + 方向性 ----

        [Test]
        public void test_relationship_view_maps_four_directional_dimensions()
        {
            var rel = new RelationshipState();
            var guan = new CharacterId("char-guan");
            var zhang = new CharacterId("char-zhang");
            rel.ApplyEvent(new RelationshipEvent("ev-1", zhang, new[] { guan },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 30 }, "并肩死守"));

            var view = RelationshipView.FromState(rel, guan, zhang);

            Assert.That(view.FromLabel, Is.EqualTo(guan.ToString()));
            Assert.That(view.ToLabel, Is.EqualTo(zhang.ToString()));
            Assert.That(view.Trust, Is.EqualTo(30));
            Assert.That(view.Respect, Is.EqualTo(0));
            // 方向性：反向无值。
            var reverse = RelationshipView.FromState(rel, zhang, guan);
            Assert.That(reverse.Trust, Is.EqualTo(0));
        }

        // ---- 军师建议集映射：并列、过时标记、定性置信 ----

        [Test]
        public void test_council_view_maps_parallel_advice_with_staleness()
        {
            var advisor = new AdvisorId("advisor-zhuge");
            var advice = new List<AdviceStatement>
            {
                new AdviceStatement(advisor, "route-ambush", "敌追兵骄进", "地形可设伏",
                    new[] { "诱敌深入" }, new[] { "敌不追则徒劳" }, new IntelSubjectId[0], FixedPoint.FromFraction(8, 10)),
                new AdviceStatement(advisor, "route-siege", "城坚粮足", "可久守",
                    new[] { "守军士气" }, new[] { "外援断绝" }, new IntelSubjectId[0], FixedPoint.FromFraction(2, 10)),
            };
            var set = new CouncilAdviceSet(new KnowledgeSnapshotId("snap-1"), advice);

            var sameSnap = CouncilView.FromSet(set, new KnowledgeSnapshotId("snap-1"));
            var newSnap = CouncilView.FromSet(set, new KnowledgeSnapshotId("snap-2"));

            Assert.That(sameSnap.Advice.Count, Is.EqualTo(2), "并列两条，无折叠/排序。");
            Assert.That(sameSnap.IsStale, Is.False);
            Assert.That(newSnap.IsStale, Is.True, "知识快照变化后标过时（不静默重算）。");
            // 置信为定性标签而非数值百分比。
            Assert.That(sameSnap.Advice[0].EvidenceConfidenceLabel, Is.EqualTo("依据扎实"));
            Assert.That(sameSnap.Advice[1].EvidenceConfidenceLabel, Is.EqualTo("依据薄弱"));
        }
    }
}
