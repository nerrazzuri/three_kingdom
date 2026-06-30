using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市治理态 → 战役条件输入的确定性派生（GDD_004 §System Outputs / M03 / TR-city-004）。
    /// <b>纯函数</b>：同输入 → 同输出，整数/定点运算（ADR-0004，权威路径禁 float）。
    /// 这是「喂给战争的筛选尺子」——治理选择（修工事/征用/安抚）经此差异化影响后续战役，
    /// 且每条派生附可解释代价账本，而非黑箱数值。
    /// </summary>
    public sealed class WarConditionProjection
    {
        /// <summary>
        /// 从城市治理态 + 后勤持有派生战役条件输入。
        /// 守城强度由工事派生；补给能力由后勤持有军粮派生；民心风险由城市民心派生（越低越险）。
        /// </summary>
        public WarConditionInputs Project(CityEconomyState city, long logisticsHolding, WarConditionConfig config)
        {
            if (city is null) throw new ArgumentNullException(nameof(city));
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (logisticsHolding < 0) throw new ArgumentOutOfRangeException(nameof(logisticsHolding), "后勤持有量不可为负。");

            var ledger = new List<WarConditionLedgerEntry>(3);

            // 条件①守城强度 = round(工事当前值 × 工事防御系数)。修工事 → 守城强度↑。
            long defense = (config.FortDefenseFactor * FixedPoint.FromInt(city.FortificationCurrent)).RoundToInt();
            ledger.Add(new WarConditionLedgerEntry(
                WarConditionKind.DefenseStrength, defense,
                $"守城强度 {defense}：来自工事 {city.FortificationCurrent} ×系数（修工事可提升）。"));

            // 条件②补给能力 = 后勤持有军粮。征用 → 后勤补给↑（代价见民心风险）。
            long supply = logisticsHolding;
            ledger.Add(new WarConditionLedgerEntry(
                WarConditionKind.SupplyCapacity, supply,
                $"补给能力 {supply}：来自后勤持有军粮（征用可提升，代价为城市民心）。"));

            // 条件③民心风险 = max(0, 阈值 − 民心)。民心↓（如征用）→ 风险↑；安抚可降风险。
            long risk = Math.Max(0L, (long)config.MoraleRiskThreshold - city.CivMorale);
            ledger.Add(new WarConditionLedgerEntry(
                WarConditionKind.UnrestRisk, risk,
                $"民心风险 {risk}：民心 {city.CivMorale} 相对阈值 {config.MoraleRiskThreshold}（安抚可降低，征用会升高）。"));

            return new WarConditionInputs(defense, supply, risk, ledger);
        }
    }
}
