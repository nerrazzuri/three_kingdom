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
    /// EPIC_010 竖切续（假退伏击，第三取胜路线）：设伏诱敌→在途→发动（BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0004（确定性随机流）；GDD_010；mvp-scope 三条核心条件链之一。
    /// </summary>
    [TestFixture]
    public class AmbushTests
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
        private static int AmbushLead => Scenario.AmbushLeadSegments;

        [Test]
        public void test_dispatch_ambush_lowers_fortification_immediately()
        {
            var service = new SessionService();
            var session = service.NewGame();
            int beforeFort = service.ProjectCity(session).Fortification;

            service.DispatchAmbush(session); // 示弱开口，即降工事

            Assert.That(service.ProjectCity(session).Fortification, Is.EqualTo(beforeFort - Scenario.AmbushFortCost));
        }

        [Test]
        public void test_ambush_is_not_instant_and_one_shot()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var afterDispatch = service.DispatchAmbush(session);
            Assert.That(afterDispatch.InFlight, Is.True, "设伏后伏兵在途。");
            Assert.That(afterDispatch.Resolved, Is.False, "非即时——发动需时。");

            // 在途/已用期间不可再设伏（一局一次）。
            service.Advance(session, AmbushLead);
            Assert.That(service.ProjectAmbush(session).CanDispatch, Is.False, "一局一次，已用过不可再设伏。");
        }

        [Test]
        public void test_early_ambush_routs_enemy_for_victory()
        {
            // 早发动（敌未壮大）：得手则一举击溃 → 假退伏击取胜（第三取胜路线）。
            var service = new SessionService();
            var session = service.NewGame();

            service.DispatchAmbush(session);
            service.Advance(session, AmbushLead); // 推进至发动

            var ambush = service.ProjectAmbush(session);
            Assert.That(ambush.Resolved, Is.True);
            // 基线种子下守将统御足以诱敌成功；得手即重创敌军至退兵阈值。
            Assert.That(ambush.Succeeded, Is.True, "基线场景（敌将性烈 + 守将统御高）伏击应得手。");
            var obj = service.ProjectObjective(session);
            Assert.That(obj.Outcome, Is.EqualTo(GameOutcome.Victory));
            Assert.That(new ObjectiveView(obj).BannerLabel, Does.Contain("伏击"), "胜利横幅应标明假退伏击。");
        }

        [Test]
        public void test_ambush_outcome_is_deterministic()
        {
            string Run()
            {
                var service = new SessionService();
                var s = service.NewGame();
                service.DispatchAmbush(s);
                service.Advance(s, AmbushLead);
                var a = service.ProjectAmbush(s);
                return a.Resolved + ":" + a.Succeeded;
            }
            Assert.That(Run(), Is.EqualTo(Run()), "同种子同序 → 同伏击判定（ADR-0004）。");
        }

        [Test]
        public void test_ambush_projection_does_not_leak_enemy_truth_strength()
        {
            var props = typeof(AmbushProjection).GetProperties();
            foreach (var p in props)
                Assert.That(p.Name.ToLowerInvariant(), Does.Not.Contain("strength"),
                    "伏击投影不得暴露敌真实兵力字段（P10）。");
        }

        [Test]
        public void test_save_round_trips_inflight_ambush()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            service.DispatchAmbush(session); // 设伏后立刻存档（伏兵在途 + 随机流位置）

            coordinator.Save("campaign", session);
            var restored = coordinator.Load("campaign").Session!;
            Assert.That(service.ProjectAmbush(restored).InFlight, Is.True, "读档应恢复在途伏击。");

            // 两会话同步推进至发动，结果一致（在途发动时刻 + 随机流位置已持久）。
            service.Advance(session, AmbushLead);
            service.Advance(restored, AmbushLead);
            Assert.That(service.ProjectAmbush(restored).Succeeded, Is.EqualTo(service.ProjectAmbush(session).Succeeded));
            Assert.That(service.ProjectCity(restored).CivMorale, Is.EqualTo(service.ProjectCity(session).CivMorale));
        }
    }
}
