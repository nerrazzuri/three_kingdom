// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 确定性核心结算——权威路径用定点 Q16.16，绝不用 float（ADR-0004）
// Date: 2026-06-21

using System;

namespace TkSlice.Domain.Numerics
{
    /// <summary>
    /// Q16.16 定点数。权威 Domain 结算使用，保证跨平台确定性与稳定状态哈希（ADR-0004）。
    /// 内部以 Int32 raw 存储，缩放 2^16。禁止在权威路径使用 float/double。
    /// </summary>
    public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>
    {
        public const int FractionalBits = 16;
        public const int One = 1 << FractionalBits; // 65536
        public readonly int Raw;

        private Fixed(int raw) => Raw = raw;

        public static Fixed FromRaw(int raw) => new Fixed(raw);
        public static Fixed FromInt(int value) => new Fixed(value << FractionalBits);

        /// <summary>由分数构造（确定性，截断除法）。den 必须 &gt; 0。</summary>
        public static Fixed FromFraction(int num, int den)
        {
            if (den <= 0) throw new ArgumentException("denominator must be > 0", nameof(den));
            long raw = ((long)num << FractionalBits) / den;
            return new Fixed((int)raw);
        }

        public static readonly Fixed Zero = new Fixed(0);
        public static readonly Fixed OneValue = new Fixed(One);

        public static Fixed operator +(Fixed a, Fixed b) => new Fixed(a.Raw + b.Raw);
        public static Fixed operator -(Fixed a, Fixed b) => new Fixed(a.Raw - b.Raw);
        public static Fixed operator -(Fixed a) => new Fixed(-a.Raw);

        public static Fixed operator *(Fixed a, Fixed b)
        {
            long product = (long)a.Raw * b.Raw;
            return new Fixed((int)(product >> FractionalBits));
        }

        public static Fixed operator /(Fixed a, Fixed b)
        {
            if (b.Raw == 0) throw new DivideByZeroException("Fixed divide by zero");
            long dividend = (long)a.Raw << FractionalBits;
            return new Fixed((int)(dividend / b.Raw));
        }

        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;
        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;

        public static Fixed Min(Fixed a, Fixed b) => a.Raw <= b.Raw ? a : b;
        public static Fixed Max(Fixed a, Fixed b) => a.Raw >= b.Raw ? a : b;
        public static Fixed Clamp(Fixed v, Fixed lo, Fixed hi) => Max(lo, Min(hi, v));

        /// <summary>向下取整为 int（floor）。</summary>
        public int FloorToInt() => Raw >> FractionalBits;

        /// <summary>向上取整为 int（ceil），用于时段耗时等。</summary>
        public int CeilToInt() => (Raw + One - 1) >> FractionalBits;

        public int RoundToInt() => (Raw + (One >> 1)) >> FractionalBits;

        public bool Equals(Fixed other) => Raw == other.Raw;
        public override bool Equals(object? obj) => obj is Fixed f && Raw == f.Raw;
        public override int GetHashCode() => Raw;
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);

        /// <summary>仅用于非权威的 UI/日志显示（四舍五入到 4 位小数）。</summary>
        public override string ToString()
        {
            // 显示 4 位小数（round，不参与结算）；定点本身有 ~1/65536 量化误差属正常
            long num = (long)Raw * 10000;
            long scaled = num >= 0
                ? (num + (One >> 1)) >> FractionalBits
                : -((-num + (One >> 1)) >> FractionalBits);
            return (scaled / 10000m).ToString("0.####");
        }
    }
}
