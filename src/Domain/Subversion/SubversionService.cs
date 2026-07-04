using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 人心杠杆结算（GDD_024 F1/F2/F3，<b>确定性纯函数</b>）：反全知门（未侦察折扣）→ 成功度 →
    /// 种子化判定（成/反噬/无效，ADR-0006 注入式流，可复现·非掷骰）→ 战斗接缝效果。定点 [0,1]（ADR-0004）。
    /// 只读目标投影（Intel/Relationships 派生），<b>不</b>读世界真值。
    /// </summary>
    public sealed class SubversionService
    {
        /// <summary>
        /// 结算一次施计。<paramref name="intensity"/>=投入强度 [0,1]；<paramref name="priorAttempts"/>=该守将已施计次数（递减）。
        /// 同 (计, 画像, 强度, 次数, 种子, 配置) → 同结果。
        /// </summary>
        public SubversionOutcome Resolve(
            SubversionScheme scheme, SubversionTargetProfile target, FixedPoint intensity, int priorAttempts,
            ulong seed, SubversionConfig config)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (priorAttempts < 0) throw new ArgumentOutOfRangeException(nameof(priorAttempts), "施计次数不可为负。");
            intensity = intensity.Clamp(FixedPoint.Zero, FixedPoint.One);

            // 策反门（R2）：须低忠诚 ∧ 高怨恨，否则结构性无效（不掷种子、不耗反噬风险）。
            if (scheme == SubversionScheme.InciteDefection
                && !(target.Loyalty < config.DefectLoyaltyMax && target.ResentmentToLord > config.DefectResentmentMin))
            {
                return new SubversionOutcome(SubversionResult.Ineffective, SubversionEffect.None, FixedPoint.Zero, exposed: false);
            }

            FixedPoint weakness = Weakness(scheme, target);
            FixedPoint resist = target.Charm + target.Alertness;   // 至多 2
            FixedPoint s =
                config.Base
                + config.WeightIntel * target.EffectiveIntelQuality
                + config.WeightWeakness * (weakness * intensity)
                - config.WeightResist * resist
                - config.DecayPerAttempt * FixedPoint.FromInt(priorAttempts);
            if (!target.Scouted) s = s - config.UnscoutedPenalty;   // 盲施折扣（R1）
            s = s.Clamp(FixedPoint.Zero, FixedPoint.One);

            FixedPoint roll = new DeterministicRandom(seed).NextUnit();
            if (roll < s)
                return new SubversionOutcome(SubversionResult.Success, SuccessEffect(scheme, target, intensity, config), s, exposed: false);

            FixedPoint backfireCeil = (s + config.BackfireBand).Clamp(FixedPoint.Zero, FixedPoint.One);
            if (roll < backfireCeil)
                return new SubversionOutcome(
                    SubversionResult.Backfired,
                    new SubversionEffect(config.BackfireMoraleGain, FixedPoint.Zero, FixedPoint.Zero),
                    s, exposed: true);

            return new SubversionOutcome(SubversionResult.Ineffective, SubversionEffect.None, s, exposed: false);
        }

        /// <summary>按计种取弱点（[0,1]）。</summary>
        private static FixedPoint Weakness(SubversionScheme scheme, SubversionTargetProfile t)
        {
            switch (scheme)
            {
                case SubversionScheme.SowDiscord:
                    return t.ResentmentToLord;
                case SubversionScheme.InciteDefection:
                    // 低忠诚 ∧ 高怨恨 ∧ 贪财三者综合。
                    return Avg3(FixedPoint.One - t.Loyalty, t.ResentmentToLord, t.Greed);
                case SubversionScheme.UnderminedMorale:
                    return FixedPoint.One - t.Charm;
                default:
                    return FixedPoint.Zero;
            }
        }

        /// <summary>成功的战斗接缝效果（F3）。</summary>
        private static SubversionEffect SuccessEffect(
            SubversionScheme scheme, SubversionTargetProfile t, FixedPoint intensity, SubversionConfig config)
        {
            switch (scheme)
            {
                case SubversionScheme.SowDiscord:
                {
                    FixedPoint discipline = -(config.DiscordDisciplineHit * intensity);
                    FixedPoint defect = t.ResentmentToLord >= config.DiscordDefectThreshold
                        ? config.DiscordDefectRatio * intensity
                        : FixedPoint.Zero;
                    return new SubversionEffect(FixedPoint.Zero, defect, discipline);
                }
                case SubversionScheme.InciteDefection:
                    return new SubversionEffect(FixedPoint.Zero, config.DefectRatio * intensity, FixedPoint.Zero);
                case SubversionScheme.UnderminedMorale:
                    return new SubversionEffect(-(config.RumorMoraleHit * intensity), FixedPoint.Zero, FixedPoint.Zero);
                default:
                    return SubversionEffect.None;
            }
        }

        private static FixedPoint Avg3(FixedPoint a, FixedPoint b, FixedPoint c)
            => (a + b + c) * FixedPoint.FromFraction(1, 3);
    }
}
