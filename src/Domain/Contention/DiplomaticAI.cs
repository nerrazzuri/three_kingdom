using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Contention
{
    /// <summary>势力对玩家的外交立场（ADR-0013 E5，依赖 E4 威胁/信用）。</summary>
    public enum PlayerStance
    {
        /// <summary>臣服求和：自身弱小，图自保。</summary>
        Submissive = 0,
        /// <summary>中立。</summary>
        Neutral = 1,
        /// <summary>警惕：玩家渐强或己弱于玩家。</summary>
        Wary = 2,
        /// <summary>敌意：遭玩家夺城。</summary>
        Hostile = 3,
        /// <summary>合纵：玩家太强/太反复 → 联合抗之。</summary>
        Coalition = 4,
    }

    public readonly struct PlayerStanceView
    {
        public FactionId Faction { get; }
        public PlayerStance Stance { get; }
        public PlayerStanceView(FactionId faction, PlayerStance stance) { Faction = faction; Stance = stance; }
    }

    /// <summary>
    /// 外交 AI（ADR-0013 E5）：据玩家<b>支配度</b>（据天下几城）+ <b>背信/侵略记录</b>（夺过几家城）+ 相对实力，纯函数派生
    /// 各非玩家势力对玩家的立场。核心："玩家太强或太反复 → 受威胁诸势力<b>合纵</b>围之"（世界因玩家行为改变外交）。可解释反馈。
    /// </summary>
    public static class DiplomaticAI
    {
        /// <summary>某势力对玩家的外交立场。<paramref name="wrongedByPlayer"/>=遭玩家夺城；<paramref name="playerBetrayals"/>=玩家夺城/背约累计（侵略/反复度）。</summary>
        public static PlayerStance StanceToPlayer(
            ContentionState state, FactionId faction, FactionId player, bool wrongedByPlayer, int playerBetrayals)
        {
            if (faction == player || !state.IsAlive(faction)) return PlayerStance.Neutral;

            int mine = state.CitiesOf(faction), theirs = state.CitiesOf(player);
            bool playerDominant = state.Dominance(player) >= FixedPoint.FromFraction(1, 3);   // 玩家据天下 1/3+
            bool weakVsPlayer = theirs >= mine * 2;

            if (wrongedByPlayer) return PlayerStance.Hostile;                              // 被你打过 → 敌意
            if (playerDominant && (weakVsPlayer || playerBetrayals >= 2)) return PlayerStance.Coalition; // 你太强/太反复 → 合纵
            if (mine <= 1) return PlayerStance.Submissive;                                 // 自身弱小（无合纵之势）→ 求和
            if (playerDominant || weakVsPlayer) return PlayerStance.Wary;                  // 渐强 → 警惕
            return PlayerStance.Neutral;
        }

        /// <summary>全势力对玩家立场（可解释反馈；存续非玩家势力，规范序）。</summary>
        public static IReadOnlyList<PlayerStanceView> AssessAll(
            ContentionState state, FactionId player, IReadOnlyCollection<string>? wronged, int playerBetrayals)
        {
            var views = new List<PlayerStanceView>();
            foreach (PowerStanding p in state.Powers)
            {
                if (!p.Alive || p.Faction == player) continue;
                bool w = wronged != null && p.Faction.Value != null && wronged.Contains(p.Faction.Value);
                views.Add(new PlayerStanceView(p.Faction, StanceToPlayer(state, p.Faction, player, w, playerBetrayals)));
            }
            return views;
        }

        /// <summary>合纵是否成立：判为合纵的势力 ≥ 2（多家联合围玩家）。</summary>
        public static bool CoalitionForming(
            ContentionState state, FactionId player, IReadOnlyCollection<string>? wronged, int playerBetrayals)
            => AssessAll(state, player, wronged, playerBetrayals).Count(v => v.Stance == PlayerStance.Coalition) >= 2;
    }
}
