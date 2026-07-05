using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>战略地图·一座城的投影（纯数据，无 Unity 依赖）：id + 中文名 + 当前归属势力 + 是否治所。</summary>
    public sealed class MapCityCell
    {
        public string CityId { get; }
        public string CityName { get; }
        public string OwnerFactionId { get; }
        public string OwnerName { get; }
        public bool IsCapital { get; }

        internal MapCityCell(string cityId, string cityName, string ownerFactionId, string ownerName, bool isCapital)
        {
            CityId = cityId;
            CityName = cityName;
            OwnerFactionId = ownerFactionId;
            OwnerName = ownerName;
            IsCapital = isCapital;
        }
    }

    /// <summary>战略地图·一方势力的投影：id + 中文名 + 领城数 + 是否玩家。</summary>
    public sealed class MapFactionCell
    {
        public string FactionId { get; }
        public string FactionName { get; }
        public int CityCount { get; }
        public bool IsPlayer { get; }

        internal MapFactionCell(string factionId, string factionName, int cityCount, bool isPlayer)
        {
            FactionId = factionId;
            FactionName = factionName;
            CityCount = cityCount;
            IsPlayer = isPlayer;
        }
    }

    /// <summary>
    /// 战略大地图投影（campaign map scaffold 的纯 C# 数据源）：把世界大盘（城归属 + 争霸态）投影为
    /// 地图所需的城/势力单元。<b>反全知不涉</b>（归属为明面）；<b>无 Unity 依赖</b>——坐标/邻接/立绘等布局数据
    /// 由 Unity 侧 MapLayoutData 补。供 Unity 适配器映射到 scaffold 的 TerritoryViewModel/FactionViewModel。
    /// </summary>
    public sealed class CampaignMapView
    {
        /// <summary>全部城（按 id 序）。</summary>
        public IReadOnlyList<MapCityCell> Cities { get; }
        /// <summary>存续势力（按争霸态）。</summary>
        public IReadOnlyList<MapFactionCell> Factions { get; }
        /// <summary>当前公元年。</summary>
        public int Year { get; }
        /// <summary>当前季（春/夏/秋/冬）。</summary>
        public string Season { get; }

        private CampaignMapView(IReadOnlyList<MapCityCell> cities, IReadOnlyList<MapFactionCell> factions, int year, string season)
        {
            Cities = cities;
            Factions = factions;
            Year = year;
            Season = season;
        }

        /// <summary>
        /// 从世界态构造地图投影：城归属取 <paramref name="world"/>（当前控制权），势力领城数取 <paramref name="contention"/>，
        /// 玩家势力标记 <paramref name="playerFaction"/>。中文名经 DisplayNames。
        /// </summary>
        public static CampaignMapView Build(WorldState world, ContentionState contention, FactionId playerFaction, int year, string season)
        {
            var cities = new List<MapCityCell>();
            foreach (CityId c in PlayableCampaign.AllWorldCities())
            {
                FactionId? owner = world?.OwnershipOf(c)?.Owner;
                string ownerId = owner?.Value ?? string.Empty;
                cities.Add(new MapCityCell(
                    c.Value, DisplayNames.Of(c.Value),
                    ownerId, owner.HasValue ? DisplayNames.Of(ownerId) : "—",
                    PlayableCampaign.IsCapitalCity(c)));
            }

            var factions = new List<MapFactionCell>();
            if (contention != null)
                foreach (PowerStanding p in contention.Powers)
                {
                    if (!p.Alive) continue;
                    factions.Add(new MapFactionCell(
                        p.Faction.Value, DisplayNames.Of(p.Faction.Value), p.Cities, p.Faction == playerFaction));
                }

            return new CampaignMapView(cities, factions, year, season);
        }
    }
}
