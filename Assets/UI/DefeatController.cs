using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Defeat;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 被灭·被俘流程屏（GDD_026 R9，Presentation 薄壳）：判生死→归顺?→释放?→投奔他主收留?；唯身死才终。
    /// 归顺/被收留 → 活世界复位为太守（保当前年/一生）→ 回 HUD。自立叛主者必被处死（无活路）。
    /// 逻辑（DefeatFlow/CaptivityService/ReseatGovernor）已由 dotnet 覆盖。★需 Unity 编辑器验证。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DefeatController : MonoBehaviour
    {
        [SerializeField] private string _hudScene = "Hud";
        [SerializeField] private string _mainMenuScene = "MainMenu";

        private VisualElement _actions;
        private Label _status;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _actions = root.Q<VisualElement>("defeat-actions");
            _status = root.Q<Label>("defeat-status");
            Render();
        }

        private void Render()
        {
            _actions.Clear();
            DefeatFlow flow = SessionRuntime.BeginDefeat();

            switch (flow.Stage)
            {
                case DefeatStage.Captured:
                    if (flow.WasRebellion)
                    {
                        _status.text = "叛主自立而败——阶下之囚，不赦。";
                        AddButton("听候发落", () => { flow.ResolveCaptorFate(); Render(); });
                    }
                    else
                    {
                        _status.text = "你为阶下之囚，可归顺新主，或宁折不屈。";
                        AddButton("听候发落（生死由人）", () => { flow.ResolveCaptorFate(); Render(); });
                        AddButton("归顺", () => { flow.Submit(); Reseat(); });
                        AddButton("宁折不屈", () => { flow.Refuse(); Render(); });
                    }
                    break;

                case DefeatStage.Executed:
                    _status.text = "身死。一世至此而终——待子嗣承其志。";
                    AddButton("传承子嗣（重开一世）", () => SceneManager.LoadScene(_mainMenuScene));
                    break;

                case DefeatStage.Released:
                    _status.text = "获释流亡。投奔一家诸侯，或可东山再起——然非人人肯收。";
                    foreach (PowerStanding p in SessionRuntime.Contention.Powers)
                    {
                        if (!p.Alive) continue;
                        FactionId lord = p.Faction;
                        AddButton("投奔 " + DisplayNames.Of(lord.Value), () =>
                        {
                            if (flow.SeekRefuge(lord, p.Cities)) Reseat();
                            else { _status.text = $"{DisplayNames.Of(lord.Value)} 不肯收留——另择他家。"; }
                        });
                    }
                    break;

                case DefeatStage.Imprisoned:
                    _status.text = "不肯归顺，被囚禁。囚牢之中，静待时变。";
                    AddButton("返回主菜单", () => SceneManager.LoadScene(_mainMenuScene));
                    break;

                default:
                    Reseat();
                    break;
            }
        }

        private void Reseat()
        {
            if (SessionRuntime.ContinueUnderNewLord())
                SceneManager.LoadScene(_hudScene);   // 复位为新主太守，活世界续局
            else
                _status.text = "新主已无城可授，仍为流亡之身。";
        }

        private void AddButton(string label, System.Action onClick)
        {
            var b = new Button { text = label };
            b.AddToClassList("defeat-btn");
            b.clicked += onClick;
            _actions.Add(b);
        }
    }
}
