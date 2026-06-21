// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 假退伏击成立 = 压住军纪 + 敌将鲁莽追击 + 伏兵未暴露（条件链，非按钮）
// Date: 2026-06-21

using TkSlice.Domain.Config;
using TkSlice.Domain.Forces;
using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Battle
{
    public enum FeignOutcome
    {
        RoutFailure,   // 军纪不足，弄假成真，真溃退
        NotPursued,    // 敌将谨慎/识破，未追击（可继续）
        AmbushSprung,  // 伏击成立
    }

    public sealed class FeignedRetreatResult
    {
        public FeignOutcome Outcome { get; init; }
        public Fixed PursuitScore { get; init; }
        public int PursuitDetachment { get; init; }
        public int DecoyLosses { get; init; }
        public BattleResult? Ambush { get; init; }
        public string Note { get; init; } = "";
    }

    /// <summary>
    /// 假退伏击结算。需三重条件成立：玩家军纪压得住佯退、敌将鲁莽到追击、伏兵尚未暴露。
    /// 任一不成立 → 失败但可继续（GDD 失败必产生可继续状态）。
    /// </summary>
    public static class AmbushResolver
    {
        public static FeignedRetreatResult Resolve(
            Fixed playerDiscipline,
            int decoyTroops, int ambushTroops,
            Fixed ambushMorale, Fixed ambushDiscipline,
            ForceState enemy, Fixed enemyRecklessness,
            Fixed ambushSuspicion, SiegeConfig cfg)
        {
            // 条件 1：军纪压住佯退？否则弄假成真
            if (playerDiscipline < cfg.FeignDisciplineThreshold)
            {
                int losses = (Fixed.FromInt(decoyTroops) * Fixed.FromFraction(35, 100)).FloorToInt();
                return new FeignedRetreatResult
                {
                    Outcome = FeignOutcome.RoutFailure,
                    DecoyLosses = losses,
                    Note = $"军纪 {playerDiscipline} < 阈值 {cfg.FeignDisciplineThreshold}：佯退失控成真溃，诱饵损 {losses}。",
                };
            }

            // 条件 2：敌将是否追击（鲁莽 − 伏击疑虑）
            Fixed pursuitScore = cfg.PursuitBase
                + cfg.PursuitRecklessWeight * enemyRecklessness
                - cfg.PursuitSuspicionWeight * ambushSuspicion;
            if (pursuitScore < cfg.PursuitThreshold)
            {
                return new FeignedRetreatResult
                {
                    Outcome = FeignOutcome.NotPursued,
                    PursuitScore = pursuitScore,
                    Note = $"敌将追击意愿 {pursuitScore} < 阈值 {cfg.PursuitThreshold}：未中诱敌之计，诱饵安全撤回。",
                };
            }

            // 条件 3 成立：敌追击 → 投入追击支队（鲁莽者过度投入），落入伏击
            Fixed frac = Fixed.Clamp(
                Fixed.FromFraction(50, 100) + Fixed.FromFraction(30, 100) * enemyRecklessness,
                Fixed.FromFraction(30, 100), Fixed.FromFraction(90, 100));
            int detachment = (Fixed.FromInt(enemy.Troops) * frac).FloorToInt();

            // 追击支队：成列被打乱（军纪骤降、追击致疲劳上升），措手不及
            var pursued = new ForceState("追击支队", Side.Defender, detachment,
                morale: enemy.UnitMorale,
                fatigue: Fixed.Clamp(enemy.Fatigue + Fixed.FromFraction(25, 100), Fixed.Zero, Fixed.OneValue),
                discipline: enemy.Discipline * Fixed.FromFraction(30, 100),
                supply: enemy.SupplyState);

            var ambushForce = new ForceState("伏兵", Side.Attacker, ambushTroops,
                ambushMorale, Fixed.FromFraction(20, 100), ambushDiscipline, Fixed.OneValue);

            var battle = BattleResolver.ResolveAssault(ambushForce, pursued,
                fortification: Fixed.Zero, attackerAmbush: true, cfg);

            return new FeignedRetreatResult
            {
                Outcome = FeignOutcome.AmbushSprung,
                PursuitScore = pursuitScore,
                PursuitDetachment = detachment,
                Ambush = battle,
                Note = $"敌将鲁莽追击（意愿 {pursuitScore}），伏兵未暴露：突然性压制，{BattleResolver_OutcomeText(battle.Outcome)}。",
            };
        }

        private static string BattleResolver_OutcomeText(BattleOutcome o) => o switch
        {
            BattleOutcome.AttackerDecisive => "追击支队被伏兵重创",
            BattleOutcome.AttackerRepelled => "伏兵反被击退",
            BattleOutcome.Stalemate => "伏击僵持",
            _ => "?"
        };
    }
}
