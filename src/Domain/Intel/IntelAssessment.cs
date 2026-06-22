namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报评估结果（GDD_007 §Formula 2/3/4 / TR-intel-002）。
    /// 在某读取时刻对一份报告的三信号评估：置信信号（来源可靠性 + 时效）+ 估计区间。
    /// 不可变值；确定性（同报告 + 同读取时间 + 同配置 → 同评估）。
    /// </summary>
    public readonly struct IntelAssessment
    {
        /// <summary>置信多信号（来源可靠性 / 新鲜度 / 有效置信）。</summary>
        public ConfidenceSignals Signals { get; }

        /// <summary>估计区间（随时效变宽）。</summary>
        public EstimateInterval Interval { get; }

        public IntelAssessment(ConfidenceSignals signals, EstimateInterval interval)
        {
            Signals = signals;
            Interval = interval;
        }
    }
}
