using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Characters
{
    /// <summary>
    /// epic-003 story-001：人物核心状态与不变量。
    /// 治理 ADR：ADR-0002（人物为 Domain 权威）+ ADR-0004（确定性定点系数）。GDD_005 / TR-character-001。
    /// 覆盖 AC-1 核心状态 + 构造不变量、AC-2 能力/健康→过程质量系数（非开关）、AC-3 无无条件技能解锁（负向）。
    /// </summary>
    [TestFixture]
    public class CharacterCoreStateTests
    {
        private static CapabilitySet Caps(int command, int valor, int strategy, int governance, int diplomacy)
            => new CapabilitySet(new Dictionary<CapabilityDomain, int>
            {
                [CapabilityDomain.Command] = command,
                [CapabilityDomain.Valor] = valor,
                [CapabilityDomain.Strategy] = strategy,
                [CapabilityDomain.Governance] = governance,
                [CapabilityDomain.Diplomacy] = diplomacy,
            });

        private static HealthState Health(HealthLevel level, int factorNum, int factorDen)
            => new HealthState(level, FixedPoint.FromFraction(factorNum, factorDen));

        private static CharacterState Character(CapabilitySet caps, HealthState health)
            => new CharacterState(new CharacterId("c1"), "诸葛亮", caps,
                new PersonalityProfile(null), health, new RoleId("advisor"));

        // ---- AC-1：核心状态与不变量 ----

        [Test]
        public void Valid_character_constructs()
        {
            var c = Character(Caps(60, 30, 80, 70, 75), new HealthState(HealthLevel.Healthy, FixedPoint.One));
            Assert.That(c.Identity, Is.EqualTo("诸葛亮"));
            Assert.That(c.Capabilities.Level(CapabilityDomain.Strategy), Is.EqualTo(80));
            Assert.That(c.Role, Is.EqualTo(new RoleId("advisor")));
        }

        [Test]
        public void Capability_out_of_range_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Caps(60, 30, 101, 70, 75)); // >CapabilityMax
            Assert.Throws<ArgumentOutOfRangeException>(() => Caps(-1, 30, 80, 70, 75));   // <0
        }

        [Test]
        public void Missing_capability_domain_is_rejected()
        {
            Assert.Throws<ArgumentException>(() => new CapabilitySet(new Dictionary<CapabilityDomain, int>
            {
                [CapabilityDomain.Command] = 50, // 缺其余四域
            }));
        }

        [Test]
        public void Empty_identity_is_rejected()
        {
            Assert.Throws<ArgumentException>(() => new CharacterState(new CharacterId("c1"), "  ",
                Caps(1, 1, 1, 1, 1), new PersonalityProfile(null),
                new HealthState(HealthLevel.Healthy, FixedPoint.One), new RoleId("r")));
        }

        [Test]
        public void Personality_trait_out_of_range_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PersonalityProfile(new Dictionary<PersonalityTrait, FixedPoint>
            {
                [PersonalityTrait.Risk] = FixedPoint.FromInt(2), // >1
            }));
        }

        [Test]
        public void Incapacitated_must_have_zero_factor()
        {
            Assert.Throws<ArgumentException>(() => new HealthState(HealthLevel.Incapacitated, FixedPoint.One));
            Assert.DoesNotThrow(() => new HealthState(HealthLevel.Incapacitated, FixedPoint.Zero));
        }

        [Test]
        public void Health_factor_out_of_range_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthState(HealthLevel.Injured, FixedPoint.FromInt(2)));
        }

        // ---- AC-2：能力/健康 → 过程质量系数 ----

        [Test]
        public void Quality_matches_gdd_weighted_example()
        {
            // w={谋略:7, 统御:3}，cap={谋略:80, 统御:60}，health=0.9 → ≈0.666
            var c = Character(Caps(60, 0, 80, 0, 0), new HealthState(HealthLevel.Injured, FixedPoint.FromFraction(9, 10)));
            var weights = new TaskCapabilityWeights(new Dictionary<CapabilityDomain, int>
            {
                [CapabilityDomain.Strategy] = 7,
                [CapabilityDomain.Command] = 3,
            });

            var quality = c.ComputeQuality(weights);

            Assert.That(quality > FixedPoint.FromFraction(66, 100), Is.True);
            Assert.That(quality < FixedPoint.FromFraction(67, 100), Is.True);
        }

        [Test]
        public void Quality_scales_with_health_factor()
        {
            var caps = Caps(0, 0, 80, 0, 0);
            var weights = new TaskCapabilityWeights(new Dictionary<CapabilityDomain, int> { [CapabilityDomain.Strategy] = 1 });

            var full = Character(caps, new HealthState(HealthLevel.Healthy, FixedPoint.One)).ComputeQuality(weights);
            var half = Character(caps, new HealthState(HealthLevel.Injured, FixedPoint.FromFraction(1, 2))).ComputeQuality(weights);

            Assert.That(half < full, Is.True);
            // 健康减半 → 质量减半（half×2 ≈ full）
            Assert.That(half * FixedPoint.FromInt(2), Is.EqualTo(full));
        }

        [Test]
        public void Incapacitated_yields_zero_quality()
        {
            var c = Character(Caps(100, 100, 100, 100, 100), new HealthState(HealthLevel.Incapacitated, FixedPoint.Zero));
            var weights = new TaskCapabilityWeights(new Dictionary<CapabilityDomain, int> { [CapabilityDomain.Command] = 1 });
            Assert.That(c.ComputeQuality(weights), Is.EqualTo(FixedPoint.Zero));
        }

        // ---- AC-3：无无条件技能解锁（负向断言）----

        [Test]
        public void Max_capability_yields_coefficient_not_unconditional_unlock()
        {
            // 满能力 + 健康 → 质量系数恰为 1.0（封顶的过程质量系数，而非「解锁」布尔）
            var c = Character(Caps(100, 100, 100, 100, 100), new HealthState(HealthLevel.Healthy, FixedPoint.One));
            var weights = new TaskCapabilityWeights(new Dictionary<CapabilityDomain, int>
            {
                [CapabilityDomain.Command] = 1,
                [CapabilityDomain.Valor] = 1,
                [CapabilityDomain.Strategy] = 1,
                [CapabilityDomain.Governance] = 1,
                [CapabilityDomain.Diplomacy] = 1,
            });
            Assert.That(c.ComputeQuality(weights), Is.EqualTo(FixedPoint.One));
        }

        [Test]
        public void Quality_changes_continuously_without_threshold_jump()
        {
            var weights = new TaskCapabilityWeights(new Dictionary<CapabilityDomain, int> { [CapabilityDomain.Strategy] = 1 });
            var healthy = new HealthState(HealthLevel.Healthy, FixedPoint.One);

            var q80 = Character(Caps(0, 0, 80, 0, 0), healthy).ComputeQuality(weights);
            var q79 = Character(Caps(0, 0, 79, 0, 0), healthy).ComputeQuality(weights);
            var q81 = Character(Caps(0, 0, 81, 0, 0), healthy).ComputeQuality(weights);

            // 单调连续：能力↑则质量↑，无「达标即跳变」的无条件解锁
            Assert.That(q79 < q80, Is.True);
            Assert.That(q80 < q81, Is.True);
        }
    }
}
