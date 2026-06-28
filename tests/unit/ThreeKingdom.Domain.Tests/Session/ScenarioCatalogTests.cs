using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-014 story-001：ScenarioCatalog 多场景注册 + 校验 + 按 id 开局（Integration / Assembly）。
    /// 治理 ADR：ADR-0003（数据驱动配置）+ ADR-0009（CampaignSession 装配）。TR-session-003。
    /// 覆盖 多场景按 id 开局、未知 id 稳定错误码、重复 id/空目录加载期拒。
    /// </summary>
    [TestFixture]
    public class ScenarioCatalogTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Sun = new FactionId("faction-sun");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Xuchang = new CityId("city-xuchang");
        private static readonly CityId Jianye = new CityId("city-jianye");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig Scenario(string id, FactionId faction, CharacterId lord, CityId city)
            => new CampaignStartConfig(
                id, new ConfigFingerprint(0xC0DEUL),
                new CitySeed(faction, city, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(faction, lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { city }) },
                new[] { new CityOwnership(city, faction, 800) });

        private static ScenarioCatalog Catalog()
            => new ScenarioCatalog(new[]
            {
                Scenario("scenario-xuchang", Cao, CaoCao, Xuchang),
                Scenario("scenario-jianye", Sun, new CharacterId("char-sunquan"), Jianye),
            });

        private static readonly CampaignSessionService Service = new CampaignSessionService();

        [Test]
        public void test_start_by_id_picks_correct_scenario()
        {
            ScenarioCatalog cat = Catalog();
            CampaignSession a = Service.StartCampaign(cat, "scenario-xuchang").Session!;
            CampaignSession b = Service.StartCampaign(cat, "scenario-jianye").Session!;

            Assert.That(a.Career.Career.Faction, Is.EqualTo(Cao));
            Assert.That(a.World.OwnershipOf(Xuchang)!.Owner, Is.EqualTo(Cao));
            Assert.That(b.Career.Career.Faction, Is.EqualTo(Sun));
            Assert.That(b.World.OwnershipOf(Jianye)!.Owner, Is.EqualTo(Sun));
        }

        [Test]
        public void test_unknown_scenario_id_returns_stable_code()
        {
            CampaignStartResult r = Service.StartCampaign(Catalog(), "scenario-nonexistent");
            Assert.That(r.Started, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.SessionNotFound));
            Assert.That(r.Session, Is.Null);
        }

        [Test]
        public void test_catalog_lists_registered_ids()
        {
            Assert.That(Catalog().Ids, Is.EquivalentTo(new[] { "scenario-xuchang", "scenario-jianye" }));
        }

        [Test]
        public void test_duplicate_scenario_id_rejected_at_load()
        {
            Assert.Throws<ArgumentException>(() => new ScenarioCatalog(new[]
            {
                Scenario("scenario-dup", Cao, CaoCao, Xuchang),
                Scenario("scenario-dup", Sun, new CharacterId("char-sunquan"), Jianye),
            }));
        }

        [Test]
        public void test_empty_catalog_rejected()
        {
            Assert.Throws<ArgumentException>(() => new ScenarioCatalog(Array.Empty<CampaignStartConfig>()));
        }
    }
}
