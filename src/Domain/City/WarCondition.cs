using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.City
{
    /// <summary>战役条件输入的种类（GDD_004 §System Outputs：喂给战争的派生量，M03 / TR-city-004）。</summary>
    public enum WarConditionKind
    {
        /// <summary>守城强度（由工事派生）。</summary>
        DefenseStrength = 0,

        /// <summary>补给能力（由后勤持有军粮派生）。</summary>
        SupplyCapacity = 1,

        /// <summary>民心风险（由城市民心派生，越低风险越高）。</summary>
        UnrestRisk = 2,
    }

    /// <summary>
    /// 单条战役条件派生的可解释账本（TR-city-004「可解释代价」）。
    /// 记录派生量、其治理来源与代价说明——非黑箱数值。
    /// </summary>
    public sealed class WarConditionLedgerEntry
    {
        /// <summary>条件种类。</summary>
        public WarConditionKind Kind { get; }

        /// <summary>派生值。</summary>
        public long Value { get; }

        /// <summary>可解释来源/代价说明（中文）。</summary>
        public string Note { get; }

        public WarConditionLedgerEntry(WarConditionKind kind, long value, string note)
        {
            Kind = kind;
            Value = value;
            Note = note ?? string.Empty;
        }
    }

    /// <summary>
    /// 城市治理态派生的战役条件输入（GDD_004 §System Outputs / M03 / TR-city-004）。
    /// 这是「喂给战争的筛选尺子」——治理选择经此影响后续战役/守城，而非独立经营。
    /// 完整战斗胜负由战斗系统（epic-007）消费本输入（其会话装配属 M05 / epic-018）。
    /// </summary>
    public sealed class WarConditionInputs
    {
        /// <summary>守城强度（由工事派生，≥0）。</summary>
        public long DefenseStrength { get; }

        /// <summary>补给能力（由后勤持有军粮派生，≥0）。</summary>
        public long SupplyCapacity { get; }

        /// <summary>民心风险（≥0；越大越危险，民心高则为 0）。</summary>
        public long UnrestRisk { get; }

        /// <summary>逐条可解释账本（每条战役条件的治理来源与代价）。</summary>
        public IReadOnlyList<WarConditionLedgerEntry> Ledger { get; }

        public WarConditionInputs(long defenseStrength, long supplyCapacity, long unrestRisk, IReadOnlyList<WarConditionLedgerEntry> ledger)
        {
            DefenseStrength = defenseStrength;
            SupplyCapacity = supplyCapacity;
            UnrestRisk = unrestRisk;
            Ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }
    }

    /// <summary>
    /// 战役条件派生配置（GDD_004 §Balancing / ADR-0003 数据驱动）。不可变；构造校验范围。
    /// 系数版本化，方法体内不得硬编码（ADR-0003）。
    /// </summary>
    public sealed class WarConditionConfig
    {
        /// <summary>工事 → 守城强度系数（GDD §Balancing「工事防御贡献」，≥0）。</summary>
        public FixedPoint FortDefenseFactor { get; }

        /// <summary>民心风险阈值：民心低于此值即产生风险（风险 = 阈值 − 民心），≥0。</summary>
        public int MoraleRiskThreshold { get; }

        public WarConditionConfig(FixedPoint fortDefenseFactor, int moraleRiskThreshold)
        {
            if (fortDefenseFactor < FixedPoint.Zero)
                throw new ArgumentOutOfRangeException(nameof(fortDefenseFactor), "工事防御系数不可为负。");
            if (moraleRiskThreshold < 0)
                throw new ArgumentOutOfRangeException(nameof(moraleRiskThreshold), "民心风险阈值不可为负。");
            FortDefenseFactor = fortDefenseFactor;
            MoraleRiskThreshold = moraleRiskThreshold;
        }
    }
}
