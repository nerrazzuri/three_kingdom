using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>一军编制（GDD_027 P7 / 用户裁定）：主将 1 + 副将 0~1。多军可并进（各自独立成军）。</summary>
    public readonly struct Army
    {
        public string LeaderId { get; }
        public string? DeputyId { get; }   // 可选副将
        public Army(string leaderId, string? deputyId) { LeaderId = leaderId; DeputyId = deputyId; }
        public bool HasDeputy => !string.IsNullOrEmpty(DeputyId);
    }

    /// <summary>
    /// 成军（GDD_027 P7）：从城武将册点出一军——主将取最强战阵/先锋，副将取次强（可选）。<b>一军至多 2 人</b>。
    /// 将档/标签 → 战力贡献（喂区域战斗，复用 ADR-0004 档）。多线出征=从不同城各成一军。纯函数。
    /// </summary>
    public static class ArmyFormation
    {
        /// <summary>从城册成一军（主将+可选副将）；空册 → null。<paramref name="exclude"/> 为已被别军征用之将（多军并进不重复用人）。</summary>
        public static Army? Form(IReadOnlyList<CharacterId> roster, ISet<string>? exclude = null)
        {
            string? leader = null, deputy = null;
            int leaderScore = int.MinValue, deputyScore = int.MinValue;
            foreach (CharacterId g in roster)
            {
                if (exclude != null && exclude.Contains(g.Value)) continue;
                int s = MartialScore(g);
                if (s > leaderScore)
                {
                    deputy = leader; deputyScore = leaderScore;
                    leader = g.Value; leaderScore = s;
                }
                else if (s > deputyScore)
                {
                    deputy = g.Value; deputyScore = s;
                }
            }
            if (leader == null) return null;
            return new Army(leader, deputy);
        }

        /// <summary>一军的战力贡献（GDD_027 公式，整数）：主将战阵×10 + 副将战阵×5 + 谋略辅佐×3。</summary>
        public static int PowerContribution(Army army)
        {
            int p = MartialScore(new CharacterId(army.LeaderId)) * 10;
            if (army.HasDeputy) p += MartialScore(new CharacterId(army.DeputyId!)) * 5;
            // 谋略辅佐（主副任一善谋 → 计谋加成）。
            int strat = StrategyScore(new CharacterId(army.LeaderId));
            if (army.HasDeputy) strat = System.Math.Max(strat, StrategyScore(new CharacterId(army.DeputyId!)));
            return p + strat * 3;
        }

        private static int MartialScore(CharacterId g)
        {
            GeneralDossier? d = GeneralDossiers.Find(g);
            return d == null ? 0 : (int)d.Prowess;
        }

        private static int StrategyScore(CharacterId g)
        {
            GeneralDossier? d = GeneralDossiers.Find(g);
            return d == null ? 0 : (int)d.Strategy;
        }
    }
}
