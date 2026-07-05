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
        private static readonly IReadOnlyList<GeneralDossier> Roster = BuildRoster();

        /// <summary>查某武将档案；未登记则 null。</summary>
        public static GeneralDossier? Find(CharacterId id)
            => id.Value != null && ById.TryGetValue(id.Value, out GeneralDossier? d) ? d : null;

        /// <summary>全体已登记武将档案（稳定序：按 id 规范序）——供武将目录（#2）遍历。</summary>
        public static IReadOnlyList<GeneralDossier> All => Roster;

        private static IReadOnlyList<GeneralDossier> BuildRoster()
        {
            var list = new List<GeneralDossier>(ById.Values);
            list.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));
            return list;
        }

        private static IReadOnlyDictionary<string, GeneralDossier> Build()
        {
            var list = new List<GeneralDossier>();
            void D(string id, CombatTier tier, StrategyTier strat, LoyaltyLeaning loy, Ambition amb, params GeneralTag[] tags)
                => list.Add(new GeneralDossier(new CharacterId(id), tags, loy, amb, tier, strat));

            // ---- 君主（忠于己，野心分方面/问鼎）----
            D("char-caocao", CombatTier.Sturdy, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Strategist, GeneralTag.Bloodthirsty);
            D("char-liubei", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Benevolent);
            D("char-sunce", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Reckless, GeneralTag.Cavalry);
            D("char-sunquan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);
            D("char-yuanshao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Stubborn);
            D("char-yuan", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Arrogant, GeneralTag.Hesitant);
            D("char-lubu", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless, GeneralTag.Fickle, GeneralTag.Cavalry);
            D("char-liubiao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant, GeneralTag.Benevolent);
            D("char-liuzhang", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);
            D("char-mateng", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-zhanglu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Defender);
            D("char-gongsun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-lijue", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Bloodthirsty, GeneralTag.Fickle);
            D("char-zhangxiu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-kongrong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent, GeneralTag.Arrogant);
            D("char-hansui", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-shixie", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-player-lord", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Defender);

            // ---- 蜀汉 ----
            D("char-guanyu", CombatTier.Peerless, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Awe, GeneralTag.IronBones, GeneralTag.Arrogant);
            D("char-zhangfei", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless, GeneralTag.Bloodthirsty);
            D("char-zhaoyun", CombatTier.Peerless, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-zhugeliang", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-machao", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry, GeneralTag.Bloodthirsty);
            D("char-huangzhong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-weiyan", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-pangtong", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-jiangwei", CombatTier.Valiant, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);

            // ---- 曹魏 ----
            D("char-xiahoudun", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-zhangliao", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.NightRaider, GeneralTag.Defender);
            D("char-xuchu", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-dianwei", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-guojia", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning, GeneralTag.Strategist);
            D("char-xunyu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-simayi", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Strategist, GeneralTag.Wolflook);
            D("char-caoren", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-jiaxu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);

            // ---- 孙吴 ----
            D("char-zhouyu", CombatTier.Sturdy, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-lusu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-lvmeng", CombatTier.Valiant, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Cunning);
            D("char-luxun", CombatTier.Sturdy, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-taishici", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-ganning", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.NightRaider);
            D("char-huanggai", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);

            // ---- 群雄部将 ----
            D("char-yanliang", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-wenchou", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-gaoshun", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-chengong", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-huaxiong", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-haozhao", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);

            var dict = new Dictionary<string, GeneralDossier>(StringComparer.Ordinal);
            foreach (GeneralDossier d in list) dict[d.Id.Value] = d;
            return dict;
        }
    }
}
