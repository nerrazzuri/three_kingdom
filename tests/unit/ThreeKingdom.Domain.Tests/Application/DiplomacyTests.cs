using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续（Phase B6）：外交求粮受控入口（BLOCKING）。
    /// 治理 ADR：ADR-0002（用例编排）+ ADR-0004（确定性随机流）+ GDD_012 §8（延迟交付/可背约/代价不返还）。
    /// </summary>
    [TestFixture]
    public class DiplomacyTests
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
        public void test_request_aid_baseline_is_accepted_and_one_shot()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var first = service.RequestAid(session);
            var second = service.RequestAid(session); // 受控入口：一局一次，再求无效

            Assert.That(first.Used, Is.True);
            Assert.That(first.Response, Is.EqualTo(DiplomaticResponse.Accepted), "基线 grant_score≈0.72 ≥ 接受阈值 0.6。");
            Assert.That(second.Response, Is.EqualTo(first.Response), "二次求援不改变结果（一局一次）。");
            Assert.That(second.Fulfilled, Is.EqualTo(first.Fulfilled));
        }

        [Test]
        public void test_aid_outcome_is_deterministic()
        {
            var service = new SessionService();
            var a = service.RequestAid(service.NewGame());
            var b = service.RequestAid(service.NewGame());

            Assert.That(b.Fulfilled, Is.EqualTo(a.Fulfilled), "同种子同请求 → 同兑现判定（ADR-0004）。");
            Assert.That(b.PendingArrivalDay, Is.EqualTo(a.PendingArrivalDay));
            Assert.That(b.PendingAmount, Is.EqualTo(a.PendingAmount));
        }

        [Test]
        public void test_fulfilled_aid_delivers_supply_on_arrival()
        {
            var service = new SessionService();
            var control = service.NewGame();
            var aid = service.NewGame();
            var pledge = service.RequestAid(aid); // 求粮（不立即改库存）

            // 两会话同步推进越过交付时段（commit_lead 两日）；其余结算完全一致。
            service.Advance(control, WorldTime.SegmentsPerDay * 3);
            service.Advance(aid, WorldTime.SegmentsPerDay * 3);

            long expectedDelta = pledge.Fulfilled ? Scenario.DiplomacySupplyAmount : 0;
            long actualDelta = service.ProjectCity(aid).Stock - service.ProjectCity(control).Stock;
            Assert.That(actualDelta, Is.EqualTo(expectedDelta), "兑现则到达时入城粮草，否则与无援一致。");
        }

        [Test]
        public void test_save_round_trips_diplomacy_pending_and_rng()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            var before = service.RequestAid(session); // 留下外交状态（含在途交付 + 随机流位置）

            coordinator.Save("campaign", session);
            var restored = coordinator.Load("campaign").Session!;
            var after = service.ProjectDiplomacy(restored);

            Assert.That(after.Used, Is.EqualTo(before.Used));
            Assert.That(after.Response, Is.EqualTo(before.Response));
            Assert.That(after.Fulfilled, Is.EqualTo(before.Fulfilled));
            Assert.That(after.PendingArrivalDay, Is.EqualTo(before.PendingArrivalDay));
            Assert.That(after.PendingAmount, Is.EqualTo(before.PendingAmount));

            // 读档后推进与原会话一致（含援粮交付），证明在途交付 + 随机流位置已持久。
            service.Advance(session, WorldTime.SegmentsPerDay * 3);
            service.Advance(restored, WorldTime.SegmentsPerDay * 3);
            Assert.That(service.ProjectCity(restored).Stock, Is.EqualTo(service.ProjectCity(session).Stock));
        }
    }
}
