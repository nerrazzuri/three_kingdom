using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>武将运行时人生（GDD_027 #1 / GDD_005）：忠诚/记忆/健康/被俘演化 + 叛离风险派生。纯函数确定性。</summary>
    [TestFixture]
    public class GeneralLifeTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static FactionId F(string id) => new FactionId(id);
        private static CityId City(string id) => new CityId(id);
        private static GeneralState Fresh(int loyalty = 60)
            => GeneralState.Fresh(C("char-x"), F("faction-a"), City("city-1"), loyalty);

        [Test]
        public void test_loyalty_clamped_0_100()
        {
            Assert.That(Fresh(60).WithLoyalty(200).Loyalty, Is.EqualTo(100));
            Assert.That(Fresh(60).WithLoyalty(-50).Loyalty, Is.EqualTo(0));
        }

        [Test]
        public void test_betrayal_lowers_loyalty_and_raises_defection_risk()
        {
            // Arrange
            GeneralState s = Fresh(70);
            DefectionRisk before = GeneralLifeService.RiskOf(s);
            // Act：被主君背叛（重）。
            GeneralState after = GeneralLifeService.Remember(s, MemoryKind.Betrayed, C("char-lord"), 200, weight: 2);
            // Assert
            Assert.That(after.Loyalty, Is.LessThan(s.Loyalty), "被背叛→忠诚骤降。");
            Assert.That((int)GeneralLifeService.RiskOf(after), Is.GreaterThan((int)before), "叛离风险升高。");
            Assert.That(after.MemoryWeight(MemoryKind.Betrayed), Is.EqualTo(2), "记忆留痕。");
        }

        [Test]
        public void test_rescue_raises_loyalty()
        {
            GeneralState s = Fresh(50);
            GeneralState after = GeneralLifeService.Remember(s, MemoryKind.Rescued, C("char-lord"), 200);
            Assert.That(after.Loyalty, Is.GreaterThan(s.Loyalty), "被救命→忠诚大升。");
        }

        [Test]
        public void test_wound_and_serve_gate()
        {
            GeneralState s = Fresh();
            Assert.That(GeneralLifeService.CanServe(s), Is.True, "康健可出征。");
            GeneralState wounded = GeneralLifeService.Wound(s);
            Assert.That(wounded.Health, Is.EqualTo(GeneralHealth.Wounded));
            GeneralState grave = GeneralLifeService.Wound(wounded);
            Assert.That(grave.Health, Is.EqualTo(GeneralHealth.Grave));
            Assert.That(GeneralLifeService.CanServe(grave), Is.False, "重创不可受命。");
            GeneralState rested = GeneralLifeService.Rest(grave);
            Assert.That(rested.Health, Is.EqualTo(GeneralHealth.Wounded), "将养回复一档。");
        }

        [Test]
        public void test_capture_and_release()
        {
            GeneralState s = Fresh();
            GeneralState captured = GeneralLifeService.Capture(s, F("faction-enemy"));
            Assert.That(captured.CaptiveOf.HasValue, Is.True);
            Assert.That(GeneralLifeService.CanServe(captured), Is.False, "在押不可受命。");
            GeneralState freed = GeneralLifeService.Release(captured);
            Assert.That(freed.CaptiveOf.HasValue, Is.False, "获释清除在押。");
        }

        [Test]
        public void test_reassign_resets_loyalty_keeps_memory()
        {
            GeneralState s = GeneralLifeService.Remember(Fresh(80), MemoryKind.Rewarded, C("char-old"), 200);
            GeneralState moved = GeneralLifeService.Reassign(s, F("faction-b"), City("city-2"), startLoyalty: 50);
            Assert.That(moved.Faction!.Value.Value, Is.EqualTo("faction-b"), "改换门庭。");
            Assert.That(moved.Loyalty, Is.EqualTo(50), "新主忠诚中性起点。");
            Assert.That(moved.Memories.Count, Is.EqualTo(1), "旧记忆保留（人不会忘）。");
        }

        [Test]
        public void test_ledger_seed_idempotent_and_writeback()
        {
            var ledger = new GeneralLedger();
            GeneralState a = ledger.GetOrSeed(C("char-x"), id => GeneralState.Fresh(id, F("faction-a"), City("city-1"), 60));
            GeneralState b = ledger.GetOrSeed(C("char-x"), id => GeneralState.Fresh(id, F("faction-z"), City("city-9"), 10));
            Assert.That(b.Faction!.Value.Value, Is.EqualTo("faction-a"), "已登记者不被二次铸入覆盖。");
            ledger.Set(GeneralLifeService.AdjustLoyalty(a, -30));
            Assert.That(ledger.Get(C("char-x"))!.Loyalty, Is.EqualTo(30), "演化写回台账。");
        }
    }
}
