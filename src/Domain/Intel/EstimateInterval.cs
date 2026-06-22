namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 估计区间（GDD_007 §Formula 3 / TR-intel-002）。
    /// 报告给出区间而非点值：half_width = base_error ×(1 − effective_conf)；区间随时效衰减
    /// （有效置信下降）<b>确定性变宽</b>。真值落在区间内是<b>倾向</b>而非保证。不可变值。
    /// </summary>
    public readonly struct EstimateInterval
    {
        /// <summary>区间下界（夹至 ≥0）。</summary>
        public int Lower { get; }

        /// <summary>观察中心值。</summary>
        public int Center { get; }

        /// <summary>区间上界。</summary>
        public int Upper { get; }

        /// <summary>半宽（随有效置信反向变化）。</summary>
        public int HalfWidth { get; }

        public EstimateInterval(int center, int halfWidth)
        {
            Center = center;
            HalfWidth = halfWidth;
            int lower = center - halfWidth;
            Lower = lower < 0 ? 0 : lower;
            Upper = center + halfWidth;
        }
    }
}
