using System;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 完整游戏会话脊梁（ADR-0009）。**Application 装配层**：持有当前会话的 Domain 聚合引用与会话元数据，
    /// <b>只编排、不拥有玩法规则</b>（R-5：不计算玩法公式、不直接写 city.owner/势力存续）。
    /// 生涯/世界/控制权的权威仍在各 Domain 系统；本类型只读暴露其态、经服务路由变更。
    /// <para>
    /// 本 story（001）建骨架与配置驱动开局；日界推进（002）、后果原子写回（003）、统一存档（004）
    /// 由后续 story 在本脊梁上叠加，均经 <see cref="CampaignSessionService"/> 命令路径。
    /// </para>
    /// </summary>
    public sealed class CampaignSession
    {
        private readonly WorldCityProjection _worldProjection;

        /// <summary>会话稳定 id。</summary>
        public string Id { get; }

        /// <summary>场景配置 id（追溯）。</summary>
        public string ScenarioConfigId { get; }

        /// <summary>配置指纹（进入快照、载入校验）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>生涯快照（CareerState + RetinueState）。</summary>
        public CareerSnapshot Career { get; }

        /// <summary>世界态（含订阅 GDD_004 的归属投影）。</summary>
        public WorldState World => _worldProjection.Current;

        /// <summary>当前世界时间。</summary>
        public WorldTime CurrentTime => World.CurrentTime;

        /// <summary>城池控制权权威（GDD_004，唯一写归属点；本会话只读它/经它发起，不直接写）。</summary>
        internal ICityControlAuthority Control { get; }

        internal CampaignSession(
            string id, string scenarioConfigId, ConfigFingerprint fingerprint,
            CareerSnapshot career, WorldCityProjection worldProjection, ICityControlAuthority control)
        {
            Id = id;
            ScenarioConfigId = scenarioConfigId;
            Fingerprint = fingerprint;
            Career = career ?? throw new ArgumentNullException(nameof(career));
            _worldProjection = worldProjection ?? throw new ArgumentNullException(nameof(worldProjection));
            Control = control ?? throw new ArgumentNullException(nameof(control));
        }
    }
}
