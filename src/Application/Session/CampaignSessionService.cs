using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
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
                control: authority);

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

            // 时间 → 历史世界模型（015）：确定性推进，世界读已结算态（ADR-0004）。
            session.AdvanceWorld(segments);
            return session;
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
            return SnapshotMagic + "\nid\t" + session.Id + "\nscenario\t" + session.ScenarioConfigId + "\n" + BodyMarker + "\n" + body;
        }

        /// <summary>
        /// 从快照恢复会话（ADR-0009 §R-1 / TR-session-003）。版本不兼容/指纹不符 → 整体抛 <see cref="SaveFormatException"/>，
        /// <b>不部分载入</b>。重建 GDD_004 权威（登记开局归属）+ GDD_015 投影订阅。
        /// </summary>
        public CampaignSession Restore(string text, ConfigFingerprint expectedFingerprint)
        {
            if (text is null) throw new SaveFormatException("会话存档文本为 null。");
            string[] lines = text.Split('\n');
            if (lines.Length < 4 || lines[0] != SnapshotMagic) throw new SaveFormatException("会话存档魔数不符。");
            string id = Field("id", lines[1]);
            string scenario = Field("scenario", lines[2]);
            if (lines[3] != BodyMarker) throw new SaveFormatException("缺会话体标记。");
            string body = string.Join("\n", new ArraySegment<string>(lines, 4, lines.Length - 4));

            // 委派 CampaignSaveCodec：版本/指纹不符整体拒绝。
            CampaignSaveState state = _saveCodec.Deserialize(body, CampaignSaveVersion, expectedFingerprint);

            WorldState world = state.World.World;
            var authority = new CityControlAuthority();
            foreach (CityOwnership c in world.Cities)
                if (c.Owner.HasValue) authority.RegisterInitial(c.City, c.Owner.Value, new Garrison(c.Garrison));
            var projection = new WorldCityProjection(world, authority);

            return new CampaignSession(id, scenario, expectedFingerprint, state.Career.Snapshot, projection, authority);
        }

        private static string Field(string key, string line)
        {
            string[] p = line.Split('\t');
            if (p.Length < 2 || p[0] != key) throw new SaveFormatException($"期望字段「{key}」，实得「{line}」。");
            return p[1];
        }
    }
}


