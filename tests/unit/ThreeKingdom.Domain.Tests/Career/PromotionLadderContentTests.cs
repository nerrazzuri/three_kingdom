using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// 官职晋升曲线内容（W5 #12）：完整 8 阶（太守→继承基业）皆可达，门槛递增——
    /// 忠臣晋升线成真实长期成长，非只有首阶（修正竖切"阶2+ 封顶 9999"）。
    /// </summary>
    [TestFixture]
    public class PromotionLadderContentTests
    {
        [Test]
        public void test_full_ladder_is_reachable_with_escalating_thresholds()
        {
            PromotionLadderConfig ladder = PlayableCampaign.Default().Ladder;
            int prev = -1;
            for (Rank r = Rank.SeniorGovernor; r <= Rank.Successor; r++)
            {
                int need = ladder.MeritReq[(int)r];
                Assert.That(need, Is.LessThan(9999), $"{r} 阶可达（非封顶 9999）。");
                Assert.That(need, Is.GreaterThan(prev), $"{r} 阶门槛高于前阶（递增曲线）。");
                prev = need;
            }
        }
    }
}
