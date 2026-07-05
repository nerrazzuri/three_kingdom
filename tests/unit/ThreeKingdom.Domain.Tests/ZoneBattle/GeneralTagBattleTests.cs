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

        private static Detachment Led(CombatTier tier, ZoneId at)
        {
            var gen = new OffensiveGeneral(new CharacterId("g"), F(5, 10), F(5, 10), F(3, 10), GeneralSpecialty.None, null, tier);
            return new Detachment(new DetachmentId("a"), BattleSide.Attacker, gen,
                TroopComposition.AllInfantry(1000), 1000, F(7, 10), F(2, 10), Posture.Assault, at);
        }

        private static int DefLoss(CombatTier attackerTier, ulong seed)
        {
            // 守方略强（1200 兵坚守）→ 胶着战，攻方战阵档系数决定胜负 margin（不饱和于减员上限）。
            var def = new Detachment(new DetachmentId("d"), BattleSide.Defender, null,
                TroopComposition.AllInfantry(1200), 1200, F(7, 10), F(1, 10), Posture.Hold, BattleField.Reserve);
            var s = new ZoneBattleState(BattleField.Default(),
                new[] { Led(attackerTier, BattleField.Reserve), def },
                Array.Empty<ZoneEngagementState>(), new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed);
            var svc = new RoundResolutionService();
            ZoneBattleState after = svc.ResolveRound(s, ZoneBattleContext.Default, ZoneBattleConfig.Default).State;
            return 1200 - after.TryGet(new DetachmentId("d"))!.Strength;
        }

        // ---- 战阵档：绝世之将 1000 兵杀伤 > 骁锐之将 1000 兵（同种子下绝世系数恒高，确定性）----
        [Test]
        public void test_peerless_general_kills_more_than_valiant_with_same_troops()
        {
            foreach (ulong seed in new ulong[] { 1UL, 7UL, 42UL, 100UL })
            {
                int peerless = DefLoss(CombatTier.Peerless, seed);
                int valiant = DefLoss(CombatTier.Valiant, seed);
                Assert.That(peerless, Is.GreaterThan(valiant),
                    $"seed{seed}：绝世将 1000 兵每回合杀敌 > 骁锐将 1000 兵（战阵档→更强杀伤，关羽>周仓）。");
            }
        }

        // ---- 谋略档：经天纬地之谋帅成型兵法威力 > 愚钝之将同兵法（计谋系数放大条件加成，诸葛>马谡）----
        private static Detachment StratLed(StrategyTier strat, ZoneId at)
        {
            // 智略 0.7 ≥ 门 0.6 → 干燥天时下火攻成型；主谋谋略档决定该兵法加成的放大倍率。
            var gen = new OffensiveGeneral(new CharacterId("g"), F(5, 10), F(5, 10), F(7, 10),
                GeneralSpecialty.None, null, null, strat);
            return new Detachment(new DetachmentId("a"), BattleSide.Attacker, gen,
                TroopComposition.AllInfantry(1000), 1000, F(8, 10), F(2, 10), Posture.Assault, at);
        }

        private static int StratDefLoss(StrategyTier strat, ulong seed)
        {
            // 攻强守弱且不悬殊 → 攻方两案均胜、比值未饱和于减员上限；差异纯由计谋系数放大兵法加成而来。
            var def = new Detachment(new DetachmentId("d"), BattleSide.Defender, null,
                TroopComposition.AllInfantry(1000), 1000, F(65, 100), F(1, 10), Posture.Hold, BattleField.Supply);
            var s = new ZoneBattleState(BattleField.Default(),
                new[] { StratLed(strat, BattleField.Supply), def },
                Array.Empty<ZoneEngagementState>(),
                new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed);
            var svc = new RoundResolutionService();
            ZoneBattleState after = svc.ResolveRound(s, Dry(), ZoneBattleConfig.Default).State;   // 干燥 → 火攻可成型
            return 1000 - after.TryGet(new DetachmentId("d"))!.Strength;
        }

        [Test]
        public void test_master_strategist_amplifies_tactics_more_than_dull()
        {
            foreach (ulong seed in new ulong[] { 1UL, 7UL, 42UL, 100UL })
            {
                int master = StratDefLoss(StrategyTier.Master, seed);
                int dull = StratDefLoss(StrategyTier.Dull, seed);
                Assert.That(master, Is.GreaterThan(dull),
                    $"seed{seed}：经天纬地之谋帅成型兵法威力 > 愚钝之将同兵法（谋略档→放大条件加成，诸葛>马谡）。");
            }
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
