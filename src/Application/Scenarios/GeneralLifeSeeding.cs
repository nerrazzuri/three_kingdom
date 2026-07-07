using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 武将运行时人生初态播种（GDD_027 #1 桥）：由静态档案（气质·忠义倾向）+ 归属（主君·驻城）铸 <see cref="GeneralState"/> 初态。
    /// 静态档案答"本质如何"，此把它转成"这一局的起点"。纯函数。
    /// </summary>
    public static class GeneralLifeSeeding
    {
        /// <summary>忠义倾向 → 初始忠诚（忠义90/安分70/摇摆50/怀贰30；无档案60 中庸）。</summary>
        public static int InitialLoyalty(CharacterId id)
        {
            GeneralDossier? d = GeneralDossiers.Find(id);
            if (d == null) return 60;
            return d.Leaning switch
            {
                LoyaltyLeaning.Loyal => 90,
                LoyaltyLeaning.Content => 70,
                LoyaltyLeaning.Wavering => 50,
                _ => 30, // Disloyal
            };
        }

        /// <summary>按某纪元归属铸某将初态（在职→事奉其主·驻其城；在野/不存→无主）。</summary>
        public static GeneralState Seed(CharacterId id, int anchorYear, LoreOverrides? overrides = null)
        {
            Affiliation a = GeneralAffiliations.AffiliationOf(id, anchorYear, overrides);
            FactionId? faction = a.Status == AffiliationStatus.InService ? a.Faction : (FactionId?)null;
            CityId? city = a.Status == AffiliationStatus.InService ? a.City : (CityId?)null;
            return GeneralState.Fresh(id, faction, city, InitialLoyalty(id));
        }
    }
}
