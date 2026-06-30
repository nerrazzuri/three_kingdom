using System;
using System.Collections.Generic;
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
    /// epic-022 共享夹具 + story-001：功绩累积接入会话（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（命令接入）+ ADR-0004（确定性）。TR-career-002/001。
    /// </summary>
    [TestFixture]
    public class CampaignMeritAccrualTests
    {
        internal static readonly FactionId Player = new FactionId("faction-player");
        internal static readonly FactionId Enemy = new FactionId("faction-yuan");
        internal static readonly CharacterId Lord = new CharacterId("char-player-lord");
        internal static readonly CharacterId Aide = new CharacterId("char-aide");
        internal static readonly CityId Fanshui = new CityId("city-fanshui");
        internal static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        internal static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        // 低门槛 ladder：阶1 merit40/renown10/standing0.01（一次 CombatVictory 即达）；阶2+ 不可达。
        internal static PromotionLadderConfig Ladder()
        {
            var merit = new[] { 0, 40, 9999, 9999, 9999, 9999, 9999, 9999 };
            var renown = new[] { 0, 10, 9999, 9999, 9999, 9999, 9999, 9999 };
            var standing = new[]
            {
                FixedPoint.Zero, Frac(1, 100), FixedPoint.One, FixedPoint.One,
                FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One,
            };
            var gains = new Dictionary<CareerGainSource, CareerGain>
            {
                [CareerGainSource.CombatVictory] = new CareerGain(40, 10, Frac(2, 100)),
                [CareerGainSource.MajorBattleVictory] = new CareerGain(80, 50, Frac(5, 100)),
                [CareerGainSource.LordMissionComplete] = new CareerGain(45, 15, Frac(3, 100)),   // 非战斗，竞争力
                [CareerGainSource.CityGovernance] = new CareerGain(40, 12, Frac(2, 100)),
                [CareerGainSource.RebellionSuppressed] = new CareerGain(60, 40, Frac(4, 100)),
                [CareerGainSource.TalentRecruited] = new CareerGain(20, 35, Frac(2, 100)),
            };
            return new PromotionLadderConfig(merit, renown, standing, gains);
        }

        internal static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-career", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) });

        internal static readonly CampaignSessionService Service = new CampaignSessionService();
        internal static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;

        // ---- AC-1: 战斗功绩累积 ----

        [Test]
        public void test_combat_gain_accrues_merit()
        {
            CampaignSession s = NewSession();
            int before = s.Career.Career.Merit;

            CareerCommandResult r = Service.ApplyCareerGain(s, Ladder(), CareerGainSource.CombatVictory);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.Career.Career.Merit, Is.GreaterThan(before), "功绩累积 merit 增加");
        }

        // ---- AC-2: 非战斗功绩源成长（竞争力）----

        [Test]
        public void test_non_combat_source_accrues_competitively()
        {
            CampaignSession combat = NewSession();
            CampaignSession mission = NewSession();

            Service.ApplyCareerGain(combat, Ladder(), CareerGainSource.CombatVictory);
            Service.ApplyCareerGain(mission, Ladder(), CareerGainSource.LordMissionComplete);

            Assert.That(mission.Career.Career.Merit, Is.GreaterThan(0), "非战斗源也成长");
            // 非战斗源 merit 增量与战斗源可比（45 vs 40），不被边缘化。
            Assert.That(mission.Career.Career.Merit, Is.GreaterThanOrEqualTo(combat.Career.Career.Merit),
                "非战斗源 merit 速率有竞争力");
        }

        // ---- AC-3: 累积确定性 ----

        [Test]
        public void test_accrual_is_deterministic()
        {
            CampaignSession a = NewSession();
            CampaignSession b = NewSession();
            Service.ApplyCareerGain(a, Ladder(), CareerGainSource.CombatVictory);
            Service.ApplyCareerGain(b, Ladder(), CareerGainSource.CombatVictory);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同来源同累积 → 同哈希");
        }

        // ---- AC-4: 多次累积叠加 ----

        [Test]
        public void test_repeated_accrual_stacks()
        {
            CampaignSession s = NewSession();
            Service.ApplyCareerGain(s, Ladder(), CareerGainSource.CombatVictory);
            int afterOne = s.Career.Career.Merit;
            Service.ApplyCareerGain(s, Ladder(), CareerGainSource.CombatVictory);

            Assert.That(s.Career.Career.Merit, Is.GreaterThan(afterOne), "多次累积单调增");
        }
    }
}
