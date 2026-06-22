using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Intel;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 敌方情报的展示模型（P10/P1 不完全信息核心设计锁 / ADR-0002）。
    /// <b>只</b>由阵营知识投影（<see cref="IntelProjection"/>，结构上不含真值）派生——
    /// 故本类型<b>无任何权威真值字段</b>，只有报告估计值/来源/时效（负向不变量由 PresentationLockTests 反射断言）。
    /// 不可变。
    /// </summary>
    public sealed class EnemyIntelView
    {
        /// <summary>主题标识（展示文本）。</summary>
        public string SubjectLabel { get; }

        /// <summary>估计兵力（<b>报告值</b>，非真值；来自 <see cref="IntelKnowledgeEntry.KnownStrength"/>）。</summary>
        public int EstimatedStrength { get; }

        /// <summary>来源标签（可靠性提示）。</summary>
        public string SourceLabel { get; }

        /// <summary>观察时间标签（时效基准）。</summary>
        public string ObservedAtLabel { get; }

        private EnemyIntelView(string subjectLabel, int estimatedStrength, string sourceLabel, string observedAtLabel)
        {
            SubjectLabel = subjectLabel;
            EstimatedStrength = estimatedStrength;
            SourceLabel = sourceLabel;
            ObservedAtLabel = observedAtLabel;
        }

        /// <summary>从单条知识条目构造（只搬运阵营合法字段，绝不接触真值）。</summary>
        public static EnemyIntelView FromEntry(IntelKnowledgeEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            return new EnemyIntelView(
                entry.Subject.ToString(),
                entry.KnownStrength,
                entry.Source.ToString(),
                entry.ObservedAt.ToString());
        }
    }

    /// <summary>
    /// 敌方情报面板展示模型（P10）：从只读投影派生的一组探报条目。
    /// 条目按主题标签序数升序排列，保证<b>确定性</b>（同投影 → 同展示模型）。不可变。
    /// </summary>
    public sealed class EnemyIntelPanelView
    {
        /// <summary>探报条目（只读，确定性排序）。</summary>
        public IReadOnlyList<EnemyIntelView> Entries { get; }

        private EnemyIntelPanelView(IReadOnlyList<EnemyIntelView> entries) => Entries = entries;

        /// <summary>从阵营情报投影构造敌方面板（仅探报，无真值）。</summary>
        public static EnemyIntelPanelView FromProjection(IntelProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            var list = new List<EnemyIntelView>();
            foreach (var entry in projection.Entries)
                list.Add(EnemyIntelView.FromEntry(entry));
            list.Sort((a, b) => string.CompareOrdinal(a.SubjectLabel, b.SubjectLabel));
            return new EnemyIntelPanelView(list);
        }
    }
}
