using System.Collections.Generic;
using ThreeKingdom.Domain.Conquest;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 玩家战术倾向档案（ADR-0013 E3 反套路·渐进记忆）：跨战记录玩家历次出征布势路线 + 连用连击。
    /// 战斗开始时据此给 AI <b>渐进</b>反制提示（连用越多反制越强，第1次不明显→第4次设套），非开挂——用的是<b>过去战例</b>，
    /// 非当前计划（不作弊，反全知）。不可变（Record 返新）；为持久化设计。
    /// </summary>
    public sealed class PlayerTacticProfile
    {
        private readonly IReadOnlyDictionary<int, int> _counts;   // ApproachPlan(int) → 次数
        /// <summary>末次路线（-1=无）。</summary>
        public int LastApproach { get; }
        /// <summary>末次路线连用次数。</summary>
        public int Streak { get; }

        public static PlayerTacticProfile Empty { get; } = new PlayerTacticProfile(new Dictionary<int, int>(), -1, 0);

        public PlayerTacticProfile(IReadOnlyDictionary<int, int> counts, int lastApproach, int streak)
        {
            _counts = counts; LastApproach = lastApproach; Streak = streak;
        }

        /// <summary>供持久化：各路线累计次数。</summary>
        public IReadOnlyDictionary<int, int> Counts => _counts;

        /// <summary>某路线累计次数。</summary>
        public int CountOf(ApproachPlan a) => _counts.TryGetValue((int)a, out int v) ? v : 0;

        /// <summary>记一次玩家出征路线，返回新档案（连击：同路线累加，换路线归 1）。</summary>
        public PlayerTacticProfile Record(ApproachPlan approach)
        {
            var next = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> kv in _counts) next[kv.Key] = kv.Value;
            next[(int)approach] = (next.TryGetValue((int)approach, out int c) ? c : 0) + 1;
            int streak = ((int)approach == LastApproach) ? Streak + 1 : 1;
            return new PlayerTacticProfile(next, (int)approach, streak);
        }

        /// <summary>
        /// 反制提示（GDD_021 / ADR-0013 E3）：据末次路线的主攻区 + 连击强度，返回 (守方应加固的区, 权重)。
        /// 权重 = min(连击, cap) × perStreak（渐进）；无历史返 (Front, 0)。
        /// </summary>
        public (ZoneId Zone, int Weight) CounterHint(int perStreak = 15, int cap = 4)
        {
            if (LastApproach < 0) return (BattleField.Front, 0);
            ZoneId zone = MainThrustZone((ApproachPlan)LastApproach);
            int streak = Streak > cap ? cap : Streak;
            return (zone, streak * perStreak);
        }

        /// <summary>某路线的主攻落点区（守方反制应加固处）。</summary>
        private static ZoneId MainThrustZone(ApproachPlan a) => a switch
        {
            ApproachPlan.ProtractedSiege => BattleField.Supply,   // 长围断粮 → 粮道
            ApproachPlan.NightRaid => BattleField.Cover,          // 夜袭 → 掩护区
            _ => BattleField.Front,                               // 正面强攻 / 假退诱敌(真攻正面)
        };
    }
}
