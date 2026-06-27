using System;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 守城开局事件的战役结果（GDD_014 §Main Rules）。本层只消费 GDD_010 BattleOutcome 的胜负摘要
    /// （战役本身解算属 epic-007/008，Out of Scope）。
    /// </summary>
    public enum SiegeOutcome
    {
        /// <summary>守住（胜）。</summary>
        Defended = 0,

        /// <summary>失守（败）。</summary>
        Fallen = 1,
    }

    /// <summary>
    /// 守城开局事件胜利后果的版本化配置（GDD_014 / ADR-0003）。开局禀赋不硬编码。
    /// </summary>
    public sealed class GovernorStartConfig
    {
        /// <summary>守城胜的初始功绩（≥0）。</summary>
        public int InitialMerit { get; }

        /// <summary>守城胜的君主初始信任（lord_standing，定点 ∈[0,1]）。</summary>
        public FixedPoint InitialLordStanding { get; }

        public GovernorStartConfig(int initialMerit, FixedPoint initialLordStanding)
        {
            if (initialMerit < 0) throw new ArgumentOutOfRangeException(nameof(initialMerit), "初始功绩不可为负。");
            if (initialLordStanding < FixedPoint.Zero || initialLordStanding > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(initialLordStanding), "初始君主信任须在 [0,1]。");
            InitialMerit = initialMerit;
            InitialLordStanding = initialLordStanding;
        }
    }

    /// <summary>
    /// 守城结算的外部输入（失守时归属变更目标）。失守城池经 GDD_004 控制权变更事件转给 <see cref="EnemyFaction"/>。
    /// </summary>
    public readonly struct SiegeContext
    {
        /// <summary>受围城池。</summary>
        public CityId City { get; }

        /// <summary>失守时的夺城方势力。</summary>
        public FactionId EnemyFaction { get; }

        /// <summary>失守后该城守备（夺城方进驻）。</summary>
        public Garrison EnemyGarrison { get; }

        public SiegeContext(CityId city, FactionId enemyFaction, Garrison enemyGarrison)
        {
            City = city;
            EnemyFaction = enemyFaction;
            EnemyGarrison = enemyGarrison;
        }
    }
}
