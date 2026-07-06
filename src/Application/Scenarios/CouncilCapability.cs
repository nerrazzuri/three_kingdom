using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 军师能力（GDD_027 P5）：从城武将册点出军师（谋略最高者），其谋略档 → 可提策<b>深度</b>与<b>质量</b>。
    /// 诸葛之策 ≠ 马谡之策。纯函数，无场景依赖；深度为可提策集档位（0..4），高档严格含低档（超集）。
    /// </summary>
    public static class CouncilCapability
    {
        /// <summary>从一组武将点出军师（谋略档最高，同档取 id 稳定序）；空则 null。</summary>
        public static CharacterId? AdvisorOf(IReadOnlyList<CharacterId> roster)
        {
            CharacterId? best = null;
            int bestTier = -1;
            foreach (CharacterId g in roster)
            {
                GeneralDossier? d = GeneralDossiers.Find(g);
                if (d == null) continue;
                int tier = (int)d.Strategy;
                if (tier > bestTier || (tier == bestTier && best.HasValue && string.CompareOrdinal(g.Value, best.Value.Value) < 0))
                {
                    bestTier = tier; best = g;
                }
            }
            return best;
        }

        /// <summary>某军师的献策深度档（0..4 = 谋略档；无军师 = −1，即无策可提）。</summary>
        public static int AdviceTierOf(CharacterId? advisor)
        {
            if (!advisor.HasValue) return -1;
            GeneralDossier? d = GeneralDossiers.Find(advisor.Value);
            return d == null ? -1 : (int)d.Strategy;
        }

        /// <summary>献策质量定性档（反全知呈定性）：庸言/粗策/良谋/奇策/神算。</summary>
        public static string QualityLabel(CharacterId? advisor)
        {
            int tier = AdviceTierOf(advisor);
            return tier switch
            {
                4 => "神算",
                3 => "奇策",
                2 => "良谋",
                1 => "粗策",
                0 => "庸言",
                _ => "无策可献",
            };
        }

        /// <summary>某档军师能否提某深度之策（GDD_027：高档军师可提策集为低档超集）。</summary>
        public static bool CanPropose(CharacterId? advisor, int requiredTier)
            => AdviceTierOf(advisor) >= requiredTier;
    }
}
