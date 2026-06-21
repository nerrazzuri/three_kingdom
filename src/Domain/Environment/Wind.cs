using System;

namespace ThreeKingdom.Domain.Environment
{
    /// <summary>风向（GDD_002）。MVP 四向。</summary>
    public enum WindDirection
    {
        /// <summary>北。</summary>
        North = 0,

        /// <summary>东。</summary>
        East = 1,

        /// <summary>南。</summary>
        South = 2,

        /// <summary>西。</summary>
        West = 3,
    }

    /// <summary>
    /// 风（GDD_002 §Formula 4）：方向 + 强度。纯值对象。
    /// 风力为零时方向<b>仅作历史值，不产生方向效果</b>（GDD §Edge Cases 零风），由 <see cref="HasEffect"/> 门控。
    /// </summary>
    public readonly struct Wind : IEquatable<Wind>
    {
        /// <summary>风向。</summary>
        public WindDirection Direction { get; }

        /// <summary>风力强度（≥0）。</summary>
        public int Strength { get; }

        public Wind(WindDirection direction, int strength)
        {
            if (!Enum.IsDefined(typeof(WindDirection), direction))
                throw new ArgumentOutOfRangeException(nameof(direction), "未定义的风向。");
            if (strength < 0)
                throw new ArgumentOutOfRangeException(nameof(strength), "风力强度不可为负。");
            Direction = direction;
            Strength = strength;
        }

        /// <summary>是否产生方向效果（强度 &gt; 0）。</summary>
        public bool HasEffect => Strength > 0;

        public bool Equals(Wind other) => Direction == other.Direction && Strength == other.Strength;
        public override bool Equals(object? obj) => obj is Wind other && Equals(other);
        public override int GetHashCode() => ((int)Direction * 397) ^ Strength;
        public override string ToString() => $"{Direction}/{Strength}";
    }
}
