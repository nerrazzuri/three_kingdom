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
    /// epic-015 story-001：开局围城续局可用性——胜败两支 Advance 均可执行（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配边界，只编排不拥规则）+ ADR-0004（确定性）。TR-session-004。
    /// 覆盖：胜支/败支 ResolveSiege 后 Advance 不抛异常；时间正确递增；在野态推进后不自动复职。
    /// 这补充了 ConsequenceTransactionTests（career/城权内容）——本套聚焦「后果提交后继续推进」可用性。
    /// </summary>
    [TestFixture]
    public class CampaignOpeningContinuabilityTests
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

        // ---- AC-1: 胜支 Advance 可执行 ----

        [Test]
        public void test_advance_after_victory_succeeds_without_exception()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Defended, Win(), Fall());

            Assert.DoesNotThrow(() => Service.Advance(s, 1));
        }

        // ---- AC-2: 败支 Advance 可执行（败局可继续——control-manifest §失败必须产生可继续状态）----

        [Test]
        public void test_advance_after_defeat_succeeds_without_exception()
        {
            CampaignSession s = NewSession();
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            // 在野态不阻止推进：这是 GDD_014「败局可继续」的装配层核心验证
            Assert.DoesNotThrow(() => Service.Advance(s, 1));
        }

        // ---- AC-3: 胜支推进后世界时间正确递增 ----

        [Test]
        public void test_victory_advance_increments_world_time_by_one_segment()
        {
            CampaignSession s = NewSession();
            WorldTime initialTime = s.CurrentTime;
            Service.ResolveSiege(s, SiegeOutcome.Defended, Win(), Fall());

            Service.Advance(s, 1);

            Assert.That(s.CurrentTime, Is.EqualTo(initialTime.Advance(1)));
        }

        // ---- AC-4 + AC-5: 败支推进后世界时间递增，且在野态不变 ----

        [Test]
        public void test_defeat_advance_increments_world_time_and_career_remains_unaffiliated()
        {
            CampaignSession s = NewSession();
            WorldTime initialTime = s.CurrentTime;
            Service.ResolveSiege(s, SiegeOutcome.Fallen, Win(), Fall());

            Assert.That(s.Career.Career.IsUnaffiliated, Is.True, "败后即转在野");

            Service.Advance(s, 1);

            Assert.That(s.CurrentTime, Is.EqualTo(initialTime.Advance(1)), "世界时间推进 1 段");
            Assert.That(s.Career.Career.IsUnaffiliated, Is.True, "推进不自动复职");
        }
    }
}
