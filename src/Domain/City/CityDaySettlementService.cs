using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市日界产耗结算服务（GDD_004 §Main Rules/§Formulas / TR-city-001/002 / ADR-0004 + ADR-0003）。
    /// 纯函数、确定性（同输入同结果）：按规范固定顺序结算并产出新状态 + 逐阶段账本，
    /// <b>不</b>就地修改输入。粮量为权威整数、系数为定点，权威路径无 float。
    /// 所有产耗率/阈值/系数来自注入的 <see cref="CitySettlementConfig"/>，逻辑不硬编码数值。
    /// </summary>
    public sealed class CityDaySettlementService
    {
        /// <summary>
        /// 结算一个城市日界（GDD_004 §Main Rules 稳定顺序：承诺 → 产入 → 消耗 → 短缺后果 → 工事/治安）。
        /// </summary>
        /// <param name="state">结算前城市经济状态。</param>
        /// <param name="logisticsHolding">结算前后勤持有的军粮量（承诺移交将单一累加于此，无双计）。</param>
        /// <param name="config">版本化平衡配置（数据驱动）。</param>
        /// <param name="populationPressure">人口压力系数（定点，≥0）；与基础民用消耗相乘得民用需求。</param>
        /// <returns>含逐阶段账本、最终状态与收支汇总的结算结果。</returns>
        public CitySettlementResult Settle(
            CityEconomyState state,
            long logisticsHolding,
            CitySettlementConfig config,
            FixedPoint populationPressure)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (logisticsHolding < 0) throw new ArgumentOutOfRangeException(nameof(logisticsHolding), "后勤持有量不可为负。");
            if (populationPressure < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(populationPressure), "人口压力不可为负。");

            long startStock = state.Stock;
            var ledger = new List<CitySettlementLedgerEntry>(CityDaySettlementStages.CanonicalOrder.Count);

            // 阶段 0｜承诺：执行已保留军粮征用，移交后勤（所有权转移，单一计入后勤，守恒无双计）。
            long transfer = state.Reserved;
            long stockAfterCommit = checked(startStock - transfer);          // reserved ≤ stock 不变量保证 ≥0
            long endLogisticsHolding = checked(logisticsHolding + transfer);
            ledger.Add(new CitySettlementLedgerEntry(
                CityDaySettlementStage.Commit, -transfer, 0, 0,
                $"承诺：移交后勤军粮 {transfer}（城市库存所有权转出，后勤单一计入，无双计）。"));

            // 阶段 1｜产入：基础日产入入库。
            long stockAfterYield = checked(stockAfterCommit + config.BaseYield);
            ledger.Add(new CitySettlementLedgerEntry(
                CityDaySettlementStage.Yield, config.BaseYield, 0, 0,
                $"产入：基础日产入 +{config.BaseYield}。"));

            // 阶段 2｜消耗：民用需求 = max(基础维护, 基础民用消耗 × 人口压力)；
            //          实际消耗只能扣到库存下限以上，不凭空补齐、不出负（GDD §Formula 2/3 + §Edge Cases）。
            long civDemand = ComputeCivDemand(config, populationPressure);
            long maxConsumable = Math.Max(0L, stockAfterYield - config.StockFloor);
            long actualConsumed = Math.Min(civDemand, maxConsumable);
            long stockAfterConsume = checked(stockAfterYield - actualConsumed);
            ledger.Add(new CitySettlementLedgerEntry(
                CityDaySettlementStage.Consume, -actualConsumed, 0, 0,
                $"消耗：民用需求 {civDemand}，实际消耗 {actualConsumed}（夹至下限 {config.StockFloor}）。"));

            // 阶段 3｜短缺后果：未满足需求转化为民心损耗与骚乱风险（不补齐库存）。
            long shortage = checked(civDemand - actualConsumed);
            int moraleLoss = shortage <= 0
                ? 0
                : (config.ShortageMoralePenalty * FixedPoint.FromInt(checked((int)shortage))).RoundToInt();
            int newMorale = ClampInt(checked(state.CivMorale - moraleLoss), 0, config.CivMoraleMax);
            int moraleDelta = newMorale - state.CivMorale;
            bool highUnrestRisk = shortage > config.UnrestShortageThreshold;
            ledger.Add(new CitySettlementLedgerEntry(
                CityDaySettlementStage.ShortageConsequence, 0, moraleDelta, 0,
                $"短缺后果：短缺 {shortage}，民心 {moraleDelta:+0;-0;0}，骚乱风险 {(highUnrestRisk ? "高" : "低")}。"));

            // 阶段 4｜工事/治安：工事修复（受最大值与修复速率约束，S1 无围城修正）；不触动粮食库存。
            int repairDone = Math.Min(state.FortificationMax - state.FortificationCurrent, config.FortRepairRate);
            int newFort = state.FortificationCurrent + repairDone;
            ledger.Add(new CitySettlementLedgerEntry(
                CityDaySettlementStage.FortificationSecurity, 0, 0, repairDone,
                $"工事/治安：工事修复 +{repairDone}（治安维持 {state.Security}）。"));

            var endState = state.With(
                stock: stockAfterConsume,
                reserved: 0,
                civMorale: newMorale,
                fortificationCurrent: newFort);

            return new CitySettlementResult(
                startStock,
                endState,
                endLogisticsHolding,
                config.BaseYield,
                actualConsumed,
                transfer,
                shortage,
                highUnrestRisk,
                ledger);
        }

        /// <summary>民用需求（GDD_004 §Formula 3）：max(基础维护, round(基础民用消耗 × 人口压力))。</summary>
        private static long ComputeCivDemand(CitySettlementConfig config, FixedPoint populationPressure)
        {
            FixedPoint scaled = FixedPoint.FromInt(checked((int)config.BaseCivConsume)) * populationPressure;
            long demandFromPressure = scaled.RoundToInt();
            return Math.Max(config.BaseMaintenance, demandFromPressure);
        }

        private static int ClampInt(int value, int min, int max)
            => value < min ? min : (value > max ? max : value);
    }
}
