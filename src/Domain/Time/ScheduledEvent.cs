using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 调度事件（GDD_001 §Data Model：ScheduledEvent）。承载触发时间、稳定优先级与稳定 ID。
    /// 纯 Domain 值；事件载荷本身由各系统持有，本类型只携全序解析所需的键。
    /// </summary>
    public sealed class ScheduledEvent
    {
        /// <summary>触发时间。</summary>
        public WorldTime Time { get; }

        /// <summary>稳定优先级（升序优先；小者先解析）。</summary>
        public int Priority { get; }

        /// <summary>稳定 ID（序数比较；同时间同优先级时的最终决胜键，非空）。</summary>
        public string StableId { get; }

        public ScheduledEvent(WorldTime time, int priority, string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                throw new ArgumentException("ScheduledEvent.StableId 不可为空或空白。", nameof(stableId));
            Time = time;
            Priority = priority;
            StableId = stableId;
        }

        public override string ToString() => $"{StableId}@{Time}#p{Priority}";
    }

    /// <summary>
    /// 同点事件的确定性全序解析（GDD_001 §7 / TR-time-001）。
    /// 排序键 <c>(AbsoluteIndex, Priority, StableId)</c> 三级升序，不依赖帧率或集合遍历顺序。
    /// 键全程唯一——出现相同 (时间,优先级,stableId) 视为全序歧义，<b>显式拒绝</b>（GDD_001 §Failure：重复事件 ID）。
    /// </summary>
    public static class ScheduledEventOrder
    {
        /// <summary>
        /// 解析为确定性全序列表。重复键抛 <see cref="InvalidOperationException"/>；
        /// 因键唯一，结果与输入顺序及排序算法稳定性无关（同输入 → 同序列）。
        /// </summary>
        public static IReadOnlyList<ScheduledEvent> Resolve(IEnumerable<ScheduledEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            var list = new List<ScheduledEvent>();
            var seen = new HashSet<(long, int, string)>();
            foreach (var e in events)
            {
                if (e == null) throw new ArgumentException("事件不可为 null。", nameof(events));
                var key = (e.Time.AbsoluteIndex, e.Priority, e.StableId);
                if (!seen.Add(key))
                {
                    throw new InvalidOperationException(
                        $"事件全序歧义：相同 (时间,优先级,stableId) = ({e.Time}, p{e.Priority}, {e.StableId})。");
                }
                list.Add(e);
            }

            list.Sort(Compare);
            return list;
        }

        /// <summary>三级排序比较器：时间 → 优先级 → stableId（序数）。</summary>
        private static int Compare(ScheduledEvent a, ScheduledEvent b)
        {
            int byTime = a.Time.AbsoluteIndex.CompareTo(b.Time.AbsoluteIndex);
            if (byTime != 0) return byTime;
            int byPriority = a.Priority.CompareTo(b.Priority);
            if (byPriority != 0) return byPriority;
            return string.CompareOrdinal(a.StableId, b.StableId);
        }
    }
}
