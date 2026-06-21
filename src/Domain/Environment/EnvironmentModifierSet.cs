using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Environment
{
    /// <summary>
    /// 环境修正的消费者（GDD_002 §Formula 6）。环境只对这些<b>具名</b>消费者产出修正，
    /// 各消费者在自身结算时自取——环境<b>不直接触发「计策成功」</b>（无此消费者，结构性保证）。
    /// </summary>
    public enum EnvironmentConsumer
    {
        /// <summary>移动耗时。</summary>
        Movement = 0,

        /// <summary>侦察范围/精度。</summary>
        Scouting = 1,

        /// <summary>隐蔽。</summary>
        Stealth = 2,

        /// <summary>疲劳累积。</summary>
        Fatigue = 3,

        /// <summary>补给损耗。</summary>
        Supply = 4,
    }

    /// <summary>单个消费者的具名修正：乘数 + 偏移（GDD §6：value = base × mult + offset）。</summary>
    public readonly struct EnvironmentModifier
    {
        /// <summary>乘数。</summary>
        public FixedPoint Multiplier { get; }

        /// <summary>偏移。</summary>
        public FixedPoint Offset { get; }

        public EnvironmentModifier(FixedPoint multiplier, FixedPoint offset)
        {
            Multiplier = multiplier;
            Offset = offset;
        }
    }

    /// <summary>
    /// 已结算的环境具名修正集（GDD_002 §Data Model：EnvironmentModifierSet）。只读，供消费者自取。
    /// 不变式：<see cref="EnvironmentConsumer.Movement"/> 乘数 ≥ 1.0——<b>天气只减速</b>（对齐 GDD_003 / systems-index C-W3）。
    /// 缺省消费者按中性处理（乘数 1、偏移 0）。
    /// </summary>
    public sealed class EnvironmentModifierSet
    {
        private readonly Dictionary<EnvironmentConsumer, EnvironmentModifier> _modifiers;

        /// <param name="modifiers">消费者 → 修正。Movement 乘数若 &lt;1.0 抛 <see cref="ArgumentException"/>（天气只减速）。</param>
        public EnvironmentModifierSet(IReadOnlyDictionary<EnvironmentConsumer, EnvironmentModifier> modifiers)
        {
            _modifiers = new Dictionary<EnvironmentConsumer, EnvironmentModifier>();
            if (modifiers != null)
            {
                foreach (var kv in modifiers)
                {
                    if (kv.Key == EnvironmentConsumer.Movement && kv.Value.Multiplier < FixedPoint.One)
                        throw new ArgumentException("天气对移动只减速：Movement 乘数须 ≥ 1.0。", nameof(modifiers));
                    _modifiers[kv.Key] = kv.Value;
                }
            }
        }

        /// <summary>取消费者乘数；未声明则中性 1.0。</summary>
        public FixedPoint Multiplier(EnvironmentConsumer consumer)
            => _modifiers.TryGetValue(consumer, out var m) ? m.Multiplier : FixedPoint.One;

        /// <summary>取消费者偏移；未声明则 0。</summary>
        public FixedPoint Offset(EnvironmentConsumer consumer)
            => _modifiers.TryGetValue(consumer, out var m) ? m.Offset : FixedPoint.Zero;

        /// <summary>对基础值应用具名修正：base × mult + offset（GDD §6）。</summary>
        public FixedPoint Apply(EnvironmentConsumer consumer, FixedPoint baseValue)
            => baseValue * Multiplier(consumer) + Offset(consumer);

        /// <summary>
        /// 将乘数夹取到 [1.0, <paramref name="cap"/>]（GDD §Edge：极端天气上限夹取）。供 Movement 等只减速消费者构建。
        /// </summary>
        public static FixedPoint ClampSlowdown(FixedPoint multiplier, FixedPoint cap)
        {
            if (cap < FixedPoint.One) throw new ArgumentOutOfRangeException(nameof(cap), "上限须 ≥ 1.0。");
            if (multiplier < FixedPoint.One) return FixedPoint.One;
            if (multiplier > cap) return cap;
            return multiplier;
        }
    }
}
