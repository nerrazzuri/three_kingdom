using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 晋升梯队与生涯累积的版本化配置（GDD_014 §Formula 1 / §Tuning Knobs / TR-career-002 / ADR-0003）。
    /// 不可变；构造时校验范围与按阶递增，非法即抛、无部分写入。所有门槛/权重<b>来自配置</b>，判定逻辑不硬编码。
    /// <para>
    /// 门槛数组按<b>目标官阶</b>索引（<c>MeritReq[targetRank]</c> = 晋升至 targetRank 所需），长度 = 官阶数（8，rank 0–7）。
    /// rank 0（太守）门槛通常为 0（开局即此阶）。<see cref="ConfigFingerprint"/> 为配置指纹，纳入存档校验（story-005）。
    /// </para>
    /// </summary>
    public sealed class PromotionLadderConfig
    {
        /// <summary>官阶数（= <see cref="Rank"/> 枚举值数）。</summary>
        public static readonly int RankCount = Enum.GetValues(typeof(Rank)).Length;

        private readonly int[] _meritReq;
        private readonly int[] _renownReq;
        private readonly FixedPoint[] _standingReq;
        private readonly Dictionary<CareerGainSource, CareerGain> _gains;

        /// <summary>各目标官阶所需功绩（按阶非递减）。</summary>
        public IReadOnlyList<int> MeritReq => _meritReq;

        /// <summary>各目标官阶所需名望（按阶非递减）。</summary>
        public IReadOnlyList<int> RenownReq => _renownReq;

        /// <summary>各目标官阶所需君主好感（定点 ∈[0,1]，按阶非递减）。</summary>
        public IReadOnlyList<FixedPoint> StandingReq => _standingReq;

        /// <summary>配置指纹（确定性哈希，ADR-0003）。</summary>
        public StateHash ConfigFingerprint { get; }

        public PromotionLadderConfig(
            IReadOnlyList<int> meritReq,
            IReadOnlyList<int> renownReq,
            IReadOnlyList<FixedPoint> standingReq,
            IReadOnlyDictionary<CareerGainSource, CareerGain> gains)
        {
            if (meritReq is null) throw new ArgumentNullException(nameof(meritReq));
            if (renownReq is null) throw new ArgumentNullException(nameof(renownReq));
            if (standingReq is null) throw new ArgumentNullException(nameof(standingReq));
            if (gains is null) throw new ArgumentNullException(nameof(gains));

            if (meritReq.Count != RankCount) throw new ArgumentException($"merit_req 长度须为官阶数 {RankCount}。", nameof(meritReq));
            if (renownReq.Count != RankCount) throw new ArgumentException($"renown_req 长度须为官阶数 {RankCount}。", nameof(renownReq));
            if (standingReq.Count != RankCount) throw new ArgumentException($"standing_req 长度须为官阶数 {RankCount}。", nameof(standingReq));

            _meritReq = ValidateMonotonicInts(meritReq, nameof(meritReq));
            _renownReq = ValidateMonotonicInts(renownReq, nameof(renownReq));
            _standingReq = ValidateMonotonicStanding(standingReq, nameof(standingReq));

            _gains = new Dictionary<CareerGainSource, CareerGain>();
            foreach (KeyValuePair<CareerGainSource, CareerGain> kv in gains)
            {
                if (kv.Value is null) throw new ArgumentException($"来源 {kv.Key} 增益不可为空。", nameof(gains));
                _gains[kv.Key] = kv.Value;
            }

            ConfigFingerprint = ComputeFingerprint();
        }

        /// <summary>查询某来源的单次增益；未配置返回 null。</summary>
        public CareerGain? GainFor(CareerGainSource source)
            => _gains.TryGetValue(source, out CareerGain? g) ? g : null;

        /// <summary>
        /// W5 非战斗速率护栏：非战斗来源的最大单次功绩增益 ≥ 战斗来源最大单次功绩增益 × <paramref name="minRatio"/>。
        /// 用于防"刷战斗支配"——非战斗源须在配置上有竞争力。无任一类来源时视为满足（空集不破坏护栏）。
        /// </summary>
        public bool SatisfiesNonCombatGuardrail(FixedPoint minRatio)
        {
            int maxCombat = 0;
            int maxNonCombat = 0;
            bool hasCombat = false;
            foreach (KeyValuePair<CareerGainSource, CareerGain> kv in _gains)
            {
                if (CareerGainSources.IsCombatSource(kv.Key))
                {
                    hasCombat = true;
                    if (kv.Value.Merit > maxCombat) maxCombat = kv.Value.Merit;
                }
                else if (kv.Value.Merit > maxNonCombat)
                {
                    maxNonCombat = kv.Value.Merit;
                }
            }
            if (!hasCombat) return true;
            // maxNonCombat ≥ maxCombat × minRatio （定点比较，权威路径无 float）
            FixedPoint required = FixedPoint.FromInt(maxCombat) * minRatio;
            return FixedPoint.FromInt(maxNonCombat) >= required;
        }

        private static int[] ValidateMonotonicInts(IReadOnlyList<int> source, string paramName)
        {
            var arr = new int[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] < 0) throw new ArgumentOutOfRangeException(paramName, "门槛不可为负。");
                if (i > 0 && source[i] < source[i - 1])
                    throw new ArgumentException($"{paramName} 须按阶非递减（rank+1 门槛 ≥ rank）。", paramName);
                arr[i] = source[i];
            }
            return arr;
        }

        private static FixedPoint[] ValidateMonotonicStanding(IReadOnlyList<FixedPoint> source, string paramName)
        {
            var arr = new FixedPoint[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] < FixedPoint.Zero || source[i] > FixedPoint.One)
                    throw new ArgumentOutOfRangeException(paramName, "standing_req 须在 [0,1]。");
                if (i > 0 && source[i] < source[i - 1])
                    throw new ArgumentException($"{paramName} 须按阶非递减。", paramName);
                arr[i] = source[i];
            }
            return arr;
        }

        private StateHash ComputeFingerprint()
        {
            var hasher = new StateHasher();
            for (int i = 0; i < RankCount; i++)
            {
                hasher.Append(_meritReq[i]);
                hasher.Append(_renownReq[i]);
                hasher.Append(_standingReq[i]);
            }
            // 来源按枚举序数稳定遍历，保证指纹确定性。
            foreach (CareerGainSource source in (CareerGainSource[])Enum.GetValues(typeof(CareerGainSource)))
            {
                if (!_gains.TryGetValue(source, out CareerGain? g)) continue;
                hasher.Append((int)source);
                hasher.Append(g.Merit);
                hasher.Append(g.Renown);
                hasher.Append(g.Standing);
            }
            return hasher.ToHash();
        }
    }
}
