using System;

namespace ThreeKingdom.Presentation.CampaignMap
{
    // ──────────────────────────────────────────────────────────────────────
    // Interfaces the Presentation layer depends on.
    // Implemented in your Application or Infrastructure layers.
    //
    // The Presentation layer knows ONLY these interfaces — never concrete
    // domain types directly.
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Read-only queries for the campaign map state.
    /// Implement in your Application layer, backed by domain repositories.
    /// </summary>
    public interface ICampaignMapQueryService
    {
        MapSnapshot GetCurrentMapSnapshot();
        TerritoryViewModel GetTerritory(string territoryId);
        HeroTokenViewModel GetHero(string heroId);
    }

    /// <summary>
    /// Commands the player can issue on the campaign map.
    /// Implement in your Application layer as use case orchestrators.
    /// </summary>
    public interface ICampaignActionService
    {
        void Execute(ExecuteActionRequest request);
        bool CanExecute(ExecuteActionRequest request);
    }

    /// <summary>
    /// Simple typed publish/subscribe bus.
    /// Implement once (e.g. with a dictionary of delegates) and reuse across scenes.
    /// </summary>
    public interface IGameEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T evt);
    }

    /// <summary>
    /// Looks up world-space positions for territory IDs.
    /// Keeps map layout data out of domain entities.
    /// </summary>
    public interface ITerritoryPositionLookup
    {
        UnityEngine.Vector3 GetWorldPosition(string territoryId);
        UnityEngine.Bounds  GetMapBounds();
    }
}
