using System;
using System.Collections.Generic;
using System.Globalization;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 敌情探报展示视图（P10/P1 不完全信息核心设计锁 / GDD_007 / ADR-0002）。
    /// 由阵营知识投影（<see cref="IntelProjection"/>，结构上<b>不含真值</b>）派生中文探报行，
    /// 并以<b>当前世界时间</b>对比观察时间计算<b>时效</b>（情报越旧越不可信）——只呈现估计值/来源/时效，
    /// <b>绝无真值</b>。无情报时给可行动提示。条目按主题序数排序保证确定性。不可变。
    /// </summary>
    public sealed class EnemyReportView
    {
        /// <summary>是否已持有任何敌情。</summary>
        public bool HasIntel { get; }
        /// <summary>无情报时的可行动提示（有情报时为空串）。</summary>
        public string EmptyLabel { get; }
        /// <summary>探报行（中文：主题｜估计兵力｜时效；按主题序数排序）。</summary>
        public IReadOnlyList<string> Lines { get; }

        public EnemyReportView(IntelProjection projection, WorldTime now)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            var lines = new List<string>();
            var entries = new List<IntelKnowledgeEntry>(projection.Entries);
            entries.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));

            foreach (var entry in entries)
            {
                long gap = now.AbsoluteIndex - entry.ObservedAt.AbsoluteIndex;
                string freshness = gap <= 0
                    ? "刚侦察"
                    : "已过 " + gap.ToString(CultureInfo.InvariantCulture) + " 个时段";
                lines.Add(entry.Subject + "｜估计 " + entry.KnownStrength.ToString(CultureInfo.InvariantCulture)
                    + " 兵｜" + freshness);
            }

            Lines = lines;
            HasIntel = lines.Count > 0;
            EmptyLabel = HasIntel ? string.Empty : "尚无敌情——派出侦察以获取（返报需时）";
        }
    }
}
