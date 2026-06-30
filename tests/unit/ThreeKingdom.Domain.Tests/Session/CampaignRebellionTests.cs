using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using M = ThreeKingdom.Domain.Tests.Session.CampaignMeritAccrualTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-022 story-003：自立资格三分支 + 反叛发起（Integration / Assembly）。
    /// 治理 ADR：ADR-0009 + ADR-0004。TR-career-002/001/005。
    /// 覆盖：军事分支资格（只读）；发起转新势力；资格不足稳定错误码无写入；确定性；失败可继续。
    /// </summary>
    [TestFixture]
    public class CampaignRebellionTests
    {
        private static CampaignSessionService Service => M.Service;
        private static readonly FactionId NewFaction = new FactionId("faction-rebel");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static RebellionConfig Config()
            => new RebellionConfig(rebelCityMin: 3, rebelRenownMin: 400, rebelAffinityMin: Frac(5, 10),
                defectThreshold: Frac(5, 10), loyalRatioHi: Frac(7, 10), loyalRatioMid: Frac(4, 10));

        // 军事分支达标：城数≥3 + 补给 + 兵力。
        private static RebellionContext MilitaryReady()
            => new RebellionContext(citiesOwned: 3, supplyReady: true, troopsReady: true, lordOppression: false, newFactionId: NewFaction);

        // 全不达标：无城/无补给/无压迫 + 开局 renown=0 < 400。
        private static RebellionContext NotEligible()
            => new RebellionContext(citiesOwned: 0, supplyReady: false, troopsReady: false, lordOppression: false, newFactionId: NewFaction);

        // ---- AC-1: 军事分支资格（只读）----

        [Test]
        public void test_military_branch_eligibility_readonly()
        {
            CampaignSession s = M.NewSession();
            StateHash before = s.ComputeHash();

            RebellionEligibility e = Service.CheckRebellionEligibility(s, Config(), MilitaryReady());

            Assert.That(e.MilitaryGroupMet, Is.True, "军事分支达标");
            Assert.That(e.CanRebel, Is.True);
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "资格判定只读不写");
        }

        // ---- AC-2: 资格达成发起自立 → 转新势力 ----

        [Test]
        public void test_launch_rebellion_transforms_to_new_faction()
        {
            CampaignSession s = M.NewSession();

            RebellionResult r = Service.LaunchRebellion(s, Config(), MilitaryReady());

            Assert.That(r.Launched, Is.True);
            Assert.That(s.Career.Career.Faction, Is.EqualTo(NewFaction), "自立转新势力");
        }

        // ---- AC-3: 资格不足稳定错误码无写入 ----

        [Test]
        public void test_ineligible_rebellion_rejected_no_write()
        {
            CampaignSession s = M.NewSession();
            StateHash before = s.ComputeHash();

            RebellionResult r = Service.LaunchRebellion(s, Config(), NotEligible());

            Assert.That(r.Launched, Is.False);
            Assert.That(r.Error, Is.Not.EqualTo(CareerErrorCode.None));
            Assert.That(s.ComputeHash(), Is.EqualTo(before), "资格不足零写入");
        }

        // ---- AC-4: 自立确定性 ----

        [Test]
        public void test_rebellion_is_deterministic()
        {
            CampaignSession a = M.NewSession();
            CampaignSession b = M.NewSession();
            Service.LaunchRebellion(a, Config(), MilitaryReady());
            Service.LaunchRebellion(b, Config(), MilitaryReady());

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同生涯+配置+上下文 → 同哈希");
        }

        // ---- AC-5: 失败后可继续（非死局）----

        [Test]
        public void test_continues_after_failed_rebellion()
        {
            CampaignSession s = M.NewSession();
            Service.LaunchRebellion(s, Config(), NotEligible());   // 失败
            CareerCommandResult gain = Service.ApplyCareerGain(s, M.Ladder(), CareerGainSource.CombatVictory);

            Assert.That(gain.Applied, Is.True, "自立失败后会话仍可继续（非死局）");
        }
    }
}
