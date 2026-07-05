using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdom.Presentation.CampaignMap
{
    // ──────────────────────────────────────────────────────────────────────
    // Presentation-layer event DTOs.
    //
    // These are raised by your Application layer (use cases / event handlers)
    // and consumed by the Presentation layer controllers.
    //
    // TODO: If your domain already defines equivalent events, map those
    // in your Application layer event handlers and remove these stubs.
    // ──────────────────────────────────────────────────────────────────────

    public class TerritoryOwnerChangedEvent
    {
        public string              TerritoryId    { get; init; }
        public string              NewOwnerId     { get; init; }
        public Color               NewFactionColor { get; init; }
        public IReadOnlyList<FactionViewModel> UpdatedFactions { get; init; }
    }

    public class HeroMovedEvent
    {
        public string HeroId           { get; init; }
        public string FromTerritoryId  { get; init; }
        public string ToTerritoryId    { get; init; }
    }

    public class TurnAdvancedEvent
    {
        public int         NewTurn    { get; init; }
        public TurnPhase   NewPhase   { get; init; }
        public WeatherType NewWeather { get; init; }
    }

    public class WeatherChangedEvent
    {
        public WeatherType NewWeather  { get; init; }
        public string      TerritoryId { get; init; }   // null = global
    }

    public class BattleStartedEvent
    {
        public string TerritoryId    { get; init; }
        public string AttackerHeroId { get; init; }
        public string DefenderHeroId { get; init; }
    }
}
