using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 单阶段结算账本条目（GDD_004 §Data Model：CitySettlementResult 收支条目）。
    /// 记录该阶段的粮食/民心/工事增量与可解释说明（每项产出可追溯，确定性可复盘）。
    /// </summary>
    public sealed class CitySettlementLedgerEntry
    {
        /// <summary>所属结算阶段。</summary>
        public CityDaySettlementStage Stage { get; }

        /// <summary>本阶段粮食库存增量（带符号；产入为正，消耗/移交为负）。</summary>
        public long FoodDelta { get; }

        /// <summary>本阶段城市民心增量（带符号）。</summary>
        public int MoraleDelta { get; }

        /// <summary>本阶段工事增量（带符号）。</summary>
        public int FortDelta { get; }

        /// <summary>可解释说明（中文，账本可读性）。</summary>
        public string Note { get; }

        public CitySettlementLedgerEntry(CityDaySettlementStage stage, long foodDelta, int moraleDelta, int fortDelta, string note)
        {
            Stage = stage;
            FoodDelta = foodDelta;
            MoraleDelta = moraleDelta;
            FortDelta = fortDelta;
            Note = note ?? string.Empty;
        }
    }

    /// <summary>
    /// 城市日界结算结果（GDD_004 §Data Model：CitySettlementResult）。
    /// 含按规范顺序排列的逐阶段账本、最终城市状态、移交后勤后的后勤持有量，以及
    /// 收支汇总。守恒恒等可由 <see cref="ConservationHolds"/> 验证（TR-city-001/002）。
    /// </summary>
    public sealed class CitySettlementResult
    {
        /// <summary>结算前库存（用于守恒校验）。</summary>
        public long StartStock { get; }

        /// <summary>结算后城市状态。</summary>
        public CityEconomyState EndState { get; }

        /// <summary>结算后后勤持有的军粮总量（已移交部分单一计入此处，城市不再计）。</summary>
        public long EndLogisticsHolding { get; }

        /// <summary>本日基础产入。</summary>
        public long Yield { get; }

        /// <summary>本日实际民用消耗（受库存下限约束的真实扣减量）。</summary>
        public long Consumed { get; }

        /// <summary>本日移交后勤的军粮量（承诺阶段执行）。</summary>
        public long Transferred { get; }

        /// <summary>本日民用短缺量（需求 − 实际消耗，≥0）。</summary>
        public long Shortage { get; }

        /// <summary>骚乱风险高（短缺超过阈值）。</summary>
        public bool HighUnrestRisk { get; }

        /// <summary>逐阶段账本（按 <see cref="CityDaySettlementStages.CanonicalOrder"/> 顺序）。</summary>
        public IReadOnlyList<CitySettlementLedgerEntry> Ledger { get; }

        public CitySettlementResult(
            long startStock,
            CityEconomyState endState,
            long endLogisticsHolding,
            long yield,
            long consumed,
            long transferred,
            long shortage,
            bool highUnrestRisk,
            IReadOnlyList<CitySettlementLedgerEntry> ledger)
        {
            StartStock = startStock;
            EndState = endState ?? throw new ArgumentNullException(nameof(endState));
            EndLogisticsHolding = endLogisticsHolding;
            Yield = yield;
            Consumed = consumed;
            Transferred = transferred;
            Shortage = shortage;
            HighUnrestRisk = highUnrestRisk;
            Ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }

        /// <summary>
        /// 守恒恒等（GDD_004 §Formula 2 / TR-city-002）：产入 − 消耗 − 转移 = 库存差。
        /// 真为守恒成立。库存差以实际扣减量计，故下限夹取产生短缺时仍恒成立。
        /// </summary>
        public bool ConservationHolds => Yield - Consumed - Transferred == EndState.Stock - StartStock;
    }
}
