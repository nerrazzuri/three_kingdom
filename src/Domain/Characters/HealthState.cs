using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>健康等级（GDD_005 §Formula 3）。失能中断相关任务。</summary>
    public enum HealthLevel
    {
        /// <summary>健康（折损因子通常 1.0）。</summary>
        Healthy = 0,

        /// <summary>轻伤（折损因子 &lt;1.0）。</summary>
        Injured = 1,

        /// <summary>失能（折损因子 0，能力贡献归零）。</summary>
        Incapacitated = 2,
    }

    /// <summary>
    /// 健康状态（GDD_005 §Data Model：HealthState / §Formula 3：health_factor）。
    /// 折损因子 ∈ [0,1]（配置 health_level_multiplier，定点）。<see cref="HealthLevel.Incapacitated"/> 因子必须为 0。
    /// 不可变值。
    /// </summary>
    public readonly struct HealthState
    {
        /// <summary>健康等级。</summary>
        public HealthLevel Level { get; }

        /// <summary>对执行的折损因子 ∈ [0,1]（来自配置）。</summary>
        public FixedPoint Factor { get; }

        public HealthState(HealthLevel level, FixedPoint factor)
        {
            if (!Enum.IsDefined(typeof(HealthLevel), level))
                throw new ArgumentOutOfRangeException(nameof(level), "未定义的健康等级。");
            if (factor < FixedPoint.Zero || factor > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(factor), "健康折损因子须 ∈ [0,1]。");
            if (level == HealthLevel.Incapacitated && factor != FixedPoint.Zero)
                throw new ArgumentException("失能等级的折损因子必须为 0。", nameof(factor));
            Level = level;
            Factor = factor;
        }

        /// <summary>是否可执行（折损因子 &gt; 0；GDD §Formula 4 can_assign 前提）。</summary>
        public bool CanAct => Factor > FixedPoint.Zero;

        public override string ToString() => $"{Level}({Factor})";
    }
}
