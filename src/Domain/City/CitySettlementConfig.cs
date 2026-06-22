using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市日界结算的版本化平衡配置（GDD_004 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造时校验范围，非法即抛、无部分写入。所有产耗率/阈值/系数<b>来自配置</b>，
    /// 结算逻辑<b>不</b>硬编码数值（ADR-0003 §Forbidden：硬编码产耗率）。
    /// 粮量为权威整数；民心损耗系数用定点（Q16.16），权威路径无 float（ADR-0004）。
    /// </summary>
    public sealed class CitySettlementConfig
    {
        /// <summary>基础日产入（≥0）。</summary>
        public long BaseYield { get; }

        /// <summary>基础民用消耗（≥0），乘人口压力得民用需求。</summary>
        public long BaseCivConsume { get; }

        /// <summary>基础维护需求（≥0）：人口压力为零时仍保留的最低民用消耗（GDD_004 §Edge Cases）。</summary>
        public long BaseMaintenance { get; }

        /// <summary>库存合法下限 STOCK_FLOOR（≥0）：结算不得低于此值，欠缺转短缺而非补齐。</summary>
        public long StockFloor { get; }

        /// <summary>城市民心上限 CIV_MORALE_MAX（≥0）。</summary>
        public int CivMoraleMax { get; }

        /// <summary>短缺对民心影响系数 k_shortage（≥0，定点）。</summary>
        public FixedPoint ShortageMoralePenalty { get; }

        /// <summary>骚乱风险阈值：短缺超过此值则触发高骚乱风险（≥0）。</summary>
        public long UnrestShortageThreshold { get; }

        /// <summary>工事基础修复速率（≥0）：「工事/治安」阶段每日修复上限（无围城修正，S1 siege_mod=1）。</summary>
        public int FortRepairRate { get; }

        public CitySettlementConfig(
            long baseYield,
            long baseCivConsume,
            long baseMaintenance,
            long stockFloor,
            int civMoraleMax,
            FixedPoint shortageMoralePenalty,
            long unrestShortageThreshold,
            int fortRepairRate)
        {
            if (baseYield < 0) throw new ArgumentOutOfRangeException(nameof(baseYield), "基础产入不可为负。");
            if (baseCivConsume < 0) throw new ArgumentOutOfRangeException(nameof(baseCivConsume), "基础民用消耗不可为负。");
            if (baseMaintenance < 0) throw new ArgumentOutOfRangeException(nameof(baseMaintenance), "基础维护需求不可为负。");
            if (stockFloor < 0) throw new ArgumentOutOfRangeException(nameof(stockFloor), "库存下限不可为负。");
            if (civMoraleMax < 0) throw new ArgumentOutOfRangeException(nameof(civMoraleMax), "民心上限不可为负。");
            if (shortageMoralePenalty < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(shortageMoralePenalty), "短缺民心系数不可为负。");
            if (unrestShortageThreshold < 0) throw new ArgumentOutOfRangeException(nameof(unrestShortageThreshold), "骚乱阈值不可为负。");
            if (fortRepairRate < 0) throw new ArgumentOutOfRangeException(nameof(fortRepairRate), "工事修复速率不可为负。");

            BaseYield = baseYield;
            BaseCivConsume = baseCivConsume;
            BaseMaintenance = baseMaintenance;
            StockFloor = stockFloor;
            CivMoraleMax = civMoraleMax;
            ShortageMoralePenalty = shortageMoralePenalty;
            UnrestShortageThreshold = unrestShortageThreshold;
            FortRepairRate = fortRepairRate;
        }
    }
}
