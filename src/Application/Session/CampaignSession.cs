using System;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
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

        /// <summary>生涯快照（CareerState + RetinueState）。变更只经 <see cref="CampaignSessionService"/> 命令路径。</summary>
        public CareerSnapshot Career { get; private set; }

        /// <summary>世界态（含订阅 GDD_004 的归属投影）。</summary>
        public WorldState World => _worldProjection.Current;

        /// <summary>当前世界时间。</summary>
        public WorldTime CurrentTime => World.CurrentTime;

        /// <summary>城池控制权权威（GDD_004，唯一写归属点；本会话只读它/经它发起，不直接写）。</summary>
        internal ICityControlAuthority Control { get; }

        /// <summary>
        /// 城市治理态（GDD_004 / M03）。<b>可选</b>：场景未启用城市治理时为 null（无日界城市结算、不入哈希）。
        /// 变更只经 <see cref="CampaignSessionService"/> 编排既有 <see cref="CityDaySettlementService"/> 产出。
        /// </summary>
        public CityEconomyState? CityEconomy { get; private set; }

        /// <summary>后勤持有的军粮总量（征用移交后单一计入此处，城市不再计；守恒 TR-city-001）。</summary>
        public long LogisticsHolding { get; private set; }

        /// <summary>是否启用城市治理循环（城市态存在）。</summary>
        public bool HasCityGovernance => CityEconomy != null;

        /// <summary>城市日结配置（数据驱动，ADR-0003）；仅供服务编排日结，城市治理未启用时为 null。</summary>
        internal CitySettlementConfig? SettlementConfig { get; }

        /// <summary>人口压力系数（GDD_004 §Formula 3，喂给日结民用需求）。</summary>
        internal FixedPoint PopulationPressure { get; }

        /// <summary>治理命令代价/增益配置（数据驱动，ADR-0003）；启用城市治理时必填。</summary>
        internal CityGovernanceConfig? GovernanceConfig { get; }

        internal CampaignSession(
            string id, string scenarioConfigId, ConfigFingerprint fingerprint,
            CareerSnapshot career, WorldCityProjection worldProjection, ICityControlAuthority control,
            CityEconomyState? cityEconomy = null, CitySettlementConfig? settlementConfig = null,
            FixedPoint populationPressure = default, long logisticsHolding = 0,
            CityGovernanceConfig? governanceConfig = null)
        {
            if (logisticsHolding < 0) throw new ArgumentOutOfRangeException(nameof(logisticsHolding), "后勤持有量不可为负。");
            if (cityEconomy != null && settlementConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 settlementConfig。", nameof(settlementConfig));
            if (cityEconomy != null && governanceConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 governanceConfig。", nameof(governanceConfig));

            Id = id;
            ScenarioConfigId = scenarioConfigId;
            Fingerprint = fingerprint;
            Career = career ?? throw new ArgumentNullException(nameof(career));
            _worldProjection = worldProjection ?? throw new ArgumentNullException(nameof(worldProjection));
            Control = control ?? throw new ArgumentNullException(nameof(control));
            CityEconomy = cityEconomy;
            SettlementConfig = settlementConfig;
            PopulationPressure = populationPressure;
            LogisticsHolding = logisticsHolding;
            GovernanceConfig = governanceConfig;
        }

        /// <summary>日界推进世界时间（仅供 <see cref="CampaignSessionService"/> 按全局结算顺序编排调用）。</summary>
        internal void AdvanceWorld(int segments) => _worldProjection.AdvanceTime(segments);

        /// <summary>
        /// 应用一次城市日界结算结果（仅供 <see cref="CampaignSessionService"/> 编排 <see cref="CityDaySettlementService"/> 后写回）。
        /// 装配层不算公式，只写回 Domain 服务产出的新态与后勤持有量。
        /// </summary>
        internal void ApplyCitySettlement(CityEconomyState endState, long endLogisticsHolding)
        {
            CityEconomy = endState ?? throw new ArgumentNullException(nameof(endState));
            LogisticsHolding = endLogisticsHolding;
        }

        /// <summary>替换城市治理态（仅供治理命令编排，M03 story-002）。</summary>
        internal void SetCityEconomy(CityEconomyState state)
            => CityEconomy = state ?? throw new ArgumentNullException(nameof(state));

        // --- 仅供 ConsequenceTransaction 原子写回 / 回滚使用（R-6）---
        internal void SetCareer(CareerSnapshot career) => Career = career ?? throw new ArgumentNullException(nameof(career));
        internal void CreateFaction(Domain.World.FactionRecord faction) => _worldProjection.CreateFaction(faction);
        internal void RestoreWorld(WorldState world) => _worldProjection.RestoreTo(world);

        /// <summary>会话权威态的确定性哈希（生涯 ⊕ 世界 ⊕ 城市治理）——支撑确定性回归与存档校验。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            Career.AppendTo(hasher);
            World.AppendTo(hasher);
            if (CityEconomy != null)
            {
                CityEconomy.AppendTo(hasher);
                hasher.Append(LogisticsHolding);
            }
            return hasher.ToHash();
        }
    }
}
