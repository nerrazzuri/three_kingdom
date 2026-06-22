using System;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>命令 ID（GDD_009：PreparedOrder / 依赖图节点）。序数比较，非空。</summary>
    public readonly struct OrderId : IEquatable<OrderId>, IComparable<OrderId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public OrderId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("OrderId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public int CompareTo(OrderId other) => string.CompareOrdinal(Value, other.Value);
        public bool Equals(OrderId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is OrderId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(OrderId a, OrderId b) => a.Equals(b);
        public static bool operator !=(OrderId a, OrderId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>资源类型键（GDD_009：ResourceCommitment 资源种类，如粮草）。序数比较，非空。</summary>
    public readonly struct ResourceKey : IEquatable<ResourceKey>, IComparable<ResourceKey>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public ResourceKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ResourceKey 不可为空或空白。", nameof(value));
            Value = value;
        }

        public int CompareTo(ResourceKey other) => string.CompareOrdinal(Value, other.Value);
        public bool Equals(ResourceKey other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ResourceKey other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(ResourceKey a, ResourceKey b) => a.Equals(b);
        public static bool operator !=(ResourceKey a, ResourceKey b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
