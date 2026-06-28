using System;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
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
    }
}
