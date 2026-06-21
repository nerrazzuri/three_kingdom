// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 所有随机性经显式注入的确定性流（ADR-0004）——无隐式全局随机
// Date: 2026-06-21

using System;

namespace TkSlice.Domain.Numerics
{
    /// <summary>
    /// 确定性随机流。SplitMix64 内核：同种子 → 同序列，跨平台一致。
    /// Domain 禁止使用隐式全局随机；所有随机性经此显式注入。
    /// 可按具名子流派生（同一世界种子下不同用途互不串扰且可重放）。
    /// </summary>
    public sealed class DetRng
    {
        private ulong _state;

        public DetRng(ulong seed) => _state = seed;

        /// <summary>由父种子 + 用途名派生确定性子流（FNV-1a 混入名字）。</summary>
        public static DetRng Fork(ulong worldSeed, string streamName)
        {
            ulong h = 1469598103934665603UL; // FNV offset basis
            foreach (char c in streamName)
            {
                h ^= c;
                h *= 1099511628211UL; // FNV prime
            }
            // 混合世界种子，避免不同名字偶然碰撞
            return new DetRng(worldSeed ^ Mix(h));
        }

        private static ulong Mix(ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public ulong NextUInt64()
        {
            _state += 0x9E3779B97F4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>[0, maxExclusive) 的无偏整数（拒绝采样）。maxExclusive 必须 &gt; 0。</summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            ulong range = (ulong)maxExclusive;
            ulong limit = ulong.MaxValue - (ulong.MaxValue % range);
            ulong r;
            do { r = NextUInt64(); } while (r >= limit);
            return (int)(r % range);
        }

        /// <summary>[lo, hi] 含端整数。</summary>
        public int NextIntRange(int lo, int hi)
        {
            if (hi < lo) throw new ArgumentException("hi < lo");
            return lo + NextInt(hi - lo + 1);
        }

        /// <summary>[0,1) 的确定性定点单位值，用于概率判定。</summary>
        public Fixed NextFixedUnit()
        {
            // 取高 16 位映射到 [0, One)
            ulong r = NextUInt64();
            int raw = (int)(r >> 48) & (Fixed.One - 1);
            return Fixed.FromRaw(raw);
        }

        /// <summary>当前内部状态——用于状态哈希与存档。</summary>
        public ulong PeekState() => _state;
        public void SetState(ulong state) => _state = state;
    }
}
