using System;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 命令时间窗（GDD_009：PreparedOrder 起始/结束窗口）。以绝对时段索引表示 [Start, End)，
    /// End &gt; Start。同一执行者两窗重叠即硬冲突 time_overlap（§Formula 1）。不可变值。
    /// </summary>
    public readonly struct TimeWindow : IEquatable<TimeWindow>
    {
        /// <summary>起始绝对时段索引（含）。</summary>
        public int Start { get; }

        /// <summary>结束绝对时段索引（不含）。</summary>
        public int End { get; }

        public TimeWindow(int start, int end)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "起始时段不可为负。");
            if (end <= start) throw new ArgumentOutOfRangeException(nameof(end), "结束时段须大于起始时段。");
            Start = start;
            End = end;
        }

        /// <summary>是否与另一时间窗重叠（半开区间：a.Start &lt; b.End ∧ b.Start &lt; a.End）。</summary>
        public bool Overlaps(TimeWindow other) => Start < other.End && other.Start < End;

        public bool Equals(TimeWindow other) => Start == other.Start && End == other.End;
        public override bool Equals(object? obj) => obj is TimeWindow other && Equals(other);
        public override int GetHashCode() => (Start * 397) ^ End;
        public override string ToString() => $"[{Start},{End})";
    }
}
