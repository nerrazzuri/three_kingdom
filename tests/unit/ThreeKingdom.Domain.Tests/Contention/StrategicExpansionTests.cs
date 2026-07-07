using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Contention;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Contention
{
    /// <summary>E4.2 战略化推进（ADR-0013）：意图驱动兼并（只侵略者出手）+ 对玩家施压（强势报复/扩张者夺玩家城）。</summary>
    [TestFixture]
    public class StrategicExpansionTests
    {
        private static FactionId F(string id) => new FactionId(id);
        private static ContentionState State(params (string, int)[] powers)
        {
            var list = new List<PowerStanding>();
            foreach (var (f, c) in powers) list.Add(new PowerStanding(F(f), c));
            return new ContentionState(list);
        }
        private readonly RivalExpansionService _svc = new RivalExpansionService();
        private static readonly ContentionConfig Cfg = new ContentionConfig(ThreeKingdom.Domain.Numerics.FixedPoint.One);

        [Test]
        public void test_no_aggressor_no_annexation()
        {
            // 玩家最强，其余弱小求存（无扩张/趁火/报复意图）→ 无人出手。
            var s = State(("player", 8), ("a", 5), ("b", 5));
            for (ulong seed = 0; seed < 30; seed++)
            {
                var next = _svc.StepStrategic(s, F("player"), null, new HashSet<string>(), seed, Cfg);
                Assert.That(next.TotalCities, Is.EqualTo(s.TotalCities), "无侵略意图 → 天下不变。");
                Assert.That(next.CitiesOf(F("player")), Is.EqualTo(8), "玩家城不减。");
            }
        }

        [Test]
        public void test_aggressor_annexes_weakest_rival_not_player_when_player_not_pressurable()
        {
            // 曹强(扩张·aggressor)、玩家中等(不可施压：非报复且曹<玩家×2)、weak 弱 → 曹夺 weak。
            var s = State(("cao", 6), ("player", 5), ("weak", 2));
            bool weakEverLost = false;
            for (ulong seed = 0; seed < 40; seed++)
            {
                var next = _svc.StepStrategic(s, F("player"), null, new HashSet<string>(), seed, Cfg);
                Assert.That(next.CitiesOf(F("player")), Is.EqualTo(5), "玩家不可施压时不被夺城。");
                if (next.CitiesOf(F("weak")) < 2) weakEverLost = true;
            }
            Assert.That(weakEverLost, Is.True, "曹（扩张）会夺最弱鄰 weak（某些种子）。");
        }

        [Test]
        public void test_vengeful_strong_aggressor_pressures_player()
        {
            // 孙强且遭玩家夺城(报复) → 对玩家施压夺城；玩家较弱。
            var s = State(("sun", 8), ("player", 3), ("other", 4));
            var wronged = new HashSet<string> { "sun" };
            bool playerEverLost = false, otherEverLost = false;
            for (ulong seed = 0; seed < 40; seed++)
            {
                var next = _svc.StepStrategic(s, F("player"), null, wronged, seed, Cfg);
                if (next.CitiesOf(F("player")) < 3) playerEverLost = true;
                if (next.CitiesOf(F("other")) < 4) otherEverLost = true;
            }
            Assert.That(playerEverLost, Is.True, "强势报复者会夺玩家城（世界反击）。");
            Assert.That(otherEverLost, Is.False, "施压玩家时不夺他鄰（目标锁定玩家）。");
        }
    }
}
