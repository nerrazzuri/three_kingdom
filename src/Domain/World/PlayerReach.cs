using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 玩家势力圈（GDD_015 / ADR-0007：reachability 判定输入）。不可变。
    /// 记录玩家已据有/灭掉而"触及"的势力与城池——用于判定事件前置主体是否够得着。
    /// </summary>
    public sealed class PlayerReach
    {
        private readonly HashSet<FactionId> _factions;
        private readonly HashSet<CityId> _cities;

        public PlayerReach(IReadOnlyCollection<FactionId> reachedFactions, IReadOnlyCollection<CityId> reachedCities)
        {
            if (reachedFactions is null) throw new ArgumentNullException(nameof(reachedFactions));
            if (reachedCities is null) throw new ArgumentNullException(nameof(reachedCities));
            _factions = new HashSet<FactionId>(reachedFactions);
            _cities = new HashSet<CityId>(reachedCities);
        }

        /// <summary>空势力圈（开局触及不到任何远方历史主体）。</summary>
        public static PlayerReach None { get; } = new PlayerReach(Array.Empty<FactionId>(), Array.Empty<CityId>());

        /// <summary>玩家圈是否触及某势力。</summary>
        public bool Touches(FactionId faction) => _factions.Contains(faction);

        /// <summary>玩家圈是否触及某城。</summary>
        public bool Touches(CityId city) => _cities.Contains(city);
    }
}
