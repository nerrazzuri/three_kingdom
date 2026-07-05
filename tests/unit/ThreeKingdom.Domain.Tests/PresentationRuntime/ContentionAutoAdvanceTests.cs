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
        public void test_contention_state_round_trips_through_save()
        {
            var medium = new InMemorySaveMedium();
            var a = new CampaignRuntime(medium);
            a.NewGame();
            for (int i = 0; i < 100; i++) a.Advance(1);   // 争霸自动推演改变领土格局
            ContentionState before = a.Contention;
            Assert.That(a.Save(), Is.True);

            var b = new CampaignRuntime(medium);
            Assert.That(b.Load(out _), Is.True);
            Assert.That(b.Contention.Hash(), Is.EqualTo(before.Hash()), "争霸态存读档一致（不再重置，已纳入统一存档）。");
            Assert.That(b.Contention.TotalCities, Is.EqualTo(before.TotalCities), "天下总城一致。");
        }

        [Test]
        public void test_contention_stays_multipolar_for_decades()
        {
            // 放慢（GDD_026 2026-07-05：每年至多一次缓和兼并）：天下几十年才渐渐集中，非速统一 → 一生大多时候仍多国鼎立。
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            int rivals0 = AliveRivals(runtime.Contention);

            for (int i = 0; i < 20; i++) runtime.AdvanceYear();   // 过 20 年

            Assert.That(runtime.Endgame(), Is.EqualTo(EndgameStatus.Ongoing), "20 年后天下未定（放慢，走向 AI 自掌）。");
            int rivals = AliveRivals(runtime.Contention);
            Assert.That(rivals, Is.GreaterThanOrEqualTo(5), "20 年后仍多国鼎立（≥5 家对手），非速统一。");
            Assert.That(rivals, Is.LessThanOrEqualTo(rivals0), "确有渐进兼并（对手数不增）。");
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
