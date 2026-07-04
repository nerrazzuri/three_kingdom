using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.Subversion
{
    /// <summary>
    /// 人心杠杆 × 战斗接缝端到端（GDD_024 §15 W5）：施计<b>降低</b>破城门槛（同兵同守备，施计后可胜），
    /// 但<b>不单独决定胜负</b>——裸兵靠施计仍破不了坚城（施计撬动而非替代六维准备）。
    /// </summary>
    [TestFixture]
    public class SubversionBattleIntegrationTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private const int Garrison = 800;

        // 强施计：守方开战士气 −0.3、军纪 −0.2、守军倒戈 40%。
        private static readonly SubversionEffect Strong =
            new SubversionEffect(F(-3, 10), F(4, 10), F(-2, 10));

        private static ZoneBattleOutcome Resolve(int troops, SubversionEffect subversion)
        {
            var lead = new OffensiveGeneral(new CharacterId("lead"), F(7, 10), F(7, 10), F(8, 10));
            var prep = new OffensivePreparation(troops, 300, new OffensiveCommand(lead),
                TroopComposition.AllInfantry(troops), ApproachPlan.FrontalAssault,
                new OffensiveTiming(DaySegment.Day, WeatherType.Clear), TerrainKind.Fortified, scouted: true);
            FixedPoint morale = new OffensiveSetupService().Derive(prep, OffensiveSetupConfig.Default).Morale;
            return ZoneBattleRuntime.FromOffensive(prep, morale, Garrison, seed: 4321UL, subversion: subversion)
                .AutoResolve().Outcome;
        }

        // ---- W5①：施计降门槛——同兵同守备，无施计败、有施计胜 ----
        [Test]
        public void test_subversion_lowers_the_threshold_to_break_the_city()
        {
            const int medium = 500;
            Assert.That(Resolve(medium, SubversionEffect.None), Is.EqualTo(ZoneBattleOutcome.DefenderVictory),
                "同守备下，无施计的中等兵力攻坚城退兵。");
            Assert.That(Resolve(medium, Strong), Is.EqualTo(ZoneBattleOutcome.AttackerVictory),
                "施计（守军倒戈+士气军纪崩）后，同等兵力破城——攻心降门槛。");
        }

        // ---- W5②：施计不单独决定胜负——裸兵靠施计仍破不了坚城 ----
        [Test]
        public void test_subversion_alone_does_not_decide_bare_attacker_still_loses()
        {
            const int bare = 100;
            Assert.That(Resolve(bare, Strong), Is.EqualTo(ZoneBattleOutcome.DefenderVictory),
                "裸兵（100）纵有强施计仍破不了坚城——施计撬动而非替代六维准备（W5）。");
        }
    }
}
