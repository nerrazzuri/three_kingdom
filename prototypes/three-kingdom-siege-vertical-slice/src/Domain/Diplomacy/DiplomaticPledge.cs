// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 外援延迟交付、有代价、可背约——绝非即到保证按钮（GDD_012 §8，支柱4）
// Date: 2026-06-21

using TkSlice.Domain.Config;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Time;

namespace TkSlice.Domain.Diplomacy
{
    public enum PledgeType { Relief, Supply, Deadline }   // 求援 / 求粮 / 求时限
    public enum PledgeResponse { Accept, Conditional, Reject }
    public enum PledgeStatus { Pending, Fulfilled, Betrayed }

    /// <summary>
    /// 一次外交受控请求的权威记录（GDD_012 §8）。外势力为静态背景，非完整天下外交模拟。
    /// </summary>
    public sealed class DiplomaticPledge
    {
        public PledgeType Type { get; }
        public Fixed PledgeCost { get; }     // 承诺代价 [0,1]
        public Fixed GrantScore { get; }
        public PledgeResponse Response { get; }
        public WorldDay ArrivalT { get; }    // 交付时刻（now + commit_lead）
        public Fixed BetrayRisk { get; }
        public PledgeStatus Status { get; private set; } = PledgeStatus.Pending;

        public DiplomaticPledge(PledgeType type, Fixed pledgeCost, Fixed grantScore,
            PledgeResponse response, WorldDay arrivalT, Fixed betrayRisk)
        {
            Type = type; PledgeCost = pledgeCost; GrantScore = grantScore;
            Response = response; ArrivalT = arrivalT; BetrayRisk = betrayRisk;
        }

        public void MarkFulfilled() => Status = PledgeStatus.Fulfilled;
        public void MarkBetrayed() => Status = PledgeStatus.Betrayed;

        public static string TypeName(PledgeType t) => t switch
        {
            PledgeType.Relief => "求援（援军）",
            PledgeType.Supply => "求粮（补给）",
            PledgeType.Deadline => "求时限（外交斡旋）",
            _ => "?"
        };
    }

    /// <summary>外交响应判定（GDD_012 §8.1–8.2）。纯函数，确定性。</summary>
    public static class DiplomacyEvaluator
    {
        /// <summary>grant_score = clamp(base + w_s·standing + w_c·cost − w_p·pressure, 0, 1)。</summary>
        public static Fixed GrantScore(SiegeConfig cfg, Fixed standing, Fixed pledgeCost, Fixed diplPressure)
        {
            Fixed s = cfg.DiplomacyBaseGrant
                + cfg.DiplomacyStandingWeight * standing
                + cfg.DiplomacyCostWeight * pledgeCost
                - Fixed.FromFraction(20, 100) * diplPressure;
            return Fixed.Clamp(s, Fixed.Zero, Fixed.OneValue);
        }

        public static PledgeResponse Respond(SiegeConfig cfg, Fixed grantScore)
        {
            if (grantScore >= cfg.DiplomacyAcceptThreshold) return PledgeResponse.Accept;
            // 接受阈值下方一段为「附条件」（需更多代价），再下为拒绝
            if (grantScore >= cfg.DiplomacyAcceptThreshold - Fixed.FromFraction(15, 100))
                return PledgeResponse.Conditional;
            return PledgeResponse.Reject;
        }

        /// <summary>构造一次已评估的请求（接受时排定延迟交付）。</summary>
        public static DiplomaticPledge Create(SiegeConfig cfg, PledgeType type,
            Fixed standing, Fixed pledgeCost, Fixed diplPressure, WorldDay now)
        {
            Fixed grant = GrantScore(cfg, standing, pledgeCost, diplPressure);
            PledgeResponse resp = Respond(cfg, grant);
            WorldDay arrival = now.Advance(cfg.DiplomacyCommitLead);  // §8.2 绝非即到
            return new DiplomaticPledge(type, pledgeCost, grant, resp, arrival, cfg.DiplomacyBetrayRisk);
        }
    }
}
