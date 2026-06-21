using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Time
{
    /// <summary>
    /// epic-002 story-001：确定性时间推进与时段/日界结算。
    /// 治理 ADR：ADR-0004（确定性：整数、稳定全序、引擎无关，不依赖帧率）。GDD_001 / TR-time-001。
    /// 覆盖 AC-1 WorldTime 权威时间仅由命令前进、AC-2 同点事件 (priority,stableId) 全序、
    /// AC-3 日界顺序 环境→补给→城市→状态事件、AC-4 同输入→同时段变化与事件序列。
    /// </summary>
    [TestFixture]
    public class DeterministicTimeTests
    {
        // ---- AC-1：WorldTime 权威时间 + 仅由命令前进 ----

        [Test]
        public void SegmentsPerDay_is_four_from_enum()
        {
            Assert.That(WorldTime.SegmentsPerDay, Is.EqualTo(4));
        }

        [Test]
        public void AbsoluteIndex_linearizes_day_and_segment()
        {
            // GDD 示例：第 3 日黄昏(s=2) → T = 3×4 + 2 = 14
            Assert.That(new WorldTime(3, DaySegment.Dusk).AbsoluteIndex, Is.EqualTo(14));
        }

        [Test]
        public void Advance_within_day_keeps_day()
        {
            var t = new WorldTime(0, DaySegment.Dawn).Advance(2);
            Assert.That(t, Is.EqualTo(new WorldTime(0, DaySegment.Dusk)));
        }

        [Test]
        public void Advance_across_day_boundary_rolls_over()
        {
            // T=14 + 4 = 18 → d=4, s=2（黄昏）
            var t = new WorldTime(3, DaySegment.Dusk).Advance(4);
            Assert.That(t, Is.EqualTo(new WorldTime(4, DaySegment.Dusk)));
        }

        [Test]
        public void Advance_negative_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldTime(0, DaySegment.Dawn).Advance(-1));
        }

        [Test]
        public void Constructor_rejects_negative_day()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldTime(-1, DaySegment.Dawn));
        }

        [Test]
        public void Comparison_and_equality_follow_absolute_index()
        {
            Assert.That(new WorldTime(1, DaySegment.Night) < new WorldTime(2, DaySegment.Dawn), Is.True);
            Assert.That(new WorldTime(2, DaySegment.Day) == new WorldTime(2, DaySegment.Day), Is.True);
            Assert.That(new WorldTime(2, DaySegment.Day).GetHashCode(),
                Is.EqualTo(new WorldTime(2, DaySegment.Day).GetHashCode()));
        }

        [Test]
        public void WorldClock_only_advances_through_command()
        {
            var clock = new WorldClock(new WorldTime(0, DaySegment.Dawn));
            clock.Apply(new AdvanceTimeCommand(1));
            Assert.That(clock.Current, Is.EqualTo(new WorldTime(0, DaySegment.Day)));
        }

        [Test]
        public void AdvanceTimeCommand_rejects_non_positive_segments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdvanceTimeCommand(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdvanceTimeCommand(-3));
        }

        // ---- AC-2：同点事件全序确定 ----

        [Test]
        public void Resolve_orders_by_time_then_priority_then_stable_id()
        {
            var t = new WorldTime(1, DaySegment.Day);
            var later = new WorldTime(1, DaySegment.Dusk);
            var events = new[]
            {
                new ScheduledEvent(later, 0, "z"),
                new ScheduledEvent(t, 2, "a"),
                new ScheduledEvent(t, 1, "b"),
                new ScheduledEvent(t, 1, "a"), // 同时间同优先级 → 按 stableId 升序，先于 "b"
            };

            var ordered = ScheduledEventOrder.Resolve(events).Select(e => e.StableId).ToArray();

            Assert.That(ordered, Is.EqualTo(new[] { "a", "b", "a", "z" }));
            // 解释：t,p1,a → t,p1,b → t,p2,a → later,p0,z
        }

        [Test]
        public void Resolve_is_independent_of_input_order()
        {
            var t = new WorldTime(0, DaySegment.Dawn);
            var a = new ScheduledEvent(t, 1, "alpha");
            var b = new ScheduledEvent(t, 1, "beta");
            var c = new ScheduledEvent(t, 0, "gamma");

            var r1 = ScheduledEventOrder.Resolve(new[] { a, b, c }).Select(e => e.StableId);
            var r2 = ScheduledEventOrder.Resolve(new[] { c, b, a }).Select(e => e.StableId);

            Assert.That(r1, Is.EqualTo(r2));
            Assert.That(r1, Is.EqualTo(new[] { "gamma", "alpha", "beta" }));
        }

        [Test]
        public void Resolve_duplicate_key_is_rejected()
        {
            var t = new WorldTime(0, DaySegment.Dawn);
            var dup = new[]
            {
                new ScheduledEvent(t, 1, "same"),
                new ScheduledEvent(t, 1, "same"), // 同 (时间,优先级,stableId) → 全序歧义
            };

            Assert.Throws<InvalidOperationException>(() => ScheduledEventOrder.Resolve(dup));
        }

        [Test]
        public void ScheduledEvent_rejects_empty_stable_id()
        {
            Assert.Throws<ArgumentException>(() => new ScheduledEvent(new WorldTime(0, DaySegment.Dawn), 0, "  "));
        }

        // ---- AC-3：日界顺序 环境→补给→城市→状态事件 ----

        [Test]
        public void DayBoundary_canonical_order_is_env_supply_city_state()
        {
            Assert.That(DayBoundaryStages.CanonicalOrder, Is.EqualTo(new[]
            {
                DayBoundaryStage.Environment,
                DayBoundaryStage.Supply,
                DayBoundaryStage.City,
                DayBoundaryStage.StateEvents,
            }));
        }

        [Test]
        public void Advance_within_day_produces_no_day_boundary()
        {
            var clock = new WorldClock(new WorldTime(0, DaySegment.Dawn));
            var result = clock.Apply(new AdvanceTimeCommand(2)); // Dawn→Dusk，同日
            Assert.That(result.DayBoundaries, Is.Empty);
        }

        [Test]
        public void Advance_crossing_one_day_yields_one_ordered_settlement()
        {
            var clock = new WorldClock(new WorldTime(0, DaySegment.Night)); // T=3
            var result = clock.Apply(new AdvanceTimeCommand(1));            // → T=4 = D1 Dawn

            Assert.That(result.DayBoundaries.Count, Is.EqualTo(1));
            Assert.That(result.DayBoundaries[0].Day, Is.EqualTo(1));
            Assert.That(result.DayBoundaries[0].Stages, Is.EqualTo(DayBoundaryStages.CanonicalOrder));
        }

        [Test]
        public void Advance_crossing_multiple_days_yields_settlements_in_time_order()
        {
            var clock = new WorldClock(new WorldTime(0, DaySegment.Dawn)); // T=0
            var result = clock.Apply(new AdvanceTimeCommand(9));           // T=9 → D2 Day，跨 D1、D2

            var days = result.DayBoundaries.Select(b => b.Day).ToArray();
            Assert.That(days, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(clock.Current, Is.EqualTo(new WorldTime(2, DaySegment.Day)));
        }

        // ---- AC-4：同输入序列 → 同结果（可复现）----

        [Test]
        public void Same_command_sequence_yields_identical_state_and_boundaries()
        {
            int[] commands = { 1, 3, 4, 2, 5 };

            (WorldTime, List<int>) Run()
            {
                var clock = new WorldClock(new WorldTime(0, DaySegment.Dawn));
                var boundaries = new List<int>();
                foreach (var n in commands)
                    boundaries.AddRange(clock.Apply(new AdvanceTimeCommand(n)).DayBoundaries.Select(b => b.Day));
                return (clock.Current, boundaries);
            }

            var (time1, b1) = Run();
            var (time2, b2) = Run();

            Assert.That(time2, Is.EqualTo(time1));
            Assert.That(b2, Is.EqualTo(b1));
        }
    }
}
