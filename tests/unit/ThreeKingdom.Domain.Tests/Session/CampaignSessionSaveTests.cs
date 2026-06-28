using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-013 story-004：统一会话存档信封 round-trip（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（版本化 + 校验）+ ADR-0009（R-1 统一信封）。TR-session-003。
    /// 覆盖 捕获→恢复 逐字段+哈希一致、含后果的非平凡态 round-trip、版本/指纹校验整体拒绝。
    /// </summary>
    [TestFixture]
    public class CampaignSessionSaveTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-siege", Fp,
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
        public void test_capture_restore_roundtrip_preserves_session_hash()
        {
            CampaignSession s = NewSession();
            string text = Service.CaptureSnapshot(s);
            CampaignSession loaded = Service.Restore(text, Fp);

            Assert.That(loaded.Id, Is.EqualTo(s.Id));
            Assert.That(loaded.ScenarioConfigId, Is.EqualTo(s.ScenarioConfigId));
            Assert.That(loaded.ComputeHash(), Is.EqualTo(s.ComputeHash()));
            Assert.That(loaded.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player));
        }

        [Test]
        public void test_roundtrip_after_advance_and_siege_loss()
        {
            CampaignSession s = NewSession();
            Service.Advance(s, 3);
            Service.ResolveSiege(s, SiegeOutcome.Fallen, new GovernorStartConfig(30, Frac(1, 10)),
                new SiegeContext(Fanshui, Enemy, new Garrison(500)));
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
            Assert.That(loaded.Career.Career.IsUnaffiliated, Is.True);              // 在野态保留
            Assert.That(loaded.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy)); // 失城态保留
            Assert.That(loaded.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn).Advance(3)));
        }

        [Test]
        public void test_advance_after_restore_matches_direct_advance()
        {
            CampaignSession direct = NewSession();
            Service.Advance(direct, 2);
            StateHash directHash = direct.ComputeHash();

            CampaignSession restored = Service.Restore(Service.CaptureSnapshot(NewSession()), Fp);
            Service.Advance(restored, 2);

            Assert.That(restored.ComputeHash(), Is.EqualTo(directHash)); // 读档后续推进=直推
        }

        [Test]
        public void test_fingerprint_mismatch_is_rejected()
        {
            string text = Service.CaptureSnapshot(NewSession());
            Assert.Throws<SaveFormatException>(() => Service.Restore(text, new ConfigFingerprint(0xBADUL)));
        }

        [Test]
        public void test_corrupt_snapshot_is_rejected()
        {
            Assert.Throws<SaveFormatException>(() => Service.Restore("not-a-session-save", Fp));
        }
    }
}
