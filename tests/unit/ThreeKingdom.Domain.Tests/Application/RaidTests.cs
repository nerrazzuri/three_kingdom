using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续（断粮疲敌第二取胜路线）：袭扰敌补给（BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0004（确定性随机流）；mvp-scope「至少两种取胜路线」。
    /// </summary>
    [TestFixture]
    public class RaidTests
    {
        private sealed class MemMedium : ISaveMedium
        {
            private readonly Dictionary<string, string> _d = new Dictionary<string, string>(StringComparer.Ordinal);
            public bool Exists(string n) => _d.ContainsKey(n);
            public string? Read(string n) => _d.TryGetValue(n, out var v) ? v : null;
            public void Write(string n, string c) => _d[n] = c;
            public void Move(string f, string t) { _d[t] = _d[f]; _d.Remove(f); }
            public void Delete(string n) => _d.Remove(n);
        }

        private static readonly SliceScenario Scenario = SliceScenario.Default();

        [Test]
        public void test_raid_costs_stock()
        {
            var service = new SessionService();
            var session = service.NewGame();
            long before = service.ProjectCity(session).Stock;

            service.Raid(session);

            Assert.That(service.ProjectCity(session).Stock, Is.EqualTo(before - Scenario.RaidStockCost));
        }

        [Test]
        public void test_only_one_raid_per_world_day()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var first = service.Raid(session);
            var second = service.Raid(session); // 同日再袭无效

            Assert.That(first.LastPerformed, Is.True);
            Assert.That(second.LastPerformed, Is.False, "一日一袭：同日第二次不执行。");
            Assert.That(second.CanRaid, Is.False);
        }

        [Test]
        public void test_raid_available_again_after_advancing_a_day()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.Raid(session);

            service.Advance(session, WorldTime.SegmentsPerDay); // 跨日

            Assert.That(service.ProjectRaid(session).CanRaid, Is.True, "推进日界后可再袭。");
        }

        [Test]
        public void test_raid_outcome_is_deterministic()
        {
            var service = new SessionService();
            var a = service.Raid(service.NewGame());
            var b = service.Raid(service.NewGame());

            Assert.That(b.LastExposed, Is.EqualTo(a.LastExposed), "同种子同序 → 同暴露判定（ADR-0004）。");
        }

        [Test]
        public void test_sustained_raiding_wears_enemy_down_to_withdrawal_victory()
        {
            // 断粮疲敌第二取胜路线：反复袭扰（每日一次，跨日再袭）压敌至退兵阈值。
            var service = new SessionService();
            var session = service.NewGame();

            GameOutcome outcome = GameOutcome.Ongoing;
            for (int day = 0; day < Scenario.ReliefDay && outcome == GameOutcome.Ongoing; day++)
            {
                service.Raid(session);
                outcome = service.ProjectObjective(session).Outcome;
                if (outcome != GameOutcome.Ongoing) break;
                service.Advance(session, WorldTime.SegmentsPerDay);
                outcome = service.ProjectObjective(session).Outcome;
            }

            // 在援军日之前应由断粮达成胜利（敌退兵），证明存在第二条不同代价的取胜路线。
            Assert.That(outcome, Is.EqualTo(GameOutcome.Victory));
            var view = new ObjectiveView(service.ProjectObjective(session));
            Assert.That(view.BannerLabel, Does.Contain("退兵"));
        }

        [Test]
        public void test_raid_does_not_leak_enemy_truth_strength()
        {
            // 袭扰投影不含敌真实兵力字段（P10）——削减效果只能经侦察得知。
            var service = new SessionService();
            var raid = service.Raid(service.NewGame());

            var props = typeof(RaidProjection).GetProperties();
            foreach (var p in props)
                Assert.That(p.Name.ToLowerInvariant(), Does.Not.Contain("strength"),
                    "袭扰投影不得暴露敌真实兵力字段。");
            Assert.That(raid, Is.Not.Null);
        }

        [Test]
        public void test_save_round_trips_raid_state_and_rng()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            service.Raid(session);                 // 留下袭扰状态（一日一袭日 + 随机流位置）
            service.Advance(session, WorldTime.SegmentsPerDay);

            coordinator.Save("campaign", session);
            var restored = coordinator.Load("campaign").Session!;

            // 读档后两会话各再袭一次，敌情/城池一致（证明袭扰随机流位置 + 状态已持久）。
            service.Raid(session);
            service.Raid(restored);
            Assert.That(service.ProjectCity(restored).Stock, Is.EqualTo(service.ProjectCity(session).Stock));
            Assert.That(service.ProjectRaid(restored).LastExposed, Is.EqualTo(service.ProjectRaid(session).LastExposed));
        }
    }
}
