// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 战果由可解释的≤5决定性因素得出（非随机/非技能按钮），确定性可复现（GDD_010）
// Date: 2026-06-21

using System.Collections.Generic;
using TkSlice.Domain.Config;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Battle
{
    public enum BattleOutcome
    {
        AttackerDecisive,   // 攻方决定性胜
        AttackerRepelled,   // 攻方被击退（守方胜）
        Stalemate,          // 僵持消耗，未分胜负
    }

    /// <summary>战果因素（用于「来源→修正→结果」解释链，最多展示 5 项）。</summary>
    public readonly struct BattleFactor
    {
        public readonly string Name;
        public readonly string Detail;
        public BattleFactor(string name, string detail) { Name = name; Detail = detail; }
    }

    public sealed class BattleResult
    {
        public BattleOutcome Outcome { get; init; }
        public Fixed AttackerPower { get; init; }
        public Fixed DefenderPower { get; init; }
        public Fixed PowerRatio { get; init; }      // atk / def
        public int AttackerCasualties { get; init; }
        public int DefenderCasualties { get; init; }
        public IReadOnlyList<BattleFactor> Factors { get; init; } = new List<BattleFactor>();
    }

    /// <summary>
    /// 确定性战斗结算。纯函数式：相同输入 → 相同结果。
    /// 战力 = 兵力 × 状态系数，守方叠加工事，攻方在条件成立时叠加伏击突然性。
    /// </summary>
    public static class BattleResolver
    {
        /// <summary>状态系数：由士气/疲劳/军纪/补给加权（GDD_010）。</summary>
        public static Fixed ConditionFactor(ForceState f, SiegeConfig cfg)
        {
            // base 偏移使「整装」部队(morale0.7,fat0.2,disc0.7,sup1.0)系数≈1.0
            Fixed baseOffset = Fixed.FromFraction(47, 100);
            Fixed factor = baseOffset
                + cfg.MoraleWeight * f.UnitMorale
                - cfg.FatigueWeight * f.Fatigue
                + cfg.DisciplineWeight * f.Discipline
                + cfg.SupplyWeight * f.SupplyState;
            // 系数下限 0.1，避免兵力归零式崩溃；上限 2.0
            return Fixed.Clamp(factor, Fixed.FromFraction(10, 100), Fixed.FromInt(2));
        }

        public static Fixed Power(ForceState f, SiegeConfig cfg, Fixed fortification, bool ambush)
        {
            Fixed power = Fixed.FromInt(f.Troops) * ConditionFactor(f, cfg);
            if (f.Side == Side.Defender && fortification > Fixed.Zero)
                power = power * (Fixed.OneValue + cfg.FortificationBonus * fortification);
            if (ambush)
                power = power * (Fixed.OneValue + cfg.AmbushMultiplier);
            return power;
        }

        /// <summary>
        /// 攻方对守方发起决战。fortification 仅作用于守方，ambush 仅作用于攻方（需条件链门控）。
        /// </summary>
        public static BattleResult ResolveAssault(
            ForceState attacker, ForceState defender,
            Fixed fortification, bool attackerAmbush, SiegeConfig cfg)
        {
            Fixed atkPower = Power(attacker, cfg, Fixed.Zero, attackerAmbush);
            Fixed defPower = Power(defender, cfg, fortification, false);
            // 伏击突然性压制守方（措手不及，无法成列应战）
            if (attackerAmbush)
                defPower = defPower * (Fixed.OneValue - cfg.AmbushDefenderPenalty);
            Fixed ratio = defPower > Fixed.Zero ? atkPower / defPower : Fixed.FromInt(99);

            var factors = BuildFactors(attacker, defender, cfg, fortification, attackerAmbush, ratio);

            Fixed invDecisive = Fixed.OneValue / cfg.DecisiveRatio;
            BattleOutcome outcome;
            int atkCas, defCas;

            if (ratio >= cfg.DecisiveRatio)
            {
                outcome = BattleOutcome.AttackerDecisive;
                // 攻方决定性胜：守方重创，攻方轻损
                defCas = ScaleCas(defender.Troops, Fixed.FromFraction(45, 100));
                atkCas = ScaleCas(attacker.Troops, Fixed.FromFraction(12, 100));
            }
            else if (ratio <= invDecisive)
            {
                outcome = BattleOutcome.AttackerRepelled;
                atkCas = ScaleCas(attacker.Troops, Fixed.FromFraction(40, 100));
                defCas = ScaleCas(defender.Troops, Fixed.FromFraction(10, 100));
            }
            else
            {
                outcome = BattleOutcome.Stalemate;
                atkCas = ScaleCas(attacker.Troops, Fixed.FromFraction(18, 100));
                defCas = ScaleCas(defender.Troops, Fixed.FromFraction(15, 100));
            }

            return new BattleResult
            {
                Outcome = outcome,
                AttackerPower = atkPower,
                DefenderPower = defPower,
                PowerRatio = ratio,
                AttackerCasualties = atkCas,
                DefenderCasualties = defCas,
                Factors = factors,
            };
        }

        private static int ScaleCas(int troops, Fixed rate) => (Fixed.FromInt(troops) * rate).FloorToInt();

        private static List<BattleFactor> BuildFactors(
            ForceState atk, ForceState def, SiegeConfig cfg,
            Fixed fortification, bool ambush, Fixed ratio)
        {
            // 仅展示最多 5 个决定性因素（systems-index 复杂度约束）
            var list = new List<BattleFactor>
            {
                new("兵力对比", $"攻 {atk.Troops} vs 守 {def.Troops}"),
                new("攻方状态", $"士气{atk.UnitMorale} 疲劳{atk.Fatigue} 补给{atk.SupplyState}"),
                new("守方状态", $"士气{def.UnitMorale} 疲劳{def.Fatigue} 工事{fortification}"),
            };
            if (ambush) list.Add(new("伏击突然性", $"攻方战力×{Fixed.OneValue + cfg.AmbushMultiplier}（条件链门控）"));
            list.Add(new("战力比", $"{ratio}（决定性阈值 {cfg.DecisiveRatio}）"));
            return list;
        }
    }
}
