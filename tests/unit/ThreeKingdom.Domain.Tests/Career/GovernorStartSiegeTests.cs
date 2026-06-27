using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// epic-011 story-004：太守开局 + 守城事件胜败后果接入（Integration）。
    /// 治理 ADR：ADR-0008（城池控制权契约）+ ADR-0002（四层编排）。GDD_014 / TR-career-001、004。
    /// 覆盖 AC-1 开局绑定+CitySeed、AC-2 守城胜后果、AC-3/4 守城败转在野 + 归属经 004 事件、AC-5 确定性。
    /// </summary>
    [TestFixture]
    public class GovernorStartSiegeTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly CharacterId Aide = new CharacterId("char-aide");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CitySeed Seed()
            => new CitySeed(
                Cao, Fanshui, garrison: 800, fortification: 60, output: 20,
                new[] { new RetinueMember(Aide, Frac(7, 10)) });

        private static GovernorStartConfig WinConfig()
            => new GovernorStartConfig(initialMerit: 30, initialLordStanding: Frac(1, 10));

        private static SiegeContext FallContext()
            => new SiegeContext(Fanshui, Enemy, new Garrison(500));

        private static (GovernorCampaignService svc, CityControlAuthority authority) NewService()
        {
            var authority = new CityControlAuthority();
            return (new GovernorCampaignService(authority), authority);
        }

        // ---- AC-1：太守开局绑定 ----

        [Test]
        public void test_governor_start_binds_career_and_registers_city_ownership()
        {
            (GovernorCampaignService svc, CityControlAuthority authority) = NewService();

            CareerSnapshot start = svc.BeginGovernorStart(Seed());

            Assert.That(start.Career.Faction, Is.EqualTo(Cao));
            Assert.That(start.Career.Rank, Is.EqualTo(Rank.CityGovernor));
            Assert.That(start.Career.IsUnaffiliated, Is.False);
            Assert.That(start.Retinue.IsMember(Aide), Is.True);
            // 开局城池归属登记到 GDD_004 权威。
            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Cao));
            Assert.That(authority.GarrisonOf(Fanshui), Is.EqualTo(new Garrison(800)));
        }

        [Test]
        public void test_city_seed_rejects_invalid_endowment()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CitySeed(Cao, Fanshui, garrison: -1, fortification: 60, output: 20, Array.Empty<RetinueMember>()));
        }

        // ---- AC-2：守城胜后果 ----

        [Test]
        public void test_siege_defended_grants_merit_standing_and_unlocks_authority()
        {
            (GovernorCampaignService svc, CityControlAuthority authority) = NewService();
            CareerSnapshot start = svc.BeginGovernorStart(Seed());

            SiegeResolutionResult r = svc.ResolveSiege(start, SiegeOutcome.Defended, WinConfig(), FallContext());

            Assert.That(r.Outcome, Is.EqualTo(SiegeOutcome.Defended));
            Assert.That(r.FullAuthorityUnlocked, Is.True);
            Assert.That(r.Snapshot.Career.Merit, Is.EqualTo(30));
            Assert.That(r.Snapshot.Career.LordStanding, Is.EqualTo(Frac(1, 10)));
            Assert.That(r.ControlChange, Is.Null);
            // 守城胜：城池归属不变。
            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Cao));
        }

        // ---- AC-3 / AC-4：守城败 + 归属经 GDD_004 事件 ----

        [Test]
        public void test_siege_fallen_makes_career_wandering_and_retains_retinue()
        {
            (GovernorCampaignService svc, _) = NewService();
            CareerSnapshot start = svc.BeginGovernorStart(Seed());

            SiegeResolutionResult r = svc.ResolveSiege(start, SiegeOutcome.Fallen, WinConfig(), FallContext());

            Assert.That(r.Outcome, Is.EqualTo(SiegeOutcome.Fallen));
            Assert.That(r.Snapshot.Career.IsUnaffiliated, Is.True);   // 罢官沦为在野，合法可继续
            Assert.That(r.Snapshot.Career.Faction, Is.Null);
            Assert.That(r.Snapshot.Retinue.IsMember(Aide), Is.True);  // 保留核心部曲
            Assert.That(r.FullAuthorityUnlocked, Is.False);
        }

        [Test]
        public void test_siege_fallen_changes_ownership_via_gdd004_control_event()
        {
            (GovernorCampaignService svc, CityControlAuthority authority) = NewService();
            CareerSnapshot start = svc.BeginGovernorStart(Seed());

            // 独立订阅，验证归属变更确由 GDD_004 控制权事件发布（非生涯层直接写）。
            var published = new List<CityControlChanged>();
            authority.Subscribe(published.Add);

            SiegeResolutionResult r = svc.ResolveSiege(start, SiegeOutcome.Fallen, WinConfig(), FallContext());

            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Enemy));        // 归属转夺城方
            Assert.That(published, Has.Count.EqualTo(1));
            Assert.That(published[0].Cause, Is.EqualTo(ChangeCause.SiegeDefenseLost));
            Assert.That(published[0].NewOwner, Is.EqualTo(Enemy));
            Assert.That(r.ControlChange, Is.Not.Null);
            Assert.That(r.ControlChange!.City, Is.EqualTo(Fanshui));
        }

        // ---- AC-5：确定性写回 ----

        [Test]
        public void test_defended_outcome_is_deterministic()
        {
            StateHash A()
            {
                (GovernorCampaignService svc, _) = NewService();
                CareerSnapshot start = svc.BeginGovernorStart(Seed());
                return svc.ResolveSiege(start, SiegeOutcome.Defended, WinConfig(), FallContext()).Snapshot.ComputeHash();
            }
            Assert.That(A(), Is.EqualTo(A()));
        }

        [Test]
        public void test_fallen_outcome_is_deterministic()
        {
            StateHash A()
            {
                (GovernorCampaignService svc, _) = NewService();
                CareerSnapshot start = svc.BeginGovernorStart(Seed());
                return svc.ResolveSiege(start, SiegeOutcome.Fallen, WinConfig(), FallContext()).Snapshot.ComputeHash();
            }
            Assert.That(A(), Is.EqualTo(A()));
        }
    }
}
