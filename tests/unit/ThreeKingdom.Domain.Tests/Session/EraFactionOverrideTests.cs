using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>纪元覆盖（GDD_027 R1 史准细化）：跳槽者在加入终属势力前归其早纪元实际势力，不再混入终属势力早期册。</summary>
    [TestFixture]
    public class EraFactionOverrideTests
    {
        private static CharacterId C(string id) => new CharacterId(id);

        [Test]
        public void test_huangquan_serves_liuzhang_early_not_liubei()
        {
            // 190/208：黄权属刘璋（益州本土），非刘备。
            Affiliation a190 = GeneralAffiliations.AffiliationOf(C("char-huangquan"), 190);
            Assert.That(a190.Status, Is.EqualTo(AffiliationStatus.InService));
            Assert.That(a190.Faction.Equals(PlayableCampaign.LiuZhang), Is.True, "黄权190事奉刘璋（非刘备）。");
            Assert.That(a190.Faction.Equals(PlayableCampaign.LiuBei), Is.False);
        }

        [Test]
        public void test_huangquan_returns_to_liubei_after_join()
        {
            // 219：无覆盖 → 回落 baseFaction 刘备（史：214 归刘备）。
            Affiliation a219 = GeneralAffiliations.AffiliationOf(C("char-huangquan"), 219);
            Assert.That(a219.Faction.Equals(PlayableCampaign.LiuBei), Is.True, "黄权219已归刘备（baseFaction 回落）。");
        }

        [Test]
        public void test_xiaopei_190_roster_excludes_era_shifters()
        {
            // 小沛（刘备190治所）城册不应再含黄权/董和（此年属刘璋）。
            var roster = GeneralAffiliations.RosterOf(new CityId("city-xiaopei"), 190);
            Assert.That(roster, Does.Not.Contain(C("char-huangquan")), "190小沛册不含黄权。");
            Assert.That(roster, Does.Not.Contain(C("char-donghe")), "190小沛册不含董和。");
            // 关羽仍在（真属刘备）。
            Assert.That(roster, Does.Contain(C("char-guanyu")), "关羽仍在小沛册。");
        }

        [Test]
        public void test_yangyi_wandering_early()
        {
            // 杨仪早纪元在野（加入刘备前辗转）。
            Affiliation a = GeneralAffiliations.AffiliationOf(C("char-yangyi"), 200);
            Assert.That(a.Status, Is.EqualTo(AffiliationStatus.Wandering), "杨仪200在野（覆盖 null）。");
        }
    }
}
