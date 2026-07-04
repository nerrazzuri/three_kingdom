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
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 失败可继续红线（设计红线：绝不删档/卡死）：出征战败后<b>同一会话仍可行动、重整再战</b>——
    /// 败非终点。（众叛亲离流浪续局见 RebellionTests AC-6；区域战超时退兵见 ZoneBattleBalanceTests。）
    /// </summary>
    [TestFixture]
    public class FailureContinuableTests
    {
        private readonly CampaignSessionService _service = new CampaignSessionService();
        private static readonly FactionId Lord = new FactionId("faction-lord");
        private static readonly FixedPoint Zero = FixedPoint.Zero;
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static OffensiveCommand Cmd()
            => new OffensiveCommand(new OffensiveGeneral(new CharacterId("char-lead"), F(5, 10), F(5, 10), F(5, 10)));

        [Test]
        public void test_defeated_offensive_leaves_session_usable_to_retry_and_win()
        {
            var s = _service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;
            _service.AuthorizeOffensive(s, new[] { PlayableCampaign.EnemyCity });
            var setup = OffensiveSetupConfig.Default;
            var siege = SiegeResolutionConfig.Default;
            var occ = OccupationConfig.Default;
            var defense = new SiegeDefense(500, F(12, 10));
            int meritBefore = s.Career.Career.Merit;

            // 裸战 → 败：不占城，会话不删、授权仍在、生涯未清零。
            var barePrep = new OffensivePreparation(0, 0, Cmd(), TroopComposition.None,
                ApproachPlan.FrontalAssault, new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Fortified, scouted: false);
            OffensiveResult lost = _service.LaunchOffensive(s, PlayableCampaign.EnemyCity, barePrep, setup, defense, siege,
                PlayableCampaign.Player, Lord, new Garrison(600), Zero, Zero, Zero, 1UL, occ);
            Assert.That(lost.Victory, Is.False, "裸战 → 败。");
            Assert.That(lost.Conquest, Is.Null, "败不占城。");
            Assert.That(s.ConquestCount, Is.EqualTo(0));

            // 红线：同会话仍可行动——重整旗鼓、备足再战 → 胜。败非死局。
            Assert.That(_service.CheckOffensiveTarget(s, PlayableCampaign.EnemyCity, PlayableCampaign.Player),
                Is.EqualTo(OffensiveGateResult.Authorized), "败后授权仍在，可再出征。");
            var strongPrep = new OffensivePreparation(
                600, 300, new OffensiveCommand(new OffensiveGeneral(new CharacterId("char-lead"), F(7, 10), F(7, 10), F(8, 10)), null, true),
                new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 400, [TroopType.Infantry] = 200 }),
                ApproachPlan.FeintLure, new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Pass, scouted: true);
            OffensiveResult won = _service.LaunchOffensive(s, PlayableCampaign.EnemyCity, strongPrep, setup, defense, siege,
                PlayableCampaign.Player, Lord, new Garrison(600), Zero, Zero, Zero, 1UL, occ);
            Assert.That(won.Victory, Is.True, "重整再战 → 破城（败可继续，非终点）。");
            Assert.That(s.Career.Career.Merit, Is.GreaterThanOrEqualTo(meritBefore), "生涯未被战败清零。");
        }
    }
}
