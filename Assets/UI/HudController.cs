using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// HUD 视觉壳控制器（Presentation 薄壳 / ADR-0002）。
    /// 把 <see cref="HudContextView"/> 的「情境→可见元素集」绑定到 UXML：每个情境只显示规定元素，
    /// 全屏模态隐去全部 HUD。逻辑（元素集/模态/通知/因果链）已由 dotnet 测试覆盖（BLOCKING）。
    /// <para>
    /// M15 story-001：HUD 已切换到 <b>CampaignSession 战役脊梁</b>（经 <see cref="SessionRuntime"/>，ADR-0009）。
    /// 本 story 只贯通最小生命周期（时间/推进/存档）；其余面板（账本/敌情/军议/花名册/侦察/袭扰/伏击/目标）
    /// 随 story-003/004 逐屏重定向到战役投影，暂以「接入中」占位、按钮禁用。
    /// </para>
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private HudContext _context = HudContext.JudgmentLayout;
        [SerializeField] private bool _modalActive;

        /// <summary>未接线面板的占位文案（story-003/004 接入后移除）。</summary>
        private const string PendingLabel = "接入战役会话中……";

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

            // 战役会话驱动 HUD：开局渲染时间条；推进时段经 CampaignSessionService 走全局日界结算顺序
            //（时间→……→城市→历史→生涯，TR-session-001）。
            RenderTime(root, SessionRuntime.Status());
            RenderPendingPanels(root);

            var advance = root.Q<Button>("advance-time");
            if (advance != null)
                advance.clicked += () => RenderTime(root, SessionRuntime.Advance()); // 推进 + 跨日提示

            // 存档（原子写，统一信封）+ 返回主菜单。
            var save = root.Q<Button>("save-game");
            if (save != null)
                save.clicked += () =>
                {
                    bool ok = SessionRuntime.Save();
                    SetLabel(root, "save-status", ok ? "已存档" : "存档失败");
                };

            var toMenu = root.Q<Button>("to-menu");
            if (toMenu != null) toMenu.clicked += () => SceneManager.LoadScene("MainMenu");

            // 战果复盘（story-002 / TR-ux-001/004）：默认无战果时隐藏内容区，仅演示按钮可用（临时）。
            RenderBattleReview(root);
            var demoWin = root.Q<Button>("demo-victory");
            if (demoWin != null)
                demoWin.clicked += () => { _review = SessionRuntime.RunDemoBattle(OutcomeBranch.Victory); RenderBattleReview(root); };
            var demoLose = root.Q<Button>("demo-defeat");
            if (demoLose != null)
                demoLose.clicked += () => { _review = SessionRuntime.RunDemoBattle(OutcomeBranch.Defeat); RenderBattleReview(root); };
            var toggle = root.Q<Button>("review-toggle");
            if (toggle != null)
                toggle.clicked += () =>
                {
                    if (_review == null) return;
                    _review = _review.IsExpanded ? _review.Collapse() : _review.Expand();
                    RenderBattleReview(root);
                };
        }

        /// <summary>当前复盘展示模型（表现态，不入 Domain/存档；null=尚无战果）。</summary>
        private BattleReviewView _review;

        /// <summary>
        /// 渲染战果复盘（不可变 ViewModel → UXML；同模型渲染恒等）。折叠=只见一句话结论；
        /// 展开=≤5 主因素 + 兵法复盘。续局选项为按钮：点击记录选择（真实续局命令分派随 story-004
        /// 可做动作集接入——会话已可继续，推进时段等合法命令持续可用）。
        /// </summary>
        private void RenderBattleReview(VisualElement root)
        {
            bool has = _review != null;
            SetLabel(root, "review-conclusion", has ? _review.ConclusionLabel : "尚无战果——打完一局后此处复盘因果。");
            SetLabel(root, "review-notice", has ? _review.ContinuationNotice : string.Empty);
            SetLabel(root, "review-career-hint", has ? _review.CareerHintLabel : string.Empty);

            var toggle = root.Q<Button>("review-toggle");
            if (toggle != null)
            {
                toggle.SetEnabled(has);
                toggle.text = has ? _review.ToggleLabel : "展开因果";
            }

            var factors = root.Q<VisualElement>("review-factors");
            if (factors != null)
            {
                factors.Clear();
                if (has)
                    foreach (var line in _review.VisibleFactorLines)
                        factors.Add(new Label("　" + line));
            }

            var tactics = root.Q<VisualElement>("review-tactics");
            if (tactics != null)
            {
                tactics.Clear();
                if (has && _review.IsExpanded)
                    foreach (var line in _review.TacticLines)
                        tactics.Add(new Label("　" + line));
            }

            var continuations = root.Q<VisualElement>("review-continuations");
            if (continuations != null)
            {
                continuations.Clear();
                if (has)
                {
                    foreach (var option in _review.Options)
                    {
                        var captured = option;
                        var button = new Button(() => SetLabel(root, "review-selection",
                            $"已选定续局：{captured.KindLabel}——{captured.Reason}（战役继续，可推进时段）"))
                        {
                            text = captured.KindLabel,
                        };
                        continuations.Add(button);
                    }
                }
            }

            if (!has) SetLabel(root, "review-selection", string.Empty);
        }

        /// <summary>把真实世界状态投影渲染到时间条（合成时辰标签 + 跨日提示）。</summary>
        private void RenderTime(VisualElement root, WorldStatusView status)
        {
            var label = root.Q<Label>("time-bar-label");
            if (label != null) label.text = status.TimeLabel;

            var note = root.Q<Label>("advance-note");
            if (note != null) note.text = status.CrossDayNotice;
        }

        /// <summary>
        /// 未接线面板统一占位（story-003 军议/敌情、story-004 账本/备战等接入后逐一移除）：
        /// 状态标签显示「接入中」、对应命令按钮禁用——不显示旧竖切数据，避免与战役会话状态混淆。
        /// </summary>
        private static void RenderPendingPanels(VisualElement root)
        {
            SetLabel(root, "ledger-stock", PendingLabel);
            SetLabel(root, "ledger-morale", string.Empty);
            SetLabel(root, "ledger-security", string.Empty);
            SetLabel(root, "ledger-fort", string.Empty);
            SetLabel(root, "ledger-warning", string.Empty);

            SetLabel(root, "enemy-report-empty", PendingLabel);
            SetLabel(root, "diplo-status", PendingLabel);
            SetLabel(root, "council-stale", string.Empty);
            SetLabel(root, "scout-status", PendingLabel);
            SetLabel(root, "raid-status", PendingLabel);
            SetLabel(root, "ambush-status", PendingLabel);
            SetLabel(root, "hud-objective", PendingLabel);
            SetLabel(root, "hud-banner", string.Empty);

            SetEnabled(root, "convene", false);
            SetEnabled(root, "request-aid", false);
            SetEnabled(root, "scout", false);
            SetEnabled(root, "raid", false);
            SetEnabled(root, "ambush", false);
        }

        private static void SetEnabled(VisualElement root, string name, bool enabled)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.SetEnabled(enabled);
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
