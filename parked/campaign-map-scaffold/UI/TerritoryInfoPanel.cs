using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;  // TODO: add DOTween to your project, or replace with coroutine tweens

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Side panel showing selected territory details.
    ///
    /// Prefab hierarchy:
    ///   TerritoryInfoPanel (Canvas/Panel)
    ///   ├── Header
    ///   │   ├── NameLabel (TMP)          — "洛陽 · Luòyáng"
    ///   │   └── FactionBadge (Image)
    ///   ├── StatsGrid
    ///   │   ├── TroopsRow
    ///   │   ├── FoodRow
    ///   │   └── DefenseRow
    ///   ├── CityIcon (Image, shown if HasCity)
    ///   └── CapitalBadge (Image, shown if IsCapital)
    /// </summary>
    public class TerritoryInfoPanel : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text _nameChinese;
        [SerializeField] private TMP_Text _namePinyin;
        [SerializeField] private TMP_Text _factionName;
        [SerializeField] private TMP_Text _troopCount;
        [SerializeField] private TMP_Text _foodSupply;

        [Header("Icons")]
        [SerializeField] private Image _factionBadge;
        [SerializeField] private GameObject _cityIcon;
        [SerializeField] private GameObject _capitalBadge;

        [Header("Tooltip (hover)")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TMP_Text   _tooltipNameLabel;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Coroutine _tooltipCoroutine;

        // ── Public API ─────────────────────────────────────────────────────
        public void Show(TerritoryViewModel vm)
        {
            Populate(vm);
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, _fadeDuration);
        }

        public void Hide()
        {
            _canvasGroup.DOFade(0f, _fadeDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void ShowTooltip(TerritoryViewModel vm)
        {
            _tooltipNameLabel.text = $"{vm.NameChinese}";
            _tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            _tooltipPanel.SetActive(false);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private void Populate(TerritoryViewModel vm)
        {
            _nameChinese.text = vm.NameChinese;
            _namePinyin.text  = vm.NamePinyin;
            _troopCount.text  = vm.TroopCount.ToString("N0");
            _foodSupply.text  = vm.FoodSupply.ToString("N0");

            _cityIcon.SetActive(vm.HasCity);
            _capitalBadge.SetActive(vm.IsCapital);

            // TODO: set _factionBadge.sprite from your faction sprite atlas
            // TODO: set _factionName.text from faction lookup
        }
    }
}
