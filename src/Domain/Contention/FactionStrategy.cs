using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Contention
{
    /// <summary>势力战略意图（GDD_017/018 / ADR-0013 E4.1）：每势力每战略步的当前意图（可解释反馈来源）。</summary>
    public enum StrategicIntent
    {
        /// <summary>扩张：强势且据优，主动兼并。</summary>
        Expansion = 0,
        /// <summary>趁火打劫：有虚弱邻居可乘。</summary>
        Opportunist = 1,
        /// <summary>防守：中庸、受强邻威胁，固守。</summary>
        Defense = 2,
        /// <summary>恢复：刚失城，休整。</summary>
        Recovery = 3,
        /// <summary>报复：遭玩家夺城，图报。</summary>
        Revenge = 4,
        /// <summary>外交求存：弱小但未崩，图结盟自保。</summary>
        Diplomacy = 5,
        /// <summary>濒临崩溃：仅余一城。</summary>
        Collapse = 6,
    }

    /// <summary>一势力的战略投影（可解释反馈：势力 + 意图 + 对玩家威胁档）。不可变。</summary>
    public readonly struct FactionStrategyView
    {
        public FactionId Faction { get; }
        public StrategicIntent Intent { get; }
        /// <summary>对玩家威胁档 0..3（0 无威胁 / 3 大威胁）。</summary>
        public int ThreatToPlayer { get; }
        public FactionStrategyView(FactionId faction, StrategicIntent intent, int threatToPlayer)
        {
            Faction = faction; Intent = intent; ThreatToPlayer = threatToPlayer;
        }
    }

    /// <summary>
    /// 势力战略评估（GDD_017/018 / ADR-0013 E4.1）：由争霸态（领城/支配度/趋势）+ 是否遭玩家夺城，纯函数派生每势力的
    /// <see cref="StrategicIntent"/> 与对玩家威胁档。为可解释反馈（"曹操当前战略：扩张"）与后续目标/施压决策（E4.2）提供大脑第一层。
    /// </summary>
    public static class FactionStrategy
    {
        /// <summary>评估某势力当前战略意图。<paramref name="prev"/> 供趋势（刚失城→恢复）；<paramref name="wrongedByPlayer"/>→报复。</summary>
        public static StrategicIntent Assess(
            ContentionState state, FactionId faction, FactionId player, ContentionState? prev, bool wrongedByPlayer)
        {
            int cities = state.CitiesOf(faction);
            if (cities <= 0) return StrategicIntent.Collapse;
            if (cities == 1) return wrongedByPlayer ? StrategicIntent.Revenge : StrategicIntent.Collapse;

            int prevCities = prev?.CitiesOf(faction) ?? cities;
            if (cities < prevCities) return StrategicIntent.Recovery;   // 刚失城 → 休整
            if (wrongedByPlayer) return StrategicIntent.Revenge;         // 遭玩家夺城 → 图报

            int strongest = 0, total = 0, aliveCount = 0;
            bool weakRivalExists = false;
            foreach (PowerStanding p in state.Powers)
            {
                if (!p.Alive) continue;
                total += p.Cities; aliveCount++;
                if (p.Cities > strongest) strongest = p.Cities;
                if (p.Faction != faction && p.Cities == 1) weakRivalExists = true;
            }
            int avg = aliveCount > 0 ? total / aliveCount : cities;

            if (cities >= strongest) return StrategicIntent.Expansion;       // 天下最强 → 扩张
            if (weakRivalExists && cities >= avg) return StrategicIntent.Opportunist; // 强于均且有弱邻 → 趁火打劫
            if (cities >= avg) return StrategicIntent.Defense;               // 中庸 → 固守
            return StrategicIntent.Diplomacy;                               // 弱小未崩 → 求存
        }

        /// <summary>某势力对玩家的威胁档 0..3（相对实力：领城倍数）。</summary>
        public static int ThreatToPlayer(ContentionState state, FactionId faction, FactionId player)
        {
            int mine = state.CitiesOf(player);
            int theirs = state.CitiesOf(faction);
            if (theirs <= 0 || faction == player) return 0;
            if (mine <= 0) return 3;
            if (theirs >= mine * 2) return 3;
            if (theirs >= mine) return 2;
            if (theirs * 2 >= mine) return 1;
            return 0;
        }

        /// <summary>全势力战略投影（可解释反馈；存续非玩家势力，按 FactionId 规范序）。</summary>
        public static IReadOnlyList<FactionStrategyView> AssessAll(
            ContentionState state, FactionId player, ContentionState? prev, IReadOnlyCollection<string>? wronged)
        {
            var views = new List<FactionStrategyView>();
            foreach (PowerStanding p in state.Powers)
            {
                if (!p.Alive || p.Faction == player) continue;
                bool w = wronged != null && p.Faction.Value != null && wronged.Contains(p.Faction.Value);
                views.Add(new FactionStrategyView(p.Faction,
                    Assess(state, p.Faction, player, prev, w), ThreatToPlayer(state, p.Faction, player)));
            }
            return views;
        }
    }
}
