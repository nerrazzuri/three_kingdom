using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// HUD 视觉壳控制器（Presentation 薄壳 / ADR-0002）。
    /// 把 <see cref="HudContextView"/> 的「情境→可见元素集」绑定到 UXML：每个情境只显示规定元素，
    /// 全屏模态隐去全部 HUD。逻辑（元素集/模态/通知/因果链）已由 dotnet 测试覆盖（BLOCKING）。
    /// <para>
    /// M15 story-001：HUD 已切换到 <b>CampaignSession 战役脊梁</b>（经 <see cref="SessionRuntime"/>，ADR-0009）。
    /// story-003：军议/敌情/侦察已重定向到战役会话只读投影（定性置信 + 时效 + 反全知）。
    /// 其余面板（账本/花名册/袭扰/伏击/求援/目标）随 story-004 逐屏接入，暂以「接入中」占位、按钮禁用。
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
                advance.clicked += () => { RenderTime(root, SessionRuntime.Advance()); RenderLoop(root); }; // 推进 + 跨日提示 + 刷新循环

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

            // 军议/敌情屏（story-003 / TR-ux-002/003）：从战役会话只读投影渲染；反全知只经玩家知识投影。
            RenderEnemyIntel(root);
            RenderCouncil(root);

            var convene = root.Q<Button>("convene");
            if (convene != null)
                convene.clicked += () => { _council = SessionRuntime.ConveneCouncil(); RenderCouncil(root); };

            var scout = root.Q<Button>("scout");
            if (scout != null)
                scout.clicked += () =>
                {
                    bool ok = SessionRuntime.Scout();
                    SetLabel(root, "scout-status", ok ? "已侦察：敌情已更新" : "侦察失败");
                    _council = SessionRuntime.CurrentCouncil();   // 知识变化 → 旧军议随之标过时（不静默重算）
                    RenderEnemyIntel(root);
                    RenderCouncil(root);
                };

            // 战果复盘（story-002 / TR-ux-001/004）：无战果时占位；战果由「结算战果」真实产生（story-004）。
            RenderBattleReview(root);
            var toggle = root.Q<Button>("review-toggle");
            if (toggle != null)
                toggle.clicked += () =>
                {
                    if (_review == null) return;
                    _review = _review.IsExpanded ? _review.Collapse() : _review.Expand();
                    RenderBattleReview(root);
                };

            // 战役主循环（story-004 / TR-ux-001/005）：治理/备战/战斗命令经 CampaignSessionService，HUD 只读投影。
            RenderLoop(root);
            Wire(root, "requisition", () => { ShowCmd(root, SessionRuntime.Requisition(RequisitionAmount)); RenderLoop(root); });
            Wire(root, "repair-fort", () => { ShowCmd(root, SessionRuntime.Repair()); RenderLoop(root); });
            Wire(root, "appease", () => { ShowCmd(root, SessionRuntime.Appease()); RenderLoop(root); });
            Wire(root, "add-ambush", () => { SessionRuntime.AddAmbushOrder(); RenderLoop(root); });
            Wire(root, "submit-plan", () => { SessionRuntime.SubmitPlan(); RenderLoop(root); });
            Wire(root, "start-battle", () => { ShowCmd(root, SessionRuntime.StartBattle()); RenderLoop(root); });
            Wire(root, "resolve-outcome", () =>
            {
                if (SessionRuntime.Phase().Phase != CampaignPhase.Battle) return;
                _review = SessionRuntime.ResolveOutcome();
                RenderBattleReview(root);
                RenderLoop(root);
            });

            // 新手引导（story-005 / §3/§5/§7 Q2）：前 N 回合自动展开军议（配置驱动，可关闭）。
            if (OnboardingHints.ShouldAutoExpandCouncil(SessionRuntime.Round(), OnboardingConfig.Default, OnboardingRuntime.Disabled)
                && SessionRuntime.HasIntel())
            {
                _council = SessionRuntime.ConveneCouncil();
                RenderCouncil(root);
            }
            Wire(root, "onboarding-close", () => { OnboardingRuntime.Disabled = true; RenderOnboarding(root); });
            RenderOnboarding(root);
        }

        /// <summary>征用军粮的固定投放量（story-004；UXML 无输入框，用稳妥默认量，超量走稳定错误码显示）。</summary>
        private const long RequisitionAmount = 20;

        private static void Wire(VisualElement root, string name, System.Action handler)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += handler;
        }

        /// <summary>命令结果反馈：失败按稳定错误码显示原因（AC-5：不做 UI 侧预判吞掉），成功清空。</summary>
        private static void ShowCmd(VisualElement root, ThreeKingdom.Application.Session.CampaignCommandResult result)
            => SetLabel(root, "govern-status", result.Applied ? string.Empty : CampaignErrorText.For(result.Error));

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

        /// <summary>当前军议展示模型（表现态，不入 Domain/存档；null=尚未召开）。</summary>
        private CampaignCouncilView _council;

        /// <summary>
        /// 渲染敌情面板（GDD_007 / TR-ux-003）：只呈现估计值 + 来源 + 「N 段前」时效，<b>无</b>真值。
        /// 过时以「【过时】」文本前缀标注（非纯色冗余编码，色盲可辨，AC-7）。
        /// </summary>
        private void RenderEnemyIntel(VisualElement root)
        {
            var panel = SessionRuntime.EnemyIntel();
            SetLabel(root, "enemy-report-empty",
                panel.IsEmpty ? "尚无敌情——派出侦察以获取（返报需时）" : string.Empty);

            var lines = root.Q<VisualElement>("enemy-report-lines");
            if (lines != null)
            {
                lines.Clear();
                foreach (var e in panel.Entries)
                {
                    string stalePrefix = e.IsStale ? "【过时】" : string.Empty;
                    lines.Add(new Label(
                        $"{stalePrefix}{e.SubjectLabel}：约 {e.EstimatedStrength}（{e.SourceLabel}·{e.ObservedAgoLabel}）"));
                }
            }

            SetEnabled(root, "scout", SessionRuntime.HasIntel());
        }

        /// <summary>
        /// 渲染军议面板（GDD_008 / TR-ux-002）：并列条件化建议 + 定性置信档（低/中/高，<b>无</b>成功率/唯一推荐）。
        /// 侦察后旧军议标过时（<see cref="CampaignCouncilView.StaleNotice"/>），但不禁用旧建议（决策自由 P11）。
        /// </summary>
        private void RenderCouncil(VisualElement root)
        {
            SetEnabled(root, "convene", SessionRuntime.HasIntel());
            SetLabel(root, "council-stale", _council != null ? _council.StaleNotice : string.Empty);

            var list = root.Q<VisualElement>("council-advice");
            if (list == null) return;
            list.Clear();

            if (_council == null)
            {
                list.Add(new Label("　尚未召开军议——点「召开军议」听取条件化建议（并列，无最优解）。"));
                return;
            }

            foreach (var a in _council.Advice)
            {
                list.Add(new Label($"▸ {a.CandidateLabel}（依据置信：{a.ConfidenceLabel}）"));
                list.Add(new Label($"　观察：{a.Observation}"));
                list.Add(new Label($"　假设：{a.Assumption}"));
                foreach (var c in a.RequiredConditions) list.Add(new Label($"　所需：{c}"));
                foreach (var r in a.Risks) list.Add(new Label($"　风险：{r}"));
                foreach (var m in a.MissingIntel) list.Add(new Label($"　待查：{m}"));
            }
        }

        /// <summary>
        /// 战役主循环渲染（story-004 / TR-ux-001/005）：相位横幅 + 治理 + 备战 + 战斗条件。
        /// 每次命令后调用，保证任一相位玩家都看得到当前状态与下一步可做动作。纯读会话只读投影。
        /// </summary>
        private void RenderLoop(VisualElement root)
        {
            RenderPhase(root);
            RenderGovernance(root);
            RenderPrep(root);
            RenderBattleConditions(root);
            RenderOnboarding(root);
        }

        /// <summary>
        /// 新手引导渲染（story-005 / §3 渐进暴露序 + §7 Q2 果·长线）：按当前相位取一条未见 in-world 提示，
        /// 不模态、不替选、可关闭；关闭后不再打扰（表现层偏好，不进权威存档，见 OnboardingViewModelTests）。
        /// </summary>
        private void RenderOnboarding(VisualElement root)
        {
            var hint = root.Q<Label>("onboarding-hint");
            bool disabled = OnboardingRuntime.Disabled;
            SetEnabled(root, "onboarding-close", !disabled);
            if (disabled)
            {
                if (hint != null) hint.text = string.Empty;
                return;
            }

            var candidates = new List<OnboardingCue>();
            switch (SessionRuntime.Phase().Phase)
            {
                case CampaignPhase.Governance:
                    candidates.Add(OnboardingCue.Observe);
                    candidates.Add(OnboardingCue.Plan);
                    candidates.Add(OnboardingCue.AdvanceToHistory);
                    break;
                case CampaignPhase.Preparing:
                    candidates.Add(OnboardingCue.Prepare);
                    candidates.Add(OnboardingCue.Fight);
                    break;
                case CampaignPhase.Battle:
                    candidates.Add(OnboardingCue.Fight);
                    break;
                case CampaignPhase.Aftermath:
                    candidates.Add(OnboardingCue.OutcomeToCareer);
                    break;
            }

            var cues = OnboardingHints.CuesFor(candidates, OnboardingRuntime.Seen(), disabled);
            if (hint == null) return;
            if (cues.Count == 0) { hint.text = string.Empty; return; }

            hint.text = "【引导】" + cues[0];
            // 标记已见（cues[0] 对应候选中第一个未见者），避免下次重复打扰。
            foreach (var c in candidates)
                if (!OnboardingRuntime.HasSeen(c)) { OnboardingRuntime.MarkSeen(c); break; }
        }

        /// <summary>相位横幅：当前相位 + 该相位可做动作集（AC-5）。</summary>
        private static void RenderPhase(VisualElement root)
        {
            HudPhaseView phase = SessionRuntime.Phase();
            SetLabel(root, "hud-phase",
                $"相位：{PhaseLabel(phase.Phase)}｜可做：{string.Join("、", phase.AvailableActions)}");
        }

        private static string PhaseLabel(CampaignPhase phase) => phase switch
        {
            CampaignPhase.Governance => "治理",
            CampaignPhase.Preparing => "备战",
            CampaignPhase.Battle => "战中",
            CampaignPhase.Aftermath => "战后",
            _ => phase.ToString(),
        };

        /// <summary>治理面板：多维账本（分列不合并，P6）+ 三动作因果方向说明（TR-ux-001）。</summary>
        private static void RenderGovernance(VisualElement root)
        {
            GovernanceActionView gov = SessionRuntime.Governance();
            SetLabel(root, "ledger-stock", gov.Ledger.StockLabel);
            SetLabel(root, "ledger-morale", gov.Ledger.MoraleLabel);
            SetLabel(root, "ledger-security", gov.Ledger.SecurityLabel);
            SetLabel(root, "ledger-fort", gov.Ledger.FortificationLabel);
            SetLabel(root, "ledger-warning", gov.Ledger.WarningLabel);

            foreach (var a in gov.Actions)
            {
                string hintElement = a.ActionId switch
                {
                    "requisition" => "requisition-hint",
                    "repair-fort" => "repair-hint",
                    "appease" => "appease-hint",
                    _ => null,
                };
                if (hintElement != null) SetLabel(root, hintElement, a.CausalHint);
            }
        }

        /// <summary>备战面板：草稿（可移除按钮）vs 已提交（不可移除·朱批承诺态）。</summary>
        private void RenderPrep(VisualElement root)
        {
            PrepPanelView prep = SessionRuntime.Prep();
            SetLabel(root, "prep-status", prep.StatusLabel);

            var draft = root.Q<VisualElement>("prep-draft");
            if (draft != null)
            {
                draft.Clear();
                foreach (var o in prep.DraftOrders)
                {
                    var captured = o;
                    draft.Add(new Button(() => { SessionRuntime.RemoveOrder(captured.OrderId); RenderLoop(root); })
                    {
                        text = "移除：" + captured.Label,
                    });
                }
            }

            var committed = root.Q<VisualElement>("prep-committed");
            if (committed != null)
            {
                committed.Clear();
                foreach (var o in prep.CommittedOrders)
                    committed.Add(new Label("　朱批已承诺：" + o.Label));
            }

            SetEnabled(root, "add-ambush", !prep.IsCommitted);
            SetEnabled(root, "submit-plan", !prep.IsCommitted && prep.DraftOrders.Count > 0);
            SetEnabled(root, "start-battle", SessionRuntime.Phase().Phase == CampaignPhase.Preparing && prep.IsCommitted);
        }

        /// <summary>战斗条件进度：战中/战后显示各兵法链已满足✓/未满足✗ + 还差 N 条（非按钮，纯状态指示）。</summary>
        private static void RenderBattleConditions(VisualElement root)
        {
            CampaignPhase phase = SessionRuntime.Phase().Phase;
            bool inBattle = phase == CampaignPhase.Battle;
            bool aftermath = phase == CampaignPhase.Aftermath;

            SetLabel(root, "battle-status",
                inBattle ? "战斗进行中——满足兵法条件后「结算战果」。"
                : aftermath ? "已结算——见下方复盘。"
                : "尚未开战（备战提交后可开战）。");

            var area = root.Q<VisualElement>("battle-conditions");
            if (area != null)
            {
                area.Clear();
                if (inBattle || aftermath)
                {
                    BattleConditionProgressView progress = SessionRuntime.BattleConditionProgress();
                    foreach (var line in progress.Lines)
                    {
                        area.Add(new Label($"{line.TacticName}：{line.ProgressLabel}"));
                        foreach (var met in line.Satisfied) area.Add(new Label("　✓ " + met));
                        foreach (var unmet in line.Unsatisfied) area.Add(new Label("　✗ " + unmet));
                    }
                }
            }

            SetEnabled(root, "resolve-outcome", inBattle);
        }

        /// <summary>
        /// 未接线面板统一占位（后续故事接入后逐一移除）：状态标签显示「接入中」、对应命令按钮禁用。
        /// 军议/敌情/侦察（story-003）、账本/治理/备战/战斗（story-004）已接入战役投影，不在此占位；
        /// 残留占位：外交求援 + 袭扰/伏击战术动作（另属准备/战斗子系统）+ 目标横幅。
        /// </summary>
        private static void RenderPendingPanels(VisualElement root)
        {
            SetLabel(root, "diplo-status", PendingLabel);
            SetLabel(root, "raid-status", PendingLabel);
            SetLabel(root, "ambush-status", PendingLabel);
            SetLabel(root, "hud-objective", PendingLabel);
            SetLabel(root, "hud-banner", string.Empty);

            SetEnabled(root, "request-aid", false);
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
