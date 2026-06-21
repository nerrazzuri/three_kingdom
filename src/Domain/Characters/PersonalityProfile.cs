using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>性格倾向（GDD_005：具名倾向，改变判断权重与意愿，不提供战斗光环）。</summary>
    public enum PersonalityTrait
    {
        /// <summary>风险偏好。</summary>
        Risk = 0,

        /// <summary>纪律观。</summary>
        Discipline = 1,

        /// <summary>荣誉观。</summary>
        Honor = 2,

        /// <summary>耐心。</summary>
        Patience = 3,
    }

    /// <summary>
    /// 性格档案（GDD_005 §Data Model：PersonalityProfile）。每倾向强度 ∈ [-1,1]（定点，ADR-0004）。
    /// 不可变；未声明倾向按中性 0。性格<b>只</b>影响判断权重/意愿（Story 002 消费），不提供无条件效果。
    /// </summary>
    public sealed class PersonalityProfile
    {
        private readonly Dictionary<PersonalityTrait, FixedPoint> _traits;

        /// <param name="traits">倾向 → 强度 [-1,1]。null 或缺项视为中性 0。</param>
        public PersonalityProfile(IReadOnlyDictionary<PersonalityTrait, FixedPoint>? traits)
        {
            _traits = new Dictionary<PersonalityTrait, FixedPoint>();
            if (traits != null)
            {
                foreach (var kv in traits)
                {
                    if (kv.Value < FixedPoint.FromInt(-1) || kv.Value > FixedPoint.One)
                        throw new ArgumentOutOfRangeException(nameof(traits), $"性格 {kv.Key} 强度越界 [-1,1]。");
                    _traits[kv.Key] = kv.Value;
                }
            }
        }

        /// <summary>取倾向强度；未声明则中性 0。</summary>
        public FixedPoint Strength(PersonalityTrait trait)
            => _traits.TryGetValue(trait, out var v) ? v : FixedPoint.Zero;
    }
}
