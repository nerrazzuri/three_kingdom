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
    /// epic-013 story-002：日界推进复用全局结算顺序（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（Day Boundary Order）+ ADR-0004（确定性）。TR-session-001。
    /// 覆盖 AC：按全局序 Meta 层确定性推进（时间→世界 015），同前态同序列→同哈希，顺序/量敏感，负值拒。
    /// </summary>
    [TestFixture]
    public class CampaignDayBoundaryTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

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

        [Test]
        public void test_advance_moves_world_time_deterministically()
        {
            CampaignSession s = NewSession();
            Service.Advance(s, 1);
            Service.Advance(s, 2);
            Assert.That(s.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn).Advance(3)));
        }

        [Test]
        public void test_same_state_same_advance_sequence_yields_identical_hash()
        {
            CampaignSession a = NewSession();
            Service.Advance(a, 1); Service.Advance(a, 2); Service.Advance(a, 1);
            CampaignSession b = NewSession();
            Service.Advance(b, 1); Service.Advance(b, 2); Service.Advance(b, 1);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));
            Assert.That(a.CurrentTime, Is.EqualTo(b.CurrentTime));
        }

        [Test]
        public void test_different_advance_total_yields_different_hash()
        {
            CampaignSession a = NewSession(); Service.Advance(a, 2);
            CampaignSession b = NewSession(); Service.Advance(b, 5);
            Assert.That(a.ComputeHash(), Is.Not.EqualTo(b.ComputeHash()));
        }

        [Test]
        public void test_zero_advance_preserves_hash()
        {
            CampaignSession s = NewSession();
            StateHash before = s.ComputeHash();
            Service.Advance(s, 0);
            Assert.That(s.ComputeHash(), Is.EqualTo(before));
        }

        [Test]
        public void test_negative_advance_is_rejected()
        {
            CampaignSession s = NewSession();
            Assert.Throws<ArgumentOutOfRangeException>(() => Service.Advance(s, -1));
        }

        [Test]
        public void test_advance_preserves_ownership_projection()
        {
            // 时间推进不改归属（归属只经 004 事件）；世界投影仍反映开局归属。
            CampaignSession s = NewSession();
            Service.Advance(s, 4);
            Assert.That(s.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player));
        }
    }
}
