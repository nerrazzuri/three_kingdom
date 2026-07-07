using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>一城的武将任命推荐（GDD_027 / ADR-0013 E4.3）：谁内政/谁守城/谁军师/谁主攻。不可变。</summary>
    public readonly struct CityAssignment
    {
        public CharacterId? Governor { get; }     // 内政官
        public CharacterId? DefenderLead { get; } // 守将
        public CharacterId? Advisor { get; }      // 军师
        public CharacterId? Vanguard { get; }     // 主攻先锋
        public CityAssignment(CharacterId? governor, CharacterId? defenderLead, CharacterId? advisor, CharacterId? vanguard)
        {
            Governor = governor; DefenderLead = defenderLead; Advisor = advisor; Vanguard = vanguard;
        }
    }

    /// <summary>
    /// 武将任命 AI（ADR-0013 E4.3）：据城武将册按<b>气质标签/档</b>推荐各职人选——AI 与玩家共用一套"会用人"逻辑
    /// （善守镇城、诡谋作军师、猛将当先锋、仁德/谋略理政）。复用既有派生（内政/军师），补守将/先锋择选。可解释反馈。纯函数。
    /// </summary>
    public static class GeneralAssignmentService
    {
        /// <summary>为某城某纪元推荐任命（应用演义覆盖层排除已陨落/移籍者）。</summary>
        public static CityAssignment Recommend(CityId city, int anchorYear, LoreOverrides? overrides = null)
        {
            IReadOnlyList<CharacterId> roster = GeneralAffiliations.RosterOf(city, anchorYear, overrides);
            CharacterId? governor = GovernanceContribution.AdministratorOf(roster);   // 内政（P4）
            CharacterId? advisor = CouncilCapability.AdvisorOf(roster);               // 军师（P5）
            CharacterId? defender = PickBy(roster, prowessy: true, GeneralTag.Defender, GeneralTag.IronBones);
            CharacterId? vanguard = PickBy(roster, prowessy: true, exclude: defender, GeneralTag.LoneValor, GeneralTag.Cavalry, GeneralTag.Reckless);
            return new CityAssignment(governor, defender, advisor, vanguard);
        }

        /// <summary>从册中按偏好标签择一（命中标签优先，再按战阵档/谋略档；可排除某人）。</summary>
        private static CharacterId? PickBy(IReadOnlyList<CharacterId> roster, bool prowessy, params GeneralTag[] preferred)
            => PickBy(roster, prowessy, null, preferred);

        private static CharacterId? PickBy(IReadOnlyList<CharacterId> roster, bool prowessy, CharacterId? exclude, params GeneralTag[] preferred)
        {
            CharacterId? best = null; int bestKey = int.MinValue;
            foreach (CharacterId g in roster)
            {
                if (exclude.HasValue && g.Value == exclude.Value.Value) continue;
                GeneralDossier? d = GeneralDossiers.Find(g);
                if (d == null) continue;
                int key = 0;
                foreach (GeneralTag t in preferred) if (d.HasTag(t)) { key = 1000; break; }
                key += prowessy ? (int)d.Prowess : (int)d.Strategy;
                if (key > bestKey) { bestKey = key; best = g; }
            }
            return best;
        }
    }
}
