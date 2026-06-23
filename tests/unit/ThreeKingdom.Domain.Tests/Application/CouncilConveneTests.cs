using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续（Phase B1）：军议条件化建议（BLOCKING）。
    /// 治理 ADR：ADR-0002 + GDD_008 + 强制设计锁 P11（并列无最优解、过时不静默更新、置信非成功率）。
    /// </summary>
    [TestFixture]
    public class CouncilConveneTests
    {
        [Test]
        public void test_convene_returns_parallel_advice_for_all_templates()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var (set, _) = service.Convene(session);

            Assert.That(set, Is.Not.Null);
            Assert.That(set!.Advice, Has.Count.EqualTo(3), "三条条件化建议并列呈现（断粮/守城/伏击）。");
        }

        [Test]
        public void test_council_not_convened_yet_is_null()
        {
            var service = new SessionService();

            var (set, _) = service.ProjectCouncil(service.NewGame());

            Assert.That(set, Is.Null, "未召开军议则无建议集。");
        }

        [Test]
        public void test_advice_marks_missing_intel_before_scouting()
        {
            var service = new SessionService();
            var session = service.NewGame(); // 未侦察 → 敌情未知

            var (set, snapshot) = service.Convene(session);
            var view = CouncilView.FromSet(set!, snapshot);

            // 未侦察：建议应识别出缺失情报（依据敌情主题未知）。
            bool anyMissing = false;
            foreach (var a in view.Advice) if (a.MissingIntel.Count > 0) anyMissing = true;
            Assert.That(anyMissing, Is.True, "未侦察时军师应提示缺失情报。");
        }

        [Test]
        public void test_advice_goes_stale_after_scouting_changes_knowledge()
        {
            var service = new SessionService();
            var session = service.NewGame();
            var (set, snapAtConvene) = service.Convene(session); // 召开（知识=空）

            service.DispatchScout(session);                       // 侦察非即时
            service.Advance(session, SliceScenario.Default().ScoutLeadSegments); // 返报 → 知识变 → 快照变

            var current = service.ProjectCouncil(session).Snapshot;
            var view = CouncilView.FromSet(set!, current);
            Assert.That(view.IsStale, Is.True, "侦察后知识快照变化 → 已召开建议过时（不静默更新）。");
            Assert.That(current, Is.Not.EqualTo(snapAtConvene));
        }

        [Test]
        public void test_advice_view_confidence_is_qualitative_not_percentage()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.DispatchScout(session);                       // 派出侦察
            service.Advance(session, SliceScenario.Default().ScoutLeadSegments); // 返报 → 有依据
            var (set, snapshot) = service.Convene(session);

            var view = CouncilView.FromSet(set!, snapshot);

            foreach (var a in view.Advice)
            {
                Assert.That(a.EvidenceConfidenceLabel, Does.StartWith("依据"), "置信为定性标签（依据薄弱/中等/扎实），非百分比。");
                Assert.That(a.EvidenceConfidenceLabel, Does.Not.Contain("%"));
            }
        }
    }
}
