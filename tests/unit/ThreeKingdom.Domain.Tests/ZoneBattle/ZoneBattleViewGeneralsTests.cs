using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>战斗界面呈现守将/副将（GDD_027 B/C 前端）：我方支队显将领名；敌方将领反全知——已侦察现真名，否则「未探明之将」。</summary>
    [TestFixture]
    public class ZoneBattleViewGeneralsTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static OffensiveGeneral G(string id, GeneralTag tag)
            => new OffensiveGeneral(new CharacterId(id), F(6, 10), F(7, 10), F(6, 10), GeneralSpecialty.None, new[] { tag }, CombatTier.Valiant, StrategyTier.Plain);

        private static ZoneBattleState BuildState()
        {
            var field = BattleFieldCatalog.ForTerrain(TerrainKind.Pass);
            var dets = new List<Detachment>
            {
                new Detachment(new DetachmentId("atk-front"), BattleSide.Attacker, G("char-guanyu", GeneralTag.Cavalry),
                    TroopComposition.AllInfantry(300), 300, F(7, 10), F(1, 10), Posture.Assault, BattleField.Front),
                new Detachment(new DetachmentId("def-front"), BattleSide.Defender, G("char-caoren", GeneralTag.Defender),
                    TroopComposition.AllInfantry(400), 400, F(7, 10), F(1, 10), Posture.Hold, BattleField.Front),
            };
            return new ZoneBattleService().Start(field, dets, BattleSide.Attacker, 6, 123UL);
        }

        private static ZoneLineView FrontZone(ZoneBattleView v)
        {
            foreach (ZoneLineView z in v.Zones) if (z.ZoneId == BattleField.Front.Value) return z;
            return null!;
        }

        [Test]
        public void test_own_detachment_shows_leader_name()
        {
            // Act
            ZoneBattleView v = ZoneBattleView.FromState(BuildState(), ZoneBattleOutcome.Ongoing, null, defendersRevealed: true);
            ZoneLineView front = FrontZone(v);
            // Assert：我方支队摘要含主将名（关羽）。
            Assert.That(front.OwnDetachments.Count, Is.GreaterThan(0));
            Assert.That(front.OwnDetachments[0].Contains(DisplayNames.Of("char-guanyu")), Is.True, "我方支队显主将名。");
        }

        [Test]
        public void test_enemy_commander_revealed_when_scouted()
        {
            // Act：已侦察 → 敌将现真名。
            ZoneBattleView v = ZoneBattleView.FromState(BuildState(), ZoneBattleOutcome.Ongoing, null, defendersRevealed: true);
            ZoneLineView front = FrontZone(v);
            // Assert
            Assert.That(front.EnemyCommanders.Count, Is.GreaterThan(0), "守方将领应投影到目标区。");
            Assert.That(front.EnemyCommanders[0], Is.EqualTo(DisplayNames.Of("char-caoren")), "已侦察 → 敌将现真名（曹仁）。");
        }

        [Test]
        public void test_enemy_commander_hidden_when_not_scouted()
        {
            // Act：未侦察 → 未探明（反全知）。
            ZoneBattleView v = ZoneBattleView.FromState(BuildState(), ZoneBattleOutcome.Ongoing, null, defendersRevealed: false);
            ZoneLineView front = FrontZone(v);
            // Assert
            Assert.That(front.EnemyCommanders[0], Is.EqualTo("未探明之将"), "未侦察 → 敌将未探明（反全知）。");
        }
    }
}
