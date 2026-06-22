using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-001：表现层设计锁负向断言（结构性保证，反射固化）。
    /// 治理 ADR：ADR-0002。覆盖 P10 敌方无真值泄露、P6 多维不合并、P11 无最优解，
    /// 以及 Presentation 逻辑层不依赖 UnityEngine（边界回归）。
    /// </summary>
    [TestFixture]
    public class PresentationLockTests
    {
        private static string[] PropNames(Type t) =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name.ToLowerInvariant()).ToArray();

        private static void AssertNoPropContains(Type t, params string[] forbiddenSubstrings)
        {
            foreach (var name in PropNames(t))
                foreach (var bad in forbiddenSubstrings)
                    Assert.That(name.Contains(bad), Is.False,
                        $"{t.Name} 不得含属性「{name}」（设计锁禁止 '{bad}'）。");
        }

        // ---- P10: 敌方展示模型无任何真值字段 ----

        [Test]
        public void test_enemy_view_has_no_truth_fields()
        {
            AssertNoPropContains(typeof(EnemyIntelView), "truth", "actual", "real");
            // 正向：只暴露估计/来源/时效字段。
            Assert.That(PropNames(typeof(EnemyIntelView)), Does.Contain("estimatedstrength"));
        }

        // ---- P6: 多维状态无单一综合值 ----

        [Test]
        public void test_cohesion_view_has_no_merged_single_value()
        {
            AssertNoPropContains(typeof(CohesionView), "combined", "overall", "aggregate", "total", "score", "index");
            var names = PropNames(typeof(CohesionView));
            Assert.That(names, Does.Contain("morale"));
            Assert.That(names, Does.Contain("fatigue"));
            Assert.That(names, Does.Contain("discipline"));
        }

        [Test]
        public void test_relationship_view_has_no_merged_favorability()
        {
            AssertNoPropContains(typeof(RelationshipView), "combined", "overall", "favor", "score", "total", "aggregate");
            var names = PropNames(typeof(RelationshipView));
            Assert.That(names, Does.Contain("trust"));
            Assert.That(names, Does.Contain("respect"));
            Assert.That(names, Does.Contain("gratitude"));
            Assert.That(names, Does.Contain("resentment"));
        }

        // ---- P11: 军师建议无成功率/最优解/排序 ----

        [Test]
        public void test_advice_view_has_no_success_rate_or_optimal_marker()
        {
            AssertNoPropContains(typeof(AdviceView),
                "success", "winrate", "winprob", "probability", "optimal", "recommended", "best", "rank", "ranking", "score");
        }

        [Test]
        public void test_council_view_has_no_best_or_ranking_marker()
        {
            AssertNoPropContains(typeof(CouncilView), "best", "optimal", "recommended", "rank", "ranking", "score", "top");
        }

        // ---- 边界回归：Presentation 逻辑层不依赖 UnityEngine ----

        [Test]
        public void test_presentation_assembly_does_not_reference_unityengine()
        {
            var asm = typeof(EnemyIntelView).Assembly;
            bool referencesUnity = asm.GetReferencedAssemblies()
                .Any(a => a.Name != null && a.Name.IndexOf("UnityEngine", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(referencesUnity, Is.False, "Presentation 逻辑层须纯 C#，不依赖 UnityEngine（UXML 外壳另在 Assets/）。");
        }
    }
}
