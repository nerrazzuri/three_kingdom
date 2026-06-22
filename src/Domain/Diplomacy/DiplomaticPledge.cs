using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 外交承诺（GDD_012 §Data Model：DiplomaticPledge / §8.1–8.2）。
    /// <see cref="DiplomacyService.Evaluate"/> 的确定性结构化输出：响应、grant_score、
    /// 延迟交付到达时间、背约风险与承诺代价。不可变。<b>非即到保证</b>——接受也须经
    /// 延迟（<see cref="ArrivalTime"/>）与兑现判定（<see cref="DiplomacyService.Resolve"/>）。
    /// </summary>
    public sealed class DiplomaticPledge
    {
        /// <summary>原始请求。</summary>
        public DiplomaticRequest Request { get; }

        /// <summary>响应判定结果（接受/附条件/拒绝）。</summary>
        public DiplomaticResponse Response { get; }

        /// <summary>响应评分 grant_score（[0,1]，确定性）。</summary>
        public FixedPoint GrantScore { get; }

        /// <summary>交付时段数 commit_lead。</summary>
        public int CommitLeadSegments { get; }

        /// <summary>到达时间 arrival_T（仅接受时非空；外援不即时）。</summary>
        public WorldTime? ArrivalTime { get; }

        /// <summary>背约风险 betray_risk（[0,1]，兑现检查点比较）。</summary>
        public FixedPoint BetrayRisk { get; }

        /// <summary>接受时承诺交付的规模（援军/补给/时限缩减；拒绝/附条件为 0）。</summary>
        public long DeliveredAmount { get; }

        public DiplomaticPledge(
            DiplomaticRequest request,
            DiplomaticResponse response,
            FixedPoint grantScore,
            int commitLeadSegments,
            WorldTime? arrivalTime,
            FixedPoint betrayRisk,
            long deliveredAmount)
        {
            Request = request;
            Response = response;
            GrantScore = grantScore;
            CommitLeadSegments = commitLeadSegments;
            ArrivalTime = arrivalTime;
            BetrayRisk = betrayRisk;
            DeliveredAmount = deliveredAmount;
        }
    }
}
