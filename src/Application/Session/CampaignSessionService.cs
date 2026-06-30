using System;
using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// CampaignSession 的 Presentation 可调用入口（ADR-0009）。**只编排**：校验命令时机/入参，
    /// 调既有 Domain/Application 服务装配会话；<b>不计算玩法公式、不直接写归属/势力存续</b>（R-5）。
    /// <para>
    /// 本 story（001）实现 <see cref="StartCampaign"/>（配置驱动开局）。Advance / Execute / CaptureSnapshot /
    /// Restore 由后续 story 叠加（ADR-0009 §Key Interfaces）。
    /// </para>
    /// </summary>
    public sealed class CampaignSessionService
    {
        /// <summary>
        /// 配置驱动开局：从 <see cref="CampaignStartConfig"/> 装配初始会话。
        /// 登记开局城池归属经 GDD_004 权威；生涯经 epic-011 编排；世界经 epic-012 投影订阅 004。
        /// 失败返回稳定错误码、无部分写入。
        /// </summary>
        public CampaignStartResult StartCampaign(CampaignStartConfig config)
        {
            if (config is null)
                return CampaignStartResult.Failure(CampaignErrorCode.NullConfig, "开局配置为空。");

            // GDD_004 城池控制权唯一权威（ADR-0008）。装配层只经它发起/读，不直接写归属。
            var authority = new CityControlAuthority();

            // 生涯：epic-011 编排（登记开局城归属到 authority + 绑太守生涯）。
            var governor = new GovernorCampaignService(authority);
            CareerSnapshot career = governor.BeginGovernorStart(config.GovernorSeed);

            // 世界：epic-012 权威态 + 归属只读投影订阅 004。
            var world = new WorldState(
                config.StartTime, config.InitialFactions, config.InitialCities,
                triggeredEvents: Array.Empty<string>(), divergedEvents: Array.Empty<string>());
            var worldProjection = new WorldCityProjection(world, authority);

            var session = new CampaignSession(
                id: config.ScenarioConfigId,           // S1：以场景 id 作会话 id（多会话/槽位属后续）
                scenarioConfigId: config.ScenarioConfigId,
                fingerprint: config.Fingerprint,
                career: career,
                worldProjection: worldProjection,
                control: authority,
                cityEconomy: config.CityEconomy,       // M03：城市治理态（可选；null 则不启用治理循环）
                settlementConfig: config.SettlementConfig,
                populationPressure: config.PopulationPressure,
                logisticsHolding: config.InitialLogisticsHolding,
                governanceConfig: config.GovernanceConfig,
                truth: config.WorldTruth,             // M04：情报态（可选；null 则不启用情报循环）
                playerIntel: config.PlayerIntel,
                intelConfig: config.IntelConfig,
                council: config.CouncilSetup,
                pool: config.ResourcePool,            // M05：准备态（可选；null 则不启用准备循环）
                prepConfig: config.PreparationConfig,
                reachableRegions: config.ReachableRegions,
                authorizedOrders: config.AuthorizedOrders);

            return CampaignStartResult.Success(session);
        }

        /// <summary>
        /// 日界推进（ADR-0009 §Day Boundary Order / TR-session-001）。按 `systems-index.md` 全局结算顺序编排：
        /// 时间 → 环境(002) → 补给(012) → 城市/控制权(004) → 状态事件(011) → 历史世界模型(015) → 生涯(014) → 敌方AI(016)。
        /// <para>
        /// 本 story（002）实现 Meta 层片段（时间 + 世界模型 015 确定性推进）；基础层（002/012/004/011）随
        /// 治理循环（M03）接入会话时叠加于本顺序之内。015/014/016 只读已结算值、不回读未结算（破环见 systems-index）。
        /// 纯时间推进下生涯/敌方AI 为 no-op（生涯变更经后果写回 story-003，敌方 AI 属 epic-021）。
        /// </para>
        /// </summary>
        public CampaignSession Advance(CampaignSession session, int segments)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (segments < 0) throw new ArgumentOutOfRangeException(nameof(segments), "推进时段数不可为负。");

            int dayBefore = session.CurrentTime.Day;

            // 时间 → 历史世界模型（015）：确定性推进，世界读已结算态（ADR-0004）。
            session.AdvanceWorld(segments);

            // 城市/控制权（004 / M03）：每跨一个日界结算一次城市日结（GDD_004「日界结算」）。
            // 装配层只编排——复用既有 CityDaySettlementService（Domain 纯函数），不在此重写公式。
            if (session.HasCityGovernance)
            {
                int dayAfter = session.CurrentTime.Day;
                for (int d = dayBefore; d < dayAfter; d++)
                {
                    CitySettlementResult result = _citySettlement.Settle(
                        session.CityEconomy!, session.LogisticsHolding, session.SettlementConfig!, session.PopulationPressure);
                    session.ApplyCitySettlement(result.EndState, result.EndLogisticsHolding);
                }
            }

            return session;
        }

        private readonly CityDaySettlementService _citySettlement = new CityDaySettlementService();

        // --- 城市治理命令（M03 / TR-city-003）。经命令路径改城市态；前置校验失败 → 稳定错误码、零部分写入。---

        /// <summary>
        /// 征用军粮（GDD_004 §Formula 5）：设置已承诺保留量（日界移交后勤）+ 即时扣城市民心。
        /// 校验 <c>available ≥ amount</c>，否则 <see cref="CampaignErrorCode.InsufficientStock"/> 拒绝、零写入。
        /// </summary>
        public CampaignCommandResult RequisitionFood(CampaignSession session, long amount)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");
            if (amount < 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, $"征用量不可为负：{amount}。");

            CityEconomyState city = session.CityEconomy!;
            if (amount > city.Available)
                return CampaignCommandResult.Failure(CampaignErrorCode.InsufficientStock,
                    $"可分配量不足：available={city.Available}，请求={amount}。");

            CityGovernanceConfig gov = session.GovernanceConfig!;
            int moralePenalty = (gov.RequisitionMoralePenalty * FixedPoint.FromInt(checked((int)amount))).RoundToInt();
            int newMorale = ClampInt(checked(city.CivMorale - moralePenalty), 0, session.SettlementConfig!.CivMoraleMax);

            session.SetCityEconomy(city.With(reserved: city.Reserved + amount, civMorale: newMorale));
            return CampaignCommandResult.Success();
        }

        /// <summary>
        /// 修工事（GDD_004 §Formula 6）：即时投入修复 <c>min(上限余量, FortRepairPerOrder)</c>。
        /// 工事已满 → <see cref="CampaignErrorCode.FortificationFull"/> 拒绝（多余投入不转其他资源，GDD §Edge Cases）。
        /// </summary>
        public CampaignCommandResult RepairFortification(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");

            CityEconomyState city = session.CityEconomy!;
            int room = city.FortificationMax - city.FortificationCurrent;
            if (room <= 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.FortificationFull, "工事已满，无可修复余量。");

            int repair = Math.Min(room, session.GovernanceConfig!.FortRepairPerOrder);
            session.SetCityEconomy(city.With(fortificationCurrent: city.FortificationCurrent + repair));
            return CampaignCommandResult.Success();
        }

        /// <summary>
        /// 安抚（GDD_004 §Formula 4 民心有源有汇）：即时提升城市民心 <c>AppeaseMoraleGain</c>，夹至 CivMoraleMax。
        /// </summary>
        public CampaignCommandResult Appease(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");

            CityEconomyState city = session.CityEconomy!;
            int newMorale = ClampInt(checked(city.CivMorale + session.GovernanceConfig!.AppeaseMoraleGain), 0, session.SettlementConfig!.CivMoraleMax);
            session.SetCityEconomy(city.With(civMorale: newMorale));
            return CampaignCommandResult.Success();
        }

        private static int ClampInt(int value, int min, int max)
            => value < min ? min : (value > max ? max : value);

        // --- 情报命令（M04 / TR-intel-002）。侦察经会话路径产生报告并入玩家知识；前置校验失败稳定错误码、零写入。---

        private readonly IntelService _intel = new IntelService();

        /// <summary>
        /// 侦察（GDD_007）：对指定对象以指定方法侦察 → 经 <see cref="IntelService"/> 解析观察/报告 → 并入玩家阵营知识。
        /// "侦察全部"非法——须指定登记于世界真值的具体对象，否则 <see cref="CampaignErrorCode.UnknownIntelSubject"/> 拒绝、零写入。
        /// </summary>
        public CampaignCommandResult Scout(CampaignSession session, IntelSubjectId subject, IntelSource method)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasIntel)
                return CampaignCommandResult.Failure(CampaignErrorCode.IntelDisabled, "会话未启用情报。");
            if (!session.Truth!.Has(subject))
                return CampaignCommandResult.Failure(CampaignErrorCode.UnknownIntelSubject,
                    $"侦察对象未登记或非法：{subject}。");

            FactionId observer = session.PlayerIntel!.Faction;
            Observation observation = _intel.Observe(session.Truth!, subject, observer, session.CurrentTime);
            IntelReport report = _intel.ToReport(observation, observer, method);
            session.PlayerIntel!.ApplyReport(report);
            return CampaignCommandResult.Success();
        }

        private readonly WarCouncilService _council = new WarCouncilService();

        /// <summary>
        /// 召开军议（GDD_008 / TR-council-001/002）：读会话当前知识快照 → 军师输出条件化建议集（并列、无最优解）。
        /// 建议绑定召开时 <see cref="CampaignSession.CurrentKnowledgeSnapshotId"/>；之后侦察改变知识 →
        /// 该建议 <see cref="CouncilAdviceSet.IsStaleAgainst"/> 为真（不静默更新）。军师只条件化建议（不给成功率/唯一推荐/自动命令）。
        /// </summary>
        public CouncilAdviceSet ConveneCouncil(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasIntel || session.Council == null)
                throw new InvalidOperationException("会话未启用军议（须同时启用情报与军议配置）。");

            SessionCouncilSetup setup = session.Council;
            IntelProjection knowledge = session.PlayerKnowledge!;   // 只读阵营知识，绝不触真值（反全知）
            var confidences = new Dictionary<IntelSubjectId, FixedPoint>();
            foreach (IntelKnowledgeEntry e in knowledge.Entries)
                confidences[e.Subject] = setup.KnownClaimConfidence;

            return _council.Convene(
                session.CurrentKnowledgeSnapshotId!.Value, knowledge, confidences,
                setup.Advisor, setup.Templates, setup.Config);
        }

        // --- 战役准备命令（M05 / TR-prep-001/002）。草稿编辑不改权威态；提交经 PlanCommitService 原子。---

        private readonly PlanCommitService _planCommit = new PlanCommitService();

        /// <summary>
        /// 加入一条计划草稿命令（GDD_009）。<b>只改草稿，不改权威 state</b>（资源池/承诺计划不变，TR-prep-001）。
        /// 同 id 已存在 → <see cref="CampaignErrorCode.InvalidAmount"/>（复用为"非法命令"语义）拒绝。
        /// </summary>
        public CampaignCommandResult AddPlanOrder(CampaignSession session, PreparedOrder order)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (order is null) throw new ArgumentNullException(nameof(order));
            if (!session.HasPreparation)
                return CampaignCommandResult.Failure(CampaignErrorCode.PreparationDisabled, "会话未启用战役准备。");

            foreach (PreparedOrder existing in session.Draft!.Orders)
                if (existing.Id == order.Id)
                    return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, $"命令 id 已存在：{order.Id}。");

            session.Draft!.AddOrder(order);
            return CampaignCommandResult.Success();
        }

        /// <summary>移除一条计划草稿命令（只改草稿）。不存在 → 返回失败码但不抛（可继续）。</summary>
        public CampaignCommandResult RemovePlanOrder(CampaignSession session, OrderId orderId)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasPreparation)
                return CampaignCommandResult.Failure(CampaignErrorCode.PreparationDisabled, "会话未启用战役准备。");

            bool removed = session.Draft!.RemoveOrder(orderId);
            return removed
                ? CampaignCommandResult.Success()
                : CampaignCommandResult.Failure(CampaignErrorCode.UnknownIntelSubject, $"草稿无此命令：{orderId}。");
        }

        /// <summary>
        /// 提交计划（GDD_009 / TR-prep-001/002）：经 <see cref="PlanCommitService.Submit"/> 校验——
        /// 全部通过才<b>原子</b>生成 <see cref="CommittedPlan"/> + 一次性扣减资源池（全有或全无）；
        /// 任一硬冲突（占用/资源/可达/权限/循环依赖 DAG）则全单拒绝、资源池不变、无部分写入。
        /// </summary>
        public SubmitPlanResult SubmitPlan(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasPreparation)
                throw new InvalidOperationException("会话未启用战役准备。");

            SubmitPlanResult result = _planCommit.Submit(
                session.Draft!, session.Pool!, session.ReachableRegions, session.AuthorizedOrders, session.PrepConfig!);

            if (result.Committed)
                session.ApplyCommittedPlan(result.Plan!, result.ResultingPool);   // 成功才写回（失败资源池不变）

            return result;
        }

        // --- 战斗命令（M06 / TR-battle-001/002/003）。开战须有 CommittedPlan；阶段经 BattleResolver 原子。---

        private readonly BattleResolver _battleResolver = new BattleResolver();
        private readonly TacticRecognizer _tacticRecognizer = new TacticRecognizer();

        /// <summary>
        /// 开战（GDD_010）：以 M05 <see cref="CommittedPlan"/> 为可执行战役初始条件，建立战斗快照
        /// （玩家 + 确定性预设敌方单位）。无 CommittedPlan → <see cref="CampaignErrorCode.PreparationDisabled"/> 拒绝。
        /// 敌方为确定性预设（非智能 AI；智能 AI 属 M08/epic-021）。
        /// </summary>
        public CampaignCommandResult StartBattle(
            CampaignSession session, IReadOnlyList<BattleUnitState> units,
            BattleConfig config, ulong seed, TacticChainConfig tacticChains)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (units is null) throw new ArgumentNullException(nameof(units));
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (tacticChains is null) throw new ArgumentNullException(nameof(tacticChains));
            if (!session.HasPreparation || session.CommittedPlan == null)
                return CampaignCommandResult.Failure(CampaignErrorCode.PreparationDisabled,
                    "开战需先有已提交计划（可执行战役初始条件）。");

            var snapshot = new BattleSnapshot(units, new DetectionState(), session.Fingerprint.ToString());
            session.StartBattleState(snapshot, config, seed, tacticChains);
            return CampaignCommandResult.Success();
        }

        /// <summary>
        /// 解析一个战斗阶段（GDD_010 / TR-battle-001/003）：经 <see cref="BattleResolver.ResolvePhase"/> 稳定管线解析；
        /// 成功更新会话战斗快照，异常原子回滚（会话战斗态不变）。同快照+配置+种子+命令流 → 同 hash。
        /// </summary>
        public BattleResolution ResolveBattlePhase(CampaignSession session, IReadOnlyList<BattleOrder> orders)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (orders is null) throw new ArgumentNullException(nameof(orders));
            if (!session.HasBattle)
                throw new InvalidOperationException("会话未开战。");

            BattleResolution resolution = _battleResolver.ResolvePhase(
                session.Battle!, orders, session.BattleSeed, session.BattleConfig!);

            if (resolution.Committed)
                session.SetBattle(resolution.State);   // 成功才更新（回滚则战斗态不变）

            return resolution;
        }

        /// <summary>
        /// 标记一条战斗中满足的兵法条件（GDD_010）。条件由战斗命令/事件经确定性映射累积，
        /// 供 <see cref="RecognizeTactics"/> 事后识别——兵法是条件涌现，非无条件按钮。
        /// </summary>
        public void MarkTacticCondition(CampaignSession session, TacticCondition condition)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasBattle) throw new InvalidOperationException("会话未开战。");
            session.AddBattleCondition(condition);
        }

        /// <summary>
        /// 事后识别涌现兵法（GDD_010 / TR-battle-002）：对会话累积的满足条件集经 <see cref="TacticRecognizer"/> 打复盘标签。
        /// 仅当某链<b>全部</b>条件成立才识别（含 FeintAmbush 机动招式）；条件不全不识别（无无条件按钮）。
        /// </summary>
        public IReadOnlyList<RecognizedTactic> RecognizeTactics(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasBattle) throw new InvalidOperationException("会话未开战。");

            var context = new RetrospectiveContext(session.BattleConditions);
            return _tacticRecognizer.Recognize(context, session.TacticChains!);
        }

        // --- 后果与恢复命令（M07 / TR-outcome-001/002）。四分支变更集原子写回会话 + 续局选项。---

        private readonly FailureContinuationService _failureContinuation = new FailureContinuationService();

        /// <summary>
        /// 结算战果分支后果（GDD_010 §后果 / TR-outcome-001/002）：从会话城市态构造 <see cref="OutcomeWorld"/>，
        /// 经 <see cref="FailureContinuationService"/> 生成变更集 → 原子写回 → 给出四分支续局选项。
        /// 仅 <see cref="OutcomeWritebackResult.Committed"/> 时更新会话态（全有或全无）；败局必含 ≥1 合法可继续命令。
        /// reputation/relationship/vitality 在 OutcomeWorld 内计算暴露，<b>不</b>写回会话独立态（裁断，见 epic-020）。
        /// </summary>
        public OutcomeContinuation ResolveBattleOutcome(
            CampaignSession session, OutcomeBranch branch, OutcomeContext context, OutcomeConsequenceConfig config)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (!session.HasCityGovernance)
                throw new InvalidOperationException("后果写回需会话已启用城市治理（城市态为写回目标）。");

            OutcomeWorld world = OutcomeWorld.Empty.WithCity(session.CityEconomy!);
            OutcomeContinuation continuation = _failureContinuation.Resolve(world, branch, context, config);

            // 原子：仅写回成功才更新会话态（失败则会话城市态不变）。
            if (continuation.Writeback.Committed)
            {
                CityId cityId = session.CityEconomy!.Id;
                if (continuation.Writeback.ResultingWorld.HasCity(cityId))
                    session.SetCityEconomy(continuation.Writeback.ResultingWorld.GetCity(cityId));
                session.SetLastOutcome(branch, continuation.Options);
            }

            return continuation;
        }

        /// <summary>
        /// 按场景 id 从目录配置驱动开局（M01 / ADR-0003）。未知 id → <see cref="CampaignErrorCode.SessionNotFound"/>。
        /// 切换场景仅换 id、无代码改动。
        /// </summary>
        public CampaignStartResult StartCampaign(ScenarioCatalog catalog, string scenarioId)
        {
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            CampaignStartConfig? config = catalog.Find(scenarioId);
            if (config is null)
                return CampaignStartResult.Failure(CampaignErrorCode.SessionNotFound, $"场景不存在：{scenarioId}。");
            return StartCampaign(config);
        }

        /// <summary>开一个后果原子写回事务（ADR-0009 §R-6）。调用方暂存变更后 <see cref="ConsequenceTransaction.Commit"/>。</summary>
        public ConsequenceTransaction BeginConsequence(CampaignSession session)
            => new ConsequenceTransaction(session ?? throw new ArgumentNullException(nameof(session)));

        private readonly GovernorOutcomeService _governorOutcome = new GovernorOutcomeService();

        /// <summary>
        /// 守城开局事件后果原子写回（TR-session-002/004）。胜→功绩/信任（生涯）；
        /// 败→生涯转在野（合法可继续）+ 失城经 GDD_004 控制权变更。全程经 <see cref="ConsequenceTransaction"/> 原子提交。
        /// </summary>
        public CampaignCommandResult ResolveSiege(
            CampaignSession session, SiegeOutcome outcome, GovernorStartConfig config, SiegeContext context)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (config is null) throw new ArgumentNullException(nameof(config));

            ConsequenceTransaction tx = BeginConsequence(session);

            if (outcome == SiegeOutcome.Defended)
            {
                CareerCommandResult win = _governorOutcome.ResolveDefended(session.Career, config);
                if (!win.Applied)
                    return CampaignCommandResult.Failure(CampaignErrorCode.InvalidConfig, "守城胜结算失败：" + win.Detail);
                tx.StageCareer(win.Snapshot);   // 归属不变
            }
            else
            {
                CareerSnapshot wandering = _governorOutcome.ResolveFallen(session.Career);  // 罢官转在野
                tx.StageCareer(wandering)
                  .StageControlChange(context.City, context.EnemyFaction, context.EnemyGarrison, ChangeCause.SiegeDefenseLost);
            }

            return tx.Commit();
        }

        // --- 统一会话存档（ADR-0009 §R-1/R-7 / TR-session-003，复用 FIX-8 CampaignSaveCodec）---

        /// <summary>当前会话存档 schema 版本。</summary>
        public static readonly SaveVersion CampaignSaveVersion = new SaveVersion(1, 0);

        private readonly CampaignSaveCodec _saveCodec = new CampaignSaveCodec();
        private const string SnapshotMagic = "TKSESSION/1";
        private const string BodyMarker = "--BODY--";

        /// <summary>
        /// 捕获会话快照为确定性文本（ADR-0009 §R-1）。当前覆盖 生涯段 + 世界段（复用 CampaignSaveCodec）
        /// 与会话元数据；时间在世界段。RNG/情报/战役 checkpoint 段随对应模块接入会话后并入（R-1 段集合）。
        /// </summary>
        public string CaptureSnapshot(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            var career = new CareerSaveState(CampaignSaveVersion, session.Fingerprint, session.Career, rebellion: null, missions: LordMissionLog.Empty);
            var world = new WorldSaveState(CampaignSaveVersion, session.Fingerprint, session.World);
            var campaign = new CampaignSaveState(CampaignSaveVersion, session.Fingerprint, career, world);

            string body = _saveCodec.Serialize(campaign);
            string head = SnapshotMagic + "\nid\t" + session.Id + "\nscenario\t" + session.ScenarioConfigId + "\n";

            // 城市治理段（M03 / TR-city-005）：仅序列化城市**态**（配置数据驱动，按指纹由载入方提供，不入存档体）。
            if (session.HasCityGovernance)
            {
                CityEconomyState c = session.CityEconomy!;
                head += "city\t" + c.Id.Value + "\t" + c.Stock + "\t" + c.Reserved + "\t" + c.CivMorale
                      + "\t" + c.Security + "\t" + c.FortificationCurrent + "\t" + c.FortificationMax
                      + "\t" + session.LogisticsHolding + "\n";
            }

            // 情报段（M04 / TR-intel-003）：世界真值与玩家知识**分别序列化**（独立段，加载不交叉污染）。
            if (session.HasIntel)
            {
                head += "intel\t" + session.PlayerIntel!.Faction.Value + "\n";

                var truthRecords = new List<TruthRecord>(session.Truth!.Records);
                truthRecords.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
                foreach (TruthRecord r in truthRecords)
                    head += "truth\t" + r.Subject.Value + "\t" + r.ActualStrength + "\t" + r.Owner.Value + "\n";

                var entries = new List<IntelKnowledgeEntry>(session.PlayerIntel!.Project().Entries);
                entries.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
                foreach (IntelKnowledgeEntry e in entries)
                    head += "knowledge\t" + e.Subject.Value + "\t" + e.KnownStrength + "\t" + (int)e.Source
                          + "\t" + e.ObservedAt.Day + "\t" + (int)e.ObservedAt.Segment + "\n";
            }

            // 战役准备段（M05 / TR-prep-001）：资源池 + 草稿 + 承诺（配置数据驱动，由载入方提供，不入存档体）。
            if (session.HasPreparation)
            {
                head += "prep\n";
                foreach (KeyValuePair<ResourceKey, long> kv in session.Pool!.AsAvailable().OrderBy(k => k.Key.Value, StringComparer.Ordinal))
                    head += "pool\t" + kv.Key.Value + "\t" + kv.Value + "\n";
                foreach (PreparedOrder o in session.Draft!.Orders.OrderBy(o => o.Id.Value, StringComparer.Ordinal))
                    head += "draftorder\t" + EncodeOrder(o) + "\n";
                if (session.CommittedPlan != null)
                {
                    head += "committed\t" + EncodeResources(session.CommittedPlan.CommittedResources) + "\n";
                    foreach (PreparedOrder o in session.CommittedPlan.Orders.OrderBy(o => o.Id.Value, StringComparer.Ordinal))
                        head += "committedorder\t" + EncodeOrder(o) + "\n";
                }
            }

            // 战斗段（M06 / TR-battle-001）：单位 + 侦测 + 种子 + 已满足兵法条件（配置数据驱动，载入方提供）。
            if (session.HasBattle)
            {
                head += "battle\t" + session.BattleSeed + "\t" + session.Battle!.ConfigFingerprint + "\n";
                foreach (BattleUnitState u in session.Battle!.Units.OrderBy(u => u.Id.Value, StringComparer.Ordinal))
                    head += "battleunit\t" + u.Id.Value + "\t" + u.Faction.Value + "\t" + u.Region.Value + "\t" + u.Force
                          + "\t" + u.Morale.Raw + "\t" + u.Fatigue.Raw + "\t" + u.Discipline.Raw
                          + "\t" + u.TerrainMod.Raw + "\t" + u.PostureMod.Raw + "\t" + u.Support.Raw + "\n";
                foreach (KeyValuePair<(FactionId Observer, BattleUnitId Target), Awareness> d in
                         session.Battle!.Detection.Entries
                            .OrderBy(e => e.Key.Observer.Value, StringComparer.Ordinal)
                            .ThenBy(e => e.Key.Target.Value, StringComparer.Ordinal))
                    head += "detection\t" + d.Key.Observer.Value + "\t" + d.Key.Target.Value + "\t" + (int)d.Value + "\n";
                foreach (TacticCondition c in session.BattleConditions.OrderBy(c => (int)c))
                    head += "battlecond\t" + (int)c + "\n";
            }

            // 后果续局段（M07 / TR-outcome-002）：最近战果分支 + 续局选项。
            if (session.HasOutcome)
            {
                head += "outcome\t" + (int)session.LastOutcomeBranch!.Value + "\n";
                foreach (ContinuationOption o in session.LastContinuationOptions)
                    head += "outcomeopt\t" + (int)o.Kind + "\t" + o.Reason + "\n";
            }

            return head + BodyMarker + "\n" + body;
        }

        /// <summary>
        /// 从快照恢复会话（ADR-0009 §R-1 / TR-session-003）。版本不兼容/指纹不符 → 整体抛 <see cref="SaveFormatException"/>，
        /// <b>不部分载入</b>。重建 GDD_004 权威（登记开局归属）+ GDD_015 投影订阅。
        /// </summary>
        public CampaignSession Restore(
            string text, ConfigFingerprint expectedFingerprint,
            CitySettlementConfig? settlementConfig = null, CityGovernanceConfig? governanceConfig = null,
            FixedPoint populationPressure = default,
            IntelConfig? intelConfig = null, SessionCouncilSetup? councilSetup = null,
            PreparationConfig? prepConfig = null,
            IReadOnlyCollection<RegionId>? reachableRegions = null, IReadOnlyCollection<OrderId>? authorizedOrders = null,
            BattleConfig? battleConfig = null, TacticChainConfig? tacticChains = null)
        {
            if (text is null) throw new SaveFormatException("会话存档文本为 null。");
            string[] lines = text.Split('\n');
            if (lines.Length < 4 || lines[0] != SnapshotMagic) throw new SaveFormatException("会话存档魔数不符。");
            string id = Field("id", lines[1]);
            string scenario = Field("scenario", lines[2]);

            // 可选城市治理段（M03）：解析城市态 + 后勤持有；配置由载入方提供（数据驱动）。
            int idx = 3;
            CityEconomyState? city = null;
            long cityLogistics = 0;
            if (idx < lines.Length && lines[idx].StartsWith("city\t", StringComparison.Ordinal))
            {
                (city, cityLogistics) = ParseCity(lines[idx]);
                idx++;
            }

            // 可选情报段（M04 / TR-intel-003）：世界真值与玩家知识**分别**重建，互不污染。
            WorldTruthLedger? truth = null;
            FactionIntel? playerIntel = null;
            if (idx < lines.Length && lines[idx].StartsWith("intel\t", StringComparison.Ordinal))
            {
                string[] ip = lines[idx].Split('\t');
                if (ip.Length != 2) throw new SaveFormatException($"情报段头格式不符：「{lines[idx]}」。");
                var playerFaction = new FactionId(ip[1]);
                truth = new WorldTruthLedger();
                playerIntel = new FactionIntel(playerFaction);
                idx++;

                // 真值段：只建真值，绝不填入玩家知识。
                while (idx < lines.Length && lines[idx].StartsWith("truth\t", StringComparison.Ordinal))
                {
                    truth.Set(ParseTruth(lines[idx]));
                    idx++;
                }
                // 知识段：只建玩家知识（经报告路径），绝不读真值。
                while (idx < lines.Length && lines[idx].StartsWith("knowledge\t", StringComparison.Ordinal))
                {
                    playerIntel.ApplyReport(ParseKnowledge(lines[idx], playerFaction));
                    idx++;
                }
            }

            // 可选战役准备段（M05 / TR-prep-001）：资源池 + 草稿 + 承诺；配置由载入方提供（数据驱动）。
            ResourcePool? pool = null;
            PlanDraft? draft = null;
            CommittedPlan? committed = null;
            if (idx < lines.Length && lines[idx] == "prep")
            {
                idx++;
                var poolDict = new Dictionary<ResourceKey, long>();
                while (idx < lines.Length && lines[idx].StartsWith("pool\t", StringComparison.Ordinal))
                {
                    string[] pp = lines[idx].Split('\t');
                    if (pp.Length != 3) throw new SaveFormatException($"资源池段格式不符：「{lines[idx]}」。");
                    poolDict[new ResourceKey(pp[1])] = long.Parse(pp[2]);
                    idx++;
                }
                pool = new ResourcePool(poolDict);

                draft = new PlanDraft();
                while (idx < lines.Length && lines[idx].StartsWith("draftorder\t", StringComparison.Ordinal))
                {
                    draft.AddOrder(ParseOrder(lines[idx], "draftorder"));
                    idx++;
                }

                if (idx < lines.Length && lines[idx].StartsWith("committed\t", StringComparison.Ordinal))
                {
                    string[] cp = lines[idx].Split('\t');
                    IReadOnlyDictionary<ResourceKey, long> cres = cp.Length == 2 ? DecodeResources(cp[1]) : DecodeResources("");
                    idx++;
                    var corders = new List<PreparedOrder>();
                    while (idx < lines.Length && lines[idx].StartsWith("committedorder\t", StringComparison.Ordinal))
                    {
                        corders.Add(ParseOrder(lines[idx], "committedorder"));
                        idx++;
                    }
                    committed = new CommittedPlan(corders, cres);
                }
            }

            // 可选战斗段（M06 / TR-battle-001）：单位 + 侦测 + 种子 + 已满足兵法条件；配置由载入方提供。
            BattleSnapshot? battle = null;
            ulong battleSeed = 0;
            var battleConditions = new List<TacticCondition>();
            if (idx < lines.Length && lines[idx].StartsWith("battle\t", StringComparison.Ordinal))
            {
                string[] bh = lines[idx].Split('\t');
                if (bh.Length != 3) throw new SaveFormatException($"战斗段头格式不符：「{lines[idx]}」。");
                battleSeed = ulong.Parse(bh[1]);
                string battleFingerprint = bh[2];
                idx++;

                var units = new List<BattleUnitState>();
                while (idx < lines.Length && lines[idx].StartsWith("battleunit\t", StringComparison.Ordinal))
                {
                    units.Add(ParseBattleUnit(lines[idx]));
                    idx++;
                }
                var detection = new DetectionState();
                while (idx < lines.Length && lines[idx].StartsWith("detection\t", StringComparison.Ordinal))
                {
                    string[] dp = lines[idx].Split('\t');
                    if (dp.Length != 4) throw new SaveFormatException($"侦测段格式不符：「{lines[idx]}」。");
                    detection.Set(new FactionId(dp[1]), new BattleUnitId(dp[2]), (Awareness)int.Parse(dp[3]));
                    idx++;
                }
                while (idx < lines.Length && lines[idx].StartsWith("battlecond\t", StringComparison.Ordinal))
                {
                    string[] cp = lines[idx].Split('\t');
                    if (cp.Length != 2) throw new SaveFormatException($"兵法条件段格式不符：「{lines[idx]}」。");
                    battleConditions.Add((TacticCondition)int.Parse(cp[1]));
                    idx++;
                }
                battle = new BattleSnapshot(units, detection, battleFingerprint);
            }

            // 可选后果续局段（M07）：分支 + 续局选项（无独立配置，纯状态恢复）。
            OutcomeBranch? lastOutcomeBranch = null;
            var lastOptions = new List<ContinuationOption>();
            if (idx < lines.Length && lines[idx].StartsWith("outcome\t", StringComparison.Ordinal))
            {
                string[] oh = lines[idx].Split('\t');
                if (oh.Length != 2) throw new SaveFormatException($"后果段头格式不符：「{lines[idx]}」。");
                lastOutcomeBranch = (OutcomeBranch)int.Parse(oh[1]);
                idx++;
                while (idx < lines.Length && lines[idx].StartsWith("outcomeopt\t", StringComparison.Ordinal))
                {
                    string[] op = lines[idx].Split('\t');
                    if (op.Length != 3) throw new SaveFormatException($"续局选项段格式不符：「{lines[idx]}」。");
                    lastOptions.Add(new ContinuationOption((ContinuationCommandKind)int.Parse(op[1]), op[2]));
                    idx++;
                }
            }

            if (idx >= lines.Length || lines[idx] != BodyMarker) throw new SaveFormatException("缺会话体标记。");
            string body = string.Join("\n", new ArraySegment<string>(lines, idx + 1, lines.Length - idx - 1));

            // 委派 CampaignSaveCodec：版本/指纹不符整体拒绝。
            CampaignSaveState state = _saveCodec.Deserialize(body, CampaignSaveVersion, expectedFingerprint);

            WorldState world = state.World.World;
            var authority = new CityControlAuthority();
            foreach (CityOwnership c in world.Cities)
                if (c.Owner.HasValue) authority.RegisterInitial(c.City, c.Owner.Value, new Garrison(c.Garrison));
            var projection = new WorldCityProjection(world, authority);

            if (city != null && (settlementConfig == null || governanceConfig == null))
                throw new SaveFormatException("存档含城市治理态，但未提供城市配置（settlementConfig/governanceConfig）以恢复。");
            if (truth != null && intelConfig == null)
                throw new SaveFormatException("存档含情报态，但未提供情报配置（intelConfig）以恢复。");
            if (pool != null && prepConfig == null)
                throw new SaveFormatException("存档含战役准备态，但未提供准备配置（prepConfig）以恢复。");
            if (battle != null && (battleConfig == null || tacticChains == null))
                throw new SaveFormatException("存档含战斗态，但未提供战斗配置（battleConfig/tacticChains）以恢复。");

            return new CampaignSession(
                id, scenario, expectedFingerprint, state.Career.Snapshot, projection, authority,
                cityEconomy: city, settlementConfig: settlementConfig, populationPressure: populationPressure,
                logisticsHolding: cityLogistics, governanceConfig: governanceConfig,
                truth: truth, playerIntel: playerIntel, intelConfig: intelConfig, council: councilSetup,
                pool: pool, draft: draft, prepConfig: prepConfig,
                reachableRegions: reachableRegions, authorizedOrders: authorizedOrders, committedPlan: committed,
                battle: battle, battleConfig: battleConfig, battleSeed: battleSeed,
                tacticChains: tacticChains, battleConditions: battleConditions,
                lastOutcomeBranch: lastOutcomeBranch, lastOptions: lastOptions);
        }

        /// <summary>解析战斗单位段：<c>battleunit\t{id}\t{faction}\t{region}\t{force}\t{morale.Raw}…{support.Raw}</c>。</summary>
        private static BattleUnitState ParseBattleUnit(string line)
        {
            string[] p = line.Split('\t');
            if (p.Length != 11 || p[0] != "battleunit")
                throw new SaveFormatException($"战斗单位段格式不符：「{line}」。");
            try
            {
                return new BattleUnitState(
                    new BattleUnitId(p[1]), new FactionId(p[2]), new RegionId(p[3]), int.Parse(p[4]),
                    FixedPoint.FromRaw(int.Parse(p[5])), FixedPoint.FromRaw(int.Parse(p[6])), FixedPoint.FromRaw(int.Parse(p[7])),
                    FixedPoint.FromRaw(int.Parse(p[8])), FixedPoint.FromRaw(int.Parse(p[9])), FixedPoint.FromRaw(int.Parse(p[10])));
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("战斗单位段数值解析失败：" + ex.Message);
            }
        }

        /// <summary>解析真值段：<c>truth\t{subject}\t{strength}\t{owner}</c>。</summary>
        private static TruthRecord ParseTruth(string line)
        {
            string[] p = line.Split('\t');
            if (p.Length != 4 || p[0] != "truth")
                throw new SaveFormatException($"真值段格式不符：「{line}」。");
            try
            {
                return new TruthRecord(new IntelSubjectId(p[1]), int.Parse(p[2]), new FactionId(p[3]));
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("真值段数值解析失败：" + ex.Message);
            }
        }

        /// <summary>解析知识段：<c>knowledge\t{subject}\t{strength}\t{source}\t{day}\t{segment}</c>，重建为报告并入知识。</summary>
        private static IntelReport ParseKnowledge(string line, FactionId faction)
        {
            string[] p = line.Split('\t');
            if (p.Length != 6 || p[0] != "knowledge")
                throw new SaveFormatException($"知识段格式不符：「{line}」。");
            try
            {
                var subject = new IntelSubjectId(p[1]);
                int strength = int.Parse(p[2]);
                var source = (IntelSource)int.Parse(p[3]);
                var observedAt = new WorldTime(int.Parse(p[4]), (DaySegment)int.Parse(p[5]));
                return new IntelReport(subject, faction, strength, source, observedAt);
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("知识段数值解析失败：" + ex.Message);
            }
        }

        // --- 准备段编解码（M05）。受控字符串（res/order/region id 无 \t;,= 特殊字符），MVP 不做转义。---

        private static string EncodeResources(IReadOnlyDictionary<ResourceKey, long> res)
            => string.Join(";", res.OrderBy(k => k.Key.Value, StringComparer.Ordinal).Select(kv => kv.Key.Value + "=" + kv.Value));

        private static string EncodeOrder(PreparedOrder o)
        {
            string needs = EncodeResources(o.ResourceNeeds);
            string deps = string.Join(",", o.Dependencies.OrderBy(d => d.Value, StringComparer.Ordinal).Select(d => d.Value));
            return o.Id.Value + "\t" + o.Executor.Value + "\t" + o.Target.Value
                 + "\t" + o.Window.Start + "\t" + o.Window.End + "\t" + needs + "\t" + deps;
        }

        private static IReadOnlyDictionary<ResourceKey, long> DecodeResources(string encoded)
        {
            var dict = new Dictionary<ResourceKey, long>();
            if (string.IsNullOrEmpty(encoded)) return dict;
            foreach (string pair in encoded.Split(';'))
            {
                string[] kv = pair.Split('=');
                if (kv.Length != 2) throw new SaveFormatException($"资源编码格式不符：「{pair}」。");
                dict[new ResourceKey(kv[0])] = long.Parse(kv[1]);
            }
            return dict;
        }

        /// <summary>解析准备命令行（draftorder/committedorder 后的 7 字段）。</summary>
        private static PreparedOrder ParseOrder(string line, string tag)
        {
            string[] p = line.Split('\t');
            if (p.Length != 8 || p[0] != tag)
                throw new SaveFormatException($"准备命令段格式不符：「{line}」。");
            try
            {
                var deps = string.IsNullOrEmpty(p[7])
                    ? (IReadOnlyList<OrderId>)Array.Empty<OrderId>()
                    : p[7].Split(',').Select(v => new OrderId(v)).ToList();
                return new PreparedOrder(
                    new OrderId(p[1]), new CharacterId(p[2]), new RegionId(p[3]),
                    new ThreeKingdom.Domain.Preparation.TimeWindow(int.Parse(p[4]), int.Parse(p[5])),
                    DecodeResources(p[6]), deps);
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("准备命令段数值解析失败：" + ex.Message);
            }
        }

        /// <summary>解析城市治理段：<c>city\t{id}\t{stock}\t{reserved}\t{morale}\t{security}\t{fortCur}\t{fortMax}\t{logistics}</c>。</summary>
        private static (CityEconomyState, long) ParseCity(string line)
        {
            string[] p = line.Split('\t');
            if (p.Length != 9 || p[0] != "city")
                throw new SaveFormatException($"城市治理段格式不符：「{line}」。");
            try
            {
                var city = new CityEconomyState(
                    new CityId(p[1]),
                    long.Parse(p[2]), long.Parse(p[3]),
                    int.Parse(p[4]), int.Parse(p[5]),
                    int.Parse(p[6]), int.Parse(p[7]));
                long logistics = long.Parse(p[8]);
                return (city, logistics);
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("城市治理段数值解析失败：" + ex.Message);
            }
        }

        private static string Field(string key, string line)
        {
            string[] p = line.Split('\t');
            if (p.Length < 2 || p[0] != key) throw new SaveFormatException($"期望字段「{key}」，实得「{line}」。");
            return p[1];
        }
    }
}


