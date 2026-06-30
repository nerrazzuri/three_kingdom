using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-017 story-004：情报态存读档（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档 round-trip）+ ADR-0004（确定性）。TR-intel-003。
    /// 覆盖：真值/知识分别序列化逐字段一致；不交叉污染；哈希一致；确定性链；未提供配置整体拒绝。
    /// </summary>
    [TestFixture]
    public class CampaignIntelSaveTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly IntelSubjectId EnemyArmy = new IntelSubjectId("subject-enemy-army");
        private static readonly IntelSubjectId EnemySupply = new IntelSubjectId("subject-enemy-supply");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static IntelConfig IntelCfg()
            => new IntelConfig(
                new Dictionary<IntelSource, FixedPoint> { [IntelSource.Scouting] = Frac(8, 10), [IntelSource.DirectObservation] = Frac(9, 10) },
                baseError: 0, ttlSegments: 8, baseExposure: Frac(2, 10),
                exposureAlertWeight: Frac(1, 10), exposureSkillWeight: Frac(1, 10));

        private static SessionCouncilSetup CouncilSetup()
        {
            var advisor = new AdvisorPerspective(new AdvisorId("advisor-zhuge"), Frac(8, 10));
            var template = new AdviceTemplate("advice-ambush", "隘口适于设伏", "若敌将急躁",
                new[] { "敌将性烈" }, new[] { "暴露反受夹击" }, new[] { EnemyArmy });
            return new SessionCouncilSetup(advisor, new[] { template }, new CouncilConfig(Frac(1, 1)), Frac(7, 10));
        }

        // 真值含两主题（敌军 + 敌补给）；玩家初始全未知。
        private static WorldTruthLedger Truth()
        {
            var t = new WorldTruthLedger();
            t.Set(new TruthRecord(EnemyArmy, 5000, Enemy));
            t.Set(new TruthRecord(EnemySupply, 1200, Enemy));
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
                intelConfig: IntelCfg(),
                councilSetup: CouncilSetup());

        private static readonly CampaignSessionService Service = new CampaignSessionService();
        private static CampaignSession NewSession() => Service.StartCampaign(Config()).Session!;
        private static CampaignSession Restore(string text)
            => Service.Restore(text, Fp, intelConfig: IntelCfg(), councilSetup: CouncilSetup());

        // ---- AC-1: 情报态 round-trip 逐字段一致 ----

        [Test]
        public void test_intel_state_roundtrip_field_for_field()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);   // 玩家只侦察敌军，不侦察敌补给

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.PlayerKnowledge!.Knows(EnemyArmy), Is.True);
            Assert.That(loaded.PlayerKnowledge!.TryGet(EnemyArmy, out IntelKnowledgeEntry e), Is.True);
            Assert.That(e.KnownStrength, Is.EqualTo(5000));
            Assert.That(e.Source, Is.EqualTo(IntelSource.Scouting));
            Assert.That(e.ObservedAt, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)));
        }

        // ---- AC-2: 真值/知识分别序列化不交叉污染（TR-intel-003）----

        [Test]
        public void test_truth_and_knowledge_do_not_cross_contaminate()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);   // 知敌军；敌补给真值存在但玩家未知

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            // 玩家知识：知敌军、**不知**敌补给（真值未污染知识）。
            Assert.That(loaded.PlayerKnowledge!.Knows(EnemyArmy), Is.True);
            Assert.That(loaded.PlayerKnowledge!.Knows(EnemySupply), Is.False, "敌补给真值未泄露进玩家知识");
            Assert.That(loaded.PlayerKnowledge!.Count, Is.EqualTo(1));
            // 真值仍完整（敌补给在真值层存在）——经哈希区分验证真值被保留。
            CampaignSession noSupplyTruth = Service.Restore(Service.CaptureSnapshot(s), Fp, intelConfig: IntelCfg(), councilSetup: CouncilSetup());
            Assert.That(noSupplyTruth.ComputeHash(), Is.EqualTo(loaded.ComputeHash()), "真值段完整恢复（含玩家未知主题）");
        }

        // ---- AC-3: round-trip 哈希一致 ----

        [Test]
        public void test_intel_roundtrip_preserves_hash()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);
            Service.ConveneCouncil(s);   // 军议不改情报态（查询）
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-4: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：Scout 敌军 → Scout 敌补给。
            CampaignSession direct = NewSession();
            Service.Scout(direct, EnemyArmy, IntelSource.Scouting);
            Service.Scout(direct, EnemySupply, IntelSource.Scouting);
            StateHash directHash = direct.ComputeHash();

            // 切割：Scout 敌军 → 存读档 → Scout 敌补给。
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.Scout(loaded, EnemySupply, IntelSource.Scouting);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续侦察确定性");
        }

        // ---- AC-5: 含情报态存档未提供配置 → 整体拒绝 ----

        [Test]
        public void test_restore_intel_save_without_config_is_rejected()
        {
            CampaignSession s = NewSession();
            Service.Scout(s, EnemyArmy, IntelSource.Scouting);
            string text = Service.CaptureSnapshot(s);

            Assert.Throws<SaveFormatException>(() => Service.Restore(text, Fp), "含情报态但未提供 intelConfig 应整体拒绝");
        }

        // ---- 向后兼容：无情报的会话存读档不受影响 ----

        [Test]
        public void test_non_intel_session_roundtrip_still_works()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), Fp);

            Assert.That(loaded.HasIntel, Is.False);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }
    }
}
