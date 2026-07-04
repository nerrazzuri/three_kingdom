using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 区域战斗屏控制器（Presentation 薄壳 / ADR-0002，GDD_021 §12 / epic-031 S7）——<b>独立战斗场景</b>。
    /// 按 <see cref="ZoneBattleSession"/> 模式驱动 campaign 真战斗（出征/守城）或演示局：各区态势 + 排兵布阵调动 +
    /// <b>亲自打</b>（逐回合）/ <b>挂 AI 代打</b>（不作弊、不保证赢）+ 结算返回来源场景。逻辑已 dotnet 单测；本壳只渲染。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ZoneBattleController : MonoBehaviour
    {
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Render(root);

            Wire(root, "zb-resolve", () => { if (!ZoneBattleSession.IsOver) ZoneBattleSession.ResolveRound(); Render(root); });
            Wire(root, "zb-auto", () => { if (!ZoneBattleSession.IsOver) ZoneBattleSession.AutoResolve(); Render(root); });
            Wire(root, "zb-conclude", () =>
            {
                if (!ZoneBattleSession.IsOver) return;
                ZoneBattleSession.Conclude();                        // 出征→占城/退兵（权威）；守城→守土成败
                SceneManager.LoadScene(ZoneBattleSession.ReturnScene); // 返回来源场景（HUD）
            });
            Wire(root, "zb-to-menu", () => SceneManager.LoadScene("MainMenu"));
        }

        /// <summary>渲染当前战斗投影（纯读；同态渲染恒等）。</summary>
        private void Render(VisualElement root)
        {
            ZoneBattleView view = ZoneBattleSession.View();

            SetLabel(root, "zb-round", $"第 {view.Round} / {view.MaxRounds} 回合");
            SetLabel(root, "zb-outcome", view.IsOver ? view.OutcomeLabel : string.Empty);
            SetLabel(root, "zb-result", view.IsOver ? "点「结算战果并返回」收兵。" : string.Empty);

            SetEnabled(root, "zb-resolve", !view.IsOver);
            SetEnabled(root, "zb-auto", !view.IsOver);
            SetEnabled(root, "zb-conclude", view.IsOver);

            var zones = root.Q<VisualElement>("zb-zones");
            if (zones != null)
            {
                zones.Clear();
                foreach (ZoneLineView z in view.Zones)
                {
                    string star = z.IsObjective ? "★" : "　";
                    zones.Add(new Label($"{star}{z.ZoneLabel}　我 {z.OwnStrength} ｜ 敌 {z.EnemyStrength}"));
                    foreach (string c in z.FormedConditions) zones.Add(new Label($"　　✦ {c}"));
                    foreach (string d in z.OwnDetachments) zones.Add(new Label($"　　· {d}"));
                }
            }

            var moves = root.Q<VisualElement>("zb-moves");
            if (moves != null)
            {
                moves.Clear();
                if (view.IsOver)
                    moves.Add(new Label("　战斗已终局。"));
                else
                    foreach (ZoneMoveOption m in view.MoveOptions)
                    {
                        var captured = m;
                        moves.Add(new Button(() => { ZoneBattleSession.Move(captured.DetachmentId, captured.TargetZoneId); Render(root); })
                        {
                            text = $"{captured.DetachmentLabel} → {captured.TargetZoneLabel}",
                        });
                    }
            }

            var emerg = root.Q<VisualElement>("zb-emergences");
            if (emerg != null)
            {
                emerg.Clear();
                foreach (string e in view.Emergences) emerg.Add(new Label("　⚡ " + e));
            }
        }

        private static void Wire(VisualElement root, string name, System.Action handler)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += handler;
        }

        private static void SetLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }

        private static void SetEnabled(VisualElement root, string name, bool enabled)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.SetEnabled(enabled);
        }
    }
}
