using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Application.Talent;
using ThreeKingdom.Application.Theater;
using ThreeKingdom.Domain.Theater;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>
    /// 战役会话运行期核心（epic-028 story-001 / TR-ux-005）：Unity 壳与完整 <see cref="CampaignSession"/>
    /// 脊梁之间的<b>纯 C# 生命周期接缝</b>——新局 / 推进 / 状态投影 / 统一信封存读档。
    /// <para>
    /// 架构边界（ADR-0002 / ADR-0009）：只持 Application 会话句柄，一切变更经 <see cref="CampaignSessionService"/>；
    /// 投影为纯函数（同会话态渲染恒等，ADR-0004）；存档 I/O 经 <see cref="ISaveMedium"/> 端口注入（R-7），
    /// 原子写回沿 SaveRepository 同款「临时槽 + 原子改名」编排（失败保留上一份有效存档，ADR-0005）。
    /// 场景配置来自注入的 <see cref="PlayableCampaign"/>（与 console harness 单一同源，勿复制数值）。
    /// 本类无 UnityEngine 依赖，可 <c>dotnet test</c>（Unity 侧薄壳见 Assets/UI/SessionRuntime.cs）。
    /// </para>
    /// </summary>
    public sealed class CampaignRuntime
    {
        /// <summary>默认存档槽名（与旧竖切 "campaign" 槽区分，避免旧格式误读）。</summary>
        public const string DefaultSlot = "campaign-session";

        private readonly CampaignSessionService _service = new CampaignSessionService();
        private readonly ISaveMedium _medium;
        private readonly PlayableCampaign _scenario;
        private readonly string _slot;
        private CampaignSession? _session;
        private int _daysCrossedLastAdvance;

        /// <summary>军议/敌情屏调节项（置信档阈值等；表现态，不入会话/存档）。</summary>
        private readonly CouncilIntelTuning _councilTuning = CouncilIntelTuning.Default;

        /// <summary>最近一次召开军议的建议集（绑定召开时知识快照；用于对实时快照重判过时）。null=尚未召开。</summary>
        private CouncilAdviceSet? _lastCouncil;

        /// <summary>构造运行期核心；存档介质必须注入（端口），场景缺省为「汜水关太守」共享场景源。</summary>
        public CampaignRuntime(ISaveMedium medium, PlayableCampaign? scenario = null, string slot = DefaultSlot)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _scenario = scenario ?? PlayableCampaign.Default();
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            _slot = slot;
        }

        /// <summary>当前会话（首访自动开局，保证 HUD 单独打开也可玩）。仅供后续屏 story 经服务命令使用。</summary>
        public CampaignSession Session => _session ??= StartNew();

        /// <summary>共享场景源（不可变配置；供屏 story 取卫星配置/梯队等，勿复制数值）。</summary>
        public PlayableCampaign Scenario => _scenario;

        /// <summary>开新局（MainMenu「新游戏」）：以共享场景配置重开会话，返回初始世界状态视图。</summary>
        public WorldStatusView NewGame()
        {
            _session = StartNew();
            _daysCrossedLastAdvance = 0;
            return Status();
        }

        /// <summary>推进 <paramref name="segments"/> 个时段（HUD「推进时段」），返回推进后的世界状态视图（含跨日提示）。</summary>
        public WorldStatusView Advance(int segments = 1)
        {
            CampaignSession session = Session;
            int dayBefore = session.CurrentTime.Day;
            _service.Advance(session, segments);
            _daysCrossedLastAdvance = session.CurrentTime.Day - dayBefore;
            return Status();
        }

        /// <summary>取当前世界状态视图（不推进；纯函数——同会话态两次调用结果恒等）。</summary>
        public WorldStatusView Status()
        {
            WorldTime t = Session.CurrentTime;
            return new WorldStatusView(new WorldStatusProjection(t.Day, t.Segment, t.AbsoluteIndex, _daysCrossedLastAdvance));
        }

        // --- 军议/敌情屏（epic-028 story-003 / TR-ux-002/003）。只读投影 + 军议编排，反全知：UI 只经玩家知识投影。---

        /// <summary>会话是否启用情报/军议循环（军议与敌情屏可用性）。</summary>
        public bool HasIntel => Session.HasIntel;

        /// <summary>
        /// 召开军议（GDD_008）：经 <see cref="CampaignSessionService.ConveneCouncil"/> 读当前知识快照产出并列建议集，
        /// 绑定召开时快照并缓存；返回军议屏展示模型（小数置信经调节项映射为定性档，无成功率/唯一推荐）。
        /// </summary>
        public CampaignCouncilView ConveneCouncil()
        {
            _lastCouncil = _service.ConveneCouncil(Session);
            return CampaignCouncilView.FromSet(_lastCouncil, Session.CurrentKnowledgeSnapshotId!.Value, _councilTuning);
        }

        /// <summary>
        /// 取最近一次军议对<b>当前</b>知识快照的展示模型（不重开）；侦察改变知识后其 <c>IsStale</c> 变真（不静默重算）。
        /// 尚未召开过军议则返回 null。
        /// </summary>
        public CampaignCouncilView? CurrentCouncilView()
            => _lastCouncil == null
                ? null
                : CampaignCouncilView.FromSet(_lastCouncil, Session.CurrentKnowledgeSnapshotId!.Value, _councilTuning);

        /// <summary>
        /// 敌情面板展示模型（GDD_007）：从玩家阵营知识只读投影派生（结构上无真值，反全知）。
        /// 时效阈值取自场景 <see cref="IntelConfig.TtlSegments"/>（与情报评估同源，勿另立常量）。未启用情报时返回空面板。
        /// </summary>
        public CampaignEnemyIntelPanelView EnemyIntel()
        {
            if (!Session.HasIntel) return CampaignEnemyIntelPanelView.Empty;
            return CampaignEnemyIntelPanelView.FromProjection(
                Session.PlayerKnowledge!, Session.CurrentTime,
                _scenario.StartConfig.IntelConfig!.TtlSegments, Session.PendingScouts);
        }

        /// <summary>
        /// 派出侦察（GDD_007 派出→在途→返报，<b>非即时</b>）：记一支在途侦察兵，约 <see cref="PlayableCampaign.ScoutLeadSegments"/>
        /// 时段后返报——须「推进时段」到返报时刻，敌情数字才出现。返回命令结果（校验失败稳定错误码、零写入）。
        /// </summary>
        public CampaignCommandResult ScoutEnemy()
            => _service.DispatchScout(Session, PlayableCampaign.EnemyArmy, IntelSource.Scouting, _scenario.ScoutLeadSegments);

        // --- 战役主循环（epic-028 story-004 / TR-ux-001/005 / ADR-0002/0009）。所有操作经服务命令，UI 只读投影。---

        /// <summary>当前回合数（1 起；用于新手引导前 N 回合判定，story-005）。</summary>
        public int Round => Session.CurrentTime.Day + 1;

        /// <summary>当前相位 + 该相位合法可做动作集（AC-5：任一相位都看得到下一步能做什么）。</summary>
        public HudPhaseView Phase() => HudPhaseView.ForSession(Session);

        /// <summary>治理面板（多维账本 + 三动作因果说明）。</summary>
        public GovernanceActionView Governance() => GovernanceActionView.FromSession(Session);

        /// <summary>下令征用军粮（GDD_004 派人处理→需时见效）：校验后记为在办，约 1 日后见效；超可分配量稳定错误码。</summary>
        public CampaignCommandResult Requisition(long amount)
            => _service.DispatchRequisition(Session, amount, _scenario.RequisitionLeadSegments);

        /// <summary>下令修工事（GDD_004）：工事已满稳定错误码，否则记为在办，约 1 日后见效。</summary>
        public CampaignCommandResult Repair()
            => _service.DispatchRepair(Session, _scenario.RepairLeadSegments);

        /// <summary>下令安抚民心（GDD_004）：记为在办，约半日后见效。</summary>
        public CampaignCommandResult Appease()
            => _service.DispatchAppease(Session, _scenario.AppeaseLeadSegments);

        /// <summary>备战面板（草稿 vs 已提交视觉区分）。</summary>
        public PrepPanelView Prep() => PrepPanelView.FromSession(Session);

        /// <summary>加入一条设伏草稿命令（GDD_009；只改草稿不改权威态）。</summary>
        public CampaignCommandResult AddAmbushOrder() => _service.AddPlanOrder(Session, _scenario.AmbushPlan());

        /// <summary>移除一条草稿命令（只改草稿）。</summary>
        public CampaignCommandResult RemoveOrder(string orderId) => _service.RemovePlanOrder(Session, new OrderId(orderId));

        /// <summary>提交计划（原子承诺，不可反悔）；返回是否成功。</summary>
        public bool SubmitPlan() => _service.SubmitPlan(Session).Committed;

        /// <summary>兵法条件进度（战中相位显示；每链已满足/未满足 + 还差 N 条，非按钮）。</summary>
        public BattleConditionProgressView BattleConditionProgress()
            => BattleConditionProgressView.Build(_scenario.TacticChains, Session.BattleConditions);

        /// <summary>假退伏击三条件（脚本战斗满足；与 console harness 同源，兵法=条件组合非按钮）。</summary>
        private static readonly TacticCondition[] FeintAmbushConditions =
        {
            TacticCondition.ControlledRetreatKeptFormation,
            TacticCondition.EnemyPursued,
            TacticCondition.AmbushSurprise,
        };

        /// <summary>
        /// 开战（story-004 复用既有脚本战斗，替换 story-002 演示按钮）：以<b>已提交计划</b>为可执行初始条件，
        /// 建立战斗 + 解析一阶段 + 标记假退伏击条件（进入战中相位，条件进度可见）。确定性种子/夹具。
        /// 无已提交计划 → 稳定错误码；幂等：已开战则跳过。
        /// </summary>
        public CampaignCommandResult StartBattle()
        {
            if (Session.CommittedPlan == null)
                return CampaignCommandResult.Failure(CampaignErrorCode.PreparationDisabled, "开战需先提交备战计划。");
            if (!Session.HasBattle)
            {
                CampaignCommandResult started = _service.StartBattle(
                    Session, _scenario.Units(), _scenario.BattleConfig, _scenario.BattleSeed, _scenario.TacticChains);
                if (!started.Applied) return started;
                _service.ResolveBattlePhase(Session, ScriptedOrders());
                foreach (TacticCondition c in FeintAmbushConditions) _service.MarkTacticCondition(Session, c);
            }
            return CampaignCommandResult.Success();
        }

        /// <summary>
        /// 结算战果（story-004）：识别涌现兵法 + 结算胜局后果（原子写回）+ 构造复盘展示模型（进入战后相位）。
        /// 要求已开战。战役继续——败/撤退亦有续局（本脚本战胜局）。
        /// </summary>
        public BattleReviewView ResolveOutcome()
        {
            if (!Session.HasBattle) throw new InvalidOperationException("尚未开战，无战果可结算。");
            IReadOnlyList<RecognizedTactic> tactics = _service.RecognizeTactics(Session);
            var context = new OutcomeContext(PlayableCampaign.Player, PlayableCampaign.Fanshui);
            OutcomeContinuation continuation = _service.ResolveBattleOutcome(
                Session, OutcomeBranch.Victory, context, _scenario.OutcomeConfig);
            CareerGain? gain = _scenario.Ladder.GainFor(CareerGainSource.CombatVictory);
            CareerState career = Session.Career.Career;
            return BattleReviewView.From(
                OutcomeBranch.Victory, continuation.Consequences.Changes, tactics, continuation.Options,
                gain, career.Merit, career.Renown, BattleReviewTuning.Default);
        }

        private static BattleOrder[] ScriptedOrders() => new[]
        {
            new BattleOrder(0, PlayableCampaign.PlayerUnit, BattleOrderType.Engage, targetUnit: PlayableCampaign.EnemyUnit),
        };

        // --- 出征攻城入口（GDD_019 v2 / ADR-0010/0011）：选目标 + 授权门 + 六维组装 + 发起 + 占城归属。---
        // 出征准备草稿为发起前临时态（ADR-0011 D7），不入存档；UI 只经此接口，权威结算在 CampaignSessionService。

        private readonly OffensiveSetupService _offensiveDerive = new OffensiveSetupService();

        /// <summary>当前出征计划草稿（null=尚未选目标开始组装）。</summary>
        public OffensivePlan? CurrentOffensivePlan => _offensivePlan;
        private OffensivePlan? _offensivePlan;

        /// <summary>可选副将花名册（GDD_014 僚属；供 UI 挑选加为副将）。</summary>
        public IReadOnlyList<OffensiveGeneral> DeputyRoster => _scenario.DeputyRoster;

        /// <summary>请君主授权出征（GDD_019 R1）：把场景可攻目标登记为授权集（受命后目标门转 Authorized）。</summary>
        public void RequestOffensiveAuthorization()
            => _service.AuthorizeOffensive(Session, _scenario.OffensiveTargetCities);

        /// <summary>
        /// 出征选目标视图（GDD_019 §7 / R1/R2）：列场景目标城 + 各自授权门（反全知只读控制权投影）。
        /// 不可攻的也列出并说明原因（AC-5）。
        /// </summary>
        public OffensiveTargetsView OffensiveTargets()
        {
            var lines = new List<OffensiveTargetLine>();
            bool authorized = false;
            foreach (CityId city in _scenario.OffensiveTargetCities)
            {
                OffensiveGateResult gate = _service.CheckOffensiveTarget(Session, city, PlayableCampaign.Player);
                if (gate != OffensiveGateResult.NotAuthorized) authorized = true;
                lines.Add(new OffensiveTargetLine(city.Value, DisplayNames.Of(city.Value), gate));
            }
            // authorized 判据：授权集非空（任一目标不再是 NotAuthorized）。
            authorized = Session.OffensiveAuthorization.AuthorizedTargets.Count > 0;
            return new OffensiveTargetsView(lines, authorized);
        }

        /// <summary>开始组装出征计划（GDD_019 §4a）：以场景默认建草稿（主将=太守亲征、正面强攻、当前时段）。返回草稿供 UI 修改六维。</summary>
        public OffensivePlan BeginOffensive(CityId target)
        {
            _offensivePlan = new OffensivePlan(
                target, _scenario.LeadGeneral, defaultMuster: 400, defaultSupply: 200, segment: Session.CurrentTime.Segment);
            return _offensivePlan;
        }

        /// <summary>以场景首个可攻目标开始组装（Unity 壳便捷入口，等价 BeginOffensive(首目标)）。</summary>
        public OffensivePlan BeginOffensiveDefault() => BeginOffensive(_scenario.OffensiveTargetCities[0]);

        /// <summary>当前草稿的计划预览（GDD_019 R3 闭合因果可见性）：dry-run 派生战力/士气/成型条件 + 缺失提示，无胜率。未开始则抛。</summary>
        public OffensivePlanView PreviewOffensive()
        {
            if (_offensivePlan == null) throw new InvalidOperationException("尚未开始组装出征（先 BeginOffensive）。");
            bool scouted = TargetScouted();
            OffensivePreparation prep = _offensivePlan.Build(_scenario.TerrainOf(_offensivePlan.Target), scouted);
            OffensiveForce preview = _offensiveDerive.Derive(prep, _scenario.OffensiveSetup);
            return OffensivePlanView.FromPlan(_offensivePlan, preview, scouted);
        }

        private ZoneBattleRuntime? _offensiveBattle;
        private CityId _offensiveTarget;

        /// <summary>
        /// 发起出征（GDD_019 + GDD_021 端到端）：授权门通过 → <b>进入区域战斗</b>（多回合排兵布阵，替换一击结算）；
        /// 被门拒则即时返回拒绝。战斗由 <see cref="OffensiveBattleResolveRound"/> 等推进，终局后经 <see cref="ConcludeOffensive"/>
        /// 结算占城归属 C。未开始组装则抛。
        /// </summary>
        public OffensiveResultView LaunchOffensive()
        {
            if (_offensivePlan == null) throw new InvalidOperationException("尚未开始组装出征（先 BeginOffensive）。");
            CityId target = _offensivePlan.Target;
            OffensiveGateResult gate = _service.CheckOffensiveTarget(Session, target, PlayableCampaign.Player);
            if (gate != OffensiveGateResult.Authorized)
                return OffensiveResultView.FromResult(OffensiveResult.Rejected(gate));

            bool scouted = TargetScouted();
            OffensivePreparation prep = _offensivePlan.Build(_scenario.TerrainOf(target), scouted);
            FixedPoint morale = _offensiveDerive.Derive(prep, _scenario.OffensiveSetup).Morale;
            int garrison = _scenario.DefenseOf(target).Garrison;
            _offensiveBattle = ZoneBattleRuntime.FromOffensive(prep, morale, garrison, _scenario.OffensiveSeed);
            _offensiveTarget = target;
            return OffensiveResultView.Started();
        }

        /// <summary>出征区域战斗进行中（未分胜负）。</summary>
        public bool HasOffensiveBattle => _offensiveBattle != null && !_offensiveBattle.IsOver;
        /// <summary>出征区域战斗已分胜负（待 ConcludeOffensive 结算后果）。</summary>
        public bool OffensiveBattleOver => _offensiveBattle != null && _offensiveBattle.IsOver;

        /// <summary>出征战斗当前投影（各区态势 + 涌现 + 排兵布阵选项）。未发起则抛。</summary>
        public ZoneBattleView OffensiveBattleView() => Battle().View();
        /// <summary>战中调动己方支队到相邻区（排兵布阵）。</summary>
        public ZoneCommandResult OffensiveBattleMove(string detachmentId, string zoneId) => Battle().MoveDetachment(detachmentId, zoneId);
        /// <summary>战中改己方支队姿态。</summary>
        public ZoneCommandResult OffensiveBattleSetPosture(string detachmentId, Posture posture) => Battle().SetPosture(detachmentId, posture);
        /// <summary>推进出征战斗一回合（敌AI + 结算），返回战后投影。</summary>
        public ZoneBattleView OffensiveBattleResolveRound() => Battle().ResolveRound();
        /// <summary>挂 AI 代打出征至终局（不结算，供场景展示后再由玩家点结算），返回终局投影。</summary>
        public ZoneBattleView OffensiveBattleAutoResolve() => Battle().AutoResolve();

        private ZoneBattleRuntime Battle() => _offensiveBattle ?? throw new InvalidOperationException("尚未发起出征战斗。");

        /// <summary>
        /// 战斗终局后结算出征后果（权威）：破城 → 占城归属 C（经 <see cref="CampaignSessionService.ResolveConquest"/>：
        /// 控制权变更 + 记功 + 自立倾向）；败/超时 → 退兵可继续。清空战斗与草稿。战斗未结束则抛。
        /// </summary>
        public OffensiveResultView ConcludeOffensive()
        {
            if (_offensiveBattle == null || !_offensiveBattle.IsOver)
                throw new InvalidOperationException("战斗尚未结束，不能结算出征后果。");

            OffensiveResultView view;
            if (_offensiveBattle.Outcome == ZoneBattleOutcome.AttackerVictory)
            {
                ConquestResult conquest = _service.ResolveConquest(
                    Session, _offensiveTarget, _scenario.ConqueredGarrison, PlayableCampaign.Player, PlayableCampaign.LordFaction,
                    FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero,
                    _scenario.OffensiveSeed, _scenario.Occupation, _scenario.Ladder, CareerGainSource.MajorBattleVictory);
                if (conquest.Verdict == OwnershipVerdict.GrantToPlayer)   // 归玩家直辖 → 入多城战区（M12）
                    _theater = _theaterService.HoldConqueredCity(_theater, _offensiveTarget);
                view = OffensiveResultView.Victorious(conquest);
            }
            else
            {
                view = OffensiveResultView.Defeated();
            }

            _offensiveBattle = null;
            _offensivePlan = null;
            return view;
        }

        /// <summary>挂 AI 代打出征至终局并结算后果（玩家可选亲自打或代打；代打不保证赢，胜负由六维准备/对阵定）。</summary>
        public OffensiveResultView AutoResolveOffensive()
        {
            Battle().AutoResolve();
            return ConcludeOffensive();
        }

        // --- 守城区域防御战（GDD_021 R7 攻守统一：玩家=守方，攻方=敌AI）。替换脚本守城的可玩战斗。---

        private ZoneBattleRuntime? _defenseBattle;

        /// <summary>守城区域防御战进行中。</summary>
        public bool HasDefenseBattle => _defenseBattle != null && !_defenseBattle.IsOver;
        /// <summary>守城已分胜负。</summary>
        public bool DefenseBattleOver => _defenseBattle != null && _defenseBattle.IsOver;
        /// <summary>守城是否守住（守方胜=退敌）。</summary>
        public bool DefenseHeld => _defenseBattle != null && _defenseBattle.IsOver
            && _defenseBattle.Outcome == ZoneBattleOutcome.DefenderVictory;

        /// <summary>发起守城区域防御战：以守军分区布防，敌军来攻（敌AI驱动攻方）。返回初始战斗投影。</summary>
        public ZoneBattleView StartDefenseBattle()
        {
            var field = BattleField.Default();
            var planner = new OffensiveDeploymentPlanner();
            FixedPoint morale = FixedPoint.FromFraction(7, 10);
            var dets = new List<Detachment>(planner.PlanDefender(
                new SiegeDefense(_scenario.DefenseGarrison, FixedPoint.FromFraction(12, 10)), morale, field));
            dets.Add(new Detachment(new DetachmentId("enemy-assault"), BattleSide.Attacker, null,
                TroopComposition.AllInfantry(_scenario.EnemyAssaultForce), _scenario.EnemyAssaultForce,
                morale, FixedPoint.FromFraction(2, 10), Posture.Assault, BattleField.Front));
            ZoneBattleState start = new ZoneBattleService().Start(field, dets, BattleSide.Defender, 6, _scenario.OffensiveSeed);
            _defenseBattle = new ZoneBattleRuntime(start, ZoneBattleContext.Default);
            return _defenseBattle.View();
        }

        /// <summary>守城战当前投影。</summary>
        public ZoneBattleView DefenseBattleView() => Defense().View();
        /// <summary>守城战中调动己方守军到相邻区。</summary>
        public ZoneCommandResult DefenseBattleMove(string detachmentId, string zoneId) => Defense().MoveDetachment(detachmentId, zoneId);
        /// <summary>守城战中改己方守军姿态。</summary>
        public ZoneCommandResult DefenseBattleSetPosture(string detachmentId, Posture posture) => Defense().SetPosture(detachmentId, posture);
        /// <summary>推进守城战一回合（敌AI + 结算），返回战后投影。</summary>
        public ZoneBattleView DefenseBattleResolveRound() => Defense().ResolveRound();
        /// <summary>挂 AI 代打守城至终局（不结算），返回终局投影。</summary>
        public ZoneBattleView DefenseBattleAutoResolve() => Defense().AutoResolve();

        /// <summary>挂 AI 代打守城至终局；返回是否守住（代打不保证守成，胜负由守备/对阵定）。</summary>
        public bool AutoResolveDefense()
        {
            Defense().AutoResolve();
            return DefenseHeld;
        }

        private ZoneBattleRuntime Defense() => _defenseBattle ?? throw new InvalidOperationException("尚未发起守城战。");

        // --- 多城战区（GDD_022 / M12）：占城 C 归玩家的城入战区；委任下属打理；掌管范围随官阶；反全知报告 ---

        private readonly TheaterService _theaterService = new TheaterService();
        private TheaterState _theater = TheaterState.Empty;

        /// <summary>当前多城战区态（直辖城 + 委任）。</summary>
        public TheaterState Theater => _theater;

        /// <summary>委任某直辖城给下属打理（须已持有）。</summary>
        public TheaterCommandResult DelegateCity(ThreeKingdom.Domain.City.CityId city, ThreeKingdom.Domain.Characters.CharacterId governor)
        {
            TheaterCommandResult r = _theaterService.Delegate(_theater, city, governor);
            if (r.Applied) _theater = r.State;
            return r;
        }

        /// <summary>收回某城亲管（受官阶亲管范围约束——取玩家当前官阶）。</summary>
        public TheaterCommandResult SelfGovernCity(ThreeKingdom.Domain.City.CityId city)
        {
            int rank = (int)Session.Career.Career.Rank;
            TheaterCommandResult r = _theaterService.SelfGovern(_theater, city, rank, SpanOfControlConfig.Default);
            if (r.Applied) _theater = r.State;
            return r;
        }

        /// <summary>战区报告（亲管城即时、委任城下属汇报·反全知）。</summary>
        public IReadOnlyList<TheaterCityReport> TheaterReports(TheaterResources reported)
            => new TheaterReportService().Build(_theater, reported);

        // --- 人才招揽（GDD_020）：出现随历史 · 知晓靠情报（反全知）· 入伙靠条件+种子判定 · 喂给战斗/生涯 ---

        private readonly TalentService _talentService = new TalentService();
        private ThreeKingdom.Domain.Talent.TalentState _talent = ThreeKingdom.Domain.Talent.TalentState.Empty;

        /// <summary>玩家可见人才（已登场 ∩ 已知晓；反全知，未知晓者不入）。</summary>
        public IReadOnlyList<ThreeKingdom.Domain.Talent.TalentProfile> VisibleTalents()
            => _talentService.Visible(_scenario.TalentRoster, _talent, Session.CurrentTime);

        /// <summary>经渠道知晓某人才（侦察/军师/部曲人脉/历史事件）→ 进入视野。</summary>
        public void RevealTalent(ThreeKingdom.Domain.Talent.TalentId id, ThreeKingdom.Domain.Talent.TalentChannel channel)
            => _talent = _talentService.Reveal(_talent, id, channel);

        /// <summary>发起招揽（须已登场+已知晓）：条件+种子判定出仕与否；出仕则入伙（返回为将）。返回尝试结果。</summary>
        public TalentRecruitAttempt RecruitTalent(ThreeKingdom.Domain.Talent.TalentId id, ThreeKingdom.Domain.Talent.RecruitmentOffer offer)
        {
            TalentRecruitAttempt r = _talentService.AttemptRecruit(
                _scenario.TalentRoster, _talent, id, Session.CurrentTime, offer,
                _scenario.TalentSeed, PlayableCampaign.Player, _scenario.TalentRecruit);
            if (r.Valid) _talent = r.State;
            return r;
        }

        /// <summary>已入伙某人才。</summary>
        public bool HasRecruited(ThreeKingdom.Domain.Talent.TalentId id) => _talent.IsRecruited(id);

        /// <summary>目标是否已侦察（反全知：有非过时敌情估计 → 可得突袭类条件、免情报盲区折扣）。</summary>
        private bool TargetScouted()
        {
            if (!Session.HasIntel) return false;
            foreach (CampaignEnemyIntelView e in EnemyIntel().Entries)
                if (!e.IsStale) return true;
            return false;
        }

        /// <summary>默认槽是否有存档（主菜单「继续」可用性）。</summary>
        public bool HasSave() => _medium.Exists(_slot);

        /// <summary>
        /// 原子存档当前会话到槽（统一信封 <see cref="CampaignSessionService.CaptureSnapshot"/>）；
        /// 先写临时槽再原子改名——任一步失败返回 false 且正式槽保留上一份有效存档（ADR-0005 guardrail）。
        /// </summary>
        public bool Save()
        {
            string content = _service.CaptureSnapshot(Session);
            string tmp = _slot + ".tmp";
            try
            {
                _medium.Write(tmp, content);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            try
            {
                _medium.Move(tmp, _slot);
            }
            catch (Exception)
            {
                TryDelete(tmp);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取槽恢复会话（统一信封 <see cref="CampaignSessionService.Restore"/>，卫星配置由场景源提供——数据驱动）。
        /// 成功切换当前会话返回 true；失败（无存档 / 版本、指纹、格式不符）返回 false 与原因，<b>当前会话不变</b>（不部分载入）。
        /// </summary>
        public bool Load(out string reason)
        {
            string? text = _medium.Read(_slot);
            if (text == null)
            {
                reason = "槽内无存档。";
                return false;
            }

            CampaignStartConfig config = _scenario.StartConfig;
            try
            {
                CampaignSession restored = _service.Restore(
                    text, config.Fingerprint,
                    settlementConfig: config.SettlementConfig,
                    governanceConfig: config.GovernanceConfig,
                    populationPressure: config.PopulationPressure,
                    intelConfig: config.IntelConfig,
                    councilSetup: config.CouncilSetup,
                    prepConfig: config.PreparationConfig,
                    reachableRegions: config.ReachableRegions,
                    authorizedOrders: config.AuthorizedOrders,
                    battleConfig: _scenario.BattleConfig,
                    tacticChains: _scenario.TacticChains);
                _session = restored;
                _daysCrossedLastAdvance = 0;
                reason = string.Empty;
                return true;
            }
            catch (SaveFormatException ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private CampaignSession StartNew()
        {
            CampaignStartResult result = _service.StartCampaign(_scenario.StartConfig);
            if (!result.Started)
                throw new InvalidOperationException("场景开局失败（配置源已验证，此处失败属编程错误）：" + result.Error + " " + result.Detail);
            return result.Session!;
        }

        private void TryDelete(string tmp)
        {
            try { _medium.Delete(tmp); } catch { /* 清理失败不掩盖原始错误 */ }
        }
    }
}
