using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;

namespace ThreeKingdom.Domain.Tests.Battle
{
    /// <summary>
    /// epic-007 story-002：条件链涌现与复盘标签（无无条件按钮）。
    /// 治理 ADR：ADR-0004 + 强制设计锁。GDD_010 / TR-battle-002。
    /// 覆盖 AC-1 三链可解析、AC-2 仅事后打标签无执行按钮、AC-3 缺前置不触发（逐链负向）、
    /// AC-4 可解释 Top≤5、AC-5 夜袭可组合非独立技能。
    /// </summary>
    [TestFixture]
    public class ConditionChainEmergenceTests
    {
        private static readonly TacticRecognizer Recognizer = new TacticRecognizer();
        private static readonly TacticChainConfig Config = TacticChainConfig.SliceDefault();

        private static RetrospectiveContext Context(params TacticCondition[] satisfied)
            => new RetrospectiveContext(satisfied);

        private static readonly TacticCondition[] Feint =
            { TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued, TacticCondition.AmbushSurprise };
        private static readonly TacticCondition[] Supply =
            { TacticCondition.SupplyLineCut, TacticCondition.ShortageReachedGrace, TacticCondition.EnemyCohesionCrossedThreshold };
        private static readonly TacticCondition[] Hold =
            { TacticCondition.HeldPosition, TacticCondition.ReliefArrived, TacticCondition.SurvivedDeadline };
        private static readonly TacticCondition[] Night =
            { TacticCondition.IsNight, TacticCondition.StealthSuccess, TacticCondition.DefenderUnaware, TacticCondition.RaiderDisciplineMet };

        // ---- AC-1: 三链可解析 ----

        [Test]
        public void test_feint_ambush_emerges_when_all_conditions_met()
        {
            var result = Recognizer.Recognize(Context(Feint), Config);
            Assert.That(result.Any(t => t.Tag == TacticTag.FeintAmbush), Is.True);
        }

        [Test]
        public void test_supply_exhaustion_emerges_when_all_conditions_met()
        {
            var result = Recognizer.Recognize(Context(Supply), Config);
            Assert.That(result.Any(t => t.Tag == TacticTag.SupplyExhaustion), Is.True);
        }

        [Test]
        public void test_hold_until_relief_emerges_when_all_conditions_met()
        {
            var result = Recognizer.Recognize(Context(Hold), Config);
            Assert.That(result.Any(t => t.Tag == TacticTag.HoldUntilRelief), Is.True);
        }

        // ---- AC-3: 缺前置不触发（逐链负向，含恰好差一项）----

        [Test]
        public void test_feint_ambush_does_not_emerge_missing_one_condition()
        {
            // 缺 AmbushSurprise（恰好差一项）
            var result = Recognizer.Recognize(
                Context(TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued), Config);
            Assert.That(result.Any(t => t.Tag == TacticTag.FeintAmbush), Is.False);
        }

        [Test]
        public void test_no_chain_emerges_from_empty_context()
        {
            var result = Recognizer.Recognize(Context(), Config);
            Assert.That(result, Is.Empty, "无任何前置条件 → 无兵法涌现、无复盘标签。");
        }

        [Test]
        public void test_supply_exhaustion_missing_cohesion_threshold_does_not_emerge()
        {
            var result = Recognizer.Recognize(
                Context(TacticCondition.SupplyLineCut, TacticCondition.ShortageReachedGrace), Config);
            Assert.That(result.Any(t => t.Tag == TacticTag.SupplyExhaustion), Is.False);
        }

        // ---- AC-5: 夜袭可组合，非独立技能 ----

        [Test]
        public void test_night_raid_combines_with_another_chain()
        {
            var combined = Feint.Concat(Night).ToArray();
            var result = Recognizer.Recognize(Context(combined), Config);

            Assert.That(result.Any(t => t.Tag == TacticTag.FeintAmbush), Is.True);
            Assert.That(result.Any(t => t.Tag == TacticTag.NightRaid), Is.True, "夜袭作为组合手段与其他链并存。");
        }

        // ---- AC-4: 可解释 Top≤5 ----

        [Test]
        public void test_recognized_tactic_exposes_at_most_five_causal_factors()
        {
            var result = Recognizer.Recognize(Context(Feint.Concat(Supply).Concat(Hold).Concat(Night).ToArray()), Config);
            foreach (var tactic in result)
                Assert.That(tactic.MatchedConditions.Count, Is.LessThanOrEqualTo(5));
        }

        // ---- AC-2: 无执行按钮（结构性反射断言）----

        [Test]
        public void test_recognizer_exposes_no_execute_or_activate_method()
        {
            string[] forbidden = { "execute", "activate", "perform", "cast", "trigger", "button", "skill", "apply" };
            var methods = typeof(TacticRecognizer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(m => m.Name.ToLowerInvariant());

            foreach (var name in methods)
                foreach (var bad in forbidden)
                    Assert.That(name.Contains(bad), Is.False, $"识别器不得暴露执行语义方法「{bad}」（实为 {name}）。");
        }

        // ---- 确定性 ----

        [Test]
        public void test_recognition_is_deterministic()
        {
            var ctx = Context(Feint.Concat(Night).ToArray());
            var a = Recognizer.Recognize(ctx, Config);
            var b = Recognizer.Recognize(ctx, Config);
            Assert.That(b.Select(t => t.Tag), Is.EqualTo(a.Select(t => t.Tag)));
        }
    }
}
