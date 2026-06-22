using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Intel;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 条件化建议模板（GDD_008 §MVP：覆盖假退伏击/断粮疲敌/守城待变的条件与风险）。
    /// 数据驱动的候选论证骨架（观察/假设/所需条件/风险 + 所依据的情报主题），
    /// 由军议据合法知识实例化为 <see cref="AdviceStatement"/>。<b>不含</b>任何成功率/优劣字段。不可变。
    /// </summary>
    public sealed class AdviceTemplate
    {
        /// <summary>候选标识（路线名，如「诱敌伏击」；非排名）。</summary>
        public string CandidateId { get; }

        /// <summary>观察（基于合法知识的事实陈述）。</summary>
        public string Observation { get; }

        /// <summary>假设（须成立的前提，非断言）。</summary>
        public string Assumption { get; }

        /// <summary>所需条件（若…则可考虑…）。</summary>
        public IReadOnlyList<string> RequiredConditions { get; }

        /// <summary>主要风险。</summary>
        public IReadOnlyList<string> Risks { get; }

        /// <summary>本建议所依据的情报主题（用于置信聚合与缺失情报识别）。</summary>
        public IReadOnlyList<IntelSubjectId> ReferencedSubjects { get; }

        public AdviceTemplate(
            string candidateId,
            string observation,
            string assumption,
            IReadOnlyList<string> requiredConditions,
            IReadOnlyList<string> risks,
            IReadOnlyList<IntelSubjectId> referencedSubjects)
        {
            if (string.IsNullOrWhiteSpace(candidateId)) throw new ArgumentException("候选标识不可为空。", nameof(candidateId));
            CandidateId = candidateId;
            Observation = observation ?? string.Empty;
            Assumption = assumption ?? string.Empty;
            RequiredConditions = requiredConditions ?? Array.Empty<string>();
            Risks = risks ?? Array.Empty<string>();
            ReferencedSubjects = referencedSubjects ?? Array.Empty<IntelSubjectId>();
        }
    }
}
