using System;
using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Application.Career
{
    /// <summary>
    /// 忠臣晋升线的 Application 写路径（GDD_014 / TR-career-002 / ADR-0002 单一写路径）。
    /// 持有版本化 <see cref="PromotionLadderConfig"/>（注入），把"累积来源事件 / 申请晋升"作为命令委派
    /// Domain <see cref="CareerProgressionService"/> 结算。UI 不得绕过本路径直接改 Domain 状态。
    /// </summary>
    public sealed class LoyalistAdvancementService
    {
        private readonly PromotionLadderConfig _config;
        private readonly CareerProgressionService _domain;

        public LoyalistAdvancementService(PromotionLadderConfig config, CareerProgressionService domain)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        }

        /// <summary>默认构造：自建 Domain 结算服务。</summary>
        public LoyalistAdvancementService(PromotionLadderConfig config)
            : this(config, new CareerProgressionService()) { }

        /// <summary>记录一次具名来源累积（作战/治理/任务/招揽…），按配置权重写入 merit/renown/standing。</summary>
        public CareerCommandResult RecordGain(CareerSnapshot current, CareerGainSource source, int count = 1)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            return _domain.ApplyGain(_config, current, source, count);
        }

        /// <summary>查询当前晋升达标情况（不改状态），供 UI 显示距下一阶差距。</summary>
        public PromotionCheck CheckPromotion(CareerSnapshot current)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            return _domain.Check(_config, current);
        }

        /// <summary>申请晋升：达标晋级、未达返回稳定错误码且状态不变。</summary>
        public CareerCommandResult RequestPromotion(CareerSnapshot current)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            return _domain.RequestPromotion(_config, current);
        }
    }
}
