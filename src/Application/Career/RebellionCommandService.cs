using System;
using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Application.Career
{
    /// <summary>
    /// 自立线的 Application 写路径（GDD_014 / TR-career-002 / ADR-0002）。持有版本化 <see cref="RebellionConfig"/>，
    /// 把"查可否自立 / 发动自立（带二次确认意图）"委派 Domain <see cref="RebellionService"/>。
    /// 发动为显式命令——UI 须先 <see cref="CanRebel"/> 展示条件满足度与结局风险预览，再二次确认调 <see cref="Launch"/>。
    /// </summary>
    public sealed class RebellionCommandService
    {
        private readonly RebellionConfig _config;
        private readonly RebellionService _domain;

        public RebellionCommandService(RebellionConfig config, RebellionService domain)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        }

        /// <summary>默认构造：自建 Domain 服务。</summary>
        public RebellionCommandService(RebellionConfig config) : this(config, new RebellionService()) { }

        /// <summary>查自立可发动判定（三组条件满足度），不改状态。</summary>
        public RebellionEligibility CanRebel(CareerSnapshot current, RebellionContext context)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            return _domain.CheckEligibility(_config, current.Career, current.Retinue, context);
        }

        /// <summary>发动自立（二次确认后）。未满足条件返回稳定错误码且状态不变。</summary>
        public RebellionResult Launch(CareerSnapshot current, RebellionContext context)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            return _domain.Launch(_config, current, context);
        }
    }
}
