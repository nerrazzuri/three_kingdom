using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Configuration;
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

        public CampaignStartConfig(
            string scenarioConfigId,
            ConfigFingerprint fingerprint,
            CitySeed governorSeed,
            WorldTime startTime,
            IReadOnlyList<FactionRecord> initialFactions,
            IReadOnlyList<CityOwnership> initialCities)
        {
            if (string.IsNullOrWhiteSpace(scenarioConfigId))
                throw new ArgumentException("场景配置 id 不可为空或空白。", nameof(scenarioConfigId));
            ScenarioConfigId = scenarioConfigId;
            Fingerprint = fingerprint;
            GovernorSeed = governorSeed ?? throw new ArgumentNullException(nameof(governorSeed));
            StartTime = startTime;
            InitialFactions = initialFactions ?? throw new ArgumentNullException(nameof(initialFactions));
            InitialCities = initialCities ?? throw new ArgumentNullException(nameof(initialCities));
        }
    }
}
