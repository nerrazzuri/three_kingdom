using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;

namespace ThreeKingdom.Domain.Cohesion
{
    /// <summary>
    /// 凝聚力结算服务（GDD_011 §Formula 1/4/5 / TR-cohesion-001/002 / ADR-0004）。
    /// 纯函数、确定性。<b>011 为 morale/fatigue 唯一施加点</b>：消费 GDD_012 断粮事件、聚合士气事件；
    /// GDD_010 只读结算值不再施加。士气事件按受众权重聚合且<b>幂等</b>（同 ID 不重复结算）；
    /// 阈值检查综合军纪/指挥/退路（非单一士气）；拆分/合并按<b>人数加权</b>（非取最大）。
    /// 三维独立——各方法只动其声明维度。全程定点。
    /// </summary>
    public sealed class CohesionService
    {
        /// <summary>
        /// 应用一批士气事件（GDD_011 §Formula 1）：按受众权重聚合 + 指挥影响（封顶），幂等去重。
        /// 只改士气；疲劳/军纪不变（三维独立）。
        /// </summary>
        public CohesionState ApplyMoraleEvents(
            CohesionState state, IReadOnlyList<MoraleEvent> events, FixedPoint commandInfluence, CohesionConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var seen = new HashSet<MoraleEventId>();
            FixedPoint delta = FixedPoint.Zero;
            foreach (MoraleEvent e in events)
            {
                if (!seen.Add(e.Id)) continue; // 幂等：同一事件只结算一次
                delta += e.AudienceWeight * e.Intensity;
            }

            FixedPoint cmd = commandInfluence < config.CommandInfluenceCap ? commandInfluence : config.CommandInfluenceCap;
            FixedPoint morale = (state.Morale + delta + cmd).Clamp(FixedPoint.Zero, FixedPoint.One);
            return state.WithMorale(morale);
        }

        /// <summary>
        /// 消费断粮后果事件施加 morale/fatigue（GDD_011 §Formula 2 / GDD_012 §5：011 唯一施加点）。
        /// 按累计短缺时段逐步施加；军纪不变（三维独立）。GDD_010 不得再施加同一惩罚。
        /// </summary>
        public CohesionState ApplySupplyCutoff(CohesionState state, SupplyCutoffEvent cutoff, CohesionConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (cutoff == null) throw new ArgumentNullException(nameof(cutoff));
            if (config == null) throw new ArgumentNullException(nameof(config));

            FixedPoint segments = FixedPoint.FromInt(cutoff.ShortageSegments);
            FixedPoint morale = (state.Morale - config.SupplyMoralePenaltyPerSegment * segments).Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint fatigue = (state.Fatigue + config.SupplyFatiguePerSegment * segments).Clamp(FixedPoint.Zero, FixedPoint.One);
            return new CohesionState(state.Unit, state.Headcount, morale, fatigue, state.Discipline);
        }

        /// <summary>
        /// 阈值检查（GDD_011 §Formula 4）：综合士气、军纪、指挥、退路<b>多输入</b>判定——
        /// 单一数值不自动决定。溃散需士气低 + 维持不足 + 随机检查；高士气但军纪崩/无退路仍可动摇。
        /// </summary>
        public CohesionStatus EvaluateThreshold(
            CohesionState state, FixedPoint commandQuality, FixedPoint routeClear, IDeterministicRandom random, CohesionConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (config == null) throw new ArgumentNullException(nameof(config));
            RequireUnit(commandQuality, nameof(commandQuality));
            RequireUnit(routeClear, nameof(routeClear));

            FixedPoint holdScore = state.Discipline * commandQuality * routeClear;
            bool maintainBroken = holdScore < config.HoldThreshold;
            bool lowMorale = state.Morale < config.MoraleFloor;

            if (lowMorale && maintainBroken && random.NextUnit() < config.RoutProbability)
                return CohesionStatus.Routed;
            if (maintainBroken || lowMorale)
                return CohesionStatus.Wavering;
            return CohesionStatus.Steady;
        }

        /// <summary>
        /// 拆分/合并三维状态（GDD_011 §Formula 5）：按人数加权平均，<b>非取最大</b>。
        /// </summary>
        public CohesionState Merge(UnitId mergedId, IReadOnlyList<CohesionState> parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            if (parts.Count == 0) throw new ArgumentException("合并部件不可为空。", nameof(parts));

            long totalHeadcount = 0;
            foreach (CohesionState p in parts) totalHeadcount += p.Headcount;
            if (totalHeadcount <= 0) throw new InvalidOperationException("合并总人数须为正。");

            FixedPoint total = FixedPoint.FromInt(checked((int)totalHeadcount));
            FixedPoint mSum = FixedPoint.Zero, fSum = FixedPoint.Zero, dSum = FixedPoint.Zero;
            foreach (CohesionState p in parts)
            {
                FixedPoint w = FixedPoint.FromInt(p.Headcount);
                mSum += w * p.Morale;
                fSum += w * p.Fatigue;
                dSum += w * p.Discipline;
            }

            return new CohesionState(mergedId, checked((int)totalHeadcount),
                (mSum / total).Clamp(FixedPoint.Zero, FixedPoint.One),
                (fSum / total).Clamp(FixedPoint.Zero, FixedPoint.One),
                (dSum / total).Clamp(FixedPoint.Zero, FixedPoint.One));
        }

        private static void RequireUnit(FixedPoint v, string n)
        { if (v < FixedPoint.Zero || v > FixedPoint.One) throw new ArgumentOutOfRangeException(n, "须在 [0,1]。"); }
    }
}
