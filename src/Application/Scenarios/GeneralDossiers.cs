using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

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

        // ---- 生卒年（GDD_026 / ADR-0015 D4）：史载近似，供在世/在职与 EraStage 判定。GeneralDossiers 仍为武将数据唯一权威。----
        // 空降者（char-player-lord）非历史武将，不入此表（不受生卒约束）。
        private static readonly IReadOnlyDictionary<string, (int Birth, int Death)> LifeYears =
            new Dictionary<string, (int, int)>(StringComparer.Ordinal)
            {
                // 君主
                ["char-caocao"] = (155, 220), ["char-liubei"] = (161, 223), ["char-sunce"] = (175, 200),
                ["char-sunquan"] = (182, 252), ["char-yuanshao"] = (154, 202), ["char-yuan"] = (155, 199),
                ["char-lubu"] = (161, 199), ["char-liubiao"] = (142, 208), ["char-liuzhang"] = (162, 219),
                ["char-mateng"] = (156, 212), ["char-zhanglu"] = (160, 216), ["char-gongsun"] = (151, 199),
                ["char-lijue"] = (150, 198), ["char-zhangxiu"] = (160, 207), ["char-kongrong"] = (153, 208),
                ["char-hansui"] = (140, 215), ["char-shixie"] = (137, 226),
                // 蜀
                ["char-guanyu"] = (160, 220), ["char-zhangfei"] = (165, 221), ["char-zhaoyun"] = (168, 229),
                ["char-zhugeliang"] = (181, 234), ["char-machao"] = (176, 222), ["char-huangzhong"] = (148, 220),
                ["char-weiyan"] = (170, 234), ["char-pangtong"] = (179, 214), ["char-jiangwei"] = (202, 264),
                // 魏
                ["char-xiahoudun"] = (157, 220), ["char-zhangliao"] = (169, 222), ["char-xuchu"] = (170, 230),
                ["char-dianwei"] = (160, 197), ["char-guojia"] = (170, 207), ["char-xunyu"] = (163, 212),
                ["char-simayi"] = (179, 251), ["char-caoren"] = (168, 223), ["char-jiaxu"] = (147, 223),
                // 吴
                ["char-zhouyu"] = (175, 210), ["char-lusu"] = (172, 217), ["char-lvmeng"] = (178, 220),
                ["char-luxun"] = (183, 245), ["char-taishici"] = (166, 206), ["char-ganning"] = (175, 215),
                ["char-huanggai"] = (145, 215),
                // 群雄部将
                ["char-yanliang"] = (160, 200), ["char-wenchou"] = (160, 200), ["char-gaoshun"] = (160, 198),
                ["char-chengong"] = (160, 198), ["char-huaxiong"] = (160, 191), ["char-haozhao"] = (180, 229),
            };

        // ---- 190 讨董布防（GDD_026 D4）：部将 → 任职城（须属该城 190 归属势力）。君主不入（本身即势力之主）。----
        // 未及冠/未生者（machao/zhugeliang/simayi/luxun/lvmeng/jiangwei/pangtong/zhouyu/ganning…）此年不布防，留待后续锚点年。
        private static readonly IReadOnlyDictionary<string, string> Placement190 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-guanyu"] = "city-xiaopei", ["char-zhangfei"] = "city-xiaopei",   // 刘备·小沛
                ["char-zhaoyun"] = "city-beiping",                                       // 公孙瓒·北平
                ["char-huangzhong"] = "city-xiangyang", ["char-weiyan"] = "city-xiangyang", // 刘表·襄阳
                ["char-xiahoudun"] = "city-chenliu", ["char-caoren"] = "city-chenliu",   // 曹操·陈留
                ["char-dianwei"] = "city-chenliu", ["char-xunyu"] = "city-chenliu",
                ["char-guojia"] = "city-chenliu", ["char-chengong"] = "city-chenliu",
                ["char-zhangliao"] = "city-changan", ["char-jiaxu"] = "city-changan",     // 李傕·长安（董卓系）
                ["char-yanliang"] = "city-ye", ["char-wenchou"] = "city-ye",             // 袁绍·邺城
                ["char-gaoshun"] = "city-xiapi",                                          // 吕布·下邳
                ["char-huaxiong"] = "city-hulao",                                         // 袁术·虎牢关
                ["char-taishici"] = "city-beihai",                                        // 孔融·北海
                ["char-lusu"] = "city-jianye", ["char-huanggai"] = "city-jianye",        // 孙氏·建业
            };

        /// <summary>某武将生卒年（公元）；未登记则 null（不受生卒约束，视为常在）。</summary>
        public static (int Birth, int Death)? LifeOf(CharacterId id)
            => id.Value != null && LifeYears.TryGetValue(id.Value, out (int, int) y) ? y : ((int, int)?)null;

        /// <summary>某武将在某公元年是否在世且已及冠出仕（GDD_026 F4）；无生卒登记者视为常在。</summary>
        public static bool AvailableAt(CharacterId id, int year, int serviceMinAge = 16)
        {
            (int Birth, int Death)? life = LifeOf(id);
            if (life == null) return true;
            return life.Value.Birth + serviceMinAge <= year && year <= life.Value.Death;
        }

        /// <summary>某锚点年、某城在职的部将（GDD_026 R4；反全知外壳另投影）。当前仅 190 有布防数据，余年返回空。</summary>
        public static IReadOnlyList<CharacterId> GeneralsAt(CityId city, int anchorYear)
        {
            var result = new List<CharacterId>();
            if (anchorYear != 190 || city.Value == null) return result;
            foreach (KeyValuePair<string, string> kv in Placement190)
            {
                if (kv.Value != city.Value) continue;
                var id = new CharacterId(kv.Key);
                if (AvailableAt(id, anchorYear)) result.Add(id);
            }
            result.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            return result;
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
