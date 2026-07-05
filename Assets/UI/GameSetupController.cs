using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 空降者开局屏控制器（GDD_026，Presentation 薄壳 / ADR-0002）：选锚点年 → 命名剧本 或 任选城做太守 → 开局。
    /// 逻辑（可选开局/选城/生成 PlayableStart）已由 dotnet 覆盖（PlayableStartCatalog/GovernorStart 测试）；
    /// 本类只把只读投影绑定到 UXML + 调 <see cref="SessionRuntime"/> 开局并切到 HUD 场景。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameSetupController : MonoBehaviour
    {
        [SerializeField] private string _hudScene = "Hud";

        private bool _cityMode;             // false=命名剧本, true=任选城
        private string _selectedId;         // 选中的开局 id 或城 id
        private bool _selectedIsCity;

        private ScrollView _list;
        private Label _selectionLabel;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _list = root.Q<ScrollView>("choice-list");
            _selectionLabel = root.Q<Label>("selection-label");

            var yearLine = root.Q<Label>("year-line");
            var years = SessionRuntime.AnchorYears();
            if (years.Count > 0) yearLine.text = years[0].Label + "·" + years[0].Blurb;

            root.Q<Button>("tab-named").clicked += () => { _cityMode = false; Refresh(root); };
            root.Q<Button>("tab-city").clicked += () => { _cityMode = true; Refresh(root); };
            root.Q<Button>("start-btn").clicked += StartGame;

            Refresh(root);
        }

        private void Refresh(VisualElement root)
        {
            _list.Clear();
            _selectedId = null;
            _selectionLabel.text = "未选择";
            var hint = root.Q<Label>("mode-hint");

            if (_cityMode)
            {
                hint.text = "任取一座非君主治所的城为太守，该年该城将佐尽听调遣。";
                foreach (GovernorCityChoiceLine c in SessionRuntime.GovernorCities().Choices)
                {
                    string label = $"{c.CityName}（{c.SuzerainName}·部将 {c.GeneralCount} 员）";
                    _list.Add(MakeRow(label, c.CityId, isCity: true));
                }
            }
            else
            {
                hint.text = "择一位诸侯麾下起家，走太守→晋升→自立的生涯。";
                foreach (ScenarioChoiceLine s in SessionRuntime.NamedStarts().Choices)
                {
                    string label = $"{s.Name} —— {s.Blurb}";
                    _list.Add(MakeRow(label, s.Id, isCity: false));
                }
            }
        }

        private Button MakeRow(string label, string id, bool isCity)
        {
            var b = new Button { text = label };
            b.AddToClassList("choice-row");
            b.clicked += () =>
            {
                _selectedId = id;
                _selectedIsCity = isCity;
                _selectionLabel.text = "已选：" + label;
            };
            return b;
        }

        private void StartGame()
        {
            if (string.IsNullOrEmpty(_selectedId)) return;
            if (_selectedIsCity) SessionRuntime.StartGovernorGame(_selectedId);
            else SessionRuntime.StartNamedGame(_selectedId);
            SceneManager.LoadScene(_hudScene);
        }
    }
}
