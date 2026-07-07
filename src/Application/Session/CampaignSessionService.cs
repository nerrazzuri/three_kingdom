using System;
using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Conquest;
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
using ThreeKingdom.Domain.Subversion;
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

            // 登记非开局城的初始归属到控制权权威（敌/中立城，供出征占城 GDD_019；开局城已由 governor 登记，跳过避重复）。
            foreach (CityOwnership c in config.InitialCities)
                if (c.Owner.HasValue && c.City != config.GovernorSeed.City)
                    authority.RegisterInitial(c.City, c.Owner.Value, new Garrison(c.Garrison));

            // 每局独立情报层：从配置初始情报<b>播种一份全新</b> FactionIntel——配置里的 PlayerIntel 是可变实例，
            // 直接复用会让多局（重开「新游戏」）共用同一知识态而互相串扰。真值只读，可共享。
            FactionIntel? playerIntel = CloneInitialIntel(config.PlayerIntel);

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
                playerIntel: playerIntel,             // 每局独立（见上 CloneInitialIntel）
                intelConfig: config.IntelConfig,
                council: config.CouncilSetup,
                pool: config.ResourcePool,            // M05：准备态（可选；null 则不启用准备循环）
                prepConfig: config.PreparationConfig,
                reachableRegions: config.ReachableRegions,
                authorizedOrders: config.AuthorizedOrders,
                historyCatalog: config.HistoryCatalog,   // M10：历史世界态（可选；null 则不启用历史循环）
                historyReach: config.PlayerReach,
                divergenceConfig: config.DivergenceConfig);

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

            // 治理（GDD_004 派人处理→需时见效）：推进后应用已到完成时刻的在办治理事务。
            ResolveArrivedGovernance(session);

            // 情报（GDD_007 派出→在途→返报）：推进后解析已到返报时刻的在途侦察 → 报告并入玩家知识。
            ResolveArrivedScouts(session);

            // 忠诚经营（GDD_014）：每跨一日 → 僚属忠诚衰减 + 对手对最不忠者种子化挖角（忠者不可挖）。
            TickRetinueLoyalty(session, dayBefore, session.CurrentTime.Day);

            return session;
        }

        /// <summary>每跨一个日界推进忠诚衰减 + 挖角（GDD_014 忠诚经营 A2）。无僚属则无操作。确定性种子。</summary>
        private void TickRetinueLoyalty(CampaignSession session, int dayBefore, int dayAfter)
        {
            if (dayAfter <= dayBefore) return;
            RetinueState retinue = session.Career.Retinue;
            if (retinue.Members.Count == 0) return;

            RetinueLoyaltyConfig cfg = RetinueLoyaltyConfig.Default;
            FixedPoint poacherPull = FixedPoint.FromFraction(3, 10);   // 对手拉拢力（适度；C11 平衡打磨）
            for (int day = dayBefore; day < dayAfter; day++)
            {
                retinue = _retinueLoyalty.Decay(retinue, cfg);
                CharacterId? weakest = LeastLoyalMember(retinue);
                if (weakest != null)
                {
                    ulong seed = LoyaltySeed(session.Id, day, weakest.Value.Value);
                    PoachResult pr = _retinueLoyalty.AttemptPoach(retinue, weakest.Value, poacherPull, seed, cfg);
                    retinue = pr.State;
                }
            }
            session.SetCareer(new CareerSnapshot(session.Career.Career, retinue));
        }

        /// <summary>最不忠僚属（好感最低者；空则 null）。规范序：同好感取 id 序数最小，确定性。</summary>
        private static CharacterId? LeastLoyalMember(RetinueState retinue)
        {
            CharacterId? weakest = null;
            FixedPoint min = FixedPoint.One;
            foreach (RetinueMember m in retinue.Members)   // 已按 id 序数升序
                if (weakest == null || m.Affinity < min) { weakest = m.Character; min = m.Affinity; }
            return weakest;
        }

        private static ulong LoyaltySeed(string sessionId, int day, string member)
        {
            ulong h = 1469598103934665603UL;
            void Mix(string s) { if (s != null) foreach (char c in s) { h ^= c; h *= 1099511628211UL; } }
            Mix(sessionId); Mix("|"); Mix(member);
            h ^= (ulong)(day + 1) * 2654435761UL;
            return h;
        }

        /// <summary>
        /// 应用已到完成时刻（CompletionTime ≤ 当前）的在办治理事务：按 (完成时刻, 类别) 稳定序调既有即时结算逻辑
        /// 应用效果 → 移出在办列表（确定性）。完成时若前置已不满足（如工事已满）则该件无效果，仍移除。
        /// </summary>
        private void ResolveArrivedGovernance(CampaignSession session)
        {
            if (!session.HasCityGovernance || session.PendingGovernance.Count == 0) return;
            WorldTime now = session.CurrentTime;

            var arrived = new List<PendingGovernanceTask>();
            foreach (PendingGovernanceTask t in session.PendingGovernance)
                if (t.CompletionTime <= now) arrived.Add(t);
            arrived.Sort((a, b) =>
            {
                int c = a.CompletionTime.AbsoluteIndex.CompareTo(b.CompletionTime.AbsoluteIndex);
                return c != 0 ? c : ((int)a.Kind).CompareTo((int)b.Kind);
            });

            foreach (PendingGovernanceTask t in arrived)
            {
                switch (t.Kind)
                {
                    case GovernanceActionKind.Requisition: RequisitionFood(session, t.Amount); break;
                    case GovernanceActionKind.RepairFortification: RepairFortification(session); break;
                    case GovernanceActionKind.Appease: Appease(session); break;
                }
                session.RemovePendingGovernance(t);
            }
        }

        /// <summary>
        /// 解析已到返报时刻（ArrivalTime ≤ 当前）的在途侦察：按 (返报时刻, 主题) 稳定序观察真值 → 报告 → 并入玩家知识 →
        /// 移出在途列表（确定性；观察取返报时刻真值快照）。反全知：只经 <see cref="IntelService"/>，UI 仍读投影。
        /// </summary>
        private void ResolveArrivedScouts(CampaignSession session)
        {
            if (!session.HasIntel || session.PendingScouts.Count == 0) return;
            WorldTime now = session.CurrentTime;

            var arrived = new List<PendingScout>();
            foreach (PendingScout s in session.PendingScouts)
                if (s.ArrivalTime <= now) arrived.Add(s);
            arrived.Sort((a, b) =>
            {
                int c = a.ArrivalTime.AbsoluteIndex.CompareTo(b.ArrivalTime.AbsoluteIndex);
                return c != 0 ? c : string.CompareOrdinal(a.Subject.Value, b.Subject.Value);
            });

            FactionId observer = session.PlayerIntel!.Faction;
            foreach (PendingScout s in arrived)
            {
                Observation observation = _intel.Observe(session.Truth!, s.Subject, observer, s.ArrivalTime);
                IntelReport report = _intel.ToReport(observation, observer, s.Method);
                session.PlayerIntel!.ApplyReport(report);
                session.RemovePendingScout(s);
            }
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

        // --- 治理下令（GDD_004 派人处理→需时见效，非即时）。校验通过记为在办，推进到完成时刻由 Advance 应用效果。---

        /// <summary>下令征用军粮（非即时）：校验可分配量足够后记为在办，约 <paramref name="leadSegments"/> 时段后见效。</summary>
        public CampaignCommandResult DispatchRequisition(CampaignSession session, long amount, int leadSegments)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");
            if (amount < 0 || leadSegments < 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, "征用量与办理时段不可为负。");
            if (amount > session.CityEconomy!.Available)
                return CampaignCommandResult.Failure(CampaignErrorCode.InsufficientStock,
                    $"可分配量不足：available={session.CityEconomy!.Available}，请求={amount}。");

            session.AddPendingGovernance(new PendingGovernanceTask(
                GovernanceActionKind.Requisition, amount, session.CurrentTime.Advance(leadSegments)));
            return CampaignCommandResult.Success();
        }

        /// <summary>下令修工事（非即时）：工事已满则拒绝，否则记为在办，约 <paramref name="leadSegments"/> 时段后见效。</summary>
        public CampaignCommandResult DispatchRepair(CampaignSession session, int leadSegments)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");
            if (leadSegments < 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, "办理时段不可为负。");
            if (session.CityEconomy!.FortificationCurrent >= session.CityEconomy!.FortificationMax)
                return CampaignCommandResult.Failure(CampaignErrorCode.FortificationFull, "工事已满，无可修复余量。");

            session.AddPendingGovernance(new PendingGovernanceTask(
                GovernanceActionKind.RepairFortification, 0, session.CurrentTime.Advance(leadSegments)));
            return CampaignCommandResult.Success();
        }

        /// <summary>下令安抚（非即时）：记为在办，约 <paramref name="leadSegments"/> 时段后见效。</summary>
        public CampaignCommandResult DispatchAppease(CampaignSession session, int leadSegments)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasCityGovernance)
                return CampaignCommandResult.Failure(CampaignErrorCode.CityGovernanceDisabled, "会话未启用城市治理。");
            if (leadSegments < 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, "办理时段不可为负。");

            session.AddPendingGovernance(new PendingGovernanceTask(
                GovernanceActionKind.Appease, 0, session.CurrentTime.Advance(leadSegments)));
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

        /// <summary>
        /// 派出侦察（GDD_007 派出→在途→返报，<b>非即时</b>）：记一支在途侦察兵，预计 <paramref name="leadSegments"/> 时段后返报；
        /// 报告在 <see cref="Advance"/> 推进到返报时刻时才并入知识。对象须登记于世界真值，否则稳定错误码、零写入。
        /// </summary>
        public CampaignCommandResult DispatchScout(CampaignSession session, IntelSubjectId subject, IntelSource method, int leadSegments)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasIntel)
                return CampaignCommandResult.Failure(CampaignErrorCode.IntelDisabled, "会话未启用情报。");
            if (leadSegments < 0)
                return CampaignCommandResult.Failure(CampaignErrorCode.InvalidAmount, $"侦察行程时段不可为负：{leadSegments}。");
            if (!session.Truth!.Has(subject))
                return CampaignCommandResult.Failure(CampaignErrorCode.UnknownIntelSubject,
                    $"侦察对象未登记或非法：{subject}。");

            WorldTime arrival = session.CurrentTime.Advance(leadSegments);
            session.AddPendingScout(new PendingScout(subject, method, arrival));
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

        // --- 生涯与权限命令（M09 / TR-career-001/002/005）。复用 epic-011 Domain；成功才写回会话生涯态。---

        private readonly CareerProgressionService _careerProgression = new CareerProgressionService();
        private readonly RebellionService _rebellion = new RebellionService();

        /// <summary>
        /// 功绩累积（GDD_014 / TR-career-002）：按功绩来源（含<b>非战斗源</b>）增长生涯 merit/renown/standing。
        /// 战斗不是唯一成长来源；成功写回会话生涯态。
        /// </summary>
        public CareerCommandResult ApplyCareerGain(CampaignSession session, PromotionLadderConfig ladder, CareerGainSource source, int count = 1)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (ladder is null) throw new ArgumentNullException(nameof(ladder));
            CareerCommandResult r = _careerProgression.ApplyGain(ladder, session.Career, source, count);
            if (r.Applied) session.SetCareer(r.Snapshot);
            return r;
        }

        /// <summary>名望惩罚（GDD_014 / W5：君主任务失败/逾期）：生涯名望减 <paramref name="amount"/>（下限 0），写回会话。</summary>
        public void PenalizeRenown(CampaignSession session, int amount)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            session.SetCareer(_careerProgression.ApplyRenownPenalty(session.Career, amount));
        }

        /// <summary>征粮上缴（GDD_014 / W5：君主任务·献纳；GDD_004 城库存扣减）：可支配库存足则扣 <paramref name="amount"/> 并写回，返回是否成功。</summary>
        public bool LevyGrain(CampaignSession session, long amount)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            CityEconomyState? econ = session.CityEconomy;
            if (econ == null || amount <= 0 || econ.Available < amount) return false;
            session.SetCityEconomy(econ.With(stock: econ.Stock - amount));
            return true;
        }

        /// <summary>
        /// 申请晋升（GDD_014 / TR-career-001/005）：门槛达成则晋一阶并写回；未达
        /// <see cref="CareerErrorCode.PromotionThresholdNotMet"/> 稳定错误码、无写入。
        /// </summary>
        public CareerCommandResult RequestPromotion(CampaignSession session, PromotionLadderConfig ladder)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (ladder is null) throw new ArgumentNullException(nameof(ladder));
            CareerCommandResult r = _careerProgression.RequestPromotion(ladder, session.Career);
            if (r.Applied) session.SetCareer(r.Snapshot);
            return r;
        }

        /// <summary>自立资格判定（GDD_014 / TR-career-002）：三分支（军事/政治/压迫）确定性判定，只读不写。</summary>
        public RebellionEligibility CheckRebellionEligibility(CampaignSession session, RebellionConfig config, RebellionContext context)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (config is null) throw new ArgumentNullException(nameof(config));
            return _rebellion.CheckEligibility(config, session.Career.Career, session.Career.Retinue, context);
        }

        /// <summary>
        /// 发起自立（GDD_014 / TR-career-001/005）：资格达成则转新势力/在野并写回；不达稳定错误码、无写入。
        /// 失败不切死局（在野亦为合法续局）。
        /// </summary>
        public RebellionResult LaunchRebellion(CampaignSession session, RebellionConfig config, RebellionContext context)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (config is null) throw new ArgumentNullException(nameof(config));
            RebellionResult r = _rebellion.Launch(config, session.Career, context);
            if (r.Launched) session.SetCareer(r.Snapshot);
            return r;
        }

        // --- 历史世界命令（M10 / TR-world-001~006）。历史事件按时间窗触发；够不着前置短路；触及分叉传播。---

        private readonly HistoryAdvancer _historyAdvancer = new HistoryAdvancer();
        private readonly DivergencePropagationService _divergence = new DivergencePropagationService();

        /// <summary>
        /// 推进历史世界（GDD_015 / ADR-0007 / TR-world-002）：对目录中时间窗已到、未触发的历史事件按稳定序触发——
        /// 够不着（reachability 外）→ 正常结局且前置短路恒成立（早期历史便宜）；够得着且前置成立 → 正常；
        /// 够得着但前置被玩家破坏 → 分叉，并向下游传播重评估（脱稿深度由配置定）。历史态写入 WorldState（存档 world 段）。
        /// </summary>
        public IReadOnlyList<HistoryAdvanceResult> AdvanceHistory(CampaignSession session)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (!session.HasHistory) return Array.Empty<HistoryAdvanceResult>();

            HistoricalEventCatalog catalog = session.HistoryCatalog!;
            WorldTime now = session.CurrentTime;
            var results = new List<HistoryAdvanceResult>();

            // 到期（Window.Start ≤ 当前）且未触发的事件，按 EventId 稳定序触发（确定性）。
            var due = new List<HistoricalEvent>();
            foreach (HistoricalEvent e in catalog.Events)
                if (!session.World.IsTriggered(e.Id.Value) && e.Window.Start <= now)
                    due.Add(e);
            due.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));

            foreach (HistoricalEvent e in due)
            {
                HistoryAdvanceResult r = _historyAdvancer.OnTimeWindowEnter(catalog, e.Id, session.World, session.HistoryReach);
                session.RestoreWorld(r.World);
                if (r.Diverged)
                {
                    DivergencePropagationResult prop = _divergence.Propagate(catalog, e, session.World, session.HistoryReach, session.DivergenceConfig);
                    session.RestoreWorld(prop.World);
                }
                results.Add(r);
            }

            return results;
        }

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

        // --- 出征攻城（M12+ / GDD_019 / ADR-0010）。授权→占城归属 C→控制权变更→功绩/自立倾向。---

        private readonly OccupationOwnershipService _occupation = new OccupationOwnershipService();
        private readonly OffensiveAuthorizationService _offensiveAuth = new OffensiveAuthorizationService();
        private readonly OffensiveSetupService _offensiveSetup = new OffensiveSetupService();
        private readonly SiegeResolutionService _siege = new SiegeResolutionService();
        private readonly SubversionService _subversion = new SubversionService();
        private readonly RetinueLoyaltyService _retinueLoyalty = new RetinueLoyaltyService();

        /// <summary>
        /// 战前人心杠杆施计（GDD_024 全循环）：反全知门（守将画像自 Intel/Relationships 投影，未侦察折扣）→
        /// 种子化结算（成/反噬/无效）→ 成功则把战斗接缝效果<b>累积</b>到该城待生效态（出征发起时消费）。
        /// 无论成败均记一次尝试（成功度递减源，W5）。授权/立场约束由调用方按 GDD_023 先行校验。
        /// </summary>
        public SubversionOutcome AttemptSubversion(
            CampaignSession session, CityId city, SubversionScheme scheme,
            SubversionTargetProfile target, FixedPoint intensity, ulong seed, SubversionConfig config)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (config is null) throw new ArgumentNullException(nameof(config));

            int prior = session.SubversionAttemptsOn(city);
            SubversionOutcome outcome = _subversion.Resolve(scheme, target, intensity, prior, seed, config);
            session.RecordSubversionAttempt(city);
            if (outcome.Result == SubversionResult.Success)
                session.AccumulateSubversion(city, outcome.Effect);
            else if (outcome.Exposed)
                session.MarkSubversionExposed(city);   // 反噬 → 该城守将警觉、情报暴露（GDD_024 R4）
            return outcome;
        }

        /// <summary>把人心杠杆效果作用于守备（GDD_024 F3 抽象攻城接缝）：有效守军×(1−倒戈比)、工事因子按士气/军纪 delta 削弱（下限 0.1）。</summary>
        private static SiegeDefense ApplySubversion(SiegeDefense defense, SubversionEffect effect)
        {
            if (effect is null || effect.IsNone) return defense;
            int garrison = (FixedPoint.FromInt(defense.Garrison) * (FixedPoint.One - effect.GarrisonDefectRatio)).RoundToInt();
            if (garrison < 0) garrison = 0;
            FixedPoint fort = defense.FortFactor + effect.DefenderMoraleDelta + effect.DefenderDisciplineDelta;
            FixedPoint floor = FixedPoint.FromFraction(1, 10);
            if (fort < floor) fort = floor;
            return new SiegeDefense(garrison, fort);
        }

        /// <summary>
        /// 出征攻城端到端（GDD_019 全循环）：授权门 → 闭合因果（准备→战力）→ 攻城结算（准备决定胜负）→
        /// 胜则占城归属 C（控制权/记功/自立倾向）、败则退兵可继续。全程确定性、无胜率。
        /// </summary>
        public OffensiveResult LaunchOffensive(
            CampaignSession session, CityId city,
            OffensivePreparation prep, OffensiveSetupConfig setupConfig,
            SiegeDefense defense, SiegeResolutionConfig siegeConfig,
            FactionId playerFaction, FactionId lordFaction, Garrison conqueredGarrison,
            FixedPoint renownNorm, FixedPoint standingNorm, FixedPoint cityValueNorm,
            ulong seed, OccupationConfig occupationConfig,
            PromotionLadderConfig? ladder = null)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (prep is null) throw new ArgumentNullException(nameof(prep));

            OffensiveGateResult gate = CheckOffensiveTarget(session, city, playerFaction);
            if (gate != OffensiveGateResult.Authorized) return OffensiveResult.Rejected(gate);

            // 人心杠杆（GDD_024）：消费该城战前累积的施计效果 → 削弱守备（守军倒戈/士气军纪崩）。
            SiegeDefense effectiveDefense = ApplySubversion(defense, session.ConsumePendingSubversion(city));

            OffensiveForce force = _offensiveSetup.Derive(prep, setupConfig);           // 闭合因果
            if (!_siege.AttackerWins(force, effectiveDefense, siegeConfig))
                return OffensiveResult.Defeated(force);                                 // 败：不占城，可继续

            ConquestResult conquest = ResolveConquest(                                   // 胜：占城归属 C
                session, city, conqueredGarrison, playerFaction, lordFaction,
                renownNorm, standingNorm, cityValueNorm, seed, occupationConfig,
                ladder, CareerGainSource.MajorBattleVictory);

            return OffensiveResult.Won(force, conquest);
        }

        private readonly ThreeKingdom.Domain.Contention.RivalExpansionService _rivalExpansion = new ThreeKingdom.Domain.Contention.RivalExpansionService();

        /// <summary>争霸编排（GDD_017）：对手兼并一步（强吞弱），持久化到会话并返回新态。纳入统一存档。</summary>
        public ThreeKingdom.Domain.Contention.ContentionState StepRivalContention(
            CampaignSession session, ThreeKingdom.Domain.Contention.ContentionState current,
            FactionId player, ulong seed, ThreeKingdom.Domain.Contention.ContentionConfig config)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (current is null) throw new ArgumentNullException(nameof(current));
            ThreeKingdom.Domain.Contention.ContentionState next = _rivalExpansion.Step(current, player, seed, config);
            session.SetContention(next);
            return next;
        }

        /// <summary>战略化推进（E4.2）：意图驱动兼并 + 对玩家施压。传入趋势基准 prev 与被夺方集 wronged。</summary>
        public ThreeKingdom.Domain.Contention.ContentionState StepRivalContentionStrategic(
            CampaignSession session, ThreeKingdom.Domain.Contention.ContentionState current, FactionId player,
            ThreeKingdom.Domain.Contention.ContentionState? prev, System.Collections.Generic.IReadOnlyCollection<string> wronged,
            ulong seed, ThreeKingdom.Domain.Contention.ContentionConfig config)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (current is null) throw new ArgumentNullException(nameof(current));
            ThreeKingdom.Domain.Contention.ContentionState next = _rivalExpansion.StepStrategic(current, player, prev, wronged, seed, config);
            session.SetContention(next);
            return next;
        }

        /// <summary>争霸编排（GDD_017）：玩家占城 → 领土 +1、被夺方 −1，持久化到会话并返回新态。</summary>
        public ThreeKingdom.Domain.Contention.ContentionState RecordPlayerConquest(
            CampaignSession session, ThreeKingdom.Domain.Contention.ContentionState current, FactionId player, FactionId? loser)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (current is null) throw new ArgumentNullException(nameof(current));
            ThreeKingdom.Domain.Contention.ContentionState c = current.WithCities(player, current.CitiesOf(player) + 1);
            if (loser.HasValue) c = c.WithCities(loser.Value, Math.Max(0, c.CitiesOf(loser.Value) - 1));
            session.SetContention(c);
            return c;
        }

        /// <summary>
        /// 复位为新主太守（GDD_026 补·东山再起）：势力被灭、归顺/投奔成功后，在<b>活世界</b>里授玩家一城续局——
        /// 新主（<paramref name="newLord"/>）割一城（<paramref name="newCity"/>）予玩家（经 GDD_004 唯一权威控制权变更，
        /// 叙事性 owner_change），争霸态玩家 +1/新主 −1，城市治理重置于新城。<b>世界时钟/生涯/一生皆不重置</b>——
        /// 只是换了个落脚处接着活。返回更新后的争霸态。城须已在控制权权威登记（新主本据之，故已登记）。
        /// </summary>
        public ThreeKingdom.Domain.Contention.ContentionState ReseatGovernor(
            CampaignSession session, ThreeKingdom.Domain.Contention.ContentionState contention,
            FactionId playerFaction, FactionId newLord, CityId newCity, Garrison garrison, CityEconomyState economy)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (contention is null) throw new ArgumentNullException(nameof(contention));
            if (economy is null) throw new ArgumentNullException(nameof(economy));

            session.Control.RequestControlChange(newCity, playerFaction, garrison, ChangeCause.HistoricalDivergence);
            ThreeKingdom.Domain.Contention.ContentionState c = contention
                .WithCities(playerFaction, contention.CitiesOf(playerFaction) + 1)
                .WithCities(newLord, Math.Max(0, contention.CitiesOf(newLord) - 1));
            session.SetContention(c);
            session.SetCityEconomy(economy);
            return c;
        }

        /// <summary>君主授权出征（GDD_019 R1）：设置可攻目标城集合（由君主政令按官阶组装）。</summary>
        public void AuthorizeOffensive(CampaignSession session, IReadOnlyCollection<CityId> targets)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            session.SetOffensiveAuthorization(new OffensiveAuthorization(targets));
        }

        /// <summary>出征授权门（GDD_019 R1/R2）：目标须授权 + 敌控城（非己方）。目标归属只读控制权投影（反全知）。</summary>
        public OffensiveGateResult CheckOffensiveTarget(CampaignSession session, CityId city, FactionId playerFaction)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            FactionId? owner = session.Control.OwnerOf(city);
            return _offensiveAuth.Check(city, session.OffensiveAuthorization, owner, playerFaction);
        }

        /// <summary>
        /// 结算占城（GDD_019 §占城 C / ADR-0010）：攻城<b>胜后</b>调用——判定占领归属（前 N 座归玩家、之后君主种子化取舍），
        /// 经 GDD_004 控制权变更事件写入（新控制方 = 玩家/君主），LordKeeps 则累积自立倾向，记占城计数，
        /// 并（若给梯队）应用出征战功。城池须已在控制权权威登记（否则抛，须先 RegisterInitial）。返回判定结果。
        /// </summary>
        public ConquestResult ResolveConquest(
            CampaignSession session, CityId city, Garrison newGarrison,
            FactionId playerFaction, FactionId lordFaction,
            FixedPoint renownNorm, FixedPoint standingNorm, FixedPoint cityValueNorm,
            ulong seed, OccupationConfig config,
            PromotionLadderConfig? ladder = null, CareerGainSource gainSource = CareerGainSource.CombatVictory)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (config is null) throw new ArgumentNullException(nameof(config));

            OwnershipVerdict verdict = _occupation.Resolve(session.ConquestCount, renownNorm, standingNorm, cityValueNorm, seed, config);
            FactionId newOwner = verdict == OwnershipVerdict.GrantToPlayer ? playerFaction : lordFaction;

            // 经 GDD_004 唯一权威发起控制权变更（ADR-0008）；装配层不直接写归属。
            session.Control.RequestControlChange(city, newOwner, newGarrison, ChangeCause.SiegeConquest);

            if (verdict == OwnershipVerdict.LordKeeps)
                session.AddRebellionLean(config.LeanPerSeizure);   // 战果被夺 → 自立倾向累积（喂 GDD_014）
            session.RecordConquest();

            bool careerApplied = false;
            if (ladder != null)
            {
                CareerCommandResult r = _careerProgression.ApplyGain(ladder, session.Career, gainSource);
                if (r.Applied) { session.SetCareer(r.Snapshot); careerApplied = true; }
            }

            return new ConquestResult(verdict, session.ConquestCount, session.RebellionLean, careerApplied);
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

                // 在办治理事务段（GDD_004）：按 (完成时刻, 类别) 稳定序，确定性。
                var govTasks = new List<PendingGovernanceTask>(session.PendingGovernance);
                govTasks.Sort((a, b) =>
                {
                    int cc = a.CompletionTime.AbsoluteIndex.CompareTo(b.CompletionTime.AbsoluteIndex);
                    return cc != 0 ? cc : ((int)a.Kind).CompareTo((int)b.Kind);
                });
                foreach (PendingGovernanceTask t in govTasks)
                    head += "govtask\t" + (int)t.Kind + "\t" + t.Amount
                          + "\t" + t.CompletionTime.Day + "\t" + (int)t.CompletionTime.Segment + "\n";
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

                // 在途侦察段（GDD_007 派出→在途→返报）：按 (返报时刻, 主题) 稳定序，确定性。
                var pending = new List<PendingScout>(session.PendingScouts);
                pending.Sort((a, b) =>
                {
                    int c = a.ArrivalTime.AbsoluteIndex.CompareTo(b.ArrivalTime.AbsoluteIndex);
                    return c != 0 ? c : string.CompareOrdinal(a.Subject.Value, b.Subject.Value);
                });
                foreach (PendingScout p in pending)
                    head += "pendingscout\t" + p.Subject.Value + "\t" + (int)p.Method
                          + "\t" + p.ArrivalTime.Day + "\t" + (int)p.ArrivalTime.Segment + "\n";
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

            // 出征攻城段（GDD_019 / ADR-0010 D4）：占城计数 + 自立倾向 + 授权目标（稳定序）。会话级 meta，恒序列化。
            head += "conquest\t" + session.ConquestCount + "\t" + session.RebellionLean + "\n";
            var authTargets = new List<CityId>(session.OffensiveAuthorization.AuthorizedTargets);
            authTargets.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            foreach (CityId t in authTargets)
                head += "authtarget\t" + t.Value + "\n";

            // 人心杠杆待生效段（GDD_024 §14）：每城累积效果（士气/倒戈/军纪 raw）+ 施计次数（稳定序，取待生效∪已施计城并集）。
            var subCities = new SortedSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<CityId, SubversionEffect> kv in session.PendingSubversionMap) subCities.Add(kv.Key.Value);
            foreach (KeyValuePair<CityId, int> kv in session.SubversionAttemptsMap) subCities.Add(kv.Key.Value);
            foreach (string cv in subCities)
            {
                var cid = new CityId(cv);
                SubversionEffect e = session.PendingSubversionFor(cid);
                head += "subversion\t" + cv
                      + "\t" + e.DefenderMoraleDelta.Raw + "\t" + e.GarrisonDefectRatio.Raw + "\t" + e.DefenderDisciplineDelta.Raw
                      + "\t" + session.SubversionAttemptsOn(cid) + "\n";
            }

            // 君主争霸段（GDD_017）：各势力领城（按势力 id 稳定序）。运行期态纳入统一存档。
            if (session.Contention != null)
            {
                var powers = new List<ThreeKingdom.Domain.Contention.PowerStanding>(session.Contention.Powers);
                powers.Sort((a, b) => string.CompareOrdinal(a.Faction.Value, b.Faction.Value));
                foreach (ThreeKingdom.Domain.Contention.PowerStanding p in powers)
                    head += "power\t" + p.Faction.Value + "\t" + p.Cities + "\n";
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
            var pendingGovernance = new List<PendingGovernanceTask>();
            if (idx < lines.Length && lines[idx].StartsWith("city\t", StringComparison.Ordinal))
            {
                (city, cityLogistics) = ParseCity(lines[idx]);
                idx++;
                // 在办治理事务段（GDD_004）：恢复未完成事务（推进到完成时刻由 Advance 应用）。
                while (idx < lines.Length && lines[idx].StartsWith("govtask\t", StringComparison.Ordinal))
                {
                    pendingGovernance.Add(ParseGovTask(lines[idx]));
                    idx++;
                }
            }

            // 可选情报段（M04 / TR-intel-003）：世界真值与玩家知识**分别**重建，互不污染。
            WorldTruthLedger? truth = null;
            FactionIntel? playerIntel = null;
            var pendingScouts = new List<PendingScout>();
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
                // 在途侦察段（GDD_007）：恢复未返报的侦察兵（推进到返报时刻由 Advance 解析）。
                while (idx < lines.Length && lines[idx].StartsWith("pendingscout\t", StringComparison.Ordinal))
                {
                    pendingScouts.Add(ParsePendingScout(lines[idx]));
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

            // 出征攻城段（GDD_019 / ADR-0010）：占城计数 + 自立倾向 + 授权目标。
            int conquestCount = 0, rebellionLean = 0;
            var authTargets = new List<CityId>();
            var pendingSubversion = new Dictionary<CityId, SubversionEffect>();
            var subversionAttempts = new Dictionary<CityId, int>();
            var powers = new List<ThreeKingdom.Domain.Contention.PowerStanding>();
            if (idx < lines.Length && lines[idx].StartsWith("conquest\t", StringComparison.Ordinal))
            {
                string[] cq = lines[idx].Split('\t');
                if (cq.Length != 3) throw new SaveFormatException($"出征段格式不符：「{lines[idx]}」。");
                try { conquestCount = int.Parse(cq[1]); rebellionLean = int.Parse(cq[2]); }
                catch (FormatException ex) { throw new SaveFormatException("出征段数值解析失败：" + ex.Message); }
                idx++;
                while (idx < lines.Length && lines[idx].StartsWith("authtarget\t", StringComparison.Ordinal))
                {
                    string[] at = lines[idx].Split('\t');
                    if (at.Length != 2) throw new SaveFormatException($"授权目标段格式不符：「{lines[idx]}」。");
                    authTargets.Add(new CityId(at[1]));
                    idx++;
                }
                // 人心杠杆待生效段（GDD_024 §14）。
                while (idx < lines.Length && lines[idx].StartsWith("subversion\t", StringComparison.Ordinal))
                {
                    string[] sv = lines[idx].Split('\t');
                    if (sv.Length != 6) throw new SaveFormatException($"人心杠杆段格式不符：「{lines[idx]}」。");
                    try
                    {
                        var cid = new CityId(sv[1]);
                        var eff = new SubversionEffect(
                            FixedPoint.FromRaw(int.Parse(sv[2])), FixedPoint.FromRaw(int.Parse(sv[3])), FixedPoint.FromRaw(int.Parse(sv[4])));
                        if (!eff.IsNone) pendingSubversion[cid] = eff;
                        int attempts = int.Parse(sv[5]);
                        if (attempts > 0) subversionAttempts[cid] = attempts;
                    }
                    catch (FormatException ex) { throw new SaveFormatException("人心杠杆段数值解析失败：" + ex.Message); }
                    idx++;
                }
                // 君主争霸段（GDD_017）。
                while (idx < lines.Length && lines[idx].StartsWith("power\t", StringComparison.Ordinal))
                {
                    string[] pw = lines[idx].Split('\t');
                    if (pw.Length != 3) throw new SaveFormatException($"争霸段格式不符：「{lines[idx]}」。");
                    try { powers.Add(new ThreeKingdom.Domain.Contention.PowerStanding(new FactionId(pw[1]), int.Parse(pw[2]))); }
                    catch (FormatException ex) { throw new SaveFormatException("争霸段数值解析失败：" + ex.Message); }
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
                pendingScouts: pendingScouts, pendingGovernance: pendingGovernance,
                offensiveAuthorization: new OffensiveAuthorization(authTargets),
                conquestCount: conquestCount, rebellionLean: rebellionLean,
                pool: pool, draft: draft, prepConfig: prepConfig,
                reachableRegions: reachableRegions, authorizedOrders: authorizedOrders, committedPlan: committed,
                battle: battle, battleConfig: battleConfig, battleSeed: battleSeed,
                tacticChains: tacticChains, battleConditions: battleConditions,
                lastOutcomeBranch: lastOutcomeBranch, lastOptions: lastOptions,
                pendingSubversion: pendingSubversion, subversionAttempts: subversionAttempts,
                contention: powers.Count > 0 ? new ThreeKingdom.Domain.Contention.ContentionState(powers) : null);
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

        /// <summary>
        /// 从配置初始情报播种一份<b>全新</b>玩家知识层（每局独立，防多局共用可变实例串知识）。
        /// 逐条重放初始报告以保留场景可能预置的情报；真值不在此（读只共享）。
        /// </summary>
        private static FactionIntel? CloneInitialIntel(FactionIntel? source)
        {
            if (source == null) return null;
            var fresh = new FactionIntel(source.Faction);
            foreach (IntelKnowledgeEntry e in source.Project().Entries)
                fresh.ApplyReport(new IntelReport(e.Subject, source.Faction, e.KnownStrength, e.Source, e.ObservedAt));
            return fresh;
        }

        /// <summary>解析在办治理段：<c>govtask\t{kind}\t{amount}\t{completionDay}\t{completionSegment}</c>。</summary>
        private static PendingGovernanceTask ParseGovTask(string line)
        {
            string[] p = line.Split('\t');
            if (p.Length != 5 || p[0] != "govtask")
                throw new SaveFormatException($"在办治理段格式不符：「{line}」。");
            try
            {
                return new PendingGovernanceTask(
                    (GovernanceActionKind)int.Parse(p[1]), long.Parse(p[2]),
                    new WorldTime(int.Parse(p[3]), (DaySegment)int.Parse(p[4])));
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("在办治理段数值解析失败：" + ex.Message);
            }
        }

        /// <summary>解析在途侦察段：<c>pendingscout\t{subject}\t{method}\t{arrivalDay}\t{arrivalSegment}</c>。</summary>
        private static PendingScout ParsePendingScout(string line)
        {
            string[] p = line.Split('\t');
            if (p.Length != 5 || p[0] != "pendingscout")
                throw new SaveFormatException($"在途侦察段格式不符：「{line}」。");
            try
            {
                return new PendingScout(
                    new IntelSubjectId(p[1]), (IntelSource)int.Parse(p[2]),
                    new WorldTime(int.Parse(p[3]), (DaySegment)int.Parse(p[4])));
            }
            catch (FormatException ex)
            {
                throw new SaveFormatException("在途侦察段数值解析失败：" + ex.Message);
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


