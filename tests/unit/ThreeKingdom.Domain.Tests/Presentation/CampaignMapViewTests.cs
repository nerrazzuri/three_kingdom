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

        [Test]
        public void test_map_places_available_heroes_on_their_cities()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            CampaignMapView map = rt.MapView();

            Assert.That(map.Heroes.Count, Is.GreaterThan(0), "地图上有在场武将棋子。");
            MapHeroCell guanyu = null!;
            foreach (MapHeroCell h in map.Heroes) if (h.HeroId == "char-guanyu") guanyu = h;
            Assert.That(guanyu, Is.Not.Null, "关羽 190 在场。");
            Assert.That(guanyu.HeroName, Is.EqualTo("关羽"));
            Assert.That(guanyu.CityId, Is.EqualTo("city-xiaopei"), "随刘备在小沛。");
            Assert.That(guanyu.FactionId, Is.EqualTo(PlayableCampaign.LiuBei.Value), "效力随所在城归属（刘备）。");

            // 反全知：董卓（非名将、在敌境洛阳）身份不泄，只显「未探明」；孔明 190 尚幼 → 不在图。
            bool dongLeaked = false, hasKongming = false, luoyangHidden = false;
            foreach (MapHeroCell h in map.Heroes)
            {
                if (h.HeroId == "char-dongzhuo" || h.HeroName == "董卓") dongLeaked = true;
                if (h.HeroId == "char-zhugeliang") hasKongming = true;
                if (!h.Known && h.CityId == "city-luoyang") luoyangHidden = true;
            }
            Assert.That(dongLeaked, Is.False, "董卓在敌境·非名将 → 未探明，身份不泄（反全知）。");
            Assert.That(luoyangHidden, Is.True, "洛阳有一员未探明之敌将。");
            Assert.That(hasKongming, Is.False, "孔明 190 尚幼未出仕 → 不在图（生卒驱动）。");
        }

        [Test]
        public void test_map_fog_reveals_own_and_legends_hides_obscure_enemy()
        {
            // 反全知棋子（GDD_026 R6）：己方城之将、传世名将露真名；敌境无名之将「未探明」。
            var rt = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.LiubeiXiaopei));
            rt.NewGame();
            CampaignMapView map = rt.MapView();

            MapHeroCell guanyu = null!, gaoshun = null!;
            foreach (MapHeroCell h in map.Heroes)
            {
                if (h.HeroId == "char-guanyu") guanyu = h;                       // 己方·小沛
                if (h.CityId == "city-xiapi" && !h.Known) gaoshun = h;           // 敌境下邳·未探明（高顺）
            }
            Assert.That(guanyu, Is.Not.Null, "己方名将关羽露真名。");
            Assert.That(guanyu.Known, Is.True);
            Assert.That(gaoshun, Is.Not.Null, "下邳吕布之将未探明。");
            Assert.That(gaoshun.HeroName, Is.EqualTo("未探明"));
            Assert.That(gaoshun.HeroId, Is.Empty, "未探明者不泄 id。");
        }

        [Test]
        public void test_scouting_target_faction_reveals_its_generals()
        {
            // 「可被发觉」：派探敌情后，目标势力（吕布）之将由未探明转露真名——高顺现形于下邳。
            var rt = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.LiubeiXiaopei));
            rt.NewGame();
            Assert.That(Named(rt.MapView(), "char-gaoshun"), Is.False, "探知前高顺未探明。");

            rt.ScoutEnemy();
            Assert.That(Named(rt.MapView(), "char-gaoshun"), Is.True, "派探后发觉高顺据下邳。");
        }

        private static bool Named(CampaignMapView map, string heroId)
        {
            foreach (MapHeroCell h in map.Heroes) if (h.HeroId == heroId) return true;
            return false;
        }
    }
}
