using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报第 3 层——报告（GDD_007 / TR-intel-001）。
    /// 由观察派生、带<b>来源</b>的可上报情报。S1 承载来源 + 报告值 + 观察时间；
    /// 置信度/时效衰减/估计区间由 Story 002 在此基础上派生。独立于真值与观察层。不可变。
    /// </summary>
    public sealed class IntelReport
    {
        /// <summary>主题。</summary>
        public IntelSubjectId Subject { get; }

        /// <summary>归属阵营（持有该报告的一方）。</summary>
        public FactionId Faction { get; }

        /// <summary>报告的兵力（来自观察，非真值——可能与真值冲突）。</summary>
        public int ReportedStrength { get; }

        /// <summary>来源（决定可靠性）。</summary>
        public IntelSource Source { get; }

        /// <summary>原始观察时间（时效基准）。</summary>
        public WorldTime ObservedAt { get; }

        public IntelReport(IntelSubjectId subject, FactionId faction, int reportedStrength, IntelSource source, WorldTime observedAt)
        {
            Subject = subject;
            Faction = faction;
            ReportedStrength = reportedStrength;
            Source = source;
            ObservedAt = observedAt;
        }
    }
}
