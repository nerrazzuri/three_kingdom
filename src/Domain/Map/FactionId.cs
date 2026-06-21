using System;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>阵营稳定 ID。序数比较，非空。用于地图真值归属与阵营知识分表。</summary>
    public readonly struct FactionId : IEquatable<FactionId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public FactionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("FactionId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(FactionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is FactionId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(FactionId a, FactionId b) => a.Equals(b);
        public static bool operator !=(FactionId a, FactionId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
