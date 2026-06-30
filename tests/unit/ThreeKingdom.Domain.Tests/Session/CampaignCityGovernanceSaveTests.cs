using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-016 story-004：治理态存读档 round-trip + 日界确定性（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档 round-trip）+ ADR-0004（确定性）。TR-city-005。
    /// 覆盖：治理态逐字段一致、哈希一致、read-back 后下一日结一致、日界确定性、存档不中断确定性链。
    /// 城市配置数据驱动（按指纹由载入方提供），存档体只存城市态。
    /// </summary>
    [TestFixture]
    public class CampaignCityGovernanceSaveTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly int OneDay = WorldTime.SegmentsPerDay;
        private static readonly FixedPoint Pressure = FixedPoint.FromInt(1);

        private static CitySettlementConfig SettlementConfig()
            => new CitySettlementConfig(
                baseYield: 20, baseCivConsume: 30, baseMaintenance: 10, stockFloor: 0,
                civMoraleMax: 100, shortageMoralePenalty: Frac(1, 2), unrestShortageThreshold: 50, fortRepairRate: 15);

        private static CityGovernanceConfig GovernanceConfig()
            => new CityGovernanceConfig(requisitionMoralePenalty: Frac(1, 2), appeaseMoraleGain: 10, fortRepairPerOrder: 10);

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
                populationPressure: Pressure,
                initialLogisticsHolding: logistics,
                governanceConfig: GovernanceConfig());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession(CityEconomyState? city = null, long logistics = 0)
            => Service.StartCampaign(Config(city, logistics)).Session!;

        // 恢复时提供城市配置（数据驱动，按指纹）。
        private static CampaignSession Restore(string text)
            => Service.Restore(text, Fp, SettlementConfig(), GovernanceConfig(), Pressure);

        // ---- AC-1: 治理态 round-trip 逐字段一致 ----

        [Test]
        public void test_city_state_roundtrip_field_for_field()
        {
            CampaignSession s = NewSession(CityState(stock: 100, reserved: 0, morale: 60, fortCur: 20));
            Service.RequisitionFood(s, 30);       // 产生非平凡态：reserved=30, morale=45
            Service.Advance(s, OneDay);           // 日结：移交后勤、产耗、修工事

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.CityEconomy!.Stock, Is.EqualTo(s.CityEconomy!.Stock));
            Assert.That(loaded.CityEconomy!.Reserved, Is.EqualTo(s.CityEconomy!.Reserved));
            Assert.That(loaded.CityEconomy!.CivMorale, Is.EqualTo(s.CityEconomy!.CivMorale));
            Assert.That(loaded.CityEconomy!.Security, Is.EqualTo(s.CityEconomy!.Security));
            Assert.That(loaded.CityEconomy!.FortificationCurrent, Is.EqualTo(s.CityEconomy!.FortificationCurrent));
            Assert.That(loaded.CityEconomy!.FortificationMax, Is.EqualTo(s.CityEconomy!.FortificationMax));
            Assert.That(loaded.LogisticsHolding, Is.EqualTo(s.LogisticsHolding));
        }

        // ---- AC-2: round-trip 哈希一致 ----

        [Test]
        public void test_city_governance_roundtrip_preserves_hash()
        {
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));
            Service.RepairFortification(s);
            Service.Advance(s, OneDay);
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-3: read-back 后下一日结一致 ----

        [Test]
        public void test_next_day_settlement_after_restore_matches_direct()
        {
            // 直推：Advance 两个日界。
            CampaignSession direct = NewSession(CityState(stock: 100, fortCur: 20));
            Service.Advance(direct, OneDay * 2);
            StateHash directHash = direct.ComputeHash();

            // 切割：Advance 一日 → 存读档 → 再 Advance 一日。
            CampaignSession s = NewSession(CityState(stock: 100, fortCur: 20));
            Service.Advance(s, OneDay);
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.Advance(loaded, OneDay);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "读档后下一日结与直推一致");
        }

        // ---- AC-4: 日界确定性 ----

        [Test]
        public void test_governance_command_stream_is_deterministic()
        {
            CampaignSession a = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));
            CampaignSession b = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));

            foreach (CampaignSession s in new[] { a, b })
            {
                Service.RequisitionFood(s, 20);
                Service.RepairFortification(s);
                Service.Advance(s, OneDay * 2);
                Service.Appease(s);
            }

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同命令流 → 同哈希");
        }

        // ---- AC-5: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：征用 → Advance(2 日)。
            CampaignSession direct = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));
            Service.RequisitionFood(direct, 30);
            Service.Advance(direct, OneDay * 2);
            StateHash directHash = direct.ComputeHash();

            // 切割：征用 → Advance(1 日) → 存读档 → Advance(1 日)。
            CampaignSession s = NewSession(CityState(stock: 100, morale: 60, fortCur: 20));
            Service.RequisitionFood(s, 30);
            Service.Advance(s, OneDay);
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.Advance(loaded, OneDay);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续确定性");
        }

        // ---- 边界：含城市态存档未提供配置 → 整体拒绝 ----

        [Test]
        public void test_restore_city_save_without_config_is_rejected()
        {
            CampaignSession s = NewSession(CityState(stock: 100));
            string text = Service.CaptureSnapshot(s);

            Assert.Throws<SaveFormatException>(() => Service.Restore(text, Fp), "含城市态但未提供配置应整体拒绝");
        }

        // ---- 向后兼容：无城市治理的会话存读档不受影响 ----

        [Test]
        public void test_non_governance_session_roundtrip_still_works()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);   // 无城市配置参数

            Assert.That(loaded.HasCityGovernance, Is.False);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }
    }
}
