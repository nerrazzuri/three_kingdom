// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 时间只由推进命令前进，确定性、引擎无关（GDD_001）
// Date: 2026-06-21

using System;

namespace TkSlice.Domain.Time
{
    /// <summary>一日内的时段。slice 固定 4 段。</summary>
    public enum DaySegment
    {
        Dawn = 0,   // 黎明
        Day = 1,    // 白昼
        Dusk = 2,   // 黄昏
        Night = 3,  // 夜
    }

    public enum Season { Spring = 0, Summer = 1, Autumn = 2, Winter = 3 }

    /// <summary>
    /// 世界时间值对象（不可变）。以「自开局起的总时段数」为权威整数，派生日/段/季。
    /// 确定性：纯整数推进，无系统时间、无帧依赖（GDD_001 设计锁）。
    /// </summary>
    public readonly struct WorldDay : IEquatable<WorldDay>, IComparable<WorldDay>
    {
        public const int SegmentsPerDay = 4;
        public const int DaysPerSeason = 30;

        /// <summary>自开局起累计的总时段数（权威）。</summary>
        public readonly int TotalSegments;

        public WorldDay(int totalSegments)
        {
            if (totalSegments < 0) throw new ArgumentOutOfRangeException(nameof(totalSegments));
            TotalSegments = totalSegments;
        }

        public static readonly WorldDay Start = new WorldDay(0);

        /// <summary>第几天（从 1 起）。</summary>
        public int Day => TotalSegments / SegmentsPerDay + 1;
        public DaySegment Segment => (DaySegment)(TotalSegments % SegmentsPerDay);
        public Season Season => (Season)((TotalSegments / SegmentsPerDay / DaysPerSeason) % 4);
        public bool IsNight => Segment == DaySegment.Night;

        public WorldDay Advance(int segments)
        {
            if (segments < 0) throw new ArgumentOutOfRangeException(nameof(segments));
            return new WorldDay(TotalSegments + segments);
        }

        /// <summary>两个时刻之间相隔的时段数（other 在后则为正）。</summary>
        public int SegmentsUntil(WorldDay other) => other.TotalSegments - TotalSegments;

        public bool Equals(WorldDay other) => TotalSegments == other.TotalSegments;
        public override bool Equals(object? obj) => obj is WorldDay w && Equals(w);
        public override int GetHashCode() => TotalSegments;
        public int CompareTo(WorldDay other) => TotalSegments.CompareTo(other.TotalSegments);

        public static bool operator <(WorldDay a, WorldDay b) => a.TotalSegments < b.TotalSegments;
        public static bool operator >(WorldDay a, WorldDay b) => a.TotalSegments > b.TotalSegments;
        public static bool operator <=(WorldDay a, WorldDay b) => a.TotalSegments <= b.TotalSegments;
        public static bool operator >=(WorldDay a, WorldDay b) => a.TotalSegments >= b.TotalSegments;

        public string SegmentName => Segment switch
        {
            DaySegment.Dawn => "黎明",
            DaySegment.Day => "白昼",
            DaySegment.Dusk => "黄昏",
            DaySegment.Night => "夜",
            _ => "?"
        };

        public override string ToString() => $"第{Day}日·{SegmentName}";
    }
}
