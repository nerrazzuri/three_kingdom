using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Cohesion
{
    /// <summary>士气事件 ID（GDD_011：具名事件，用于幂等去重）。序数比较，非空。</summary>
    public readonly struct MoraleEventId : IEquatable<MoraleEventId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public MoraleEventId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("MoraleEventId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(MoraleEventId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is MoraleEventId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(MoraleEventId a, MoraleEventId b) => a.Equals(b);
        public static bool operator !=(MoraleEventId a, MoraleEventId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>
    /// 士气事件（GDD_011 §Data Model：MoraleEvent / §Formula 1）。
    /// 具名（<see cref="Id"/> 用于<b>幂等</b>去重）+ 强度 + 受众权重。同一事件不重复结算。不可变。
    /// </summary>
    public sealed class MoraleEvent
    {
        /// <summary>事件 ID（幂等键）。</summary>
        public MoraleEventId Id { get; }

        /// <summary>士气变化强度 Δm（可正可负，定点）。</summary>
        public FixedPoint Intensity { get; }

        /// <summary>对该单位的受众权重 audience_w（[0,1]）。</summary>
        public FixedPoint AudienceWeight { get; }

        public MoraleEvent(MoraleEventId id, FixedPoint intensity, FixedPoint audienceWeight)
        {
            if (audienceWeight < FixedPoint.Zero || audienceWeight > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(audienceWeight), "受众权重须在 [0,1]。");
            Id = id;
            Intensity = intensity;
            AudienceWeight = audienceWeight;
        }
    }
}
