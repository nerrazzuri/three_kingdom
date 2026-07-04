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
            ["faction-yuan"] = "曹魏前锋",
            ["region-pass"] = "隘口",
            ["city-fanshui"] = "汜水关",
        };

        /// <summary>取某 id 的中文展示名；未登记则回退原文。</summary>
        public static string Of(string id)
            => Names.TryGetValue(id, out string? name) ? name : id;
    }
}
