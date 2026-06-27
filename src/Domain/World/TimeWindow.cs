using System;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 历史事件时间窗（GDD_015 / ADR-0007：事件四元组之一）。闭区间 [Start, End]（含端点）。
    /// 纯值对象、引擎无关；比较经 <see cref="WorldTime.AbsoluteIndex"/>（ADR-0004 确定）。
    /// </summary>
    public readonly struct TimeWindow : IEquatable<TimeWindow>
    {
        /// <summary>窗口起（含）。</summary>
        public WorldTime Start { get; }

        /// <summary>窗口止（含）。</summary>
        public WorldTime End { get; }

        public TimeWindow(WorldTime start, WorldTime end)
        {
            if (end < start)
                throw new ArgumentException("时间窗 End 不可早于 Start。", nameof(end));
            Start = start;
            End = end;
        }

        /// <summary>给定时间是否落在窗口闭区间内。</summary>
        public bool Contains(WorldTime time) => time >= Start && time <= End;

        public bool Equals(TimeWindow other) => Start == other.Start && End == other.End;
        public override bool Equals(object? obj) => obj is TimeWindow other && Equals(other);
        public override int GetHashCode() => (Start.GetHashCode() * 397) ^ End.GetHashCode();
        public override string ToString() => $"[{Start} .. {End}]";
    }
}
