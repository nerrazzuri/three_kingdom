using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Preparation;

namespace ThreeKingdom.Domain.Tests.Preparation
{
    /// <summary>
    /// epic-006 story-002：硬冲突校验与 DAG 依赖图。
    /// 治理 ADR：ADR-0004（确定性；平局稳定序）。GDD_009 / TR-prep-002。
    /// 覆盖 AC-1 循环依赖拒绝、AC-2 错误 vs 风险、硬冲突五类聚合、校验确定性。
    /// </summary>
    [TestFixture]
    public class ConflictDagValidationTests
    {
        private static readonly PlanValidator Validator = new PlanValidator();
        private static readonly RegionId R1 = new RegionId("region-fanshui");
        private static readonly ResourceKey Grain = new ResourceKey("grain");

        private static PreparedOrder Order(
            string id, string executor = "char-a", RegionId? target = null,
            int start = 0, int end = 2, long grainNeed = 0, params string[] deps)
            => new PreparedOrder(
                new OrderId(id),
                new CharacterId(executor),
                target ?? R1,
                new TimeWindow(start, end),
                grainNeed > 0 ? new Dictionary<ResourceKey, long> { [Grain] = grainNeed } : null,
                deps.Select(d => new OrderId(d)).ToArray());

        private static PreparationContext Context(
            long grainAvail = 10000, IEnumerable<RegionId>? reachable = null, IEnumerable<string>? authorized = null)
            => new PreparationContext(
                reachable ?? new[] { R1 },
                new Dictionary<ResourceKey, long> { [Grain] = grainAvail },
                (authorized ?? new[] { "o1", "o2", "o3", "a", "b", "c" }).Select(s => new OrderId(s)));

        private static PreparationConfig Config(long margin = 0) => new PreparationConfig(margin);

        // ---- AC-1: 循环依赖拒绝 ----

        [Test]
        public void test_cyclic_dependency_is_rejected()
        {
            var orders = new[] { Order("a", deps: "b"), Order("b", deps: "a") };
            var result = Validator.Validate(orders, Context(authorized: new[] { "a", "b" }), Config());

            Assert.That(result.HasError(PlanErrorCode.CyclicDependency), Is.True);
            Assert.That(result.CanCommit, Is.False);
        }

        [Test]
        public void test_self_dependency_is_a_cycle()
        {
            var orders = new[] { Order("a", deps: "a") };
            var result = Validator.Validate(orders, Context(authorized: new[] { "a" }), Config());

            Assert.That(result.HasError(PlanErrorCode.CyclicDependency), Is.True);
        }

        [Test]
        public void test_acyclic_dependency_chain_passes_dag_check()
        {
            // a → b → c（无环）
            var orders = new[] { Order("c", deps: "b"), Order("b", deps: "a"), Order("a") };
            var result = Validator.Validate(orders, Context(authorized: new[] { "a", "b", "c" }), Config());

            Assert.That(result.HasError(PlanErrorCode.CyclicDependency), Is.False);
        }

        // ---- AC-2: 错误 vs 风险 ----

        [Test]
        public void test_resource_shortage_is_a_blocking_error()
        {
            var orders = new[] { Order("o1", grainNeed: 600) };
            var result = Validator.Validate(orders, Context(grainAvail: 500), Config());

            Assert.That(result.HasError(PlanErrorCode.ResourceShortage), Is.True);
            Assert.That(result.CanCommit, Is.False);
        }

        [Test]
        public void test_resource_exactly_enough_is_risk_not_error()
        {
            var orders = new[] { Order("o1", grainNeed: 500) };
            var result = Validator.Validate(orders, Context(grainAvail: 500), Config(margin: 0));

            Assert.That(result.Errors, Is.Empty, "恰好够不是硬冲突。");
            Assert.That(result.CanCommit, Is.True);
            Assert.That(result.Risks.Any(r => r.Code == PlanRiskCode.TightResource), Is.True, "恰好够列为软风险。");
        }

        [Test]
        public void test_ample_resource_yields_neither_error_nor_risk()
        {
            var orders = new[] { Order("o1", grainNeed: 100) };
            var result = Validator.Validate(orders, Context(grainAvail: 10000), Config(margin: 0));

            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.Risks, Is.Empty);
        }

        // ---- 硬冲突五类 + 聚合 ----

        [Test]
        public void test_time_overlap_for_same_executor_is_error()
        {
            var orders = new[]
            {
                Order("o1", executor: "char-a", start: 0, end: 4),
                Order("o2", executor: "char-a", start: 2, end: 6),
            };
            var result = Validator.Validate(orders, Context(), Config());

            Assert.That(result.HasError(PlanErrorCode.TimeOverlap), Is.True);
        }

        [Test]
        public void test_non_overlapping_windows_same_executor_pass()
        {
            var orders = new[]
            {
                Order("o1", executor: "char-a", start: 0, end: 2),
                Order("o2", executor: "char-a", start: 2, end: 4),
            };
            var result = Validator.Validate(orders, Context(), Config());

            Assert.That(result.HasError(PlanErrorCode.TimeOverlap), Is.False);
        }

        [Test]
        public void test_unreachable_target_is_error()
        {
            var orders = new[] { Order("o1", target: new RegionId("region-faraway")) };
            var result = Validator.Validate(orders, Context(reachable: new[] { R1 }), Config());

            Assert.That(result.HasError(PlanErrorCode.Unreachable), Is.True);
        }

        [Test]
        public void test_unauthorized_order_is_error()
        {
            var orders = new[] { Order("o1") };
            var result = Validator.Validate(orders, Context(authorized: Array.Empty<string>()), Config());

            Assert.That(result.HasError(PlanErrorCode.NoAuthority), Is.True);
        }

        [Test]
        public void test_all_hard_conflicts_are_aggregated_not_first_stop()
        {
            // 同时：不可达 + 无权限 + 资源不足
            var orders = new[] { Order("o1", target: new RegionId("nowhere"), grainNeed: 9999) };
            var result = Validator.Validate(orders, Context(grainAvail: 100, reachable: new[] { R1 }, authorized: Array.Empty<string>()), Config());

            Assert.That(result.HasError(PlanErrorCode.Unreachable), Is.True);
            Assert.That(result.HasError(PlanErrorCode.NoAuthority), Is.True);
            Assert.That(result.HasError(PlanErrorCode.ResourceShortage), Is.True);
            Assert.That(result.Errors.Count, Is.GreaterThanOrEqualTo(3), "一次列全部硬冲突。");
        }

        // ---- 确定性 ----

        [Test]
        public void test_validation_is_deterministic()
        {
            var orders = new[] { Order("b", grainNeed: 600, deps: "a"), Order("a") };
            var ctx = Context(grainAvail: 500, authorized: new[] { "a", "b" });

            var r1 = Validator.Validate(orders, ctx, Config());
            var r2 = Validator.Validate(orders, ctx, Config());

            Assert.That(r2.Errors.Select(e => e.Code), Is.EqualTo(r1.Errors.Select(e => e.Code)));
        }
    }
}
