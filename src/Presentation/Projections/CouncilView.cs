using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 单条军师建议的展示模型（P11 无最优解 / GDD_008 / ADR-0002）。
    /// 只呈现观察/假设/所需条件/风险/缺失情报 + <b>依据可靠性</b>标签。
    /// 本类型<b>刻意不含</b>成功率、最优/推荐标记、排序分值（负向不变量由 PresentationLockTests 反射断言）。
    /// <see cref="EvidenceConfidenceLabel"/> 是依据可靠性的<b>定性</b>标签，<b>非</b>成功概率数值。不可变。
    /// </summary>
    public sealed class AdviceView
    {
        /// <summary>候选标识（路线名，<b>非</b>排名）。</summary>
        public string CandidateLabel { get; }

        /// <summary>观察。</summary>
        public string Observation { get; }

        /// <summary>假设。</summary>
        public string Assumption { get; }

        /// <summary>所需条件。</summary>
        public IReadOnlyList<string> RequiredConditions { get; }

        /// <summary>主要风险。</summary>
        public IReadOnlyList<string> Risks { get; }

        /// <summary>缺失情报（展示文本）。</summary>
        public IReadOnlyList<string> MissingIntel { get; }

        /// <summary>依据可靠性定性标签（低/中/高，非成功率）。</summary>
        public string EvidenceConfidenceLabel { get; }

        private AdviceView(string candidate, string observation, string assumption,
            IReadOnlyList<string> required, IReadOnlyList<string> risks, IReadOnlyList<string> missing, string confidenceLabel)
        {
            CandidateLabel = candidate;
            Observation = observation;
            Assumption = assumption;
            RequiredConditions = required;
            Risks = risks;
            MissingIntel = missing;
            EvidenceConfidenceLabel = confidenceLabel;
        }

        /// <summary>从军师建议构造（不排优劣、不暴露成功率）。</summary>
        public static AdviceView FromStatement(AdviceStatement s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var missing = new List<string>();
            foreach (var m in s.MissingIntel) missing.Add(m.ToString());
            return new AdviceView(
                s.CandidateId, s.Observation, s.Assumption,
                new List<string>(s.RequiredConditions),
                new List<string>(s.Risks),
                missing,
                ConfidenceBand(s.Confidence));
        }

        // 定性分档（依据可靠性），刻意不暴露数值百分比——避免被误读为成功率。
        private static string ConfidenceBand(FixedPoint confidence)
        {
            decimal c = Display.ToDecimal(confidence);
            if (c < 0.34m) return "依据薄弱";
            if (c < 0.67m) return "依据中等";
            return "依据扎实";
        }
    }

    /// <summary>
    /// 军议建议集展示模型（P11 / GDD_008）。建议<b>并列</b>呈现，<b>无</b>全局排名或「最佳方案」标记；
    /// 知识快照变化后标 <see cref="IsStale"/>（不静默重算）。不可变。
    /// </summary>
    public sealed class CouncilView
    {
        /// <summary>并列建议（保持 Domain 给定顺序，不重排为优劣序）。</summary>
        public IReadOnlyList<AdviceView> Advice { get; }

        /// <summary>是否相对当前知识快照已过时。</summary>
        public bool IsStale { get; }

        private CouncilView(IReadOnlyList<AdviceView> advice, bool isStale)
        {
            Advice = advice;
            IsStale = isStale;
        }

        /// <summary>从建议集构造（按当前知识快照判定过时）。</summary>
        public static CouncilView FromSet(CouncilAdviceSet set, KnowledgeSnapshotId currentSnapshot)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            var views = new List<AdviceView>();
            foreach (var s in set.Advice) views.Add(AdviceView.FromStatement(s));
            return new CouncilView(views, set.IsStaleAgainst(currentSnapshot));
        }
    }
}
