using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Domain.Theater;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 多城战区屏控制器（GDD_022，薄壳）：列玩家直辖/委任的城（TheaterState）。委任经 console/delegate 或后续交互屏。
    /// 逻辑在已测 CampaignRuntime。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TheaterScreenController : MonoBehaviour
    {
        [SerializeField] private string _backScene = "Hud";

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var list = root.Q<ScrollView>("theater-list");
            list.Clear();

            TheaterState st = SessionRuntime.TheaterState;
            if (st.Holdings.Count == 0)
                list.Add(new Label("尚无直辖城——出征占城归你直辖后，此处列出。"));
            foreach (CityHolding h in st.Holdings)
            {
                string mode = h.Mode == GovernanceMode.Delegated ? "委任" : "亲管";
                string gov = h.Governor.HasValue ? "·" + DisplayNames.Of(h.Governor.Value.Value) : "";
                list.Add(new Label($"{DisplayNames.Of(h.City.Value)} — {mode}{gov}"));
            }

            var back = root.Q<Button>("theater-back");
            if (back != null) back.clicked += () => SceneManager.LoadScene(_backScene);
        }
    }
}
