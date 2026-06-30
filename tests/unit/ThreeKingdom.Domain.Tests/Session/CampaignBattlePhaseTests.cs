using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Numerics;
using B = ThreeKingdom.Domain.Tests.Session.CampaignBattleStateTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-019 story-002：阶段命令解析（Integration / Assembly）。
    /// 治理 ADR：ADR-0004（确定性）+ ADR-0009（装配，复用 BattleResolver）。TR-battle-001/003。
    /// 覆盖：阶段解析更新态；确定性；异常原子回滚；多阶段推进。
    /// </summary>
    [TestFixture]
    public class CampaignBattlePhaseTests
    {
        private static readonly CampaignSessionService Service = B.Service;

        private static CampaignSession StartedBattle(ulong seed = 42)
        {
            CampaignSession s = B.SessionWithCommittedPlan();
            Service.StartBattle(s, B.Units(), B.BattleCfg(), seed, B.TacticChains());
            return s;
        }

        // ---- AC-1: 阶段命令解析更新战斗态 ----

        [Test]
        public void test_resolve_phase_commits_and_updates_state()
        {
            CampaignSession s = StartedBattle();
            var orders = new List<BattleOrder>
            {
                new BattleOrder(0, B.PlayerUnit, BattleOrderType.Engage, targetUnit: B.EnemyUnit),
            };

            BattleResolution r = Service.ResolveBattlePhase(s, orders);

            Assert.That(r.Committed, Is.True);
            Assert.That(s.Battle, Is.Not.Null);
        }

        // ---- AC-2: 解析确定性 ----

        [Test]
        public void test_resolve_phase_is_deterministic()
        {
            CampaignSession a = StartedBattle(seed: 7);
            CampaignSession b = StartedBattle(seed: 7);
            var orders = new List<BattleOrder>
            {
                new BattleOrder(0, B.PlayerUnit, BattleOrderType.Engage, targetUnit: B.EnemyUnit),
            };

            BattleResolution ra = Service.ResolveBattlePhase(a, orders);
            BattleResolution rb = Service.ResolveBattlePhase(b, orders);

            Assert.That(ra.Hash, Is.EqualTo(rb.Hash), "同快照+种子+命令 → 同 BattleResolution.Hash");
            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "会话哈希一致");
        }

        // ---- AC-3: 异常原子回滚 ----

        [Test]
        public void test_illegal_order_rolls_back_atomically()
        {
            CampaignSession s = StartedBattle();
            StateHash before = s.ComputeHash();
            var badOrders = new List<BattleOrder>
            {
                new BattleOrder(0, new BattleUnitId("unit-ghost"), BattleOrderType.Hold),   // actor 不存在
            };

            BattleResolution r = Service.ResolveBattlePhase(s, badOrders);

            Assert.That(r.Committed, Is.False, "非法命令整阶段回滚");
            Assert.That(r.Error, Is.Not.Null);
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "回滚后会话战斗态不变");
        }

        // ---- AC-4: 多阶段连续推进确定性 ----

        [Test]
        public void test_multi_phase_advance_deterministic()
        {
            var orders = new List<BattleOrder>
            {
                new BattleOrder(0, B.PlayerUnit, BattleOrderType.Engage, targetUnit: B.EnemyUnit),
            };

            CampaignSession a = StartedBattle(seed: 9);
            Service.ResolveBattlePhase(a, orders);
            Service.ResolveBattlePhase(a, orders);

            CampaignSession b = StartedBattle(seed: 9);
            Service.ResolveBattlePhase(b, orders);
            Service.ResolveBattlePhase(b, orders);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "多阶段直推确定性一致");
        }
    }
}
