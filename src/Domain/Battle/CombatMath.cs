using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 战斗数学（GDD_010 §Formula 3/4/5 / ADR-0004）。纯函数、确定性、全程定点（无 float）。
    /// </summary>
    public static class CombatMath
    {
        /// <summary>
        /// 有效战斗力（§Formula 3）：force × morale × (1−fatigue) × discipline × terr × posture × (1+support)。
        /// 任一软状态低下按比例削弱——非战斗系统影响战争的传导路径。
        /// </summary>
        public static FixedPoint CombatPower(BattleUnitState u)
            => FixedPoint.FromInt(u.Force)
               * u.Morale
               * (FixedPoint.One - u.Fatigue)
               * u.Discipline
               * u.TerrainMod
               * u.PostureMod
               * (FixedPoint.One + u.Support);

        /// <summary>
        /// 突然性（§Formula 4）：A 对 B 突然 = A 已确认 B ∧ B 未察觉 A。
        /// 涌现自双方认知差，非「伏兵按钮」。
        /// </summary>
        public static bool IsSurprise(DetectionState detection, BattleUnitState attacker, BattleUnitState defender)
            => detection.Of(attacker.Faction, defender.Id) == Awareness.Confirmed
               && detection.Of(defender.Faction, attacker.Id) == Awareness.Unaware;

        /// <summary>突然性加成（§Formula 4）：突然则取配置倍率，否则 1。</summary>
        public static FixedPoint AmbushBonus(bool surprise, BattleConfig config)
            => surprise ? config.AmbushMultiplier : FixedPoint.One;

        /// <summary>
        /// 交战损耗（§Formula 5）：casualty(B) = cpA/(cpA+cpB) × casualty_curve × ambush_bonus(A) × force(B)。
        /// 向下取整、夹至 [0, force(B)]（确定性整数伤亡）。
        /// </summary>
        public static int Casualty(FixedPoint attackerPower, FixedPoint defenderPower, int defenderForce,
            FixedPoint casualtyCurve, FixedPoint ambushBonus)
        {
            FixedPoint denom = attackerPower + defenderPower;
            if (denom <= FixedPoint.Zero || defenderForce <= 0) return 0;

            FixedPoint ratio = attackerPower / denom;
            FixedPoint casualtyFx = ratio * casualtyCurve * ambushBonus * FixedPoint.FromInt(defenderForce);
            int casualty = casualtyFx.FloorToInt();
            if (casualty < 0) casualty = 0;
            if (casualty > defenderForce) casualty = defenderForce;
            return casualty;
        }
    }
}
