using System;

namespace ThreeKingdom.Domain.Supply
{
    /// <summary>运输批次 ID（GDD_012：SupplyConvoy 具名批次）。序数比较，非空。</summary>
    public readonly struct ConvoyId : IEquatable<ConvoyId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public ConvoyId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ConvoyId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(ConvoyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ConvoyId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(ConvoyId a, ConvoyId b) => a.Equals(b);
        public static bool operator !=(ConvoyId a, ConvoyId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>单位 ID（GDD_012：UnitProvisions 携行库存归属）。序数比较，非空。</summary>
    public readonly struct UnitId : IEquatable<UnitId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public UnitId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("UnitId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(UnitId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is UnitId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(UnitId a, UnitId b) => a.Equals(b);
        public static bool operator !=(UnitId a, UnitId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
