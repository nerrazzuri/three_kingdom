using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>守备接城池（GDD_027 #6）：DefenseOf 取世界大盘该城真实守军 + 工事随城规模分级，不再恒 600/1.2。</summary>
    [TestFixture]
    public class CityDefenseTests
    {
        private static CityId City(string id) => new CityId(id);

        [Test]
        public void test_defense_reflects_real_city_garrison()
        {
            // Arrange：默认盘（公元190）。
            var camp = PlayableCampaign.Default();
            // Act
            SiegeDefense ye = camp.DefenseOf(City("city-ye"));         // 袁绍治所·邺 900
            SiegeDefense hulao = camp.DefenseOf(City("city-hulao"));    // 袁术·虎牢 600
            SiegeDefense xiaopei = camp.DefenseOf(City("city-xiaopei")); // 刘备·小沛 400
            // Assert：守军取真值，各城不同（不再恒 600）。
            Assert.That(ye.Garrison, Is.EqualTo(900), "邺为治所坚城。");
            Assert.That(hulao.Garrison, Is.EqualTo(600), "虎牢关中城。");
            Assert.That(xiaopei.Garrison, Is.EqualTo(400), "小沛边邑。");
        }

        [Test]
        public void test_fortification_scales_with_city_scale()
        {
            var camp = PlayableCampaign.Default();
            // Act
            SiegeDefense ye = camp.DefenseOf(City("city-ye"));
            SiegeDefense hulao = camp.DefenseOf(City("city-hulao"));
            SiegeDefense xiaopei = camp.DefenseOf(City("city-xiaopei"));
            // Assert：治所工事 > 中城 > 边邑。
            Assert.That(ye.FortFactor > hulao.FortFactor, Is.True, "治所工事强于中城。");
            Assert.That(hulao.FortFactor > xiaopei.FortFactor, Is.True, "中城工事强于边邑。");
        }

        [Test]
        public void test_unknown_city_falls_back_to_default()
        {
            var camp = PlayableCampaign.Default();
            // Act：非大盘城（新占/中立）。
            SiegeDefense unknown = camp.DefenseOf(City("city-does-not-exist"));
            // Assert：回退默认 600（不崩、不 0）。
            Assert.That(unknown.Garrison, Is.EqualTo(600), "未知城回退默认守军。");
        }
    }
}
