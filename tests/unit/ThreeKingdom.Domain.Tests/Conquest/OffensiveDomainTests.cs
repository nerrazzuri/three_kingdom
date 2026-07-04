using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Conquest
{
    /// <summary>
    /// 出征攻城 Domain 核心（GDD_019 / ADR-0010）：占城归属 C（S4）、出征授权门（S1）、闭合因果映射（S3）。
    /// 全纯函数、确定性。
    /// </summary>
    [TestFixture]
    public class OffensiveDomainTests
    {
        // ---- S4 占城归属 C（ADR-0010）----

        private static readonly FixedPoint Half = FixedPoint.FromFraction(1, 2);
        private static readonly FixedPoint Zero = FixedPoint.Zero;

        private static OccupationConfig Cfg(FixedPoint baseGrant, int n = 2) =>
            new OccupationConfig(n, baseGrant, Zero, Zero, Zero, leanPerSeizure: 10);

        [Test]
        public void test_first_n_conquests_always_granted_to_player()
        {
            var svc = new OccupationOwnershipService();
            var cfg = Cfg(Zero);   // base 0 → 第3座起几乎必被君主收；但前2座恒归玩家
            Assert.That(svc.Resolve(0, Zero, Zero, Zero, 1UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
            Assert.That(svc.Resolve(1, Zero, Zero, Zero, 1UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
        }

        [Test]
        public void test_p_grant_one_always_player_zero_always_lord()
        {
            var svc = new OccupationOwnershipService();
            var always = Cfg(FixedPoint.One);   // p=1
            var never = Cfg(Zero);              // p=0
            for (ulong seed = 1; seed <= 5; seed++)
            {
                Assert.That(svc.Resolve(2, Zero, Zero, Zero, seed, always), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
                Assert.That(svc.Resolve(2, Zero, Zero, Zero, seed, never), Is.EqualTo(OwnershipVerdict.LordKeeps));
            }
        }

        [Test]
        public void test_ownership_is_deterministic_for_same_seed()
        {
            var svc = new OccupationOwnershipService();
            var cfg = Cfg(Half);   // p=0.5，取决于种子
            OwnershipVerdict a = svc.Resolve(3, Zero, Zero, Zero, 0xABCDUL, cfg);
            OwnershipVerdict b = svc.Resolve(3, Zero, Zero, Zero, 0xABCDUL, cfg);
            Assert.That(b, Is.EqualTo(a), "同 (index,因子,种子,配置) → 同结果（可复现）。");
        }

        [Test]
        public void test_renown_raises_grant_probability()
        {
            // 高名望权重 → p_grant 抬高 → 更倾向归玩家；用极端权重使名望满时 p→1。
            var svc = new OccupationOwnershipService();
            var cfg = new OccupationConfig(2, Zero, FixedPoint.One, Zero, Zero, 10);   // base0 + 名望权重1
            // 名望归一化 1 → p=1 → 恒归玩家；名望 0 → p=0 → 恒君主。
            Assert.That(svc.Resolve(2, FixedPoint.One, Zero, Zero, 7UL, cfg), Is.EqualTo(OwnershipVerdict.GrantToPlayer));
            Assert.That(svc.Resolve(2, Zero, Zero, Zero, 7UL, cfg), Is.EqualTo(OwnershipVerdict.LordKeeps));
        }

        // ---- S1 出征授权门（GDD_019 R1/R2）----

        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-enemy");
        private static readonly CityId Target = new CityId("city-target");

        [Test]
        public void test_authorized_enemy_city_passes_gate()
        {
            var svc = new OffensiveAuthorizationService();
            var auth = new OffensiveAuthorization(new[] { Target });
            Assert.That(svc.Check(Target, auth, Enemy, Player), Is.EqualTo(OffensiveGateResult.Authorized));
        }

        [Test]
        public void test_gate_rejects_unauthorized_own_and_ownerless()
        {
            var svc = new OffensiveAuthorizationService();
            Assert.That(svc.Check(Target, OffensiveAuthorization.None, Enemy, Player),
                Is.EqualTo(OffensiveGateResult.NotAuthorized), "不在授权集 → 拒。");

            var auth = new OffensiveAuthorization(new[] { Target });
            Assert.That(svc.Check(Target, auth, Player, Player),
                Is.EqualTo(OffensiveGateResult.OwnCity), "己方城 → 拒。");
            Assert.That(svc.Check(Target, auth, null, Player),
                Is.EqualTo(OffensiveGateResult.NotEnemyControlled), "无主/未登记 → 拒。");
        }

        // ---- S3 闭合因果映射（GDD_019 R3/F1/F2）----

        [Test]
        public void test_more_preparation_yields_more_force_and_morale()
        {
            var svc = new OffensiveSetupService();
            var cfg = OffensiveSetupConfig.Default;

            OffensiveForce weak = svc.Derive(new OffensivePreparation(0, 0, Array.Empty<TacticCondition>()), cfg);
            OffensiveForce strong = svc.Derive(new OffensivePreparation(600, 300, Array.Empty<TacticCondition>()), cfg);

            Assert.That(strong.Force, Is.GreaterThan(weak.Force), "兵多→战力高。");
            Assert.That(strong.Morale, Is.GreaterThan(weak.Morale), "粮足→士气高。");
        }

        [Test]
        public void test_planned_conditions_carried_and_morale_capped()
        {
            var svc = new OffensiveSetupService();
            var cfg = OffensiveSetupConfig.Default;
            var conds = new[] { TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued };

            OffensiveForce f = svc.Derive(new OffensivePreparation(100, 1_000_000, conds), cfg);

            Assert.That(f.Conditions, Is.EquivalentTo(conds), "备战计划兵法条件随军携入。");
            Assert.That(f.Morale, Is.EqualTo(cfg.MaxMorale), "补给极多 → 士气封顶，不溢出。");
        }

        [Test]
        public void test_offensive_setup_is_deterministic()
        {
            var svc = new OffensiveSetupService();
            var cfg = OffensiveSetupConfig.Default;
            var prep = new OffensivePreparation(250, 175, Array.Empty<TacticCondition>());
            OffensiveForce a = svc.Derive(prep, cfg);
            OffensiveForce b = svc.Derive(prep, cfg);
            Assert.That(b.Force, Is.EqualTo(a.Force));
            Assert.That(b.Morale, Is.EqualTo(a.Morale));
        }
    }
}
