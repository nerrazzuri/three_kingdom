using NUnit.Framework;
using ThreeKingdom.Domain.Defeat;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Defeat
{
    /// <summary>
    /// 被灭处境流程（GDD_026 补）：被俘→判生死→归顺?→释放?→投奔他主收留?。唯身死才终，余皆可续（复为太守）。
    /// 确定性状态机——玩家选择驱动、AI 判定种子化。
    /// </summary>
    [TestFixture]
    public class DefeatFlowTests
    {
        private static readonly FactionId Captor = new FactionId("faction-cao");
        private static DefeatFlow Flow(int renown, ulong seed) => new DefeatFlow(Captor, renown, seed, CaptivityConfig.Default);

        [Test]
        public void test_executed_ends_the_life()
        {
            DefeatFlow? found = null;
            for (ulong s = 0; s < 1000 && found == null; s++)
            {
                var g = Flow(0, s);   // 无名之辈，较易被杀
                g.ResolveCaptorFate();
                if (g.Stage == DefeatStage.Executed) found = g;
            }
            Assert.That(found, Is.Not.Null, "存在被处死的走向。");
            DefeatFlow f = found!;
            Assert.That(f.IsLifeEnded, Is.True, "被处死 → 身死 → 一世终。");
            Assert.That(f.CanPlayOn, Is.False);
        }

        [Test]
        public void test_spared_then_submit_reseats_under_captor()
        {
            DefeatFlow? found = null;
            for (ulong s = 0; s < 1000 && found == null; s++)
            {
                var g = Flow(1000, s);   // 名将，较易获留
                g.ResolveCaptorFate();
                if (g.Stage == DefeatStage.Captured) found = g;
            }
            Assert.That(found, Is.Not.Null, "存在不杀（被俘待发落）的走向。");
            DefeatFlow f = found!;
            f.Submit();
            Assert.That(f.Stage, Is.EqualTo(DefeatStage.Submitted));
            Assert.That(f.CanPlayOn, Is.True, "归顺 → 复为太守，可续玩。");
            Assert.That(f.NewLord, Is.EqualTo(Captor), "归顺即事擒获者。");
            Assert.That(f.IsLifeEnded, Is.False, "命还在。");
        }

        [Test]
        public void test_refuse_then_released_then_refuge_accepted_reseats_under_new_lord()
        {
            var newLord = new FactionId("faction-sun");
            DefeatFlow? found = null;
            for (ulong s = 0; s < 4000 && found == null; s++)
            {
                var g = Flow(480, s);   // 有些名望、未过猜忌阈
                g.ResolveCaptorFate();
                if (g.Stage != DefeatStage.Captured) continue;   // 须先被留
                if (!g.Refuse()) continue;                        // 须获释
                if (g.SeekRefuge(newLord, 5)) found = g;          // 投奔被收留
            }
            Assert.That(found, Is.Not.Null, "存在 不归顺→获释→投奔被收留 的完整走向。");
            DefeatFlow f = found!;
            Assert.That(f.Stage, Is.EqualTo(DefeatStage.Reseated));
            Assert.That(f.CanPlayOn, Is.True, "被收留 → 复为太守，东山再起。");
            Assert.That(f.NewLord, Is.EqualTo(newLord), "效力收留之新主。");
        }

        [Test]
        public void test_refuge_rejected_by_small_lord_stays_exiled()
        {
            DefeatFlow? found = null;
            for (ulong s = 0; s < 1000 && found == null; s++)
            {
                var g = Flow(300, s);
                g.ResolveCaptorFate();
                if (g.Stage != DefeatStage.Captured) continue;
                if (g.Refuse()) found = g;   // 获释
            }
            Assert.That(found, Is.Not.Null);
            DefeatFlow f = found!;
            bool taken = f.SeekRefuge(new FactionId("faction-tiny"), 1);   // 一城之主容不下
            Assert.That(taken, Is.False, "小势力不收 → 投奔失败。");
            Assert.That(f.Stage, Is.EqualTo(DefeatStage.Released), "仍为流亡之身，可再投他家。");
            Assert.That(f.IsLifeEnded, Is.False, "流亡非终局。");
        }

        [Test]
        public void test_flow_is_deterministic()
        {
            var a = Flow(500, 42UL); a.ResolveCaptorFate();
            var b = Flow(500, 42UL); b.ResolveCaptorFate();
            Assert.That(a.Stage, Is.EqualTo(b.Stage), "同参数 → 同走向（可复现）。");
        }
    }
}
