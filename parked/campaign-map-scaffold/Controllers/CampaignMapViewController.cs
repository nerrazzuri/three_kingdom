using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: Replace these using directives with your actual domain/application namespaces
// e.g. using ThreeKingdom.Domain.Entities;
//      using ThreeKingdom.Application.UseCases;
//      using ThreeKingdom.Application.Events;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Root Presentation layer controller for the Campaign Map screen.
    /// Follows MVP pattern: this is the Presenter. It owns no game logic —
    /// it translates domain state into view commands and user input into
    /// application-layer use case calls.
    ///
    /// Attach to: CampaignMapScene root GameObject.
    /// </summary>
    public class CampaignMapViewController : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────
        [Header("Sub-Controllers")]
        [SerializeField] private TerritoryLayerController _territoryLayer;
        [SerializeField] private HeroTokenLayerController _heroTokenLayer;
        [SerializeField] private WeatherOverlayController _weatherOverlay;
        [SerializeField] private CampaignMapCameraController _cameraController;

        [Header("UI Panels")]
        [SerializeField] private TerritoryInfoPanel _territoryInfoPanel;
        [SerializeField] private FactionStatusBar _factionStatusBar;
        [SerializeField] private TurnPhaseHUD _turnPhaseHUD;
        [SerializeField] private ActionMenuPanel _actionMenuPanel;

        [Header("Config")]
        [SerializeField] private CampaignMapConfig _config;

        // ── Application-layer dependencies (inject via your DI or ServiceLocator) ──
        // TODO: Replace these with your actual application service interfaces
        private ICampaignMapQueryService _mapQueryService;
        private ICampaignActionService _campaignActionService;
        private IGameEventBus _eventBus;

        // ── Internal state ─────────────────────────────────────────────────
        private TerritoryViewModel _selectedTerritory;
        private bool _isAwaitingPlayerAction;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            ResolveServices();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            InitialiseMap();
        }

        // ── Initialisation ─────────────────────────────────────────────────
        private void ResolveServices()
        {
            // TODO: Replace with your actual DI / ServiceLocator pattern
            // Example: _mapQueryService = ServiceLocator.Get<ICampaignMapQueryService>();
            _mapQueryService      = FindObjectOfType<CampaignMapQueryServiceAdapter>();
            _campaignActionService = FindObjectOfType<CampaignActionServiceAdapter>();
            _eventBus             = FindObjectOfType<GameEventBusAdapter>();

            if (_mapQueryService == null)
                Debug.LogError("[CampaignMapVC] ICampaignMapQueryService not found. Wire up your service locator.");
        }

        private void SubscribeToEvents()
        {
            // Domain / Application events → Presentation reactions
            _eventBus?.Subscribe<TerritoryOwnerChangedEvent>(OnTerritoryOwnerChanged);
            _eventBus?.Subscribe<HeroMovedEvent>(OnHeroMoved);
            _eventBus?.Subscribe<TurnAdvancedEvent>(OnTurnAdvanced);
            _eventBus?.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            _eventBus?.Subscribe<BattleStartedEvent>(OnBattleStarted);

            // Sub-controller events → this controller
            _territoryLayer.OnTerritoryClicked   += HandleTerritoryClicked;
            _territoryLayer.OnTerritoryHovered   += HandleTerritoryHovered;
            _heroTokenLayer.OnHeroTokenClicked   += HandleHeroTokenClicked;
            _actionMenuPanel.OnActionSelected    += HandleActionSelected;
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<TerritoryOwnerChangedEvent>(OnTerritoryOwnerChanged);
            _eventBus?.Unsubscribe<HeroMovedEvent>(OnHeroMoved);
            _eventBus?.Unsubscribe<TurnAdvancedEvent>(OnTurnAdvanced);
            _eventBus?.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
            _eventBus?.Unsubscribe<BattleStartedEvent>(OnBattleStarted);

            if (_territoryLayer != null)
            {
                _territoryLayer.OnTerritoryClicked -= HandleTerritoryClicked;
                _territoryLayer.OnTerritoryHovered -= HandleTerritoryHovered;
            }
            if (_heroTokenLayer != null)
                _heroTokenLayer.OnHeroTokenClicked -= HandleHeroTokenClicked;
            if (_actionMenuPanel != null)
                _actionMenuPanel.OnActionSelected  -= HandleActionSelected;
        }

        private void InitialiseMap()
        {
            // TODO: Replace with your actual domain query
            // var mapSnapshot = _mapQueryService.GetCurrentMapSnapshot();
            var mapSnapshot = _mapQueryService?.GetCurrentMapSnapshot();
            if (mapSnapshot == null) return;

            _territoryLayer.Initialise(mapSnapshot.Territories);
            _heroTokenLayer.Initialise(mapSnapshot.HeroPositions);
            _weatherOverlay.Initialise(mapSnapshot.CurrentWeather);
            _factionStatusBar.Refresh(mapSnapshot.Factions);
            _turnPhaseHUD.Refresh(mapSnapshot.CurrentTurn, mapSnapshot.CurrentPhase);

            _cameraController.CentreOnMap(mapSnapshot.MapBounds);
        }

        // ── User input handlers ────────────────────────────────────────────
        private void HandleTerritoryClicked(TerritoryViewModel territory)
        {
            if (_selectedTerritory?.Id == territory.Id)
            {
                DeselectTerritory();
                return;
            }

            SelectTerritory(territory);
        }

        private void HandleTerritoryHovered(TerritoryViewModel territory)
        {
            _territoryInfoPanel.ShowTooltip(territory);
        }

        private void HandleHeroTokenClicked(HeroTokenViewModel heroToken)
        {
            // Show hero detail panel and highlight their movement range
            _territoryLayer.HighlightMovementRange(heroToken.CurrentTerritoryId, heroToken.MoveRange);
            _actionMenuPanel.ShowHeroActions(heroToken);
        }

        private void HandleActionSelected(CampaignAction action)
        {
            if (_selectedTerritory == null && action.RequiresTargetTerritory)
            {
                Debug.LogWarning("[CampaignMapVC] Action requires a territory selection.");
                return;
            }

            // TODO: Call your application-layer use case
            // _campaignActionService.Execute(action, _selectedTerritory?.Id);
            _campaignActionService?.Execute(new ExecuteActionRequest
            {
                Action      = action,
                TerritoryId = _selectedTerritory?.Id
            });

            _actionMenuPanel.Hide();
            DeselectTerritory();
        }

        // ── Domain event reactions ─────────────────────────────────────────
        private void OnTerritoryOwnerChanged(TerritoryOwnerChangedEvent evt)
        {
            // TODO: Map evt.TerritoryId and evt.NewOwnerId to your domain types
            _territoryLayer.RefreshTerritoryOwner(evt.TerritoryId, evt.NewFactionColor);
            _factionStatusBar.Refresh(evt.UpdatedFactions);
        }

        private void OnHeroMoved(HeroMovedEvent evt)
        {
            _heroTokenLayer.MoveHeroToken(evt.HeroId, evt.FromTerritoryId, evt.ToTerritoryId);
        }

        private void OnTurnAdvanced(TurnAdvancedEvent evt)
        {
            _turnPhaseHUD.Refresh(evt.NewTurn, evt.NewPhase);
            _weatherOverlay.Refresh(evt.NewWeather);
            _territoryLayer.ClearAllHighlights();
            _actionMenuPanel.Hide();
            DeselectTerritory();
        }

        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            _weatherOverlay.Refresh(evt.NewWeather);
        }

        private void OnBattleStarted(BattleStartedEvent evt)
        {
            // Transition to battle scene — keep map state in Application layer
            // TODO: wire to your SceneTransitionService
            Debug.Log($"[CampaignMapVC] Battle started at {evt.TerritoryId}. Transitioning...");
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private void SelectTerritory(TerritoryViewModel territory)
        {
            _selectedTerritory = territory;
            _territoryLayer.SetSelection(territory.Id);
            _territoryInfoPanel.Show(territory);
            _actionMenuPanel.ShowTerritoryActions(territory);
        }

        private void DeselectTerritory()
        {
            _selectedTerritory = null;
            _territoryLayer.ClearSelection();
            _territoryInfoPanel.Hide();
            _actionMenuPanel.Hide();
        }
    }
}
