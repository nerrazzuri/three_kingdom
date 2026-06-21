using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Numerics
{
    /// <summary>
    /// epic-001 story-002 AC1：Q16.16 加减乘除/比较/转换确定性且跨平台一致；覆盖溢出与舍入边界。
    /// 治理 ADR：ADR-0004（数值支柱：整数/定点为权威，禁 float）。
    /// </summary>
    [TestFixture]
    public class FixedPointTests
    {
        [Test]
        public void FromInt_then_round_and_floor_recover_integer()
        {
            var five = FixedPoint.FromInt(5);
            Assert.That(five.RoundToInt(), Is.EqualTo(5));
            Assert.That(five.FloorToInt(), Is.EqualTo(5));
            Assert.That(five.Raw, Is.EqualTo(5 * 65536));
        }

        [Test]
        public void FromFraction_half_has_expected_raw()
        {
            Assert.That(FixedPoint.FromFraction(1, 2).Raw, Is.EqualTo(32768));
            Assert.That(FixedPoint.FromFraction(1, 2), Is.EqualTo(FixedPoint.FromInt(1) / FixedPoint.FromInt(2)));
        }

        [Test]
        public void Multiply_is_deterministic_and_commutative()
        {
            var half = FixedPoint.FromFraction(1, 2);
            Assert.That((half * half).Raw, Is.EqualTo(16384)); // 0.25
            Assert.That((half * FixedPoint.FromInt(10)).RoundToInt(), Is.EqualTo(5));

            var a = FixedPoint.FromFraction(3, 7);
            var b = FixedPoint.FromFraction(11, 13);
            Assert.That(a * b, Is.EqualTo(b * a)); // 交换律 → 确定一致
        }

        [Test]
        public void Divide_inverts_multiply_for_exact_values()
        {
            var ten = FixedPoint.FromInt(10);
            var two = FixedPoint.FromInt(2);
            Assert.That(ten / two, Is.EqualTo(FixedPoint.FromInt(5)));
        }

        [Test]
        public void Add_and_subtract_are_exact()
        {
            var a = FixedPoint.FromFraction(1, 4);
            var b = FixedPoint.FromFraction(3, 4);
            Assert.That((a + b), Is.EqualTo(FixedPoint.One));
            Assert.That((b - a), Is.EqualTo(FixedPoint.FromFraction(1, 2)));
        }

        [Test]
        public void Comparisons_follow_numeric_order()
        {
            var lo = FixedPoint.FromFraction(1, 4);
            var hi = FixedPoint.FromFraction(3, 4);
            Assert.That(lo < hi, Is.True);
            Assert.That(hi > lo, Is.True);
            Assert.That(lo <= FixedPoint.FromFraction(1, 4), Is.True);
            Assert.That(lo.CompareTo(hi), Is.LessThan(0));
        }

        [Test]
        public void Clamp_bounds_value()
        {
            var min = FixedPoint.Zero;
            var max = FixedPoint.One;
            Assert.That(FixedPoint.FromInt(2).Clamp(min, max), Is.EqualTo(max));
            Assert.That(FixedPoint.FromInt(-1).Clamp(min, max), Is.EqualTo(min));
            Assert.That(FixedPoint.FromFraction(1, 2).Clamp(min, max), Is.EqualTo(FixedPoint.FromFraction(1, 2)));
        }

        // 边界：舍入规则明确（RoundToInt 半数向 +∞；FloorToInt 向 -∞）
        [Test]
        public void Rounding_boundaries_are_defined()
        {
            var oneAndHalf = FixedPoint.FromFraction(3, 2); // 1.5
            Assert.That(oneAndHalf.RoundToInt(), Is.EqualTo(2));
            Assert.That(oneAndHalf.FloorToInt(), Is.EqualTo(1));

            var negHalf = FixedPoint.FromFraction(-1, 2); // -0.5
            Assert.That(negHalf.RoundToInt(), Is.EqualTo(0));   // 半数向 +∞
            Assert.That(negHalf.FloorToInt(), Is.EqualTo(-1));  // 向 -∞
        }

        // 边界：溢出抛 OverflowException（不静默回绕）
        [Test]
        public void FromInt_overflow_throws()
        {
            Assert.Throws<OverflowException>(() => FixedPoint.FromInt(40000)); // 40000<<16 > int.Max
        }

        [Test]
        public void Add_overflow_throws()
        {
            var nearMax = FixedPoint.FromRaw(int.MaxValue);
            Assert.Throws<OverflowException>(() => { var _ = nearMax + FixedPoint.FromRaw(1); });
        }

        [Test]
        public void Divide_by_zero_throws()
        {
            Assert.Throws<DivideByZeroException>(() => { var _ = FixedPoint.One / FixedPoint.Zero; });
        }
    }
}
