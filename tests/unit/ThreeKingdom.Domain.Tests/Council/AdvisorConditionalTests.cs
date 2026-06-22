using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Council
{
    /// <summary>
    /// epic-005 story-003：军师条件化建议（无最优解/无成功率）。
    /// 治理 ADR：ADR-0002（分层；只读知识投影）+ 强制设计锁 P11。GDD_008 / TR-council-001/002。
    /// 覆盖 AC-1 读召开时只读快照、AC-2 过时标记不静默更新、AC-3 条件化建议六要素、
    /// AC-4 负向（无综合成功率/唯一推荐/自动命令）、AC-5 不暴露真值（只读投影）。
    /// </summary>
    [TestFixture]
    public class AdvisorConditionalTests
    {
        private static readonly WarCouncilService Council = new WarCouncilService();
        private static readonly IntelService Intel = new IntelService();
        private static readonly FactionId Wei = new FactionId("wei");
        private static readonly FactionId Shu = new FactionId("shu");
        private static readonly WorldTime T0 = new WorldTime(2, DaySegment.Day);

        private static readonly IntelSubjectId EnemyCommander = new IntelSubjectId("enemy-commander-trait");
        private static readonly IntelSubjectId EnemyStrength = new IntelSubjectId("enemy-strength");
        private static readonly IntelSubjectId SupplyRoute = new IntelSubjectId("enemy-supply-route");
        private static readonly IntelSubjectId Terrain = new IntelSubjectId("ambush-terrain");

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        /// <summary>构造一个只含给定主题的阵营知识投影（经真值→观察→报告→知识流转）。</summary>
        private static IntelProjection ProjectionWith(params IntelSubjectId[] knownSubjects)
        {
            var truth = new WorldTruthLedger();
            var intel = new FactionIntel(Wei);
            foreach (var s in knownSubjects)
            {
                truth.Set(new TruthRecord(s, 1000, Shu));
                intel.ApplyReport(Intel.ToReport(Intel.Observe(truth, s, Wei, T0), Wei, IntelSource.Scouting));
            }
            return intel.Project();
        }

        private static AdviceTemplate AmbushTemplate(params IntelSubjectId[] referenced)
            => new AdviceTemplate(
                candidateId: "诱敌伏击",
                observation: "敌前锋突出、追击意图明显",
                assumption: "敌将急躁且地形适合设伏",
                requiredConditions: new[] { "敌将性格急躁", "伏击地形可用", "我军可分兵设伏" },
                risks: new[] { "弄假成真被反咬", "敌不追则徒劳" },
                referencedSubjects: referenced);

        private static AdvisorPerspective Advisor(int capN, int capD)
            => new AdvisorPerspective(new AdvisorId("advisor-zhuge"), F(capN, capD));

        private static CouncilConfig Config() => new CouncilConfig(FixedPoint.One);

        // ---- AC-3: 条件化建议六要素 ----

        [Test]
        public void test_advice_contains_conditional_elements()
        {
            var projection = ProjectionWith(EnemyCommander, EnemyStrength);
            var confidences = new Dictionary<IntelSubjectId, FixedPoint> { [EnemyCommander] = F(6, 10), [EnemyStrength] = F(42, 100) };
            var set = Council.Convene(new KnowledgeSnapshotId("snap-1"), projection, confidences,
                Advisor(8, 10), new[] { AmbushTemplate(EnemyCommander, EnemyStrength) }, Config());

            var advice = set.Advice.Single();
            Assert.That(advice.Observation, Is.Not.Empty);
            Assert.That(advice.Assumption, Is.Not.Empty);
            Assert.That(advice.RequiredConditions, Is.Not.Empty);
            Assert.That(advice.Risks, Is.Not.Empty);
            Assert.That(advice.Confidence, Is.GreaterThan(FixedPoint.Zero));
        }

        [Test]
        public void test_confidence_is_weakest_basis_times_capability()
        {
            // min(0.6, 0.42) × adv_cap 0.8 = 0.42 × 0.8（依据可靠性聚合，非成功率）
            var projection = ProjectionWith(EnemyCommander, EnemyStrength);
            var confidences = new Dictionary<IntelSubjectId, FixedPoint> { [EnemyCommander] = F(6, 10), [EnemyStrength] = F(42, 100) };
            var set = Council.Convene(new KnowledgeSnapshotId("snap-1"), projection, confidences,
                Advisor(8, 10), new[] { AmbushTemplate(EnemyCommander, EnemyStrength) }, Config());

            Assert.That(set.Advice.Single().Confidence, Is.EqualTo(F(42, 100) * F(8, 10)));
        }

        [Test]
        public void test_confidence_is_zero_without_any_known_basis()
        {
            var projection = ProjectionWith(); // 空知识
            var set = Council.Convene(new KnowledgeSnapshotId("snap-1"), projection,
                new Dictionary<IntelSubjectId, FixedPoint>(),
                Advisor(8, 10), new[] { AmbushTemplate(EnemyCommander) }, Config());

            Assert.That(set.Advice.Single().Confidence, Is.EqualTo(FixedPoint.Zero), "无已知依据不凭空提升置信。");
        }

        // ---- 缺失情报随能力（不无中生有）----

        [Test]
        public void test_missing_intel_surfaced_scales_with_capability()
        {
            var projection = ProjectionWith(EnemyCommander); // 已知敌将；缺 路线 + 地形
            var confidences = new Dictionary<IntelSubjectId, FixedPoint> { [EnemyCommander] = F(6, 10) };
            var template = AmbushTemplate(EnemyCommander, SupplyRoute, Terrain);

            var lowCap = Council.Convene(new KnowledgeSnapshotId("s"), projection, confidences, Advisor(4, 10), new[] { template }, Config());
            var highCap = Council.Convene(new KnowledgeSnapshotId("s"), projection, confidences, Advisor(9, 10), new[] { template }, Config());

            // 客观缺口 2（路线/地形）：低能力 round(2×0.4)=1，高能力 round(2×0.9)=2
            Assert.That(lowCap.Advice.Single().MissingIntel.Count, Is.EqualTo(1));
            Assert.That(highCap.Advice.Single().MissingIntel.Count, Is.EqualTo(2));
            // 不无中生有：识别出的缺口都是客观未知主题
            Assert.That(highCap.Advice.Single().MissingIntel, Is.SubsetOf(new[] { SupplyRoute, Terrain }));
        }

        // ---- AC-2: 过时标记，不静默更新 ----

        [Test]
        public void test_advice_marked_stale_when_snapshot_changes()
        {
            var set = Council.Convene(new KnowledgeSnapshotId("snap-1"), ProjectionWith(EnemyCommander),
                new Dictionary<IntelSubjectId, FixedPoint> { [EnemyCommander] = F(6, 10) },
                Advisor(8, 10), new[] { AmbushTemplate(EnemyCommander) }, Config());

            Assert.That(set.IsStaleAgainst(new KnowledgeSnapshotId("snap-1")), Is.False);
            Assert.That(set.IsStaleAgainst(new KnowledgeSnapshotId("snap-2")), Is.True, "知识变化后建议过时。");
            // 不静默更新：建议集仍绑定原快照、内容不变
            Assert.That(set.SnapshotId, Is.EqualTo(new KnowledgeSnapshotId("snap-1")));
        }

        // ---- AC-4: 负向——无综合成功率/唯一推荐/自动命令（结构性断言）----

        [Test]
        public void test_advice_statement_exposes_no_success_rate_or_optimal_or_command()
        {
            string[] forbidden = { "success", "optimal", "recommend", "score", "rank", "best", "winner", "probability", "command", "auto", "execute", "submit" };
            var members = typeof(AdviceStatement).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name.ToLowerInvariant());

            foreach (var name in members)
                foreach (var bad in forbidden)
                    Assert.That(name.Contains(bad), Is.False, $"AdviceStatement 不得暴露成员含「{bad}」（实为 {name}）。");
        }

        [Test]
        public void test_advice_set_has_no_ranking_or_best_marker()
        {
            string[] forbidden = { "best", "rank", "optimal", "recommend", "top", "winner", "success" };
            var members = typeof(CouncilAdviceSet).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name.ToLowerInvariant());

            foreach (var name in members)
                foreach (var bad in forbidden)
                    Assert.That(name.Contains(bad), Is.False, $"CouncilAdviceSet 不得含「{bad}」排名标记。");
        }

        [Test]
        public void test_single_candidate_is_not_marked_optimal()
        {
            var set = Council.Convene(new KnowledgeSnapshotId("snap-1"), ProjectionWith(EnemyCommander),
                new Dictionary<IntelSubjectId, FixedPoint> { [EnemyCommander] = F(6, 10) },
                Advisor(8, 10), new[] { AmbushTemplate(EnemyCommander) }, Config());

            // 单一候选：仍只是并列的一条建议，无任何「最优」语义字段（由上一断言保证结构）
            Assert.That(set.Advice.Count, Is.EqualTo(1));
        }

        // ---- AC-5: 不暴露隐藏真值——只读投影，无真值入参 ----

        [Test]
        public void test_convene_consumes_only_knowledge_projection_not_world_truth()
        {
            var method = typeof(WarCouncilService).GetMethod(nameof(WarCouncilService.Convene))!;
            foreach (var p in method.GetParameters())
                Assert.That(p.ParameterType.Name.Contains("Truth"), Is.False,
                    $"军议入参不得包含世界真值类型（实为 {p.ParameterType.Name}）。");
        }

        // ---- 构造不变量 ----

        [Test]
        public void test_advisor_rejects_capability_out_of_range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdvisorPerspective(new AdvisorId("x"), FixedPoint.FromInt(2)));
        }
    }
}
