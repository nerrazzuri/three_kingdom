using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// HUD 席位目标随所选开局真实投影（GDD_026）——回归：此前 HUD 顶栏硬编码「汜水关太守」，
    /// 无论选何开局都显汜水关。修复后须反映真实治所/宗主/锋芒。
    /// </summary>
    [TestFixture]
    public class SeatObjectiveTests
    {
        private static CampaignRuntime Runtime(PlayableStart start)
        {
            var r = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(start));
            r.NewGame();
            return r;
        }

        [Test]
        public void test_named_liubei_xiaopei_start_shows_xiaopei_not_fanshui()
        {
            // Arrange / Act：玩家选「刘玄德·小沛」命名开局。
            string objective = Runtime(PlayableStartCatalog.LiubeiXiaopei).SeatObjective;

            // Assert：显示小沛开局，绝不再冒出汜水关。
            Assert.That(objective, Does.Contain("小沛"), "席位目标应反映所选治所小沛。");
            Assert.That(objective, Does.Not.Contain("汜水关"), "回归：不得再硬编码汜水关。");
        }

        [Test]
        public void test_governor_start_shows_city_and_suzerain()
        {
            // Arrange / Act：空降为陈留太守（宗主曹操）。
            string objective = Runtime(PlayableCampaign.GovernorStartOf(new CityId("city-chenliu"))).SeatObjective;

            // Assert：治所 + 宗主中文名皆真实投影。
            Assert.That(objective, Does.Contain("陈留太守"), "治所中文名 + 太守身份。");
            Assert.That(objective, Does.Contain("奉曹操号令"), "宗主中文名。");
            Assert.That(objective, Does.Not.Contain("汜水关"));
        }

        [Test]
        public void test_default_fanshui_start_still_reads_fanshui()
        {
            // 默认剧本仍应显示汜水关（此开局本就是汜水关太守）。
            string objective = Runtime(PlayableStartCatalog.FanshuiGovernor).SeatObjective;
            Assert.That(objective, Does.Contain("汜水关"), "默认剧本仍是汜水关太守。");
        }
    }
}
