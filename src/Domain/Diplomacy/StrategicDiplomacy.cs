using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>战略外交立场（GDD M11：约束战争的势力间关系）。</summary>
    public enum DiplomaticStance
    {
        Hostile = 0,          // 敌对（可攻）
        Neutral = 1,          // 中立（可攻，无约）
        NonAggression = 2,    // 互不侵犯（攻须背约）
        Alliance = 3,         // 盟约（攻须背约，代价更重）
    }

    /// <summary>
    /// 战略外交立场态（GDD M11 §权威）：玩家与各势力的立场（按 FactionId）。不可变；纳入存档、确定性哈希。
    /// 缺省势力视为 <see cref="DiplomaticStance.Neutral"/>。
    /// </summary>
    public sealed class DiplomaticStanceState
    {
        private readonly Dictionary<string, DiplomaticStance> _stances;

        public DiplomaticStanceState(IReadOnlyDictionary<string, DiplomaticStance>? stances)
        {
            _stances = new Dictionary<string, DiplomaticStance>(StringComparer.Ordinal);
            if (stances != null) foreach (KeyValuePair<string, DiplomaticStance> kv in stances) _stances[kv.Key] = kv.Value;
        }

        public static DiplomaticStanceState Empty { get; } = new DiplomaticStanceState(null);

        /// <summary>与某势力的立场（缺省中立）。</summary>
        public DiplomaticStance StanceWith(FactionId power)
            => _stances.TryGetValue(power.Value ?? "", out DiplomaticStance s) ? s : DiplomaticStance.Neutral;

        public DiplomaticStanceState With(FactionId power, DiplomaticStance stance)
        {
            var next = new Dictionary<string, DiplomaticStance>(_stances, StringComparer.Ordinal) { [power.Value] = stance };
            return new DiplomaticStanceState(next);
        }

        public StateHash Hash()
        {
            var h = new StateHasher();
            var keys = new List<string>(_stances.Keys);
            keys.Sort(StringComparer.Ordinal);
            h.Append(keys.Count);
            foreach (string k in keys) { h.Append(k.Length); foreach (char c in k) h.Append((int)c); h.Append((int)_stances[k]); }
            return h.ToHash();
        }
    }

    /// <summary>缔约条件（归一化 [0,1]，定点）。不可变。</summary>
    public sealed class PactFactors
    {
        public FixedPoint RenownNorm { get; }
        public FixedPoint RelationNorm { get; }
        public FixedPoint GiftNorm { get; }
        public PactFactors(FixedPoint renownNorm, FixedPoint relationNorm, FixedPoint giftNorm)
        {
            RenownNorm = renownNorm;
            RelationNorm = relationNorm;
            GiftNorm = giftNorm;
        }
        public static PactFactors None { get; } = new PactFactors(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
    }

    /// <summary>战略外交配置（GDD M11 §Balancing，数据驱动）。不可变。</summary>
    public sealed class StrategicDiplomacyConfig
    {
        public FixedPoint BaseAccept { get; }
        public FixedPoint WeightRenown { get; }
        public FixedPoint WeightRelation { get; }
        public FixedPoint WeightGift { get; }
        /// <summary>盟约较互不侵犯更难成（额外阻力）。</summary>
        public FixedPoint AllianceExtraResistance { get; }
        /// <summary>背互不侵犯的声誉代价。</summary>
        public int BreachNonAggressionPenalty { get; }
        /// <summary>背盟约的声誉代价（更重）。</summary>
        public int BreachAlliancePenalty { get; }

        public StrategicDiplomacyConfig(
            FixedPoint baseAccept, FixedPoint weightRenown, FixedPoint weightRelation, FixedPoint weightGift,
            FixedPoint allianceExtraResistance, int breachNonAggressionPenalty, int breachAlliancePenalty)
        {
            BaseAccept = baseAccept;
            WeightRenown = weightRenown;
            WeightRelation = weightRelation;
            WeightGift = weightGift;
            AllianceExtraResistance = allianceExtraResistance;
            BreachNonAggressionPenalty = breachNonAggressionPenalty;
            BreachAlliancePenalty = breachAlliancePenalty;
        }

        public static StrategicDiplomacyConfig Default { get; } = new StrategicDiplomacyConfig(
            baseAccept: FixedPoint.FromFraction(2, 10),
            weightRenown: FixedPoint.FromFraction(3, 10), weightRelation: FixedPoint.FromFraction(3, 10),
            weightGift: FixedPoint.FromFraction(2, 10), allianceExtraResistance: FixedPoint.FromFraction(3, 10),
            breachNonAggressionPenalty: 20, breachAlliancePenalty: 40);
    }

    /// <summary>缔约判定结果（是否成 + 内部概率 + 新态）。不可变。</summary>
    public sealed class PactResult
    {
        public bool Accepted { get; }
        public FixedPoint Probability { get; }
        public DiplomaticStanceState State { get; }
        public PactResult(bool accepted, FixedPoint probability, DiplomaticStanceState state)
        {
            Accepted = accepted;
            Probability = probability;
            State = state;
        }
    }

    /// <summary>攻打某势力的战略约束（GDD M11：是否受盟约/互不侵犯约束、背约声誉代价）。不可变。</summary>
    public sealed class WarConstraint
    {
        /// <summary>是否无约束可攻（敌对/中立）。</summary>
        public bool Allowed { get; }
        /// <summary>是否须背约方可攻（互不侵犯/盟约）。</summary>
        public bool RequiresBreach { get; }
        /// <summary>背约声誉代价（RequiresBreach 时 &gt;0）。</summary>
        public int BreachReputationCost { get; }

        internal WarConstraint(bool allowed, bool requiresBreach, int breachReputationCost)
        {
            Allowed = allowed;
            RequiresBreach = requiresBreach;
            BreachReputationCost = breachReputationCost;
        }
    }

    /// <summary>背约结果（新态：被背方转敌对 + 声誉惩罚）。不可变。</summary>
    public sealed class BreachResult
    {
        public DiplomaticStanceState State { get; }
        public int ReputationPenalty { get; }
        public BreachResult(DiplomaticStanceState state, int reputationPenalty)
        {
            State = state;
            ReputationPenalty = reputationPenalty;
        }
    }

    /// <summary>
    /// 战略外交（GDD M11，确定性）：缔约（条件+种子判定，人各有志/邦交有度）· 战争约束（盟约/互不侵犯下攻须背约）·
    /// 背约代价（被背方转敌对 + 声誉惩罚，写回 GDD_006）。纯函数、注入式确定性随机（ADR-0004/0006）。
    /// </summary>
    public sealed class StrategicDiplomacyService
    {
        /// <summary>提议缔约（互不侵犯/盟约）：条件式 p_accept + 种子判定；成则立约。种子由调用方组装。</summary>
        public PactResult ProposePact(
            DiplomaticStanceState state, FactionId power, DiplomaticStance target,
            PactFactors factors, ulong seed, StrategicDiplomacyConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (target != DiplomaticStance.NonAggression && target != DiplomaticStance.Alliance)
                throw new ArgumentException("只可提议互不侵犯或盟约。", nameof(target));

            FixedPoint p = config.BaseAccept
                + config.WeightRenown * factors.RenownNorm
                + config.WeightRelation * factors.RelationNorm
                + config.WeightGift * factors.GiftNorm;
            if (target == DiplomaticStance.Alliance) p -= config.AllianceExtraResistance;   // 盟约更难
            p = p.Clamp(FixedPoint.Zero, FixedPoint.One);

            bool accepted = new DeterministicRandom(seed).NextUnit() < p;
            DiplomaticStanceState next = accepted ? state.With(power, target) : state;
            return new PactResult(accepted, p, next);
        }

        /// <summary>攻打某势力的约束：敌对/中立无约束；互不侵犯/盟约须背约（代价随立场）。</summary>
        public WarConstraint CheckWarTarget(DiplomaticStanceState state, FactionId power, StrategicDiplomacyConfig config)
        {
            DiplomaticStance stance = state.StanceWith(power);
            switch (stance)
            {
                case DiplomaticStance.NonAggression:
                    return new WarConstraint(false, true, config.BreachNonAggressionPenalty);
                case DiplomaticStance.Alliance:
                    return new WarConstraint(false, true, config.BreachAlliancePenalty);
                default:
                    return new WarConstraint(true, false, 0);   // 敌对/中立
            }
        }

        /// <summary>背约攻打：被背方转敌对 + 声誉惩罚（按立场）。已敌对/中立则无背约（惩罚 0）。</summary>
        public BreachResult Breach(DiplomaticStanceState state, FactionId power, StrategicDiplomacyConfig config)
        {
            DiplomaticStance stance = state.StanceWith(power);
            int penalty = stance switch
            {
                DiplomaticStance.NonAggression => config.BreachNonAggressionPenalty,
                DiplomaticStance.Alliance => config.BreachAlliancePenalty,
                _ => 0,
            };
            return new BreachResult(state.With(power, DiplomaticStance.Hostile), penalty);
        }
    }
}
