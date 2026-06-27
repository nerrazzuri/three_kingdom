using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立触发与结局结算服务（GDD_014 §Formula 2/3 / TR-career-002 / ADR-0004 + ADR-0003）。
    /// 纯函数、确定性、无随机：判定可否自立，发动时对好感取快照、据快照确定性产出三分支结局。
    /// </summary>
    public sealed class RebellionService
    {
        /// <summary>
        /// 自立可发动判定（三组条件独立）。avg(affinity)/loyal 比率用定点；N=0 时平均与比率取 0（不除零）。
        /// </summary>
        public RebellionEligibility CheckEligibility(
            RebellionConfig config, CareerState career, RetinueState retinue, RebellionContext context)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (career is null) throw new ArgumentNullException(nameof(career));
            if (retinue is null) throw new ArgumentNullException(nameof(retinue));

            bool group1 = context.CitiesOwned >= config.RebelCityMin && context.SupplyReady && context.TroopsReady;

            FixedPoint avgAffinity = AverageAffinity(retinue);
            bool group2 = career.Renown >= config.RebelRenownMin && avgAffinity >= config.RebelAffinityMin;

            bool group3 = context.LordOppression;

            return new RebellionEligibility(group1, group2, group3);
        }

        /// <summary>
        /// 发动自立。不满足条件 → <see cref="CareerErrorCode.RebellionConditionNotMet"/>、状态不变（无部分写入）。
        /// 满足 → 取好感快照、算 loyal_ratio、定分支、产出新生涯态 + <see cref="RebellionState"/>。
        /// </summary>
        /// <param name="context">发动输入；<see cref="RebellionContext.NewFactionId"/> 须非空（玩家发动前已命名新势力）。</param>
        public RebellionResult Launch(RebellionConfig config, CareerSnapshot snapshot, RebellionContext context)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
            if (context.NewFactionId is null)
                throw new ArgumentException("发动自立须提供新势力 id（NewFactionId）。", nameof(context));

            RebellionEligibility eligibility = CheckEligibility(config, snapshot.Career, snapshot.Retinue, context);
            if (!eligibility.CanRebel)
                return RebellionResult.Failure(snapshot, CareerErrorCode.RebellionConditionNotMet, "自立条件三组均未满足。");

            // 发动瞬间固化好感快照（快照隔离：此后好感变动不改已定分支）。
            var snapshotAffinities = new List<FixedPoint>(snapshot.Retinue.Members.Count);
            int loyalCount = 0;
            foreach (RetinueMember m in snapshot.Retinue.Members)
            {
                snapshotAffinities.Add(m.Affinity);
                if (m.Affinity >= config.DefectThreshold) loyalCount++;
            }

            int n = snapshotAffinities.Count;
            FixedPoint loyalRatio = n == 0 ? FixedPoint.Zero : FixedPoint.FromFraction(loyalCount, n);
            RebellionOutcome outcome = DecideOutcome(loyalRatio, config);

            CareerState newCareer;
            Map.FactionId? recordedFaction;
            if (outcome == RebellionOutcome.Abandoned)
            {
                newCareer = snapshot.Career.IntoWandering();   // 流浪势力，合法可继续
                recordedFaction = null;
            }
            else
            {
                newCareer = snapshot.Career.IntoOwnFaction(context.NewFactionId.Value);
                recordedFaction = context.NewFactionId.Value;
            }

            var rebellion = new RebellionState(snapshotAffinities, loyalRatio, outcome, recordedFaction);
            var newSnapshot = new CareerSnapshot(newCareer, snapshot.Retinue);
            return RebellionResult.Success(newSnapshot, rebellion);
        }

        private static RebellionOutcome DecideOutcome(FixedPoint loyalRatio, RebellionConfig config)
        {
            if (loyalRatio >= config.LoyalRatioHi) return RebellionOutcome.FullSupport;
            if (loyalRatio >= config.LoyalRatioMid) return RebellionOutcome.PartialFollow;
            return RebellionOutcome.Abandoned;
        }

        private static FixedPoint AverageAffinity(RetinueState retinue)
        {
            int n = retinue.Members.Count;
            if (n == 0) return FixedPoint.Zero;
            FixedPoint sum = FixedPoint.Zero;
            foreach (RetinueMember m in retinue.Members) sum += m.Affinity;
            return sum / FixedPoint.FromInt(n);
        }
    }
}
