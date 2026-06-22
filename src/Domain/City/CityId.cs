using System;

namespace ThreeKingdom.Domain.City
{
    /// <summary>城市稳定 ID（GDD_004：城市为战争的物质基础）。序数比较，非空。</summary>
    public readonly struct CityId : IEquatable<CityId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public CityId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CityId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(CityId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is CityId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(CityId a, CityId b) => a.Equals(b);
        public static bool operator !=(CityId a, CityId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
