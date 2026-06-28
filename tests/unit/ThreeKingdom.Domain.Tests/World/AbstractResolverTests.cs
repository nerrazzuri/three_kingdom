using System;
using NUnit.Framework;
using ThreeKingdom.Application.World;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-012 story-005：抽象结算器（玩家不在场势力混战）。
    /// 治理 ADR：ADR-0007 §4（抽象结算）+ ADR-0004（注入随机确定性）+ ADR-0008（易主经 004）。GDD_015 / TR-world-004。
    /// 覆盖 AC-1/2 确定性+体量加权、AC-3 够得着/够不着边界、AC-4 易主经 GDD_004（守住不触发）。
    /// </summary>
    [TestFixture]
    public class AbstractResolverTests
    {
        private static readonly CityId Contested = new CityId("city-runan");
        private static readonly StrengthAbstractResolver Resolver = new StrengthAbstractResolver();

        private static FactionRecord Faction(string id, int cityCount)
        {
            var cities = new CityId[cityCount];
            for (int i = 0; i < cityCount; i++) cities[i] = new CityId($"{id}-c{i}");
            return new FactionRecord(
                new FactionId(id), new CharacterId($"{id}-lord"),
                SurvivalStatus.Active, RelationToPlayer.Neutral, cities);
        }

        private static ContestContext Ctx(FixedPoint bias)
            => new ContestContext(Contested, bias, new Garrison(300));

        private static StateHash HashOf(AbstractOutcome o)
        {
            var h = new StateHasher();
            o.AppendTo(h);
            return h.ToHash();
        }

        // ---- AC-1 / AC-2：确定性 + 体量加权 ----

        [Test]
        public void test_resolution_is_deterministic_for_same_seed_and_position()
        {
            FactionRecord a = Faction("faction-a", 5), b = Faction("faction-b", 5);
            AbstractOutcome o1 = Resolver.Resolve(a, b, Ctx(FixedPoint.One), new DeterministicRandom(42));
            AbstractOutcome o2 = Resolver.Resolve(a, b, Ctx(FixedPoint.One), new DeterministicRandom(42));

            Assert.That(o1.Kind, Is.EqualTo(o2.Kind));
            Assert.That(HashOf(o1), Is.EqualTo(HashOf(o2)));
        }

        [Test]
        public void test_dominant_attacker_wins_majority_of_seeds()
        {
            // 体量悬殊：攻方 20 城 vs 守方 0 城（threshold≈0.95）→ 多数随机种子下攻方占据。
            FactionRecord strong = Faction("faction-strong", 20), weak = Faction("faction-weak", 0);
            int attackerWins = 0;
            for (ulong seed = 0; seed < 100; seed++)
            {
                AbstractOutcome o = Resolver.Resolve(strong, weak, Ctx(FixedPoint.One), new DeterministicRandom(seed));
                if (o.Kind == AbstractOutcomeKind.AttackerTakes) attackerWins++;
            }
            Assert.That(attackerWins, Is.GreaterThan(80), "体量悬殊时强势力应高概率占据。");
        }

        [Test]
        public void test_zero_attacker_bias_always_defender_holds()
        {
            // 攻方加成 0 → 攻方强度 0 → 占据概率 0 → 必守住（确定）。
            FactionRecord a = Faction("faction-a", 50), b = Faction("faction-b", 1);
            for (ulong seed = 0; seed < 20; seed++)
            {
                AbstractOutcome o = Resolver.Resolve(a, b, Ctx(FixedPoint.Zero), new DeterministicRandom(seed));
                Assert.That(o.Kind, Is.EqualTo(AbstractOutcomeKind.DefenderHolds));
                Assert.That(o.OwnershipChanged, Is.False);
            }
        }

        // ---- AC-3：够得着 vs 够不着边界 ----

        [Test]
        public void test_policy_uses_abstract_only_when_both_unreachable()
        {
            FactionRecord a = Faction("faction-a", 3), b = Faction("faction-b", 3);
            var none = PlayerReach.None;
            var reachA = new PlayerReach(new[] { new FactionId("faction-a") }, Array.Empty<CityId>());

            Assert.That(AbstractContestPolicy.ShouldResolveAbstractly(a, b, none), Is.True);   // 双方够不着 → 抽象
            Assert.That(AbstractContestPolicy.ShouldResolveAbstractly(a, b, reachA), Is.False); // 一方够得着 → 走 GDD_016
        }

        // ---- AC-4：易主经 GDD_004（守住不触发）----

        [Test]
        public void test_attacker_take_applies_ownership_via_gdd004()
        {
            var authority = new CityControlAuthority();
            authority.RegisterInitial(Contested, new FactionId("faction-b"), new Garrison(200));
            var service = new AbstractContestService(new FixedResolver(AbstractOutcomeKind.AttackerTakes), authority);

            AbstractOutcome o = service.ResolveAndApply(
                Faction("faction-a", 5), Faction("faction-b", 1), Ctx(FixedPoint.One), new DeterministicRandom(1));

            Assert.That(o.OwnershipChanged, Is.True);
            Assert.That(authority.OwnerOf(Contested), Is.EqualTo(new FactionId("faction-a"))); // 经 004 落地
        }

        [Test]
        public void test_defender_hold_does_not_change_ownership()
        {
            var authority = new CityControlAuthority();
            authority.RegisterInitial(Contested, new FactionId("faction-b"), new Garrison(200));
            int events = 0;
            authority.Subscribe(_ => events++);
            var service = new AbstractContestService(new FixedResolver(AbstractOutcomeKind.DefenderHolds), authority);

            service.ResolveAndApply(
                Faction("faction-a", 5), Faction("faction-b", 1), Ctx(FixedPoint.One), new DeterministicRandom(1));

            Assert.That(authority.OwnerOf(Contested), Is.EqualTo(new FactionId("faction-b"))); // 不变
            Assert.That(events, Is.EqualTo(0)); // 无控制权事件
        }

        /// <summary>测试桩：返回固定结局类别，隔离 Application 路由测试（不依赖 rng）。</summary>
        private sealed class FixedResolver : IAbstractResolver
        {
            private readonly AbstractOutcomeKind _kind;
            public FixedResolver(AbstractOutcomeKind kind) => _kind = kind;

            public AbstractOutcome Resolve(FactionRecord attacker, FactionRecord defender, ContestContext ctx, IDeterministicRandom rng)
                => _kind == AbstractOutcomeKind.AttackerTakes
                    ? new AbstractOutcome(AbstractOutcomeKind.AttackerTakes, attacker.Id, defender.Id, ctx.ContestedCity, true)
                    : new AbstractOutcome(AbstractOutcomeKind.DefenderHolds, defender.Id, attacker.Id, ctx.ContestedCity, false);
        }
    }
}
