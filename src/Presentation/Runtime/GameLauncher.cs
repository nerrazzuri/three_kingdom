using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>一个可选锚点年（GDD_026 R1）：公元年 + 命名 + 情境。当前仅 190 讨董，后续补 200/208/220/184。</summary>
    public sealed class AnchorYearLine
    {
        public int Year { get; }
        public string Label { get; }
        public string Blurb { get; }
        internal AnchorYearLine(int year, string label, string blurb) { Year = year; Label = label; Blurb = blurb; }
    }

    /// <summary>
    /// 开局串联（GDD_026 §13 / ADR-0015 D3）：把「选锚点年 → 选城/选剧本 → 开局」连成一条运行期流程——
    /// 纯 C# 无 UnityEngine，供 Unity 主菜单薄壳与 console/测试共用。选定后产出一个已开新局的 <see cref="CampaignRuntime"/>。
    /// </summary>
    public static class GameLauncher
    {
        /// <summary>可选锚点年（GDD_026：当前仅 190 讨董之世）。</summary>
        public static IReadOnlyList<AnchorYearLine> AnchorYears() => new[]
        {
            new AnchorYearLine(190, "讨董·公元190", "群雄讨董卓，天下方乱。诸侯并起，正是空降者立业之始。"),
        };

        /// <summary>命名开局（精选剧本：汜水关太守 / 刘备·小沛 / 孙策·江东）。</summary>
        public static ScenarioChoiceView NamedStarts() => ScenarioChoiceView.FromCatalog();

        /// <summary>某锚点年可做太守的城（任选城开局）。</summary>
        public static GovernorCityChoiceView GovernorCities(int anchorYear = 190) => GovernorCityChoiceView.Build(anchorYear);

        /// <summary>以某命名剧本开局，返回已开新局的运行期。</summary>
        public static CampaignRuntime StartNamed(string startId, ISaveMedium medium, string slot = CampaignRuntime.DefaultSlot)
        {
            PlayableStart? start = PlayableStartCatalog.ById(startId);
            if (start == null) throw new ArgumentException($"无此命名开局：{startId}", nameof(startId));
            var runtime = new CampaignRuntime(medium, PlayableCampaign.ForStart(start), slot);
            runtime.NewGame();
            return runtime;
        }

        /// <summary>空降为某城太守开局（任选城；该年该城武将归你），返回已开新局的运行期。</summary>
        public static CampaignRuntime StartGovernor(string cityId, ISaveMedium medium, string slot = CampaignRuntime.DefaultSlot)
        {
            PlayableStart start = PlayableCampaign.GovernorStartOf(new CityId(cityId));
            var runtime = new CampaignRuntime(medium, PlayableCampaign.ForStart(start), slot);
            runtime.NewGame();
            return runtime;
        }
    }
}
