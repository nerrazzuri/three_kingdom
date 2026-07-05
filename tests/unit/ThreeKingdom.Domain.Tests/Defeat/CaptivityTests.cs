using NUnit.Framework;
using ThreeKingdom.Domain.Defeat;

namespace ThreeKingdom.Domain.Tests.Defeat
{
    /// <summary>
    /// 被俘/流亡的 AI 判定（GDD_026 补：势力被灭→被俘→判生死→归顺?→释放?→投奔他主收留?）：
    /// 全部种子化确定性 + 名声调制。名将不轻杀（有用），然名高则怕放虎、亦招功高之忌。
    /// </summary>
    [TestFixture]
    public class CaptivityTests
    {
        private static readonly CaptivityService Svc = new CaptivityService();
        private static readonly CaptivityConfig Cfg = CaptivityConfig.Default;   // RenownReference 800

        private static int Count(System.Func<ulong, bool> f, int n = 300)
        {
            int c = 0;
            for (ulong s = 0; s < (ulong)n; s++) if (f(s)) c++;
            return c;
        }

        [Test]
        public void test_verdicts_are_deterministic()
        {
            Assert.That(Svc.CaptorSpares(500, 42UL, Cfg), Is.EqualTo(Svc.CaptorSpares(500, 42UL, Cfg)));
            Assert.That(Svc.CaptorReleases(500, 42UL, Cfg), Is.EqualTo(Svc.CaptorReleases(500, 42UL, Cfg)));
            Assert.That(Svc.LordAcceptsRefuge(500, 4, 42UL, Cfg), Is.EqualTo(Svc.LordAcceptsRefuge(500, 4, 42UL, Cfg)));
        }

        [Test]
        public void test_famous_captive_more_likely_spared()
        {
            // 有用之才不轻杀：名声越高越可能被留（不杀）。
            int lowRenown = Count(s => Svc.CaptorSpares(0, s, Cfg));
            int highRenown = Count(s => Svc.CaptorSpares(1000, s, Cfg));
            Assert.That(highRenown, Is.GreaterThan(lowRenown), "名将被俘更可能获留用（少杀）。");
        }

        [Test]
        public void test_famous_captive_less_likely_released()
        {
            // 怕放虎归山：不归顺时，名声越高越不肯放。
            int lowRenown = Count(s => Svc.CaptorReleases(0, s, Cfg));
            int highRenown = Count(s => Svc.CaptorReleases(1000, s, Cfg));
            Assert.That(highRenown, Is.LessThan(lowRenown), "名将不归顺则更难获释（怕放虎归山）。");
        }

        [Test]
        public void test_small_lord_cannot_take_in_a_governor()
        {
            // 自顾不暇：太小的势力（< 最少领城）无以安置太守，一律不收。
            for (ulong s = 0; s < 20; s++)
                Assert.That(Svc.LordAcceptsRefuge(500, 1, s, Cfg), Is.False, "一城之主容不下流亡太守。");
        }

        [Test]
        public void test_reputation_helps_refuge_at_low_end_but_suspicion_bites_at_top()
        {
            // 名声助投奔（无名之辈无人要）；然功高震主之虑 → 名声过顶反招猜忌，收留不升反抑。
            int nobody = Count(s => Svc.LordAcceptsRefuge(0, 5, s, Cfg));
            int renowned = Count(s => Svc.LordAcceptsRefuge(480, 5, s, Cfg));       // norm 0.6，未过猜忌阈
            int overshadow = Count(s => Svc.LordAcceptsRefuge(1600, 5, s, Cfg));    // norm 1，过阈招忌
            Assert.That(renowned, Is.GreaterThan(nobody), "有些名望更易被收留（无名之辈无人问津）。");
            Assert.That(overshadow, Is.LessThan(renowned), "名声过顶招猜忌 → 收留反被抑（功高震主之虑）。");
        }
    }
}
