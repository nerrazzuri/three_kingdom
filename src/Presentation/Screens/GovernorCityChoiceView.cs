using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 选城屏一行（GDD_026 R3/§13）：一座可做太守的城——中文名 + 所属势力（宗主）+ 该年在职部将人数。
    /// 反全知：只给"有几员将"，不投影其数值/隐藏档。玩家选定后以 <see cref="CityId"/> 经
    /// <see cref="PlayableCampaign.GovernorStartOf"/> → 开局。君主治所不入此列（不可为太守）。
    /// </summary>
    public sealed class GovernorCityChoiceLine
    {
        /// <summary>城 id（选定键）。</summary>
        public string CityId { get; }
        /// <summary>城中文名。</summary>
        public string CityName { get; }
        /// <summary>宗主势力中文名（该城原属势力）。</summary>
        public string SuzerainName { get; }
        /// <summary>该年该城在职部将人数（反全知——只给数目，不露其能力）。</summary>
        public int GeneralCount { get; }

        internal GovernorCityChoiceLine(CityId city, FactionId owner, int generalCount)
        {
            CityId = city.Value;
            CityName = DisplayNames.Of(city.Value);
            SuzerainName = DisplayNames.Of(owner.Value);
            GeneralCount = generalCount;
        }
    }

    /// <summary>
    /// 选城屏投影（GDD_026）：列出全部可做太守的城（非君主治所），供玩家挑选起家之地。
    /// 数据驱动——世界大盘增城/新增锚点年布防，此屏自动反映。
    /// </summary>
    public sealed class GovernorCityChoiceView
    {
        /// <summary>可选城列表（按 id 规范序）。</summary>
        public IReadOnlyList<GovernorCityChoiceLine> Choices { get; }

        private GovernorCityChoiceView(IReadOnlyList<GovernorCityChoiceLine> choices) => Choices = choices;

        /// <summary>从世界大盘构造某锚点年的选城屏（默认 190）。</summary>
        public static GovernorCityChoiceView Build(int anchorYear = 190)
        {
            var lines = new List<GovernorCityChoiceLine>();
            foreach (CityId c in PlayableCampaign.SelectableGovernorCities())
            {
                FactionId? owner = PlayableCampaign.OwnerOfCity(c);
                if (owner == null) continue;
                int count = GeneralDossiers.GeneralsAt(c, anchorYear).Count;
                lines.Add(new GovernorCityChoiceLine(c, owner.Value, count));
            }
            return new GovernorCityChoiceView(lines);
        }
    }
}
