using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 阵营知识单条（GDD_007 第 4 层）。由报告累积而成的<b>玩家可见</b>条目，
    /// <b>只含阵营合法字段</b>（报告值/来源/观察时间），<b>绝无</b>真值字段。不可变。
    /// </summary>
    public sealed class IntelKnowledgeEntry
    {
        /// <summary>主题。</summary>
        public IntelSubjectId Subject { get; }

        /// <summary>已知兵力（来自报告，非真值）。</summary>
        public int KnownStrength { get; }

        /// <summary>来源（可靠性）。</summary>
        public IntelSource Source { get; }

        /// <summary>观察时间（时效基准）。</summary>
        public WorldTime ObservedAt { get; }

        public IntelKnowledgeEntry(IntelSubjectId subject, int knownStrength, IntelSource source, WorldTime observedAt)
        {
            Subject = subject;
            KnownStrength = knownStrength;
            Source = source;
            ObservedAt = observedAt;
        }
    }

    /// <summary>
    /// 显示层只读投影（GDD_007 / TR-intel-001 / P1 不完全信息）。
    /// UI <b>只能</b>读取本投影；结构上<b>不含</b>任何世界真值字段，故无从泄露真值。
    /// 不可变快照。
    /// </summary>
    public sealed class IntelProjection
    {
        private readonly Dictionary<IntelSubjectId, IntelKnowledgeEntry> _entries;

        internal IntelProjection(Dictionary<IntelSubjectId, IntelKnowledgeEntry> entries)
            => _entries = entries;

        /// <summary>已知主题数。</summary>
        public int Count => _entries.Count;

        /// <summary>是否持有某主题的知识。</summary>
        public bool Knows(IntelSubjectId subject) => _entries.ContainsKey(subject);

        /// <summary>取某主题的知识条目；无则 false。</summary>
        public bool TryGet(IntelSubjectId subject, out IntelKnowledgeEntry entry) => _entries.TryGetValue(subject, out entry!);

        /// <summary>全部已知条目（只读）。</summary>
        public IReadOnlyCollection<IntelKnowledgeEntry> Entries => _entries.Values;
    }

    /// <summary>
    /// 阵营知识层（GDD_007 第 4 层 / TR-intel-001 / ADR-0002）。
    /// 累积某阵营由报告获得的知识；<see cref="Project"/> 导出只读投影供显示层读取。
    /// 同一主题以最新报告覆盖（Story 002 将引入时效/置信权衡）。
    /// </summary>
    public sealed class FactionIntel
    {
        private readonly Dictionary<IntelSubjectId, IntelKnowledgeEntry> _knowledge = new Dictionary<IntelSubjectId, IntelKnowledgeEntry>();

        /// <summary>所属阵营。</summary>
        public FactionId Faction { get; }

        public FactionIntel(FactionId faction) => Faction = faction;

        /// <summary>
        /// 应用一份报告以更新知识（报告须属本阵营）。从报告派生知识条目——只搬运阵营合法字段，
        /// 不接触真值。
        /// </summary>
        public void ApplyReport(IntelReport report)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (report.Faction != Faction)
                throw new InvalidOperationException("报告归属阵营与本知识层不符。");

            _knowledge[report.Subject] = new IntelKnowledgeEntry(
                report.Subject, report.ReportedStrength, report.Source, report.ObservedAt);
        }

        /// <summary>是否已知某主题。</summary>
        public bool Knows(IntelSubjectId subject) => _knowledge.ContainsKey(subject);

        /// <summary>导出只读投影（不含真值；显示层唯一可读入口）。</summary>
        public IntelProjection Project()
            => new IntelProjection(new Dictionary<IntelSubjectId, IntelKnowledgeEntry>(_knowledge));
    }
}
