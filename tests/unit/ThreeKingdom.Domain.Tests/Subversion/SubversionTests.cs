using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;

namespace ThreeKingdom.Domain.Tests.Subversion
{
    /// <summary>
    /// 人心杠杆（GDD_024 §15）：反全知门 · 策反门 · 确定性 · 三计效果映射 · 反噬暴露 · 重复递减 · 强度单调。
    /// 用定制配置强制各判定分支（Base=1→必成 / s=0+band=1→必反噬 / band=0→必无效），稳健不猎种子。
    /// </summary>
    [TestFixture]
    public class SubversionTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private readonly SubversionService _svc = new SubversionService();

        private static SubversionTargetProfile Prof(
            int loyalty, int resentment, int greed, int charm, int alertness, bool scouted, int intel)
            => new SubversionTargetProfile(new CharacterId("gen"),
                F(loyalty, 10), F(resentment, 10), F(greed, 10), F(charm, 10), F(alertness, 10), scouted, F(intel, 10));

        // 除 base/band 外沿用 Default 的效果系数与门槛。
        private static SubversionConfig Cfg(FixedPoint @base, FixedPoint band) => new SubversionConfig(
            @base: @base, weightIntel: F(3, 10), weightWeakness: F(4, 10), weightResist: F(25, 100),
            decayPerAttempt: F(12, 100), backfireBand: band, unscoutedPenalty: F(3, 10),
            discordDisciplineHit: F(3, 10), defectRatio: F(35, 100), rumorMoraleHit: F(25, 100), backfireMoraleGain: F(15, 100),
            defectLoyaltyMax: F(4, 10), defectResentmentMin: F(5, 10), discordDefectThreshold: F(7, 10), discordDefectRatio: F(15, 100));

        // ---- R1 反全知门：未侦察成功度显著低于已侦察 ----
        [Test]
        public void test_unscouted_far_lower_success_than_scouted()
        {
            SubversionConfig cfg = SubversionConfig.Default;
            SubversionTargetProfile scouted = Prof(loyalty: 5, resentment: 6, greed: 3, charm: 2, alertness: 2, scouted: true, intel: 8);
            SubversionTargetProfile blind = Prof(loyalty: 5, resentment: 6, greed: 3, charm: 2, alertness: 2, scouted: false, intel: 8);
            FixedPoint sc = _svc.Resolve(SubversionScheme.SowDiscord, scouted, FixedPoint.One, 0, 1UL, cfg).Chance;
            FixedPoint bl = _svc.Resolve(SubversionScheme.SowDiscord, blind, FixedPoint.One, 0, 1UL, cfg).Chance;
            Assert.That(sc.Raw, Is.GreaterThan(bl.Raw), "已侦察成功度应显著高于盲施（反全知门）。");
        }

        // ---- R2 策反门：须低忠诚 ∧ 高怨恨方成型 ----
        [Test]
        public void test_defection_gate_requires_low_loyalty_and_high_resentment()
        {
            SubversionConfig sure = Cfg(FixedPoint.One, F(0, 1));   // base=1 → s 饱和，门过则必成
            SubversionTargetProfile loyal = Prof(loyalty: 8, resentment: 6, greed: 5, charm: 2, alertness: 2, scouted: true, intel: 8);
            SubversionTargetProfile wavering = Prof(loyalty: 2, resentment: 7, greed: 5, charm: 2, alertness: 2, scouted: true, intel: 8);
            Assert.That(_svc.Resolve(SubversionScheme.InciteDefection, loyal, FixedPoint.One, 0, 3UL, sure).Result,
                Is.EqualTo(SubversionResult.Ineffective), "高忠诚 → 策反门不成型（无效）。");
            Assert.That(_svc.Resolve(SubversionScheme.InciteDefection, wavering, FixedPoint.One, 0, 3UL, sure).Result,
                Is.EqualTo(SubversionResult.Success), "低忠诚+高怨恨 → 门成型可策反。");
        }

        // ---- 确定性：同参同种子 → 同结果 ----
        [Test]
        public void test_resolve_is_deterministic()
        {
            SubversionTargetProfile t = Prof(3, 6, 4, 2, 3, true, 7);
            SubversionOutcome a = _svc.Resolve(SubversionScheme.SowDiscord, t, F(7, 10), 1, 999UL, SubversionConfig.Default);
            SubversionOutcome b = _svc.Resolve(SubversionScheme.SowDiscord, t, F(7, 10), 1, 999UL, SubversionConfig.Default);
            Assert.That(b.Result, Is.EqualTo(a.Result));
            Assert.That(b.Chance.Raw, Is.EqualTo(a.Chance.Raw), "同种子成功度一致。");
            Assert.That(b.Effect.DefenderDisciplineDelta.Raw, Is.EqualTo(a.Effect.DefenderDisciplineDelta.Raw), "同种子效果一致。");
        }

        // ---- 三计效果映射（成功）----
        [Test]
        public void test_effect_mapping_per_scheme_on_success()
        {
            SubversionConfig sure = Cfg(FixedPoint.One, F(0, 1));   // 必成
            SubversionTargetProfile t = Prof(loyalty: 2, resentment: 8, greed: 6, charm: 2, alertness: 2, scouted: true, intel: 8);

            SubversionEffect discord = _svc.Resolve(SubversionScheme.SowDiscord, t, FixedPoint.One, 0, 1UL, sure).Effect;
            Assert.That(discord.DefenderDisciplineDelta.Raw, Is.LessThan(0), "离间成功 → 守方军纪↓。");
            Assert.That(discord.GarrisonDefectRatio.Raw, Is.GreaterThan(0), "怨恨越阈 → 离间追加倒戈倾向。");

            SubversionEffect defect = _svc.Resolve(SubversionScheme.InciteDefection, t, FixedPoint.One, 0, 1UL, sure).Effect;
            Assert.That(defect.GarrisonDefectRatio.Raw, Is.GreaterThan(0), "策反成功 → 有效守军减（倒戈比>0）。");

            SubversionEffect rumor = _svc.Resolve(SubversionScheme.UnderminedMorale, t, FixedPoint.One, 0, 1UL, sure).Effect;
            Assert.That(rumor.DefenderMoraleDelta.Raw, Is.LessThan(0), "攻心成功 → 守方开战士气↓。");
        }

        // ---- R4 反噬：必反噬 → 守方士气反升 + 暴露 ----
        [Test]
        public void test_backfire_raises_defender_morale_and_exposes()
        {
            SubversionConfig alwaysBackfire = Cfg(F(0, 1), FixedPoint.One);   // s=0 且 band=1 → 任何 roll∈[0,1) 落入反噬带
            SubversionTargetProfile t = Prof(loyalty: 5, resentment: 3, greed: 3, charm: 5, alertness: 8, scouted: true, intel: 4);
            SubversionOutcome o = _svc.Resolve(SubversionScheme.SowDiscord, t, F(1, 10), 5, 7UL, alwaysBackfire);
            Assert.That(o.Result, Is.EqualTo(SubversionResult.Backfired));
            Assert.That(o.Exposed, Is.True, "反噬 → 情报暴露。");
            Assert.That(o.Effect.DefenderMoraleDelta.Raw, Is.GreaterThan(0), "反噬 → 守方同仇敌忾、士气反升。");
        }

        // ---- band=0 且 s=0 → 必无效（不报错，失败可继续）----
        [Test]
        public void test_zero_chance_no_band_is_ineffective()
        {
            SubversionConfig noEffect = Cfg(F(0, 1), F(0, 1));
            SubversionTargetProfile t = Prof(5, 3, 3, 5, 8, scouted: true, intel: 3);
            SubversionOutcome o = _svc.Resolve(SubversionScheme.UnderminedMorale, t, F(1, 10), 0, 2UL, noEffect);
            Assert.That(o.Result, Is.EqualTo(SubversionResult.Ineffective));
            Assert.That(o.Effect.IsNone, Is.True, "无效 → 无战斗效果。");
        }

        // ---- W5 递减：重复施计成功度递减 ----
        [Test]
        public void test_repeated_attempts_diminish_chance()
        {
            SubversionConfig cfg = SubversionConfig.Default;
            SubversionTargetProfile t = Prof(5, 6, 4, 2, 2, true, 7);
            FixedPoint first = _svc.Resolve(SubversionScheme.SowDiscord, t, FixedPoint.One, 0, 1UL, cfg).Chance;
            FixedPoint third = _svc.Resolve(SubversionScheme.SowDiscord, t, FixedPoint.One, 2, 1UL, cfg).Chance;
            Assert.That(third.Raw, Is.LessThan(first.Raw), "重复施计边际递减（防无脑刷谣言，W5）。");
        }

        // ---- 强度单调：投入越大成功度越高（其余同）----
        [Test]
        public void test_higher_intensity_raises_chance()
        {
            SubversionConfig cfg = SubversionConfig.Default;
            SubversionTargetProfile t = Prof(5, 6, 4, 2, 2, true, 7);
            FixedPoint low = _svc.Resolve(SubversionScheme.SowDiscord, t, F(3, 10), 0, 1UL, cfg).Chance;
            FixedPoint high = _svc.Resolve(SubversionScheme.SowDiscord, t, F(9, 10), 0, 1UL, cfg).Chance;
            Assert.That(high.Raw, Is.GreaterThan(low.Raw), "投入强度越大成功度越高（单调）。");
        }
    }
}
