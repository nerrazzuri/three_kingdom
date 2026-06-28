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
    /// epic-015 story-004：两支 E2E 确定性——同种子同 hash + 两结果不同 hash（Integration / Assembly）。
    /// 治理 ADR：ADR-0004（确定性，状态哈希覆盖全部权威态）+ ADR-0005（存档不中断确定性链）。
    /// TR-session-005 / TR-session-004 / TR-career-003。
    /// 覆盖：胜/败支各自重放 → 同哈希；两支从同开局 → 不同哈希（区分力）；
    /// 存档切割点不影响后续推进确定性（restore 后续推 = 直推）。
    /// </summary>
    [TestFixture]
    public class CampaignOpeningDeterminismTests
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

        // ---- AC-1: 胜支重放产生相同哈希 ----

        [Test]
        public void test_victory_branch_replays_to_same_hash()
        {
            CampaignSession s1 = NewSession();
            Service.ResolveSiege(s1, SiegeOutcome.Defended, Win(), Fall());

            CampaignSession s2 = NewSession();
            Service.ResolveSiege(s2, SiegeOutcome.Defended, Win(), Fall());

            Assert.That(s1.ComputeHash(), Is.EqualTo(s2.ComputeHash()), "同配置胜支重放 → 同哈希");
        }

        // ---- AC-2: 败支重放产生相同哈希 ----

        [Test]
        public void test_defeat_branch_replays_to_same_hash()
        {
            CampaignSession s1 = NewSession();
            Service.ResolveSiege(s1, SiegeOutcome.Fallen, Win(), Fall());

            CampaignSession s2 = NewSession();
            Service.ResolveSiege(s2, SiegeOutcome.Fallen, Win(), Fall());

            Assert.That(s1.ComputeHash(), Is.EqualTo(s2.ComputeHash()), "同配置败支重放 → 同哈希");
        }

        // ---- AC-3: 胜支与败支从同开局产生不同哈希（哈希区分力）----

        [Test]
        public void test_victory_and_defeat_yield_different_hashes()
        {
            CampaignSession win = NewSession();
            Service.ResolveSiege(win, SiegeOutcome.Defended, Win(), Fall());

            CampaignSession lose = NewSession();
            Service.ResolveSiege(lose, SiegeOutcome.Fallen, Win(), Fall());

            Assert.That(win.ComputeHash(), Is.Not.EqualTo(lose.ComputeHash()), "不同决策路径 → 不同状态哈希");
        }

        // ---- AC-4: 存档不中断确定性链（TR-session-005 Session 层核心验证）----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：败支 → Advance(2)
            CampaignSession direct = NewSession();
            Service.ResolveSiege(direct, SiegeOutcome.Fallen, Win(), Fall());
            Service.Advance(direct, 2);
            StateHash directHash = direct.ComputeHash();

            // 切割：败支 → Advance(1) → 存读档 → Advance(1)
            CampaignSession restored = NewSession();
            Service.ResolveSiege(restored, SiegeOutcome.Fallen, Win(), Fall());
            Service.Advance(restored, 1);
            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(restored), Fp);
            Service.Advance(loaded, 1);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续推进确定性");
        }
    }
}
