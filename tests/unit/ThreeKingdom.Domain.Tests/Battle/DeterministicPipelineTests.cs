using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Battle
{
    /// <summary>
    /// epic-007 story-001：确定性战役解析管线与状态哈希。
    /// 治理 ADR：ADR-0004（整数/定点 + 稳定管线 + 状态哈希）。GDD_010 / TR-battle-001/003。
    /// 覆盖 AC-1 管线固定顺序、AC-2 同输入同哈希（含乱序稳定化/不同种子不同哈希）、
    /// AC-3 阶段原子回滚。
    /// </summary>
    [TestFixture]
    public class DeterministicPipelineTests
    {
        private static readonly BattleResolver Resolver = new BattleResolver();
        private static readonly RegionId R1 = new RegionId("region-fanshui");
        private static readonly RegionId R2 = new RegionId("region-pass");
        private static readonly FactionId Wei = new FactionId("wei");
        private static readonly FactionId Shu = new FactionId("shu");

        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);

        private static BattleUnitState Unit(string id, FactionId faction, RegionId region, int force)
            => new BattleUnitState(new BattleUnitId(id), faction, region, force,
                morale: F(8, 10), fatigue: F(2, 10), discipline: F(9, 10),
                terrainMod: FixedPoint.One, postureMod: FixedPoint.One, support: FixedPoint.Zero);

        private static BattleConfig Config() => new BattleConfig(F(3, 2), F(3, 10));

        private static BattleSnapshot Snapshot(DetectionState? detection = null, params BattleUnitState[] units)
            => new BattleSnapshot(units, detection ?? new DetectionState(), "fp-v1");

        // ---- AC-1: 管线固定顺序 ----

        [Test]
        public void test_pipeline_has_canonical_eight_step_order()
        {
            Assert.That(BattlePhasePipeline.CanonicalOrder, Is.EqualTo(new[]
            {
                BattlePhaseStep.Validate, BattlePhaseStep.Move, BattlePhaseStep.Detect, BattlePhaseStep.Engage,
                BattlePhaseStep.Casualty, BattlePhaseStep.Cohesion, BattlePhaseStep.Trigger, BattlePhaseStep.Publish,
            }));
        }

        // ---- AC-2: 同输入同哈希 ----

        [Test]
        public void test_same_inputs_reproduce_identical_hash_and_events()
        {
            var snap = Snapshot(null, Unit("wei-1", Wei, R1, 1000), Unit("shu-1", Shu, R1, 1000));
            var orders = new[] { new BattleOrder(0, new BattleUnitId("wei-1"), BattleOrderType.Engage, targetUnit: new BattleUnitId("shu-1")) };

            var a = Resolver.ResolvePhase(snap, orders, seed: 42UL, Config());
            var b = Resolver.ResolvePhase(snap, orders, seed: 42UL, Config());

            Assert.That(a.Committed, Is.True);
            Assert.That(b.Hash, Is.EqualTo(a.Hash));
            Assert.That(b.Events.Count, Is.EqualTo(a.Events.Count));
        }

        [Test]
        public void test_shuffled_command_order_yields_same_hash_after_stabilization()
        {
            var snap = Snapshot(null, Unit("wei-1", Wei, R1, 1000), Unit("shu-1", Shu, R2, 1000));
            var o0 = new BattleOrder(0, new BattleUnitId("wei-1"), BattleOrderType.Move, targetRegion: R2);
            var o1 = new BattleOrder(1, new BattleUnitId("shu-1"), BattleOrderType.Hold);

            var forward = Resolver.ResolvePhase(snap, new[] { o0, o1 }, 7UL, Config());
            var reversed = Resolver.ResolvePhase(snap, new[] { o1, o0 }, 7UL, Config());

            Assert.That(reversed.Hash, Is.EqualTo(forward.Hash), "命令流按稳定序号归一后结果一致。");
        }

        [Test]
        public void test_different_seed_yields_different_hash()
        {
            var snap = Snapshot(null, Unit("wei-1", Wei, R1, 1000), Unit("shu-1", Shu, R1, 1000));
            var orders = Array.Empty<BattleOrder>();

            var a = Resolver.ResolvePhase(snap, orders, 1UL, Config());
            var b = Resolver.ResolvePhase(snap, orders, 2UL, Config());

            Assert.That(b.Hash, Is.Not.EqualTo(a.Hash));
        }

        // ---- 交战结算 + 突然性 ----

        [Test]
        public void test_engagement_inflicts_casualties_on_both_sides()
        {
            var snap = Snapshot(null, Unit("wei-1", Wei, R1, 1000), Unit("shu-1", Shu, R1, 1000));
            var result = Resolver.ResolvePhase(snap, Array.Empty<BattleOrder>(), 0UL, Config());

            Assert.That(result.Committed, Is.True);
            Assert.That(result.State.Unit(new BattleUnitId("wei-1")).Force, Is.LessThan(1000));
            Assert.That(result.State.Unit(new BattleUnitId("shu-1")).Force, Is.LessThan(1000));
            Assert.That(result.Events.Any(e => e.Type == BattleEventType.Casualty), Is.True);
        }

        [Test]
        public void test_surprise_increases_casualties_on_unaware_defender()
        {
            var weiUnit = Unit("wei-1", Wei, R1, 1000);
            var shuUnit = Unit("shu-1", Shu, R1, 1000);

            // 突然性：魏确认蜀，蜀未察觉魏 → 魏对蜀有伏击加成
            var detection = new DetectionState();
            detection.Set(Wei, new BattleUnitId("shu-1"), Awareness.Confirmed);
            detection.Set(Shu, new BattleUnitId("wei-1"), Awareness.Unaware);

            var surprised = Resolver.ResolvePhase(Snapshot(detection, weiUnit, shuUnit), Array.Empty<BattleOrder>(), 0UL, Config());
            var symmetric = Resolver.ResolvePhase(Snapshot(null, weiUnit, shuUnit), Array.Empty<BattleOrder>(), 0UL, Config());

            int shuForceSurprised = surprised.State.Unit(new BattleUnitId("shu-1")).Force;
            int shuForceSymmetric = symmetric.State.Unit(new BattleUnitId("shu-1")).Force;
            Assert.That(shuForceSurprised, Is.LessThan(shuForceSymmetric), "被突然袭击的未察觉方损失更大。");
        }

        // ---- AC-3: 阶段原子回滚 ----

        [Test]
        public void test_exception_mid_phase_rolls_back_entire_phase()
        {
            var snap = Snapshot(null, Unit("wei-1", Wei, R1, 1000), Unit("shu-1", Shu, R1, 1000));
            // 先一条有效移动（改工作副本），再一条侦察不存在单位（在侦测步抛异常）
            var orders = new[]
            {
                new BattleOrder(0, new BattleUnitId("wei-1"), BattleOrderType.Move, targetRegion: R2),
                new BattleOrder(1, new BattleUnitId("wei-1"), BattleOrderType.Scout, targetUnit: new BattleUnitId("ghost")),
            };

            var result = Resolver.ResolvePhase(snap, orders, 0UL, Config());

            Assert.That(result.Committed, Is.False);
            Assert.That(result.Error, Is.Not.Null);
            // 原子回滚：原快照未改动——wei-1 仍在 R1，兵力未变
            Assert.That(result.State.Unit(new BattleUnitId("wei-1")).Region, Is.EqualTo(R1));
            Assert.That(result.State.Unit(new BattleUnitId("wei-1")).Force, Is.EqualTo(1000));
        }
    }
}
