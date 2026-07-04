using System;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Talent;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Talent;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Talent
{
    /// <summary>
    /// 人才招揽（GDD_020 / epic-030）：登场时间窗 · 知晓反全知 · 招揽条件+种子判定（单调/边界/确定性/无胜率）·
    /// 喂给为将 · 存读档哈希。守两魂：反全知 + 条件涌现。
    /// </summary>
    [TestFixture]
    public class TalentTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly FactionId Player = new FactionId("faction-player");

        private static TalentProfile Talent(FixedPoint will, FixedPoint reluct, int appearDay = 0, string id = "t-x")
            => new TalentProfile(new TalentId(id), new CharacterId("char-" + id), F(7, 10), F(6, 10), F(8, 10),
                GeneralSpecialty.None, will, reluct, new WorldTime(appearDay, DaySegment.Dawn));

        // ---- S1 登场时间窗 ----

        [Test]
        public void test_talent_appears_only_within_time_window()
        {
            TalentRoster roster = PlayableCampaign.Default().TalentRoster;
            // 卧龙第 3 日才出世；骁将开局即在。
            Assert.That(Contains(roster.Appeared(new WorldTime(0, DaySegment.Dawn)), PlayableCampaign.Wolong), Is.False, "卧龙未到登场之时。");
            Assert.That(Contains(roster.Appeared(new WorldTime(0, DaySegment.Dawn)), PlayableCampaign.Xiaojiang), Is.True, "骁将开局即在。");
            Assert.That(Contains(roster.Appeared(new WorldTime(3, DaySegment.Dawn)), PlayableCampaign.Wolong), Is.True, "第 3 日卧龙出世。");
        }

        // ---- S2 知晓反全知 ----

        [Test]
        public void test_unknown_talent_not_visible_until_revealed()
        {
            var svc = new TalentService();
            TalentRoster roster = PlayableCampaign.Default().TalentRoster;
            var day0 = new WorldTime(0, DaySegment.Dawn);

            // 骁将已登场但未知晓 → 不可见（反全知）。
            Assert.That(Contains(svc.Visible(roster, TalentState.Empty, day0), PlayableCampaign.Xiaojiang), Is.False, "未知晓 → 不入视野。");

            TalentState known = svc.Reveal(TalentState.Empty, PlayableCampaign.Xiaojiang, TalentChannel.Scouting);
            Assert.That(Contains(svc.Visible(roster, known, day0), PlayableCampaign.Xiaojiang), Is.True, "侦察知晓后可见。");

            // 卧龙已知晓但未登场 → 仍不可见（登场门）。
            TalentState knownWolong = svc.Reveal(known, PlayableCampaign.Wolong, TalentChannel.HistoricalEvent);
            Assert.That(Contains(svc.Visible(roster, knownWolong, day0), PlayableCampaign.Wolong), Is.False, "未登场 → 不可见。");
        }

        // ---- S3 招揽条件 + 种子判定 ----

        [Test]
        public void test_recruit_probability_monotonic_in_offer()
        {
            var svc = new TalentRecruitmentService();
            TalentProfile p = Talent(F(5, 10), F(2, 10));
            FixedPoint weak = svc.Resolve(p, RecruitmentOffer.None, 1UL, TalentRecruitmentConfig.Default).Probability;
            FixedPoint strong = svc.Resolve(p, new RecruitmentOffer(FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One), 1UL, TalentRecruitmentConfig.Default).Probability;
            Assert.That(strong, Is.GreaterThanOrEqualTo(weak), "名望/官职/关系/待遇↑ → 招揽概率不降（单调）。");
        }

        [Test]
        public void test_recruit_boundary_certain_join_and_certain_decline()
        {
            var svc = new TalentRecruitmentService();
            // p=1：base 1、阻力 0 → 恒出仕（任何种子）。
            var willingCfg = new TalentRecruitmentConfig(FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
            TalentProfile eager = Talent(F(5, 10), FixedPoint.Zero);
            for (ulong s = 1; s <= 4; s++)
                Assert.That(svc.Resolve(eager, RecruitmentOffer.None, s, willingCfg).Verdict, Is.EqualTo(RecruitmentVerdict.Joined), "p=1 恒出仕。");

            // p=0：全 0 权重 + 阻力 1 → 恒婉拒。
            var coldCfg = new TalentRecruitmentConfig(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
            TalentProfile hermit = Talent(F(5, 10), FixedPoint.One);
            for (ulong s = 1; s <= 4; s++)
                Assert.That(svc.Resolve(hermit, new RecruitmentOffer(FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One), s, coldCfg).Verdict, Is.EqualTo(RecruitmentVerdict.Declined), "p=0 恒婉拒（隐士之志）。");
        }

        [Test]
        public void test_recruit_is_deterministic_for_same_seed()
        {
            var svc = new TalentRecruitmentService();
            TalentProfile p = Talent(F(5, 10), F(4, 10));
            RecruitmentVerdict a = svc.Resolve(p, RecruitmentOffer.None, 0xABCDUL, TalentRecruitmentConfig.Default).Verdict;
            RecruitmentVerdict b = svc.Resolve(p, RecruitmentOffer.None, 0xABCDUL, TalentRecruitmentConfig.Default).Verdict;
            Assert.That(b, Is.EqualTo(a), "同 (人才,条件,种子) → 同结果（可复现·人各有志）。");
        }

        // ---- S4 招揽流程 + 喂给为将 ----

        [Test]
        public void test_attempt_rejects_unknown_and_not_appeared()
        {
            var svc = new TalentService();
            PlayableCampaign sc = PlayableCampaign.Default();
            var day0 = new WorldTime(0, DaySegment.Dawn);

            // 未知晓 → 无效（反全知）。
            TalentRecruitAttempt unknown = svc.AttemptRecruit(sc.TalentRoster, TalentState.Empty, PlayableCampaign.Xiaojiang, day0, RecruitmentOffer.None, sc.TalentSeed, Player, sc.TalentRecruit);
            Assert.That(unknown.Valid, Is.False, "未知晓不可招（反全知）。");

            // 已知晓但未登场（卧龙 day0）→ 无效。
            TalentState knowWolong = svc.Reveal(TalentState.Empty, PlayableCampaign.Wolong, TalentChannel.HistoricalEvent);
            TalentRecruitAttempt early = svc.AttemptRecruit(sc.TalentRoster, knowWolong, PlayableCampaign.Wolong, day0, RecruitmentOffer.None, sc.TalentSeed, Player, sc.TalentRecruit);
            Assert.That(early.Valid, Is.False, "未登场不可招。");
        }

        [Test]
        public void test_successful_recruit_updates_state_and_yields_general()
        {
            var svc = new TalentService();
            PlayableCampaign sc = PlayableCampaign.Default();
            var day0 = new WorldTime(0, DaySegment.Dawn);
            TalentState known = svc.Reveal(TalentState.Empty, PlayableCampaign.Xiaojiang, TalentChannel.Scouting);

            // 厚待骁将（志向高·易招）→ 出仕（强条件使 p→1，确定性出仕）。
            var strong = new RecruitmentOffer(FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One);
            TalentRecruitAttempt r = svc.AttemptRecruit(sc.TalentRoster, known, PlayableCampaign.Xiaojiang, day0, strong, sc.TalentSeed, Player, sc.TalentRecruit);

            Assert.That(r.Valid, Is.True);
            Assert.That(r.Verdict, Is.EqualTo(RecruitmentVerdict.Joined), "厚待易招之将 → 出仕。");
            Assert.That(r.State.IsRecruited(PlayableCampaign.Xiaojiang), Is.True, "入伙记入态。");
            Assert.That(r.State.Attempts(PlayableCampaign.Xiaojiang), Is.EqualTo(1), "尝试计数 +1。");
            Assert.That(r.Joined, Is.Not.Null, "入伙人才为将（喂战斗）。");
            Assert.That(r.Joined!.Command, Is.EqualTo(F(8, 10)), "为将属性来自人才档案。");
        }

        // ---- 存读档 ----

        [Test]
        public void test_talent_state_hash_roundtrip_and_sensitive()
        {
            TalentState s = TalentState.Empty
                .Reveal(new TalentId("t-a"), TalentChannel.Scouting)
                .RecordAttempt(new TalentId("t-a"))
                .Recruit(new TalentId("t-a"));
            // 同态重构 → 同哈希。
            TalentState rebuilt = new TalentState(s.Known, MakeAttempts(), s.Recruited);
            Assert.That(rebuilt.Hash(), Is.EqualTo(s.Hash()), "同态 → 同哈希（存读档一致）。");
            // 变态 → 变哈希。
            Assert.That(s.Reveal(new TalentId("t-b"), TalentChannel.Council).Hash(), Is.Not.EqualTo(s.Hash()), "知晓新人才 → 哈希变。");
        }

        private static System.Collections.Generic.IReadOnlyDictionary<string, int> MakeAttempts()
            => new System.Collections.Generic.Dictionary<string, int> { ["t-a"] = 1 };

        private static bool Contains(System.Collections.Generic.IReadOnlyList<TalentProfile> list, TalentId id)
        {
            foreach (TalentProfile p in list) if (p.Id == id) return true;
            return false;
        }
    }
}
