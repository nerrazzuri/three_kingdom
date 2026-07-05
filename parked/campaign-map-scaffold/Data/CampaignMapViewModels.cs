using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdom.Presentation.CampaignMap
{
    // ──────────────────────────────────────────────────────────────────────
    // ViewModels: flat, serialisable DTOs that cross the
    // Application → Presentation boundary.
    //
    // These are NOT domain entities. They carry only what the UI needs.
    // Map your domain entities to these in a Query/Mapper class (Application layer).
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Full map snapshot delivered on scene load.</summary>
    public class MapSnapshot
    {
        public IReadOnlyList<TerritoryViewModel>    Territories   { get; init; }
        public IReadOnlyList<HeroPositionViewModel> HeroPositions { get; init; }
        public IReadOnlyList<FactionViewModel>      Factions      { get; init; }
        public WeatherType                          CurrentWeather { get; init; }
        public int                                  CurrentTurn   { get; init; }
        public TurnPhase                            CurrentPhase  { get; init; }
        public Bounds                               MapBounds     { get; init; }
    }

    /// <summary>Everything the UI needs to render a single territory.</summary>
    public class TerritoryViewModel
    {
        // TODO: match field names to your domain Territory entity
        public string  Id             { get; init; }
        public string  NameChinese    { get; init; }   // e.g. "洛陽"
        public string  NamePinyin     { get; init; }   // e.g. "Luòyáng"
        public string  OwnerFactionId { get; init; }
        public int     TroopCount     { get; init; }
        public int     FoodSupply     { get; init; }
        public bool    HasCity        { get; init; }
        public bool    IsCapital      { get; init; }
        public Vector3 WorldPosition  { get; init; }

        // Adjacency used for movement range highlight
        public IReadOnlyList<string> AdjacentTerritoryIds { get; init; }
    }

    /// <summary>Hero position on the map (not full hero stats — that's the hero detail panel).</summary>
    public class HeroPositionViewModel
    {
        public string  HeroId           { get; init; }
        public string  HeroNameChinese  { get; init; }   // e.g. "關羽"
        public string  FactionId        { get; init; }
        public string  TerritoryId      { get; init; }
        public int     MoveRange        { get; init; }
        public Sprite  PortraitSprite   { get; init; }   // loaded from Addressables
        public Vector3 InitialWorldPosition { get; init; }

        public HeroTokenViewModel ToViewModel() => new()
        {
            HeroId              = HeroId,
            HeroNameChinese     = HeroNameChinese,
            FactionId           = FactionId,
            CurrentTerritoryId  = TerritoryId,
            MoveRange           = MoveRange,
            PortraitSprite      = PortraitSprite,
        };
    }

    public class HeroTokenViewModel
    {
        public string  HeroId             { get; init; }
        public string  HeroNameChinese    { get; init; }
        public string  FactionId          { get; init; }
        public string  CurrentTerritoryId { get; init; }
        public int     MoveRange          { get; init; }
        public Sprite  PortraitSprite     { get; init; }
    }

    public class FactionViewModel
    {
        public string  Id           { get; init; }
        public string  NameChinese  { get; init; }   // e.g. "魏"
        public Color   PrimaryColor { get; init; }
        public int     TotalTroops  { get; init; }
        public int     TerritoryCount { get; init; }
        public bool    IsPlayerControlled { get; init; }
    }

    // ── Enums ──────────────────────────────────────────────────────────────
    // TODO: replace with your domain enums if already defined

    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Fog,
        Storm
    }

    public enum TurnPhase
    {
        Politics,    // 政務
        Military,    // 軍事
        Diplomacy,   // 外交
        Event,       // 事件
        EndTurn      // 回合結束
    }

    public enum CampaignAction
    {
        Move,
        Attack,
        Recruit,
        BuildFort,
        Diplomacy,
        Supply,
        EndTurn
    }

    // ── Request types ──────────────────────────────────────────────────────
    public class ExecuteActionRequest
    {
        public CampaignAction Action      { get; init; }
        public string         TerritoryId { get; init; }
        public string         HeroId      { get; init; }
    }
}
