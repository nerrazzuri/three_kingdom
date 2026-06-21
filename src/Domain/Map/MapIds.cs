using System;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>区域稳定 ID（GDD_003：Region.稳定 ID）。序数比较，非空。单位无权威像素坐标——位置即区域/路线（AC-1）。</summary>
    public readonly struct RegionId : IEquatable<RegionId>, IComparable<RegionId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public RegionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("RegionId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public int CompareTo(RegionId other) => string.CompareOrdinal(Value, other.Value);
        public bool Equals(RegionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is RegionId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(RegionId a, RegionId b) => a.Equals(b);
        public static bool operator !=(RegionId a, RegionId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>路线稳定 ID（GDD_003：Route.稳定 ID）。序数比较，非空；寻路平局按其字典序破除（§2）。</summary>
    public readonly struct RouteId : IEquatable<RouteId>, IComparable<RouteId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public RouteId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("RouteId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public int CompareTo(RouteId other) => string.CompareOrdinal(Value, other.Value);
        public bool Equals(RouteId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is RouteId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(RouteId a, RouteId b) => a.Equals(b);
        public static bool operator !=(RouteId a, RouteId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
