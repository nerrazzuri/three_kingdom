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
    /// epic-015 story-003：败支后果——在野延续存读档 + 部曲保留（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（败支生成合法可继续状态）+ ADR-0008（失城经004）+ ADR-0005（存档 round-trip）。
    /// TR-session-004 / TR-career-003 / TR-career-004 / TR-career-005。
    /// 覆盖：defeat → Advance → 存读档哈希一致（补 SaveTests 仅 advance-before-defeat 的缺口）；
    /// 在野态/无归属/部曲保留/城权经004易主 全部持久化。
    /// </summary>
    [TestFixture]
    public class CampaignOpeningDefeatBranchTests
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
        private static GovernorStartConfig Win() => new GovernorStartConfig(initialMerit: 30, initialLordStanding: Frac(1, 10));
        private static SiegeContext Fall() => new SiegeContext(Fanshui, Enemy, new Garrison(500));

        // ---- AC-1: 败支 Advance 后存读档哈希一致（核心 gap 补充）----

        [Test]
        public void test_defeat_then_advance_roundtrip_preserves_hash()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());
            Service.Advance(s, 1);                       // 失城后继续推进一段——SaveTests 未覆盖此序
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
            Assert.That(loaded.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn).Advance(1)), "推进态保留");
        }

        // ---- AC-2: 败支存读档后在野态持久化 ----

        [Test]
        public void test_defeat_roundtrip_preserves_unaffiliated_state()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.Career.Career.IsUnaffiliated, Is.True, "在野态保留");
            Assert.That(loaded.Career.Career.Faction, Is.Null, "无归属保留");
        }

        // ---- AC-3: 败支存读档后部曲保留（TR-career-003 RetinueState round-trip）----

        [Test]
        public void test_defeat_roundtrip_preserves_retinue_members()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            Assert.That(s.Career.Retinue.IsMember(Aide), Is.True, "败后保留核心部曲");

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.Career.Retinue.IsMember(Aide), Is.True, "部曲经存读档保留");
        }

        // ---- AC-4: 败支城池归属经 GDD_004 路径易主（ADR-0008 合规验证）----

        [Test]
        public void test_defeat_roundtrip_preserves_city_ownership_transfer_to_enemy()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy), "失城——归属经004转夺城方");
        }

        // ---- 副验证：多次存读档归属不漂移 ----

        [Test]
        public void test_defeat_double_roundtrip_keeps_ownership_stable()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            CampaignSession once = Service.Restore(Service.CaptureSnapshot(s), Fp);
            CampaignSession twice = Service.Restore(Service.CaptureSnapshot(once), Fp);

            Assert.That(twice.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy), "二次存读档归属不漂移");
            Assert.That(twice.ComputeHash(), Is.EqualTo(s.ComputeHash()), "二次 round-trip 哈希仍一致");
        }
    }
}
