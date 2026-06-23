using System;
using System.Collections.Generic;
using System.Globalization;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 世界状态展示视图（ADR-0002：Presentation 把 Application 只读投影翻译为 UI 文案）。
    /// 把 <see cref="WorldStatusProjection"/> 映射为中文展示串（时段中文名、「第 N 日」、合成时辰标签、跨日提示）。
    /// 不可变、纯映射，无规则、不触达 Domain 可变状态。逻辑由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    public sealed class WorldStatusView
    {
        /// <summary>时段中文名（DaySegment 黎明/白昼/黄昏/夜间，GDD_001）。</summary>
        private static readonly IReadOnlyDictionary<DaySegment, string> SegmentLabels =
            new Dictionary<DaySegment, string>
            {
                [DaySegment.Dawn] = "黎明",
                [DaySegment.Day] = "白昼",
                [DaySegment.Dusk] = "黄昏",
                [DaySegment.Night] = "夜间",
            };

        /// <summary>玩家日标签（「第 N 日」，由 0 基日序号 +1）。</summary>
        public string DayLabel { get; }

        /// <summary>时段中文标签（如「黎明」）。</summary>
        public string SegmentLabel { get; }

        /// <summary>合成时辰标签（如「第 1 日 · 黎明」），供时间条直接显示。</summary>
        public string TimeLabel { get; }

        /// <summary>最近一次推进是否跨入新一日。</summary>
        public bool CrossedDay { get; }

        /// <summary>跨日提示文案（未跨日为空串）。</summary>
        public string CrossDayNotice { get; }

        public WorldStatusView(WorldStatusProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            if (!SegmentLabels.TryGetValue(projection.Segment, out var seg))
                throw new ArgumentOutOfRangeException(nameof(projection), "未知时段，无中文标签。");

            DayLabel = "第 " + (projection.Day + 1).ToString(CultureInfo.InvariantCulture) + " 日";
            SegmentLabel = seg;
            TimeLabel = DayLabel + " · " + SegmentLabel;
            CrossedDay = projection.DaysCrossedLastAdvance > 0;
            CrossDayNotice = CrossedDay
                ? "跨入新一日（穿越 " + projection.DaysCrossedLastAdvance.ToString(CultureInfo.InvariantCulture) + " 个日界）"
                : string.Empty;
        }
    }
}
