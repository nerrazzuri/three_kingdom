using System;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>人物稳定 ID（GDD_005：稳定身份）。序数比较，非空。</summary>
    public readonly struct CharacterId : IEquatable<CharacterId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public CharacterId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CharacterId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(CharacterId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is CharacterId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(CharacterId a, CharacterId b) => a.Equals(b);
        public static bool operator !=(CharacterId a, CharacterId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>职责 ID（GDD_005：职责决定合法权限；权限计算在 Story 002）。序数比较，非空。</summary>
    public readonly struct RoleId : IEquatable<RoleId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public RoleId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("RoleId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(RoleId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is RoleId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(RoleId a, RoleId b) => a.Equals(b);
        public static bool operator !=(RoleId a, RoleId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }
}
