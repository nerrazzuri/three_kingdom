using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 麾下凝聚（GDD_027 P6）：一组同僚武将的羁绊（知己/血脉/师徒协同、仇怨互斥）+ 忠诚倾向 → 凝聚度与离心风险。
    /// 复用 <see cref="GeneralBonds"/> 与武将忠诚倾向。纯函数，整数计（ADR-0004）。
    /// </summary>
    public static class RetinueCohesion
    {
        /// <summary>凝聚度（GDD_027 公式）：知己/血脉/师徒 +2、仇怨 −3；忠义 +1、摇摆 −1、怀贰 −2。</summary>
        public static int CohesionOf(IReadOnlyList<CharacterId> generals)
        {
            int score = 0;
            foreach (Bond b in GeneralBonds.Among(generals))
                score += b.Type == BondType.Feud ? -3 : 2;
            foreach (CharacterId g in generals)
            {
                GeneralDossier? d = GeneralDossiers.Find(g);
                if (d == null) continue;
                score += d.Leaning switch
                {
                    LoyaltyLeaning.Loyal => 1,
                    LoyaltyLeaning.Wavering => -1,
                    LoyaltyLeaning.Disloyal => -2,
                    _ => 0,
                };
                if (d.HasTag(GeneralTag.Fickle)) score -= 1;   // 反复无信
            }
            return score;
        }

        /// <summary>离心风险定性（GDD_027）：怀贰/反复者众且凝聚低 → 离心。</summary>
        public static string DefectionRiskLabel(IReadOnlyList<CharacterId> generals)
        {
            int risky = 0;
            foreach (CharacterId g in generals)
            {
                GeneralDossier? d = GeneralDossiers.Find(g);
                if (d == null) continue;
                if (d.Leaning == LoyaltyLeaning.Disloyal || d.HasTag(GeneralTag.Fickle)) risky++;
            }
            int cohesion = CohesionOf(generals);
            if (risky >= 2 && cohesion < 0) return "离心";
            if (risky >= 1 || cohesion < 0) return "尚可";
            return "稳固";
        }
    }
}
