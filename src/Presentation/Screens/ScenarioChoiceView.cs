using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 势力选择屏一行（#1 多剧本）：一个可选开局的展示投影——开局 id + 中文名 + 情境副文案 + 治所/目标中文名。
    /// 纯只读，供 Unity 选择屏渲染；玩家选定后以其 <see cref="Id"/> 经 <see cref="PlayableStartCatalog.ById"/> → 开局。
    /// </summary>
    public sealed class ScenarioChoiceLine
    {
        /// <summary>稳定开局 id（选定键）。</summary>
        public string Id { get; }
        /// <summary>开局名（如「刘玄德·小沛」）。</summary>
        public string Name { get; }
        /// <summary>一句话情境。</summary>
        public string Blurb { get; }
        /// <summary>起家治所中文名。</summary>
        public string CapitalName { get; }
        /// <summary>首要出征目标城中文名。</summary>
        public string TargetName { get; }

        internal ScenarioChoiceLine(PlayableStart start)
        {
            Id = start.Id;
            Name = start.DisplayName;
            Blurb = start.Blurb;
            CapitalName = DisplayNames.Of(start.Capital.Value);
            TargetName = DisplayNames.Of(start.OffensiveTarget.Value);
        }
    }

    /// <summary>
    /// 势力选择屏投影（#1）：列出全部可选开局供玩家挑选。数据驱动——新增开局只需登记一条
    /// <see cref="PlayableStart"/>，此屏自动出现（运行期零改动）。
    /// </summary>
    public sealed class ScenarioChoiceView
    {
        /// <summary>可选开局列表（目录遍历序；默认开局居首）。</summary>
        public IReadOnlyList<ScenarioChoiceLine> Choices { get; }
        /// <summary>缺省选中的开局 id（未选择时）。</summary>
        public string DefaultId { get; }

        private ScenarioChoiceView(IReadOnlyList<ScenarioChoiceLine> choices, string defaultId)
        {
            Choices = choices;
            DefaultId = defaultId;
        }

        /// <summary>从可选开局目录构造选择屏投影。</summary>
        public static ScenarioChoiceView FromCatalog()
        {
            var lines = new List<ScenarioChoiceLine>();
            foreach (PlayableStart s in PlayableStartCatalog.All) lines.Add(new ScenarioChoiceLine(s));
            return new ScenarioChoiceView(lines, PlayableStartCatalog.Default.Id);
        }
    }
}
