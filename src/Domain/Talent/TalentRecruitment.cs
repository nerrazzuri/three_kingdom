using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Talent
{
    /// <summary>招揽条件（GDD_020 §6，各项归一化 [0,1]，定点 ADR-0004）。不可变。</summary>
    public sealed class RecruitmentOffer
    {
        /// <summary>名望归一化。</summary>
        public FixedPoint RenownNorm { get; }
        /// <summary>许官归一化。</summary>
        public FixedPoint OfficeNorm { get; }
        /// <summary>关系亲疏归一化。</summary>
        public FixedPoint RelationNorm { get; }
        /// <summary>待遇厚薄归一化。</summary>
        public FixedPoint OfferNorm { get; }

        public RecruitmentOffer(FixedPoint renownNorm, FixedPoint officeNorm, FixedPoint relationNorm, FixedPoint offerNorm)
        {
            RenownNorm = renownNorm;
            OfficeNorm = officeNorm;
            RelationNorm = relationNorm;
            OfferNorm = offerNorm;
        }

        /// <summary>空条件（全 0，仅凭人才自身志向）。</summary>
        public static RecruitmentOffer None { get; } =
            new RecruitmentOffer(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
    }

    /// <summary>招揽概率映射配置（GDD_020 §5 F2，数据驱动）。不可变。全部权重 ≥0 → p_join 对各条件单调不降。</summary>
    public sealed class TalentRecruitmentConfig
    {
        public FixedPoint BaseWill { get; }
        public FixedPoint WeightRenown { get; }
        public FixedPoint WeightOffice { get; }
        public FixedPoint WeightRelation { get; }
        public FixedPoint WeightOffer { get; }
        public FixedPoint WeightWillingness { get; }

        public TalentRecruitmentConfig(
            FixedPoint baseWill, FixedPoint weightRenown, FixedPoint weightOffice,
            FixedPoint weightRelation, FixedPoint weightOffer, FixedPoint weightWillingness)
        {
            BaseWill = baseWill;
            WeightRenown = weightRenown;
            WeightOffice = weightOffice;
            WeightRelation = weightRelation;
            WeightOffer = weightOffer;
            WeightWillingness = weightWillingness;
        }

        /// <summary>默认（占位权重，平衡延后）。</summary>
        public static TalentRecruitmentConfig Default { get; } = new TalentRecruitmentConfig(
            baseWill: FixedPoint.FromFraction(1, 10),
            weightRenown: FixedPoint.FromFraction(3, 10),
            weightOffice: FixedPoint.FromFraction(2, 10),
            weightRelation: FixedPoint.FromFraction(2, 10),
            weightOffer: FixedPoint.FromFraction(2, 10),
            weightWillingness: FixedPoint.FromFraction(3, 10));
    }

    /// <summary>招揽判定结果（判定 + 内部概率——概率仅供内部/测试，<b>不向玩家显示胜率</b>，GDD_020 R3）。不可变。</summary>
    public sealed class TalentRecruitmentResult
    {
        public RecruitmentVerdict Verdict { get; }
        /// <summary>内部 p_join（不展示；用于测试单调性/边界）。</summary>
        public FixedPoint Probability { get; }

        public TalentRecruitmentResult(RecruitmentVerdict verdict, FixedPoint probability)
        {
            Verdict = verdict;
            Probability = probability;
        }
    }

    /// <summary>
    /// 招揽判定（GDD_020 R3/§5 / ADR-0006）：条件式 p_join + <b>种子化确定性判定</b>（注入式确定性流，非掷骰，
    /// 可复现·不可预测）。纯函数、无胜率展示。同 (profile,offer,seed) → 同结果（人各有志的质感）。
    /// </summary>
    public sealed class TalentRecruitmentService
    {
        /// <summary>判定是否出仕。<paramref name="seed"/> 由调用方以 Hash(worldTick,talentId,faction,attemptIndex) 组装（各次尝试独立）。</summary>
        public TalentRecruitmentResult Resolve(
            TalentProfile profile, RecruitmentOffer offer, ulong seed, TalentRecruitmentConfig config)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (offer == null) throw new ArgumentNullException(nameof(offer));
            if (config == null) throw new ArgumentNullException(nameof(config));

            FixedPoint p = config.BaseWill
                + config.WeightRenown * offer.RenownNorm
                + config.WeightOffice * offer.OfficeNorm
                + config.WeightRelation * offer.RelationNorm
                + config.WeightOffer * offer.OfferNorm
                + config.WeightWillingness * profile.Willingness
                - profile.Reluctance;
            p = p.Clamp(FixedPoint.Zero, FixedPoint.One);

            var rng = new DeterministicRandom(seed);
            bool joined = rng.NextUnit() < p;   // 注入式确定性流首抽（可复现）
            return new TalentRecruitmentResult(joined ? RecruitmentVerdict.Joined : RecruitmentVerdict.Declined, p);
        }
    }

    /// <summary>已登场且已知晓的人才视图（GDD_020 R2 反全知：未登场或未知晓者<b>不入</b>）。纯函数。</summary>
    public sealed class TalentAwarenessService
    {
        /// <summary>玩家可见人才（已登场 ∩ 已知晓）；未知晓者结构上不进列表（反全知）。</summary>
        public System.Collections.Generic.IReadOnlyList<TalentProfile> Visible(
            TalentRoster roster, TalentState state, ThreeKingdom.Domain.Time.WorldTime worldTime)
        {
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (state == null) throw new ArgumentNullException(nameof(state));
            var list = new System.Collections.Generic.List<TalentProfile>();
            foreach (TalentProfile p in roster.Appeared(worldTime))
                if (state.Knows(p.Id)) list.Add(p);
            return list;
        }
    }
}
