using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 开局串联（GDD_026 §13）：选锚点年 → 选城/剧本 → 开局跑通。一条流程把纪元/选城/太守/一生连起来，MVP 可从头玩。
    /// </summary>
    [TestFixture]
    public class GameLauncherTests
    {
        [Test]
        public void test_launcher_lists_years_and_cities()
        {
            Assert.That(GameLauncher.AnchorYears()[0].Year, Is.EqualTo(190), "首个锚点年=190 讨董。");
            Assert.That(GameLauncher.NamedStarts().Choices.Count, Is.GreaterThanOrEqualTo(3), "命名剧本 ≥3。");
            Assert.That(GameLauncher.GovernorCities(190).Choices.Count, Is.GreaterThan(0), "有可做太守的城。");
        }

        [Test]
        public void test_start_governor_flow_runs_end_to_end()
        {
            CampaignRuntime rt = GameLauncher.StartGovernor("city-chenliu", new InMemorySaveMedium());
            Assert.That(rt.CurrentYear, Is.EqualTo(190));
            Assert.That(rt.Scenario.PlayerCapital, Is.EqualTo(new ThreeKingdom.Domain.City.CityId("city-chenliu")));
            Assert.That(rt.DeputyRoster.Count, Is.EqualTo(6), "陈留 6 部将归你。");
            Assert.That(rt.LifeView().Age, Is.EqualTo(20), "弱冠入场。");
            // 能推进（一生在纪元里流转）。
            rt.Advance(1);
            Assert.That(rt.Endgame(), Is.EqualTo(EndgameStatus.Ongoing));
        }

        [Test]
        public void test_start_named_flow_runs_end_to_end()
        {
            CampaignRuntime rt = GameLauncher.StartNamed("liubei-xiaopei", new InMemorySaveMedium());
            Assert.That(rt.Scenario.PlayerFaction, Is.EqualTo(PlayableCampaign.LiuBei));
            Assert.That(rt.CurrentYear, Is.EqualTo(190));
            rt.Advance(1);
            Assert.That(rt.Endgame(), Is.EqualTo(EndgameStatus.Ongoing));
        }

        [Test]
        public void test_unknown_named_start_rejected()
        {
            Assert.Throws<System.ArgumentException>(() => GameLauncher.StartNamed("no-such", new InMemorySaveMedium()));
        }
    }
}
