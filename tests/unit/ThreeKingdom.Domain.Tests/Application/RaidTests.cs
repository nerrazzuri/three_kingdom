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
    /// EPIC_010 竖切续（断粮疲敌第二取胜路线）：袭扰敌补给——派出→在途→见效（BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0004（确定性随机流）；mvp-scope「至少两种取胜路线」+「非即时」。
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
        private static int RaidLead => Scenario.RaidLeadSegments;

        [Test]
        public void test_dispatch_raid_costs_stock_immediately()
        {
            var service = new SessionService();
            var session = service.NewGame();
            long before = service.ProjectCity(session).Stock;

            service.DispatchRaid(session); // 派出即兑付代价

            Assert.That(service.ProjectCity(session).Stock, Is.EqualTo(before - Scenario.RaidStockCost));
        }

        [Test]
        public void test_raid_is_not_instant_effect_arrives_after_travel()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var afterDispatch = service.DispatchRaid(session);
            Assert.That(afterDispatch.InFlight, Is.True, "派出后袭扰队在途。");
            Assert.That(afterDispatch.HasResult, Is.False, "见效非即时——派出当时无结果。");

            service.Advance(session, RaidLead); // 推进至见效
            var afterArrival = service.ProjectRaid(session);
            Assert.That(afterArrival.InFlight, Is.False);
            Assert.That(afterArrival.HasResult, Is.True, "行军时延后袭扰见效。");
        }

        [Test]
        public void test_only_one_raid_in_flight_at_a_time()
        {
            var service = new SessionService();
            var session = service.NewGame();

            service.DispatchRaid(session);
            var second = service.DispatchRaid(session); // 在途期间不可再派

            Assert.That(second.InFlight, Is.True);
            Assert.That(second.CanDispatch, Is.False, "在途期间不可重复派出袭扰。");
        }

        [Test]
        public void test_raid_available_again_after_resolution()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.DispatchRaid(session);
            service.Advance(session, RaidLead); // 见效

            Assert.That(service.ProjectRaid(session).CanDispatch, Is.True, "见效后可再派出。");
        }

        [Test]
        public void test_raid_outcome_is_deterministic()
        {
            string Run()
            {
                var service = new SessionService();
                var s = service.NewGame();
                service.DispatchRaid(s);
                service.Advance(s, RaidLead);
                var r = service.ProjectRaid(s);
                return r.HasResult + ":" + r.LastExposed;
            }
            Assert.That(Run(), Is.EqualTo(Run()), "同种子同序 → 同见效判定（ADR-0004）。");
        }

        [Test]
        public void test_sustained_raiding_wears_enemy_down_to_withdrawal_victory()
        {
            // 断粮疲敌：反复「派出袭扰 + 推进至见效」压敌至退兵阈值，早于援军日取胜。
            var service = new SessionService();
            var session = service.NewGame();

            GameOutcome outcome = GameOutcome.Ongoing;
            for (int i = 0; i < Scenario.ReliefDay && outcome == GameOutcome.Ongoing; i++)
            {
                service.DispatchRaid(session);
                service.Advance(session, RaidLead); // 推进至本次袭扰见效（约一日）
                outcome = service.ProjectObjective(session).Outcome;
            }

            Assert.That(outcome, Is.EqualTo(GameOutcome.Victory));
            var view = new ObjectiveView(service.ProjectObjective(session));
            Assert.That(view.BannerLabel, Does.Contain("退兵"), "应由断粮疲敌取胜（敌退兵）。");
        }

        [Test]
        public void test_raid_projection_does_not_leak_enemy_truth_strength()
        {
            var props = typeof(RaidProjection).GetProperties();
            foreach (var p in props)
                Assert.That(p.Name.ToLowerInvariant(), Does.Not.Contain("strength"),
                    "袭扰投影不得暴露敌真实兵力字段（P10）。");
        }

        [Test]
        public void test_save_round_trips_inflight_raid_and_rng()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            service.DispatchRaid(session); // 派出后立刻存档（袭扰队在途 + 随机流位置）

            coordinator.Save("campaign", session);
            var restored = coordinator.Load("campaign").Session!;

            Assert.That(service.ProjectRaid(restored).InFlight, Is.True, "读档应恢复在途袭扰。");

            // 两会话同步推进至见效，城池/结果一致（证明在途见效时刻 + 随机流位置已持久）。
            service.Advance(session, RaidLead);
            service.Advance(restored, RaidLead);
            Assert.That(service.ProjectCity(restored).Stock, Is.EqualTo(service.ProjectCity(session).Stock));
            Assert.That(service.ProjectRaid(restored).LastExposed, Is.EqualTo(service.ProjectRaid(session).LastExposed));
        }
    }
}
