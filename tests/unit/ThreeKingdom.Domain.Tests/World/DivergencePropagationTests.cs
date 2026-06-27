using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-012 story-003：分叉传播（下游按 EventId 稳定序重评估）。
    /// 治理 ADR：ADR-0007 §3（稳定序传播）+ ADR-0004（确定性）。GDD_015 / TR-world-002。
    /// 覆盖 AC-1/2 稳定序重评估、AC-3 脱稿深度 + 够不着不脱稿、AC-4 链式有界确定性（含环安全终止）。
    /// </summary>
    [TestFixture]
    public class DivergencePropagationTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Sun = new FactionId("faction-sun");
        private static readonly FactionId Far = new FactionId("faction-far");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");

        private static readonly EventId A = new EventId("evt-a");
        private static readonly EventId B = new EventId("evt-b");
        private static readonly EventId C = new EventId("evt-c");
        private static readonly EventId D = new EventId("evt-d");

        private static readonly DivergencePropagationService Service = new DivergencePropagationService();
        private static readonly HistoryAdvancer Advancer = new HistoryAdvancer();

        // 玩家已灭孙权（前置 FactionAlive(Sun) 被破坏），且玩家圈触及孙权。
        private static WorldState SunDeadWorld()
            => new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>()),
                    new FactionRecord(Sun, null, SurvivalStatus.Destroyed, RelationToPlayer.Neutral, Array.Empty<CityId>()),
                },
                Array.Empty<CityOwnership>(), Array.Empty<string>(), Array.Empty<string>());

        private static PlayerReach ReachSun() => new PlayerReach(new[] { Sun }, Array.Empty<CityId>());

        private static HistoricalEvent Event(EventId id, FactionId subject, params EventId[] downstream)
            => new HistoricalEvent(
                id,
                new TimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(9, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(subject) },
                new HistoricalOutcome($"{id}-normal"),
                new HistoricalOutcome($"{id}-divergence"),
                downstream);

        private static HistoricalEventCatalog Catalog(params HistoricalEvent[] events)
        {
            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(events);
            Assert.That(r.IsSuccess, Is.True, string.Join("; ", AsStrings(r)));
            return r.Value;
        }

        private static IEnumerable<string> AsStrings(Result<HistoricalEventCatalog> r)
        {
            foreach (ConfigError e in r.Errors) yield return e.ToString();
        }

        // 先用推进器把源事件 A 分叉，再传播其下游。
        private static (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) DivergeA(params HistoricalEvent[] events)
        {
            HistoricalEventCatalog cat = Catalog(events);
            HistoryAdvanceResult adv = Advancer.OnTimeWindowEnter(cat, A, SunDeadWorld(), ReachSun());
            Assert.That(adv.Diverged, Is.True);
            return (cat, adv.World, cat.Find(A)!);
        }

        // ---- AC-1 / AC-2：下游稳定序重评估 ----

        [Test]
        public void test_downstream_reevaluated_in_stable_eventid_order()
        {
            // A 的下游以乱序 [C, B] 给出，应按 EventId 稳定序 [B, C] 处理。
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                DivergeA(Event(A, Sun, C, B), Event(B, Sun), Event(C, Sun));

            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), DivergencePropagationConfig.Default);

            Assert.That(r.DivergedDownstream, Is.EqualTo(new[] { B, C }));
            Assert.That(r.World.IsDiverged(B.Value), Is.True);
            Assert.That(r.World.IsDiverged(C.Value), Is.True);
        }

        [Test]
        public void test_no_downstream_is_noop()
        {
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) = DivergeA(Event(A, Sun));
            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), DivergencePropagationConfig.Default);

            Assert.That(r.DivergedDownstream, Is.Empty);
            Assert.That(r.World.ComputeHash(), Is.EqualTo(world.ComputeHash()));
        }

        [Test]
        public void test_reevaluation_is_deterministic()
        {
            DivergencePropagationResult Run()
            {
                (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                    DivergeA(Event(A, Sun, B, C), Event(B, Sun), Event(C, Sun));
                return Service.Propagate(cat, origin, world, ReachSun(), DivergencePropagationConfig.Default);
            }
            Assert.That(Run().World.ComputeHash(), Is.EqualTo(Run().World.ComputeHash()));
        }

        // ---- AC-3：脱稿深度 + 够不着不脱稿 ----

        [Test]
        public void test_depth_zero_only_direct_downstream()
        {
            // A→B→D。depth=0：仅直接下游 B 分叉，D（B 的下游）不波及。
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                DivergeA(Event(A, Sun, B), Event(B, Sun, D), Event(D, Sun));

            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), new DivergencePropagationConfig(0));

            Assert.That(r.DivergedDownstream, Is.EqualTo(new[] { B }));
            Assert.That(r.World.IsDiverged(D.Value), Is.False);
        }

        [Test]
        public void test_depth_one_ripples_one_more_hop()
        {
            // A→B→D。depth=1：B 分叉后再扩一跳到 D。
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                DivergeA(Event(A, Sun, B), Event(B, Sun, D), Event(D, Sun));

            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), new DivergencePropagationConfig(1));

            Assert.That(r.DivergedDownstream, Has.Member(B));
            Assert.That(r.DivergedDownstream, Has.Member(D));
            Assert.That(r.World.IsDiverged(D.Value), Is.True);
        }

        [Test]
        public void test_unreachable_downstream_stays_on_rails()
        {
            // A→B，但 B 的前置主体是够不着的远方势力 Far → B 不脱稿（即便前置被破坏）。
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                DivergeA(Event(A, Sun, B), Event(B, Far));

            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), new DivergencePropagationConfig(2));

            Assert.That(r.DivergedDownstream, Is.Empty);
            Assert.That(r.World.IsDiverged(B.Value), Is.False);
        }

        // ---- AC-4：链式有界确定性（含环安全终止）----

        [Test]
        public void test_chain_propagation_is_bounded_and_deterministic()
        {
            // A→B→C→D 链，depth 足够 → 全链分叉，且两次运行哈希一致。
            HistoricalEvent[] Events() => new[]
            {
                Event(A, Sun, B), Event(B, Sun, C), Event(C, Sun, D), Event(D, Sun),
            };
            DivergencePropagationResult Run()
            {
                (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) = DivergeA(Events());
                return Service.Propagate(cat, origin, world, ReachSun(), new DivergencePropagationConfig(5));
            }

            DivergencePropagationResult r = Run();
            Assert.That(r.DivergedDownstream, Has.Count.EqualTo(3)); // B, C, D
            Assert.That(r.World.ComputeHash(), Is.EqualTo(Run().World.ComputeHash()));
        }

        [Test]
        public void test_cyclic_downstream_terminates_safely()
        {
            // A→B→A（环）。已处理集合保证终止，不无限递归。
            (HistoricalEventCatalog cat, WorldState world, HistoricalEvent origin) =
                DivergeA(Event(A, Sun, B), Event(B, Sun, A));

            DivergencePropagationResult r = Service.Propagate(cat, origin, world, ReachSun(), new DivergencePropagationConfig(10));

            Assert.That(r.DivergedDownstream, Is.EqualTo(new[] { B })); // A 已在 visited，不重复
        }
    }
}
