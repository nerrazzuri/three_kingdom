using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Time
{
    /// <summary>
    /// epic-002 story-002：嵌套战斗时段预算与跨时段触发。
    /// 治理 ADR：ADR-0004（确定性、整数、禁 float 计时）+ ADR-0003（预算配置驱动）。GDD_001 / TR-time-002/003。
    /// 覆盖 AC-1 配置预算消耗 + 耗尽确定性跨段并触发天气/补给/疲劳结算、AC-2/3 行动耗时/期限/取消数据模型可序列化。
    /// </summary>
    [TestFixture]
    public class BattlePhaseBudgetTests
    {
        private static WorldClock NewWorldClock(int day = 0, DaySegment seg = DaySegment.Dawn)
            => new WorldClock(new WorldTime(day, seg));

        // ---- AC-1：时段预算消耗与跨段 ----

        [Test]
        public void Consume_exact_budget_advances_one_segment()
        {
            var clock = NewWorldClock();
            var battle = new BattleClock(clock, phaseBudget: 3);

            var result = battle.ConsumePhases(3);

            Assert.That(result.SegmentsAdvanced, Is.EqualTo(1));
            Assert.That(result.RemainingPhasesInSegment, Is.EqualTo(0));
            Assert.That(battle.Current, Is.EqualTo(new WorldTime(0, DaySegment.Day)));
        }

        [Test]
        public void Consume_below_budget_does_not_cross_and_carries_remainder()
        {
            var battle = new BattleClock(NewWorldClock(), phaseBudget: 3);

            var first = battle.ConsumePhases(2);
            Assert.That(first.SegmentsAdvanced, Is.EqualTo(0));
            Assert.That(first.RemainingPhasesInSegment, Is.EqualTo(2));

            var second = battle.ConsumePhases(2); // 2+2=4 → 1 段，余 1
            Assert.That(second.SegmentsAdvanced, Is.EqualTo(1));
            Assert.That(second.RemainingPhasesInSegment, Is.EqualTo(1));
        }

        [Test]
        public void Consume_over_budget_crosses_multiple_segments()
        {
            // GDD 示例：PHASE_BUDGET=3，7 阶段 → floor(7/3)=2 段
            var battle = new BattleClock(NewWorldClock(), phaseBudget: 3);

            var result = battle.ConsumePhases(7);

            Assert.That(result.SegmentsAdvanced, Is.EqualTo(2));
            Assert.That(result.RemainingPhasesInSegment, Is.EqualTo(1));
            Assert.That(result.Crossings.Count, Is.EqualTo(2));
        }

        [Test]
        public void Crossing_triggers_settlement_in_weather_supply_fatigue_order()
        {
            var battle = new BattleClock(NewWorldClock(), phaseBudget: 1);

            var result = battle.ConsumePhases(1);

            Assert.That(result.Crossings[0].SegmentStages, Is.EqualTo(new[]
            {
                SegmentSettlementStage.Weather,
                SegmentSettlementStage.Supply,
                SegmentSettlementStage.Fatigue,
            }));
        }

        [Test]
        public void Segment_crossing_that_also_crosses_day_carries_day_boundary()
        {
            // 起于 D0 Night(T=3)，预算 1，消耗 1 → 跨入 D1 Dawn：既跨段又跨日（复用 Story 001 日界编排）
            var battle = new BattleClock(NewWorldClock(0, DaySegment.Night), phaseBudget: 1);

            var result = battle.ConsumePhases(1);

            Assert.That(battle.Current, Is.EqualTo(new WorldTime(1, DaySegment.Dawn)));
            Assert.That(result.Crossings[0].DayBoundaries.Select(b => b.Day), Is.EqualTo(new[] { 1 }));
            Assert.That(result.Crossings[0].DayBoundaries[0].Stages, Is.EqualTo(DayBoundaryStages.CanonicalOrder));
        }

        [Test]
        public void Budget_must_be_positive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleClock(NewWorldClock(), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleClock(NewWorldClock(), -1));
        }

        [Test]
        public void ConsumePhases_must_be_positive()
        {
            var battle = new BattleClock(NewWorldClock(), 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => battle.ConsumePhases(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => battle.ConsumePhases(-2));
        }

        [Test]
        public void BattleClock_requires_world_clock()
        {
            Assert.Throws<ArgumentNullException>(() => new BattleClock(null!, 3));
        }

        // ---- AC-2/3：行动耗时 / 期限 / 取消数据模型 ----

        [Test]
        public void ActionCost_applies_ceil_of_product()
        {
            // base=2, terrain=1.5, weather=1.3 → ceil(≈3.9) = 4
            int cost = ActionCost.Compute(2, FixedPoint.FromFraction(3, 2), FixedPoint.FromFraction(13, 10));
            Assert.That(cost, Is.EqualTo(4));
        }

        [Test]
        public void ActionCost_is_at_least_one()
        {
            // base=1, terrain=0.5, weather=0.5 → 0.25 → ceil=1（且不低于 1）
            int cost = ActionCost.Compute(1, FixedPoint.FromFraction(1, 2), FixedPoint.FromFraction(1, 2));
            Assert.That(cost, Is.EqualTo(1));
        }

        [Test]
        public void ActionCost_rejects_non_positive_inputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionCost.Compute(0, FixedPoint.FromInt(1), FixedPoint.FromInt(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionCost.Compute(1, FixedPoint.Zero, FixedPoint.FromInt(1)));
        }

        [Test]
        public void TimedAction_end_time_is_start_plus_duration()
        {
            var action = new TimedAction("scout-1", "general-7", new WorldTime(1, DaySegment.Day), 5, ActionStatus.InProgress, true);
            Assert.That(action.EndTime, Is.EqualTo(new WorldTime(1, DaySegment.Day).Advance(5)));
        }

        [Test]
        public void TimedAction_round_trips_through_fields()
        {
            var original = new TimedAction("move-3", "unit-2", new WorldTime(2, DaySegment.Dusk), 3, ActionStatus.Pending, false);

            // 占位 round-trip：经全部公开字段重建（完整 round-trip 在 epic-009 DTO）
            var rebuilt = new TimedAction(original.ActionId, original.ActorId, original.Start,
                original.DurationSegments, original.Status, original.Interruptible);

            Assert.That(rebuilt.ActionId, Is.EqualTo(original.ActionId));
            Assert.That(rebuilt.ActorId, Is.EqualTo(original.ActorId));
            Assert.That(rebuilt.Start, Is.EqualTo(original.Start));
            Assert.That(rebuilt.DurationSegments, Is.EqualTo(original.DurationSegments));
            Assert.That(rebuilt.Status, Is.EqualTo(original.Status));
            Assert.That(rebuilt.Interruptible, Is.EqualTo(original.Interruptible));
        }

        [Test]
        public void TimedAction_with_status_preserves_other_fields()
        {
            var action = new TimedAction("a", "b", new WorldTime(0, DaySegment.Dawn), 2, ActionStatus.InProgress, true);
            var cancelled = action.WithStatus(ActionStatus.Cancelled);

            Assert.That(cancelled.Status, Is.EqualTo(ActionStatus.Cancelled));
            Assert.That(cancelled.ActionId, Is.EqualTo("a"));
            Assert.That(cancelled.DurationSegments, Is.EqualTo(2));
            Assert.That(action.Status, Is.EqualTo(ActionStatus.InProgress), "原对象不可变");
        }

        [Test]
        public void TimedAction_rejects_invalid_fields()
        {
            Assert.Throws<ArgumentException>(() => new TimedAction("", "b", new WorldTime(0, DaySegment.Dawn), 1, ActionStatus.Pending, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimedAction("a", "b", new WorldTime(0, DaySegment.Dawn), 0, ActionStatus.Pending, true));
        }

        [Test]
        public void Deadline_remaining_and_expiry()
        {
            var deadline = new Deadline("siege-1", new WorldTime(5, DaySegment.Dawn), DeadlineConsequence.Fail); // T=20
            var now = new WorldTime(3, DaySegment.Dusk); // T=14

            Assert.That(deadline.RemainingSegments(now), Is.EqualTo(6));
            Assert.That(deadline.IsExpired(now), Is.False);
            Assert.That(deadline.IsExpired(new WorldTime(5, DaySegment.Dawn)), Is.True);   // 恰到期
            Assert.That(deadline.IsExpired(new WorldTime(6, DaySegment.Dawn)), Is.True);   // 逾期
        }

        [Test]
        public void Deadline_rejects_empty_target()
        {
            Assert.Throws<ArgumentException>(() => new Deadline("  ", new WorldTime(0, DaySegment.Dawn), DeadlineConsequence.Penalty));
        }

        [Test]
        public void Cancellation_time_lost_is_ceil_of_elapsed_times_loss()
        {
            // elapsed=4, loss=0.5 → ceil(2.0)=2
            Assert.That(CancellationPolicy.ComputeTimeLost(4, FixedPoint.FromFraction(1, 2)), Is.EqualTo(2));
            // elapsed=3, loss=0.5 → ceil(1.5)=2
            Assert.That(CancellationPolicy.ComputeTimeLost(3, FixedPoint.FromFraction(1, 2)), Is.EqualTo(2));
            // loss=0 → 0
            Assert.That(CancellationPolicy.ComputeTimeLost(4, FixedPoint.Zero), Is.EqualTo(0));
        }

        [Test]
        public void Cancellation_rejects_out_of_range_loss()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CancellationPolicy.ComputeTimeLost(1, FixedPoint.FromInt(2)));
            Assert.Throws<ArgumentOutOfRangeException>(() => CancellationPolicy.ComputeTimeLost(1, FixedPoint.FromInt(-1)));
        }
    }
}
