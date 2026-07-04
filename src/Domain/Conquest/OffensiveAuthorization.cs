using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 君主授权出征（GDD_019 R1）：君主政令给定的可攻目标城集合（范围随官阶放宽，由外部按 Rank 组装）。不可变。
    /// </summary>
    public sealed class OffensiveAuthorization
    {
        private readonly HashSet<CityId> _targets;

        /// <summary>授权可攻的目标城（只读）。</summary>
        public IReadOnlyCollection<CityId> AuthorizedTargets => _targets;

        public OffensiveAuthorization(IReadOnlyCollection<CityId> targets)
            => _targets = targets == null ? new HashSet<CityId>() : new HashSet<CityId>(targets);

        /// <summary>无授权（未受命出征）。</summary>
        public static OffensiveAuthorization None { get; } = new OffensiveAuthorization(Array.Empty<CityId>());

        /// <summary>是否授权攻打某城。</summary>
        public bool Authorizes(CityId city) => _targets.Contains(city);
    }

    /// <summary>出征授权门判定结果（GDD_019 R1/R2）。</summary>
    public enum OffensiveGateResult
    {
        /// <summary>通过：授权 + 敌控城。</summary>
        Authorized = 0,
        /// <summary>未授权（不在君主政令目标内/越权）。</summary>
        NotAuthorized = 1,
        /// <summary>目标非敌方控制（无主/未登记）。</summary>
        NotEnemyControlled = 2,
        /// <summary>目标为己方城（不可攻）。</summary>
        OwnCity = 3,
    }

    /// <summary>
    /// 出征授权门（GDD_019 R1/R2，纯函数）：目标须在君主授权集内 + 为敌方控制城（非己方）。
    /// 目标合法性只读世界控制权投影（GDD_004/015），不读敌方战力真值（反全知）。
    /// </summary>
    public sealed class OffensiveAuthorizationService
    {
        /// <summary>判定能否对 <paramref name="target"/> 出征。<paramref name="currentOwner"/> 为该城当前控制方（null=无主/未登记）。</summary>
        public OffensiveGateResult Check(
            CityId target, OffensiveAuthorization authorization, FactionId? currentOwner, FactionId playerFaction)
        {
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));
            if (!authorization.Authorizes(target)) return OffensiveGateResult.NotAuthorized;
            if (currentOwner == null) return OffensiveGateResult.NotEnemyControlled;
            if (currentOwner.Value == playerFaction) return OffensiveGateResult.OwnCity;
            return OffensiveGateResult.Authorized;
        }
    }
}
