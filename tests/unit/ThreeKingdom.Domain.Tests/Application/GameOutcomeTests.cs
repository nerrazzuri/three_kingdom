using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续（Phase A）：一局闭环——守城待变胜负条件 + 目标视图（BLOCKING）。
    /// 治理 ADR：ADR-0002（用例编排，UI 拿投影）+ ADR-0004（确定性）。
    /// </summary>
    [TestFixture]
    public class GameOutcomeTests
    {
        private static readonly SliceScenario Scenario = SliceScenario.Default();

        [Test]
        public void test_new_game_is_ongoing()
        {
            var service = new SessionService();

            var obj = service.ProjectObjective(service.NewGame());

            Assert.That(obj.Outcome, Is.EqualTo(GameOutcome.Ongoing));
            Assert.That(obj.ReliefDay, Is.EqualTo(Scenario.ReliefDay));
        }

        [Test]
        public void test_holding_until_relief_day_is_victory()
        {
            var service = new SessionService();
            var session = service.NewGame();

            // 推进至援军日（守城待变）。
            service.Advance(session, WorldTime.SegmentsPerDay * Scenario.ReliefDay);

            Assert.That(service.ProjectObjective(session).Outcome, Is.EqualTo(GameOutcome.Victory));
        }

        [Test]
        public void test_morale_collapse_is_defeat_and_takes_priority()
        {
            var service = new SessionService();
            var session = service.NewGame();

            // 推进足够多日：库存触底持续短缺 → 民心耗尽（≤0）→ 失城（败优先于胜）。
            service.Advance(session, WorldTime.SegmentsPerDay * 20);
            var obj = service.ProjectObjective(session);

            Assert.That(obj.Outcome, Is.EqualTo(GameOutcome.Defeat));
            Assert.That(obj.DefeatReason, Is.Not.Empty);
        }

        [Test]
        public void test_objective_view_shows_remaining_days_while_ongoing()
        {
            var view = new ObjectiveView(new ObjectiveProjection(0, 8, GameOutcome.Ongoing, string.Empty));

            Assert.That(view.IsOver, Is.False);
            Assert.That(view.ObjectiveLabel, Does.Contain("第 9 日"));
            Assert.That(view.ObjectiveLabel, Does.Contain("还余 8 日"));
            Assert.That(view.BannerLabel, Is.Empty);
        }

        [Test]
        public void test_objective_view_shows_victory_banner()
        {
            var view = new ObjectiveView(new ObjectiveProjection(8, 8, GameOutcome.Victory, string.Empty));

            Assert.That(view.IsOver, Is.True);
            Assert.That(view.IsVictory, Is.True);
            Assert.That(view.BannerLabel, Does.Contain("守住"));
        }

        [Test]
        public void test_objective_view_shows_defeat_banner_with_reason()
        {
            var view = new ObjectiveView(new ObjectiveProjection(5, 8, GameOutcome.Defeat, "民心崩溃，城池陷落。"));

            Assert.That(view.IsOver, Is.True);
            Assert.That(view.IsVictory, Is.False);
            Assert.That(view.BannerLabel, Does.Contain("城破"));
            Assert.That(view.BannerLabel, Does.Contain("民心崩溃"));
        }
    }
}
