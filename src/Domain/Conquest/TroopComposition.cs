using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 兵种编成（GDD_019 D4，<b>杠杆非克制</b>）：把可投入兵力按兵种分配的数量。不可变。
    /// 各兵种数 ≥ 0；总和须 ≤ 出征投入兵力（由 <see cref="OffensivePreparation"/> 构造时校验）。
    /// 份额（某兵种 / 总投入）是路线条件门与战力契合的输入；<b>不含任何兵种克制关系</b>（ADR-0011 D3）。
    /// </summary>
    public sealed class TroopComposition
    {
        private readonly IReadOnlyDictionary<TroopType, int> _counts;

        /// <summary>各兵种数量（只读；缺省视为 0）。</summary>
        public IReadOnlyDictionary<TroopType, int> Counts => _counts;

        /// <summary>各兵种数量之和。</summary>
        public int Total { get; }

        /// <summary>构造并校验各兵种数非负。</summary>
        public TroopComposition(IReadOnlyDictionary<TroopType, int>? counts)
        {
            var copy = new Dictionary<TroopType, int>();
            int total = 0;
            if (counts != null)
            {
                foreach (KeyValuePair<TroopType, int> kv in counts)
                {
                    if (kv.Value < 0)
                        throw new ArgumentOutOfRangeException(nameof(counts), "兵种数不可为负。");
                    if (kv.Value == 0) continue;
                    copy[kv.Key] = kv.Value;
                    total = checked(total + kv.Value);
                }
            }
            _counts = copy;
            Total = total;
        }

        /// <summary>某兵种数量（缺省 0）。</summary>
        public int Count(TroopType type) => _counts.TryGetValue(type, out int v) ? v : 0;

        /// <summary>全步卒的简易编成（未细分兵种时的默认，用于纯战力路线）。</summary>
        public static TroopComposition AllInfantry(int troops)
            => new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Infantry] = troops });

        /// <summary>空编成（不细分兵种；份额均为 0，兵种门不成立）。</summary>
        public static TroopComposition None { get; } = new TroopComposition(null);
    }
}
