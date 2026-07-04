using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>占领归属判定结果（GDD_019 §占城 C / ADR-0010）。</summary>
    public enum OwnershipVerdict
    {
        /// <summary>交玩家直辖。</summary>
        GrantToPlayer = 0,
        /// <summary>君主直辖（玩家只得功绩/名望/赏赐 + 累积自立倾向）。</summary>
        LordKeeps = 1,
    }

    /// <summary>
    /// 占城归属方案 C 的配置（ADR-0010 数据驱动）。<c>DefaultPlayerGrants</c> 座之前恒归玩家；
    /// 之后按 <c>BaseGrant + Σ 权重·归一化因子</c> 求 p_grant。权重定点 [.]，因子归一化 [0,1]。不可变，构造校验。
    /// </summary>
    public sealed class OccupationConfig
    {
        /// <summary>前 N 座默认归玩家（默认 2；≥0）。</summary>
        public int DefaultPlayerGrants { get; }
        /// <summary>基础归玩家概率。</summary>
        public FixedPoint BaseGrant { get; }
        /// <summary>名望权重。</summary>
        public FixedPoint WeightRenown { get; }
        /// <summary>君主好感权重。</summary>
        public FixedPoint WeightStanding { get; }
        /// <summary>城池战略价值权重（价值越高君主越想自留 → 通常为负）。</summary>
        public FixedPoint WeightCityValue { get; }
        /// <summary>每次战果被君主收走累积的自立倾向量（≥0）。</summary>
        public int LeanPerSeizure { get; }

        public OccupationConfig(
            int defaultPlayerGrants, FixedPoint baseGrant,
            FixedPoint weightRenown, FixedPoint weightStanding, FixedPoint weightCityValue, int leanPerSeizure)
        {
            if (defaultPlayerGrants < 0) throw new ArgumentOutOfRangeException(nameof(defaultPlayerGrants), "默认归玩家座数不可为负。");
            if (leanPerSeizure < 0) throw new ArgumentOutOfRangeException(nameof(leanPerSeizure), "自立倾向增量不可为负。");
            DefaultPlayerGrants = defaultPlayerGrants;
            BaseGrant = baseGrant;
            WeightRenown = weightRenown;
            WeightStanding = weightStanding;
            WeightCityValue = weightCityValue;
            LeanPerSeizure = leanPerSeizure;
        }

        /// <summary>方案 C 默认（前 2 座归玩家；base 0.5，名望+、好感+、城价值-）。</summary>
        public static OccupationConfig Default { get; } = new OccupationConfig(
            defaultPlayerGrants: 2,
            baseGrant: FixedPoint.FromFraction(1, 2),
            weightRenown: FixedPoint.FromFraction(3, 10),
            weightStanding: FixedPoint.FromFraction(2, 10),
            weightCityValue: FixedPoint.FromFraction(-3, 10),
            leanPerSeizure: 10);
    }

    /// <summary>
    /// 占城归属判定（GDD_019 §占城 C / ADR-0010 D2/D5）。<b>纯确定性 Domain 规则</b>：
    /// 前 N 座恒归玩家；之后种子化伯努利（注入式确定性流，ADR-0006 同款，可复现·不可预测·非掷骰）。
    /// 只读玩家合法态（名望/好感/城价值归一化），<b>不</b>读敌方真值（反全知）。定点 [0,1]（ADR-0004）。
    /// </summary>
    public sealed class OccupationOwnershipService
    {
        /// <summary>
        /// 判定占领第 <paramref name="conquestIndex"/>（0 起）座城的归属。归一化因子须在 [0,1]。
        /// 同 (index, 因子, 种子, 配置) → 同结果。
        /// </summary>
        public OwnershipVerdict Resolve(
            int conquestIndex, FixedPoint renownNorm, FixedPoint standingNorm, FixedPoint cityValueNorm,
            ulong seed, OccupationConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (conquestIndex < 0) throw new ArgumentOutOfRangeException(nameof(conquestIndex), "占城序号不可为负。");

            // 前 N 座恒归玩家（启动动力）。
            if (conquestIndex < config.DefaultPlayerGrants) return OwnershipVerdict.GrantToPlayer;

            FixedPoint pGrant = Clamp01(
                config.BaseGrant
                + config.WeightRenown * renownNorm
                + config.WeightStanding * standingNorm
                + config.WeightCityValue * cityValueNorm);

            var rng = new DeterministicRandom(seed);
            return rng.NextUnit() < pGrant ? OwnershipVerdict.GrantToPlayer : OwnershipVerdict.LordKeeps;
        }

        private static FixedPoint Clamp01(FixedPoint v)
            => v < FixedPoint.Zero ? FixedPoint.Zero : (v > FixedPoint.One ? FixedPoint.One : v);
    }
}
