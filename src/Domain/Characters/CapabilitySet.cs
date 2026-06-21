using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>能力域（GDD_005：MVP 五能力域，影响过程质量，不解锁无条件技能）。</summary>
    public enum CapabilityDomain
    {
        /// <summary>统御。</summary>
        Command = 0,

        /// <summary>武勇。</summary>
        Valor = 1,

        /// <summary>谋略。</summary>
        Strategy = 2,

        /// <summary>治理。</summary>
        Governance = 3,

        /// <summary>交涉。</summary>
        Diplomacy = 4,
    }

    /// <summary>
    /// 人物能力集（GDD_005 §Data Model：CapabilitySet）。每能力域等级 0..<see cref="CapabilityMax"/>。
    /// 不可变；构造时校验全部五域齐备且在范围内（不变量保护，AC-1）。能力<b>只</b>经 <see cref="CharacterState"/>
    /// 产出过程质量系数，无任何「达标解锁」语义（AC-3）。
    /// </summary>
    public sealed class CapabilitySet
    {
        /// <summary>能力等级量表上限（结构性刻度，0..CapabilityMax）。</summary>
        public const int CapabilityMax = 100;

        private readonly Dictionary<CapabilityDomain, int> _levels;

        /// <param name="levels">五能力域 → 等级。须全部齐备，各 ∈ [0, CapabilityMax]。</param>
        public CapabilitySet(IReadOnlyDictionary<CapabilityDomain, int> levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            _levels = new Dictionary<CapabilityDomain, int>();
            foreach (CapabilityDomain domain in Enum.GetValues(typeof(CapabilityDomain)))
            {
                if (!levels.TryGetValue(domain, out int level))
                    throw new ArgumentException($"能力域缺失：{domain}。", nameof(levels));
                if (level < 0 || level > CapabilityMax)
                    throw new ArgumentOutOfRangeException(nameof(levels), $"能力 {domain}={level} 越界 [0,{CapabilityMax}]。");
                _levels[domain] = level;
            }
        }

        /// <summary>取能力域等级。</summary>
        public int Level(CapabilityDomain domain) => _levels[domain];
    }
}
