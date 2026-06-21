using System;

namespace ThreeKingdom.Domain.Numerics
{
    /// <summary>
    /// Q16.16 定点数（32 位：高 16 位整数 + 低 16 位小数）。ADR-0004 数值支柱：
    /// 所有进入状态哈希的权威结算用整数/定点，**禁止 float/double**（跨平台逐位一致）。
    /// 用于 GDD 中的 [0,1] 比例、乘数、阈值等小数表达；大整数计数（兵力等）仍用 int/long。
    /// 表示范围约 ±32767，分辨率 1/65536。乘除采用**向零截断**（与整数除法一致，跨平台确定）；
    /// 加减乘除均经 <c>checked</c>，溢出抛 <see cref="OverflowException"/>（确定且可测，不静默回绕）。
    /// </summary>
    public readonly struct FixedPoint : IEquatable<FixedPoint>, IComparable<FixedPoint>
    {
        /// <summary>小数位数。</summary>
        public const int FractionalBits = 16;

        /// <summary>1.0 的原始整数表示（2^16）。</summary>
        internal const long OneRaw = 1L << FractionalBits;

        /// <summary>原始 Q16.16 整数值（value = Raw / 65536）。纳入状态哈希的权威字节即此值。</summary>
        public int Raw { get; }

        private FixedPoint(int raw) => Raw = raw;

        /// <summary>由原始 Q16.16 整数构造（如随机流取小数位）。</summary>
        public static FixedPoint FromRaw(int raw) => new FixedPoint(raw);

        /// <summary>由整数构造；超出表示范围抛 OverflowException。</summary>
        public static FixedPoint FromInt(int value) => new FixedPoint(checked((int)((long)value * OneRaw)));

        /// <summary>由分数 numerator/denominator 构造（向零截断）。denominator=0 抛 DivideByZeroException。</summary>
        public static FixedPoint FromFraction(int numerator, int denominator)
        {
            if (denominator == 0) throw new DivideByZeroException("FixedPoint.FromFraction：分母不可为 0。");
            return new FixedPoint(checked((int)(((long)numerator << FractionalBits) / denominator)));
        }

        /// <summary>0。</summary>
        public static FixedPoint Zero => new FixedPoint(0);

        /// <summary>1。</summary>
        public static FixedPoint One => new FixedPoint((int)OneRaw);

        /// <summary>向下取整（向 -∞）。</summary>
        public int FloorToInt() => Raw >> FractionalBits;

        /// <summary>四舍五入到最近整数（半数向 +∞）。</summary>
        public int RoundToInt() => (Raw + (int)(OneRaw >> 1)) >> FractionalBits;

        /// <summary>绝对值（int.MinValue 取反会溢出，经 checked 抛出）。</summary>
        public FixedPoint Abs() => new FixedPoint(checked(Math.Abs(Raw)));

        /// <summary>夹取到 [min, max]。min &gt; max 抛 ArgumentException。</summary>
        public FixedPoint Clamp(FixedPoint min, FixedPoint max)
        {
            if (min.Raw > max.Raw) throw new ArgumentException("FixedPoint.Clamp：min 不可大于 max。");
            if (Raw < min.Raw) return min;
            if (Raw > max.Raw) return max;
            return this;
        }

        public static FixedPoint operator +(FixedPoint a, FixedPoint b)
            => new FixedPoint(checked((int)((long)a.Raw + b.Raw)));

        public static FixedPoint operator -(FixedPoint a, FixedPoint b)
            => new FixedPoint(checked((int)((long)a.Raw - b.Raw)));

        public static FixedPoint operator -(FixedPoint a)
            => new FixedPoint(checked(-a.Raw));

        public static FixedPoint operator *(FixedPoint a, FixedPoint b)
        {
            long product = (long)a.Raw * b.Raw;   // 标度 2^32
            long scaled = product / OneRaw;        // 向零截断 → 标度 2^16
            return new FixedPoint(checked((int)scaled));
        }

        public static FixedPoint operator /(FixedPoint a, FixedPoint b)
        {
            if (b.Raw == 0) throw new DivideByZeroException("FixedPoint：除数不可为 0。");
            long numerator = (long)a.Raw << FractionalBits; // 标度 2^32
            long quotient = numerator / b.Raw;               // 向零截断 → 标度 2^16
            return new FixedPoint(checked((int)quotient));
        }

        public static bool operator ==(FixedPoint a, FixedPoint b) => a.Raw == b.Raw;
        public static bool operator !=(FixedPoint a, FixedPoint b) => a.Raw != b.Raw;
        public static bool operator <(FixedPoint a, FixedPoint b) => a.Raw < b.Raw;
        public static bool operator >(FixedPoint a, FixedPoint b) => a.Raw > b.Raw;
        public static bool operator <=(FixedPoint a, FixedPoint b) => a.Raw <= b.Raw;
        public static bool operator >=(FixedPoint a, FixedPoint b) => a.Raw >= b.Raw;

        public bool Equals(FixedPoint other) => Raw == other.Raw;
        public override bool Equals(object? obj) => obj is FixedPoint other && Raw == other.Raw;
        public override int GetHashCode() => Raw;
        public int CompareTo(FixedPoint other) => Raw.CompareTo(other.Raw);

        /// <summary>非权威显示（用 decimal，不用 float，避免任何浮点进入代码）。</summary>
        public override string ToString() => ((decimal)Raw / OneRaw).ToString("0.######");
    }
}
