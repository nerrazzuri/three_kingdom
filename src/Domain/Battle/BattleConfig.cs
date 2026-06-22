using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 战役解析的版本化平衡配置（GDD_010 §Balancing / ADR-0003 数据驱动）。
    /// 不可变；构造校验范围。突然性倍率/伤亡曲线来自配置，结算不硬编码。定点（ADR-0004）。
    /// </summary>
    public sealed class BattleConfig
    {
        /// <summary>突然性（伏击）倍率 ambush_multiplier（≥1）。</summary>
        public FixedPoint AmbushMultiplier { get; }

        /// <summary>伤亡曲线烈度 casualty_curve（[0,1]）。</summary>
        public FixedPoint CasualtyCurve { get; }

        public BattleConfig(FixedPoint ambushMultiplier, FixedPoint casualtyCurve)
        {
            if (ambushMultiplier < FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(ambushMultiplier), "突然性倍率须≥1。");
            if (casualtyCurve < FixedPoint.Zero || casualtyCurve > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(casualtyCurve), "伤亡曲线须在 [0,1]。");
            AmbushMultiplier = ambushMultiplier;
            CasualtyCurve = casualtyCurve;
        }
    }
}
