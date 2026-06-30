using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.EnemyAI;
using ThreeKingdom.Domain.Numerics;
using V = ThreeKingdom.Domain.Tests.EnemyAI.AiWorldViewTests;

namespace ThreeKingdom.Domain.Tests.EnemyAI
{
    /// <summary>
    /// epic-021 story-002：效用评分 + 硬可行性门（Logic / Domain）。
    /// 治理 ADR：ADR-0006 §1（效用评分）+ ADR-0004（确定性定点）。TR-ai-003。
    /// 覆盖：每动作效用；可行性门淘汰；评分确定性；性格影响。
    /// </summary>
    [TestFixture]
    public class ActionScorerTests
    {
        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static ScorerConfig Config()
            => new ScorerConfig(
                baseUtility: Frac(5, 10), forceAdvantageWeight: Frac(4, 10), riskWeight: Frac(3, 10),
                patienceWeight: Frac(3, 10), urgencyWeight: Frac(2, 10), pursueFeasibleRatio: Frac(12, 10));

        private static PersonalityProfile Personality(int risk, int patience)
            => new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Risk] = Frac(risk, 10),
                [PersonalityTrait.Patience] = Frac(patience, 10),
            });

        private static readonly ActionScorer Scorer = new ActionScorer();

        private static ScoredAction Of(IReadOnlyList<ScoredAction> list, StrategicAction a)
            => list.First(s => s.Action == a);

        // ---- AC-1: 评分产出每动作效用 ----

        [Test]
        public void test_score_returns_all_actions()
        {
            IReadOnlyList<ScoredAction> scored = Scorer.Score(V.View(), Personality(5, 5), Config());
            Assert.That(scored.Count, Is.EqualTo(4));
            Assert.That(scored.Select(s => s.Action), Is.EquivalentTo(new[]
            {
                StrategicAction.Pursue, StrategicAction.Retreat, StrategicAction.Hold, StrategicAction.FeintLure,
            }));
        }

        // ---- AC-2: 硬可行性门淘汰不可行动作 ----

        [Test]
        public void test_pursue_infeasible_when_force_ratio_below_threshold()
        {
            // 己方 3000 vs 感知敌 5000 → ratio 0.6 < 1.2 门槛 → 追击不可行。
            AiWorldView v = V.View(perceivedEnemy: 5000, ownForce: 3000);
            IReadOnlyList<ScoredAction> scored = Scorer.Score(v, Personality(5, 5), Config());

            Assert.That(Of(scored, StrategicAction.Pursue).Feasible, Is.False, "兵力不足追击不可行");
            Assert.That(Of(scored, StrategicAction.Hold).Feasible, Is.True, "坚守恒兜底可行");
        }

        [Test]
        public void test_retreat_infeasible_when_must_defend()
        {
            AiWorldView v = V.View(mustDefend: true);
            IReadOnlyList<ScoredAction> scored = Scorer.Score(v, Personality(5, 5), Config());

            Assert.That(Of(scored, StrategicAction.Retreat).Feasible, Is.False, "守土目标撤退不可行");
        }

        // ---- AC-3: 评分确定性 ----

        [Test]
        public void test_scoring_is_deterministic()
        {
            AiWorldView v = V.View(perceivedEnemy: 3000, ownForce: 5000);
            PersonalityProfile p = Personality(6, 4);

            IReadOnlyList<ScoredAction> a = Scorer.Score(v, p, Config());
            IReadOnlyList<ScoredAction> b = Scorer.Score(v, p, Config());

            for (int i = 0; i < a.Count; i++)
            {
                Assert.That(b[i].Action, Is.EqualTo(a[i].Action));
                Assert.That(b[i].Utility, Is.EqualTo(a[i].Utility), "效用逐位相同（定点无漂移）");
                Assert.That(b[i].Feasible, Is.EqualTo(a[i].Feasible));
            }
        }

        // ---- AC-4: 性格影响效用 ----

        [Test]
        public void test_personality_shifts_utility()
        {
            AiWorldView v = V.View(perceivedEnemy: 3000, ownForce: 5000);

            IReadOnlyList<ScoredAction> risky = Scorer.Score(v, Personality(risk: 9, patience: 1), Config());
            IReadOnlyList<ScoredAction> patientP = Scorer.Score(v, Personality(risk: 1, patience: 9), Config());

            Assert.That(Of(risky, StrategicAction.Pursue).Utility,
                Is.GreaterThan(Of(patientP, StrategicAction.Pursue).Utility), "高风险者追击效用更高");
            Assert.That(Of(patientP, StrategicAction.Hold).Utility,
                Is.GreaterThan(Of(risky, StrategicAction.Hold).Utility), "高耐心者坚守效用更高");
        }
    }
}
