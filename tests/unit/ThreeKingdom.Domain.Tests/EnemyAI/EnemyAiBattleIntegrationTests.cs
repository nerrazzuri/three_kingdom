using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.EnemyAI;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using V = ThreeKingdom.Domain.Tests.EnemyAI.AiWorldViewTests;

namespace ThreeKingdom.Domain.Tests.EnemyAI
{
    /// <summary>
    /// epic-021 story-004：敌方 AI 决策接入战区命令（Integration / Assembly）。
    /// 治理 ADR：ADR-0006 + ADR-0004（同源确定性）+ ADR-0009（接入战斗）。TR-ai-004。
    /// 覆盖：动作→命令映射；AI 命令驱动战斗 + 确定性重放；随机与战斗同源；反全知贯穿。
    /// </summary>
    [TestFixture]
    public class EnemyAiBattleIntegrationTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Ai = new FactionId("faction-yuan");
        private static readonly RegionId Field = new RegionId("region-field");
        private static readonly BattleUnitId PlayerUnit = new BattleUnitId("unit-player-1");
        private static readonly BattleUnitId AiUnit = new BattleUnitId("unit-ai-1");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static ScorerConfig Config()
            => new ScorerConfig(Frac(5, 10), Frac(4, 10), Frac(3, 10), Frac(3, 10), Frac(2, 10), Frac(12, 10));
        private static PersonalityProfile Personality()
            => new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Risk] = Frac(7, 10), [PersonalityTrait.Patience] = Frac(3, 10),
            });

        private static BattleUnitState Unit(BattleUnitId id, FactionId faction, int force)
            => new BattleUnitState(id, faction, Field, force, Frac(7, 10), Frac(2, 10), Frac(6, 10), Frac(1, 1), Frac(1, 1), Frac(0, 1));

        private static BattleSnapshot Snapshot()
            => new BattleSnapshot(new[] { Unit(PlayerUnit, Player, 800), Unit(AiUnit, Ai, 5000) }, new DetectionState(), "fp");

        private static readonly EnemyAiBattleAdapter Adapter = new EnemyAiBattleAdapter();
        private static readonly BattleResolver Resolver = new BattleResolver();
        private static BattleConfig BattleCfg() => new BattleConfig(Frac(15, 10), Frac(1, 1));

        // ---- AC-1: 动作→命令映射 ----

        [Test]
        public void test_action_to_battle_order_mapping()
        {
            Assert.That(EnemyAiBattleAdapter.ToBattleOrder(StrategicAction.Pursue, 0, AiUnit, PlayerUnit).Type,
                Is.EqualTo(BattleOrderType.Engage));
            Assert.That(EnemyAiBattleAdapter.ToBattleOrder(StrategicAction.Retreat, 0, AiUnit, PlayerUnit).Type,
                Is.EqualTo(BattleOrderType.Retreat));
            Assert.That(EnemyAiBattleAdapter.ToBattleOrder(StrategicAction.Hold, 0, AiUnit, PlayerUnit).Type,
                Is.EqualTo(BattleOrderType.Hold));
            Assert.That(EnemyAiBattleAdapter.ToBattleOrder(StrategicAction.FeintLure, 0, AiUnit, PlayerUnit).Type,
                Is.EqualTo(BattleOrderType.Conceal));
        }

        // ---- AC-2: AI 命令驱动战斗 + 确定性重放 ----

        [Test]
        public void test_ai_command_drives_battle_deterministically()
        {
            ulong battleSeed = 42;

            // 两次：AI 决策种子从战斗种子派生（同源）→ 同 AI 命令 → 同战斗 hash。
            BattleResolution Run()
            {
                var rng = new DeterministicRandom(battleSeed);   // AI 与战斗同源
                var (_, aiOrder) = Adapter.DecideOrder(V.View(perceivedEnemy: 800, ownForce: 5000),
                    Personality(), Config(), Frac(5, 10), rng, sequence: 1, AiUnit, PlayerUnit);
                var orders = new List<BattleOrder>
                {
                    new BattleOrder(0, PlayerUnit, BattleOrderType.Hold),
                    aiOrder,
                };
                return Resolver.ResolvePhase(Snapshot(), orders, battleSeed, BattleCfg());
            }

            BattleResolution a = Run();
            BattleResolution b = Run();

            Assert.That(a.Hash, Is.EqualTo(b.Hash), "同种子+同态势→同 AI 命令→同战斗哈希（重放）");
        }

        // ---- AC-3: AI 随机与战斗同源（同 battleSeed 整局可复现）----

        [Test]
        public void test_ai_decision_deterministic_under_same_seed()
        {
            var (d1, o1) = Adapter.DecideOrder(V.View(perceivedEnemy: 800, ownForce: 5000),
                Personality(), Config(), Frac(5, 10), new DeterministicRandom(99), 1, AiUnit, PlayerUnit);
            var (d2, o2) = Adapter.DecideOrder(V.View(perceivedEnemy: 800, ownForce: 5000),
                Personality(), Config(), Frac(5, 10), new DeterministicRandom(99), 1, AiUnit, PlayerUnit);

            Assert.That(d1.Selected, Is.EqualTo(d2.Selected));
            Assert.That(o1.Type, Is.EqualTo(o2.Type), "同种子→同命令");
        }

        // ---- AC-4: AI 反全知贯穿战区（按情报决策，含错误信念）----

        [Test]
        public void test_ai_decides_by_intel_not_battle_truth()
        {
            // AI 感知玩家军 800（弱）→ 兵力优势 → 倾向追击/进攻；决策依据是情报感知，非战场真值。
            var (decision, _) = Adapter.DecideOrder(V.View(perceivedEnemy: 800, ownForce: 5000),
                Personality(), Config(), Frac(3, 10), new DeterministicRandom(5), 1, AiUnit, PlayerUnit);

            Assert.That(decision.PerceivedEnemyForce, Is.EqualTo(800), "决策依据=情报感知（错误信念可读，非战场真值）");
        }
    }
}
