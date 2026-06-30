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
    /// epic-017 story-002：侦察命令经会话路径（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（命令路径，前置校验失败零写入）+ ADR-0004（确定性）。TR-intel-002/001。
    /// 覆盖：侦察更新知识；非法对象稳定错误码无写入；确定性；知识增长可观察（反全知）。
    /// </summary>
    [TestFixture]
    public class CampaignScoutCommandTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly IntelSubjectId EnemyArmy = new IntelSubjectId("subject-enemy-army");
        private static readonly IntelSubjectId Unregistered = new IntelSubjectId("subject-unknown");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static IntelConfig IntelCfg()
            => new IntelConfig(
                new Dictionary<IntelSource, FixedPoint> { [IntelSource.Scouting] = Frac(8, 10), [IntelSource.DirectObservation] = Frac(9, 10) },
                baseError: 0, ttlSegments: 8, baseExposure: Frac(2, 10),
                exposureAlertWeight: Frac(1, 10), exposureSkillWeight: Frac(1, 10));

        private static WorldTruthLedger Truth(int enemyStrength = 5000)
        {
            var t = new WorldTruthLedger();
            t.Set(new TruthRecord(EnemyArmy, enemyStrength, Enemy));
            return t;
        }

        private static CampaignStartConfig Config()
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
                worldTruth: Truth(),
                playerIntel: new FactionIntel(Player),
                intelConfig: IntelCfg());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;

        // ---- AC-1: 侦察更新玩家知识 ----

        [Test]
        public void test_scout_updates_player_knowledge()
        {
            CampaignSession s = NewSession();
            Assert.That(s.PlayerKnowledge!.Knows(EnemyArmy), Is.False, "侦察前未知");

            CampaignCommandResult r = Service.Scout(s, EnemyArmy, IntelSource.Scouting);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.PlayerKnowledge!.Knows(EnemyArmy), Is.True, "侦察后已知");
        }

        [Test]
        public void test_repeated_scout_refreshes_without_error()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);
            CampaignCommandResult again = Service.Scout(s, EnemyArmy, IntelSource.Scouting);

            Assert.That(again.Applied, Is.True, "重复侦察刷新不报错");
        }

        // ---- AC-2: 非法对象稳定错误码 + 无写入 ----

        [Test]
        public void test_scout_unregistered_subject_rejected_no_write()
        {
            CampaignSession s = NewSession();
            StateHash before = s.ComputeHash();

            CampaignCommandResult r = Service.Scout(s, Unregistered, IntelSource.Scouting);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.UnknownIntelSubject));
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "非法侦察零写入，哈希不变");
        }

        [Test]
        public void test_scout_on_intel_disabled_session_rejected()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            CampaignCommandResult r = Service.Scout(s, EnemyArmy, IntelSource.Scouting);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.IntelDisabled));
        }

        // ---- AC-3: 侦察解析确定性 ----

        [Test]
        public void test_scout_is_deterministic()
        {
            CampaignSession a = NewSession();
            CampaignSession b = NewSession();

            Service.Scout(a, EnemyArmy, IntelSource.Scouting);
            Service.Scout(b, EnemyArmy, IntelSource.Scouting);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同态同侦察 → 同哈希");
        }

        // ---- AC-4: 知识增长可观察（反全知，来自报告路径）----

        [Test]
        public void test_scouted_knowledge_comes_from_report_path()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);

            Assert.That(s.PlayerKnowledge!.TryGet(EnemyArmy, out IntelKnowledgeEntry entry), Is.True);
            Assert.That(entry.Source, Is.EqualTo(IntelSource.Scouting), "知识来自侦察报告路径");
            Assert.That(entry.ObservedAt, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)), "观察时间=侦察时刻");
        }
    }
}
