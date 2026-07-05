using System.Collections.Generic;
using UnityEngine;
using ThreeKingdom.Unity.UI;                 // SessionRuntime（Unity 壳桥，接 CampaignRuntime）
using ScreenViews = ThreeKingdom.Presentation.Screens;   // 纯 C# 地图投影 CampaignMapView

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// 战略地图数据适配器（scaffold ICampaignMapQueryService + ITerritoryPositionLookup 实现）：
    /// 把我们已单测的世界投影 <see cref="ScreenViews.CampaignMapView"/>（经 <see cref="SessionRuntime.MapView"/>）
    /// 映射为 scaffold 的 <see cref="MapSnapshot"/>/<see cref="TerritoryViewModel"/>/<see cref="FactionViewModel"/>。
    /// 坐标/邻接为地图布局数据（保持在表现层，不入领域，见 SCAFFOLD_README §4）。英雄棋子 MVP 暂空。
    /// ★这些为 Unity MonoBehaviour，无法无头编译，需在编辑器验证（挂到 ServiceAdapters）。
    /// </summary>
    public sealed class CampaignMapDataAdapter : MonoBehaviour, ICampaignMapQueryService, ITerritoryPositionLookup
    {
        // ── 城世界坐标（表现层布局；粗略按地理方位摆放，编辑器可再调）──
        private static readonly Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>
        {
            // 北方（袁绍/公孙瓒）
            ["city-ye"] = new Vector3(0, 0, 8), ["city-nanpi"] = new Vector3(3, 0, 9),
            ["city-pingyuan"] = new Vector3(1, 0, 6), ["city-jinyang"] = new Vector3(-4, 0, 7),
            ["city-beiping"] = new Vector3(6, 0, 11), ["city-jicheng"] = new Vector3(4, 0, 11),
            // 中原（曹操/吕布/袁术/李傕/张绣）
            ["city-xuchang"] = new Vector3(0, 0, 2), ["city-puyang"] = new Vector3(1, 0, 4),
            ["city-chenliu"] = new Vector3(-1, 0, 3), ["city-juancheng"] = new Vector3(0, 0, 4),
            ["city-xiapi"] = new Vector3(4, 0, 2), ["city-xuzhou"] = new Vector3(5, 0, 3),
            ["city-xiaopei"] = new Vector3(3, 0, 3), ["city-shouchun"] = new Vector3(4, 0, 0),
            ["city-hulao"] = new Vector3(-1, 0, 3), ["city-runan"] = new Vector3(2, 0, 1),
            ["city-luoyang"] = new Vector3(-3, 0, 3), ["city-changan"] = new Vector3(-6, 0, 3),
            ["city-wancheng"] = new Vector3(-2, 0, 1), ["city-fanshui"] = new Vector3(-1.5f, 0, 3.3f),
            // 西凉（马腾/韩遂）
            ["city-xiliang"] = new Vector3(-9, 0, 5), ["city-wuwei"] = new Vector3(-10, 0, 6),
            ["city-hanyang"] = new Vector3(-8, 0, 4),
            // 益州（刘璋/张鲁）
            ["city-chengdu"] = new Vector3(-6, 0, -3), ["city-jiangzhou"] = new Vector3(-4, 0, -4),
            ["city-zitong"] = new Vector3(-5, 0, -2), ["city-hanzhong"] = new Vector3(-5, 0, 0),
            // 荆州（刘表）
            ["city-xiangyang"] = new Vector3(-1, 0, -2), ["city-jiangling"] = new Vector3(-1, 0, -4),
            ["city-jiangxia"] = new Vector3(1, 0, -3), ["city-changsha"] = new Vector3(0, 0, -6),
            // 江东（孙）
            ["city-jianye"] = new Vector3(5, 0, -3), ["city-wujun"] = new Vector3(6, 0, -4),
            ["city-kuaiji"] = new Vector3(6, 0, -6), ["city-lujiang"] = new Vector3(4, 0, -2),
            // 其他
            ["city-beihai"] = new Vector3(4, 0, 5), ["city-jiaozhou"] = new Vector3(2, 0, -10),
        };

        // ── 城邻接（粗略战略相邻；供移动范围高亮，编辑器可再调）──
        private static readonly Dictionary<string, string[]> Adjacency = new Dictionary<string, string[]>
        {
            ["city-xuchang"] = new[] { "city-chenliu", "city-puyang", "city-runan", "city-hulao" },
            ["city-chenliu"] = new[] { "city-xuchang", "city-hulao", "city-juancheng" },
            ["city-hulao"] = new[] { "city-luoyang", "city-chenliu", "city-fanshui", "city-runan" },
            ["city-luoyang"] = new[] { "city-changan", "city-hulao", "city-wancheng" },
            ["city-changan"] = new[] { "city-luoyang", "city-hanyang", "city-hanzhong" },
            ["city-xiapi"] = new[] { "city-xuzhou", "city-xiaopei", "city-shouchun" },
            ["city-xiaopei"] = new[] { "city-xiapi", "city-runan", "city-shouchun" },
            ["city-shouchun"] = new[] { "city-runan", "city-xiaopei", "city-lujiang" },
            ["city-ye"] = new[] { "city-pingyuan", "city-nanpi", "city-jinyang" },
            ["city-xiangyang"] = new[] { "city-wancheng", "city-jiangling", "city-jiangxia", "city-hanzhong" },
            ["city-jianye"] = new[] { "city-wujun", "city-lujiang", "city-jiangxia" },
            ["city-chengdu"] = new[] { "city-zitong", "city-jiangzhou", "city-hanzhong" },
            ["city-xiliang"] = new[] { "city-wuwei", "city-hanyang" },
        };

        private static readonly Dictionary<string, Color> FactionColors = new Dictionary<string, Color>
        {
            ["faction-player"] = new Color(0.95f, 0.80f, 0.20f),   // 金
            ["faction-cao"] = new Color(0.25f, 0.45f, 0.85f),      // 蓝（魏）
            ["faction-liubei"] = new Color(0.85f, 0.25f, 0.25f),   // 红（蜀）
            ["faction-sun"] = new Color(0.25f, 0.70f, 0.40f),      // 绿（吴）
            ["faction-yuanshao"] = new Color(0.55f, 0.35f, 0.75f),
            ["faction-lubu"] = new Color(0.80f, 0.50f, 0.20f),
        };

        // ── ICampaignMapQueryService ──
        public MapSnapshot GetCurrentMapSnapshot()
        {
            ScreenViews.CampaignMapView map = SessionRuntime.MapView();

            var territories = new List<TerritoryViewModel>();
            foreach (ScreenViews.MapCityCell c in map.Cities)
                territories.Add(ToTerritory(c));

            var factions = new List<FactionViewModel>();
            foreach (ScreenViews.MapFactionCell f in map.Factions)
                factions.Add(new FactionViewModel
                {
                    Id = f.FactionId,
                    NameChinese = f.FactionName,
                    PrimaryColor = ColorOf(f.FactionId),
                    TotalTroops = 0,
                    TerritoryCount = f.CityCount,
                    IsPlayerControlled = f.IsPlayer,
                });

            var heroes = new List<HeroPositionViewModel>();
            foreach (ScreenViews.MapHeroCell h in map.Heroes)
                heroes.Add(ToHero(h));

            return new MapSnapshot
            {
                Territories = territories,
                HeroPositions = heroes,   // 在场武将棋子（按城摆位，立绘经 Addressables 后续）
                Factions = factions,
                CurrentWeather = WeatherType.Clear,
                CurrentTurn = map.Year,
                CurrentPhase = TurnPhase.Politics,
                MapBounds = GetMapBounds(),
            };
        }

        public TerritoryViewModel GetTerritory(string territoryId)
        {
            foreach (ScreenViews.MapCityCell c in SessionRuntime.MapView().Cities)
                if (c.CityId == territoryId) return ToTerritory(c);
            return null;
        }

        public HeroTokenViewModel GetHero(string heroId)
        {
            foreach (ScreenViews.MapHeroCell h in SessionRuntime.MapView().Heroes)
                if (h.HeroId == heroId) return ToHero(h).ToViewModel();
            return null;
        }

        private static HeroPositionViewModel ToHero(ScreenViews.MapHeroCell h) => new HeroPositionViewModel
        {
            HeroId = h.HeroId,
            HeroNameChinese = h.HeroName,
            FactionId = h.FactionId,
            TerritoryId = h.CityId,
            MoveRange = 2,
            PortraitSprite = null,   // 经 Addressables 按 HeroId 异步加载（美术后续）
            InitialWorldPosition = GetWorldPosition(h.CityId),
        };

        private static TerritoryViewModel ToTerritory(ScreenViews.MapCityCell c) => new TerritoryViewModel
        {
            Id = c.CityId,
            NameChinese = c.CityName,
            NamePinyin = string.Empty,
            OwnerFactionId = c.OwnerFactionId,
            TroopCount = 0,
            FoodSupply = 0,
            HasCity = true,
            IsCapital = c.IsCapital,
            WorldPosition = GetWorldPosition(c.CityId),
            AdjacentTerritoryIds = Adjacency.TryGetValue(c.CityId, out var adj) ? adj : System.Array.Empty<string>(),
        };

        private static Color ColorOf(string factionId)
            => FactionColors.TryGetValue(factionId, out Color col) ? col : new Color(0.6f, 0.6f, 0.6f);

        // ── ITerritoryPositionLookup ──
        public static Vector3 GetWorldPosition(string territoryId)
            => Positions.TryGetValue(territoryId, out Vector3 p) ? p : Vector3.zero;

        Vector3 ITerritoryPositionLookup.GetWorldPosition(string territoryId) => GetWorldPosition(territoryId);

        public Bounds GetMapBounds()
        {
            var b = new Bounds(Vector3.zero, Vector3.zero);
            bool first = true;
            foreach (var kv in Positions)
            {
                if (first) { b = new Bounds(kv.Value, Vector3.zero); first = false; }
                else b.Encapsulate(kv.Value);
            }
            b.Expand(4f);   // 留边
            return b;
        }
    }
}
