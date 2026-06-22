using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报评估的版本化平衡配置（GDD_007 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围，非法即抛、无部分写入。来源可靠性映射/区间误差/时效 TTL/暴露曲线
    /// 均来自配置，逻辑不硬编码。置信/暴露全程定点（[0,1] 域），确定性（ADR-0004）。
    /// </summary>
    public sealed class IntelConfig
    {
        private readonly Dictionary<IntelSource, FixedPoint> _sourceReliability;

        /// <summary>区间误差基数 base_error（≥0，整数兵力单位）。</summary>
        public int BaseError { get; }

        /// <summary>主题时效 ttl（≥1 时段）：freshness = clamp(1 − age/ttl)。</summary>
        public int TtlSegments { get; }

        /// <summary>暴露基础概率 base_expose（[0,1]）。</summary>
        public FixedPoint BaseExposure { get; }

        /// <summary>暴露的警戒权重 k_alert（≥0）。</summary>
        public FixedPoint ExposureAlertWeight { get; }

        /// <summary>暴露的技能折减 k_skill（≥0）。</summary>
        public FixedPoint ExposureSkillWeight { get; }

        public IntelConfig(
            IReadOnlyDictionary<IntelSource, FixedPoint> sourceReliability,
            int baseError,
            int ttlSegments,
            FixedPoint baseExposure,
            FixedPoint exposureAlertWeight,
            FixedPoint exposureSkillWeight)
        {
            if (sourceReliability == null) throw new ArgumentNullException(nameof(sourceReliability));
            if (sourceReliability.Count == 0) throw new ArgumentException("来源可靠性映射不可为空。", nameof(sourceReliability));
            if (baseError < 0) throw new ArgumentOutOfRangeException(nameof(baseError), "区间误差基数不可为负。");
            if (ttlSegments < 1) throw new ArgumentOutOfRangeException(nameof(ttlSegments), "时效 TTL 须≥1。");
            RequireUnit(baseExposure, nameof(baseExposure));
            RequireNonNegative(exposureAlertWeight, nameof(exposureAlertWeight));
            RequireNonNegative(exposureSkillWeight, nameof(exposureSkillWeight));

            _sourceReliability = new Dictionary<IntelSource, FixedPoint>();
            foreach (KeyValuePair<IntelSource, FixedPoint> kv in sourceReliability)
            {
                RequireUnit(kv.Value, $"sourceReliability[{kv.Key}]");
                _sourceReliability[kv.Key] = kv.Value;
            }

            BaseError = baseError;
            TtlSegments = ttlSegments;
            BaseExposure = baseExposure;
            ExposureAlertWeight = exposureAlertWeight;
            ExposureSkillWeight = exposureSkillWeight;
        }

        /// <summary>来源可靠性 base_conf（来源可靠性，<b>非</b>真实概率）；未配置来源抛。</summary>
        public FixedPoint SourceReliability(IntelSource source)
        {
            if (!_sourceReliability.TryGetValue(source, out FixedPoint value))
                throw new KeyNotFoundException($"未配置来源 {source} 的可靠性。");
            return value;
        }

        private static void RequireUnit(FixedPoint value, string name)
        {
            if (value < FixedPoint.Zero || value > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "须在 [0,1]。");
        }

        private static void RequireNonNegative(FixedPoint value, string name)
        {
            if (value < FixedPoint.Zero)
                throw new ArgumentOutOfRangeException(name, "不可为负。");
        }
    }
}
