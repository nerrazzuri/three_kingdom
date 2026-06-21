using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 人物核心权威状态（GDD_005 §Data Model：CharacterState / TR-character-001 / ADR-0002）。
    /// 持有身份、能力、性格、健康、职责；构造时校验不变量（AC-1）。不可变聚合——
    /// 状态变更经 Command 产生带原因的事件（变更路径在后续 story / 各消费 epic）。
    /// <para>
    /// 能力/性格/健康<b>只</b>产出过程质量与意愿的<b>修正系数</b>（<see cref="ComputeQuality"/>），
    /// <b>无</b>任何「能力达标即解锁无条件技能」的路径（AC-3 负向不变量：本类型不暴露技能解锁 API）。
    /// </para>
    /// </summary>
    public sealed class CharacterState
    {
        /// <summary>人物稳定 ID。</summary>
        public CharacterId Id { get; }

        /// <summary>身份（姓名/称号，非空）。</summary>
        public string Identity { get; }

        /// <summary>能力集。</summary>
        public CapabilitySet Capabilities { get; }

        /// <summary>性格档案。</summary>
        public PersonalityProfile Personality { get; }

        /// <summary>健康状态。</summary>
        public HealthState Health { get; }

        /// <summary>职责（合法权限由 Story 002 据此计算）。</summary>
        public RoleId Role { get; }

        public CharacterState(
            CharacterId id,
            string identity,
            CapabilitySet capabilities,
            PersonalityProfile personality,
            HealthState health,
            RoleId role)
        {
            if (string.IsNullOrWhiteSpace(identity)) throw new ArgumentException("身份不可为空。", nameof(identity));
            Id = id;
            Identity = identity;
            Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
            Personality = personality ?? throw new ArgumentNullException(nameof(personality));
            Health = health;
            Role = role;
        }

        /// <summary>
        /// 任务过程质量（GDD_005 §Formula 1：<c>quality = (Σ w_cap[k]·cap[k]/CAP_MAX) × health_factor</c>）。
        /// 以整数权重归一化 + 定点计算，结果 ∈ [0,1]，是<b>过程质量系数</b>而非成败开关或技能解锁。
        /// </summary>
        public FixedPoint ComputeQuality(TaskCapabilityWeights weights)
        {
            if (weights == null) throw new ArgumentNullException(nameof(weights));

            long weightedSum = 0; // Σ w[k]·cap[k]
            foreach (CapabilityDomain domain in Enum.GetValues(typeof(CapabilityDomain)))
                weightedSum += (long)weights.Weight(domain) * Capabilities.Level(domain);

            // 归一化基底 = Σ w[k]·cap[k] / (Total × CAP_MAX) ∈ [0,1]
            long denominator = (long)weights.Total * CapabilitySet.CapabilityMax;
            FixedPoint baseQuality = FixedPoint.FromFraction(checked((int)weightedSum), checked((int)denominator));

            return baseQuality * Health.Factor;
        }
    }
}
