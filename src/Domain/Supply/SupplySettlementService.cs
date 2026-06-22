using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 后勤补给结算服务（GDD_012 §Formula 1/2/4/5 / TR-supply-001/002 / ADR-0004 + ADR-0003）。
    /// 纯函数、确定性：三类持有者间转移为原子「一减一加」事务（守恒，无双计）；
    /// 断粮经路线拓扑切断派生（见 <see cref="RouteSupplyLink"/>），逐时段累积传导、
    /// 达宽限期才发断粮后果事件——<b>不立即崩溃、无成功按钮</b>。
    /// 本服务<b>只</b>发事件，<b>不</b>改士气/疲劳（单一权威，交 GDD_011 消费）。
    /// 补给量为权威整数；损耗率为定点（权威路径无 float）。
    /// </summary>
    public sealed class SupplySettlementService
    {
        /// <summary>
        /// 创建运输：从城市/营地库存原子移交到在途批次（GDD_012 §Main Rules：原子扣除，不能同时留城与在途）。
        /// </summary>
        public SupplyChainState DispatchConvoy(SupplyChainState state, long amount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "运输量不可为负。");
            if (amount > state.CityStock) throw new InvalidOperationException("运输量超过城市库存，拒绝（不重复扣粮）。");

            return state.With(
                cityStock: checked(state.CityStock - amount),
                convoyLoad: checked(state.ConvoyLoad + amount));
        }

        /// <summary>
        /// 在途损耗结算（GDD_012 §Formula 4）：S_convoy' = round(S_convoy × (1 − loss_rate × env_mult))；
        /// 损耗量计入 <see cref="SupplyChainState.Lost"/>（守恒，不凭空消失）。
        /// </summary>
        /// <param name="environmentMultiplier">天气/道路损耗放大系数（≥0，定点；恶劣环境 >1）。</param>
        public SupplyChainState ApplyTransitLoss(SupplyChainState state, SupplyConfig config, FixedPoint environmentMultiplier)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (environmentMultiplier < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(environmentMultiplier), "环境系数不可为负。");
            if (state.ConvoyLoad == 0) return state;

            FixedPoint effectiveRate = (config.TransitLossRate * environmentMultiplier).Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint retainedFx = FixedPoint.FromInt(checked((int)state.ConvoyLoad)) * (FixedPoint.One - effectiveRate);
            long retained = retainedFx.RoundToInt();
            if (retained < 0) retained = 0;
            if (retained > state.ConvoyLoad) retained = state.ConvoyLoad;
            long lost = state.ConvoyLoad - retained;

            return state.With(
                convoyLoad: retained,
                lost: checked(state.Lost + lost));
        }

        /// <summary>
        /// 单时段单位需求结算（GDD_012 §Formula 2/5：先携行后交付；短缺累计；达宽限期发事件）。
        /// </summary>
        /// <param name="state">结算前补给链状态。</param>
        /// <param name="unit">结算的单位（事件归属）。</param>
        /// <param name="demand">本时段需求（≥0）。</param>
        /// <param name="routeDeliverable">补给路线是否可交付（由 <see cref="RouteSupplyLink.IsDeliverable"/> 据拓扑派生）。</param>
        /// <param name="config">版本化配置（宽限期）。</param>
        public SupplySegmentResult SettleSegment(
            SupplyChainState state,
            UnitId unit,
            long demand,
            bool routeDeliverable,
            SupplyConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (demand < 0) throw new ArgumentOutOfRangeException(nameof(demand), "需求不可为负。");

            // 先携行后交付（GDD §Formula 2）：携行不足再取在途；路线断则无交付。
            long fromCarried = Math.Min(state.UnitCarried, demand);
            long remain = demand - fromCarried;
            long fromConvoy = routeDeliverable ? Math.Min(state.ConvoyLoad, remain) : 0;
            long shortage = remain - fromConvoy;
            long consumed = fromCarried + fromConvoy;

            // 短缺时长累积（GDD §Formula 5）：短缺则 +1，满足则清零（恢复补给即回补）。
            int shortageSegments = shortage > 0 ? checked(state.ShortageSegments + 1) : 0;

            var newState = state.With(
                unitCarried: state.UnitCarried - fromCarried,
                convoyLoad: state.ConvoyLoad - fromConvoy,
                consumed: checked(state.Consumed + consumed),
                shortageSegments: shortageSegments);

            // 达宽限期才发断粮后果事件——渐进恶化、不立即崩溃；本系统不改士气（单一权威）。
            bool cutoff = shortage > 0 && shortageSegments >= config.GracePeriodSegments;
            SupplyStatusLevel status = shortage <= 0
                ? SupplyStatusLevel.Sufficient
                : (cutoff ? SupplyStatusLevel.Cutoff : SupplyStatusLevel.Strained);
            SupplyCutoffEvent? cutoffEvent = cutoff
                ? new SupplyCutoffEvent(unit, shortageSegments, shortage)
                : null;

            return new SupplySegmentResult(newState, consumed, shortage, status, cutoffEvent);
        }
    }
}
