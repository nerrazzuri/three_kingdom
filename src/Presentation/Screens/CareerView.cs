using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 生涯展示投影（GDD_014 / W5）：官阶中文头衔 + 功绩/名望 + 是否在野（自立/流亡）。纯只读——供 HUD 显示你的仕途。
    /// 忠臣线：太守 → 上守 → 刺史 → 中郎将 → 镇军将军 → 副都督 → 大都督 → 继业之主。
    /// </summary>
    public sealed class CareerView
    {
        /// <summary>官阶中文头衔。</summary>
        public string RankTitle { get; }
        /// <summary>功绩（战功/治绩累积；晋升凭据之一）。</summary>
        public int Merit { get; }
        /// <summary>名望（声望；晋升凭据 + 被俘时影响生死/收留）。</summary>
        public int Renown { get; }
        /// <summary>是否在野（自立/流亡，暂不奉主）。</summary>
        public bool IsUnaffiliated { get; }

        internal CareerView(CareerState c)
        {
            RankTitle = IsUnaffiliatedOf(c) ? "在野之身" : TitleOf(c.Rank);
            Merit = c.Merit;
            Renown = c.Renown;
            IsUnaffiliated = IsUnaffiliatedOf(c);
        }

        private static bool IsUnaffiliatedOf(CareerState c) => c.IsUnaffiliated;

        /// <summary>官阶 → 中文头衔（忠臣线八阶）。</summary>
        public static string TitleOf(Rank rank) => rank switch
        {
            Rank.CityGovernor => "太守",
            Rank.SeniorGovernor => "上守",
            Rank.ProvincialInspector => "刺史",
            Rank.RegionalGeneral => "中郎将",
            Rank.GuardianGeneral => "镇军将军",
            Rank.DeputyCommander => "副都督",
            Rank.GrandCommander => "大都督",
            Rank.Successor => "继业之主",
            _ => rank.ToString(),
        };
    }
}
