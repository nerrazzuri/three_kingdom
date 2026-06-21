using System;

namespace ThreeKingdom.Domain.Numerics
{
    /// <summary>
    /// 确定性随机流实现，基于 SplitMix64（ADR-0004 支柱 2）。状态完全由 (seed, position) 决定，
    /// 不依赖系统熵/时间/帧率，跨平台逐位一致。位置即权威状态：以 (seed, position) 重建即可精确续抽，
    /// 支撑存档 round-trip 后续推进一致（GDD_013 / epic-009）。
    /// <para>无符号算术在 C# 默认 unchecked 上下文下按 2^64 取模回绕——这是 SplitMix64 的预期语义。</para>
    /// </summary>
    public sealed class DeterministicRandom : IDeterministicRandom
    {
        private const ulong Gamma = 0x9E3779B97F4A7C15UL;

        private readonly ulong _seed;
        private ulong _position;

        /// <summary>以种子（可选自指定位置）构造。position 用于从存档恢复流游标。</summary>
        public DeterministicRandom(ulong seed, ulong position = 0UL)
        {
            _seed = seed;
            _position = position;
        }

        /// <summary>本流种子（权威状态的一半）。</summary>
        public ulong Seed => _seed;

        /// <inheritdoc/>
        public ulong Position => _position;

        /// <inheritdoc/>
        public ulong NextBits()
        {
            // 前进位置，再由 (seed, position) 纯函数式导出当前值——故 (seed, position) 可完全重建序列。
            _position++;
            ulong z = _seed + _position * Gamma;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <inheritdoc/>
        public FixedPoint NextUnit()
        {
            // 取高 16 位作为 Q16.16 小数位 → [0, 65535]/65536 ∈ [0, 1)。
            int raw = (int)(NextBits() >> 48);
            return FixedPoint.FromRaw(raw);
        }

        /// <inheritdoc/>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentException("DeterministicRandom.NextInt：maxExclusive 必须大于 minInclusive。");

            ulong range = (ulong)((long)maxExclusive - minInclusive);
            // 取模偏置在 MVP 区间（小 range）可忽略；确定性优先。如需无偏可后续加 rejection sampling。
            ulong value = NextBits() % range;
            return minInclusive + (int)value;
        }
    }
}
