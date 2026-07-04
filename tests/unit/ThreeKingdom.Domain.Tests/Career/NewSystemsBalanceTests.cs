using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// 新系统平衡不变量（C11 打磨锁定）：忠诚经营节奏（久疏约 10 日方入可挖角、一次赏赐即拔出）；
    /// 人心杠杆情报门频谱（已侦察成功度显著高于盲施）。锁定调后默认值，防未来静默破坏。
    /// </summary>
    [TestFixture]
    public class NewSystemsBalanceTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly CharacterId Aide = new CharacterId("char-aide");

        private static RetinueState One(int aff10)
            => new RetinueState(new[] { new RetinueMember(Aide, F(aff10, 10)) },
                System.Array.Empty<KeyValuePair<OfficeRole, CharacterId>>());

        private static FixedPoint Aff(RetinueState r)
        {
            foreach (RetinueMember m in r.Members) return m.Affinity;
            return FixedPoint.FromInt(-1);
        }

        // ---- 忠诚经营节奏：0.6 起，久疏约 10 日方跌破可挖角阈（非 1-2 日即失人，也非永不失）----
        [Test]
        public void test_loyalty_neglect_pace_is_neither_instant_nor_never()
        {
            var svc = new RetinueLoyaltyService();
            var cfg = RetinueLoyaltyConfig.Default;
            RetinueState r = One(6);
            for (int i = 0; i < 3; i++) r = svc.Decay(r, cfg);
            Assert.That(Aff(r).Raw, Is.GreaterThan(cfg.PoachThreshold.Raw), "短暂疏忽（3日）不至沦为可挖角——非一掉就失人。");

            for (int i = 0; i < 12; i++) r = svc.Decay(r, cfg);   // 共 15 日
            Assert.That(Aff(r).Raw, Is.LessThan(cfg.PoachThreshold.Raw), "长期疏忽（约两周）→ 跌破可挖角阈——久疏必付代价（W5）。");
        }

        // ---- 一次赏赐把濒临流失者拔回忠诚区 ----
        [Test]
        public void test_one_reward_lifts_member_out_of_poachable_range()
        {
            var svc = new RetinueLoyaltyService();
            var cfg = RetinueLoyaltyConfig.Default;
            RetinueState low = One(3);   // 0.3 < 阈 0.4，可挖角
            Assert.That(Aff(low).Raw, Is.LessThan(cfg.PoachThreshold.Raw));
            RetinueState rewarded = svc.Reward(low, Aide, FixedPoint.One, cfg);   // +0.2 → 0.5
            Assert.That(Aff(rewarded).Raw, Is.GreaterThan(cfg.PoachThreshold.Raw), "一次赏赐即拔回忠诚区——经营有效、非杯水车薪。");
        }

        // ---- 人心杠杆情报门频谱：已侦察成功度显著高于盲施 ----
        [Test]
        public void test_subversion_intel_gate_spectrum_is_significant()
        {
            var svc = new SubversionService();
            var cfg = SubversionConfig.Default;
            var scouted = new SubversionTargetProfile(new CharacterId("g"), F(5, 10), F(6, 10), F(3, 10), F(2, 10), F(2, 10), scouted: true, F(8, 10));
            var blind = new SubversionTargetProfile(new CharacterId("g"), F(5, 10), F(6, 10), F(3, 10), F(2, 10), F(2, 10), scouted: false, F(8, 10));
            FixedPoint sc = svc.Resolve(SubversionScheme.SowDiscord, scouted, FixedPoint.One, 0, 1UL, cfg).Chance;
            FixedPoint bl = svc.Resolve(SubversionScheme.SowDiscord, blind, FixedPoint.One, 0, 1UL, cfg).Chance;
            Assert.That((sc - bl).Raw, Is.GreaterThan(F(3, 10).Raw), "已侦察成功度显著高于盲施（≥0.3 差）——侦察是攻心前提（反全知，非可有可无）。");
        }
    }
}
