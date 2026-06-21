using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Environment
{
    /// <summary>
    /// 天气转移权重表（GDD_002 §Formula 1 / TR-weather-001）。配置驱动：<c>W[from][to] ≥ 0</c>，权重不硬编码。
    /// 构造时校验（GDD §Failure：非法权重/缺转移阻止加载）：权重非负；任一声明的 from 行权重和必须 &gt; 0。
    /// 构造后不可变（防御性拷贝）。
    /// </summary>
    public sealed class WeatherTransitionTable
    {
        private readonly Dictionary<WeatherType, Dictionary<WeatherType, int>> _weights;
        private readonly Dictionary<WeatherType, int> _totals;

        /// <param name="weights">from → (to → 权重)。每个权重 ≥0；每个声明的 from 行总和 &gt;0。</param>
        public WeatherTransitionTable(IReadOnlyDictionary<WeatherType, IReadOnlyDictionary<WeatherType, int>> weights)
        {
            if (weights == null) throw new ArgumentNullException(nameof(weights));

            _weights = new Dictionary<WeatherType, Dictionary<WeatherType, int>>();
            _totals = new Dictionary<WeatherType, int>();

            foreach (var fromRow in weights)
            {
                var row = new Dictionary<WeatherType, int>();
                long total = 0;
                foreach (var kv in fromRow.Value)
                {
                    if (kv.Value < 0)
                        throw new ArgumentException($"天气转移权重不可为负：{fromRow.Key}→{kv.Key} = {kv.Value}。", nameof(weights));
                    row[kv.Key] = kv.Value;
                    total += kv.Value;
                }
                if (total <= 0)
                    throw new ArgumentException($"天气转移行权重和必须 > 0：from = {fromRow.Key}。", nameof(weights));

                _weights[fromRow.Key] = row;
                _totals[fromRow.Key] = checked((int)total);
            }
        }

        /// <summary>是否含 from 的转移行。</summary>
        public bool Has(WeatherType from) => _weights.ContainsKey(from);

        /// <summary>取 from 行（to → 权重，只读）；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public IReadOnlyDictionary<WeatherType, int> Row(WeatherType from)
        {
            if (_weights.TryGetValue(from, out var row)) return row;
            throw new KeyNotFoundException($"无天气转移行：from = {from}。");
        }

        /// <summary>from 行的权重总和（&gt;0）。</summary>
        public int Total(WeatherType from)
        {
            if (_totals.TryGetValue(from, out var t)) return t;
            throw new KeyNotFoundException($"无天气转移行：from = {from}。");
        }
    }
}
