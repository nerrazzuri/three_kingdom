using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Characters
{
    /// <summary>
    /// epic-003 story-002：职责权限与命令执行意愿。
    /// 治理 ADR：ADR-0004（确定性意愿）+ ADR-0002（职责权限）。GDD_005 §2/§4 / TR-character-002（消费 coop_score）。
    /// 覆盖 AC-1 职责权限能力不绕过、AC-2 同时段任务冲突拒绝、AC-3 意愿读已结算 coop_score 确定性计算。
    /// </summary>
    [TestFixture]
    public class DutyAuthorityTests
    {
        // ---- AC-1：授权不可绕过（能力不补权限）----

        private static AuthorityRegistry Registry()
            => new AuthorityRegistry(new Dictionary<RoleId, IReadOnlyList<CommandType>>
            {
                [new RoleId("governor")] = new[] { CommandType.Appoint, CommandType.Construct, CommandType.Requisition },
                [new RoleId("scout")] = new[] { CommandType.Dispatch },
            });

        [Test]
        public void Authority_is_granted_by_role()
        {
            var reg = Registry();
            Assert.That(reg.IsAuthorized(new RoleId("governor"), CommandType.Appoint), Is.True);
            Assert.That(reg.IsAuthorized(new RoleId("scout"), CommandType.Dispatch), Is.True);
        }

        [Test]
        public void Authority_denied_for_command_outside_role()
        {
            var reg = Registry();
            // scout 无任命权——即便其人能力极高也无关（IsAuthorized 不接受能力参数，结构性）
            Assert.That(reg.IsAuthorized(new RoleId("scout"), CommandType.Appoint), Is.False);
        }

        [Test]
        public void Unregistered_role_has_no_authority()
        {
            var reg = Registry();
            Assert.That(reg.IsAuthorized(new RoleId("peasant"), CommandType.Construct), Is.False);
        }

        // ---- AC-2：同时段任务兼容性 ----

        private static TaskConflictPolicy Policy()
            => new TaskConflictPolicy(new[]
            {
                (TaskKind.Besieging, TaskKind.Resting),
                (TaskKind.Scouting, TaskKind.Defending),
            });

        [Test]
        public void Conflicting_task_is_rejected_same_segment()
        {
            var policy = Policy();
            Assert.That(policy.CanAssign(new[] { TaskKind.Besieging }, TaskKind.Resting), Is.False);
            Assert.That(policy.Conflicts(TaskKind.Resting, TaskKind.Besieging), Is.True, "冲突为无序对");
        }

        [Test]
        public void Compatible_task_is_allowed()
        {
            var policy = Policy();
            Assert.That(policy.CanAssign(new[] { TaskKind.Governing }, TaskKind.Negotiating), Is.True);
        }

        [Test]
        public void Assign_blocked_if_any_current_task_conflicts()
        {
            var policy = Policy();
            Assert.That(policy.CanAssign(new[] { TaskKind.Governing, TaskKind.Scouting }, TaskKind.Defending), Is.False);
        }

        // ---- AC-3：执行意愿确定性 ----

        [Test]
        public void Willingness_matches_gdd_trait_example()
        {
            // base_will=0.5，w_trait={风险:0.4}，trait={风险:-0.5}，无关系项 → 0.3
            var personality = new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Risk] = FixedPoint.FromFraction(-1, 2),
            });
            var traitWeights = new Dictionary<PersonalityTrait, FixedPoint> { [PersonalityTrait.Risk] = FixedPoint.FromFraction(2, 5) };

            var w = WillingnessCalculator.Compute(FixedPoint.FromFraction(1, 2), personality, traitWeights, FixedPoint.Zero, FixedPoint.Zero);

            Assert.That(w > FixedPoint.FromFraction(29, 100), Is.True);
            Assert.That(w < FixedPoint.FromFraction(31, 100), Is.True);
        }

        [Test]
        public void Willingness_incorporates_settled_coop_score()
        {
            var personality = new PersonalityProfile(null);
            // base 0.5 + coop 0.5 × weight 0.4 = 0.7
            var w = WillingnessCalculator.Compute(
                FixedPoint.FromFraction(1, 2), personality, null,
                coopScore: FixedPoint.FromFraction(1, 2), relationWeight: FixedPoint.FromFraction(2, 5));

            Assert.That(w > FixedPoint.FromFraction(69, 100), Is.True);
            Assert.That(w < FixedPoint.FromFraction(71, 100), Is.True);
        }

        [Test]
        public void Willingness_is_deterministic()
        {
            var personality = new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Discipline] = FixedPoint.FromFraction(3, 10),
            });
            var weights = new Dictionary<PersonalityTrait, FixedPoint> { [PersonalityTrait.Discipline] = FixedPoint.FromFraction(1, 2) };

            FixedPoint Run() => WillingnessCalculator.Compute(
                FixedPoint.FromFraction(1, 2), personality, weights, FixedPoint.FromFraction(1, 4), FixedPoint.FromFraction(1, 5));

            Assert.That(Run(), Is.EqualTo(Run()));
        }

        [Test]
        public void Willingness_clamps_to_unit_interval()
        {
            var personality = new PersonalityProfile(null);
            // 远超 1 → 夹到 1
            var high = WillingnessCalculator.Compute(FixedPoint.One, personality, null, FixedPoint.One, FixedPoint.One);
            Assert.That(high, Is.EqualTo(FixedPoint.One));
            // 远低于 0 → 夹到 0
            var low = WillingnessCalculator.Compute(FixedPoint.Zero, personality, null, FixedPoint.FromInt(-1), FixedPoint.One);
            Assert.That(low, Is.EqualTo(FixedPoint.Zero));
        }

        [Test]
        public void Willingness_rejects_base_out_of_range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => WillingnessCalculator.Compute(
                FixedPoint.FromInt(2), new PersonalityProfile(null), null, FixedPoint.Zero, FixedPoint.Zero));
        }
    }
}
