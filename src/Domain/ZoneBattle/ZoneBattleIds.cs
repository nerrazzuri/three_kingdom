using System;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>战场区域稳定 ID（GDD_021：区域为战斗的空间单位，<b>非坐标</b>）。序数比较，非空。</summary>
    public readonly struct ZoneId : IEquatable<ZoneId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public ZoneId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ZoneId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(ZoneId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ZoneId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(ZoneId a, ZoneId b) => a.Equals(b);
        public static bool operator !=(ZoneId a, ZoneId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>支队稳定 ID（GDD_021：支队=将+兵种+兵力的可部署单位）。序数比较，非空。</summary>
    public readonly struct DetachmentId : IEquatable<DetachmentId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public DetachmentId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("DetachmentId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public bool Equals(DetachmentId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is DetachmentId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(DetachmentId a, DetachmentId b) => a.Equals(b);
        public static bool operator !=(DetachmentId a, DetachmentId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>战斗阵营角色（GDD_021 R7 攻守统一：同引擎、仅初始角色异）。</summary>
    public enum BattleSide
    {
        /// <summary>攻方。</summary>
        Attacker = 0,
        /// <summary>守方。</summary>
        Defender = 1,
    }

    /// <summary>支队姿态（GDD_021 R4 战中调整之一；影响战力/条件/暴露）。</summary>
    public enum Posture
    {
        /// <summary>主攻（战力偏攻、暴露高）。</summary>
        Assault = 0,
        /// <summary>佯攻（牵制、诱敌，利假退）。</summary>
        Feint = 1,
        /// <summary>守（战力偏守、稳）。</summary>
        Hold = 2,
    }
}
