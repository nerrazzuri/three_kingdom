using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
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

                // ③④ 交战结算（该区）。种子=战斗种子⊕回合，供将领战阵档系数每回合右偏抽取（确定性、可复现）。
                ulong roundSeed = working.Seed ^ ((ulong)working.Clock.Round * 2654435761UL);
                ResolveZoneCombat(zone, working, eng, byId, attFormed, newlyFormed, config, roundSeed);
            }

            // ⑤ 回合钟推进 + 以新支队/交战写回。
            ZoneBattleState next = working.With(new List<Detachment>(byId.Values), newEngagements, working.Clock.Next());
            emergences.Sort(StringComparer.Ordinal);
            return new RoundResolution(next, emergences);
        }

        /// <summary>某区交战：攻/守战力比较 → 败方减员+掉士气，双方增疲劳；新涌现→守方额外士气冲击。写回 <paramref name="byId"/>。</summary>
        private static void ResolveZoneCombat(
            Zone zone, ZoneBattleState state, ZoneEngagementState eng, Dictionary<string, Detachment> byId,
            int attackerFormedCount, bool newlyFormed, ZoneBattleConfig config, ulong roundSeed)
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

            // 已成型兵法的战力加成，再乘攻方主谋的<b>计谋系数</b>（GDD_025 谋略档：诸葛之计比马谡同计更狠，
            // 偶有"一计定乾坤"；无谋帅档则中性 1.0）。系数每回合右偏抽取，确定性可复现。
            FixedPoint stratMul = StrategyMultiplier(attackers, roundSeed);
            FixedPoint condBonus = config.ConditionBonusEach * FixedPoint.FromInt(attackerFormedCount) * stratMul;
            FixedPoint condMul = FixedPoint.One + condBonus;
            FixedPoint attPower = SidePower(attackers, config, roundSeed) * condMul;
            // 城防之利：守方在坚固地形（城门正面）得工事加成——破坚城须真优势（W5），非均势可下。
            FixedPoint defMul = zone.Terrain == TerrainKind.Fortified
                ? FixedPoint.One + config.FortifiedDefenseBonus
                : FixedPoint.One;
            FixedPoint defPower = SidePower(defenders, config, roundSeed) * defMul;

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

        /// <summary>
        /// 阵营有效战力：Σ 兵力×士气×姿态×<b>疲劳侵蚀</b>×<b>将领战阵档系数</b>（GDD_025：有档之将带兵杀伤更强，
        /// 系数每回合右偏抽取——低端常见、高端罕见，故名将偶有神勇爆发。无档之将系数中性 1.0）。
        /// </summary>
        private static FixedPoint SidePower(IReadOnlyList<Detachment> dets, ZoneBattleConfig config, ulong roundSeed)
        {
            FixedPoint sum = FixedPoint.Zero;
            foreach (Detachment d in dets)
            {
                FixedPoint prowess = FixedPoint.One;
                if (d.General?.Prowess is CombatTier tier)   // 有战阵档 → 种子化右偏抽取杀伤系数（同局可复现）
                    prowess = CombatProwess.Roll(tier, new DeterministicRandom(roundSeed ^ FnvId(d.Id.Value)));
                sum += FixedPoint.FromInt(d.Strength) * d.Morale * config.PostureMod(d.Posture) * config.FatiguePowerMul(d.Fatigue) * prowess;
            }
            return sum;
        }

        /// <summary>
        /// 攻方计谋系数（GDD_025 谋略档）：取在场攻方将领<b>最高</b>谋略档为主谋，种子化右偏抽取一系数放大兵法加成。
        /// 无一将带谋略档 → 中性 1.0（不放大不削减）。种子分流于战阵档（异或黄金比常量），同局可复现。
        /// </summary>
        private static FixedPoint StrategyMultiplier(IReadOnlyList<Detachment> attackers, ulong roundSeed)
        {
            bool has = false;
            StrategyTier best = StrategyTier.Dull;
            string bestId = "";
            foreach (Detachment d in attackers)
            {
                if (d.General?.Strategy is StrategyTier t)
                {
                    if (!has || t > best) { best = t; bestId = d.Id.Value; }
                    has = true;
                }
            }
            if (!has) return FixedPoint.One;
            ulong seed = roundSeed ^ FnvId(bestId) ^ 0x9E3779B97F4A7C15UL;
            return CombatProwess.RollStrategy(best, new DeterministicRandom(seed));
        }

        /// <summary>支队 id 稳定散列（FNV-1a，供将领战阵档系数种子分派，确定性）。</summary>
        private static ulong FnvId(string s)
        {
            ulong h = 1469598103934665603UL;
            if (s != null) foreach (char c in s) { h ^= c; h *= 1099511628211UL; }
            return h;
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
