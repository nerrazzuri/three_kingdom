using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 一条兵法条件链定义（GDD_010：可观察线索/必要条件的数据驱动描述）。
    /// 标签 + 必要条件集——<b>全部</b>成立方涌现。不可变。
    /// </summary>
    public sealed class TacticChainDefinition
    {
        /// <summary>复盘标签。</summary>
        public TacticTag Tag { get; }

        /// <summary>必要条件（全部成立才涌现；缺一不可）。</summary>
        public IReadOnlyList<TacticCondition> Required { get; }

        public TacticChainDefinition(TacticTag tag, IReadOnlyList<TacticCondition> required)
        {
            if (required == null || required.Count == 0)
                throw new ArgumentException("必要条件不可为空（兵法须由条件涌现，不存在无条件兵法）。", nameof(required));
            Tag = tag;
            Required = required;
        }
    }

    /// <summary>
    /// 兵法条件链配置（GDD_010 §MVP / ADR-0003 数据驱动）。
    /// 链定义来自配置，识别逻辑不硬编码具体兵法。不可变。
    /// </summary>
    public sealed class TacticChainConfig
    {
        /// <summary>全部链定义。</summary>
        public IReadOnlyList<TacticChainDefinition> Chains { get; }

        public TacticChainConfig(IReadOnlyList<TacticChainDefinition> chains)
            => Chains = chains ?? throw new ArgumentNullException(nameof(chains));

        /// <summary>slice 三链（假退伏击/断粮疲敌/守城待变）+ 夜袭组合手段的默认配置。</summary>
        public static TacticChainConfig SliceDefault() => new TacticChainConfig(new[]
        {
            new TacticChainDefinition(TacticTag.FeintAmbush, new[]
            {
                TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued, TacticCondition.AmbushSurprise,
            }),
            new TacticChainDefinition(TacticTag.SupplyExhaustion, new[]
            {
                TacticCondition.SupplyLineCut, TacticCondition.ShortageReachedGrace, TacticCondition.EnemyCohesionCrossedThreshold,
            }),
            new TacticChainDefinition(TacticTag.HoldUntilRelief, new[]
            {
                TacticCondition.HeldPosition, TacticCondition.ReliefArrived, TacticCondition.SurvivedDeadline,
            }),
            new TacticChainDefinition(TacticTag.NightRaid, new[]
            {
                TacticCondition.IsNight, TacticCondition.StealthSuccess, TacticCondition.DefenderUnaware, TacticCondition.RaiderDisciplineMet,
            }),
            new TacticChainDefinition(TacticTag.FireAttack, new[]
            {
                TacticCondition.DryField, TacticCondition.EnemyExposedToFire, TacticCondition.FireIgnited,
            }),
            new TacticChainDefinition(TacticTag.FloodAttack, new[]
            {
                TacticCondition.EnemyInLowGround, TacticCondition.WaterworksHeld, TacticCondition.FloodReleased,
            }),
            new TacticChainDefinition(TacticTag.FeignedSurrender, new[]
            {
                TacticCondition.SurrenderFeigned, TacticCondition.EnemyLuredOpen, TacticCondition.StrikeFromWithin,
            }),
            new TacticChainDefinition(TacticTag.BesiegeRelief, new[]
            {
                TacticCondition.PointBesieged, TacticCondition.ReliefIntercepted, TacticCondition.AmbushOnRoute,
            }),
        });
    }
}
