using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 军议服务（GDD_008 §Formula 1/2/4 / TR-council-001/002 / ADR-0002 + 强制设计锁 P11）。
    /// 纯函数、确定性：读召开时<b>只读阵营知识投影</b>（不触世界真值），将合法知识整理为
    /// <b>条件化建议</b>。绝不输出综合成功率、唯一推荐或自动命令；建议绑定知识快照 ID，
    /// 变化后过时不静默更新。置信源自依据可靠性聚合（取最弱链路）× 能力，不凭空提升。
    /// </summary>
    public sealed class WarCouncilService
    {
        /// <summary>
        /// 召开军议，生成条件化建议集（GDD_008）。
        /// </summary>
        /// <param name="snapshotId">召开时冻结的知识快照 ID。</param>
        /// <param name="knowledge">只读阵营知识投影（GDD_007 第 4 层；唯一合法信息来源，禁触真值）。</param>
        /// <param name="claimConfidences">已知主题的有效置信（来自 GDD_007 effective_conf，调用方据 Story 002 评估提供）。</param>
        /// <param name="advisor">军师视角（能力影响缺口发现与置信）。</param>
        /// <param name="candidates">候选论证模板（数据驱动）。</param>
        /// <param name="config">军议平衡配置。</param>
        public CouncilAdviceSet Convene(
            KnowledgeSnapshotId snapshotId,
            IntelProjection knowledge,
            IReadOnlyDictionary<IntelSubjectId, FixedPoint> claimConfidences,
            AdvisorPerspective advisor,
            IReadOnlyList<AdviceTemplate> candidates,
            CouncilConfig config)
        {
            if (knowledge == null) throw new ArgumentNullException(nameof(knowledge));
            if (claimConfidences == null) throw new ArgumentNullException(nameof(claimConfidences));
            if (advisor == null) throw new ArgumentNullException(nameof(advisor));
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var advice = new List<AdviceStatement>(candidates.Count);
            foreach (AdviceTemplate template in candidates)
            {
                IReadOnlyList<IntelSubjectId> missing = DetectMissingIntel(template, knowledge, advisor, config);
                FixedPoint confidence = AggregateConfidence(template, knowledge, claimConfidences, advisor);

                advice.Add(new AdviceStatement(
                    advisor.Advisor,
                    template.CandidateId,
                    template.Observation,
                    template.Assumption,
                    template.RequiredConditions,
                    template.Risks,
                    missing,
                    confidence));
            }

            return new CouncilAdviceSet(snapshotId, advice);
        }

        /// <summary>
        /// 缺口识别（GDD_008 §Formula 1）：客观缺失情报中，按能力识别出 round(exist×clamp(w_gap×adv_cap)) 条，
        /// 不无中生有。低能力可能遗漏；高能力接近识别全部。
        /// </summary>
        private static IReadOnlyList<IntelSubjectId> DetectMissingIntel(
            AdviceTemplate template, IntelProjection knowledge, AdvisorPerspective advisor, CouncilConfig config)
        {
            var objectiveGaps = new List<IntelSubjectId>();
            foreach (IntelSubjectId subject in template.ReferencedSubjects)
                if (!knowledge.Knows(subject))
                    objectiveGaps.Add(subject);

            if (objectiveGaps.Count == 0) return objectiveGaps;

            FixedPoint factor = (config.GapDetectionWeight * advisor.Capability).Clamp(FixedPoint.Zero, FixedPoint.One);
            int detected = (FixedPoint.FromInt(objectiveGaps.Count) * factor).RoundToInt();
            if (detected < 0) detected = 0;
            if (detected > objectiveGaps.Count) detected = objectiveGaps.Count;

            return objectiveGaps.GetRange(0, detected);
        }

        /// <summary>
        /// 建议置信（GDD_008 §Formula 2）：min_i(claim_conf[i]) × adv_cap——结论不强于最弱依据。
        /// 仅聚合已知且有置信记录的依据；无任何已知依据则置信为 0（不凭空提升）。
        /// </summary>
        private static FixedPoint AggregateConfidence(
            AdviceTemplate template,
            IntelProjection knowledge,
            IReadOnlyDictionary<IntelSubjectId, FixedPoint> claimConfidences,
            AdvisorPerspective advisor)
        {
            FixedPoint weakest = FixedPoint.One;
            bool any = false;
            foreach (IntelSubjectId subject in template.ReferencedSubjects)
            {
                if (!knowledge.Knows(subject)) continue;
                if (!claimConfidences.TryGetValue(subject, out FixedPoint conf)) continue;
                any = true;
                if (conf < weakest) weakest = conf;
            }

            return any ? weakest * advisor.Capability : FixedPoint.Zero;
        }
    }
}
