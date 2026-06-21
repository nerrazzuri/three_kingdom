using System;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 配置条目的稳定标识（ADR-0003 §2：stable_id 跨版本不变，交叉引用用）。
    /// 序数比较（不区分文化），非空非空白；空值经构造函数被拒绝。
    /// </summary>
    public readonly struct StableId : IEquatable<StableId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        /// <summary>由非空字符串构造；空或空白抛 <see cref="ArgumentException"/>。</summary>
        public StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("StableId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(StableId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is StableId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(StableId a, StableId b) => a.Equals(b);
        public static bool operator !=(StableId a, StableId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>
    /// 一组配置的标识（供 <see cref="IConfigLoader.Load"/>）。语义同 <see cref="StableId"/>。
    /// </summary>
    public readonly struct ConfigSetId : IEquatable<ConfigSetId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        /// <summary>由非空字符串构造；空或空白抛 <see cref="ArgumentException"/>。</summary>
        public ConfigSetId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ConfigSetId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(ConfigSetId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ConfigSetId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(ConfigSetId a, ConfigSetId b) => a.Equals(b);
        public static bool operator !=(ConfigSetId a, ConfigSetId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
