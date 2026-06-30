using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 太守开局的已校验配置（ADR-0009 §R-5 / ADR-0003）。不可变；构造校验范围，非法即抛、无部分写入。
    /// CampaignSession 经此<b>配置驱动</b>开局——**不以 <c>SliceScenario.Default()</c> 作为完整游戏唯一源**。
    /// 本类型为最小开局配置；完整多场景 ScenarioCatalog 属 epic-014（M01），届时由其产出本配置。
    /// </summary>
    public sealed class CampaignStartConfig
    {
        /// <summary>场景配置稳定 id（进入会话快照元数据，便于追溯）。</summary>
        public string ScenarioConfigId { get; }

        /// <summary>配置指纹（ADR-0003，进入快照、载入时校验）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>太守开局禀赋（复用 epic-011 CitySeed）。</summary>
        public CitySeed GovernorSeed { get; }

        /// <summary>开局世界时间。</summary>
        public WorldTime StartTime { get; }

        /// <summary>开局世界势力。</summary>
        public IReadOnlyList<FactionRecord> InitialFactions { get; }

        /// <summary>开局城池归属投影（只读，权威在 GDD_004）。</summary>
        public IReadOnlyList<CityOwnership> InitialCities { get; }

        /// <summary>开局城市治理态（GDD_004 / M03）。<b>可选</b>：null 表示该场景不启用城市治理循环。</summary>
        public CityEconomyState? CityEconomy { get; }

        /// <summary>城市日结配置（数据驱动，ADR-0003）；启用城市治理时必填。</summary>
        public CitySettlementConfig? SettlementConfig { get; }

        /// <summary>人口压力系数（GDD_004 §Formula 3）。</summary>
        public FixedPoint PopulationPressure { get; }

        /// <summary>开局后勤持有军粮（≥0）。</summary>
        public long InitialLogisticsHolding { get; }

        /// <summary>治理命令代价/增益配置（ADR-0003）；启用城市治理时必填。</summary>
        public CityGovernanceConfig? GovernanceConfig { get; }

        public CampaignStartConfig(
            string scenarioConfigId,
            ConfigFingerprint fingerprint,
            CitySeed governorSeed,
            WorldTime startTime,
            IReadOnlyList<FactionRecord> initialFactions,
            IReadOnlyList<CityOwnership> initialCities,
            CityEconomyState? cityEconomy = null,
            CitySettlementConfig? settlementConfig = null,
            FixedPoint populationPressure = default,
            long initialLogisticsHolding = 0,
            CityGovernanceConfig? governanceConfig = null)
        {
            if (string.IsNullOrWhiteSpace(scenarioConfigId))
                throw new ArgumentException("场景配置 id 不可为空或空白。", nameof(scenarioConfigId));
            if (initialLogisticsHolding < 0)
                throw new ArgumentOutOfRangeException(nameof(initialLogisticsHolding), "开局后勤持有不可为负。");
            if (cityEconomy != null && settlementConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 settlementConfig。", nameof(settlementConfig));
            if (cityEconomy != null && governanceConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 governanceConfig。", nameof(governanceConfig));
            ScenarioConfigId = scenarioConfigId;
            Fingerprint = fingerprint;
            GovernorSeed = governorSeed ?? throw new ArgumentNullException(nameof(governorSeed));
            StartTime = startTime;
            InitialFactions = initialFactions ?? throw new ArgumentNullException(nameof(initialFactions));
            InitialCities = initialCities ?? throw new ArgumentNullException(nameof(initialCities));
            CityEconomy = cityEconomy;
            SettlementConfig = settlementConfig;
            PopulationPressure = populationPressure;
            InitialLogisticsHolding = initialLogisticsHolding;
            GovernanceConfig = governanceConfig;
        }
    }
}
