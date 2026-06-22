using System;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报评估服务（GDD_007 §Formula 2/3/4 / TR-intel-002 / ADR-0004）。
    /// 纯函数、确定性：在某读取时刻评估一份报告，产出三信号（来源可靠性 + 时效衰减）+ 估计区间。
    /// <b>时效衰减权威归本系统（GDD_007）</b>——以报告的 observed_time 与当前时段计 age（systems-index C-W2）。
    /// 置信/区间全程定点，权威路径无 float。
    /// </summary>
    public sealed class IntelAssessmentService
    {
        /// <summary>
        /// 评估报告（GDD_007 §Formula 4：freshness=clamp(1−age/ttl)；effective=reliability×freshness；
        /// §Formula 3：half_width=round(base_error×(1−effective))，区间随时效变宽）。
        /// </summary>
        public IntelAssessment Assess(IntelReport report, WorldTime now, IntelConfig config)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (now < report.ObservedAt) throw new ArgumentException("读取时间不可早于观察时间。", nameof(now));

            int age = checked((int)(now.AbsoluteIndex - report.ObservedAt.AbsoluteIndex));

            FixedPoint reliability = config.SourceReliability(report.Source);
            FixedPoint freshness = (FixedPoint.One - FixedPoint.FromFraction(age, config.TtlSegments))
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint effective = reliability * freshness;

            var signals = new ConfidenceSignals(report.Source, reliability, age, freshness, effective);

            int halfWidth = (FixedPoint.FromInt(config.BaseError) * (FixedPoint.One - effective)).RoundToInt();
            if (halfWidth < 0) halfWidth = 0;
            var interval = new EstimateInterval(report.ReportedStrength, halfWidth);

            return new IntelAssessment(signals, interval);
        }
    }
}
