using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 命令执行意愿计算（GDD_005 §Formula 2 / TR-character-002）：
    /// <c>willingness = clamp(base_will + Σ_t w_trait[t]·trait[t] + relation_term, 0, 1)</c>。
    /// <para>
    /// 关系项 <c>relation_term = coop_score × relation_weight</c>，coop_score 为 GDD_006 <b>已结算</b>值
    /// （破环顺序：关系事件结算 → 人物意愿；本计算只读已结算值，不触关系本体，AC-4）。
    /// 全程定点、纯函数 → 同输入同输出可复现（ADR-0004）。意愿低不等于拒绝执行，而是降质量/要条件（可解释，非随机）。
    /// </para>
    /// </summary>
    public static class WillingnessCalculator
    {
        /// <param name="baseWill">基础意愿 ∈ [0,1]（配置）。</param>
        /// <param name="personality">人物性格档案。</param>
        /// <param name="traitWeights">该决策对各性格倾向的权重（配置；可正可负）。null 视为空。</param>
        /// <param name="coopScore">关系已结算协作分（GDD_006 产出，注入）。</param>
        /// <param name="relationWeight">关系项权重（配置）。</param>
        /// <returns>意愿 ∈ [0,1]。</returns>
        public static FixedPoint Compute(
            FixedPoint baseWill,
            PersonalityProfile personality,
            IReadOnlyDictionary<PersonalityTrait, FixedPoint>? traitWeights,
            FixedPoint coopScore,
            FixedPoint relationWeight)
        {
            if (personality == null) throw new ArgumentNullException(nameof(personality));
            if (baseWill < FixedPoint.Zero || baseWill > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(baseWill), "基础意愿须 ∈ [0,1]。");

            FixedPoint sum = baseWill;

            if (traitWeights != null)
            {
                foreach (var kv in traitWeights)
                    sum += kv.Value * personality.Strength(kv.Key);
            }

            sum += coopScore * relationWeight; // 关系项（读已结算 coop_score）

            return Clamp01(sum);
        }

        private static FixedPoint Clamp01(FixedPoint v)
        {
            if (v < FixedPoint.Zero) return FixedPoint.Zero;
            if (v > FixedPoint.One) return FixedPoint.One;
            return v;
        }
    }
}
