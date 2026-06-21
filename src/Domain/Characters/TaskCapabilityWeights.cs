using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 任务对各能力域的权重（GDD_005 §Formula 1：w_cap[k]，配置「任务档案」）。
    /// 以<b>整数相对权重</b>表达（≥0，总和&gt;0），过程质量计算时按总和归一化——
    /// 等价于 GDD 的 Σw=1 形式，但避免定点分数和的精度问题，保持确定性。不可变。
    /// </summary>
    public sealed class TaskCapabilityWeights
    {
        private readonly Dictionary<CapabilityDomain, int> _weights;

        /// <summary>权重总和（&gt;0），用作归一化分母。</summary>
        public int Total { get; }

        /// <param name="weights">能力域 → 相对权重（≥0）。总和须 &gt;0。未声明域权重视为 0。</param>
        public TaskCapabilityWeights(IReadOnlyDictionary<CapabilityDomain, int> weights)
        {
            if (weights == null) throw new ArgumentNullException(nameof(weights));
            _weights = new Dictionary<CapabilityDomain, int>();
            long total = 0;
            foreach (var kv in weights)
            {
                if (kv.Value < 0)
                    throw new ArgumentException($"能力权重不可为负：{kv.Key}={kv.Value}。", nameof(weights));
                _weights[kv.Key] = kv.Value;
                total += kv.Value;
            }
            if (total <= 0)
                throw new ArgumentException("能力权重总和须 > 0。", nameof(weights));
            Total = checked((int)total);
        }

        /// <summary>取能力域权重；未声明则 0。</summary>
        public int Weight(CapabilityDomain domain) => _weights.TryGetValue(domain, out int w) ? w : 0;
    }
}
