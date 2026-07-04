using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// 气质标签→战斗条件涌现（GDD_025 R2）：武将标签在条件涌现中发作。
    /// 例：【诡谋】之将纵智略未达门槛，亦善用奇谋——降智略门，使伏兵/火攻等诡策类条件更易成型。
    /// </summary>
    [TestFixture]
    public class GeneralTagBattleTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;   // GuileMin = 0.6
        private readonly RoundResolutionService _rounds = new RoundResolutionService();

        private static Detachment Atk(ZoneId at, FixedPoint guile, bool cunning)
        {
            var tags = cunning ? new List<GeneralTag> { GeneralTag.Cunning } : new List<GeneralTag>();
            var gen = new OffensiveGeneral(new CharacterId("g"), F(7, 10), F(7, 10), guile, GeneralSpecialty.None, tags);
            return new Detachment(new DetachmentId("a"), BattleSide.Attacker, gen,
                TroopComposition.AllInfantry(300), 300, F(7, 10), F(2, 10), Posture.Assault, at);
        }

        private static Detachment Def(ZoneId at)
            => new Detachment(new DetachmentId("d"), BattleSide.Defender, null,
                TroopComposition.AllInfantry(300), 300, F(7, 10), F(1, 10), Posture.Hold, at);

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed: 7UL);

        private static ZoneBattleContext Dry() => new ZoneBattleContext(false, false, true, isDry: true);

        private static bool Has(IReadOnlyList<string> em, string c)
        {
            foreach (string e in em) if (e.EndsWith(":" + c, StringComparison.Ordinal)) return true;
            return false;
        }

        [Test]
        public void test_cunning_tag_forms_fire_despite_low_guile()
        {
            // 智略 0.3 < 门 0.6：无【诡谋】则纵火门不齐；带【诡谋】则善用奇谋，纵火成型。
            IReadOnlyList<string> noTag = _rounds.ResolveRound(
                State(Atk(BattleField.Supply, F(3, 10), cunning: false), Def(BattleField.Supply)), Dry(), Cfg).Emergences;
            Assert.That(Has(noTag, "FireIgnited"), Is.False, "智略未达门 + 无诡谋 → 纵火不成型。");

            IReadOnlyList<string> cunning = _rounds.ResolveRound(
                State(Atk(BattleField.Supply, F(3, 10), cunning: true), Def(BattleField.Supply)), Dry(), Cfg).Emergences;
            Assert.That(Has(cunning, "FireIgnited"), Is.True, "【诡谋】之将纵智略未达门亦善纵火（标签→条件涌现）。");
        }
    }
}
