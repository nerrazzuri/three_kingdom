using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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

            // 竖切：真实 Application 会话驱动时间条。开局状态 + 推进按钮经 SessionService 推进世界时钟。
            RenderTime(root, SessionRuntime.Status());
            var advance = root.Q<Button>("advance-time");
            if (advance != null) advance.clicked += () => RenderTime(root, SessionRuntime.Advance());
        }

        /// <summary>把真实世界状态投影渲染到时间条（合成时辰标签 + 跨日提示）。</summary>
        private void RenderTime(VisualElement root, WorldStatusView status)
        {
            var label = root.Q<Label>("time-bar-label");
            if (label != null) label.text = status.TimeLabel;

            var note = root.Q<Label>("advance-note");
            if (note != null) note.text = status.CrossDayNotice;
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
