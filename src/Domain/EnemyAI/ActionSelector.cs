using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>种子化动作选择器（ADR-0006 §1）。</summary>
    public interface IActionSelector
    {
        /// <summary>
        /// 从评分动作中以种子化 softmax 风格抽样选一个（只在 <see cref="ScoredAction.Feasible"/> 间抽样，
        /// 绝不选被淘汰动作）。经注入 <see cref="IDeterministicRandom"/> 抽样——同种子同选择。
        /// </summary>
        StrategicAction Select(IReadOnlyList<ScoredAction> scored, FixedPoint temperature, IDeterministicRandom rng);
    }

    /// <summary>
    /// 种子化 softmax 风格选择（ADR-0006 §1 / TR-ai-002/003）。
    /// <list type="bullet">
    ///   <item><b>只在可行动作间抽样</b>——硬可行性门淘汰者绝不选出。</item>
    ///   <item>温度调节锐度：低温趋 argmax（抖动只在效用接近者间），高温趋均匀（分布趋平，单调）。</item>
    ///   <item>经注入 <see cref="IDeterministicRandom"/>（ADR-0004 流，不旁路）——同种子同选择、可复现。</item>
    /// </list>
    /// 定点实现：以"基线 + 效用差 × (1/温度)"作权重的累积分布抽样（避免定点 exp，保留 softmax 的温度单调性质）。
    /// </summary>
    public sealed class SoftmaxActionSelector : IActionSelector
    {
        public StrategicAction Select(IReadOnlyList<ScoredAction> scored, FixedPoint temperature, IDeterministicRandom rng)
        {
            if (scored is null) throw new ArgumentNullException(nameof(scored));
            if (rng is null) throw new ArgumentNullException(nameof(rng));
            if (temperature <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(temperature), "温度须为正。");

            // 只取可行动作，按动作枚举稳定排序（确定性）。
            var feasible = new List<ScoredAction>();
            foreach (ScoredAction s in scored) if (s.Feasible) feasible.Add(s);
            feasible.Sort((a, b) => ((int)a.Action).CompareTo((int)b.Action));

            if (feasible.Count == 0) throw new InvalidOperationException("无可行动作可选（调用方须保证至少一个可行）。");
            if (feasible.Count == 1) return feasible[0].Action;

            FixedPoint minU = feasible[0].Utility;
            foreach (ScoredAction s in feasible) if (s.Utility < minU) minU = s.Utility;

            FixedPoint invTemp = FixedPoint.One / temperature;   // 温度越低 → 权重差异越大（锐）
            var weights = new FixedPoint[feasible.Count];
            FixedPoint total = FixedPoint.Zero;
            for (int i = 0; i < feasible.Count; i++)
            {
                // 基线 1 保证每动作非零概率；效用差 × invTemp 拉开对比（温度单调）。
                FixedPoint w = FixedPoint.One + (feasible[i].Utility - minU) * invTemp;
                weights[i] = w;
                total += w;
            }

            FixedPoint r = rng.NextUnit() * total;   // 落在 [0, total)
            FixedPoint cum = FixedPoint.Zero;
            for (int i = 0; i < feasible.Count; i++)
            {
                cum += weights[i];
                if (r < cum) return feasible[i].Action;
            }
            return feasible[feasible.Count - 1].Action;   // 数值边界兜底
        }
    }
}
