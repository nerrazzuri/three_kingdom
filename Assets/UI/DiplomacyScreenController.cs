using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 外交屏控制器（GDD_023，薄壳）：列各存续势力立场（DiplomacyView，dotnet 已测）+ 缔互不侵犯/背约按钮
    /// （经 SessionRuntime 便捷桥）。逻辑在已测 CampaignRuntime。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DiplomacyScreenController : MonoBehaviour
    {
        [SerializeField] private string _backScene = "Hud";

        private void OnEnable() => Render();

        public void Render()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var list = root.Q<ScrollView>("diplo-list");
            list.Clear();

            foreach (DiplomacyLine d in SessionRuntime.DiplomacyView().Factions)
            {
                var row = new VisualElement();
                row.AddToClassList("diplo-row");
                row.Add(new Label($"{d.FactionName}　立场：{d.StanceLabel}"));

                string fid = d.FactionId;
                if (d.CanAttackFreely)
                {
                    var pact = new Button { text = "缔互不侵犯" };
                    pact.clicked += () => { SessionRuntime.ProposeNonAggression(fid); Render(); };
                    row.Add(pact);
                }
                else
                {
                    var breach = new Button { text = "背约（损名望）" };
                    breach.clicked += () => { SessionRuntime.Breach(fid); Render(); };
                    row.Add(breach);
                }
                list.Add(row);
            }

            var back = root.Q<Button>("diplo-back");
            if (back != null) back.clicked += () => SceneManager.LoadScene(_backScene);
        }
    }
}
