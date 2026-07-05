using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// 生涯视图（GDD_014 / W5）：官阶中文头衔（忠臣线八阶）+ 功绩/名望 + 在野。开局为太守，功名皆零。
    /// </summary>
    [TestFixture]
    public class CareerViewTests
    {
        [Test]
        public void test_rank_titles_cover_all_eight_tiers()
        {
            Assert.That(CareerView.TitleOf(Rank.CityGovernor), Is.EqualTo("太守"));
            Assert.That(CareerView.TitleOf(Rank.ProvincialInspector), Is.EqualTo("刺史"));
            Assert.That(CareerView.TitleOf(Rank.GrandCommander), Is.EqualTo("大都督"));
            Assert.That(CareerView.TitleOf(Rank.Successor), Is.EqualTo("继业之主"));
            // 每阶都有中文头衔（非枚举名）。
            foreach (Rank r in System.Enum.GetValues(typeof(Rank)))
                Assert.That(CareerView.TitleOf(r), Is.Not.EqualTo(r.ToString()), $"{r} 应有中文头衔。");
        }

        [Test]
        public void test_runtime_career_view_at_start()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            CareerView c = runtime.CareerView();
            Assert.That(c.RankTitle, Is.EqualTo("太守"), "开局即一城太守。");
            Assert.That(c.Merit, Is.EqualTo(0), "功绩自零始。");
            Assert.That(c.Renown, Is.EqualTo(0), "名望自零始。");
            Assert.That(c.IsUnaffiliated, Is.False, "开局奉主，非在野。");
        }
    }
}
