using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>敌方区域AI强化（ADR-0013）：E1 切粮道（乘虚突袭弱守粮道）· E2 依己方守将性格调整打法（莽将悍勇/善守死守）。</summary>
    [TestFixture]
    public class EnemyAiTacticsTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static Detachment Det(BattleSide side, ZoneId at, int strength, GeneralTag? tag)
        {
            OffensiveGeneral? g = tag.HasValue
                ? new OffensiveGeneral(new CharacterId("char-x"), F(6, 10), F(7, 10), F(6, 10), GeneralSpecialty.None, new[] { tag.Value }, CombatTier.Valiant, StrategyTier.Plain)
                : null;
            return new Detachment(new DetachmentId("d"), side, g, TroopComposition.AllInfantry(strength), strength, F(7, 10), F(1, 10), Posture.Hold, at);
        }

        private static EnemyAiConfig Cfg(int supplyRaid = 45, int stubborn = 40)
            => new EnemyAiConfig(null, 30, 20, 40, 25, 15, 2, supplyRaidBonus: supplyRaid, stubbornDefenderBonus: stubborn);

        // ---- E1 切粮道 ----
        [Test]
        public void test_attacker_values_raiding_weakly_held_supply()
        {
            Detachment det = Det(BattleSide.Attacker, BattleField.Reserve, 300, null);
            // 攻方移入弱守粮道（我300 > 敌100）。
            int withRaid = EnemyZoneAiService.Score(BattleField.Supply, true, BattleSide.Attacker, det, 100, 0, 0, Cfg(supplyRaid: 45));
            int noRaid = EnemyZoneAiService.Score(BattleField.Supply, true, BattleSide.Attacker, det, 100, 0, 0, Cfg(supplyRaid: 0));
            Assert.That(withRaid - noRaid, Is.EqualTo(45), "弱守粮道 → 攻方切粮道效用 +SupplyRaidBonus。");
        }

        [Test]
        public void test_supply_raid_not_applied_when_supply_strongly_held()
        {
            Detachment det = Det(BattleSide.Attacker, BattleField.Reserve, 100, null);
            // 敌 400 强守粮道，我 100 移入（effectiveOwn 100 < 400）→ 不加突袭。
            int withRaid = EnemyZoneAiService.Score(BattleField.Supply, true, BattleSide.Attacker, det, 400, 0, 0, Cfg(supplyRaid: 45));
            int noRaid = EnemyZoneAiService.Score(BattleField.Supply, true, BattleSide.Attacker, det, 400, 0, 0, Cfg(supplyRaid: 0));
            Assert.That(withRaid, Is.EqualTo(noRaid), "强守粮道不触发切粮道（不送死）。");
        }

        // ---- E2 守将性格 ----
        [Test]
        public void test_aggressive_general_assaults_at_parity_plain_holds()
        {
            Detachment reckless = Det(BattleSide.Attacker, BattleField.Front, 100, GeneralTag.Reckless);
            Detachment plain = Det(BattleSide.Attacker, BattleField.Front, 100, null);
            // 势均（我100 敌100）。
            Assert.That(EnemyZoneAiService.DecidePosture(BattleSide.Attacker, BattleField.Front, reckless, 100, 100, Cfg()),
                Is.EqualTo(Posture.Assault), "莽勇之将势均即冲。");
            Assert.That(EnemyZoneAiService.DecidePosture(BattleSide.Attacker, BattleField.Front, plain, 100, 100, Cfg()),
                Is.EqualTo(Posture.Hold), "无性格加成者势均则守。");
        }

        [Test]
        public void test_stubborn_defender_general_values_holding_objective()
        {
            Detachment warden = Det(BattleSide.Defender, BattleField.Front, 300, GeneralTag.Defender);
            Detachment plain = Det(BattleSide.Defender, BattleField.Front, 300, null);
            int wardenScore = EnemyZoneAiService.Score(BattleField.Front, false, BattleSide.Defender, warden, 200, 300, 200, Cfg(stubborn: 40));
            int plainScore = EnemyZoneAiService.Score(BattleField.Front, false, BattleSide.Defender, plain, 200, 300, 200, Cfg(stubborn: 40));
            Assert.That(wardenScore - plainScore, Is.EqualTo(40), "善守之将死守要点 +StubbornDefenderBonus。");
        }
    }
}
