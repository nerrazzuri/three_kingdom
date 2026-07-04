using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 人心杠杆战役闭环（GDD_024）：AttemptSubversion 累积待生效效果 → LaunchOffensive 消费削弱守备 →
    /// 同城同兵，施计后从败转胜（施计降门槛）；待生效效果存读档往返。
    /// </summary>
    [TestFixture]
    public class SubversionCampaignTests
    {
        private readonly CampaignSessionService _service = new CampaignSessionService();
        private static readonly FactionId Lord = new FactionId("faction-lord");
        private static readonly FixedPoint Zero = FixedPoint.Zero;
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private CampaignSession NewAuthorized()
        {
            CampaignSession s = _service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });
            return s;
        }

        // 必成配置（Base=1 → s 饱和；band=0）。
        private static SubversionConfig Sure() => new SubversionConfig(
            @base: FixedPoint.One, weightIntel: F(3, 10), weightWeakness: F(4, 10), weightResist: F(25, 100),
            decayPerAttempt: F(12, 100), backfireBand: Zero, unscoutedPenalty: F(3, 10),
            discordDisciplineHit: F(3, 10), defectRatio: F(35, 100), rumorMoraleHit: F(25, 100), backfireMoraleGain: F(15, 100),
            defectLoyaltyMax: F(4, 10), defectResentmentMin: F(5, 10), discordDefectThreshold: F(7, 10), discordDefectRatio: F(15, 100));

        // 弱守将（低忠诚+高怨恨，已侦察）→ 策反门成型、必成。
        private static SubversionTargetProfile Wavering()
            => new SubversionTargetProfile(new CharacterId("def-gen"),
                loyalty: F(2, 10), resentmentToLord: F(8, 10), greed: F(6, 10),
                charm: F(2, 10), alertness: F(2, 10), scouted: true, intelQuality: F(8, 10));

        // 中等准备：守备 600×1.2=720 战力下退兵（无施计败），施计削弱守备后可破。
        private static OffensivePreparation MediumPrep()
            => new OffensivePreparation(
                400, 120,
                new OffensiveCommand(new OffensiveGeneral(new CharacterId("char-lead"), F(5, 10), F(5, 10), F(5, 10))),
                TroopComposition.AllInfantry(400), ApproachPlan.FrontalAssault,
                new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Fortified, scouted: true);

        private static readonly SiegeDefense Defense = new SiegeDefense(600, FixedPoint.FromFraction(12, 10));

        private OffensiveResult Launch(CampaignSession s)
            => _service.LaunchOffensive(
                s, PlayableCampaign.EnemyCity, MediumPrep(), OffensiveSetupConfig.Default,
                Defense, SiegeResolutionConfig.Default,
                PlayableCampaign.Player, Lord, new Garrison(600), Zero, Zero, Zero, seed: 1UL, OccupationConfig.Default);

        [Test]
        public void test_subversion_lowers_offensive_threshold_in_campaign()
        {
            // 无施计：中等兵力攻坚城退兵。
            Assert.That(Launch(NewAuthorized()).Victory, Is.False, "无施计 → 中等兵力破不了坚城。");

            // 施计：策反（守军倒戈）+ 攻心（守方士气跌）累积 → 同兵破城。
            CampaignSession s = NewAuthorized();
            SubversionOutcome d = _service.AttemptSubversion(s, PlayableCampaign.EnemyCity, SubversionScheme.InciteDefection, Wavering(), FixedPoint.One, 3UL, Sure());
            SubversionOutcome r = _service.AttemptSubversion(s, PlayableCampaign.EnemyCity, SubversionScheme.UnderminedMorale, Wavering(), FixedPoint.One, 5UL, Sure());
            Assert.That(d.Result, Is.EqualTo(SubversionResult.Success), "策反成功。");
            Assert.That(r.Result, Is.EqualTo(SubversionResult.Success), "攻心成功。");
            Assert.That(s.PendingSubversionFor(PlayableCampaign.EnemyCity).IsNone, Is.False, "待生效效果已累积。");

            OffensiveResult won = Launch(s);
            Assert.That(won.Victory, Is.True, "施计削弱守备后，同等兵力破城（攻心降门槛）。");
            Assert.That(s.PendingSubversionFor(PlayableCampaign.EnemyCity).IsNone, Is.True, "出征已消费待生效效果。");
        }

        [Test]
        public void test_subversion_attempt_count_diminishes_and_persists()
        {
            CampaignSession s = NewAuthorized();
            _service.AttemptSubversion(s, PlayableCampaign.EnemyCity, SubversionScheme.SowDiscord, Wavering(), FixedPoint.One, 1UL, SubversionConfig.Default);
            _service.AttemptSubversion(s, PlayableCampaign.EnemyCity, SubversionScheme.SowDiscord, Wavering(), FixedPoint.One, 1UL, SubversionConfig.Default);
            Assert.That(s.SubversionAttemptsOn(PlayableCampaign.EnemyCity), Is.EqualTo(2), "施计次数累计（递减源）。");
        }

        [Test]
        public void test_pending_subversion_round_trips_through_save()
        {
            CampaignSession s = NewAuthorized();
            _service.AttemptSubversion(s, PlayableCampaign.EnemyCity, SubversionScheme.InciteDefection, Wavering(), FixedPoint.One, 3UL, Sure());
            FixedPoint defectBefore = s.PendingSubversionFor(PlayableCampaign.EnemyCity).GarrisonDefectRatio;

            string saved = _service.CaptureSnapshot(s);
            CampaignStartConfig cfg = PlayableCampaign.Default().StartConfig;
            CampaignSession restored = _service.Restore(
                saved, cfg.Fingerprint,
                settlementConfig: cfg.SettlementConfig, governanceConfig: cfg.GovernanceConfig,
                populationPressure: cfg.PopulationPressure,
                intelConfig: cfg.IntelConfig, councilSetup: cfg.CouncilSetup,
                prepConfig: cfg.PreparationConfig,
                reachableRegions: cfg.ReachableRegions, authorizedOrders: cfg.AuthorizedOrders);

            Assert.That(restored.PendingSubversionFor(PlayableCampaign.EnemyCity).GarrisonDefectRatio.Raw,
                Is.EqualTo(defectBefore.Raw), "待生效倒戈比存读档一致。");
            Assert.That(restored.SubversionAttemptsOn(PlayableCampaign.EnemyCity), Is.EqualTo(1), "施计次数存读档一致。");
        }
    }
}
