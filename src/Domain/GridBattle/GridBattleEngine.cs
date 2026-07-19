using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>设目的地命令结果（前置校验，稳定错误码，无部分写入——命令契约）。</summary>
    public enum GridCommandResult
    {
        Ok = 0,
        UnitNotFound = 1,
        DestinationOutOfBounds = 2,
        DestinationImpassable = 3,
    }

    /// <summary>一次时间段推进的结果（新态 + 终局 + 半路遭遇 + 简要日志）。</summary>
    public sealed class SegmentResult
    {
        public GridBattleState State { get; }
        public GridBattleOutcome Outcome { get; }
        /// <summary>本段"半路遭遇"的玩家部队（行军中新贴上敌军）——Application/UI 据此暂停给玩家抉择（GDD-028 §3.5）。</summary>
        public IReadOnlyList<BattleUnitId> Encounters { get; }
        public IReadOnlyList<string> Log { get; }

        internal SegmentResult(GridBattleState state, GridBattleOutcome outcome, IReadOnlyList<BattleUnitId> encounters, IReadOnlyList<string> log)
        {
            State = state; Outcome = outcome; Encounters = encounters; Log = log;
        }
    }

    /// <summary>
    /// 确定性格子战斗引擎（ADR-0018 D1/D5 / GDD-028）。<see cref="Advance"/> 是纯函数段推进：
    /// 于 <b>克隆态</b>上依序结算 移动→遭遇→火攻→交战→补给→回血/叛逃→剔除→终局，返回新态（原态不变）。
    /// 全整数、无隐式随机、规范序遍历（同态→同哈希，可回放）。
    /// </summary>
    public static class GridBattleEngine
    {
        /// <summary>设目的地命令（GDD-028 §3.4）：校验单位存在/存活、目的地在场且可通行；直接改在给定态上。</summary>
        public static GridCommandResult SetDestination(GridBattleState state, BattleUnitId unitId, GridCoord dest)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            GridUnit? unit = null;
            foreach (GridUnit u in state.Units) if (u.Alive && u.Id == unitId) { unit = u; break; }
            if (unit == null) return GridCommandResult.UnitNotFound;
            if (!state.Grid.InBounds(dest)) return GridCommandResult.DestinationOutOfBounds;
            if (!state.Grid.Passable(dest)) return GridCommandResult.DestinationImpassable;
            unit.Destination = dest;
            return GridCommandResult.Ok;
        }

        /// <summary>推进一个时间段（DaySegment），返回段结果。原态不变（在克隆上推进）。</summary>
        public static SegmentResult Advance(GridBattleState state, GridBattleConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));

            GridBattleState s = state.Clone();
            s.Clock = s.Clock.Advance(1);
            DaySegment seg = s.Clock.Segment;
            bool night = seg == DaySegment.Night;
            bool day = seg != DaySegment.Night; // 白昼/黄昏/黎明皆可视——火攻可行
            var log = new List<string>();

            // ── 遭遇前记录：哪些玩家部队原本已贴敌。
            var wasAdj = new Dictionary<BattleUnitId, bool>();
            foreach (GridUnit u in s.AliveOf(GridSide.Player)) wasAdj[u.Id] = AnyAdjacentFoe(s, u);

            // ── ① 移动（规范序；兵种速度；BFS 绕障避占）。
            foreach (GridUnit u in s.OrderedAlive())
            {
                int speed = config.SpeedOf(u.Kind);
                for (int step = 0; step < speed; step++)
                {
                    if (!u.EnRoute) break;
                    GridCoord? next = GridPathfinder.NextStep(s, u.Position, u.Destination!.Value);
                    if (next == null) break;
                    u.Position = next.Value;
                    if (!u.EnRoute) break;
                }
            }

            // ── ② 半路遭遇：仍行军、且本段新贴上敌的玩家部队。
            var encounters = new List<BattleUnitId>();
            foreach (GridUnit u in s.AliveOf(GridSide.Player))
                if (u.EnRoute && !wasAdj[u.Id] && AnyAdjacentFoe(s, u))
                    encounters.Add(u.Id);

            // ── ③ 火攻：贴敌粮仓 + 白昼/黄昏/黎明 → 焚仓，敌补给暴跌（幂等）。
            foreach (GridSide side in new[] { GridSide.Player, GridSide.Enemy })
            {
                GridSide opp = side.Opponent();
                if (s.GranaryBurnedOf(opp) || !day) continue;
                GridCoord gran = opp == GridSide.Player ? s.Grid.PlayerGranary : s.Grid.EnemyGranary;
                bool attacker = false;
                foreach (GridUnit u in s.AliveOf(side)) if (GridCoord.Chebyshev(u.Position, gran) <= 1) { attacker = true; break; }
                if (!attacker) continue;
                if (opp == GridSide.Player) { s.PlayerGranaryBurned = true; s.PlayerSupply = Math.Max(0, s.PlayerSupply - config.GranaryBurnLoss); }
                else { s.EnemyGranaryBurned = true; s.EnemySupply = Math.Max(0, s.EnemySupply - config.GranaryBurnLoss); }
                log.Add($"{seg}：火攻！{(side == GridSide.Player ? "我焚敌粮仓" : "敌焚我粮仓")}——{(opp == GridSide.Player ? "我" : "敌")}军补给暴跌。");
            }

            // ── ④ 交战：射程内造伤（林中伏击 ×AmbushPct、夜段近战 ×NightPct），补给告急方受伤 ×StarvingPct；同时结算。
            bool playerStarving = s.PlayerSupply < config.SupplyLowThreshold;
            bool enemyStarving = s.EnemySupply < config.SupplyLowThreshold;
            var dmg = new Dictionary<BattleUnitId, long>();
            IReadOnlyList<GridUnit> ordered = s.OrderedAlive();
            foreach (GridUnit a in ordered)
            {
                int reach = config.RangeOf(a.Kind);
                foreach (GridUnit b in ordered)
                {
                    if (a.Side == b.Side) continue;
                    int d = GridCoord.Chebyshev(a.Position, b.Position);
                    if (d > reach) continue;
                    long hit = a.Attack;
                    if (s.Grid.TerrainAt(a.Position) == TerrainKind.Forest && s.Grid.TerrainAt(b.Position) == TerrainKind.Pass && d == 1)
                        hit = hit * config.AmbushPct / 100; // 林中伏击
                    if (night && config.RangeOf(a.Kind) == 1)
                        hit = hit * config.NightPct / 100; // 夜袭
                    dmg.TryGetValue(b.Id, out long acc);
                    dmg[b.Id] = acc + hit;
                }
            }
            foreach (GridUnit b in ordered)
            {
                if (!dmg.TryGetValue(b.Id, out long raw)) continue;
                bool starving = b.Side == GridSide.Player ? playerStarving : enemyStarving;
                long taken = starving ? raw * config.StarvingDamagePct / 100 : raw;
                b.Strength -= (int)taken;
                if (b.Strength <= 0) log.Add($"{seg}：{b.Name} 部队溃灭。");
            }

            // ── ⑤ 补给更新：守住己仓→回升；被焚/被断→下滑。
            foreach (GridSide side in new[] { GridSide.Player, GridSide.Enemy })
            {
                GridCoord gran = side == GridSide.Player ? s.Grid.PlayerGranary : s.Grid.EnemyGranary;
                bool foeNear = false;
                foreach (GridUnit u in s.AliveOf(side.Opponent())) if (GridCoord.Chebyshev(u.Position, gran) <= 1) { foeNear = true; break; }
                bool healthy = !s.GranaryBurnedOf(side) && !foeNear;
                int delta = healthy ? config.SupplyReplenish : -config.SupplyDrain;
                int val = Math.Max(0, Math.Min(config.MaxSupply, s.SupplyOf(side) + delta));
                if (side == GridSide.Player) s.PlayerSupply = val; else s.EnemySupply = val;
            }

            // ── ⑥ 逐段：补给充足→回血；告急→叛逃减员。
            foreach (GridUnit u in s.OrderedAlive())
            {
                int sp = s.SupplyOf(u.Side);
                if (sp >= config.SupplyHighThreshold) u.Strength = Math.Min(u.MaxStrength, u.Strength + config.RecoverPerSegment);
                else if (sp < config.SupplyLowThreshold)
                {
                    u.Strength -= config.DesertPerSegment;
                    if (u.Strength <= 0) log.Add($"{seg}：{u.Name} 补给断绝，部众叛散。");
                }
            }

            s.RemoveDead();

            // ── ⑦ 终局。
            GridBattleOutcome outcome = EvaluateOutcome(s);
            return new SegmentResult(s, outcome, encounters, log);
        }

        /// <summary>终局判定（GDD-028 §3.10）：一方全灭→对方胜。</summary>
        public static GridBattleOutcome EvaluateOutcome(GridBattleState s)
        {
            bool player = false, enemy = false;
            foreach (GridUnit u in s.Units)
            {
                if (!u.Alive) continue;
                if (u.Side == GridSide.Player) player = true; else enemy = true;
            }
            if (!enemy && player) return GridBattleOutcome.PlayerVictory;
            if (!player) return GridBattleOutcome.EnemyVictory;
            return GridBattleOutcome.Ongoing;
        }

        private static bool AnyAdjacentFoe(GridBattleState s, GridUnit u)
        {
            foreach (GridUnit e in s.AliveOf(u.Side.Opponent()))
                if (GridCoord.Chebyshev(u.Position, e.Position) == 1) return true;
            return false;
        }
    }
}
