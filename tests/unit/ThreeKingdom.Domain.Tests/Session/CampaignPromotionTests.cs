using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Numerics;
using M = ThreeKingdom.Domain.Tests.Session.CampaignMeritAccrualTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-022 story-002：晋升申请命令（Integration / Assembly）。
    /// 治理 ADR：ADR-0009 + ADR-0004。TR-career-001/005。
    /// 覆盖：门槛达成晋级；未达稳定错误码无写入；最高阶；失败后可继续。
    /// </summary>
    [TestFixture]
    public class CampaignPromotionTests
    {
        private static CampaignSessionService Service => M.Service;

        // ---- AC-1: 门槛达成晋级 ----

        [Test]
        public void test_promotion_succeeds_when_threshold_met()
        {
            CampaignSession s = M.NewSession();
            Rank before = s.Career.Career.Rank;
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);   // 一次达阶1门槛

            CareerCommandResult r = Service.RequestPromotion(s, M.Ladder());

            Assert.That(r.Applied, Is.True);
            Assert.That((int)s.Career.Career.Rank, Is.GreaterThan((int)before), "晋一阶");
        }

        // ---- AC-2: 门槛未达稳定错误码无写入 ----

        [Test]
        public void test_promotion_below_threshold_rejected_no_write()
        {
            CampaignSession s = M.NewSession();   // merit=0，未累积
            StateHash before = s.ComputeHash();

            CareerCommandResult r = Service.RequestPromotion(s, M.Ladder());

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CareerErrorCode.PromotionThresholdNotMet));
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "未达门槛零写入");
        }

        // ---- AC-3: 已达最高阶 ----

        [Test]
        public void test_promotion_caps_progression()
        {
            // 阶2+ 门槛不可达（9999），晋到阶1后再申请 → 门槛未达（阶2 不可达）。
            CampaignSession s = M.NewSession();
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);
            Service.RequestPromotion(s, M.Ladder());   // → 阶1
            CareerCommandResult second = Service.RequestPromotion(s, M.Ladder());   // 阶2 门槛 9999 不可达

            Assert.That(second.Applied, Is.False, "阶2 门槛不可达，再申请被拒");
        }

        // ---- AC-4: 失败后可继续（累积→晋升）----

        [Test]
        public void test_continues_after_failed_promotion()
        {
            CampaignSession s = M.NewSession();
            CareerCommandResult fail = Service.RequestPromotion(s, M.Ladder());   // 未达
            Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.MajorBattleVictory);   // 累积达门槛
            CareerCommandResult ok = Service.RequestPromotion(s, M.Ladder());

            Assert.That(fail.Applied, Is.False);
            Assert.That(ok.Applied, Is.True, "累积后晋升成功（失败不卡死）");
        }
    }
}
