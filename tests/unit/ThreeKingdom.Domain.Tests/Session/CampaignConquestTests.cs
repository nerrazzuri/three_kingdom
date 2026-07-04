using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 出征攻城 Application 编排（GDD_019 / ADR-0010）：授权门 + 占城归属 C 结算（控制权变更 + 自立倾向 + 记功）+ 存档。
    /// </summary>
    [TestFixture]
    public class CampaignConquestTests
    {
        private readonly CampaignSessionService _service = new CampaignSessionService();
        private static readonly FactionId Lord = new FactionId("faction-lord");
        private static readonly FixedPoint Zero = FixedPoint.Zero;

        private CampaignSession NewSession() => _service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;

        // p=0 配置（前2座仍归玩家，第3座起恒 LordKeeps）——便于验证归君主分支。
        private static OccupationConfig LordKeepsCfg() => new OccupationConfig(2, Zero, Zero, Zero, Zero, leanPerSeizure: 10);

        [Test]
        public void test_authorize_and_gate()
        {
            var s = NewSession();
            // 授权含敌城 + （为触达 OwnCity 分支）己城。
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity, PlayableCampaign.Fanshui });

            Assert.That(_service.CheckOffensiveTarget(s, PlayableCampaign.EnemyCity, PlayableCampaign.Player),
                Is.EqualTo(OffensiveGateResult.Authorized), "授权 + 敌控城 → 通过。");
            Assert.That(_service.CheckOffensiveTarget(s, PlayableCampaign.Fanshui, PlayableCampaign.Player),
                Is.EqualTo(OffensiveGateResult.OwnCity), "己方城 → 拒。");

            var s2 = NewSession();   // 未授权
            Assert.That(_service.CheckOffensiveTarget(s2, PlayableCampaign.EnemyCity, PlayableCampaign.Player),
                Is.EqualTo(OffensiveGateResult.NotAuthorized), "未授权 → 拒。");
        }

        [Test]
        public void test_first_conquest_grants_to_player_and_changes_control()
        {
            var s = NewSession();
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });

            ConquestResult r = _service.ResolveConquest(
                s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord,
                Zero, Zero, Zero, seed: 1UL, config: OccupationConfig.Default);

            Assert.That(r.Verdict, Is.EqualTo(OwnershipVerdict.GrantToPlayer), "首座归玩家。");
            Assert.That(r.ConquestCount, Is.EqualTo(1));
        }

        [Test]
        public void test_third_conquest_lord_keeps_and_accumulates_rebellion_lean()
        {
            var s = NewSession();
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });
            var cfg = LordKeepsCfg();

            // 前两座归玩家，第三座（index 2, p=0）归君主 → 自立倾向 +10。
            _service.ResolveConquest(s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord, Zero, Zero, Zero, 1UL, cfg);
            _service.ResolveConquest(s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord, Zero, Zero, Zero, 1UL, cfg);
            ConquestResult third = _service.ResolveConquest(s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord, Zero, Zero, Zero, 1UL, cfg);

            Assert.That(third.Verdict, Is.EqualTo(OwnershipVerdict.LordKeeps), "第三座归君主。");
            Assert.That(third.RebellionLean, Is.EqualTo(10), "战果被夺 → 自立倾向累积。");
        }

        [Test]
        public void test_conquest_applies_career_gain()
        {
            var s = NewSession();
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });
            int meritBefore = s.Career.Career.Merit;

            ConquestResult r = _service.ResolveConquest(
                s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord,
                Zero, Zero, Zero, 1UL, OccupationConfig.Default,
                ladder: PlayableCampaign.Default().Ladder,
                gainSource: ThreeKingdom.Domain.Career.CareerGainSource.CombatVictory);

            Assert.That(r.CareerApplied, Is.True, "出征胜记功。");
            Assert.That(s.Career.Career.Merit, Is.GreaterThan(meritBefore), "功绩增长。");
        }

        [Test]
        public void test_conquest_state_round_trips_through_save()
        {
            var s = NewSession();
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });
            _service.ResolveConquest(s, PlayableCampaign.EnemyCity, new Garrison(600), PlayableCampaign.Player, Lord, Zero, Zero, Zero, 1UL, OccupationConfig.Default);

            string saved = _service.CaptureSnapshot(s);
            CampaignStartConfig cfg = PlayableCampaign.Default().StartConfig;
            CampaignSession restored = _service.Restore(
                saved, cfg.Fingerprint,
                settlementConfig: cfg.SettlementConfig, governanceConfig: cfg.GovernanceConfig,
                populationPressure: cfg.PopulationPressure,
                intelConfig: cfg.IntelConfig, councilSetup: cfg.CouncilSetup,
                prepConfig: cfg.PreparationConfig,
                reachableRegions: cfg.ReachableRegions, authorizedOrders: cfg.AuthorizedOrders);

            Assert.That(restored.ConquestCount, Is.EqualTo(1), "占城计数存读档一致。");
            Assert.That(restored.OffensiveAuthorization.Authorizes(PlayableCampaign.EnemyCity), Is.True, "授权目标存读档保留。");
        }
    }
}
