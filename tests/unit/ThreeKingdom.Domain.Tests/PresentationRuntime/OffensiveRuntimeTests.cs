using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 出征攻城入口 <see cref="CampaignRuntime"/>（GDD_019 v2 / ADR-0010/0011）：选目标 + 授权门 + 六维组装 +
    /// 闭合因果预览 + 发起 + 占城归属。验证 UI 只经运行期接口、权威结算在 Application、准备决定胜负、失败可继续。
    /// 介质用纯内存替身（test-standards：无文件 I/O）。
    /// </summary>
    [TestFixture]
    public class OffensiveRuntimeTests
    {
        private CampaignRuntime _runtime = null!;

        [SetUp]
        public void SetUp()
        {
            _runtime = new CampaignRuntime(new InMemorySaveMedium());
            _runtime.NewGame();
        }

        [Test]
        public void test_target_gate_unauthorized_until_lord_authorizes()
        {
            OffensiveTargetsView before = _runtime.OffensiveTargets();
            Assert.That(before.Authorized, Is.False, "开局未受命。");
            Assert.That(before.AnyAttackable, Is.False, "未授权 → 无可攻目标。");
            Assert.That(before.Targets[0].Gate, Is.EqualTo(OffensiveGateResult.NotAuthorized));

            _runtime.RequestOffensiveAuthorization();

            OffensiveTargetsView after = _runtime.OffensiveTargets();
            Assert.That(after.Authorized, Is.True, "君主受命后已授权。");
            Assert.That(after.AnyAttackable, Is.True);
            Assert.That(after.Targets[0].Gate, Is.EqualTo(OffensiveGateResult.Authorized), "授权 + 敌控城 → 可攻。");
        }

        [Test]
        public void test_offensive_is_available_in_governance_phase()
        {
            Assert.That(_runtime.Phase().AvailableActions, Contains.Item("出征"), "治理相位看得到出征入口（AC-5）。");
        }

        [Test]
        public void test_begin_offensive_yields_default_draft_with_preview()
        {
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            Assert.That(plan.Approach, Is.EqualTo(ApproachPlan.FrontalAssault), "默认正面强攻。");
            Assert.That(plan.Lead, Is.Not.Null, "默认主将=太守亲征。");

            OffensivePlanView view = _runtime.PreviewOffensive();
            Assert.That(view.ForcePreview, Is.GreaterThan(0), "预览派生进攻方战力（闭合因果可见）。");
        }

        [Test]
        public void test_strong_six_dimensional_plan_wins_and_conquers()
        {
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FeintLure;
            plan.Composition[TroopType.Cavalry] = 400;   // 骑兵份额 → 追击条件
            plan.Composition[TroopType.Infantry] = 500;
            plan.Advisor = true;

            OffensiveResultView result = _runtime.LaunchOffensive();

            Assert.That(result.Launched, Is.True);
            Assert.That(result.Victory, Is.True, "六维准备充分 → 破城（准备决定胜负）。");
            Assert.That(result.OwnershipLabel, Does.Contain("直辖"), "首座占城归玩家直辖（占城 C 前两座）。");
            Assert.That(result.Tactics, Is.Not.Empty, "携入兵法条件（诱敌链成型）。");
        }

        [Test]
        public void test_bare_plan_loses_but_campaign_continues()
        {
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 0;
            plan.Supply = 0;
            plan.Approach = ApproachPlan.FrontalAssault;

            OffensiveResultView result = _runtime.LaunchOffensive();

            Assert.That(result.Launched, Is.True, "授权通过 → 出征。");
            Assert.That(result.Victory, Is.False, "裸战 → 败。");
            Assert.That(result.Notes, Has.Some.Contains("继续"), "失败必留可继续状态（红线）。");
        }

        [Test]
        public void test_launch_without_authorization_is_rejected()
        {
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;

            OffensiveResultView result = _runtime.LaunchOffensive();

            Assert.That(result.Launched, Is.False, "未受命 → 未出征。");
            Assert.That(result.ConclusionLabel, Does.Contain("未授权"));
        }

        [Test]
        public void test_preview_flags_missing_conditions_for_feint_without_cavalry_or_scout()
        {
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Approach = ApproachPlan.FeintLure;   // 无骑兵、未侦察

            OffensivePlanView view = _runtime.PreviewOffensive();

            Assert.That(view.FormingConditions, Contains.Item("受控撤退保持队形"), "路线提供的基础条件成型。");
            Assert.That(view.MissingConditions, Contains.Item("敌军追击"), "缺骑兵 → 追击条件未成型（军师风格提示，无胜率）。");
            Assert.That(view.MissingConditions, Contains.Item("伏兵突然性"), "未侦察/非智谋 → 突然性未成型。");
        }
    }
}
