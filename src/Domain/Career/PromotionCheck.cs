using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 晋升门槛判定结果（GDD_014 §Formula 1：三项独立可验 / §UI「距下一阶差距」）。不可变。
    /// 三项（merit/renown/lord_standing）各自独立标注是否达标与缺口，供 UI 显示距下一阶差距。
    /// </summary>
    public sealed class PromotionCheck
    {
        /// <summary>是否可晋升（三项全达标且非最高阶且非在野）。</summary>
        public bool CanPromote { get; }

        /// <summary>目标官阶（当前阶+1；已最高阶时等于当前阶）。</summary>
        public Rank TargetRank { get; }

        /// <summary>功绩是否达标。</summary>
        public bool MeritMet { get; }

        /// <summary>名望是否达标。</summary>
        public bool RenownMet { get; }

        /// <summary>君主好感是否达标。</summary>
        public bool StandingMet { get; }

        /// <summary>功绩缺口（未达为正数，达标为 0）。</summary>
        public int MeritShortfall { get; }

        /// <summary>名望缺口（未达为正数，达标为 0）。</summary>
        public int RenownShortfall { get; }

        /// <summary>君主好感缺口（定点，未达为正，达标为 0）。</summary>
        public FixedPoint StandingShortfall { get; }

        /// <summary>是否因在野/已最高阶等结构原因不可晋升（非门槛缺口）。</summary>
        public bool Blocked { get; }

        public PromotionCheck(
            bool canPromote, Rank targetRank,
            bool meritMet, bool renownMet, bool standingMet,
            int meritShortfall, int renownShortfall, FixedPoint standingShortfall,
            bool blocked)
        {
            CanPromote = canPromote;
            TargetRank = targetRank;
            MeritMet = meritMet;
            RenownMet = renownMet;
            StandingMet = standingMet;
            MeritShortfall = meritShortfall;
            RenownShortfall = renownShortfall;
            StandingShortfall = standingShortfall;
            Blocked = blocked;
        }
    }
}
