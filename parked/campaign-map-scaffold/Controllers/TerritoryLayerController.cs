using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Manages the visual territory layer: spawning TerritoryView prefabs,
    /// handling selection/highlight state, and reacting to ownership changes.
    ///
    /// Attach to: CampaignMapScene/Layers/TerritoryLayer GameObject.
    /// </summary>
    public class TerritoryLayerController : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        public event Action<TerritoryViewModel> OnTerritoryClicked;
        public event Action<TerritoryViewModel> OnTerritoryHovered;

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Prefabs")]
        [SerializeField] private TerritoryView _territoryViewPrefab;

        [Header("Materials (URP)")]
        [SerializeField] private Material _defaultTerritoryMat;
        [SerializeField] private Material _selectedTerritoryMat;
        [SerializeField] private Material _highlightedTerritoryMat;
        [SerializeField] private Material _movementRangeMat;

        [Header("Faction Colours")]
        [SerializeField] private FactionColourMap _factionColourMap;

        // ── Internal state ─────────────────────────────────────────────────
        private readonly Dictionary<string, TerritoryView> _views = new();
        private string _currentSelectionId;
        private readonly HashSet<string> _highlighted = new();

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Called once by CampaignMapViewController at scene start.</summary>
        public void Initialise(IReadOnlyList<TerritoryViewModel> territories)
        {
            foreach (var territory in territories)
                SpawnTerritoryView(territory);
        }

        public void SetSelection(string territoryId)
        {
            ClearSelection();
            _currentSelectionId = territoryId;
            if (_views.TryGetValue(territoryId, out var view))
                view.SetState(TerritoryViewState.Selected, _selectedTerritoryMat);
        }

        public void ClearSelection()
        {
            if (_currentSelectionId != null && _views.TryGetValue(_currentSelectionId, out var prev))
                RestoreDefaultMaterial(prev);
            _currentSelectionId = null;
        }

        public void HighlightMovementRange(string originTerritoryId, int moveRange)
        {
            ClearHighlights();
            // TODO: call your domain graph traversal to get reachable territory IDs
            // var reachable = _mapGraph.GetReachableIds(originTerritoryId, moveRange);
            // foreach (var id in reachable) HighlightTerritory(id);
            Debug.Log($"[TerritoryLayer] Highlight move range {moveRange} from {originTerritoryId} — wire to domain graph.");
        }

        public void ClearHighlights()
        {
            foreach (var id in _highlighted)
            {
                if (_views.TryGetValue(id, out var view))
                    RestoreDefaultMaterial(view);
            }
            _highlighted.Clear();
        }

        public void ClearAllHighlights()
        {
            ClearHighlights();
            ClearSelection();
        }

        public void RefreshTerritoryOwner(string territoryId, Color factionColor)
        {
            if (!_views.TryGetValue(territoryId, out var view)) return;
            view.SetFactionColor(factionColor);
        }

        // ── Spawn ──────────────────────────────────────────────────────────
        private void SpawnTerritoryView(TerritoryViewModel vm)
        {
            var view = Instantiate(_territoryViewPrefab, transform);
            view.name = $"Territory_{vm.Id}_{vm.NameChinese}";

            // TODO: set world position from your map layout data
            // view.transform.position = _mapLayout.GetWorldPosition(vm.Id);
            view.transform.position = vm.WorldPosition;

            var factionColor = _factionColourMap.GetColor(vm.OwnerFactionId);
            view.Initialise(vm, factionColor);

            view.OnClicked += () => OnTerritoryClicked?.Invoke(vm);
            view.OnHovered += () => OnTerritoryHovered?.Invoke(vm);

            _views[vm.Id] = view;
        }

        private void HighlightTerritory(string territoryId)
        {
            if (!_views.TryGetValue(territoryId, out var view)) return;
            view.SetState(TerritoryViewState.MovementRange, _movementRangeMat);
            _highlighted.Add(territoryId);
        }

        private void RestoreDefaultMaterial(TerritoryView view)
        {
            view.SetState(TerritoryViewState.Default, _defaultTerritoryMat);
        }
    }
}
