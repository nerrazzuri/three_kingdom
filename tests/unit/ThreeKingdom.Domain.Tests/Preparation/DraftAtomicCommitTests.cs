using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Preparation;

namespace ThreeKingdom.Domain.Tests.Preparation
{
    /// <summary>
    /// epic-006 story-001：PlanDraft 零副作用与原子提交。
    /// 治理 ADR：ADR-0002（草稿在 App 侧、提交经 Command 原子写 Domain）。GDD_009 / TR-prep-001。
    /// 覆盖 AC-1 草稿零副作用（哈希不变）、AC-2 原子提交、AC-3 失败零部分写入+稳定错误码、
    /// AC-4 占用同步反映到资源。
    /// </summary>
    [TestFixture]
    public class DraftAtomicCommitTests
    {
        private static readonly PlanCommitService Commit = new PlanCommitService();
        private static readonly RegionId R1 = new RegionId("region-fanshui");
        private static readonly ResourceKey Grain = new ResourceKey("grain");

        private static PreparedOrder Order(string id, long grainNeed, int start = 0, int end = 2)
            => new PreparedOrder(
                new OrderId(id), new CharacterId("char-a"), R1, new TimeWindow(start, end),
                grainNeed > 0 ? new Dictionary<ResourceKey, long> { [Grain] = grainNeed } : null);

        private static ResourcePool Pool(long grain) => new ResourcePool(new Dictionary<ResourceKey, long> { [Grain] = grain });

        private static PreparationConfig Config(long margin = 0) => new PreparationConfig(margin);

        private SubmitPlanResult Submit(PlanDraft draft, ResourcePool pool, params string[] authorized)
            => Commit.Submit(draft, pool, new[] { R1 }, AsOrderIds(authorized), Config());

        private static IEnumerable<OrderId> AsOrderIds(string[] ids)
        {
            foreach (var s in ids) yield return new OrderId(s);
        }

        // ---- AC-1: 草稿零副作用 ----

        [Test]
        public void test_draft_edits_do_not_change_authoritative_state_hash()
        {
            var pool = Pool(1000);
            var hashBefore = pool.Hash();

            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 300));
            draft.AddOrder(Order("o2", 200, start: 2, end: 4));
            draft.RemoveOrder(new OrderId("o1"));
            draft.AddOrder(Order("o3", 100, start: 4, end: 6));

            // 大量草稿操作后：权威资源池哈希不变（草稿零副作用）
            Assert.That(pool.Hash(), Is.EqualTo(hashBefore));
        }

        // ---- AC-2 + AC-4: 原子提交，占用反映到资源 ----

        [Test]
        public void test_valid_plan_commits_atomically_and_deducts_resources()
        {
            var pool = Pool(1000);
            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 300));
            draft.AddOrder(Order("o2", 200, start: 2, end: 4));

            var result = Submit(draft, pool, "o1", "o2");

            Assert.That(result.Committed, Is.True);
            Assert.That(result.Plan, Is.Not.Null);
            Assert.That(result.Plan!.Orders.Count, Is.EqualTo(2));
            // 占用同步反映：新池扣减 500
            Assert.That(result.ResultingPool.Get(Grain), Is.EqualTo(500));
            Assert.That(result.Plan.CommittedResources[Grain], Is.EqualTo(500));
            // 原池不被就地修改
            Assert.That(pool.Get(Grain), Is.EqualTo(1000));
        }

        // ---- AC-3: 失败零部分写入 + 稳定错误码 ----

        [Test]
        public void test_plan_with_hard_conflict_does_not_write_partially()
        {
            var pool = Pool(500);
            var hashBefore = pool.Hash();
            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 300));
            draft.AddOrder(Order("o2", 400, start: 2, end: 4)); // 合计 700 > 500 → 资源不足

            var result = Submit(draft, pool, "o1", "o2");

            Assert.That(result.Committed, Is.False);
            Assert.That(result.Plan, Is.Null);
            Assert.That(result.Validation.HasError(PlanErrorCode.ResourceShortage), Is.True, "返回稳定错误码。");
            // 零部分写入：资源池哈希与量均不变
            Assert.That(result.ResultingPool.Get(Grain), Is.EqualTo(500));
            Assert.That(result.ResultingPool.Hash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_unauthorized_order_blocks_commit_with_stable_code()
        {
            var pool = Pool(1000);
            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 100));

            var result = Submit(draft, pool /* o1 未授权 */);

            Assert.That(result.Committed, Is.False);
            Assert.That(result.Validation.HasError(PlanErrorCode.NoAuthority), Is.True);
            Assert.That(result.ResultingPool.Get(Grain), Is.EqualTo(1000), "拒绝不扣资源。");
        }

        // ---- 边界：资源恰好够 → 可提交，列风险 ----

        [Test]
        public void test_boundary_exact_resource_commits_with_risk_flag()
        {
            var pool = Pool(500);
            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 500));

            var result = Submit(draft, pool, "o1");

            Assert.That(result.Committed, Is.True, "恰好够可提交（非硬冲突）。");
            Assert.That(result.ResultingPool.Get(Grain), Is.EqualTo(0));
            Assert.That(result.Validation.Risks, Is.Not.Empty, "恰好够列软风险。");
        }

        [Test]
        public void test_draft_rejects_duplicate_order_id()
        {
            var draft = new PlanDraft();
            draft.AddOrder(Order("o1", 100));
            Assert.Throws<InvalidOperationException>(() => draft.AddOrder(Order("o1", 50)));
        }
    }
}
