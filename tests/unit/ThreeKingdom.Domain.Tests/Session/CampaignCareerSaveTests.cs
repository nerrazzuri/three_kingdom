using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using M = ThreeKingdom.Domain.Tests.Session.CampaignMeritAccrualTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-022 story-004：生涯权限态存读档 + 确定性（Integration / Assembly）。
    /// 治理 ADR：ADR-0005 + ADR-0004。TR-career-003/001。
    /// 覆盖：功绩/晋升/自立后 career round-trip 一致；哈希一致；确定性链。
    /// </summary>
    [TestFixture]
    public class CampaignCareerSaveTests
    {
        private static CampaignSessionService Service => M.Service;
        private static readonly FactionId NewFaction = new FactionId("faction-rebel");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);
        private static CampaignSession Restore(string text) => Service.Restore(text, M.Fp);

        private static RebellionConfig RebelCfg()
            => new RebellionConfig(3, 400, Frac(5, 10), Frac(5, 10), Frac(7, 10), Frac(4, 10));
        private static RebellionContext MilitaryReady()
            => new RebellionContext(3, true, true, false, NewFaction);

        // ---- AC-1: 功绩态 round-trip ----

        [Test]
        public void test_merit_state_roundtrip()
        {
            CampaignSession s = M.NewSession();
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.CombatVictory);

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.Career.Career.Merit, Is.EqualTo(s.Career.Career.Merit));
            Assert.That(loaded.Career.Career.Renown, Is.EqualTo(s.Career.Career.Renown));
        }

        // ---- AC-2: 晋升后官阶 round-trip ----

        [Test]
        public void test_rank_after_promotion_roundtrip()
        {
            CampaignSession s = M.NewSession();
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);
            Service.RequestPromotion(s, M.Ladder());

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.Career.Career.Rank, Is.EqualTo(s.Career.Career.Rank), "晋升后官阶 round-trip");
        }

        // ---- AC-3: 自立后归属 round-trip ----

        [Test]
        public void test_faction_after_rebellion_roundtrip()
        {
            CampaignSession s = M.NewSession();
            Service.LaunchRebellion(s, RebelCfg(), MilitaryReady());

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.Career.Career.Faction, Is.EqualTo(s.Career.Career.Faction), "自立后归属 round-trip");
            Assert.That(loaded.Career.Career.IsUnaffiliated, Is.EqualTo(s.Career.Career.IsUnaffiliated));
        }

        // ---- AC-4: round-trip 哈希一致 ----

        [Test]
        public void test_career_roundtrip_preserves_hash()
        {
            CampaignSession s = M.NewSession();
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);
            Service.RequestPromotion(s, M.Ladder());
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-5: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：功绩 → 晋升。
            CampaignSession direct = M.NewSession();
            Service.ApplyCareerGain(direct, M.Ladder(), CareerGainSource.MajorBattleVictory);
            Service.RequestPromotion(direct, M.Ladder());
            StateHash directHash = direct.ComputeHash();

            // 切割：功绩 → 存读档 → 晋升。
            CampaignSession s = M.NewSession();
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.RequestPromotion(loaded, M.Ladder());

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后续晋升确定性");
        }
    }
}
