using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Projections;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// HUD 视觉壳控制器（Presentation 薄壳 / ADR-0002）。
    /// 把 <see cref="HudContextView"/> 的「情境→可见元素集」绑定到 UXML：每个情境只显示规定元素，
    /// 全屏模态隐去全部 HUD。逻辑（元素集/模态/通知/因果链）已由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private HudContext _context = HudContext.JudgmentLayout;
        [SerializeField] private bool _modalActive;

        // HudElement → UXML 元素名。
        private static readonly Dictionary<HudElement, string> ElementNames = new Dictionary<HudElement, string>
        {
            [HudElement.TimeBar] = "time-bar",
            [HudElement.OwnLedger] = "own-ledger",
            [HudElement.EnemyReport] = "enemy-report",
            [HudElement.AdvisorEntry] = "advisor-entry",
            [HudElement.CommandTray] = "command-tray",
            [HudElement.OutcomeChain] = "outcome-chain",
        };

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Apply(root);
            // 无障碍横切挂接（story-005）：文本缩放/色盲/减少动态 + HUD 元素可见性。
            // 复合于情境可见性之上——只额外隐藏用户关闭的元素，不强制显示。
            AccessibilityApplier.Apply(root, AccessibilityRuntime.Current);

            // 竖切：真实 Application 会话驱动 HUD。开局渲染三面板（时间/己方账本/敌情），
            // 推进时段经 SessionService 推进世界时钟 + 跨日结算城市；侦察刷新敌情（含时效）。
            RenderTime(root, SessionRuntime.Status());
            RenderLedger(root, SessionRuntime.Ledger());
            RenderEnemy(root, SessionRuntime.Enemy());

            var advance = root.Q<Button>("advance-time");
            if (advance != null)
                advance.clicked += () =>
                {
                    RenderTime(root, SessionRuntime.Advance()); // 推进 + 跨日提示
                    RenderLedger(root, SessionRuntime.Ledger()); // 跨日结算后账本更新
                    RenderEnemy(root, SessionRuntime.Enemy());   // 情报随时间过时
                };

            var scout = root.Q<Button>("scout");
            if (scout != null) scout.clicked += () => RenderEnemy(root, SessionRuntime.Scout());

            // 竖切：存档（原子写，真实持久栈）+ 返回主菜单。
            var save = root.Q<Button>("save-game");
            if (save != null)
                save.clicked += () =>
                {
                    bool ok = SessionRuntime.Save();
                    SetLabel(root, "save-status", ok ? "已存档" : "存档失败");
                };

            var toMenu = root.Q<Button>("to-menu");
            if (toMenu != null) toMenu.clicked += () => SceneManager.LoadScene("MainMenu");
        }

        /// <summary>把真实世界状态投影渲染到时间条（合成时辰标签 + 跨日提示）。</summary>
        private void RenderTime(VisualElement root, WorldStatusView status)
        {
            var label = root.Q<Label>("time-bar-label");
            if (label != null) label.text = status.TimeLabel;

            var note = root.Q<Label>("advance-note");
            if (note != null) note.text = status.CrossDayNotice;
        }

        /// <summary>把己方城市账本渲染到 own-ledger 卡（多维分列，P6 不合并；短缺/骚乱警示）。</summary>
        private void RenderLedger(VisualElement root, CityLedgerView ledger)
        {
            SetLabel(root, "ledger-stock", ledger.StockLabel);
            SetLabel(root, "ledger-morale", ledger.MoraleLabel);
            SetLabel(root, "ledger-security", ledger.SecurityLabel);
            SetLabel(root, "ledger-fort", ledger.FortificationLabel);
            SetLabel(root, "ledger-warning", ledger.WarningLabel);
        }

        /// <summary>把敌情探报渲染到 enemy-report 卡（只呈现估计值/时效，无真值；无情报给提示）。</summary>
        private void RenderEnemy(VisualElement root, EnemyReportView report)
        {
            var empty = root.Q<Label>("enemy-report-empty");
            if (empty != null)
            {
                empty.text = report.EmptyLabel;
                empty.style.display = report.HasIntel ? DisplayStyle.None : DisplayStyle.Flex;
            }

            var lines = root.Q<VisualElement>("enemy-report-lines");
            if (lines != null)
            {
                lines.Clear();
                foreach (var line in report.Lines)
                    lines.Add(new Label(line));
            }
        }

        private static void SetLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }

        /// <summary>按当前情境绑定元素可见性（slice 演示入口；运行期由状态驱动）。</summary>
        public void Apply(VisualElement root)
        {
            var view = HudContextView.ForContext(_context, _modalActive);
            foreach (var pair in ElementNames)
            {
                var element = root.Q<VisualElement>(pair.Value);
                if (element == null) continue;
                element.style.display = view.Shows(pair.Key) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
