using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 任选城太守开局（GDD_026 R3/R4 / ADR-0015 D3/D4）：玩家空降为任一非君主治所城的太守，治所自宗主划出，
    /// 该年该城在职武将尽归调遣。争霸盘城数守恒（宗主 −1、玩家 +1）。默认「汜水关太守」仍字节级不变。
    /// </summary>
    [TestFixture]
    public class GovernorStartTests
    {
        private static CityId City(string id) => new CityId(id);

        [Test]
        public void test_selectable_cities_exclude_ruler_seats()
        {
            var sel = PlayableCampaign.SelectableGovernorCities();
            Assert.That(PlayableCampaign.IsRulerSeat(City("city-xuchang")), Is.True, "许昌=曹操治所。");
            Assert.That(PlayableCampaign.IsRulerSeat(City("city-chenliu")), Is.False, "陈留非治所。");
            Assert.That(sel, Does.Not.Contain(City("city-xuchang")), "君主治所不可选。");
            Assert.That(sel, Does.Contain(City("city-chenliu")), "非治所城可做太守。");
            Assert.That(sel, Does.Not.Contain(City("city-xiaopei")), "小沛为刘备唯一城=治所，不可选。");
        }

        [Test]
        public void test_governor_start_carves_city_and_hands_over_generals()
        {
            PlayableStart start = PlayableCampaign.GovernorStartOf(City("city-chenliu"));
            Assert.That(start.Suzerain, Is.EqualTo(PlayableCampaign.Cao), "陈留宗主=曹操。");
            Assert.That(start.SubordinateGenerals.Count, Is.EqualTo(6), "该年陈留在职 6 部将归你。");

            PlayableCampaign camp = PlayableCampaign.ForStart(start);
            Assert.That(camp.PlayerCapital, Is.EqualTo(City("city-chenliu")));
            Assert.That(camp.DeputyRoster.Count, Is.EqualTo(6), "副将花名册=该城部将。");

            ContentionState c = camp.InitialContention();
            Assert.That(c.CitiesOf(PlayableCampaign.Player), Is.EqualTo(1), "太守持陈留 1 城。");
            Assert.That(c.CitiesOf(PlayableCampaign.Cao), Is.EqualTo(3), "曹操被划走陈留 → 4−1=3 城。");
            // 扮太守用 36 座真实世界城（陈留仅易主 Cao→玩家）；汜水关为默认剧本额外虚构城，故此局不含。
            Assert.That(c.TotalCities, Is.EqualTo(36), "城数守恒（陈留易主，非新增）。");
        }

        [Test]
        public void test_runtime_runs_as_governor_of_chosen_city()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium(),
                PlayableCampaign.ForStart(PlayableCampaign.GovernorStartOf(City("city-chenliu"))));
            runtime.NewGame();

            Assert.That(runtime.CurrentYear, Is.EqualTo(190));
            Assert.That(runtime.Scenario.PlayerCapital, Is.EqualTo(City("city-chenliu")));
            Assert.That(runtime.DeputyRoster.Count, Is.EqualTo(6), "开局即掌该城将佐。");
            Assert.That(runtime.Endgame(), Is.EqualTo(EndgameStatus.Ongoing), "一城太守，支配度低→争霸。");
        }

        [Test]
        public void test_ruler_seat_cannot_be_chosen()
        {
            Assert.Throws<System.ArgumentException>(() => PlayableCampaign.GovernorStartOf(City("city-xuchang")),
                "许昌为君主治所，不可为太守。");
        }

        [Test]
        public void test_city_choice_view_labels_in_chinese()
        {
            GovernorCityChoiceView v = GovernorCityChoiceView.Build(190);
            GovernorCityChoiceLine chenliu = null!;
            foreach (GovernorCityChoiceLine l in v.Choices) if (l.CityId == "city-chenliu") chenliu = l;
            Assert.That(chenliu, Is.Not.Null);
            Assert.That(chenliu.CityName, Is.EqualTo("陈留"));
            Assert.That(chenliu.SuzerainName, Is.EqualTo("曹操"), "宗主中文名。");
            Assert.That(chenliu.GeneralCount, Is.EqualTo(6), "该年该城在职部将数。");

            foreach (GovernorCityChoiceLine l in v.Choices)
                Assert.That(l.CityId, Is.Not.EqualTo("city-xuchang"), "选城屏不含君主治所。");
        }
    }
}
