using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Talent;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Balance
{
    /// <summary>
    /// 战略层平衡广度（E4）：对已有系统做<b>频谱扫描</b>锁定健康度——概率处处 [0,1] 有界、随利好单调、
    /// 盟约处处比互不侵犯更难、背盟代价高于背互不侵犯；人才招揽随待遇单调有界。防退化策略与静默破坏。
    /// </summary>
    [TestFixture]
    public class StrategicBalanceTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly FactionId Wei = new FactionId("faction-wei");

        // ---- 外交：礼物频谱单调 + 有界 + 盟约处处更难 ----
        [Test]
        public void test_diplomacy_gift_spectrum_monotone_bounded_and_alliance_harder()
        {
            var svc = new StrategicDiplomacyService();
            var cfg = StrategicDiplomacyConfig.Default;
            FixedPoint prevNa = FixedPoint.FromInt(-1);
            for (int g = 0; g <= 10; g++)
            {
                var factors = new PactFactors(F(5, 10), F(5, 10), F(g, 10));
                FixedPoint na = svc.ProposePact(DiplomaticStanceState.Empty, Wei, DiplomaticStance.NonAggression, factors, 1UL, cfg).Probability;
                FixedPoint al = svc.ProposePact(DiplomaticStanceState.Empty, Wei, DiplomaticStance.Alliance, factors, 1UL, cfg).Probability;

                Assert.That(na.Raw, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(FixedPoint.One.Raw), "接受概率恒在 [0,1]。");
                Assert.That(al.Raw, Is.LessThanOrEqualTo(na.Raw), $"礼 {g}/10：盟约处处比互不侵犯更难。");
                Assert.That(na.Raw, Is.GreaterThanOrEqualTo(prevNa.Raw), "礼物越厚接受概率不降（单调）。");
                prevNa = na;
            }
        }

        // ---- 外交：背盟代价高于背互不侵犯 ----
        [Test]
        public void test_breaching_alliance_costs_more_than_non_aggression()
        {
            var cfg = StrategicDiplomacyConfig.Default;
            Assert.That(cfg.BreachAlliancePenalty, Is.GreaterThan(cfg.BreachNonAggressionPenalty),
                "背盟代价 > 背互不侵犯代价（盟友相负，其罪更重）。");
        }

        // ---- 人才：待遇频谱单调 + 有界 ----
        [Test]
        public void test_talent_recruit_probability_spectrum_monotone_bounded()
        {
            var svc = new TalentRecruitmentService();
            var cfg = TalentRecruitmentConfig.Default;
            var profile = new TalentProfile(new TalentId("t-x"), new CharacterId("char-x"),
                F(7, 10), F(6, 10), F(8, 10), GeneralSpecialty.None, F(5, 10), F(2, 10), new WorldTime(0, DaySegment.Dawn));

            FixedPoint prev = FixedPoint.FromInt(-1);
            for (int o = 0; o <= 10; o++)
            {
                var offer = new RecruitmentOffer(F(o, 10), F(o, 10), F(o, 10), F(o, 10));
                FixedPoint p = svc.Resolve(profile, offer, 1UL, cfg).Probability;
                Assert.That(p.Raw, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(FixedPoint.One.Raw), "招揽概率恒在 [0,1]。");
                Assert.That(p.Raw, Is.GreaterThanOrEqualTo(prev.Raw), $"待遇 {o}/10：越厚招揽概率不降（单调）。");
                prev = p;
            }
        }
    }
}
