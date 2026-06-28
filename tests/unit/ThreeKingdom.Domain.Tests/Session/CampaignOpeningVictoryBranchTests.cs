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
    /// epic-015 story-002：胜支后果——配置驱动生涯初值 + 胜支存读档（Integration / Assembly）。
    /// 治理 ADR：ADR-0008（城权只读经004）+ ADR-0009（装配层不硬编码初值）+ ADR-0005（存档 round-trip）。
    /// TR-session-004 / TR-career-001 / TR-career-004。
    /// 覆盖：不同 GovernorStartConfig → 不同 career 初值（数据驱动）；胜支 round-trip 后非在野态/城归玩家/哈希一致。
    /// 补充 CampaignSessionSaveTests（仅有败支 round-trip）的胜支存读档缺口。
    /// </summary>
    [TestFixture]
    public class CampaignOpeningVictoryBranchTests
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
        private static SiegeContext Fall() => new SiegeContext(Fanshui, Enemy, new Garrison(500));

        // ---- AC-1: 非默认 config 产生对应 merit（数据驱动，非硬编码）----

        [Test]
        public void test_victory_merit_comes_from_config_not_hardcoded()
        {
            CampaignSession s = NewSession();
            // 用与默认（30/0.1）不同的值，证明初值来自传入配置
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2, 10)), Fall());

            Assert.That(s.Career.Career.Merit, Is.EqualTo(50));
        }

        [Test]
        public void test_victory_zero_merit_config_yields_zero_merit()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(0, Frac(1, 10)), Fall());

            Assert.That(s.Career.Career.Merit, Is.EqualTo(0), "允许零功绩配置");
        }

        // ---- AC-2: 非默认 config 产生对应 lord standing ----

        [Test]
        public void test_victory_lord_standing_comes_from_config()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2, 10)), Fall());

            Assert.That(s.Career.Career.LordStanding, Is.EqualTo(Frac(2, 10)));
        }

        // ---- AC-3: 胜支存读档后 career 保持非在野 ----

        [Test]
        public void test_victory_roundtrip_preserves_non_unaffiliated_career()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2, 10)), Fall());

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.Career.Career.IsUnaffiliated, Is.False, "胜支留任，非在野");
            Assert.That(loaded.Career.Career.Merit, Is.EqualTo(s.Career.Career.Merit), "merit 完整恢复");
            Assert.That(loaded.Career.Career.LordStanding, Is.EqualTo(s.Career.Career.LordStanding), "standing 完整恢复");
        }

        // ---- AC-4: 胜支存读档后城池归属保持 Player（经 004 路径）----

        [Test]
        public void test_victory_roundtrip_preserves_city_ownership_to_player()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2, 10)), Fall());

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player), "胜支守住城——归属仍属玩家");
        }

        // ---- AC-5: 胜支 round-trip 哈希一致 ----

        [Test]
        public void test_victory_roundtrip_preserves_hash()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, new GovernorStartConfig(50, Frac(2, 10)), Fall());
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- 副验证：不同 merit config → 不同哈希（哈希纳入 career 状态，ADR-0004）----

        [Test]
        public void test_different_merit_config_yields_different_hash()
        {
            CampaignSession low = NewSession();
            Service.ResolveSiege(low, SiegeOutcome.Defended, new GovernorStartConfig(10, Frac(1, 10)), Fall());

            CampaignSession high = NewSession();
            Service.ResolveSiege(high, SiegeOutcome.Defended, new GovernorStartConfig(90, Frac(1, 10)), Fall());

            Assert.That(low.ComputeHash(), Is.Not.EqualTo(high.ComputeHash()), "merit 纳入状态哈希");
        }
    }
}
