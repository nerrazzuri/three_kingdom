using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Application.Session;
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
            RenderDiplomacy(root, SessionRuntime.Diplomacy());
            RenderCouncil(root, SessionRuntime.Council());
            RenderRoster(root, SessionRuntime.Roster());
            RenderScout(root, SessionRuntime.ScoutStatus());
            RenderRaid(root, SessionRuntime.RaidStatus());
            RenderObjective(root);

            var convene = root.Q<Button>("convene");
            if (convene != null) convene.clicked += () => RenderCouncil(root, SessionRuntime.Convene());

            var advance = root.Q<Button>("advance-time");
            if (advance != null)
                advance.clicked += () =>
                {
                    RenderTime(root, SessionRuntime.Advance()); // 推进 + 跨日提示
                    RenderLedger(root, SessionRuntime.Ledger()); // 跨日结算 + 袭扰见效/援粮抵达可能改账本
                    RenderEnemy(root, SessionRuntime.Enemy());   // 侦察返报/情报过时
                    RenderDiplomacy(root, SessionRuntime.Diplomacy()); // 援粮可能抵达
                    RenderCouncil(root, SessionRuntime.Council());      // 知识/时间变化，建议可能过时
                    RenderScout(root, SessionRuntime.ScoutStatus());    // 侦察队可能已返报
                    RenderRaid(root, SessionRuntime.RaidStatus());      // 袭扰队可能已见效
                    RenderObjective(root);                       // 推进可能触发胜负
                };

            var requestAid = root.Q<Button>("request-aid");
            if (requestAid != null) requestAid.clicked += () => RenderDiplomacy(root, SessionRuntime.RequestAid());

            // 侦察/袭扰均为「派出」——非即时，结果在推进时段抵达时显现。
            var scout = root.Q<Button>("scout");
            if (scout != null) scout.clicked += () => RenderScout(root, SessionRuntime.DispatchScout());

            var raid = root.Q<Button>("raid");
            if (raid != null)
                raid.clicked += () =>
                {
                    RenderRaid(root, SessionRuntime.DispatchRaid()); // 派出（即兑付粮草代价）
                    RenderLedger(root, SessionRuntime.Ledger());     // 反映粮草扣减
                };

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

        /// <summary>渲染一局目标/胜负（守城待变）；局终禁用推进/侦察/存档并显示横幅。</summary>
        private void RenderObjective(VisualElement root)
        {
            var view = SessionRuntime.Objective();
            SetLabel(root, "hud-objective", view.ObjectiveLabel);
            SetLabel(root, "hud-banner", view.BannerLabel);

            if (view.IsOver)
            {
                // 一局已结束：冻结推进/侦察/求援/存档（返回主菜单仍可用，可继续/读档）。
                SetEnabled(root, "advance-time", false);
                SetEnabled(root, "scout", false);
                SetEnabled(root, "request-aid", false);
                SetEnabled(root, "convene", false);
                SetEnabled(root, "raid", false);
                SetEnabled(root, "save-game", false);
            }
        }

        /// <summary>渲染侦察派出（派出→在途→返报；非即时）。</summary>
        private void RenderScout(VisualElement root, ScoutView view)
        {
            SetEnabled(root, "scout", view.CanDispatch);
            SetLabel(root, "scout-status", view.StatusLabel);
        }

        /// <summary>渲染袭扰（断粮疲敌；派出→在途→见效）：按钮可用性 + 中文状态（不泄露敌真值）。</summary>
        private void RenderRaid(VisualElement root, RaidView view)
        {
            SetEnabled(root, "raid", view.CanDispatch);
            SetLabel(root, "raid-status", view.StatusLabel);
        }

        /// <summary>渲染外交求粮状态（中文）+ 求援按钮可用性（受控一局一次）。</summary>
        private void RenderDiplomacy(VisualElement root, DiplomacyView view)
        {
            SetLabel(root, "diplo-status", view.StatusLabel);
            SetEnabled(root, "request-aid", view.CanRequest);
        }

        /// <summary>渲染人物花名册（GDD_005：身份/职责/健康 + 能力五域）。</summary>
        private void RenderRoster(VisualElement root, RosterView view)
        {
            var list = root.Q<VisualElement>("roster-list");
            if (list == null) return;
            list.Clear();
            foreach (var c in view.Characters)
            {
                list.Add(new Label(c.Title));
                list.Add(new Label("　" + c.Capabilities));
            }
        }

        /// <summary>渲染军议建议（GDD_008：并列条件化建议；过时提示；无最优解高亮，P11）。view 为 null = 未召开。</summary>
        private void RenderCouncil(VisualElement root, CouncilView view)
        {
            SetLabel(root, "council-stale", view != null && view.IsStale ? "（情报已变，建议过时——请重开军议）" : string.Empty);

            var list = root.Q<VisualElement>("council-advice");
            if (list == null) return;
            list.Clear();
            if (view == null) return;

            foreach (var a in view.Advice)
            {
                // 每条建议：路线名 + 依据可靠性（定性）+ 所需条件 + 风险 + 缺失情报。并列，无优劣排序。
                list.Add(new Label("【" + a.CandidateLabel + "】" + a.EvidenceConfidenceLabel));
                foreach (var cond in a.RequiredConditions) list.Add(new Label("　需：" + cond));
                foreach (var risk in a.Risks) list.Add(new Label("　险：" + risk));
                foreach (var miss in a.MissingIntel) list.Add(new Label("　缺情报：" + miss));
            }
        }

        private static void SetEnabled(VisualElement root, string name, bool enabled)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.SetEnabled(enabled);
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
