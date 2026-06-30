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
    /// epic-016 story-002：治理命令入口（征用/修工事/安抚）+ 非法命令稳定错误码（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（命令路径，前置校验失败零写入）+ ADR-0003（代价数据驱动）。TR-city-003。
    /// 覆盖：三命令经会话路径生效；非法命令返回稳定错误码、哈希不变；失败不卡死会话。
    /// </summary>
    [TestFixture]
    public class CampaignGovernanceCommandTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly int OneDay = WorldTime.SegmentsPerDay;

        private static CitySettlementConfig SettlementConfig()
            => new CitySettlementConfig(
                baseYield: 20, baseCivConsume: 30, baseMaintenance: 10, stockFloor: 0,
                civMoraleMax: 100, shortageMoralePenalty: Frac(1, 2), unrestShortageThreshold: 50, fortRepairRate: 15);

        // 治理命令配置：征用每单位民心代价 0.5、安抚 +10、修工事每令 +10。
        private static CityGovernanceConfig GovernanceConfig()
            => new CityGovernanceConfig(requisitionMoralePenalty: Frac(1, 2), appeaseMoraleGain: 10, fortRepairPerOrder: 10);

        private static CityEconomyState CityState(long stock = 100, long reserved = 0, int morale = 60, int fortCur = 20, int fortMax = 100)
            => new CityEconomyState(Fanshui, stock, reserved, morale, security: 50, fortificationCurrent: fortCur, fortificationMax: fortMax);

        private static CampaignStartConfig Config(CityEconomyState city, long logistics = 0)
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
                cityEconomy: city,
                settlementConfig: SettlementConfig(),
                populationPressure: FixedPoint.FromInt(1),
                initialLogisticsHolding: logistics,
                governanceConfig: GovernanceConfig());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession(CityEconomyState? city = null, long logistics = 0)
            => Service.StartCampaign(Config(city ?? CityState(), logistics)).Session!;

        // ---- AC-1: 征用军粮命令 ----

        [Test]
        public void test_requisition_sets_reserved_and_reduces_morale()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60));

            CampaignCommandResult r = Service.RequisitionFood(s, 40);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.CityEconomy!.Reserved, Is.EqualTo(40));
            // 民心代价 = round(0.5 × 40) = 20 → 60−20=40。
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(40));
        }

        [Test]
        public void test_requisition_then_advance_transfers_to_logistics()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60), logistics: 0);
            Service.RequisitionFood(s, 40);

            Service.Advance(s, OneDay);   // 日界承诺阶段移交后勤

            Assert.That(s.LogisticsHolding, Is.EqualTo(40), "征用军粮经日结移交后勤");
            // stock: 100 −40(移交) +20(产入) −30(消耗) = 50。
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(50));
        }

        // ---- AC-2: 修工事命令 ----

        [Test]
        public void test_repair_fortification_increases_fort()
        {
            CampaignSession s = NewSession(CityState(fortCur: 20, fortMax: 100));

            CampaignCommandResult r = Service.RepairFortification(s);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(30), "+min(80,10)=10");
        }

        [Test]
        public void test_repair_fortification_clamps_to_max()
        {
            CampaignSession s = NewSession(CityState(fortCur: 95, fortMax: 100));

            Service.RepairFortification(s);

            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(100), "+min(5,10)=5 夹至上限");
        }

        // ---- AC-3: 安抚命令 ----

        [Test]
        public void test_appease_increases_morale()
        {
            CampaignSession s = NewSession(CityState(morale: 60));

            CampaignCommandResult r = Service.Appease(s);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(70), "+10");
        }

        [Test]
        public void test_appease_clamps_to_morale_max()
        {
            CampaignSession s = NewSession(CityState(morale: 95));

            Service.Appease(s);

            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(100), "+10 夹至上限 100");
        }

        // ---- AC-4: 非法命令稳定错误码 + 无部分写入 ----

        [Test]
        public void test_requisition_over_available_is_rejected_with_no_write()
        {
            CampaignSession s = NewSession(CityState(stock: 100, reserved: 0, morale: 60));
            StateHash before = s.ComputeHash();

            CampaignCommandResult r = Service.RequisitionFood(s, 200);   // > available 100

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.InsufficientStock));
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "拒绝命令零写入，哈希不变");
        }

        [Test]
        public void test_requisition_negative_amount_is_rejected()
        {
            CampaignSession s = NewSession(CityState(stock: 100));
            StateHash before = s.ComputeHash();

            CampaignCommandResult r = Service.RequisitionFood(s, -5);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.InvalidAmount));
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        [Test]
        public void test_repair_full_fortification_is_rejected()
        {
            CampaignSession s = NewSession(CityState(fortCur: 100, fortMax: 100));
            StateHash before = s.ComputeHash();

            CampaignCommandResult r = Service.RepairFortification(s);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.FortificationFull));
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        [Test]
        public void test_governance_command_on_disabled_session_is_rejected()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            CampaignCommandResult r = Service.RequisitionFood(s, 10);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.CityGovernanceDisabled));
        }

        // ---- AC-5: 非法命令后会话可继续 ----

        [Test]
        public void test_session_continues_after_rejected_command()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60));

            Service.RequisitionFood(s, 999);                 // 被拒
            CampaignCommandResult ok = Service.RequisitionFood(s, 30);   // 合法
            Service.Advance(s, OneDay);

            Assert.That(ok.Applied, Is.True, "失败命令后合法命令仍可执行");
            Assert.That(s.LogisticsHolding, Is.EqualTo(30));
        }
    }
}
