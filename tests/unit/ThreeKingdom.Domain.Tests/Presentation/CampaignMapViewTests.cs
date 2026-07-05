using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// 战略大地图投影（campaign map scaffold 数据源）：世界城归属 + 势力领城 + 纪元 → 纯 C# 地图单元。
    /// 无 Unity 依赖；坐标/邻接由 Unity 侧 MapLayoutData 补。
    /// </summary>
    [TestFixture]
    public class CampaignMapViewTests
    {
        [Test]
        public void test_map_projects_all_cities_with_owner_and_capital_flag()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            CampaignMapView map = rt.MapView();

            Assert.That(map.Cities.Count, Is.GreaterThanOrEqualTo(36), "全部世界城入图。");
            Assert.That(map.Year, Is.EqualTo(190));

            MapCityCell xuchang = null!;
            foreach (MapCityCell c in map.Cities) if (c.CityId == "city-xuchang") xuchang = c;
            Assert.That(xuchang, Is.Not.Null);
            Assert.That(xuchang.CityName, Is.EqualTo("许昌"));
            Assert.That(xuchang.OwnerFactionId, Is.EqualTo(PlayableCampaign.Cao.Value), "许昌属曹操。");
            Assert.That(xuchang.OwnerName, Is.EqualTo("曹操"));
            Assert.That(xuchang.IsCapital, Is.True, "许昌为曹操治所（首城）。");

            MapCityCell chenliu = null!;
            foreach (MapCityCell c in map.Cities) if (c.CityId == "city-chenliu") chenliu = c;
            Assert.That(chenliu.IsCapital, Is.False, "陈留非治所。");
        }

        [Test]
        public void test_map_projects_factions_with_player_flag()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            CampaignMapView map = rt.MapView();

            MapFactionCell player = null!, cao = null!;
            foreach (MapFactionCell f in map.Factions)
            {
                if (f.FactionId == PlayableCampaign.Player.Value) player = f;
                if (f.FactionId == PlayableCampaign.Cao.Value) cao = f;
            }
            Assert.That(player, Is.Not.Null);
            Assert.That(player.IsPlayer, Is.True, "玩家势力标记。");
            Assert.That(cao.IsPlayer, Is.False);
            Assert.That(cao.FactionName, Is.EqualTo("曹操"));
            Assert.That(cao.CityCount, Is.EqualTo(4), "曹操 4 城。");
        }
    }
}
