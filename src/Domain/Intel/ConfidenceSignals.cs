using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 置信多信号表达（GDD_007 §Formula 2/4 / TR-intel-002 / P2 可信度多维）。
    /// <b>刻意非单一百分比</b>：分别承载来源可靠性、时效新鲜度与其合成的有效置信，
    /// 配合估计区间共同表达「这条情报多可信」——UI 不得折叠为单一「75% 可信」。
    /// 置信表达<b>来源可靠性</b>而非系统公开的真实概率。不可变值。
    /// </summary>
    public readonly struct ConfidenceSignals
    {
        /// <summary>情报来源。</summary>
        public IntelSource Source { get; }

        /// <summary>来源可靠性 base_conf（[0,1]，非真实概率）。</summary>
        public FixedPoint SourceReliability { get; }

        /// <summary>自观察以来的时段数 age（≥0）。</summary>
        public int Age { get; }

        /// <summary>时效新鲜度 freshness = clamp(1 − age/ttl)（[0,1]，权威归 GDD_007）。</summary>
        public FixedPoint Freshness { get; }

        /// <summary>有效置信 effective_conf = 来源可靠性 × 新鲜度（[0,1]）。</summary>
        public FixedPoint EffectiveConfidence { get; }

        /// <summary>是否已过时效（freshness 降为 0；不删除报告，仅标记）。</summary>
        public bool Expired => Freshness == FixedPoint.Zero;

        public ConfidenceSignals(IntelSource source, FixedPoint sourceReliability, int age, FixedPoint freshness, FixedPoint effectiveConfidence)
        {
            Source = source;
            SourceReliability = sourceReliability;
            Age = age;
            Freshness = freshness;
            EffectiveConfidence = effectiveConfidence;
        }
    }
}
