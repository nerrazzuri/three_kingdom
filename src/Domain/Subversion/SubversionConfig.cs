using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 人心杠杆配置（GDD_024 §11 Balancing，数据驱动，权威定点 ADR-0004）。不可变。
    /// 含成功度权重、反噬带、递减，及各计的战斗效果系数与策反门槛。
    /// </summary>
    public sealed class SubversionConfig
    {
        // ---- 成功度 F1 ----
        /// <summary>基础成功度。</summary>
        public FixedPoint Base { get; }
        /// <summary>侦察质量权重（反全知：未侦察此项恒 0）。</summary>
        public FixedPoint WeightIntel { get; }
        /// <summary>弱点权重（离间用怨恨、策反用低忠诚∧怨恨、攻心用 1−魅力）。</summary>
        public FixedPoint WeightWeakness { get; }
        /// <summary>抵抗权重（作用于 魅力+警觉）。</summary>
        public FixedPoint WeightResist { get; }
        /// <summary>重复施计每次的成功度递减（防无脑刷，W5）。</summary>
        public FixedPoint DecayPerAttempt { get; }
        /// <summary>反噬带宽（判定落在 [s, s+band) → 反噬）。</summary>
        public FixedPoint BackfireBand { get; }
        /// <summary>未侦察额外折扣（盲施惩罚，R1）。</summary>
        public FixedPoint UnscoutedPenalty { get; }

        // ---- 战斗效果系数 F3 ----
        /// <summary>离间成功的守方军纪罚幅（负向应用）。</summary>
        public FixedPoint DiscordDisciplineHit { get; }
        /// <summary>策反成功的守军倒戈比。</summary>
        public FixedPoint DefectRatio { get; }
        /// <summary>攻心成功的守方士气罚幅。</summary>
        public FixedPoint RumorMoraleHit { get; }
        /// <summary>反噬的守方士气反升幅（同仇敌忾）。</summary>
        public FixedPoint BackfireMoraleGain { get; }

        // ---- 策反门 + 离间倒戈阈 ----
        /// <summary>策反门：忠诚须低于此。</summary>
        public FixedPoint DefectLoyaltyMax { get; }
        /// <summary>策反门：怨恨须高于此。</summary>
        public FixedPoint DefectResentmentMin { get; }
        /// <summary>离间倒戈阈：怨恨达此，离间成功追加小幅倒戈倾向。</summary>
        public FixedPoint DiscordDefectThreshold { get; }
        /// <summary>离间越阈追加的倒戈比。</summary>
        public FixedPoint DiscordDefectRatio { get; }

        public SubversionConfig(
            FixedPoint @base, FixedPoint weightIntel, FixedPoint weightWeakness, FixedPoint weightResist,
            FixedPoint decayPerAttempt, FixedPoint backfireBand, FixedPoint unscoutedPenalty,
            FixedPoint discordDisciplineHit, FixedPoint defectRatio, FixedPoint rumorMoraleHit, FixedPoint backfireMoraleGain,
            FixedPoint defectLoyaltyMax, FixedPoint defectResentmentMin, FixedPoint discordDefectThreshold, FixedPoint discordDefectRatio)
        {
            Base = @base;
            WeightIntel = weightIntel;
            WeightWeakness = weightWeakness;
            WeightResist = weightResist;
            DecayPerAttempt = decayPerAttempt;
            BackfireBand = backfireBand;
            UnscoutedPenalty = unscoutedPenalty;
            DiscordDisciplineHit = discordDisciplineHit;
            DefectRatio = defectRatio;
            RumorMoraleHit = rumorMoraleHit;
            BackfireMoraleGain = backfireMoraleGain;
            DefectLoyaltyMax = defectLoyaltyMax;
            DefectResentmentMin = defectResentmentMin;
            DiscordDefectThreshold = discordDefectThreshold;
            DiscordDefectRatio = discordDefectRatio;
        }

        /// <summary>默认（GDD_024 §11 初值，待打磨；施计撬动而非替代六维准备，W5）。</summary>
        public static SubversionConfig Default { get; } = new SubversionConfig(
            @base: FixedPoint.FromFraction(15, 100),
            weightIntel: FixedPoint.FromFraction(3, 10),
            weightWeakness: FixedPoint.FromFraction(4, 10),
            weightResist: FixedPoint.FromFraction(25, 100),
            decayPerAttempt: FixedPoint.FromFraction(12, 100),
            backfireBand: FixedPoint.FromFraction(2, 10),
            unscoutedPenalty: FixedPoint.FromFraction(3, 10),
            discordDisciplineHit: FixedPoint.FromFraction(3, 10),
            defectRatio: FixedPoint.FromFraction(35, 100),
            rumorMoraleHit: FixedPoint.FromFraction(25, 100),
            backfireMoraleGain: FixedPoint.FromFraction(15, 100),
            defectLoyaltyMax: FixedPoint.FromFraction(4, 10),
            defectResentmentMin: FixedPoint.FromFraction(5, 10),
            discordDefectThreshold: FixedPoint.FromFraction(7, 10),
            discordDefectRatio: FixedPoint.FromFraction(15, 100));
    }
}
