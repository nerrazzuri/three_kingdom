using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>内政官对城市治理的加成（GDD_027 P4，整数百分比，禁 float 权威路径 ADR-0004）。</summary>
    public readonly struct GovernanceModifier
    {
        public int GrainPercent { get; }   // 征粮增益
        public int MoralePercent { get; }  // 民心增益
        public int FortPercent { get; }    // 城防/工事增益
        public GovernanceModifier(int grain, int morale, int fort) { GrainPercent = grain; MoralePercent = morale; FortPercent = fort; }
        public static GovernanceModifier None => new GovernanceModifier(0, 0, 0);
    }

    /// <summary>
    /// 内政贡献（GDD_027 P4）：从城武将册点出内政官（最善内政者），其气质/档 → 治理产出加成。纯函数，无场景依赖。
    /// </summary>
    public static class GovernanceContribution
    {
        /// <summary>从一组武将中点出内政官（GDD_027 R4）：仁德/内政役优先，次取谋略档最高者；空则 null。</summary>
        public static CharacterId? AdministratorOf(IReadOnlyList<CharacterId> roster)
        {
            CharacterId? best = null;
            int bestScore = int.MinValue;
            foreach (CharacterId g in roster)
            {
                GeneralDossier? d = GeneralDossiers.Find(g);
                if (d == null) continue;
                int score = AdminScore(d);
                if (score > bestScore || (score == bestScore && best.HasValue && string.CompareOrdinal(g.Value, best.Value.Value) < 0))
                {
                    bestScore = score; best = g;
                }
            }
            return best;
        }

        /// <summary>某内政官的治理加成（GDD_027 公式）：仁德→民心；谋略档→征粮效率；善守→城防。</summary>
        public static GovernanceModifier ModifierOf(CharacterId administrator)
        {
            GeneralDossier? d = GeneralDossiers.Find(administrator);
            if (d == null) return GovernanceModifier.None;
            int morale = d.HasTag(GeneralTag.Benevolent) ? 30 : 0;
            int grain = 10 * (int)d.Strategy;                          // 谋略档 0..4 → +0..40%
            int fort = (d.HasTag(GeneralTag.Defender) ? 25 : 0) + (d.HasTag(GeneralTag.IronBones) ? 10 : 0);
            if (d.HasTag(GeneralTag.Bloodthirsty)) morale -= 20;       // 嗜杀 → 民心流失
            return new GovernanceModifier(grain, morale, fort);
        }

        /// <summary>城册内政官的加成（便捷组合）。</summary>
        public static GovernanceModifier ForRoster(IReadOnlyList<CharacterId> roster)
        {
            CharacterId? admin = AdministratorOf(roster);
            return admin.HasValue ? ModifierOf(admin.Value) : GovernanceModifier.None;
        }

        private static int AdminScore(GeneralDossier d)
        {
            int s = (int)d.Strategy * 2;
            if (d.HasTag(GeneralTag.Benevolent)) s += 5;
            if (d.HasTag(GeneralTag.Strategist)) s += 3;
            if (d.HasTag(GeneralTag.Bloodthirsty)) s -= 4;
            if (d.HasTag(GeneralTag.Reckless)) s -= 2;
            return s;
        }
    }
}
