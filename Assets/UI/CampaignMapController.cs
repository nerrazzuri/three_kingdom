using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 战略图屏控制器（GDD_026 / ADR-0015，薄壳）：投影 CampaignMapView——各势力领城 + 在场武将棋子。
    /// <b>反全知</b>：己方城之将/传世名将露真名，敌境无名之将显「未探明」（MapHeroCell.Known）。
    /// 全部逻辑在已测 CampaignRuntime/CampaignMapView（dotnet 覆盖）。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CampaignMapController : MonoBehaviour
    {
        [SerializeField] private string _backScene = "Hud";

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            CampaignMapView map = SessionRuntime.MapView();

            var era = root.Q<Label>("map-era");
            if (era != null) era.text = $"公元 {map.Year} · {map.Season} — 天下 {map.Cities.Count} 城 · {map.Factions.Count} 家逐鹿";

            // 势力（按领城数降序）：玩家标 ★。
            var factions = root.Q<ScrollView>("map-factions");
            if (factions != null)
            {
                factions.Clear();
                var ordered = new System.Collections.Generic.List<MapFactionCell>(map.Factions);
                ordered.Sort((a, b) => b.CityCount.CompareTo(a.CityCount));
                foreach (MapFactionCell f in ordered)
                    factions.Add(new Label($"{(f.IsPlayer ? "★ " : "　")}{f.FactionName} — {f.CityCount} 城"));
            }

            // 在场武将棋子（按城名聚合展示；反全知未探明者只显所在城 + 效力势力）。
            var heroes = root.Q<ScrollView>("map-heroes");
            if (heroes != null)
            {
                heroes.Clear();
                int hidden = 0;
                foreach (MapHeroCell h in map.Heroes)
                {
                    string cityName = DisplayNames.Of(h.CityId);
                    string faction = string.IsNullOrEmpty(h.FactionId) ? "" : "（" + DisplayNames.Of(h.FactionId) + "）";
                    if (h.Known)
                        heroes.Add(new Label($"{cityName} · {h.HeroName}{faction}"));
                    else
                    {
                        hidden++;
                        heroes.Add(new Label($"{cityName} · 未探明之将{faction}"));
                    }
                }
                if (map.Heroes.Count == 0)
                    heroes.Add(new Label("此纪元暂无具名布防。"));
                var note = root.Q<Label>("map-note");
                if (note != null && hidden > 0)
                    note.text = $"棋子行反全知——己方与名将露真名；当前有 {hidden} 员敌将「未探明」，占城或派探可发觉。";
            }

            var back = root.Q<Button>("map-back");
            if (back != null) back.clicked += () => SceneManager.LoadScene(_backScene);
        }
    }
}
