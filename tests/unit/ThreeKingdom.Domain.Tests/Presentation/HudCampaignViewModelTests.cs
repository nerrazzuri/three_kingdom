using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Presentation.Projections;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-028 story-004：HUD 战役主循环 ViewModel（TR-ux-001/005 / ADR-0002/0009）。
    /// 覆盖 AC-2 治理+因果说明+错误码文案、AC-3 草稿vs承诺、AC-4 兵法条件进度（非按钮）、
    /// AC-5 四相位可做动作集、AC-6/7 多维分列+渲染恒等。经真实 <see cref="CampaignSessionService"/> 装配各相位会话，确定性。
    /// </summary>
    [TestFixture]
    public class HudCampaignViewModelTests
    {
        private static readonly PlayableCampaign Scenario = PlayableCampaign.Default();
        private readonly CampaignSessionService _service = new CampaignSessionService();

        private CampaignSession NewSession()
        {
            CampaignStartResult r = _service.StartCampaign(Scenario.StartConfig);
            Assert.That(r.Started, Is.True, "场景开局应成功（已验证夹具）。");
            return r.Session!;
        }

        // ---- AC-2：治理三动作各带因果方向说明 + 多维账本分列 ----

        [Test]
        public void test_governance_view_has_three_actions_each_with_causal_direction()
        {
            var view = GovernanceActionView.FromSession(NewSession());

            Assert.That(view.Actions.Count, Is.EqualTo(3));
            foreach (var a in view.Actions)
                Assert.That(a.CausalHint.Contains("↑") || a.CausalHint.Contains("↓"), Is.True,
                    $"治理动作「{a.Label}」须含因果方向说明。");
            Assert.That(view.Ledger.MoraleLabel, Does.Contain("民心"));
            Assert.That(view.Ledger.SecurityLabel, Does.Contain("治安"));
            Assert.That(view.Ledger.FortificationLabel, Does.Contain("工事"));
        }

        // ---- AC-2：非法治理命令 → 稳定错误码文案，账本不变（无部分写入） ----

        [Test]
        public void test_requisition_over_stock_returns_stable_error_and_leaves_ledger_unchanged()
        {
            var s = NewSession();
            long stockBefore = s.CityEconomy!.Stock;
            int moraleBefore = s.CityEconomy!.CivMorale;

            CampaignCommandResult result = _service.RequisitionFood(s, 1_000_000);   // 远超库存

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CampaignErrorCode.InsufficientStock));
            Assert.That(CampaignErrorText.For(result.Error), Is.Not.Empty, "错误码须有对应玩家文案。");
            Assert.That(s.CityEconomy!.Stock, Is.EqualTo(stockBefore), "账本不变（无部分写入）。");
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(moraleBefore));
        }

        // ---- AC-3：备战草稿（可移除） vs 已提交（不可移除，不可反悔） ----

        [Test]
        public void test_prep_panel_distinguishes_draft_from_committed()
        {
            var s = NewSession();
            _service.AddPlanOrder(s, Scenario.AmbushPlan());

            var draftView = PrepPanelView.FromSession(s);
            Assert.That(draftView.IsCommitted, Is.False);
            Assert.That(draftView.DraftOrders.Count, Is.EqualTo(1));
            Assert.That(draftView.DraftOrders[0].Removable, Is.True);
            Assert.That(draftView.CommittedOrders.Count, Is.EqualTo(0));

            var submit = _service.SubmitPlan(s);
            Assert.That(submit.Committed, Is.True);

            var committedView = PrepPanelView.FromSession(s);
            Assert.That(committedView.IsCommitted, Is.True);
            Assert.That(committedView.CommittedOrders.Count, Is.EqualTo(1));
            Assert.That(committedView.CommittedOrders[0].Removable, Is.False, "承诺后不可移除。");
            Assert.That(committedView.DraftOrders.Count, Is.EqualTo(0), "承诺后不再显示残留草稿。");
        }

        // ---- AC-4：兵法条件进度 = 已满足/未满足 + 还差 N 条；非可执行命令项 ----

        [Test]
        public void test_battle_condition_progress_lists_met_unmet_and_is_not_buttons()
        {
            // 假退伏击链 3 条件，满足其 2 → 还差 1。
            var satisfied = new[] { TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued };
            var view = BattleConditionProgressView.Build(Scenario.TacticChains, satisfied);

            var feint = view.Lines.First(l => l.TacticName == "假退伏击");
            Assert.That(feint.Satisfied.Count, Is.EqualTo(2));
            Assert.That(feint.Unsatisfied.Count, Is.EqualTo(1));
            Assert.That(feint.RemainingCount, Is.EqualTo(1));
            Assert.That(feint.ProgressLabel, Is.EqualTo("还差 1 条"));
            Assert.That(feint.IsFormed, Is.False);

            // 结构性：条件进度是状态指示，非可执行命令项（无「执行/命令/按钮」字段）。
            AssertNoPropContains(typeof(TacticProgressLine), "command", "execute", "button", "invoke");
        }

        [Test]
        public void test_battle_condition_all_met_is_formed()
        {
            var all = new[]
            {
                TacticCondition.ControlledRetreatKeptFormation,
                TacticCondition.EnemyPursued,
                TacticCondition.AmbushSurprise,
            };
            var view = BattleConditionProgressView.Build(Scenario.TacticChains, all);
            var feint = view.Lines.First(l => l.TacticName == "假退伏击");
            Assert.That(feint.IsFormed, Is.True);
            Assert.That(feint.RemainingCount, Is.EqualTo(0));
            Assert.That(feint.ProgressLabel, Is.EqualTo("已成型"));
        }

        // ---- AC-5：四相位各有非空且互不相同的可做动作集 ----

        [Test]
        public void test_each_phase_has_distinct_nonempty_action_set()
        {
            var governance = HudPhaseView.ForSession(NewSession());
            Assert.That(governance.Phase, Is.EqualTo(CampaignPhase.Governance));

            var prepSession = NewSession();
            _service.AddPlanOrder(prepSession, Scenario.AmbushPlan());
            var preparing = HudPhaseView.ForSession(prepSession);
            Assert.That(preparing.Phase, Is.EqualTo(CampaignPhase.Preparing));

            var battleSession = NewSession();
            _service.AddPlanOrder(battleSession, Scenario.AmbushPlan());
            _service.SubmitPlan(battleSession);
            _service.StartBattle(battleSession, Scenario.Units(), Scenario.BattleConfig, Scenario.BattleSeed, Scenario.TacticChains);
            var battle = HudPhaseView.ForSession(battleSession);
            Assert.That(battle.Phase, Is.EqualTo(CampaignPhase.Battle));

            var context = new OutcomeContext(PlayableCampaign.Player, PlayableCampaign.Fanshui);
            _service.ResolveBattleOutcome(battleSession, OutcomeBranch.Victory, context, Scenario.OutcomeConfig);
            var aftermath = HudPhaseView.ForSession(battleSession);
            Assert.That(aftermath.Phase, Is.EqualTo(CampaignPhase.Aftermath));

            var all = new[] { governance, preparing, battle, aftermath };
            foreach (var v in all)
                Assert.That(v.AvailableActions, Is.Not.Empty, $"{v.Phase} 相位动作集不可为空（AC-5）。");
            var signatures = all.Select(v => string.Join("|", v.AvailableActions)).ToList();
            Assert.That(signatures.Distinct().Count(), Is.EqualTo(4), "四相位动作集须互不相同。");
        }

        // ---- AC-6/7：渲染恒等 + 多维账本无合并总分 ----

        [Test]
        public void test_view_models_are_deterministic_and_ledger_has_no_merged_total()
        {
            var s = NewSession();
            var g1 = GovernanceActionView.FromSession(s);
            var g2 = GovernanceActionView.FromSession(s);

            Assert.That(g2.Actions.Count, Is.EqualTo(g1.Actions.Count));
            for (int i = 0; i < g1.Actions.Count; i++)
            {
                Assert.That(g2.Actions[i].Label, Is.EqualTo(g1.Actions[i].Label));
                Assert.That(g2.Actions[i].CausalHint, Is.EqualTo(g1.Actions[i].CausalHint));
            }
            Assert.That(g2.Ledger.StockLabel, Is.EqualTo(g1.Ledger.StockLabel));

            // P6 多维不合并：账本无 total/overall/combined 等单一综合字段。
            AssertNoPropContains(typeof(CityLedgerView), "total", "overall", "combined", "aggregate", "power");
        }

        // ---- 帮助 ----

        private static string[] PropNames(Type t) =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name.ToLowerInvariant()).ToArray();

        private static void AssertNoPropContains(Type t, params string[] forbiddenSubstrings)
        {
            foreach (var name in PropNames(t))
                foreach (var bad in forbiddenSubstrings)
                    Assert.That(name.Contains(bad), Is.False,
                        $"{t.Name} 不得含属性「{name}」（设计锁禁止 '{bad}'）。");
        }
    }
}
