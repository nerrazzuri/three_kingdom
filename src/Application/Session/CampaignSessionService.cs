using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
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
                governanceConfig: config.GovernanceConfig);

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

            return head + BodyMarker + "\n" + body;
        }

        /// <summary>
        /// 从快照恢复会话（ADR-0009 §R-1 / TR-session-003）。版本不兼容/指纹不符 → 整体抛 <see cref="SaveFormatException"/>，
        /// <b>不部分载入</b>。重建 GDD_004 权威（登记开局归属）+ GDD_015 投影订阅。
        /// </summary>
        public CampaignSession Restore(
            string text, ConfigFingerprint expectedFingerprint,
            CitySettlementConfig? settlementConfig = null, CityGovernanceConfig? governanceConfig = null,
            FixedPoint populationPressure = default)
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
            if (lines[idx].StartsWith("city\t", StringComparison.Ordinal))
            {
                (city, cityLogistics) = ParseCity(lines[idx]);
                idx++;
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

            return new CampaignSession(
                id, scenario, expectedFingerprint, state.Career.Snapshot, projection, authority,
                cityEconomy: city, settlementConfig: settlementConfig, populationPressure: populationPressure,
                logisticsHolding: cityLogistics, governanceConfig: governanceConfig);
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


