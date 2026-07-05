using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 武将生卒 + 锚点年布防（GDD_026 R4/F4 / ADR-0015 D4）：某年某城在职部将由生卒年 + 布防表决定。
    /// 名将随时代登场/老去——190 讨董之世，诸葛/司马尚幼未出，关张已随刘备；华雄在、至208已亡。
    /// </summary>
    [TestFixture]
    public class GeneralPlacementTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);

        [Test]
        public void test_availability_follows_birth_and_death_years()
        {
            // 诸葛亮 181 生 → 190 年仅 9 岁未出仕；208 年已可出仕。
            Assert.That(GeneralDossiers.AvailableAt(C("char-zhugeliang"), 190), Is.False, "190 孔明尚幼。");
            Assert.That(GeneralDossiers.AvailableAt(C("char-zhugeliang"), 208), Is.True, "208 孔明已出。");
            // 关羽 160 生、220 卒 → 190 在世出仕；280 已亡。
            Assert.That(GeneralDossiers.AvailableAt(C("char-guanyu"), 190), Is.True);
            Assert.That(GeneralDossiers.AvailableAt(C("char-guanyu"), 280), Is.False, "280 关羽早亡。");
            // 华雄 160–191 → 190 在、208 已亡。
            Assert.That(GeneralDossiers.AvailableAt(C("char-huaxiong"), 190), Is.True);
            Assert.That(GeneralDossiers.AvailableAt(C("char-huaxiong"), 208), Is.False);
            // 空降者无生卒登记 → 视为常在。
            Assert.That(GeneralDossiers.AvailableAt(C("char-player-lord"), 190), Is.True);
        }

        [Test]
        public void test_190_placement_puts_generals_in_their_faction_cities()
        {
            var xiaopei = GeneralDossiers.GeneralsAt(City("city-xiaopei"), 190);
            Assert.That(xiaopei, Does.Contain(C("char-guanyu")), "关羽随刘备在小沛。");
            Assert.That(xiaopei, Does.Contain(C("char-zhangfei")), "张飞随刘备在小沛。");
            Assert.That(xiaopei, Does.Not.Contain(C("char-zhaoyun")), "赵云此年尚在公孙瓒麾下（北平），非小沛。");

            var beiping = GeneralDossiers.GeneralsAt(City("city-beiping"), 190);
            Assert.That(beiping, Does.Contain(C("char-zhaoyun")), "赵云 190 事公孙瓒于北平。");
        }

        [Test]
        public void test_no_placement_data_for_other_years_or_empty_cities()
        {
            Assert.That(GeneralDossiers.GeneralsAt(City("city-xiaopei"), 208), Is.Empty, "208 布防数据留待后续锚点年。");
            Assert.That(GeneralDossiers.GeneralsAt(City("city-jiaozhou"), 190), Is.Empty, "该城 190 无具名部将布防。");
        }
    }
}
