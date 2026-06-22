using System;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>外势力 ID（GDD_012 §8：静态背景外势力，无完整 AI）。序数比较，非空。</summary>
    public readonly struct ForeignPowerId : IEquatable<ForeignPowerId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public ForeignPowerId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ForeignPowerId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(ForeignPowerId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ForeignPowerId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(ForeignPowerId a, ForeignPowerId b) => a.Equals(b);
        public static bool operator !=(ForeignPowerId a, ForeignPowerId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
