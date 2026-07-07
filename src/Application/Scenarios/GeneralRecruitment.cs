using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>一名可招在野武将（GDD_027 P2 / GDD_020 统一）：中文名 + 招揽难度定性档（反全知，无数字）。</summary>
    public readonly struct RecruitCandidate
    {
        public string GeneralId { get; }
        /// <summary>招揽难度定性：易招 / 尚可 / 难招（由野心/傲物/名望派生，无数字）。</summary>
        public string DifficultyLabel { get; }

        public RecruitCandidate(string generalId, string difficultyLabel)
        {
            GeneralId = generalId; DifficultyLabel = difficultyLabel;
        }
    }

    /// <summary>
    /// 武将招揽（GDD_027 P2）：把「在野」在世武将统一为可招池（扩 GDD_020 从 3 原型到全谱）。
    /// 难度自隐秘心（野心/傲物）+ 玩家名望派生，反全知呈定性档。纯函数，无场景依赖。
    /// </summary>
    public static class GeneralRecruitment
    {
        /// <summary>某纪元的在野在世可招池（GDD_027 R5.1）：归属为在野者入池。</summary>
        public static IReadOnlyList<RecruitCandidate> PoolAt(int anchorYear, int renownTier = 0)
        {
            var pool = new List<RecruitCandidate>();
            foreach (GeneralDossier d in GeneralDossiers.All)
            {
                if (GeneralAffiliations.AffiliationOf(d.Id, anchorYear).Status != AffiliationStatus.Wandering) continue;
                pool.Add(new RecruitCandidate(d.Id.Value, Difficulty(d, renownTier)));
            }
            pool.Sort((a, b) => string.CompareOrdinal(a.GeneralId, b.GeneralId));
            return pool;
        }

        /// <summary>某在野武将对某名望玩家的招揽难度定性档（GDD_027 公式 · 无数字反全知）。</summary>
        public static string DifficultyOf(CharacterId general, int renownTier = 0)
        {
            GeneralDossier? d = GeneralDossiers.Find(general);
            return d == null ? "尚可" : Difficulty(d, renownTier);
        }

        /// <summary>招揽难度<b>数值</b>（内部用；招揽结算/状态机消费，不呈玩家）。净成本越高越难。</summary>
        public static int DifficultyScore(CharacterId general, int renownTier = 0)
        {
            GeneralDossier? d = GeneralDossiers.Find(general);
            return d == null ? 1 : DifficultyScoreOf(d, renownTier);
        }

        private static int DifficultyScoreOf(GeneralDossier d, int renownTier)
        {
            int ambitionCost = d.Ambition switch
            {
                Ambition.None => 0,
                Ambition.Aspiring => 1,
                Ambition.Grand => 2,
                _ => 3,
            };
            return ambitionCost
                   + (d.HasTag(GeneralTag.Arrogant) ? 1 : 0)
                   - (d.HasTag(GeneralTag.Benevolent) ? 1 : 0)
                   - renownTier;
        }

        // 净成本 = 野心成本(0狼顾3) + 傲物(+1) − 仁德(−1) − 名望档。<=0 易招 / 1..2 尚可 / >=3 难招。
        private static string Difficulty(GeneralDossier d, int renownTier)
        {
            int score = DifficultyScoreOf(d, renownTier);
            if (score <= 0) return "易招";
            if (score <= 2) return "尚可";
            return "难招";
        }
    }
}
