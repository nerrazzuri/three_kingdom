using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续：会话存档/读档经真实 epic-009 持久栈（BLOCKING）。
    /// 治理 ADR：ADR-0005（原子写 + 先校验后载入）+ TR-save-001/002/003 + TR-intel-003（分段不污染）。
    /// </summary>
    [TestFixture]
    public class SaveCoordinatorTests
    {
        /// <summary>纯内存存档介质（可植入损坏内容）。</summary>
        private sealed class MemMedium : ISaveMedium
        {
            private readonly Dictionary<string, string> _d = new Dictionary<string, string>(StringComparer.Ordinal);
            public bool Exists(string n) => _d.ContainsKey(n);
            public string? Read(string n) => _d.TryGetValue(n, out var v) ? v : null;
            public void Write(string n, string c) => _d[n] = c;
            public void Move(string f, string t) { _d[t] = _d[f]; _d.Remove(f); }
            public void Delete(string n) => _d.Remove(n);
            public void Seed(string n, string c) => _d[n] = c;
        }

        private static readonly SliceScenario Scenario = SliceScenario.Default();

        [Test]
        public void test_save_then_load_round_trips_time_city_and_intel()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            service.Advance(session, WorldTime.SegmentsPerDay * 3); // 推进三日（城市结算 + 敌真值漂移）
            service.DispatchScout(session);                         // 派出侦察
            service.Advance(session, SliceScenario.Default().ScoutLeadSegments); // 返报，留下情报 + 时效基准

            SaveResult saved = coordinator.Save("campaign", session);
            SessionLoadResult loaded = coordinator.Load("campaign");

            Assert.That(saved.Succeeded, Is.True);
            Assert.That(loaded.Succeeded, Is.True);
            var restored = loaded.Session!;

            // 时间
            Assert.That(service.Project(restored).AbsoluteIndex, Is.EqualTo(service.Project(session).AbsoluteIndex));
            // 城市账本
            var c0 = service.ProjectCity(session);
            var c1 = service.ProjectCity(restored);
            Assert.That(c1.Stock, Is.EqualTo(c0.Stock));
            Assert.That(c1.CivMorale, Is.EqualTo(c0.CivMorale));
            Assert.That(c1.Security, Is.EqualTo(c0.Security));
            Assert.That(c1.Fortification, Is.EqualTo(c0.Fortification));
            // 情报（已知估计值 + 观察时间一致）
            service.ProjectIntel(session).TryGet(Scenario.EnemySubject, out var e0);
            service.ProjectIntel(restored).TryGet(Scenario.EnemySubject, out var e1);
            Assert.That(e1.KnownStrength, Is.EqualTo(e0.KnownStrength));
            Assert.That(e1.ObservedAt, Is.EqualTo(e0.ObservedAt));
        }

        [Test]
        public void test_restored_session_continues_deterministically()
        {
            var service = new SessionService();
            var coordinator = new SaveCoordinator(new MemMedium());
            var session = service.NewGame();
            service.Advance(session, WorldTime.SegmentsPerDay * 2);
            coordinator.Save("campaign", session);
            var restored = coordinator.Load("campaign").Session!;

            // 存档后原会话继续推进，与「读档后继续推进」逐项一致（同输入同结果，ADR-0004）。
            service.Advance(session, WorldTime.SegmentsPerDay);
            service.Advance(restored, WorldTime.SegmentsPerDay);

            Assert.That(service.ProjectCity(restored).Stock, Is.EqualTo(service.ProjectCity(session).Stock));
            Assert.That(service.Project(restored).AbsoluteIndex, Is.EqualTo(service.Project(session).AbsoluteIndex));
        }

        [Test]
        public void test_load_empty_slot_reports_slot_empty()
        {
            var coordinator = new SaveCoordinator(new MemMedium());

            var result = coordinator.Load("none");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.SlotEmpty));
            Assert.That(result.Session, Is.Null);
        }

        [Test]
        public void test_load_corrupted_slot_is_rejected_without_session()
        {
            var medium = new MemMedium();
            medium.Seed("campaign", "完全不是存档文本");
            var coordinator = new SaveCoordinator(medium);

            var result = coordinator.Load("campaign");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(LoadErrorCode.Corrupted));
            Assert.That(result.Session, Is.Null, "损坏存档零部分载入。");
        }
    }
}
