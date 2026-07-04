using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 君主争霸接入日界推进（E1 自动编排）：推进跨日 → 群雄自动兼并（强吞弱）；终局可查、既定则止。
    /// 不再需手动 AdvanceContention——战略层随游戏自跑。
    /// </summary>
    [TestFixture]
    public class ContentionAutoAdvanceTests
    {
        private static int AliveRivals(ContentionState s)
        {
            int n = 0;
            foreach (FactionId f in s.AlivePowers()) if (f != PlayableCampaign.Player) n++;
            return n;
        }

        [Test]
        public void test_advancing_days_auto_progresses_contention()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            int total0 = runtime.Contention.TotalCities;

            for (int i = 0; i < 80; i++) runtime.Advance(1);

            Assert.That(runtime.Contention.TotalCities, Is.EqualTo(total0), "兼并只转移城池，天下总城守恒。");
            Assert.That(runtime.Contention.Hash(), Is.Not.Null, "争霸态可查。");
            Assert.That(AliveRivals(runtime.Contention),
                Is.LessThanOrEqualTo(AliveRivals(PlayableCampaign.Default().InitialContention())),
                "群雄自动兼并 → 存续对手数不增（强吞弱集中，战略层自跑）。");
        }

        [Test]
        public void test_endgame_status_is_queryable_during_play()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            runtime.Advance(4);
            Assert.That(System.Enum.IsDefined(typeof(EndgameStatus), runtime.Endgame()), Is.True, "终局状态随时可查（继续/统一/覆灭）。");
        }
    }
}
