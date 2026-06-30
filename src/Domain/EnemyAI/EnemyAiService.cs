using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>
    /// 敌方 AI 决策编排（ADR-0006 / GDD_016 §MVP）。把评分（<see cref="IActionScorer"/>）+ 种子化选择
    /// （<see cref="IActionSelector"/>）串成一次决策，产出 <see cref="DecisionRecord"/>（含缘由码，错误信念可读）。
    /// <para>纯决策，确定性：同 (种子+情报+性格+配置) → 同 DecisionRecord（随机经注入 <see cref="IDeterministicRandom"/>，不旁路）。</para>
    /// </summary>
    public sealed class EnemyAiService
    {
        private readonly IActionScorer _scorer;
        private readonly IActionSelector _selector;

        public EnemyAiService(IActionScorer? scorer = null, IActionSelector? selector = null)
        {
            _scorer = scorer ?? new ActionScorer();
            _selector = selector ?? new SoftmaxActionSelector();
        }

        /// <summary>决策一次：评分 → 种子化选择 → 记录（含缘由码）。</summary>
        public DecisionRecord Decide(
            AiWorldView view, PersonalityProfile personality, ScorerConfig config,
            FixedPoint temperature, IDeterministicRandom rng)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            if (personality is null) throw new ArgumentNullException(nameof(personality));
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            IReadOnlyList<ScoredAction> scored = _scorer.Score(view, personality, config);
            StrategicAction selected = _selector.Select(scored, temperature, rng);
            AiReasonCode reason = DetermineReason(scored, selected);
            return new DecisionRecord(selected, reason, scored, view.PerceivedEnemyForce);
        }

        /// <summary>缘由码判定：唯一可行 / 明显最优 / 抖动选出非最优（ADR-0006 §3 可读）。</summary>
        private static AiReasonCode DetermineReason(IReadOnlyList<ScoredAction> scored, StrategicAction selected)
        {
            int feasibleCount = 0;
            bool haveArgmax = false;
            StrategicAction argmax = selected;
            FixedPoint maxU = FixedPoint.Zero;
            foreach (ScoredAction s in scored)
            {
                if (!s.Feasible) continue;
                feasibleCount++;
                if (!haveArgmax || s.Utility > maxU)
                {
                    maxU = s.Utility;
                    argmax = s.Action;
                    haveArgmax = true;
                }
            }

            if (feasibleCount <= 1) return AiReasonCode.OnlyFeasible;
            return selected == argmax ? AiReasonCode.TopUtility : AiReasonCode.SoftmaxJitter;
        }
    }
}
