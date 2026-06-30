using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.EnemyAI;
using ThreeKingdom.Domain.Numerics;
using V = ThreeKingdom.Domain.Tests.EnemyAI.AiWorldViewTests;

namespace ThreeKingdom.Domain.Tests.EnemyAI
{
    /// <summary>
    /// epic-021 story-003：种子化 softmax 选择 + DecisionRecord（Logic / Domain）。
    /// 治理 ADR：ADR-0006 §1（种子 softmax）+ ADR-0004（确定性）。TR-ai-002/003。
    /// 覆盖：同种子同选择；绝不选被淘汰动作；温度单调性；DecisionRecord 缘由码 + 错误信念；Decide 确定性。
    /// </summary>
    [TestFixture]
    public class SoftmaxDecisionTests
    {
        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static ScorerConfig Config()
            => new ScorerConfig(Frac(5, 10), Frac(4, 10), Frac(3, 10), Frac(3, 10), Frac(2, 10), Frac(12, 10));

        private static PersonalityProfile Personality(int risk = 5, int patience = 5)
            => new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Risk] = Frac(risk, 10),
                [PersonalityTrait.Patience] = Frac(patience, 10),
            });

        private static readonly SoftmaxActionSelector Selector = new SoftmaxActionSelector();
        private static readonly EnemyAiService Service = new EnemyAiService();

        private static List<ScoredAction> Scored(params (StrategicAction a, int util, bool feasible)[] items)
        {
            var list = new List<ScoredAction>();
            foreach (var it in items) list.Add(new ScoredAction(it.a, Frac(it.util, 10), it.feasible));
            return list;
        }

        // ---- AC-1: 同种子同选择 ----

        [Test]
        public void test_same_seed_same_selection()
        {
            var scored = Scored(
                (StrategicAction.Pursue, 8, true), (StrategicAction.Hold, 5, true), (StrategicAction.Retreat, 3, true));

            StrategicAction a = Selector.Select(scored, Frac(5, 10), new DeterministicRandom(42));
            StrategicAction b = Selector.Select(scored, Frac(5, 10), new DeterministicRandom(42));

            Assert.That(a, Is.EqualTo(b));
        }

        // ---- AC-2: 绝不选被淘汰动作 ----

        [Test]
        public void test_never_selects_infeasible_action()
        {
            // Pursue 效用最高但不可行 → 任何种子都不选。
            var scored = Scored(
                (StrategicAction.Pursue, 100, false), (StrategicAction.Hold, 5, true), (StrategicAction.Retreat, 4, true));

            for (ulong seed = 0; seed < 30; seed++)
            {
                StrategicAction sel = Selector.Select(scored, Frac(8, 10), new DeterministicRandom(seed));
                Assert.That(sel, Is.Not.EqualTo(StrategicAction.Pursue), $"种子 {seed} 不得选不可行的追击");
            }
        }

        [Test]
        public void test_single_feasible_always_selected()
        {
            var scored = Scored(
                (StrategicAction.Pursue, 100, false), (StrategicAction.Hold, 5, true), (StrategicAction.Retreat, 9, false));
            StrategicAction sel = Selector.Select(scored, Frac(5, 10), new DeterministicRandom(7));
            Assert.That(sel, Is.EqualTo(StrategicAction.Hold), "仅一个可行必选它");
        }

        // ---- AC-3: 温度单调性（低温更集中于占优动作）----

        [Test]
        public void test_temperature_monotonicity()
        {
            // 一动作明显占优（Pursue 9 vs 其余 1）；统计多种子选中 Pursue 比例：低温 > 高温。
            var scored = Scored(
                (StrategicAction.Pursue, 90, true), (StrategicAction.Hold, 10, true), (StrategicAction.Retreat, 10, true));

            int lowTempPursue = 0, highTempPursue = 0;
            const int N = 200;
            for (ulong seed = 0; seed < N; seed++)
            {
                if (Selector.Select(scored, Frac(1, 10), new DeterministicRandom(seed)) == StrategicAction.Pursue) lowTempPursue++;
                if (Selector.Select(scored, Frac(50, 10), new DeterministicRandom(seed)) == StrategicAction.Pursue) highTempPursue++;
            }

            Assert.That(lowTempPursue, Is.GreaterThan(highTempPursue), "低温更集中于占优动作（高温分布趋平）");
        }

        // ---- AC-4: Decide 产 DecisionRecord（错误信念可读）----

        [Test]
        public void test_decide_produces_decision_record_with_belief()
        {
            // AI 感知敌 3000（来自情报，可能与真值不同）。
            AiWorldView view = V.View(perceivedEnemy: 3000, ownForce: 5000);
            DecisionRecord rec = Service.Decide(view, Personality(8, 2), Config(), Frac(5, 10), new DeterministicRandom(42));

            Assert.That(rec.Candidates.Count, Is.EqualTo(4), "含全部候选评分（复盘）");
            Assert.That(rec.PerceivedEnemyForce, Is.EqualTo(3000), "AI 错误信念（感知敌情）可读");
            Assert.That(System.Enum.IsDefined(typeof(AiReasonCode), rec.Reason), Is.True, "缘由码有效");
        }

        // ---- AC-5: Decide 确定性 ----

        [Test]
        public void test_decide_is_deterministic()
        {
            AiWorldView view = V.View(perceivedEnemy: 3000, ownForce: 5000);
            PersonalityProfile p = Personality(6, 4);

            DecisionRecord a = Service.Decide(view, p, Config(), Frac(5, 10), new DeterministicRandom(99));
            DecisionRecord b = Service.Decide(view, p, Config(), Frac(5, 10), new DeterministicRandom(99));

            Assert.That(a.Selected, Is.EqualTo(b.Selected));
            Assert.That(a.Reason, Is.EqualTo(b.Reason));
            Assert.That(a.PerceivedEnemyForce, Is.EqualTo(b.PerceivedEnemyForce));
        }
    }
}
