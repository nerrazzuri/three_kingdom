using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 世界状态的<b>只读投影 DTO</b>（ADR-0002：Application → Presentation 只暴露不可变投影，不泄露可变聚合）。
    /// 仅承载展示所需的权威读值——日序号（0 基权威值）、时段、绝对索引、最近一次推进穿越的日界数。
    /// 不含任何 UI 文案（中文标签由 Presentation 的展示视图负责）；不可变、值语义。
    /// </summary>
    public sealed class WorldStatusProjection
    {
        /// <summary>世界日序号（0 基权威值；玩家展示「第 N 日」由 Presentation +1）。</summary>
        public int Day { get; }

        /// <summary>当前日内时段（Domain 枚举；中文标签由 Presentation 映射）。</summary>
        public DaySegment Segment { get; }

        /// <summary>绝对时间索引 T = Day×SegmentsPerDay + Segment（用于先后/时间差）。</summary>
        public long AbsoluteIndex { get; }

        /// <summary>最近一次推进穿越的日界数（0=未跨日；用于「跨入新一日」提示）。</summary>
        public int DaysCrossedLastAdvance { get; }

        public WorldStatusProjection(int day, DaySegment segment, long absoluteIndex, int daysCrossedLastAdvance)
        {
            Day = day;
            Segment = segment;
            AbsoluteIndex = absoluteIndex;
            DaysCrossedLastAdvance = daysCrossedLastAdvance;
        }
    }
}
