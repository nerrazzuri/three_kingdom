using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>一回合结算的产出（新态 + 本回合涌现事件，供 UI/复盘）。不可变。</summary>
    public sealed class RoundResolution
    {
        public ZoneBattleState State { get; }
        /// <summary>本回合新成型的（区,兵法条件）涌现事件（规范序）。</summary>
        public IReadOnlyList<string> Emergences { get; }

        public RoundResolution(ZoneBattleState state, IReadOnlyList<string> emergences)
        {
            State = state;
            Emergences = emergences;
        }
    }

    /// <summary>
    /// 回合同步结算（GDD_021 R3/R5 / ADR-0012 D4/D5，<b>确定性纯函数</b>）：
    /// ① 在途推进 → ② 各区按区条件涌现（攻方）→ ③ 各区交战/减员/士气/疲劳 → ④ 涌现兵法一次性冲击 → ⑤ 回合钟推进。
    /// 遍历序：区域按 id 规范序（D5 优先序）。整数/定点、无随机。命令（部署/调整/AI）由调用方在结算<b>前</b>已应用。
    /// </summary>
    public sealed class RoundResolutionService
    {
        private readonly ZoneConditionService _conditions = new ZoneConditionService();

        public RoundResolution ResolveRound(ZoneBattleState state, ZoneBattleContext context, ZoneBattleConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // ① 在途推进（上回合下的调动逐步到位）。
            var byId = new Dictionary<string, Detachment>(StringComparer.Ordinal);
            foreach (Detachment d in state.Detachments)
            {
                Detachment moved = d.AdvanceTransit();
                byId[moved.Id.Value] = moved;
            }
            ZoneBattleState working = state.WithDetachments(new List<Detachment>(byId.Values));

            var newEngagements = new List<ZoneEngagementState>();
            var emergences = new List<string>();

            // 区域规范序遍历（确定性优先序，D5）。
            foreach (Zone zone in working.Field.Zones)
            {
                ZoneEngagementState prev = working.EngagementOf(zone.Id);
                int prevOffensive = prev.FormedConditions.Count;

                // ② 按区条件涌现（攻方视角；守方靠正面战力，MVP 无守方条件）。
                ZoneConditionOutcome outcome = _conditions.Evaluate(zone, BattleSide.Attacker, working, context, prev, config);
                var eng = new ZoneEngagementState(zone.Id, outcome.AmbushCharge, outcome.StarveTurns, outcome.Formed);
                newEngagements.Add(eng);

                int attFormed = outcome.Formed.Count;
                bool newlyFormed = attFormed > prevOffensive;
                foreach (TacticCondition c in outcome.Formed)
                {
                    // 记新成型事件（差集）。
                    bool wasFormed = false;
                    foreach (TacticCondition p in prev.FormedConditions) if (p == c) { wasFormed = true; break; }
                    if (!wasFormed) emergences.Add(zone.Id.Value + ":" + c);
                }

                // ③④ 交战结算（该区）。
                ResolveZoneCombat(zone, working, eng, byId, attFormed, newlyFormed, config);
            }

            // ⑤ 回合钟推进 + 以新支队/交战写回。
            ZoneBattleState next = working.With(new List<Detachment>(byId.Values), newEngagements, working.Clock.Next());
            emergences.Sort(StringComparer.Ordinal);
            return new RoundResolution(next, emergences);
        }

        /// <summary>某区交战：攻/守战力比较 → 败方减员+掉士气，双方增疲劳；新涌现→守方额外士气冲击。写回 <paramref name="byId"/>。</summary>
        private static void ResolveZoneCombat(
            Zone zone, ZoneBattleState state, ZoneEngagementState eng, Dictionary<string, Detachment> byId,
            int attackerFormedCount, bool newlyFormed, ZoneBattleConfig config)
        {
            var attackers = new List<Detachment>();
            var defenders = new List<Detachment>();
            foreach (Detachment d in state.DetachmentsIn(zone.Id))
            {
                if (d.InTransit || d.IsBroken) continue;
                if (d.Side == BattleSide.Attacker) attackers.Add(d);
                else defenders.Add(d);
            }
            if (attackers.Count == 0 && defenders.Count == 0) return;

            FixedPoint condMul = FixedPoint.One + config.ConditionBonusEach * FixedPoint.FromInt(attackerFormedCount);
            FixedPoint attPower = SidePower(attackers, config) * condMul;
            // 城防之利：守方在坚固地形（城门正面）得工事加成——破坚城须真优势（W5），非均势可下。
            FixedPoint defMul = zone.Terrain == TerrainKind.Fortified
                ? FixedPoint.One + config.FortifiedDefenseBonus
                : FixedPoint.One;
            FixedPoint defPower = SidePower(defenders, config) * defMul;

            // 单方占据：无交战（占据推进目标，如断粮/破口），仅增疲劳。
            if (attackers.Count == 0 || defenders.Count == 0)
            {
                ApplyFatigue(attackers, byId, config);
                ApplyFatigue(defenders, byId, config);
                return;
            }

            bool attackerWinsZone = attPower > defPower;
            bool tie = attPower == defPower;
            FixedPoint winnerLoss = Half(config.AttritionRate);
            FixedPoint ratio = PowerRatio(attPower, defPower, tie, attackerWinsZone);
            FixedPoint loserLoss = Cap(config.AttritionRate * ratio, config.AttritionCap);
            FixedPoint loserMoraleDrop = Cap(config.MoraleDropOnLoss * ratio, MoraleDropCap);   // 被碾压→士气崩得快

            foreach (Detachment d in attackers)
            {
                FixedPoint loss = tie ? winnerLoss : (attackerWinsZone ? winnerLoss : loserLoss);
                FixedPoint moraleDelta = attackerWinsZone || tie ? FixedPoint.Zero : loserMoraleDrop;
                byId[d.Id.Value] = ApplyCombat(d, loss, moraleDelta, config);
            }
            foreach (Detachment d in defenders)
            {
                FixedPoint loss = tie ? winnerLoss : (attackerWinsZone ? loserLoss : winnerLoss);
                FixedPoint moraleDelta = attackerWinsZone ? loserMoraleDrop : FixedPoint.Zero;
                if (attackerWinsZone && newlyFormed) moraleDelta = moraleDelta + config.EmergenceMoraleShock;   // 涌现兵法冲击守方
                byId[d.Id.Value] = ApplyCombat(d, loss, moraleDelta, config);
            }
        }

        /// <summary>阵营有效战力：Σ 兵力×士气×姿态乘数×<b>疲劳侵蚀</b>（久战/猛攻的兵疲则战力衰减）。</summary>
        private static FixedPoint SidePower(IReadOnlyList<Detachment> dets, ZoneBattleConfig config)
        {
            FixedPoint sum = FixedPoint.Zero;
            foreach (Detachment d in dets)
                sum += FixedPoint.FromInt(d.Strength) * d.Morale * config.PostureMod(d.Posture) * config.FatiguePowerMul(d.Fatigue);
            return sum;
        }

        private static Detachment ApplyCombat(Detachment d, FixedPoint lossRate, FixedPoint moraleDelta, ZoneBattleConfig config)
        {
            int loss = (FixedPoint.FromInt(d.Strength) * lossRate).RoundToInt();
            int newStrength = Math.Max(0, d.Strength - loss);
            FixedPoint morale = (d.Morale - moraleDelta).Clamp(FixedPoint.Zero, FixedPoint.One);
            return d.WithCombat(newStrength, morale, NextFatigue(d, config));
        }

        private static void ApplyFatigue(IReadOnlyList<Detachment> dets, Dictionary<string, Detachment> byId, ZoneBattleConfig config)
        {
            foreach (Detachment d in dets)
                byId[d.Id.Value] = d.WithCombat(d.Strength, d.Morale, NextFatigue(d, config));
        }

        /// <summary>按姿态差异化累积疲劳（主攻耗力快、坚守省力），封顶 1。</summary>
        private static FixedPoint NextFatigue(Detachment d, ZoneBattleConfig config)
            => (d.Fatigue + config.FatiguePerRound * config.PostureFatigueMul(d.Posture)).Clamp(FixedPoint.Zero, FixedPoint.One);

        /// <summary>士气跌幅上限（防单回合直接归零，留博弈余地）。</summary>
        private static readonly FixedPoint MoraleDropCap = FixedPoint.FromFraction(45, 100);

        /// <summary>战力比 winner/loser（≥1；悬殊→大，封顶于调用处）。tie=1。</summary>
        private static FixedPoint PowerRatio(FixedPoint attPower, FixedPoint defPower, bool tie, bool attackerWins)
        {
            if (tie) return FixedPoint.One;
            FixedPoint loserPower = attackerWins ? defPower : attPower;
            FixedPoint winnerPower = attackerWins ? attPower : defPower;
            if (loserPower.Raw <= 0) return FixedPoint.FromInt(8);   // 悬殊上界（配合各 Cap）
            return winnerPower / loserPower;
        }

        private static FixedPoint Cap(FixedPoint v, FixedPoint cap) => v > cap ? cap : v;
        private static FixedPoint Half(FixedPoint v) => v * FixedPoint.FromFraction(1, 2);
    }
}
