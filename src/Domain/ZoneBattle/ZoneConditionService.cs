using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>某区条件评估输出（更新后的已成条件集 + 累积计数）。不可变。</summary>
    public sealed class ZoneConditionOutcome
    {
        public IReadOnlyList<TacticCondition> Formed { get; }
        public int AmbushCharge { get; }
        public int StarveTurns { get; }

        public ZoneConditionOutcome(IReadOnlyList<TacticCondition> formed, int ambushCharge, int starveTurns)
        {
            Formed = formed;
            AmbushCharge = ambushCharge;
            StarveTurns = starveTurns;
        }
    }

    /// <summary>
    /// 按区条件涌现（GDD_021 R5 / ADR-0012 D4，复用 GDD_010 §7 + ADR-0011 门语义）：给定某区某方的部署 + 地形 +
    /// 时机/天气 + 侦察 + 累计回合，确定性判定该区<b>已成型</b>的兵法条件。<b>门不齐不成型</b>（延续负向不变量）。
    /// 累积类（伏兵蓄势/断粮累时）逐回合累计、中断即重置——<b>战中保持</b>方能凑齐，是"战中调整"的意义。
    /// 纯函数、无随机。<b>无兵种克制</b>（只用份额门）。
    /// </summary>
    public sealed class ZoneConditionService
    {
        /// <summary>评估某区对 <paramref name="forSide"/> 的条件成型（一旦成型即保留；累积类可推进/重置）。</summary>
        public ZoneConditionOutcome Evaluate(
            Zone zone, BattleSide forSide, ZoneBattleState state,
            ZoneBattleContext context, ZoneEngagementState engagement, ZoneBattleConfig config)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));
            if (state == null) throw new ArgumentNullException(nameof(state));

            // 聚合该区己方在场支队（排除在途/被打散）。
            long total = 0, cavalry = 0;
            FixedPoint guile = FixedPoint.Zero, discipline = FixedPoint.Zero;
            bool present = false, hasFeint = false, enemyContesting = false;
            foreach (Detachment d in state.DetachmentsIn(zone.Id))
            {
                bool active = !d.InTransit && !d.IsBroken;
                if (d.Side == forSide)
                {
                    if (!active) continue;
                    present = true;
                    total += d.Strength;
                    cavalry += d.Composition.Count(TroopType.Cavalry);
                    if (d.General != null)
                    {
                        if (d.General.Guile > guile) guile = d.General.Guile;
                        if (d.General.Command > discipline) discipline = d.General.Command;
                    }
                    if (d.Posture == Posture.Feint) hasFeint = true;
                }
                else if (active) enemyContesting = true;
            }

            FixedPoint cavShare = total > 0
                ? FixedPoint.FromFraction((int)Math.Min(cavalry, int.MaxValue), (int)Math.Min(total, int.MaxValue))
                : FixedPoint.Zero;
            bool guileEnough = guile >= config.GuileMin;
            bool disciplined = discipline >= config.DisciplineMin;

            var formed = new HashSet<TacticCondition>(engagement.FormedConditions);
            int ambushCharge = engagement.AmbushCharge;
            int starveTurns = engagement.StarveTurns;

            // 瞬时条件（门齐即成型；仅限该区禀赋）。
            if (Affords(zone, TacticCondition.HeldPosition) && present) formed.Add(TacticCondition.HeldPosition);
            if (Affords(zone, TacticCondition.ControlledRetreatKeptFormation) && present && hasFeint) formed.Add(TacticCondition.ControlledRetreatKeptFormation);
            if (Affords(zone, TacticCondition.EnemyPursued) && present && cavShare >= config.CavalryMinShare) formed.Add(TacticCondition.EnemyPursued);
            if (Affords(zone, TacticCondition.IsNight) && context.IsNight) formed.Add(TacticCondition.IsNight);
            if (Affords(zone, TacticCondition.StealthSuccess) && context.IsNight && (context.IsFoggy || disciplined)) formed.Add(TacticCondition.StealthSuccess);
            if (Affords(zone, TacticCondition.DefenderUnaware) && context.IsNight && context.AttackerScouted) formed.Add(TacticCondition.DefenderUnaware);
            if (Affords(zone, TacticCondition.RaiderDisciplineMet) && context.IsNight && disciplined) formed.Add(TacticCondition.RaiderDisciplineMet);

            // 切断补给（瞬时：己方占据敌粮道区）。
            bool cuttingSupply = Affords(zone, TacticCondition.SupplyLineCut) && present;
            if (cuttingSupply) formed.Add(TacticCondition.SupplyLineCut);

            // 累积：伏兵蓄势（隘口 + 侦察 + 智谋 + 未被敌接触；中断重置）。
            if (Affords(zone, TacticCondition.AmbushSurprise))
            {
                bool charging = present && context.AttackerScouted && guileEnough
                    && zone.Terrain == TerrainKind.Pass && !enemyContesting;
                ambushCharge = charging ? ambushCharge + 1 : 0;
                if (ambushCharge >= config.AmbushChargeRounds) formed.Add(TacticCondition.AmbushSurprise);
            }

            // 累积：断粮达宽限（持续切断；中断重置）。
            if (Affords(zone, TacticCondition.ShortageReachedGrace))
            {
                starveTurns = cuttingSupply ? starveTurns + 1 : 0;
                if (starveTurns >= config.StarveRounds) formed.Add(TacticCondition.ShortageReachedGrace);
            }

            // 依赖条件：敌士气疲劳跨阈（断粮达宽限之后）。
            if (Affords(zone, TacticCondition.EnemyCohesionCrossedThreshold) && formed.Contains(TacticCondition.ShortageReachedGrace))
                formed.Add(TacticCondition.EnemyCohesionCrossedThreshold);

            // 规范枚举序输出。
            var ordered = new SortedSet<int>();
            foreach (TacticCondition c in formed) ordered.Add((int)c);
            var list = new List<TacticCondition>();
            foreach (int v in ordered) list.Add((TacticCondition)v);

            return new ZoneConditionOutcome(list, ambushCharge, starveTurns);
        }

        private static bool Affords(Zone zone, TacticCondition c) => zone.Affords(c);
    }
}
