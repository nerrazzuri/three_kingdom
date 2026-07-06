using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 武将登场跨纪元验证（GDD_026 / ADR-0015）——核实三条契约：
    ///   R1 该出现的必须出现：同一武将随纪元在不同城布防（关羽 190小沛→194下邳→200汝南→208长沙→219江陵）。
    ///   R3 生卒门：未及冠者当年不出（姜维202→190/200不在），生年到则出（208+），身故则退（华雄191→200不在）。
    ///       且门键于<b>当前年</b>——同一局推进跨年，已故武将自动退出战略图（动态）。
    ///   R2 反全知探知：人才未知晓时隐藏、经渠道探知后方可见（现落地于人才招揽原型；棋子层为公开情报，另述）。
    /// 纯 C# 验证，无引擎依赖。
    /// </summary>
    [TestFixture]
    public class GeneralAppearanceAcrossErasTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);

        private static string CityOfGeneralAt(string generalId, int anchorYear)
        {
            foreach ((CharacterId g, CityId city) in GeneralDossiers.AllPlacements(anchorYear))
                if (g.Value == generalId) return city.Value;
            return null!;
        }

        [Test]
        public void test_r1_same_general_stationed_in_different_cities_across_eras()
        {
            // 关羽随刘备沉浮：治所随纪元而异——该出现处必出现，且城随史事迁移。
            Assert.That(CityOfGeneralAt("char-guanyu", 190), Is.EqualTo("city-xiaopei"), "190 随刘备寄小沛。");
            Assert.That(CityOfGeneralAt("char-guanyu", 194), Is.EqualTo("city-xiapi"), "194 保徐州下邳。");
            Assert.That(CityOfGeneralAt("char-guanyu", 200), Is.EqualTo("city-runan"), "200 寄汝南。");
            Assert.That(CityOfGeneralAt("char-guanyu", 208), Is.EqualTo("city-changsha"), "208 据荆南长沙。");
            Assert.That(CityOfGeneralAt("char-guanyu", 219), Is.EqualTo("city-jiangling"), "219 董督荆州·江陵。");
        }

        [Test]
        public void test_r3_general_absent_before_coming_of_age_present_after()
        {
            // 姜维 202 生 → 及冠(16)在 218 后：190/200/208 未出仕（且此三纪元未布防）；234 五丈原随孔明北伐现于汉中。
            Assert.That(GeneralDossiers.AvailableAt(C("char-jiangwei"), 190), Is.False, "190 姜维未生。");
            Assert.That(GeneralDossiers.AvailableAt(C("char-jiangwei"), 208), Is.False, "208 姜维方六岁。");
            Assert.That(GeneralDossiers.AvailableAt(C("char-jiangwei"), 234), Is.True, "234 姜维已仕。");
            Assert.That(CityOfGeneralAt("char-jiangwei", 234), Is.EqualTo("city-hanzhong"), "234 姜维随孔明驻汉中。");

            // 诸葛亮 181 生 → 190 年方九岁未出；208 隆中已出、随刘备据荆南。
            Assert.That(GeneralDossiers.AvailableAt(C("char-zhugeliang"), 190), Is.False, "190 孔明尚幼。");
            Assert.That(GeneralDossiers.AvailableAt(C("char-zhugeliang"), 208), Is.True, "208 孔明已出。");
            Assert.That(CityOfGeneralAt("char-zhugeliang", 208), Is.EqualTo("city-changsha"));
        }

        [Test]
        public void test_r3_general_leaves_after_death_year()
        {
            // 华雄 160–191：190 讨董在虎牢关；200 已亡，不再登场（且未布防）。
            Assert.That(GeneralDossiers.AvailableAt(C("char-huaxiong"), 190), Is.True, "190 华雄在。");
            Assert.That(CityOfGeneralAt("char-huaxiong", 190), Is.EqualTo("city-hulao"));
            Assert.That(GeneralDossiers.AvailableAt(C("char-huaxiong"), 200), Is.False, "200 华雄已亡。");

            // 关羽 160–220：219 在，221 已殁。
            Assert.That(GeneralDossiers.AvailableAt(C("char-guanyu"), 219), Is.True);
            Assert.That(GeneralDossiers.AvailableAt(C("char-guanyu"), 221), Is.False, "221 关羽已殁于临沮。");
        }

        [Test]
        public void test_r3_dynamic_general_drops_from_strategic_map_as_campaign_crosses_death_year()
        {
            // 端到端：襄樊(219)开局，战略图有关羽在江陵；推进过 220（关羽卒年）后，棋子自动退出——门键于当前年。
            var rt = new CampaignRuntime(new InMemorySaveMedium(), PlayableCampaign.ForStart(PlayableStartCatalog.GuanyuXiangfan));
            rt.NewGame();
            Assert.That(rt.CurrentYear, Is.EqualTo(219));
            Assert.That(HasHero(rt.MapView(), "char-guanyu"), Is.True, "219 关羽在战略图·江陵。");

            rt.AdvanceYear();
            rt.AdvanceYear();   // → 221
            Assert.That(rt.CurrentYear, Is.EqualTo(221));
            Assert.That(HasHero(rt.MapView(), "char-guanyu"), Is.False, "跨 220 后关羽自战略图退出（生卒门动态生效）。");
        }

        private static bool HasHero(CampaignMapView map, string heroId)
        {
            foreach (MapHeroCell h in map.Heroes) if (h.HeroId == heroId) return true;
            return false;
        }

        [Test]
        public void test_r2_recruitable_talent_hidden_until_discovered()
        {
            // 反全知探知：未探知的人才不入可见录；经渠道探知后方可见（GDD_020 落地于招揽原型）。
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            rt.Advance(1);   // 骁将 appearFrom 0，已登场

            bool VisibleHas(string talentId)
            {
                foreach (TalentRecruitLine t in rt.TalentView().Talents) if (t.TalentId == talentId) return true;
                return false;
            }

            Assert.That(VisibleHas("talent-xiaojiang"), Is.False, "未探知前隐藏（虽已登场）。");
            rt.RevealTalent(new ThreeKingdom.Domain.Talent.TalentId("talent-xiaojiang"), ThreeKingdom.Domain.Talent.TalentChannel.Scouting);
            Assert.That(VisibleHas("talent-xiaojiang"), Is.True, "经斥候探知后进入可见录。");
        }
    }
}
