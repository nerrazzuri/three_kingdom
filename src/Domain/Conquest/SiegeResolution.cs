using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>守城方防御（GDD_019 攻城对象）：守军 + 工事因子（&gt;1 越难攻）。不可变。</summary>
    public sealed class SiegeDefense
    {
        /// <summary>守军兵力。</summary>
        public int Garrison { get; }
        /// <summary>工事因子（≥1；越高守方越强）。</summary>
        public FixedPoint FortFactor { get; }

        public SiegeDefense(int garrison, FixedPoint fortFactor)
        {
            if (garrison < 0) throw new ArgumentOutOfRangeException(nameof(garrison));
            Garrison = garrison;
            FortFactor = fortFactor;
        }
    }

    /// <summary>攻城结算配置（数据驱动）：每条满足兵法条件的战力加成。不可变。</summary>
    public sealed class SiegeResolutionConfig
    {
        /// <summary>每满足一条兵法条件的攻方战力加成（定点，≥0）。</summary>
        public FixedPoint ConditionBonusEach { get; }

        public SiegeResolutionConfig(FixedPoint conditionBonusEach) => ConditionBonusEach = conditionBonusEach;

        /// <summary>默认（每条兵法条件 +0.15 攻方战力）。</summary>
        public static SiegeResolutionConfig Default { get; } =
            new SiegeResolutionConfig(FixedPoint.FromFraction(15, 100));
    }

    /// <summary>
    /// 攻城胜负结算（GDD_019 R3/F1 闭合因果的收口，<b>确定性、无随机、无胜率</b>）：
    /// 攻方战力 = 兵力 × 士气 × (1 + 每条兵法条件加成 × 已成条件数)；守方战力 = 守军 × 工事因子。
    /// 攻方战力 &gt; 守方战力 → 破城。<b>准备越足（兵/粮/兵法条件）→ 攻方战力越高 → 越可能破城。</b>
    /// </summary>
    public sealed class SiegeResolutionService
    {
        /// <summary>攻方是否破城（准备决定胜负）。</summary>
        public bool AttackerWins(OffensiveForce attacker, SiegeDefense defense, SiegeResolutionConfig config)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defense == null) throw new ArgumentNullException(nameof(defense));
            if (config == null) throw new ArgumentNullException(nameof(config));

            FixedPoint conditionMultiplier = FixedPoint.One
                + config.ConditionBonusEach * FixedPoint.FromInt(attacker.Conditions.Count);
            FixedPoint attackerPower = FixedPoint.FromInt(attacker.Force) * attacker.Morale * conditionMultiplier;
            FixedPoint defenderPower = FixedPoint.FromInt(defense.Garrison) * defense.FortFactor;

            return attackerPower > defenderPower;
        }
    }
}
