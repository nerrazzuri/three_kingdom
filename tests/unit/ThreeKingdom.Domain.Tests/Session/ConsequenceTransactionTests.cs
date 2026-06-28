using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-013 story-003：后果原子写回（ConsequenceTransaction，Integration / Assembly）。
    /// 治理 ADR：ADR-0009（R-6 原子写回）+ ADR-0008（归属经004）。TR-session-002/004。
    /// 覆盖 守城胜/败后果链、原子性（校验失败零应用哈希不变）、R-3 势力创建经015、失败可继续。
    /// </summary>
    [TestFixture]
    public class ConsequenceTransactionTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly FactionId NewState = new FactionId("faction-rebel");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly CityId Unregistered = new CityId("city-unknown");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-siege", new ConfigFingerprint(0xCA11AB1EUL),
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) });

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;
        private static GovernorStartConfig Win() => new GovernorStartConfig(initialMerit: 30, initialLordStanding: Frac(1, 10));
        private static SiegeContext Fall() => new SiegeContext(Fanshui, Enemy, new Garrison(500));

        // ---- 守城胜/败后果链 ----

        [Test]
        public void test_siege_defended_grants_career_and_keeps_ownership()
        {
            CampaignSession s = NewSession();
            CampaignCommandResult r = Service.ResolveSiege(s, SiegeOutcome.Defended, Win(), Fall());

            Assert.That(r.Applied, Is.True);
            Assert.That(s.Career.Career.Merit, Is.EqualTo(30));
            Assert.That(s.Career.Career.LordStanding, Is.EqualTo(Frac(1, 10)));
            Assert.That(s.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player)); // 归属不变
        }

        [Test]
        public void test_siege_fallen_makes_wandering_and_changes_ownership_via_gdd004()
        {
            CampaignSession s = NewSession();
            CampaignCommandResult r = Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            Assert.That(r.Applied, Is.True);
            Assert.That(s.Career.Career.IsUnaffiliated, Is.True);          // 罢官转在野，合法可继续
            Assert.That(s.Career.Career.Faction, Is.Null);
            Assert.That(s.Career.Retinue.IsMember(Aide), Is.True);          // 保留部曲
            Assert.That(s.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy)); // 失城经 004 → 世界投影同步
        }

        // ---- 原子性：校验失败 → 零应用、哈希不变 ----

        [Test]
        public void test_transaction_validation_failure_applies_nothing()
        {
            CampaignSession s = NewSession();
            StateHash before = s.ComputeHash();

            // 暂存一个合法生涯变更 + 一个非法控制权变更（城未登记）→ 校验阶段失败、零应用。
            CareerSnapshot bumped = new CareerStateService().Apply(s.Career, new GainMeritCommand(99, 0)).Snapshot;
            CampaignCommandResult r = Service.BeginConsequence(s)
                .StageCareer(bumped)
                .StageControlChange(Unregistered, Enemy, new Garrison(100), ChangeCause.SiegeDefenseLost)
                .Commit();

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.InvalidConfig));
            Assert.That(s.Career.Career.Merit, Is.EqualTo(0));            // 生涯未被改（原子）
            Assert.That(s.ComputeHash(), Is.EqualTo(before));            // 整会话哈希不变
        }

        // ---- R-3：自立新势力创建经 GDD_015 ----

        [Test]
        public void test_faction_creation_routed_through_world_model()
        {
            CampaignSession s = NewSession();
            var newFaction = new FactionRecord(NewState, Lord, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>());

            CampaignCommandResult r = Service.BeginConsequence(s).StageFactionCreation(newFaction).Commit();

            Assert.That(r.Applied, Is.True);
            Assert.That(s.World.FactionById(NewState), Is.Not.Null);     // 015 创建势力
            Assert.That(s.World.FactionById(NewState)!.Relation, Is.EqualTo(RelationToPlayer.Self));
        }

        [Test]
        public void test_duplicate_faction_creation_is_rejected_atomically()
        {
            CampaignSession s = NewSession();
            StateHash before = s.ComputeHash();
            // Player 势力已存在 → 重复创建校验失败、零应用。
            var dup = new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>());

            CampaignCommandResult r = Service.BeginConsequence(s).StageFactionCreation(dup).Commit();

            Assert.That(r.Applied, Is.False);
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        [Test]
        public void test_resolve_siege_is_deterministic()
        {
            StateHash Run(SiegeOutcome o)
            {
                CampaignSession s = NewSession();
                Service.ResolveSiege(s, o, Win(), Fall());
                return s.ComputeHash();
            }
            Assert.That(Run(SiegeOutcome.Fallen), Is.EqualTo(Run(SiegeOutcome.Fallen)));
            Assert.That(Run(SiegeOutcome.Defended), Is.EqualTo(Run(SiegeOutcome.Defended)));
        }
    }
}
