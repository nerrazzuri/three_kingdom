using System;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 后果结算的数据驱动幅度配置（gdd-010 §后果 / coding-standards「平衡值数据驱动不硬编码」）。
    /// 各字段为<b>非负幅度</b>；写回时由 <see cref="FailureContinuationService"/> 按分支取负并以当前值上限夹取
    /// （不会写出负值，故不破坏原子写回的不变量）。运行期不可变。
    /// </summary>
    public sealed class OutcomeConsequenceConfig
    {
        /// <summary>败北分支的名声损失幅度。</summary>
        public long ReputationLossDefeat { get; }

        /// <summary>撤退分支的名声损失幅度（保存实力，损失较小）。</summary>
        public long ReputationLossRetreat { get; }

        /// <summary>失城分支的名声损失幅度（据点易手，损失较大）。</summary>
        public long ReputationLossCityLost { get; }

        /// <summary>失利时城市民心损失幅度。</summary>
        public long CivMoraleLoss { get; }

        /// <summary>失利时城市治安损失幅度（失城尤甚）。</summary>
        public long SecurityLoss { get; }

        /// <summary>失利时工事损毁幅度。</summary>
        public long FortificationDamage { get; }

        /// <summary>撤退/败北时部队减员幅度（人物计量损失）。</summary>
        public long ForceAttrition { get; }

        public OutcomeConsequenceConfig(
            long reputationLossDefeat,
            long reputationLossRetreat,
            long reputationLossCityLost,
            long civMoraleLoss,
            long securityLoss,
            long fortificationDamage,
            long forceAttrition)
        {
            if (reputationLossDefeat < 0) throw new ArgumentOutOfRangeException(nameof(reputationLossDefeat));
            if (reputationLossRetreat < 0) throw new ArgumentOutOfRangeException(nameof(reputationLossRetreat));
            if (reputationLossCityLost < 0) throw new ArgumentOutOfRangeException(nameof(reputationLossCityLost));
            if (civMoraleLoss < 0) throw new ArgumentOutOfRangeException(nameof(civMoraleLoss));
            if (securityLoss < 0) throw new ArgumentOutOfRangeException(nameof(securityLoss));
            if (fortificationDamage < 0) throw new ArgumentOutOfRangeException(nameof(fortificationDamage));
            if (forceAttrition < 0) throw new ArgumentOutOfRangeException(nameof(forceAttrition));

            ReputationLossDefeat = reputationLossDefeat;
            ReputationLossRetreat = reputationLossRetreat;
            ReputationLossCityLost = reputationLossCityLost;
            CivMoraleLoss = civMoraleLoss;
            SecurityLoss = securityLoss;
            FortificationDamage = fortificationDamage;
            ForceAttrition = forceAttrition;
        }
    }
}
