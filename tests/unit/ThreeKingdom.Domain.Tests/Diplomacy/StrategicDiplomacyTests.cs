using NUnit.Framework;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Diplomacy
{
    /// <summary>
    /// 战略外交（GDD M11 / epic-024）：外交立场态 · 缔约（条件+种子·盟约更难）· 战争约束（盟/邻攻须背约）· 背约代价。
    /// </summary>
    [TestFixture]
    public class StrategicDiplomacyTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly FactionId Wu = new FactionId("faction-sun");
        private readonly StrategicDiplomacyService _svc = new StrategicDiplomacyService();
        private static readonly StrategicDiplomacyConfig Cfg = StrategicDiplomacyConfig.Default;

        [Test]
        public void test_stance_defaults_neutral_and_hash_roundtrips()
        {
            Assert.That(DiplomaticStanceState.Empty.StanceWith(Wu), Is.EqualTo(DiplomaticStance.Neutral), "缺省中立。");
            DiplomaticStanceState a = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.Alliance);
            DiplomaticStanceState b = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.Alliance);
            Assert.That(b.Hash(), Is.EqualTo(a.Hash()), "同态同哈希（存档）。");
            Assert.That(a.With(Wu, DiplomaticStance.Hostile).Hash(), Is.Not.EqualTo(a.Hash()), "立场变→哈希变。");
        }

        [Test]
        public void test_pact_probability_monotonic_and_alliance_harder()
        {
            FixedPoint weakNa = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.NonAggression, PactFactors.None, 1UL, Cfg).Probability;
            FixedPoint strongNa = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.NonAggression, new PactFactors(FixedPoint.One, FixedPoint.One, FixedPoint.One), 1UL, Cfg).Probability;
            Assert.That(strongNa, Is.GreaterThanOrEqualTo(weakNa), "厚礼睦邻→缔约概率不降（单调）。");

            var factors = new PactFactors(F(5, 10), F(5, 10), F(5, 10));
            FixedPoint na = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.NonAggression, factors, 1UL, Cfg).Probability;
            FixedPoint alliance = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.Alliance, factors, 1UL, Cfg).Probability;
            Assert.That(alliance, Is.LessThan(na), "盟约较互不侵犯更难成。");
        }

        [Test]
        public void test_pact_accepted_sets_stance_deterministically()
        {
            var strong = new PactFactors(FixedPoint.One, FixedPoint.One, FixedPoint.One);   // p→1 恒成
            PactResult a = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.NonAggression, strong, 1UL, Cfg);
            Assert.That(a.Accepted, Is.True);
            Assert.That(a.State.StanceWith(Wu), Is.EqualTo(DiplomaticStance.NonAggression), "缔约成 → 立约。");
            PactResult b = _svc.ProposePact(DiplomaticStanceState.Empty, Wu, DiplomaticStance.NonAggression, strong, 1UL, Cfg);
            Assert.That(b.Accepted, Is.EqualTo(a.Accepted), "同 (势力,条件,种子)→同结果（可复现）。");
        }

        [Test]
        public void test_war_constraint_by_stance()
        {
            DiplomaticStanceState hostile = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.Hostile);
            Assert.That(_svc.CheckWarTarget(hostile, Wu, Cfg).Allowed, Is.True, "敌对无约束可攻。");
            Assert.That(_svc.CheckWarTarget(DiplomaticStanceState.Empty, Wu, Cfg).Allowed, Is.True, "中立可攻。");

            DiplomaticStanceState na = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.NonAggression);
            WarConstraint c = _svc.CheckWarTarget(na, Wu, Cfg);
            Assert.That(c.RequiresBreach, Is.True, "互不侵犯 → 攻须背约。");
            Assert.That(c.BreachReputationCost, Is.EqualTo(Cfg.BreachNonAggressionPenalty));

            DiplomaticStanceState ally = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.Alliance);
            Assert.That(_svc.CheckWarTarget(ally, Wu, Cfg).BreachReputationCost, Is.EqualTo(Cfg.BreachAlliancePenalty), "背盟代价更重。");
        }

        [Test]
        public void test_breach_turns_hostile_and_penalizes()
        {
            DiplomaticStanceState ally = DiplomaticStanceState.Empty.With(Wu, DiplomaticStance.Alliance);
            BreachResult r = _svc.Breach(ally, Wu, Cfg);
            Assert.That(r.State.StanceWith(Wu), Is.EqualTo(DiplomaticStance.Hostile), "背约 → 被背方转敌对。");
            Assert.That(r.ReputationPenalty, Is.EqualTo(Cfg.BreachAlliancePenalty), "背盟声誉重罚。");
            Assert.That(_svc.Breach(DiplomaticStanceState.Empty, Wu, Cfg).ReputationPenalty, Is.EqualTo(0), "中立无背约、无罚。");
        }
    }
}
