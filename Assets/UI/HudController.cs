using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Time;
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

            // 主角人设（GDD_015：开局随机性情，给天下事件"心里话"着色）。
            var persona = SessionRuntime.Persona();
            SetLabel(root, "hud-persona", "性情 · " + persona.Name + "（" + persona.Description + "）");

            // E3：战略状态（争霸/终局）+ 攻心入口——把此前够不到的差异化系统接给玩家。
            RenderStrategic(root);
            Wire(root, "subvert-enemy", () =>
            {
                var v = SessionRuntime.Subvert("city-hulao", ThreeKingdom.Domain.Subversion.SubversionScheme.UnderminedMorale, 100);
                SetLabel(root, "subvert-status", v.ResultLabel);
                RenderStrategic(root);
            });

            var advance = root.Q<Button>("advance-time");
            if (advance != null)
                advance.clicked += () =>
                {
                    RenderTime(root, SessionRuntime.AdvanceWeek());  // 世界地图一步=一周（GDD_026）
                    RenderEnemyIntel(root);                      // 在途侦察可能返报 → 敌情更新
                    _council = SessionRuntime.CurrentCouncil();  // 知识若变 → 旧军议标过时
                    RenderCouncil(root);
                    RenderLoop(root);                            // 相位/治理/备战/战斗刷新
                    // 势力覆灭 → 转被俘流程屏（GDD_026 R9：唯身死才终）。
                    if (SessionRuntime.IsEliminated()) SceneManager.LoadScene("Defeat");
                };

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

            // 屏间导航到独立场景屏（各屏 _backScene 默认回 "Hud"）。
            Wire(root, "nav-roster", () => SceneManager.LoadScene("Roster"));
            Wire(root, "nav-diplomacy", () => SceneManager.LoadScene("Diplomacy"));
            Wire(root, "nav-theater", () => SceneManager.LoadScene("Theater"));

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
                    SetLabel(root, "scout-status", ok
                        ? "已派出侦察——侦察兵在途，返报需时（推进时段等待返报）"
                        : "派出失败");
                    RenderEnemyIntel(root);   // 显示「在途·约第 X 日返报」（此刻无数值）
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
            Wire(root, "requisition", () => { ShowGovern(root, SessionRuntime.Requisition(RequisitionAmount)); RenderLoop(root); });
            Wire(root, "repair-fort", () => { ShowGovern(root, SessionRuntime.Repair()); RenderLoop(root); });
            Wire(root, "appease", () => { ShowGovern(root, SessionRuntime.Appease()); RenderLoop(root); });
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

            // 出征攻城（GDD_019 v2 / ADR-0010/0011）：请缨受命 → 选目标 → 组装六维 → 发起。逻辑经 CampaignRuntime（dotnet 已单测）。
            RenderOffensive(root);
            Wire(root, "offensive-authorize", () => { SessionRuntime.RequestOffensiveAuthorization(); RenderOffensive(root); });
            Wire(root, "offensive-begin", () => { SessionRuntime.BeginOffensive(); RenderOffensive(root); });
            Wire(root, "offensive-frontal", () => SetApproach(root, ApproachPlan.FrontalAssault));
            Wire(root, "offensive-feint", () => SetApproach(root, ApproachPlan.FeintLure));
            Wire(root, "offensive-siege", () => SetApproach(root, ApproachPlan.ProtractedSiege));
            Wire(root, "offensive-nightraid", () => SetApproach(root, ApproachPlan.NightRaid));
            Wire(root, "offensive-muster", () => AdjustPlan(root, p => p.Muster += 100));
            Wire(root, "offensive-supply", () => AdjustPlan(root, p => p.Supply += 50));
            Wire(root, "offensive-cavalry", () => AdjustPlan(root, AddCavalry));
            Wire(root, "offensive-advisor", () => AdjustPlan(root, p => p.Advisor = !p.Advisor));
            Wire(root, "offensive-deputy", () => AdjustPlan(root, AddDeputy));
            // 发起出征：授权通过 → 进入<b>独立区域战斗场景</b>（结算后返回本 HUD）。
            Wire(root, "offensive-launch", () =>
            {
                if (SessionRuntime.CurrentOffensivePlan == null) return;
                OffensiveResultView r = SessionRuntime.LaunchOffensive();
                if (r.BattleInProgress)
                {
                    ZoneBattleSession.EnterOffensive(SceneManager.GetActiveScene().name);
                    SceneManager.LoadScene("ZoneBattle");
                }
                else { _offensiveResult = r; RenderOffensive(root); }   // 被门拒 → 显示原因
            });

            // 守城迎敌：进入独立区域防御战场景。
            Wire(root, "offensive-defend", () =>
            {
                SessionRuntime.StartDefenseBattle();
                ZoneBattleSession.EnterDefense(SceneManager.GetActiveScene().name);
                SceneManager.LoadScene("ZoneBattle");
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

        /// <summary>治理下令反馈：成功=已派人处理（需时见效），失败=稳定错误码文案。</summary>
        private static void ShowGovern(VisualElement root, ThreeKingdom.Application.Session.CampaignCommandResult result)
            => SetLabel(root, "govern-status", result.Applied ? "已派人处理——需时见效（推进时段等待完成）" : CampaignErrorText.For(result.Error));

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
        /// <summary>E3：战略状态条（争霸领城 + 终局走向）。逻辑经 SessionRuntime（CampaignRuntime dotnet 已测）。</summary>
        private static void RenderStrategic(VisualElement root)
        {
            var c = SessionRuntime.Contention;
            string endgame;
            switch (SessionRuntime.Endgame())
            {
                case ThreeKingdom.Domain.Contention.EndgameStatus.PlayerUnifies: endgame = "已统一天下"; break;
                case ThreeKingdom.Domain.Contention.EndgameStatus.PlayerEliminated: endgame = "势力覆灭"; break;
                default: endgame = "群雄争霸中"; break;
            }
            SetLabel(root, "strategic-status", "争霸：天下共 " + c.TotalCities + " 城 · " + endgame);
        }

        private void RenderTime(VisualElement root, WorldStatusView status)
        {
            var label = root.Q<Label>("time-bar-label");
            if (label != null)
            {
                // 纪元 + 一生（GDD_026）：公元年·季　| 年龄·人生阶段（定性，不给精确寿数）。
                ArrivalLifeView life = SessionRuntime.Life();
                label.text = $"公元{SessionRuntime.CurrentYear()}·{SessionRuntime.Season()}　享年{life.Age}·{life.PhaseLabel}　{status.TimeLabel}";
            }

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
                // 在途侦察（GDD_007 派出→在途→返报）：只显示「约第 X 日返报」，无数值（须推进时段等返报）。
                foreach (var transit in panel.InTransit)
                    lines.Add(new Label("⏳ " + transit));
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
                    candidates.Add(OnboardingCue.PersonaIntro);      // 开局先点出主角性情
                    candidates.Add(OnboardingCue.Observe);
                    candidates.Add(OnboardingCue.Plan);
                    candidates.Add(OnboardingCue.AdvanceToHistory);
                    break;
                case CampaignPhase.Preparing:
                    candidates.Add(OnboardingCue.PreparationDecides); // 备战：六维准备决定胜负
                    candidates.Add(OnboardingCue.MindLever);          // 攻城亦可攻心
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

            // 在办治理事务（GDD_004 派人处理→需时见效）：显示「处理中，约第 X 日完成」。
            var inProgress = root.Q<VisualElement>("govern-inprogress");
            if (inProgress != null)
            {
                inProgress.Clear();
                foreach (var line in gov.InProgress) inProgress.Add(new Label("⏳ " + line));
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

        /// <summary>战况/复盘区元素（战后复盘的一组，与战中条件<b>互斥</b>显示，避免同框重叠）。</summary>
        private static readonly string[] ReviewElements =
        {
            "review-conclusion", "review-toggle", "review-detail",
            "review-notice", "review-continuations", "review-career-hint", "review-selection",
        };

        /// <summary>
        /// 战况/战果复盘区（战中/战后才显示；其余相位整块隐藏）。
        /// <b>战中</b>只显兵法条件进度 + 「结算战果」；<b>战后</b>只显复盘——两者互斥，杜绝重叠。
        /// </summary>
        private static void RenderBattleConditions(VisualElement root)
        {
            CampaignPhase phase = SessionRuntime.Phase().Phase;
            bool inBattle = phase == CampaignPhase.Battle;
            bool aftermath = phase == CampaignPhase.Aftermath;

            SetDisplay(root, "outcome-chain", inBattle || aftermath);

            // 战中元素只在战中显示；复盘元素只在战后显示。
            SetDisplay(root, "battle-status", inBattle);
            SetDisplay(root, "battle-conditions", inBattle);
            SetDisplay(root, "resolve-outcome", inBattle);
            foreach (var name in ReviewElements) SetDisplay(root, name, aftermath);

            if (inBattle)
            {
                SetLabel(root, "battle-status", "战斗进行中——满足兵法条件后点「结算战果」。");
                var area = root.Q<VisualElement>("battle-conditions");
                if (area != null)
                {
                    area.Clear();
                    BattleConditionProgressView progress = SessionRuntime.BattleConditionProgress();
                    foreach (var line in progress.Lines)
                    {
                        area.Add(new Label($"{line.TacticName}：{line.ProgressLabel}"));
                        foreach (var met in line.Satisfied) area.Add(new Label("　✓ " + met));
                        foreach (var unmet in line.Unsatisfied) area.Add(new Label("　✗ " + unmet));
                    }
                }
                SetEnabled(root, "resolve-outcome", true);
            }
        }

        /// <summary>
        /// 未接线面板占位收敛（后续接入后移除）：不再刷屏「接入中」——顶栏给真实目标，未接按钮仅禁用、状态留空。
        /// 军议/敌情/侦察（003）、账本/治理/备战/战斗（004）已接入；残留：外交求援 + 袭扰/伏击（另属子系统）。
        /// </summary>
        private static void RenderPendingPanels(VisualElement root)
        {
            // 席位目标随所选开局真实投影（GDD_026；不再硬编码汜水关）。
            SetLabel(root, "hud-objective", SessionRuntime.SeatObjective());
            SetLabel(root, "hud-banner", string.Empty);
            // 未接面板：状态留空（不刷「接入中」），按钮禁用即可。
            SetLabel(root, "diplo-status", string.Empty);
            SetLabel(root, "raid-status", string.Empty);
            SetLabel(root, "ambush-status", string.Empty);

            SetEnabled(root, "request-aid", false);
            SetEnabled(root, "raid", false);
            SetEnabled(root, "ambush", false);
        }

        /// <summary>当前出征结果展示模型（表现态，不入 Domain/存档；null=尚未发起）。</summary>
        private OffensiveResultView _offensiveResult;

        /// <summary>出征攻城面板（GDD_019 v2）：目标门 + 六维草稿控件可用性 + 计划预览 + 结果。纯读运行期投影。</summary>
        private void RenderOffensive(VisualElement root)
        {
            OffensiveTargetsView targets = SessionRuntime.OffensiveTargets();
            var lines = new List<string>();
            foreach (OffensiveTargetLine t in targets.Targets) lines.Add($"{t.CityLabel}：{t.GateLabel}");
            SetLabel(root, "offensive-targets", string.Join("　", lines));

            SetEnabled(root, "offensive-authorize", !targets.Authorized);
            SetEnabled(root, "offensive-begin", targets.AnyAttackable);

            OffensivePlan plan = SessionRuntime.CurrentOffensivePlan;
            bool hasPlan = plan != null;
            foreach (string btn in OffensiveDimButtons) SetEnabled(root, btn, hasPlan);
            SetEnabled(root, "offensive-launch", hasPlan && targets.AnyAttackable);

            var advisorBtn = root.Q<Button>("offensive-advisor");
            if (advisorBtn != null && hasPlan) advisorBtn.text = "军师随军：" + (plan.Advisor ? "是" : "否");

            var forming = root.Q<VisualElement>("offensive-forming");
            var missing = root.Q<VisualElement>("offensive-missing");
            forming?.Clear();
            missing?.Clear();

            if (hasPlan)
            {
                OffensivePlanView view = SessionRuntime.PreviewOffensive();
                SetLabel(root, "offensive-preview",
                    $"目标{view.TargetLabel}｜路线{OffensiveText.Approach(plan.Approach)}｜预计战力 {view.ForcePreview}·士气 {view.MoraleLabel}｜{(view.Scouted ? "已侦察" : "未侦察")}");
                if (forming != null)
                    foreach (string c in view.FormingConditions) forming.Add(new Label("　✓ " + c));
                if (missing != null)
                    foreach (string c in view.MissingConditions) missing.Add(new Label("　✗ 尚缺：" + c));
            }
            else
            {
                SetLabel(root, "offensive-preview", targets.Authorized ? "已受命——点「组装出征」选目标布势。" : "先请缨受命（君主授权）。");
            }

            RenderOffensiveResult(root);
        }

        /// <summary>渲染出征结果（被门拒/战败退兵/破城占城归属；无胜率）。</summary>
        private void RenderOffensiveResult(VisualElement root)
        {
            bool has = _offensiveResult != null;
            SetLabel(root, "offensive-result", has
                ? _offensiveResult.ConclusionLabel + (string.IsNullOrEmpty(_offensiveResult.OwnershipLabel) ? "" : "　" + _offensiveResult.OwnershipLabel)
                : string.Empty);

            var notes = root.Q<VisualElement>("offensive-result-notes");
            if (notes == null) return;
            notes.Clear();
            if (!has) return;
            foreach (string n in _offensiveResult.Notes) notes.Add(new Label("　· " + n));
            foreach (string tactic in _offensiveResult.Tactics) notes.Add(new Label("　兵法：" + tactic));
        }

        /// <summary>出征六维草稿控件（须已开始组装才可用）。</summary>
        private static readonly string[] OffensiveDimButtons =
        {
            "offensive-frontal", "offensive-feint", "offensive-siege", "offensive-nightraid",
            "offensive-muster", "offensive-supply", "offensive-cavalry", "offensive-advisor", "offensive-deputy",
        };

        private void SetApproach(VisualElement root, ApproachPlan approach)
        {
            OffensivePlan plan = SessionRuntime.CurrentOffensivePlan;
            if (plan == null) return;
            plan.Approach = approach;
            if (approach == ApproachPlan.ProtractedSiege && plan.SiegeSegments < 8) plan.SiegeSegments = 8;   // 承诺长围（时间成本）
            if (approach == ApproachPlan.NightRaid) plan.Segment = DaySegment.Night;                          // 择夜（夜袭需夜）
            RenderOffensive(root);
        }

        private void AdjustPlan(VisualElement root, Action<OffensivePlan> mutate)
        {
            OffensivePlan plan = SessionRuntime.CurrentOffensivePlan;
            if (plan == null) return;
            mutate(plan);
            RenderOffensive(root);
        }

        /// <summary>配骑兵（不超过投入兵力，避免编成越界）。</summary>
        private static void AddCavalry(OffensivePlan plan)
        {
            int assigned = 0;
            foreach (int v in plan.Composition.Values) assigned += v;
            if (assigned + 100 > plan.Muster) return;   // 编成不得超过投入兵力
            plan.Composition[TroopType.Cavalry] = (plan.Composition.TryGetValue(TroopType.Cavalry, out int c) ? c : 0) + 100;
        }

        /// <summary>加一名副将（从花名册取首位，避免重复添加）。</summary>
        private static void AddDeputy(OffensivePlan plan)
        {
            var roster = SessionRuntime.DeputyRoster;
            if (roster.Count == 0 || plan.Deputies.Count > 0) return;
            plan.Deputies.Add(roster[0]);
        }

        private static void SetDisplay(VisualElement root, string name, bool shown)
        {
            var element = root.Q<VisualElement>(name);
            if (element != null) element.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
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
