using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 区域战斗屏控制器（Presentation 薄壳 / ADR-0002，GDD_021 §12 / epic-031 S7）。
    /// 把只读 <see cref="ZoneBattleView"/> 绑定到 UXML：各区态势 + 排兵布阵调动按钮 + 推进回合 + 涌现 + 终局。
    /// 逻辑（部署/调整/敌AI/结算/终局）已由 dotnet 测试覆盖（BLOCKING）；本壳无规则。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ZoneBattleController : MonoBehaviour
    {
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Render(root);

            var resolve = root.Q<Button>("zb-resolve");
            if (resolve != null) resolve.clicked += () => { ZoneBattleSession.ResolveRound(); Render(root); };

            var toMenu = root.Q<Button>("zb-to-menu");
            if (toMenu != null) toMenu.clicked += () => SceneManager.LoadScene("MainMenu");
        }

        /// <summary>渲染当前战斗投影（纯读；同态渲染恒等）。</summary>
        private void Render(VisualElement root)
        {
            ZoneBattleView view = ZoneBattleSession.View();

            SetLabel(root, "zb-round", $"第 {view.Round} / {view.MaxRounds} 回合");
            SetLabel(root, "zb-outcome", view.IsOver ? view.OutcomeLabel : string.Empty);

            var resolve = root.Q<Button>("zb-resolve");
            if (resolve != null) resolve.SetEnabled(!view.IsOver);

            // 各区态势：我方/敌方投影 + 已成兵法条件。
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

            // 排兵布阵：合法调动按钮（点击 → 调动 → 重渲染）。
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

            // 涌现兵法。
            var emerg = root.Q<VisualElement>("zb-emergences");
            if (emerg != null)
            {
                emerg.Clear();
                foreach (string e in view.Emergences) emerg.Add(new Label("　⚡ " + e));
            }
        }

        private static void SetLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }
    }
}
