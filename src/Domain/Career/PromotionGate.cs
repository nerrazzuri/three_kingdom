using System;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 晋升门槛判定（GDD_014 §Formula 1 / TR-career-002 / ADR-0004）。纯函数、确定性、无随机。
    /// 读 <see cref="CareerState"/> + <see cref="PromotionLadderConfig"/>，逐项独立判定，产出 <see cref="PromotionCheck"/>。
    /// </summary>
    public sealed class PromotionGate
    {
        /// <summary>
        /// 判定能否从当前阶晋升至下一阶。在野或已最高阶 → <see cref="PromotionCheck.Blocked"/>=true、不可晋升。
        /// </summary>
        public PromotionCheck Evaluate(CareerState career, PromotionLadderConfig config)
        {
            if (career is null) throw new ArgumentNullException(nameof(career));
            if (config is null) throw new ArgumentNullException(nameof(config));

            // 结构性阻断：在野无君主、或已达最高阶，无门槛缺口语义。
            if (career.IsUnaffiliated || career.Rank == Rank.Successor)
            {
                return new PromotionCheck(
                    canPromote: false, targetRank: career.Rank,
                    meritMet: false, renownMet: false, standingMet: false,
                    meritShortfall: 0, renownShortfall: 0, standingShortfall: Numerics.FixedPoint.Zero,
                    blocked: true);
            }

            var target = (Rank)((int)career.Rank + 1);
            int meritReq = config.MeritReq[(int)target];
            int renownReq = config.RenownReq[(int)target];
            Numerics.FixedPoint standingReq = config.StandingReq[(int)target];

            bool meritMet = career.Merit >= meritReq;
            bool renownMet = career.Renown >= renownReq;
            bool standingMet = career.LordStanding >= standingReq;

            int meritShort = meritMet ? 0 : meritReq - career.Merit;
            int renownShort = renownMet ? 0 : renownReq - career.Renown;
            Numerics.FixedPoint standingShort = standingMet ? Numerics.FixedPoint.Zero : standingReq - career.LordStanding;

            bool canPromote = meritMet && renownMet && standingMet;
            return new PromotionCheck(
                canPromote, target,
                meritMet, renownMet, standingMet,
                meritShort, renownShort, standingShort,
                blocked: false);
        }
    }
}
