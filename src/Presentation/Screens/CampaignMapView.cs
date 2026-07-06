using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
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

    /// <summary>
    /// 战略地图·一员在场武将的投影：id + 中文名 + 所在城 + 效力势力（随城归属）。坐标/立绘由 Unity 侧补。
    /// <b>反全知</b>（GDD_026 R2/R6）：己方城/传世名将/已探知势力方露真名；余者 <see cref="Known"/>=false，
    /// 仅知「某城有敌将」而不知其谁（id 空、名作「未探明」）——占城或派探方发觉。
    /// </summary>
    public sealed class MapHeroCell
    {
        public string HeroId { get; }
        public string HeroName { get; }
        public string CityId { get; }
        public string FactionId { get; }
        /// <summary>是否已识得此将身份（false=未探明，id/名不泄）。</summary>
        public bool Known { get; }

        internal MapHeroCell(string heroId, string heroName, string cityId, string factionId, bool known)
        {
            HeroId = heroId;
            HeroName = heroName;
            CityId = cityId;
            FactionId = factionId;
            Known = known;
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
    /// 地图所需的城/势力单元。城归属为明面；<b>武将棋子行反全知</b>（己方/名将/已探知露真名，余者未探明）；<b>无 Unity 依赖</b>——坐标/邻接/立绘等布局数据
    /// 由 Unity 侧 MapLayoutData 补。供 Unity 适配器映射到 scaffold 的 TerritoryViewModel/FactionViewModel。
    /// </summary>
    public sealed class CampaignMapView
    {
        /// <summary>全部城（按 id 序）。</summary>
        public IReadOnlyList<MapCityCell> Cities { get; }
        /// <summary>存续势力（按争霸态）。</summary>
        public IReadOnlyList<MapFactionCell> Factions { get; }
        /// <summary>在场武将棋子（按当前年在世 + 布防城；效力势力随城归属）。</summary>
        public IReadOnlyList<MapHeroCell> Heroes { get; }
        /// <summary>当前公元年。</summary>
        public int Year { get; }
        /// <summary>当前季（春/夏/秋/冬）。</summary>
        public string Season { get; }

        private CampaignMapView(IReadOnlyList<MapCityCell> cities, IReadOnlyList<MapFactionCell> factions,
            IReadOnlyList<MapHeroCell> heroes, int year, string season)
        {
            Cities = cities;
            Factions = factions;
            Heroes = heroes;
            Year = year;
            Season = season;
        }

        /// <summary>
        /// 从世界态构造地图投影：城归属取 <paramref name="world"/>（当前控制权），势力领城数取 <paramref name="contention"/>，
        /// 玩家势力标记 <paramref name="playerFaction"/>。中文名经 DisplayNames。
        /// </summary>
        /// <summary>传世名将（战阵绝世或谋略经天纬地）——行止乃天下所共知，反全知不掩其名。</summary>
        private static bool IsLegend(CharacterId g)
        {
            Domain.Characters.GeneralDossier? d = GeneralDossiers.Find(g);
            return d != null && (d.Prowess == Domain.Characters.CombatTier.Peerless || d.Strategy == Domain.Characters.StrategyTier.Master);
        }

        public static CampaignMapView Build(
            WorldState world, ContentionState contention, FactionId playerFaction, int currentYear, int anchorYear, string season,
            System.Func<FactionId, bool>? factionRevealed = null)
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

            // 在场武将棋子：该锚点年布防 ∩ 当前年在世；效力势力随其所在城当前归属（占城则易主）。
            // 反全知（GDD_026 R2/R6）：己方城 / 传世名将 / 已探知势力 → 露真名；余者仅显「未探明」，id/名不泄。
            var heroes = new List<MapHeroCell>();
            foreach ((CharacterId g, CityId city) in GeneralDossiers.AllPlacements(anchorYear))
            {
                if (!GeneralDossiers.AvailableAt(g, currentYear)) continue;   // 已故/未出仕 → 不在图
                FactionId? owner = world?.OwnershipOf(city)?.Owner;
                bool mine = owner.HasValue && owner.Value == playerFaction;
                bool scouted = owner.HasValue && factionRevealed != null && factionRevealed(owner.Value);
                bool known = mine || IsLegend(g) || scouted;
                heroes.Add(known
                    ? new MapHeroCell(g.Value, DisplayNames.Of(g.Value), city.Value, owner?.Value ?? string.Empty, true)
                    : new MapHeroCell(string.Empty, "未探明", city.Value, owner?.Value ?? string.Empty, false));
            }

            return new CampaignMapView(cities, factions, heroes, currentYear, season);
        }
    }
}
