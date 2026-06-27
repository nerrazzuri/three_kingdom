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
    /// epic-012 story-002：历史事件四元组 + reachability 触发门 + 配置校验。
    /// 治理 ADR：ADR-0007（条件历史）+ ADR-0003（数据驱动配置）。GDD_015 / TR-world-002、005。
    /// 覆盖 AC-2 够不着短路、AC-3/4 够得着 正常/分叉、AC-5 ReachPredicate、AC-6 配置校验、AC-7 确定性。
    /// </summary>
    [TestFixture]
    public class HistoricalEventTriggerTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Sun = new FactionId("faction-sun");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CharacterId SunQuan = new CharacterId("char-sunquan");
        private static readonly EventId Chibi = new EventId("evt-chibi");

        private static readonly HistoryAdvancer Advancer = new HistoryAdvancer();

        private static WorldState World(bool sunAlive)
        {
            var sun = sunAlive
                ? new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>())
                : new FactionRecord(Sun, null, SurvivalStatus.Destroyed, RelationToPlayer.Neutral, Array.Empty<CityId>());
            return new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>()), sun },
                Array.Empty<CityOwnership>(),
                Array.Empty<string>(), Array.Empty<string>());
        }

        private static HistoricalEvent ChibiEvent()
            => new HistoricalEvent(
                Chibi,
                new TimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("historical-chibi"),
                new HistoricalOutcome("sun-fell-early"),
                Array.Empty<EventId>());

        private static HistoricalEventCatalog Catalog()
        {
            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(new[] { ChibiEvent() });
            Assert.That(r.IsSuccess, Is.True);
            return r.Value;
        }

        private static PlayerReach ReachTouchingSun()
            => new PlayerReach(new[] { Sun }, Array.Empty<CityId>());

        // ---- AC-2：够不着 → 正常结局（短路不评估前置）----

        [Test]
        public void test_unreachable_fires_normal_even_when_precondition_would_be_broken()
        {
            // 孙权已灭（前置若评估会破坏），但玩家圈够不着 → 短路走正常历史结局。
            HistoryAdvanceResult r = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(sunAlive: false), PlayerReach.None);

            Assert.That(r.Fired, Is.True);
            Assert.That(r.Diverged, Is.False);
            Assert.That(r.Reason, Is.EqualTo(FireReason.NormalUnreachable));
            Assert.That(r.FiredOutcome!.Label, Is.EqualTo("historical-chibi"));
            Assert.That(r.World.IsTriggered(Chibi.Value), Is.True);
            Assert.That(r.World.IsDiverged(Chibi.Value), Is.False);
        }

        // ---- AC-3：够得着且前置成立 → 正常 ----

        [Test]
        public void test_reachable_with_preconditions_held_fires_normal()
        {
            HistoryAdvanceResult r = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(sunAlive: true), ReachTouchingSun());

            Assert.That(r.Reason, Is.EqualTo(FireReason.NormalPreconditionsHeld));
            Assert.That(r.Diverged, Is.False);
            Assert.That(r.FiredOutcome!.Label, Is.EqualTo("historical-chibi"));
        }

        // ---- AC-4：够得着且前置被破坏 → 分叉 ----

        [Test]
        public void test_reachable_with_broken_precondition_diverges()
        {
            HistoryAdvanceResult r = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(sunAlive: false), ReachTouchingSun());

            Assert.That(r.Fired, Is.True);
            Assert.That(r.Diverged, Is.True);
            Assert.That(r.Reason, Is.EqualTo(FireReason.Diverged));
            Assert.That(r.FiredOutcome!.Label, Is.EqualTo("sun-fell-early"));
            Assert.That(r.World.IsTriggered(Chibi.Value), Is.True);
            Assert.That(r.World.IsDiverged(Chibi.Value), Is.True);
        }

        // ---- AC-5：ReachPredicate 判定触及主体 ----

        [Test]
        public void test_reach_predicate_detects_subject_touch()
        {
            var predicate = new SubjectReachPredicate();
            HistoricalEvent e = ChibiEvent();
            Assert.That(predicate.Reachable(e, ReachTouchingSun()), Is.True);
            Assert.That(predicate.Reachable(e, PlayerReach.None), Is.False);
        }

        // ---- 幂等 ----

        [Test]
        public void test_already_triggered_event_is_idempotent()
        {
            HistoryAdvanceResult first = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(true), ReachTouchingSun());
            HistoryAdvanceResult second = Advancer.OnTimeWindowEnter(Catalog(), Chibi, first.World, ReachTouchingSun());

            Assert.That(second.Fired, Is.False);
            Assert.That(second.Reason, Is.EqualTo(FireReason.AlreadyTriggered));
            Assert.That(second.World.ComputeHash(), Is.EqualTo(first.World.ComputeHash()));
        }

        // ---- AC-6：配置校验 ----

        [Test]
        public void test_config_rejects_event_missing_divergence_branch()
        {
            var bad = new HistoricalEvent(
                Chibi, new TimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("only-normal"),
                divergenceOutcome: null,
                Array.Empty<EventId>());

            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(new[] { bad });
            Assert.That(r.IsSuccess, Is.False);
            Assert.That(r.Errors, Has.Some.Matches<ConfigError>(
                e => e.Code == ConfigErrorCode.MissingRequiredField && e.Field == "DivergenceOutcome"));
        }

        [Test]
        public void test_config_rejects_event_missing_preconditions()
        {
            var bad = new HistoricalEvent(
                Chibi, new TimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                Array.Empty<Precondition>(),
                new HistoricalOutcome("normal"), new HistoricalOutcome("divergence"),
                Array.Empty<EventId>());

            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(new[] { bad });
            Assert.That(r.IsSuccess, Is.False);
            Assert.That(r.Errors, Has.Some.Matches<ConfigError>(
                e => e.Code == ConfigErrorCode.MissingRequiredField && e.Field == "Preconds"));
        }

        [Test]
        public void test_config_rejects_duplicate_event_id()
        {
            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(new[] { ChibiEvent(), ChibiEvent() });
            Assert.That(r.IsSuccess, Is.False);
            Assert.That(r.Errors, Has.Some.Matches<ConfigError>(e => e.Code == ConfigErrorCode.DuplicateStableId));
        }

        [Test]
        public void test_config_rejects_unknown_downstream_reference()
        {
            var ev = new HistoricalEvent(
                Chibi, new TimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("normal"), new HistoricalOutcome("divergence"),
                new[] { new EventId("evt-nonexistent") });

            Result<HistoricalEventCatalog> r = HistoricalEventCatalog.TryCreate(new[] { ev });
            Assert.That(r.IsSuccess, Is.False);
            Assert.That(r.Errors, Has.Some.Matches<ConfigError>(e => e.Code == ConfigErrorCode.MissingReference));
        }

        // ---- AC-7：确定性 ----

        [Test]
        public void test_same_input_yields_same_history_and_hash()
        {
            HistoryAdvanceResult a = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(false), ReachTouchingSun());
            HistoryAdvanceResult b = Advancer.OnTimeWindowEnter(Catalog(), Chibi, World(false), ReachTouchingSun());

            Assert.That(a.Diverged, Is.EqualTo(b.Diverged));
            Assert.That(a.World.ComputeHash(), Is.EqualTo(b.World.ComputeHash()));
        }
    }
}
