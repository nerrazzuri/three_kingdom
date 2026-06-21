using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Environment
{
    /// <summary>
    /// 天气转移确定性解析器（GDD_002 §Formula 1 / TR-weather-001 / ADR-0004）。
    /// 从前态按配置权重选下一态：<c>next = first to where Σweight(0..to) > r×total</c>，
    /// 其中 r 来自<b>注入的确定性随机流</b>（[0,1) 定点），全程整数/定点比较，<b>禁 float</b>，跨平台逐位一致。
    /// <para>同一 (seed, position) + 同一前态 → 同一结果；每次解析使随机流位置前进 1（权威状态，可存档复盘）。</para>
    /// </summary>
    public sealed class WeatherResolver
    {
        private static readonly WeatherType[] CanonicalOrder = (WeatherType[])Enum.GetValues(typeof(WeatherType));

        private readonly WeatherTransitionTable _table;

        public WeatherResolver(WeatherTransitionTable table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        /// <summary>
        /// 解析 <paramref name="from"/> 的下一天气；从 <paramref name="rng"/> 抽取一个单位值定位。
        /// 无 from 转移行抛 <see cref="ArgumentException"/>（GDD §Failure：缺转移）。
        /// </summary>
        public WeatherType ResolveNext(WeatherType from, IDeterministicRandom rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (!_table.Has(from))
                throw new ArgumentException($"无天气转移行：from = {from}。", nameof(from));

            var row = _table.Row(from);
            int total = _table.Total(from); // 表已保证 > 0

            // 阈值 = r × total（避免除法；cumulative_sum(P) > r ⟺ cumWeight > r×total）。
            FixedPoint threshold = rng.NextUnit() * FixedPoint.FromInt(total);

            int cumulative = 0;
            WeatherType last = from;
            for (int i = 0; i < CanonicalOrder.Length; i++)
            {
                var to = CanonicalOrder[i];
                if (!row.TryGetValue(to, out int w) || w == 0) continue;
                cumulative += w;
                last = to;
                if (FixedPoint.FromInt(cumulative) > threshold)
                    return to;
            }

            // 理论不可达：cumulative 终值 == total，FromInt(total) > r×total 恒真（r<1）。兜底返回最后一个有权重态。
            return last;
        }
    }
}
