using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 一条军师建议（GDD_008 §Data Model：AdviceStatement / TR-council-002 / 强制设计锁 P11）。
    /// 仅由观察、假设、所需条件、风险、缺失情报、依据置信组成。
    /// <para>
    /// <b>负向不变量（结构性保证）</b>：本类型<b>刻意不含</b>综合成功率、唯一「最优/推荐」标记、
    /// 排序分值或自动命令——军师只整理条件化建议，绝不替玩家裁决或排兵布阵。
    /// <see cref="Confidence"/> 是<b>依据可靠性</b>（源自 GDD_007 effective_conf 聚合），<b>非</b>成功概率。
    /// </para>
    /// 不可变。
    /// </summary>
    public sealed class AdviceStatement
    {
        /// <summary>提出该建议的军师。</summary>
        public AdvisorId Advisor { get; }

        /// <summary>候选标识（路线名，非排名）。</summary>
        public string CandidateId { get; }

        /// <summary>观察（事实陈述）。</summary>
        public string Observation { get; }

        /// <summary>假设（须成立的前提）。</summary>
        public string Assumption { get; }

        /// <summary>所需条件。</summary>
        public IReadOnlyList<string> RequiredConditions { get; }

        /// <summary>主要风险。</summary>
        public IReadOnlyList<string> Risks { get; }

        /// <summary>缺失情报（军师据能力识别出的、本建议依赖却未知的主题）。</summary>
        public IReadOnlyList<IntelSubjectId> MissingIntel { get; }

        /// <summary>依据置信（来源可靠性聚合 × 能力；非成功率）。</summary>
        public FixedPoint Confidence { get; }

        public AdviceStatement(
            AdvisorId advisor,
            string candidateId,
            string observation,
            string assumption,
            IReadOnlyList<string> requiredConditions,
            IReadOnlyList<string> risks,
            IReadOnlyList<IntelSubjectId> missingIntel,
            FixedPoint confidence)
        {
            Advisor = advisor;
            CandidateId = candidateId;
            Observation = observation ?? string.Empty;
            Assumption = assumption ?? string.Empty;
            RequiredConditions = requiredConditions ?? Array.Empty<string>();
            Risks = risks ?? Array.Empty<string>();
            MissingIntel = missingIntel ?? Array.Empty<IntelSubjectId>();
            Confidence = confidence;
        }
    }
}
