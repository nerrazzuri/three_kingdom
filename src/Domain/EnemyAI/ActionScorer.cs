using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>一个候选动作的效用评分结果（不可变）。Utility 为带符号定点；Feasible 经硬可行性门判定。</summary>
    public readonly struct ScoredAction
    {
        /// <summary>候选动作。</summary>
        public StrategicAction Action { get; }

        /// <summary>效用分（带符号定点；越高越倾向）。</summary>
        public FixedPoint Utility { get; }

        /// <summary>是否通过硬可行性门（false 则选择阶段淘汰，绝不选出）。</summary>
        public bool Feasible { get; }

        public ScoredAction(StrategicAction action, FixedPoint utility, bool feasible)
        {
            Action = action;
            Utility = utility;
            Feasible = feasible;
        }
    }

    /// <summary>
    /// 效用评分配置（GDD_016 §Balancing / ADR-0003 数据驱动）。不可变；权重版本化，方法体内不硬编码。
    /// </summary>
    public sealed class ScorerConfig
    {
        /// <summary>各动作基线效用。</summary>
        public FixedPoint BaseUtility { get; }

        /// <summary>兵力比对追击/撤退的权重。</summary>
        public FixedPoint ForceAdvantageWeight { get; }

        /// <summary>风险性格对进攻类（追击/诱敌）的权重。</summary>
        public FixedPoint RiskWeight { get; }

        /// <summary>耐心性格对坚守的权重。</summary>
        public FixedPoint PatienceWeight { get; }

        /// <summary>目标压力对坚守（守土）的权重。</summary>
        public FixedPoint UrgencyWeight { get; }

        /// <summary>追击可行的最低兵力比（己方/敌 ≥ 此值才可追）。</summary>
        public FixedPoint PursueFeasibleRatio { get; }

        public ScorerConfig(
            FixedPoint baseUtility, FixedPoint forceAdvantageWeight, FixedPoint riskWeight,
            FixedPoint patienceWeight, FixedPoint urgencyWeight, FixedPoint pursueFeasibleRatio)
        {
            if (forceAdvantageWeight < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(forceAdvantageWeight));
            if (riskWeight < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(riskWeight));
            if (patienceWeight < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(patienceWeight));
            if (urgencyWeight < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(urgencyWeight));
            if (pursueFeasibleRatio < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(pursueFeasibleRatio));
            BaseUtility = baseUtility;
            ForceAdvantageWeight = forceAdvantageWeight;
            RiskWeight = riskWeight;
            PatienceWeight = patienceWeight;
            UrgencyWeight = urgencyWeight;
            PursueFeasibleRatio = pursueFeasibleRatio;
        }
    }

    /// <summary>动作效用评分器（ADR-0006 §1）。</summary>
    public interface IActionScorer
    {
        /// <summary>对全部候选动作算定点效用 + 可行性门（纯函数，确定性）。</summary>
        IReadOnlyList<ScoredAction> Score(AiWorldView view, PersonalityProfile personality, ScorerConfig config);
    }

    /// <summary>
    /// 确定性效用评分（ADR-0006 §1 / TR-ai-003）。效用 = 态势 + 性格 + 目标叠加（全定点，无 float）。
    /// 硬可行性门淘汰不可行动作（如兵力不足却追击）；坚守为通用兜底恒可行。性格只影响权重，不给无条件效果。
    /// </summary>
    public sealed class ActionScorer : IActionScorer
    {
        public IReadOnlyList<ScoredAction> Score(AiWorldView view, PersonalityProfile personality, ScorerConfig config)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            if (personality is null) throw new ArgumentNullException(nameof(personality));
            if (config is null) throw new ArgumentNullException(nameof(config));

            int enemy = Math.Max(1, view.PerceivedEnemyForce);
            FixedPoint ratio = FixedPoint.FromInt(view.Own.Force) / FixedPoint.FromInt(enemy);
            FixedPoint risk = personality.Strength(PersonalityTrait.Risk);
            FixedPoint patience = personality.Strength(PersonalityTrait.Patience);
            bool knowsEnemy = view.PerceivedEnemyForce > 0;

            var list = new List<ScoredAction>(4);

            // 追击：兵力优势 + 风险性格 → 高效用；可行 = 知敌 ∧ 兵力比 ≥ 门槛。
            FixedPoint pursue = config.BaseUtility
                + config.ForceAdvantageWeight * (ratio - FixedPoint.One)
                + config.RiskWeight * risk;
            bool pursueFeasible = knowsEnemy && ratio >= config.PursueFeasibleRatio;
            list.Add(new ScoredAction(StrategicAction.Pursue, pursue, pursueFeasible));

            // 撤退：兵力劣势 → 高效用；可行 = 非守土目标（有退路）。
            FixedPoint retreat = config.BaseUtility
                + config.ForceAdvantageWeight * (FixedPoint.One - ratio);
            bool retreatFeasible = !view.Objective.MustDefend;
            list.Add(new ScoredAction(StrategicAction.Retreat, retreat, retreatFeasible));

            // 坚守：耐心 + 守土目标压力 → 高效用；恒可行（兜底）。
            FixedPoint hold = config.BaseUtility
                + config.PatienceWeight * patience
                + (view.Objective.MustDefend ? config.UrgencyWeight * view.Objective.Urgency : FixedPoint.Zero);
            list.Add(new ScoredAction(StrategicAction.Hold, hold, true));

            // 诱敌：风险性格主导 + 兵力相近加成；可行 = 风险性格 > 0（性格倾向）。
            FixedPoint feint = config.BaseUtility + config.RiskWeight * risk;
            bool feintFeasible = knowsEnemy && risk > FixedPoint.Zero;
            list.Add(new ScoredAction(StrategicAction.FeintLure, feint, feintFeasible));

            return list;
        }
    }
}
