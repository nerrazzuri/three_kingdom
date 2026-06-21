using System;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 权威世界时间（GDD_001 / TR-time-001）：世界日序号 + 时段。纯 Domain 值对象，不可变、引擎无关。
    /// 通过绝对索引 <see cref="AbsoluteIndex"/>（<c>T = Day×SEGMENTS_PER_DAY + Segment</c>）比较与排序，
    /// 不依赖帧率或 Unity 时间（ADR-0004）。仅能由推进命令前进（见 <see cref="WorldClock"/>）。
    /// </summary>
    public readonly struct WorldTime : IEquatable<WorldTime>, IComparable<WorldTime>
    {
        /// <summary>每个世界日的时段数（由 <see cref="DaySegment"/> 枚举值数派生，MVP=4，非硬编码）。</summary>
        public static readonly int SegmentsPerDay = Enum.GetValues(typeof(DaySegment)).Length;

        /// <summary>世界日序号（≥0）。</summary>
        public int Day { get; }

        /// <summary>日内时段。</summary>
        public DaySegment Segment { get; }

        /// <summary>构造世界时间；负日序号或未定义时段抛 <see cref="ArgumentOutOfRangeException"/>。</summary>
        public WorldTime(int day, DaySegment segment)
        {
            if (day < 0)
                throw new ArgumentOutOfRangeException(nameof(day), "世界日序号不可为负。");
            if (!Enum.IsDefined(typeof(DaySegment), segment))
                throw new ArgumentOutOfRangeException(nameof(segment), "未定义的时段。");
            Day = day;
            Segment = segment;
        }

        /// <summary>绝对时间索引：T = Day×SEGMENTS_PER_DAY + Segment（用于先后比较、时间差、期限判定）。</summary>
        public long AbsoluteIndex => (long)Day * SegmentsPerDay + (int)Segment;

        /// <summary>
        /// 前进 <paramref name="segments"/> 个时段，返回新时间（值对象，不改原值）。
        /// 负值抛 <see cref="ArgumentOutOfRangeException"/>；跨日按整除/取余反线性化（ADR-0004 确定）。
        /// </summary>
        public WorldTime Advance(int segments)
        {
            if (segments < 0)
                throw new ArgumentOutOfRangeException(nameof(segments), "推进时段数不可为负。");
            long total = checked(AbsoluteIndex + segments);
            int day = checked((int)(total / SegmentsPerDay));
            int seg = (int)(total % SegmentsPerDay);
            return new WorldTime(day, (DaySegment)seg);
        }

        public int CompareTo(WorldTime other) => AbsoluteIndex.CompareTo(other.AbsoluteIndex);

        public bool Equals(WorldTime other) => Day == other.Day && Segment == other.Segment;
        public override bool Equals(object? obj) => obj is WorldTime other && Equals(other);
        public override int GetHashCode() => (Day * 397) ^ (int)Segment;

        public static bool operator ==(WorldTime a, WorldTime b) => a.Equals(b);
        public static bool operator !=(WorldTime a, WorldTime b) => !a.Equals(b);
        public static bool operator <(WorldTime a, WorldTime b) => a.AbsoluteIndex < b.AbsoluteIndex;
        public static bool operator >(WorldTime a, WorldTime b) => a.AbsoluteIndex > b.AbsoluteIndex;
        public static bool operator <=(WorldTime a, WorldTime b) => a.AbsoluteIndex <= b.AbsoluteIndex;
        public static bool operator >=(WorldTime a, WorldTime b) => a.AbsoluteIndex >= b.AbsoluteIndex;

        public override string ToString() => $"D{Day} {Segment}";
    }
}
