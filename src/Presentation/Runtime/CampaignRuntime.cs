using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
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
                Session.PlayerKnowledge!, Session.CurrentTime, _scenario.StartConfig.IntelConfig!.TtlSegments);
        }

        /// <summary>
        /// 【story-003 最小重定向·story-004 换延迟派出循环】即时侦察场景登记的敌军主力，并入玩家知识 →
        /// 敌情面板更新、当前知识快照改变（已召开军议随之被标过时）。返回命令结果（校验失败稳定错误码、零写入）。
        /// </summary>
        public CampaignCommandResult ScoutEnemy()
            => _service.Scout(Session, PlayableCampaign.EnemyArmy, IntelSource.Scouting);

        // --- 战役主循环（epic-028 story-004 / TR-ux-001/005 / ADR-0002/0009）。所有操作经服务命令，UI 只读投影。---

        /// <summary>当前回合数（1 起；用于新手引导前 N 回合判定，story-005）。</summary>
        public int Round => Session.CurrentTime.Day + 1;

        /// <summary>当前相位 + 该相位合法可做动作集（AC-5：任一相位都看得到下一步能做什么）。</summary>
        public HudPhaseView Phase() => HudPhaseView.ForSession(Session);

        /// <summary>治理面板（多维账本 + 三动作因果说明）。</summary>
        public GovernanceActionView Governance() => GovernanceActionView.FromSession(Session);

        /// <summary>征用军粮（GDD_004）：超可分配量返回稳定错误码、账本不变。</summary>
        public CampaignCommandResult Requisition(long amount) => _service.RequisitionFood(Session, amount);

        /// <summary>修工事（GDD_004）：工事已满返回稳定错误码。</summary>
        public CampaignCommandResult Repair() => _service.RepairFortification(Session);

        /// <summary>安抚民心（GDD_004）。</summary>
        public CampaignCommandResult Appease() => _service.Appease(Session);

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
