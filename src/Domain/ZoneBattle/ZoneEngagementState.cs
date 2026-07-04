using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 某区交战累积态（GDD_021 R5 / ADR-0012 D4：条件按区、按回合涌现）。不可变。
    /// 载伏兵蓄势/断粮累计的<b>回合计数</b>与该区<b>已成型条件集</b>；交战双方支队由 <see cref="ZoneBattleState"/> 按 Location 检索。
    /// </summary>
    public sealed class ZoneEngagementState
    {
        /// <summary>所属区。</summary>
        public ZoneId Zone { get; }
        /// <summary>伏兵蓄势：该区隐蔽未破的连续回合数（达门槛→伏兵突然性成型）。</summary>
        public int AmbushCharge { get; }
        /// <summary>断粮累计：该区（敌粮道）被切断的累计回合数（达门槛→断粮达宽限）。</summary>
        public int StarveTurns { get; }
        /// <summary>该区已成型的兵法条件（规范枚举序）。</summary>
        public IReadOnlyList<TacticCondition> FormedConditions { get; }

        public ZoneEngagementState(
            ZoneId zone, int ambushCharge, int starveTurns, IReadOnlyList<TacticCondition>? formed)
        {
            if (zone.Value is null) throw new ArgumentException("Zone 不可为空。", nameof(zone));
            if (ambushCharge < 0) throw new ArgumentOutOfRangeException(nameof(ambushCharge));
            if (starveTurns < 0) throw new ArgumentOutOfRangeException(nameof(starveTurns));
            Zone = zone;
            AmbushCharge = ambushCharge;
            StarveTurns = starveTurns;

            var seen = new SortedSet<int>();
            if (formed != null) foreach (TacticCondition c in formed) seen.Add((int)c);
            var list = new List<TacticCondition>();
            foreach (int v in seen) list.Add((TacticCondition)v);
            FormedConditions = list;
        }

        /// <summary>某区的初始空态。</summary>
        public static ZoneEngagementState Empty(ZoneId zone) => new ZoneEngagementState(zone, 0, 0, null);

        /// <summary>是否已成某条件。</summary>
        public bool HasFormed(TacticCondition c)
        {
            foreach (TacticCondition f in FormedConditions) if (f == c) return true;
            return false;
        }

        /// <summary>产出更新态（结算写回）。</summary>
        public ZoneEngagementState With(int? ambushCharge = null, int? starveTurns = null, IReadOnlyList<TacticCondition>? formed = null)
            => new ZoneEngagementState(Zone, ambushCharge ?? AmbushCharge, starveTurns ?? StarveTurns, formed ?? FormedConditions);

        internal void AppendTo(StateHasher hasher)
        {
            ZoneHashing.AppendString(hasher, Zone.Value);
            hasher.Append(AmbushCharge).Append(StarveTurns);
            hasher.Append(FormedConditions.Count);
            foreach (TacticCondition c in FormedConditions) hasher.Append((int)c);
        }
    }

    /// <summary>战斗回合钟（GDD_021 R3：回合有上限，超时=攻方未克退兵）。不可变。</summary>
    public sealed class BattleClock
    {
        /// <summary>当前回合（1 起）。</summary>
        public int Round { get; }
        /// <summary>回合上限。</summary>
        public int MaxRounds { get; }

        public BattleClock(int round, int maxRounds)
        {
            if (round < 1) throw new ArgumentOutOfRangeException(nameof(round), "回合 1 起。");
            if (maxRounds < 1) throw new ArgumentOutOfRangeException(nameof(maxRounds));
            Round = round;
            MaxRounds = maxRounds;
        }

        /// <summary>是否已超时（回合超过上限）。</summary>
        public bool IsExpired => Round > MaxRounds;

        /// <summary>进入下一回合。</summary>
        public BattleClock Next() => new BattleClock(Round + 1, MaxRounds);

        internal void AppendTo(StateHasher hasher) => hasher.Append(Round).Append(MaxRounds);
    }
}
