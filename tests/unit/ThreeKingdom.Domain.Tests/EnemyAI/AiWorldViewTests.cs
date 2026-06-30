using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.EnemyAI;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.EnemyAI
{
    /// <summary>
    /// epic-021 共享夹具 + story-001：AiWorldView 反全知锁（Logic / Domain）。
    /// 治理 ADR：ADR-0006 §2（反全知锁，结构级）+ ADR-0002（分层）。TR-ai-001。
    /// </summary>
    [TestFixture]
    public class AiWorldViewTests
    {
        internal static readonly FactionId Ai = new FactionId("faction-yuan");
        internal static readonly IntelSubjectId PlayerArmy = new IntelSubjectId("subject-player-army");

        internal static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        internal static EnvironmentModifierSet Env()
            => new EnvironmentModifierSet(new Dictionary<EnvironmentConsumer, EnvironmentModifier>());

        // AI 对玩家军的情报评估（感知兵力 = 区间中心；可能与真值不同）。
        internal static IntelAssessment EnemyAssessment(int perceivedForce = 3000, int halfWidth = 500)
            => new IntelAssessment(
                new ConfidenceSignals(IntelSource.Scouting, Frac(8, 10), age: 1, freshness: Frac(9, 10), effectiveConfidence: Frac(7, 10)),
                new EstimateInterval(perceivedForce, halfWidth));

        // AI 阵营知识投影（含已侦察的玩家军主题）。
        internal static IntelProjection Knowledge(bool knowsPlayer = true)
        {
            var intel = new FactionIntel(Ai);
            if (knowsPlayer)
                intel.ApplyReport(new IntelReport(PlayerArmy, Ai, 3000, IntelSource.Scouting, new WorldTime(0, DaySegment.Dawn)));
            return intel.Project();
        }

        internal static OwnForceSnapshot Own(int force = 5000)
            => new OwnForceSnapshot(force, Frac(7, 10), Frac(6, 10));

        internal static ObjectivePressure Objective(bool mustDefend = false)
            => new ObjectivePressure(Frac(5, 10), mustDefend);

        internal static AiWorldView View(int perceivedEnemy = 3000, int ownForce = 5000, bool mustDefend = false)
            => new AiWorldView(Knowledge(), EnemyAssessment(perceivedEnemy), Own(ownForce), Env(), Objective(mustDefend));

        // ---- AC-1: AiWorldView 持阵营知识，不暴露真值 ----

        [Test]
        public void test_aiworldview_holds_faction_knowledge()
        {
            AiWorldView v = View();
            Assert.That(v.OwnIntel.Knows(PlayerArmy), Is.True, "AI 经阵营知识知玩家军");
            Assert.That(v.Own.Force, Is.EqualTo(5000), "AI 知己方真实兵力");
        }

        [Test]
        public void test_aiworldview_exposes_no_truth_types()
        {
            // 反全知锁结构级：AiWorldView 公共 API 面只含阵营知识/评估/己方/环境/目标类型，
            // 无任何 MapTruth/WorldTruth/TruthRecord。此处断言可读的敌情来自情报投影/评估（非真值）。
            AiWorldView v = View();
            Type t = typeof(AiWorldView);
            foreach (var prop in t.GetProperties())
            {
                string typeName = prop.PropertyType.Name;
                Assert.That(typeName, Does.Not.Contain("WorldTruth"), $"{prop.Name} 不得暴露 WorldTruth");
                Assert.That(typeName, Does.Not.Contain("MapTruth"), $"{prop.Name} 不得暴露 MapTruth");
                Assert.That(typeName, Does.Not.Contain("TruthRecord"), $"{prop.Name} 不得暴露 TruthRecord");
            }
            // 构造函数参数面同样无真值类型。
            foreach (var ctor in t.GetConstructors())
                foreach (var p in ctor.GetParameters())
                {
                    Assert.That(p.ParameterType.Name, Does.Not.Contain("Truth"), $"构造参数 {p.Name} 不得为真值类型");
                }
        }

        // ---- AC-2: AI 感知敌情来自情报投影（可能误判）----

        [Test]
        public void test_perceived_enemy_force_from_intel_not_truth()
        {
            AiWorldView v = View(perceivedEnemy: 3000);
            Assert.That(v.PerceivedEnemyForce, Is.EqualTo(3000), "感知兵力=情报估计中心（非真值）");
        }

        [Test]
        public void test_missing_intel_means_ai_does_not_know()
        {
            var intel = new FactionIntel(Ai);   // 未侦察
            var v = new AiWorldView(intel.Project(), EnemyAssessment(), Own(), Env(), Objective());
            Assert.That(v.OwnIntel.Knows(PlayerArmy), Is.False, "未侦察则 AI 不知该敌（信息缺失涌现）");
        }

        // ---- AC-3: StrategicAction 候选完整 ----

        [Test]
        public void test_strategic_actions_defined()
        {
            var actions = (StrategicAction[])Enum.GetValues(typeof(StrategicAction));
            Assert.That(actions, Contains.Item(StrategicAction.Pursue));
            Assert.That(actions, Contains.Item(StrategicAction.Retreat));
            Assert.That(actions, Contains.Item(StrategicAction.Hold));
            Assert.That(actions, Contains.Item(StrategicAction.FeintLure));
        }

        // ---- AC-4: AiWorldView 不可变 ----

        [Test]
        public void test_aiworldview_is_immutable_snapshot()
        {
            AiWorldView v = View(perceivedEnemy: 3000, ownForce: 5000);
            Assert.That(v.PerceivedEnemyForce, Is.EqualTo(3000));
            Assert.That(v.PerceivedEnemyForce, Is.EqualTo(3000), "多次读稳定");
            Assert.That(v.Own.Force, Is.EqualTo(5000));
        }
    }
}
