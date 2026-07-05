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

            // 群雄割据扩充（势力）。
            ["faction-lijue"] = "李傕",
            ["faction-zhangxiu"] = "张绣",
            ["faction-kongrong"] = "孔融",
            ["faction-hansui"] = "韩遂",
            ["faction-shixie"] = "士燮",
            // 城池。
            ["city-juancheng"] = "鄄城",
            ["city-pingyuan"] = "平原",
            ["city-jinyang"] = "晋阳",
            ["city-runan"] = "汝南",
            ["city-kuaiji"] = "会稽",
            ["city-lujiang"] = "庐江",
            ["city-xuzhou"] = "徐州",
            ["city-jiangling"] = "江陵",
            ["city-jiangxia"] = "江夏",
            ["city-changsha"] = "长沙",
            ["city-jiangzhou"] = "江州",
            ["city-zitong"] = "梓潼",
            ["city-wuwei"] = "武威",
            ["city-jicheng"] = "蓟城",
            ["city-changan"] = "长安",
            ["city-luoyang"] = "洛阳",
            ["city-wancheng"] = "宛城",
            ["city-beihai"] = "北海",
            ["city-hanyang"] = "汉阳",
            ["city-jiaozhou"] = "交州",
            // 君主。
            ["char-sunce"] = "孙策",
            ["char-lijue"] = "李傕",
            ["char-zhangxiu"] = "张绣",
            ["char-kongrong"] = "孔融",
            ["char-hansui"] = "韩遂",
            ["char-shixie"] = "士燮",
            // 部将（武将目录 #2）。
            ["char-guanyu"] = "关羽",
            ["char-zhangfei"] = "张飞",
            ["char-zhaoyun"] = "赵云",
            ["char-zhugeliang"] = "诸葛亮",
            ["char-machao"] = "马超",
            ["char-huangzhong"] = "黄忠",
            ["char-weiyan"] = "魏延",
            ["char-pangtong"] = "庞统",
            ["char-jiangwei"] = "姜维",
            ["char-xiahoudun"] = "夏侯惇",
            ["char-zhangliao"] = "张辽",
            ["char-xuchu"] = "许褚",
            ["char-dianwei"] = "典韦",
            ["char-guojia"] = "郭嘉",
            ["char-xunyu"] = "荀彧",
            ["char-simayi"] = "司马懿",
            ["char-caoren"] = "曹仁",
            ["char-jiaxu"] = "贾诩",
            ["char-zhouyu"] = "周瑜",
            ["char-lusu"] = "鲁肃",
            ["char-lvmeng"] = "吕蒙",
            ["char-luxun"] = "陆逊",
            ["char-taishici"] = "太史慈",
            ["char-ganning"] = "甘宁",
            ["char-huanggai"] = "黄盖",
            ["char-yanliang"] = "颜良",
            ["char-wenchou"] = "文丑",
            ["char-gaoshun"] = "高顺",
            ["char-chengong"] = "陈宫",
            ["char-huaxiong"] = "华雄",
            ["char-haozhao"] = "郝昭",
        };

        /// <summary>取某 id 的中文展示名；未登记则回退原文。</summary>
        public static string Of(string id)
            => Names.TryGetValue(id, out string? name) ? name : id;
    }
}
