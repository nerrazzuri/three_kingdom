using System.Collections.Generic;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 内部稳定 id → 玩家中文名的集中映射（占位期表现层用；后续可移入场景配置数据驱动）。
    /// 未登记的 id 回退原文，绝不崩 UI。<b>只影响展示，不影响权威 id</b>。
    /// </summary>
    public static class DisplayNames
    {
        private static readonly IReadOnlyDictionary<string, string> Names = new Dictionary<string, string>
        {
            ["subject-enemy-army"] = "敌军主力",
            ["advice-ambush"] = "设伏诱敌",
            ["faction-player"] = "本部",
            ["faction-lord"] = "君主",
            ["region-pass"] = "隘口",
            ["city-fanshui"] = "汜水关",
            ["city-hulao"] = "虎牢关",
            ["char-player-lord"] = "主公亲征",
            ["char-aide"] = "副将（军师）",

            // 群雄割据世界骨架（势力）。
            ["faction-yuan"] = "袁术",
            ["faction-sun"] = "孙吴",
            ["faction-cao"] = "曹操",
            ["faction-yuanshao"] = "袁绍",
            ["faction-liubei"] = "刘备",
            ["faction-lubu"] = "吕布",
            ["faction-liubiao"] = "刘表",
            ["faction-mateng"] = "马腾",
            ["faction-liuzhang"] = "刘璋",
            ["faction-zhanglu"] = "张鲁",
            ["faction-gongsun"] = "公孙瓒",
            // 城池。
            ["city-shouchun"] = "寿春",
            ["city-jianye"] = "建业",
            ["city-wujun"] = "吴郡",
            ["city-xuchang"] = "许昌",
            ["city-puyang"] = "濮阳",
            ["city-chenliu"] = "陈留",
            ["city-ye"] = "邺城",
            ["city-nanpi"] = "南皮",
            ["city-xiaopei"] = "小沛",
            ["city-xiapi"] = "下邳",
            ["city-xiangyang"] = "襄阳",
            ["city-xiliang"] = "西凉",
            ["city-chengdu"] = "成都",
            ["city-hanzhong"] = "汉中",
            ["city-beiping"] = "北平",
            // 君主。
            ["char-caocao"] = "曹操",
            ["char-yuanshao"] = "袁绍",
            ["char-liubei"] = "刘备",
            ["char-lubu"] = "吕布",
            ["char-liubiao"] = "刘表",
            ["char-mateng"] = "马腾",
            ["char-liuzhang"] = "刘璋",
            ["char-zhanglu"] = "张鲁",
            ["char-gongsun"] = "公孙瓒",
            ["char-sunquan"] = "孙权",
            ["char-yuan"] = "袁术",
        };

        /// <summary>取某 id 的中文展示名；未登记则回退原文。</summary>
        public static string Of(string id)
            => Names.TryGetValue(id, out string? name) ? name : id;
    }
}
