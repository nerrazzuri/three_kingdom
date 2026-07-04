using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 三国武将档案目录（GDD_025 内容，公版三国人物 + <b>自定的原创标签/心</b>，无任何数值 stat、不取自商业游戏）。
    /// 按稳定 id 查档案；未登记者由调用方回退。供人心杠杆（隐秘心）、战斗（气质标签）、羁绊等消费。
    /// 当前为名将集（~45），非穷尽全谱——可持续扩充（GDD_025 §Future）。
    /// </summary>
    public static class GeneralDossiers
    {
        private static readonly IReadOnlyDictionary<string, GeneralDossier> ById = Build();

        /// <summary>查某武将档案；未登记则 null。</summary>
        public static GeneralDossier? Find(CharacterId id)
            => id.Value != null && ById.TryGetValue(id.Value, out GeneralDossier? d) ? d : null;

        private static IReadOnlyDictionary<string, GeneralDossier> Build()
        {
            var list = new List<GeneralDossier>();
            void D(string id, LoyaltyLeaning loy, Ambition amb, params GeneralTag[] tags)
                => list.Add(new GeneralDossier(new CharacterId(id), tags, loy, amb));

            // ---- 君主（忠于己，野心分方面/问鼎）----
            D("char-caocao", LoyaltyLeaning.Loyal, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Strategist, GeneralTag.Bloodthirsty);
            D("char-liubei", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Benevolent);
            D("char-sunce", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Reckless, GeneralTag.Cavalry);
            D("char-sunquan", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);
            D("char-yuanshao", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Stubborn);
            D("char-yuan", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Arrogant, GeneralTag.Hesitant);
            D("char-lubu", LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless, GeneralTag.Fickle, GeneralTag.Cavalry);
            D("char-liubiao", LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant, GeneralTag.Benevolent);
            D("char-liuzhang", LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);
            D("char-mateng", LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-zhanglu", LoyaltyLeaning.Content, Ambition.None, GeneralTag.Defender);
            D("char-gongsun", LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-lijue", LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Bloodthirsty, GeneralTag.Fickle);
            D("char-zhangxiu", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-kongrong", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent, GeneralTag.Arrogant);
            D("char-hansui", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-shixie", LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-player-lord", LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Defender);

            // ---- 蜀汉 ----
            D("char-guanyu", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Awe, GeneralTag.IronBones, GeneralTag.Arrogant);
            D("char-zhangfei", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless, GeneralTag.Bloodthirsty);
            D("char-zhaoyun", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-zhugeliang", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-machao", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry, GeneralTag.Bloodthirsty);
            D("char-huangzhong", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-weiyan", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-pangtong", LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-jiangwei", LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);

            // ---- 曹魏 ----
            D("char-xiahoudun", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-zhangliao", LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.NightRaider, GeneralTag.Defender);
            D("char-xuchu", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-dianwei", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-guojia", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning, GeneralTag.Strategist);
            D("char-xunyu", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-simayi", LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Strategist, GeneralTag.Wolflook);
            D("char-caoren", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-jiaxu", LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);

            // ---- 孙吴 ----
            D("char-zhouyu", LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-lusu", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-lvmeng", LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Cunning);
            D("char-luxun", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-taishici", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-ganning", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.NightRaider);
            D("char-huanggai", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);

            // ---- 群雄部将 ----
            D("char-yanliang", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-wenchou", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-gaoshun", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-chengong", LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-huaxiong", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-haozhao", LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);

            var dict = new Dictionary<string, GeneralDossier>(StringComparer.Ordinal);
            foreach (GeneralDossier d in list) dict[d.Id.Value] = d;
            return dict;
        }
    }
}
