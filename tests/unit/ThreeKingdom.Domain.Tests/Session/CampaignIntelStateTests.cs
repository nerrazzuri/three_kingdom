using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-017 story-001：情报态接入会话（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配，四层分离只读阵营知识）+ ADR-0004（确定性）。TR-intel-001。
    /// 覆盖：会话持真值+知识；只读出口仅阵营知识不暴露真值（反全知）；情报态入哈希；可选向后兼容。
    /// </summary>
    [TestFixture]
    public class CampaignIntelStateTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly IntelSubjectId EnemyArmy = new IntelSubjectId("subject-enemy-army");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static IntelConfig IntelCfg()
            => new IntelConfig(
                new Dictionary<IntelSource, FixedPoint> { [IntelSource.Scouting] = Frac(8, 10), [IntelSource.DirectObservation] = Frac(9, 10) },
                baseError: 0, ttlSegments: 8, baseExposure: Frac(2, 10),
                exposureAlertWeight: Frac(1, 10), exposureSkillWeight: Frac(1, 10));

        // 真值含敌军（玩家未知）；玩家知识初始为空。
        private static WorldTruthLedger Truth(int enemyStrength = 5000)
        {
            var t = new WorldTruthLedger();
            t.Set(new TruthRecord(EnemyArmy, enemyStrength, Enemy));
            return t;
        }

        private static CampaignStartConfig Config(WorldTruthLedger? truth = null, FactionIntel? playerIntel = null)
            => new CampaignStartConfig(
                "scenario-fanshui-intel", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                worldTruth: truth ?? Truth(),
                playerIntel: playerIntel ?? new FactionIntel(Player),
                intelConfig: IntelCfg());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession(WorldTruthLedger? truth = null, FactionIntel? playerIntel = null)
            => Service.StartCampaign(Config(truth, playerIntel)).Session!;

        // ---- AC-1: 会话持有情报态 ----

        [Test]
        public void test_session_holds_intel_state()
        {
            CampaignSession s = NewSession();
            Assert.That(s.HasIntel, Is.True);
            Assert.That(s.PlayerKnowledge, Is.Not.Null);
        }

        // ---- AC-2: 只读出口不暴露真值（反全知）----

        [Test]
        public void test_player_knowledge_does_not_expose_truth()
        {
            // 真值含敌军，但玩家未侦察 → 知识不含该主题（只读投影 Knows==false）。
            CampaignSession s = NewSession();

            Assert.That(s.PlayerKnowledge!.Knows(EnemyArmy), Is.False, "未侦察则阵营知识不含敌军（真值不泄露）");
            Assert.That(s.PlayerKnowledge!.Count, Is.EqualTo(0));
        }

        // ---- AC-3: 情报态纳入会话哈希 ----

        [Test]
        public void test_intel_truth_enters_session_hash()
        {
            CampaignSession weak = NewSession(truth: Truth(enemyStrength: 3000));
            CampaignSession strong = NewSession(truth: Truth(enemyStrength: 9000));

            Assert.That(weak.ComputeHash(), Is.Not.EqualTo(strong.ComputeHash()), "真值进哈希");
        }

        [Test]
        public void test_identical_intel_state_yields_same_hash()
        {
            CampaignSession a = NewSession(truth: Truth(enemyStrength: 5000));
            CampaignSession b = NewSession(truth: Truth(enemyStrength: 5000));

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));
        }

        [Test]
        public void test_player_knowledge_difference_enters_hash()
        {
            var knownIntel = new FactionIntel(Player);
            knownIntel.ApplyReport(new IntelReport(EnemyArmy, Player, 4800, IntelSource.Scouting, new WorldTime(0, DaySegment.Dawn)));

            CampaignSession known = NewSession(playerIntel: knownIntel);
            CampaignSession unknown = NewSession(playerIntel: new FactionIntel(Player));

            Assert.That(known.ComputeHash(), Is.Not.EqualTo(unknown.ComputeHash()), "玩家知识进哈希");
            Assert.That(known.PlayerKnowledge!.Knows(EnemyArmy), Is.True);
        }

        // ---- AC-4: 无情报配置向后兼容 ----

        [Test]
        public void test_session_without_intel_config_has_no_intel()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            Assert.That(s.HasIntel, Is.False);
            Assert.That(s.PlayerKnowledge, Is.Null);
        }
    }
}
