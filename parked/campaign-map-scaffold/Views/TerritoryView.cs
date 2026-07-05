using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeKingdom.Presentation.CampaignMap
{
    public enum TerritoryViewState { Default, Selected, Highlighted, MovementRange }

    /// <summary>
    /// Prefab script for a single territory on the campaign map.
    /// Handles material state, faction colour tinting, and pointer events.
    ///
    /// Prefab setup:
    ///   - MeshFilter + MeshRenderer (territory polygon mesh)
    ///   - MeshCollider or PolygonCollider2D for click detection
    ///   - Child: NameLabel (TMP_Text)
    ///   - Child: FlagIcon (SpriteRenderer)
    ///   - Child: CityIcon (SpriteRenderer, shown if territory has a city)
    /// </summary>
    public class TerritoryView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ── Events ─────────────────────────────────────────────────────────
        public event Action OnClicked;
        public event Action OnHovered;

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Renderers")]
        [SerializeField] private Renderer _meshRenderer;
        [SerializeField] private SpriteRenderer _flagIcon;
        [SerializeField] private SpriteRenderer _cityIcon;

        [Header("Labels")]
        [SerializeField] private TMPro.TMP_Text _nameLabel;
        [SerializeField] private TMPro.TMP_Text _troopCountLabel;

        [Header("Animations")]
        [SerializeField] private Animator _animator;
        private static readonly int SelectedHash   = Animator.StringToHash("Selected");
        private static readonly int HighlightHash  = Animator.StringToHash("Highlighted");

        // ── State ──────────────────────────────────────────────────────────
        private TerritoryViewModel _viewModel;
        private TerritoryViewState _currentState;
        private MaterialPropertyBlock _propBlock;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int RimColorId  = Shader.PropertyToID("_RimColor");

        // ── Public API ─────────────────────────────────────────────────────
        public void Initialise(TerritoryViewModel vm, Color factionColor)
        {
            _viewModel   = vm;
            _propBlock   = new MaterialPropertyBlock();

            _nameLabel.text       = vm.NameChinese;   // e.g. "洛陽"
            _troopCountLabel.text = vm.TroopCount.ToString();
            _cityIcon.enabled     = vm.HasCity;

            SetFactionColor(factionColor);
            SetState(TerritoryViewState.Default, null);
        }

        public void SetState(TerritoryViewState newState, Material overrideMaterial)
        {
            _currentState = newState;

            if (overrideMaterial != null)
                _meshRenderer.material = overrideMaterial;

            _animator?.SetBool(SelectedHash,  newState == TerritoryViewState.Selected);
            _animator?.SetBool(HighlightHash, newState == TerritoryViewState.Highlighted
                                           || newState == TerritoryViewState.MovementRange);
        }

        public void SetFactionColor(Color color)
        {
            _meshRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorId, color);
            _meshRenderer.SetPropertyBlock(_propBlock);

            _flagIcon.color = color;
        }

        public void RefreshTroopCount(int count)
        {
            _troopCountLabel.text = count > 0 ? count.ToString("N0") : "—";
        }

        // ── Pointer events ─────────────────────────────────────────────────
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHovered?.Invoke();
            if (_currentState == TerritoryViewState.Default)
                _animator?.SetBool(HighlightHash, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_currentState == TerritoryViewState.Default)
                _animator?.SetBool(HighlightHash, false);
        }
    }
}
