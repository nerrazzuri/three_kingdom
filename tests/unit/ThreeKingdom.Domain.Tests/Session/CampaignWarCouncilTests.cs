using System;
using System.Collections.Generic;
using System.Linq;
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
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-017 story-003：军议经会话知识快照（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（装配，军议读快照不触真值）+ ADR-0004（确定性）。TR-council-001/002。
    /// 覆盖：军议经会话快照输出；同快照确定；侦察后旧建议过时；军师只条件化建议（无成功率/唯一推荐/自动命令）。
    /// </summary>
    [TestFixture]
    public class CampaignWarCouncilTests
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

        // 军议装配：一名军师 + 一条引用敌军的论证模板。
        private static SessionCouncilSetup CouncilSetup()
        {
            var advisor = new AdvisorPerspective(new AdvisorId("advisor-zhuge"), Frac(8, 10));
            var template = new AdviceTemplate(
                candidateId: "advice-ambush",
                observation: "隘口适于设伏",
                assumption: "若敌将急躁且经此路",
                requiredConditions: new[] { "敌将性烈", "敌军经隘口" },
                risks: new[] { "暴露则反受夹击" },
                referencedSubjects: new[] { EnemyArmy });
            return new SessionCouncilSetup(advisor, new[] { template }, new CouncilConfig(Frac(1, 1)), Frac(7, 10));
        }

        private static WorldTruthLedger Truth(int enemyStrength = 5000)
        {
            var t = new WorldTruthLedger();
            t.Set(new TruthRecord(EnemyArmy, enemyStrength, Enemy));
            return t;
        }

        private static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-council", Fp,
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

        // ---- AC-1: 军议经会话快照输出建议集 ----

        [Test]
        public void test_convene_returns_advice_bound_to_current_snapshot()
        {
            CampaignSession s = NewSession();

            CouncilAdviceSet advice = Service.ConveneCouncil(s);

            Assert.That(advice.Advice.Count, Is.GreaterThan(0), "输出条件化建议");
            Assert.That(advice.SnapshotId, Is.EqualTo(s.CurrentKnowledgeSnapshotId!.Value), "绑定当前知识快照");
        }

        // ---- AC-2: 同快照两次召开输出相同 ----

        [Test]
        public void test_same_snapshot_convene_is_deterministic()
        {
            CampaignSession s = NewSession();

            CouncilAdviceSet first = Service.ConveneCouncil(s);
            CouncilAdviceSet second = Service.ConveneCouncil(s);   // 中间无侦察

            Assert.That(second.SnapshotId, Is.EqualTo(first.SnapshotId), "快照相同");
            Assert.That(second.Advice.Count, Is.EqualTo(first.Advice.Count));
            for (int i = 0; i < first.Advice.Count; i++)
            {
                Assert.That(second.Advice[i].CandidateId, Is.EqualTo(first.Advice[i].CandidateId));
                Assert.That(second.Advice[i].Confidence, Is.EqualTo(first.Advice[i].Confidence), "同快照置信确定");
            }
        }

        // ---- AC-3: 侦察后旧建议过时（TR-council-001）----

        [Test]
        public void test_advice_becomes_stale_after_scouting_changes_knowledge()
        {
            CampaignSession s = NewSession();
            CouncilAdviceSet advice = Service.ConveneCouncil(s);
            Assert.That(advice.IsStaleAgainst(s.CurrentKnowledgeSnapshotId!.Value), Is.False, "召开当下不过时");

            Service.Scout(s, EnemyArmy, IntelSource.Scouting);   // 知识变化

            Assert.That(advice.IsStaleAgainst(s.CurrentKnowledgeSnapshotId!.Value), Is.True,
                "侦察改变知识后旧建议过时（不静默更新）");
        }

        // ---- AC-4: 军师只输出条件化建议（TR-council-002）----

        [Test]
        public void test_advice_is_conditional_only_no_success_rate_or_command()
        {
            CampaignSession s = NewSession();
            CouncilAdviceSet advice = Service.ConveneCouncil(s);

            Assert.That(advice.Advice.All(a => a.RequiredConditions.Count > 0), Is.True, "每条含所需条件");
            Assert.That(advice.Advice.All(a => !string.IsNullOrWhiteSpace(a.Observation)), Is.True, "每条含观察/缘由");
            Assert.That(advice.Advice.All(a => a.Risks.Count > 0), Is.True, "每条含风险");
            // 结构上 AdviceStatement 无任何"成功率/胜率"或"唯一推荐/自动命令"字段——TR-council-002 由类型保证。
            // 此处验证装配后仍只暴露条件化字段（观察/假设/条件/风险/缺失情报/置信）。
            Assert.That(advice.Advice.All(a => a.Confidence >= FixedPoint.Zero && a.Confidence <= FixedPoint.One), Is.True,
                "置信为定性可靠性 [0,1]，非真实概率/胜率");
        }

        [Test]
        public void test_convene_on_council_disabled_session_throws()
        {
            var bare = new CampaignStartConfig(
                "scenario-bare", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Player, 800) });
            CampaignSession s = Service.StartCampaign(bare).Session!;

            Assert.Throws<InvalidOperationException>(() => Service.ConveneCouncil(s));
        }
    }
}
