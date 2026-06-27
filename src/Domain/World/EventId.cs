using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>历史事件稳定 ID（GDD_015 / ADR-0007：下游按 EventId 稳定序遍历）。序数比较，非空。</summary>
    public readonly struct EventId : IEquatable<EventId>, IComparable<EventId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public EventId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("EventId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(EventId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is EventId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(EventId other) => string.CompareOrdinal(Value, other.Value);
        public static bool operator ==(EventId a, EventId b) => a.Equals(b);
        public static bool operator !=(EventId a, EventId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
