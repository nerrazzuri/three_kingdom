using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using B = ThreeKingdom.Domain.Tests.Session.CampaignBattleStateTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-019 story-004：战斗态存读档 + 确定性（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档 round-trip）+ ADR-0004（确定性）。TR-battle-001。
    /// 覆盖：战斗态逐字段一致；哈希一致；确定性链；未提供配置整体拒绝；向后兼容。
    /// </summary>
    [TestFixture]
    public class CampaignBattleSaveTests
    {
        private static readonly CampaignSessionService Service = B.Service;

        private static CampaignSession StartedBattle(ulong seed = 42)
        {
            CampaignSession s = B.SessionWithCommittedPlan();
            Service.StartBattle(s, B.Units(), B.BattleCfg(), seed, B.TacticChains());
            return s;
        }

        // 战斗会话同时含准备态（开战需 CommittedPlan），故 Restore 须同时提供准备 + 战斗配置。
        private static CampaignSession Restore(string text)
            => Service.Restore(text, B.Fp,
                prepConfig: new PreparationConfig(10),
                reachableRegions: new[] { B.Pass },
                authorizedOrders: new[] { new OrderId("order-ambush") },
                battleConfig: B.BattleCfg(), tacticChains: B.TacticChains());

        private static List<BattleOrder> EngageOrders()
            => new List<BattleOrder> { new BattleOrder(0, B.PlayerUnit, BattleOrderType.Engage, targetUnit: B.EnemyUnit) };

        // ---- AC-1: 战斗态 round-trip 逐字段一致 ----

        [Test]
        public void test_battle_state_roundtrip_field_for_field()
        {
            CampaignSession s = StartedBattle();
            Service.ResolveBattlePhase(s, EngageOrders());
            Service.MarkTacticCondition(s, TacticCondition.AmbushSurprise);

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            BattleUnitState orig = s.Battle!.Unit(B.PlayerUnit);
            BattleUnitState got = loaded.Battle!.Unit(B.PlayerUnit);
            Assert.That(got.Force, Is.EqualTo(orig.Force));
            Assert.That(got.Morale, Is.EqualTo(orig.Morale));
            Assert.That(got.Region, Is.EqualTo(orig.Region));
            Assert.That(loaded.Battle!.Unit(B.EnemyUnit).Force, Is.EqualTo(s.Battle!.Unit(B.EnemyUnit).Force));
        }

        // ---- AC-2: round-trip 哈希一致 ----

        [Test]
        public void test_battle_roundtrip_preserves_hash()
        {
            CampaignSession s = StartedBattle();
            Service.ResolveBattlePhase(s, EngageOrders());
            Service.MarkTacticCondition(s, TacticCondition.ControlledRetreatKeptFormation);
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-3: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：开战 → 解析A → 解析B。
            CampaignSession direct = StartedBattle(seed: 5);
            Service.ResolveBattlePhase(direct, EngageOrders());
            Service.ResolveBattlePhase(direct, EngageOrders());
            StateHash directHash = direct.ComputeHash();

            // 切割：开战 → 解析A → 存读档 → 解析B。
            CampaignSession s = StartedBattle(seed: 5);
            Service.ResolveBattlePhase(s, EngageOrders());
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.ResolveBattlePhase(loaded, EngageOrders());

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续解析确定性");
        }

        // ---- AC-4: 含战斗态存档未提供配置 → 整体拒绝 ----

        [Test]
        public void test_restore_battle_save_without_config_is_rejected()
        {
            CampaignSession s = StartedBattle();
            string text = Service.CaptureSnapshot(s);

            Assert.Throws<SaveFormatException>(() => Service.Restore(text, B.Fp),
                "含战斗态但未提供 battleConfig 应整体拒绝");
        }

        // ---- 向后兼容：无战斗的会话存读档不受影响 ----

        [Test]
        public void test_non_battle_session_roundtrip_still_works()
        {
            CampaignSession s = B.SessionWithCommittedPlan();   // 已提交计划但未开战
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(
                Service.CaptureSnapshot(s), B.Fp,
                prepConfig: new ThreeKingdom.Domain.Preparation.PreparationConfig(10),
                reachableRegions: new[] { B.Pass },
                authorizedOrders: new[] { new ThreeKingdom.Domain.Preparation.OrderId("order-ambush") });

            Assert.That(loaded.HasBattle, Is.False);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }
    }
}
