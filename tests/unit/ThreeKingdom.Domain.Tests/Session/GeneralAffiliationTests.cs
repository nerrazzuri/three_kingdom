using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 武将归属层 P1 地基（GDD_027 / ADR-0016）：归属派生（在职/在野/不存）、角色派生、城武将册（≤20 裁剪）。
    /// 纯 C# 无场景依赖。
    /// </summary>
    [TestFixture]
    public class GeneralAffiliationTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);

        [Test]
        public void test_affiliation_in_service_wandering_or_absent()
        {
            // AC1 在职：关羽 190 事奉刘备、驻小沛、先锋役。
            Affiliation guan190 = GeneralAffiliations.AffiliationOf(C("char-guanyu"), 190);
            Assert.That(guan190.Status, Is.EqualTo(AffiliationStatus.InService));
            Assert.That(guan190.Faction, Is.EqualTo(PlayableCampaign.LiuBei));
            Assert.That(guan190.City, Is.EqualTo(PlayableCampaign.Xiaopei));
            Assert.That(guan190.Role, Is.EqualTo(GeneralRole.Vanguard));

            // 同将异纪元换城：200 关羽随刘备寄汝南。
            Affiliation guan200 = GeneralAffiliations.AffiliationOf(C("char-guanyu"), 200);
            Assert.That(guan200.Status, Is.EqualTo(AffiliationStatus.InService));
            Assert.That(guan200.City, Is.EqualTo(PlayableCampaign.Runan));

            // 不存：姜维 202 生 → 190 未及冠。
            Assert.That(GeneralAffiliations.AffiliationOf(C("char-jiangwei"), 190).Status, Is.EqualTo(AffiliationStatus.Absent));

            // 在野（无本属势力登记）：司马徽名士。
            Assert.That(GeneralAffiliations.AffiliationOf(C("char-simahui"), 190).Status, Is.EqualTo(AffiliationStatus.Wandering));

            // 在野（本属势力此纪元已灭）：臧霸本属吕布，200 吕布已亡不存 → 转在野（可被别家招揽）。
            Affiliation zangba200 = GeneralAffiliations.AffiliationOf(C("char-zangba"), 200);
            Assert.That(zangba200.Status, Is.EqualTo(AffiliationStatus.Wandering), "势力覆灭 → 旧部转在野。");
        }

        [Test]
        public void test_role_derived_from_traits()
        {
            Assert.That(GeneralAffiliations.RoleOf(C("char-zhugeliang")), Is.EqualTo(GeneralRole.Strategist), "经天纬地 → 谋士。");
            Assert.That(GeneralAffiliations.RoleOf(C("char-zhouyu")), Is.EqualTo(GeneralRole.Naval), "水战 → 水军。");
            Assert.That(GeneralAffiliations.RoleOf(C("char-guanyu")), Is.EqualTo(GeneralRole.Vanguard), "绝世武 → 先锋。");
        }

        [Test]
        public void test_city_roster_capped_at_20_and_deterministic()
        {
            // 邺城 190 为袁绍治所，袁绍诸将多归此 → 册裁剪到 ≤20，且确定性可复现。
            var ye = GeneralAffiliations.RosterOf(City("city-ye"), 190);
            Assert.That(ye.Count, Is.LessThanOrEqualTo(GeneralAffiliations.RosterCap), "城册不超 20。");
            Assert.That(ye.Count, Is.GreaterThan(0));
            Assert.That(ye, Does.Contain(C("char-yanliang")), "颜良在邺城册。");

            // 确定性：再取一次结果一致（同序同集）。
            var ye2 = GeneralAffiliations.RosterOf(City("city-ye"), 190);
            Assert.That(ye2, Is.EqualTo(ye));

            // 册按档降序：首位档 ≥ 末位档。
            if (ye.Count >= 2)
            {
                int firstRank = Rank(ye[0]), lastRank = Rank(ye[ye.Count - 1]);
                Assert.That(firstRank, Is.GreaterThanOrEqualTo(lastRank), "档高者在前。");
            }
        }

        [Test]
        public void test_faction_presence_and_capital_by_era()
        {
            Assert.That(PlayableCampaign.FactionExistsAt(PlayableCampaign.LiuBei, 234), Is.True);
            Assert.That(PlayableCampaign.FactionCapitalAt(PlayableCampaign.LiuBei, 234), Is.EqualTo(PlayableCampaign.Chengdu), "234 季汉治所成都。");
            Assert.That(PlayableCampaign.FactionExistsAt(PlayableCampaign.LuBu, 200), Is.False, "200 吕布已灭。");
            Assert.That(PlayableCampaign.FactionCapitalAt(PlayableCampaign.LuBu, 200), Is.Null);
        }

        private static int Rank(CharacterId g)
        {
            GeneralDossier? d = GeneralDossiers.Find(g);
            return d == null ? 0 : System.Math.Max((int)d.Prowess, (int)d.Strategy);
        }
    }
}
