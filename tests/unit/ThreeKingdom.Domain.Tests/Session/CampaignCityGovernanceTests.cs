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
    /// epic-016 story-001：城市治理态接入会话 + Advance 日界结算（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配只编排，复用 CityDaySettlementService）+ ADR-0004（确定性）。
    /// TR-city-001（守恒）/ TR-city-002（日界稳定顺序）/ TR-city-005（部分：城市态入哈希）。
    /// 城市日结按「日界」（day rollover）触发，符合 GDD_004「日界结算」。
    /// </summary>
    [TestFixture]
    public class CampaignCityGovernanceTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly int OneDay = WorldTime.SegmentsPerDay;

        // 日结配置：产入20、民用消耗30、维护10、下限0、民心上限100、短缺系数0.5、骚乱阈值50、修复速率15。
        private static CitySettlementConfig SettlementConfig()
            => new CitySettlementConfig(
                baseYield: 20, baseCivConsume: 30, baseMaintenance: 10, stockFloor: 0,
                civMoraleMax: 100, shortageMoralePenalty: Frac(1, 2), unrestShortageThreshold: 50, fortRepairRate: 15);

        private static CityEconomyState CityState(long stock = 100, long reserved = 0, int morale = 60, int fortCur = 20)
            => new CityEconomyState(Fanshui, stock, reserved, morale, security: 50, fortificationCurrent: fortCur, fortificationMax: 100);

        private static CampaignStartConfig Config(CityEconomyState? city = null, long logistics = 0)
            => new CampaignStartConfig(
                "scenario-fanshui-governance", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                cityEconomy: city ?? CityState(),
                settlementConfig: SettlementConfig(),
                populationPressure: FixedPoint.FromInt(1),
                initialLogisticsHolding: logistics,
                governanceConfig: new CityGovernanceConfig(Frac(1, 2), 10, 10));

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession(CityEconomyState? city = null, long logistics = 0)
            => Service.StartCampaign(Config(city, logistics)).Session!;

        // ---- AC-1: CampaignSession 持有城市治理态 ----

        [Test]
        public void test_session_holds_city_economy_from_config()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));

            Assert.That(s.HasCityGovernance, Is.True);
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(100));
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(60));
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(20));
        }

        [Test]
        public void test_session_without_city_config_has_no_governance()
        {
            // 旧式 config（不传城市态）→ 不启用城市治理（向后兼容）。
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            Assert.That(s.HasCityGovernance, Is.False);
            Assert.That(s.CityEconomy, Is.Null);
        }

        // ---- AC-2: Advance 跨日界触发城市日结（稳定顺序）----

        [Test]
        public void test_advance_one_day_settles_city_in_canonical_order()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));

            Service.Advance(s, OneDay);   // 跨一个日界

            // stock: 100 +20(产入) −30(消耗) = 90；fort: 20 +min(80,15)=15 → 35；无短缺→民心不变。
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(90));
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(35));
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(60));
        }

        [Test]
        public void test_advance_within_same_day_does_not_settle_city()
        {
            CampaignSession s = NewSession(CityState(stock: 100));
            // 推进不足一日（1 段，未跨日界）→ 城市态不变。
            Service.Advance(s, 1);

            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(100), "未跨日界不结算城市");
        }

        [Test]
        public void test_shortage_day_reduces_morale_and_floors_stock()
        {
            // 低库存 + 高消耗触发短缺：stock=20, yield=20 → stock_1=40; civDemand=30; consumed=30; stock_2=10;
            // 无短缺（40≥30）此例不短缺。改用更低库存触发短缺。
            CampaignSession s = NewSession(CityState(stock: 5, morale: 60, fortCur: 20));

            Service.Advance(s, OneDay);
            // stock_1 = 5+20 = 25; civDemand=30; consumed=min(25,30)=25; shortage=5; stock_2=max(0,0)=0;
            // moraleLoss = round(0.5 × 5) = 3 → morale 60−3=57。
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(0));
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(57));
        }

        // ---- AC-3: 资源守恒（征用移交后勤不双计）----

        [Test]
        public void test_reserved_food_transfers_to_logistics_conserved()
        {
            // reserved=40：日结承诺阶段移交后勤。
            CampaignSession s = NewSession(CityState(stock: 100, reserved: 40), logistics: 0);
            long startStock = s.CityEconomy!.Stock;

            Service.Advance(s, OneDay);

            // 承诺移交40 → 后勤 0→40；stock: 100−40(移交)+20(产入)−30(消耗)=50。
            Assert.That(s.LogisticsHolding, Is.EqualTo(40), "军粮移交后勤单一计入");
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(50));
            // 守恒：产入−消耗−移交 = 库存差。20 − 30 − 40 = −50 = 50 − 100。
            Assert.That(20 - 30 - 40, Is.EqualTo(s.CityEconomy!.Stock - startStock));
        }

        // ---- AC-4: 城市态纳入会话哈希 ----

        [Test]
        public void test_city_economy_enters_session_hash()
        {
            CampaignSession a = NewSession(CityState(stock: 100));
            CampaignSession b = NewSession(CityState(stock: 200));   // 仅库存不同

            Assert.That(a.ComputeHash(), Is.Not.EqualTo(b.ComputeHash()), "城市库存进哈希");
        }

        [Test]
        public void test_identical_city_state_yields_same_hash()
        {
            CampaignSession a = NewSession(CityState(stock: 100));
            CampaignSession b = NewSession(CityState(stock: 100));

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));
        }

        // ---- AC-5: 多日推进确定性 ----

        [Test]
        public void test_multi_day_advance_is_deterministic()
        {
            CampaignSession s1 = NewSession(CityState(stock: 100));
            CampaignSession s2 = NewSession(CityState(stock: 100));

            Service.Advance(s1, OneDay * 3);
            Service.Advance(s2, OneDay * 3);

            Assert.That(s1.ComputeHash(), Is.EqualTo(s2.ComputeHash()), "同开局多日推进 → 同哈希");
        }

        [Test]
        public void test_multi_day_advance_settles_each_day()
        {
            CampaignSession s = NewSession(CityState(stock: 100, fortCur: 20));

            Service.Advance(s, OneDay * 2);   // 跨两个日界 → 结算两次

            // 第1日：stock 100→90, fort 20→35；第2日：stock 90→80, fort 35→50。
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(80));
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(50));
        }
    }
}
