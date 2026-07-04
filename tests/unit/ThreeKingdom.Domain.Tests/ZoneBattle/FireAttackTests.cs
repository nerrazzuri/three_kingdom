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
    /// 火攻内容包（GDD_010/021）：火攻是<b>多条件涌现</b>非按钮——干燥天时 + 敌暴露于易燃地形（粮营/连营）+
    /// 智将纵火，三门齐方成型；缺任一（如雨天、无智将）则不涌现。条件计入战力加成（现有 condMul）。
    /// </summary>
    [TestFixture]
    public class FireAttackTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly ZoneBattleConfig Cfg = ZoneBattleConfig.Default;
        private readonly RoundResolutionService _rounds = new RoundResolutionService();

        // 干燥上下文（晴、已侦察、干燥）。
        private static ZoneBattleContext Dry() => new ZoneBattleContext(false, false, true, isDry: true);
        private static ZoneBattleContext Wet() => new ZoneBattleContext(false, false, true, isDry: false);

        private static Detachment Cunning(string id, BattleSide side, ZoneId at)
            => new Detachment(new DetachmentId(id), side,
                new OffensiveGeneral(new CharacterId(id + "-gen"), F(7, 10), F(7, 10), F(7, 10)),   // guile 0.7 ≥ 门
                TroopComposition.AllInfantry(300), 300, F(7, 10), F(2, 10), Posture.Assault, at);

        private static Detachment Plain(string id, BattleSide side, ZoneId at)
            => new Detachment(new DetachmentId(id), side, general: null,
                TroopComposition.AllInfantry(300), 300, F(7, 10), F(2, 10), Posture.Hold, at);

        private static ZoneBattleState State(params Detachment[] dets)
            => new ZoneBattleState(BattleField.Default(), dets, Array.Empty<ZoneEngagementState>(),
                new ThreeKingdom.Domain.ZoneBattle.BattleClock(1, 6), BattleSide.Attacker, seed: 7UL);

        private IReadOnlyList<string> Emergences(ZoneBattleState s, ZoneBattleContext ctx)
            => _rounds.ResolveRound(s, ctx, Cfg).Emergences;

        private static bool Has(IReadOnlyList<string> em, string suffix)
        {
            foreach (string e in em) if (e.EndsWith(":" + suffix, StringComparison.Ordinal)) return true;
            return false;
        }

        // ---- 三门齐 → 火攻成型（粮营区，智将，干燥，敌在场）----
        [Test]
        public void test_fire_forms_in_dry_flammable_zone_with_cunning_general_and_enemy()
        {
            IReadOnlyList<string> em = Emergences(State(
                Cunning("atk", BattleSide.Attacker, BattleField.Supply),
                Plain("def", BattleSide.Defender, BattleField.Supply)), Dry());
            Assert.That(Has(em, "DryField"), Is.True, "干燥天时门。");
            Assert.That(Has(em, "EnemyExposedToFire"), Is.True, "敌暴露于易燃粮营门。");
            Assert.That(Has(em, "FireIgnited"), Is.True, "智将纵火门。");
        }

        // ---- 雨天 → 无干燥门 → 火攻不成型 ----
        [Test]
        public void test_fire_does_not_form_when_wet()
        {
            IReadOnlyList<string> em = Emergences(State(
                Cunning("atk", BattleSide.Attacker, BattleField.Supply),
                Plain("def", BattleSide.Defender, BattleField.Supply)), Wet());
            Assert.That(Has(em, "DryField"), Is.False, "雨/湿 → 无干燥天时，火攻不成型。");
        }

        // ---- 无智将 → 无纵火门 ----
        [Test]
        public void test_fire_does_not_ignite_without_cunning_general()
        {
            IReadOnlyList<string> em = Emergences(State(
                Plain("atk", BattleSide.Attacker, BattleField.Supply),   // 无将 → guile 0
                Plain("def", BattleSide.Defender, BattleField.Supply)), Dry());
            Assert.That(Has(em, "FireIgnited"), Is.False, "无智将 → 纵火门不齐。");
        }

        // ---- 敌不在场 → 无暴露门（火攻须有敌可烧）----
        [Test]
        public void test_fire_needs_enemy_present()
        {
            IReadOnlyList<string> em = Emergences(State(
                Cunning("atk", BattleSide.Attacker, BattleField.Supply)), Dry());   // 仅己方
            Assert.That(Has(em, "EnemyExposedToFire"), Is.False, "无敌暴露 → 暴露门不齐。");
        }

        // ---- B4：火攻已注册复盘链（TacticChainConfig）→ 条件齐可识别为"火攻" ----
        [Test]
        public void test_fire_attack_registered_in_tactic_chains()
        {
            ThreeKingdom.Domain.Battle.TacticChainDefinition? fire = null;
            foreach (var def in ThreeKingdom.Domain.Battle.TacticChainConfig.SliceDefault().Chains)
                if (def.Tag == ThreeKingdom.Domain.Battle.TacticTag.FireAttack) fire = def;
            Assert.That(fire, Is.Not.Null, "火攻已注册复盘链（否则条件加战力但识别不出兵法名）。");
            Assert.That(fire!.Required, Does.Contain(TacticCondition.DryField));
            Assert.That(fire.Required, Does.Contain(TacticCondition.EnemyExposedToFire));
            Assert.That(fire.Required, Does.Contain(TacticCondition.FireIgnited));
        }

        // ---- 非易燃地形（平原预备区无火攻禀赋）→ 不成型 ----
        [Test]
        public void test_fire_does_not_form_in_non_flammable_zone()
        {
            IReadOnlyList<string> em = Emergences(State(
                Cunning("atk", BattleSide.Attacker, BattleField.Reserve),
                Plain("def", BattleSide.Defender, BattleField.Reserve)), Dry());
            Assert.That(Has(em, "DryField"), Is.False, "预备区无易燃禀赋 → 火攻不成型。");
        }
    }
}
