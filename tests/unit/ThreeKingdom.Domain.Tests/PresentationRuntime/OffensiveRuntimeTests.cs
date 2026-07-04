using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Talent;
using ThreeKingdom.Application.Theater;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Talent;
using ThreeKingdom.Domain.Theater;
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

        /// <summary>推进出征战斗至终局并结算后果。</summary>
        private OffensiveResultView FightToEnd()
        {
            for (int i = 0; i < 12 && _runtime.HasOffensiveBattle; i++) _runtime.OffensiveBattleResolveRound();
            return _runtime.ConcludeOffensive();
        }

        [Test]
        public void test_strong_six_dimensional_plan_enters_battle_wins_and_conquers()
        {
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FrontalAssault;   // 强攻压破正面
            plan.Composition[TroopType.Cavalry] = 400;
            plan.Composition[TroopType.Infantry] = 500;
            plan.Advisor = true;

            OffensiveResultView launched = _runtime.LaunchOffensive();
            Assert.That(launched.BattleInProgress, Is.True, "授权通过 → 进入区域战斗（多回合，非一击）。");

            OffensiveResultView result = FightToEnd();
            Assert.That(result.Victory, Is.True, "六维准备充分 → 破城（准备决定胜负）。");
            Assert.That(result.OwnershipLabel, Does.Contain("直辖"), "首座占城归玩家直辖（占城 C 前两座）。");
            Assert.That(_runtime.Session.ConquestCount, Is.EqualTo(1), "破城 → 占城计数 +1（权威结算）。");
        }

        [Test]
        public void test_bare_plan_loses_but_campaign_continues()
        {
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 20;
            plan.Supply = 0;
            plan.Approach = ApproachPlan.FrontalAssault;

            _runtime.LaunchOffensive();
            OffensiveResultView result = FightToEnd();

            Assert.That(result.Victory, Is.False, "裸战 → 败（守军压倒）。");
            Assert.That(result.Notes, Has.Some.Contains("继续"), "失败必留可继续状态（红线）。");
            Assert.That(_runtime.Session.ConquestCount, Is.EqualTo(0), "败不占城。");
        }

        [Test]
        public void test_ai_can_autoresolve_offensive_for_the_player()
        {
            // 玩家可选挂 AI 代打：发起后 AI 代打至终局并结算占城（强部署 → 胜、占城）。
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FrontalAssault;

            OffensiveResultView launched = _runtime.LaunchOffensive();
            Assert.That(launched.BattleInProgress, Is.True);

            OffensiveResultView result = _runtime.AutoResolveOffensive();   // 挂 AI 代打
            Assert.That(result.Victory, Is.True, "强部署 → AI 代打破城。");
            Assert.That(_runtime.Session.ConquestCount, Is.EqualTo(1), "代打胜亦经权威占城结算。");
        }

        [Test]
        public void test_conquest_advances_contention_toward_endgame()
        {
            // 争霸领土（M13/M14）：破敌城 → 玩家领土增、敌减；群雄割据世界里这是"推进"而非"统一"（群雄尚存）。
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FrontalAssault;
            _runtime.LaunchOffensive();
            OffensiveResultView result = _runtime.AutoResolveOffensive();

            Assert.That(result.Victory, Is.True);
            Assert.That(_runtime.Contention.CitiesOf(PlayableCampaign.Player), Is.EqualTo(2), "破城 → 玩家领土 +1（汜水关→+虎牢关）。");
            Assert.That(_runtime.Contention.CitiesOf(PlayableCampaign.Enemy), Is.EqualTo(1), "被夺方（袁术）领土 −1（尚余寿春）。");
            Assert.That(_runtime.Endgame(), Is.EqualTo(EndgameStatus.Ongoing), "群雄割据世界：占一城是推进，群雄尚存 → 争霸继续（非一城即统一）。");
        }

        [Test]
        public void test_defense_battle_enters_zone_battle_and_holds_the_gate()
        {
            // 守城=攻守统一：玩家守方分区布防，敌AI 来攻；守军(700) 挡住来犯(500) → 守土成功。
            ZoneBattleView start = _runtime.StartDefenseBattle();
            Assert.That(start.PlayerIsAttacker, Is.False, "守城 → 玩家为守方。");
            for (int i = 0; i < 12 && _runtime.HasDefenseBattle; i++) _runtime.DefenseBattleResolveRound();
            Assert.That(_runtime.DefenseBattleOver, Is.True, "守城战分出胜负。");
            Assert.That(_runtime.DefenseHeld, Is.True, "守军挡住来犯敌军 → 守土成功（攻守统一同引擎）。");
        }

        [Test]
        public void test_talent_recruitment_through_campaign_runtime()
        {
            // 反全知：未知晓不可见 → 侦察知晓 → 可见 → 厚待招揽 → 入伙为将（GDD_020 四层）。
            Assert.That(HasTalent(_runtime.VisibleTalents(), PlayableCampaign.Xiaojiang), Is.False, "未知晓 → 不可见。");
            _runtime.RevealTalent(PlayableCampaign.Xiaojiang, TalentChannel.Scouting);
            Assert.That(HasTalent(_runtime.VisibleTalents(), PlayableCampaign.Xiaojiang), Is.True, "侦察知晓后可见。");

            var strong = new RecruitmentOffer(FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One);
            TalentRecruitAttempt r = _runtime.RecruitTalent(PlayableCampaign.Xiaojiang, strong);
            Assert.That(r.Valid && r.Verdict == RecruitmentVerdict.Joined, Is.True, "厚待易招之将 → 出仕。");
            Assert.That(_runtime.HasRecruited(PlayableCampaign.Xiaojiang), Is.True, "入伙记入。");
            Assert.That(r.Joined, Is.Not.Null, "入伙人才为将（喂战斗）。");
        }

        private static bool HasTalent(System.Collections.Generic.IReadOnlyList<TalentProfile> list, TalentId id)
        {
            foreach (TalentProfile p in list) if (p.Id == id) return true;
            return false;
        }

        [Test]
        public void test_conquered_city_enters_theater_and_can_be_delegated()
        {
            // 占城 C 归玩家（首座）→ 城入多城战区 → 可委任下属打理（M12）。
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FrontalAssault;
            _runtime.LaunchOffensive();
            OffensiveResultView result = _runtime.AutoResolveOffensive();

            Assert.That(result.Victory, Is.True);
            Assert.That(_runtime.Theater.Holds(PlayableCampaign.EnemyCity), Is.True, "占城归玩家 → 入多城战区。");

            TheaterCommandResult d = _runtime.DelegateCity(PlayableCampaign.EnemyCity, new CharacterId("char-aide"));
            Assert.That(d.Applied, Is.True);
            Assert.That(_runtime.Theater.Of(PlayableCampaign.EnemyCity)!.Mode, Is.EqualTo(GovernanceMode.Delegated), "可委任打理。");
        }

        [Test]
        public void test_offensive_blocked_by_pact_until_breach()
        {
            // 外交战略约束（M11）：与敌城之主缔互不侵犯 → 出征受阻；背约后方可攻。
            _runtime.RequestOffensiveAuthorization();
            OffensivePlan plan = _runtime.BeginOffensive(PlayableCampaign.EnemyCity);
            plan.Muster = 900;
            plan.Supply = 400;
            plan.Approach = ApproachPlan.FrontalAssault;

            PactResult pact = _runtime.ProposePact(PlayableCampaign.Enemy, DiplomaticStance.NonAggression,
                new PactFactors(FixedPoint.One, FixedPoint.One, FixedPoint.One));
            Assert.That(pact.Accepted, Is.True, "厚礼睦邻 → 缔约成。");

            OffensiveResultView blocked = _runtime.LaunchOffensive();
            Assert.That(blocked.BattleInProgress, Is.False, "有约 → 不可径攻。");
            Assert.That(blocked.ConclusionLabel, Does.Contain("不可径攻"));

            _runtime.BreachPact(PlayableCampaign.Enemy);   // 背约（承声誉代价）
            OffensiveResultView launched = _runtime.LaunchOffensive();
            Assert.That(launched.BattleInProgress, Is.True, "背约后（转敌对）可攻。");
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
