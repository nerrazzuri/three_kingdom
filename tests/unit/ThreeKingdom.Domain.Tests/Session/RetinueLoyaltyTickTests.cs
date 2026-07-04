using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 忠诚经营接入日界推进（GDD_014 A2）：推进跨日 → 僚属忠诚衰减；久疏至阈下可被对手挖走。
    /// 挖角机制本身见 RetinueLoyaltyTests（此处验证已接进运行期 Advance）。
    /// </summary>
    [TestFixture]
    public class RetinueLoyaltyTickTests
    {
        private static int MemberCount(CampaignRuntime r) => r.Session.Career.Retinue.Members.Count;

        private static FixedPoint FirstAffinity(CampaignRuntime r)
        {
            foreach (RetinueMember m in r.Session.Career.Retinue.Members) return m.Affinity;
            return FixedPoint.FromInt(-1);
        }

        [Test]
        public void test_loyalty_decays_as_days_advance()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            Assert.That(MemberCount(runtime), Is.GreaterThan(0), "默认场景有僚属（汜水关太守的随从）。");
            FixedPoint before = FirstAffinity(runtime);

            for (int i = 0; i < 12; i++) runtime.Advance(1);   // 推进数日

            if (MemberCount(runtime) > 0)
                Assert.That(FirstAffinity(runtime).Raw, Is.LessThan(before.Raw), "久疏未赏 → 僚属忠诚衰减（已接进日界推进）。");
            else
                Assert.Pass("僚属久疏忠诚跌破阈值后被对手挖走——被挖角亦已接进运行期。");
        }

        [Test]
        public void test_long_neglect_drives_member_below_threshold_or_away()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            for (int i = 0; i < 60; i++) runtime.Advance(1);   // 长期不经营

            // 久疏：要么忠诚已跌到阈下（可被挖角），要么已被挖走。
            bool below = MemberCount(runtime) == 0
                || FirstAffinity(runtime).Raw <= RetinueLoyaltyConfig.Default.PoachThreshold.Raw;
            Assert.That(below, Is.True, "长期不经营 → 忠诚跌破可挖角阈或已叛离（忠诚成活循环）。");
        }
    }
}
