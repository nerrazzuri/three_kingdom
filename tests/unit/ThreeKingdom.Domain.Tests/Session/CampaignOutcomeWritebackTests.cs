using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-020 共享夹具：启用城市治理的会话（M07 后果写回目标为城市态）。
    /// </summary>
    internal static class OutcomeFixture
    {
        internal static readonly FactionId Player = new FactionId("faction-player");
        internal static readonly FactionId Enemy = new FactionId("faction-yuan");
        internal static readonly CharacterId Lord = new CharacterId("char-player-lord");
        internal static readonly CharacterId Aide = new CharacterId("char-aide");
        internal static readonly CityId Fanshui = new CityId("city-fanshui");
        internal static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        internal static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        internal static CitySettlementConfig SettlementConfig()
            => new CitySettlementConfig(20, 30, 10, 0, 100, Frac(1, 2), 50, 15);
        internal static CityGovernanceConfig GovernanceConfig()
            => new CityGovernanceConfig(Frac(1, 2), 10, 10);
        internal static CityEconomyState CityState(int morale = 60, int security = 50, int fortCur = 40)
            => new CityEconomyState(Fanshui, 100, 0, morale, security, fortCur, 100);

        // 后果损耗配置：败北名声-10/撤退-3/失城-20；民心-15/治安-10/工事-12/减员-100。
        internal static OutcomeConsequenceConfig OutcomeCfg()
            => new OutcomeConsequenceConfig(
                reputationLossDefeat: 10, reputationLossRetreat: 3, reputationLossCityLost: 20,
                civMoraleLoss: 15, securityLoss: 10, fortificationDamage: 12, forceAttrition: 100);

        internal static OutcomeContext Ctx()
            => new OutcomeContext(Player, city: Fanshui, commander: Aide);

        internal static CampaignStartConfig Config(CityEconomyState? city = null)
            => new CampaignStartConfig(
                "scenario-fanshui-outcome", Fp,
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
                governanceConfig: GovernanceConfig());

        internal static readonly CampaignSessionService Service = new CampaignSessionService();
        internal static CampaignSession NewSession(CityEconomyState? city = null)
            => Service.StartCampaign(Config(city)).Session!;
    }

    /// <summary>
    /// epic-020 story-001：战果分支后果写回会话（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配，复用 FailureContinuationService）+ ADR-0004（确定性）。TR-outcome-001。
    /// 覆盖：四分支变更集写回城市态；损耗夹至下限；确定性。
    /// </summary>
    [TestFixture]
    public class CampaignOutcomeWritebackTests
    {
        private static CampaignSessionService Service => OutcomeFixture.Service;

        // ---- AC-1: 败北分支写回城市损耗 ----

        [Test]
        public void test_defeat_writes_city_losses()
        {
            CampaignSession s = OutcomeFixture.NewSession(OutcomeFixture.CityState(morale: 60, security: 50, fortCur: 40));

            OutcomeContinuation cont = Service.ResolveBattleOutcome(s, OutcomeBranch.Defeat, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());

            Assert.That(cont.Writeback.Committed, Is.True);
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(45), "民心 60−15");
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(28), "工事 40−12");
        }

        [Test]
        public void test_loss_capped_at_floor_no_negative()
        {
            // 低民心 5，损耗 15 → 夹至 0（不出负）。
            CampaignSession s = OutcomeFixture.NewSession(OutcomeFixture.CityState(morale: 5, security: 5, fortCur: 5));

            Service.ResolveBattleOutcome(s, OutcomeBranch.Defeat, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());

            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(0), "夹至下限不出负");
            Assert.That(s.CityEconomy!.Security, Is.GreaterThanOrEqualTo(0));
        }

        // ---- AC-2: 失城分支写回 ----

        [Test]
        public void test_city_lost_branch_writes_back()
        {
            CampaignSession s = OutcomeFixture.NewSession();

            OutcomeContinuation cont = Service.ResolveBattleOutcome(s, OutcomeBranch.CityLost, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());

            Assert.That(cont.Branch, Is.EqualTo(OutcomeBranch.CityLost));
            Assert.That(cont.Writeback.Committed, Is.True);
        }

        // ---- AC-3: 胜利分支写回（损耗小/无）----

        [Test]
        public void test_victory_branch_minimal_loss()
        {
            CampaignSession s = OutcomeFixture.NewSession(OutcomeFixture.CityState(morale: 60));

            Service.ResolveBattleOutcome(s, OutcomeBranch.Victory, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());

            // 胜利分支城市不受损（BuildConsequences: branch != Victory 才写城市损耗）。
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(60), "胜利不损民心");
        }

        // ---- AC-4: 写回确定性 ----

        [Test]
        public void test_writeback_is_deterministic()
        {
            CampaignSession a = OutcomeFixture.NewSession();
            CampaignSession b = OutcomeFixture.NewSession();

            Service.ResolveBattleOutcome(a, OutcomeBranch.Defeat, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());
            Service.ResolveBattleOutcome(b, OutcomeBranch.Defeat, OutcomeFixture.Ctx(), OutcomeFixture.OutcomeCfg());

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同分支同上下文 → 同哈希");
        }
    }
}
