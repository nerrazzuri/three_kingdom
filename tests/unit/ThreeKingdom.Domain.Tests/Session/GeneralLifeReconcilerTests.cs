using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>演义覆盖层↔人生态桥（GDD_027 #1 / ADR-0017）：斩杀标重创、移籍换主，幂等（可安全重放不重置忠诚）。</summary>
    [TestFixture]
    public class GeneralLifeReconcilerTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static FactionId F(string id) => new FactionId(id);

        [Test]
        public void test_slay_marks_grave_and_is_idempotent()
        {
            // Arrange
            var ledger = new GeneralLedger();
            ledger.Set(GeneralState.Fresh(C("char-x"), F("faction-a"), new CityId("city-1"), 60));
            var ov = new LoreOverrides();
            ov.Apply(LoreEffect.Slay(C("char-x")));
            // Act
            GeneralLifeReconciler.ApplyLore(ledger, ov, 190);
            // Assert
            Assert.That(ledger.Get(C("char-x"))!.Health, Is.EqualTo(GeneralHealth.Grave), "斩杀→重创（陨落代理）。");
            // 幂等：再放一次无异常、仍重创。
            GeneralLifeReconciler.ApplyLore(ledger, ov, 190);
            Assert.That(ledger.Get(C("char-x"))!.Health, Is.EqualTo(GeneralHealth.Grave));
        }

        [Test]
        public void test_reassign_changes_master_and_is_idempotent()
        {
            // Arrange：许攸本属袁绍，覆盖移籍曹魏。
            var ledger = new GeneralLedger();
            ledger.Set(GeneralState.Fresh(C("char-y"), F("faction-yuanshao"), new CityId("city-ye"), 80));
            var ov = new LoreOverrides();
            ov.Apply(LoreEffect.Reassign(C("char-y"), F("faction-cao")));
            // Act
            GeneralLifeReconciler.ApplyLore(ledger, ov, 200);
            GeneralState after = ledger.Get(C("char-y"))!;
            // Assert：换主 + 忠诚重置中性。
            Assert.That(after.Faction!.Value.Value, Is.EqualTo("faction-cao"), "移籍换主。");
            Assert.That(after.Loyalty, Is.EqualTo(50), "新主忠诚中性起点。");

            // 幂等：手动升忠诚后再放 → 因现主已=目标，不再重置。
            ledger.Set(GeneralLifeService.AdjustLoyalty(after, 20)); // 70
            GeneralLifeReconciler.ApplyLore(ledger, ov, 200);
            Assert.That(ledger.Get(C("char-y"))!.Loyalty, Is.EqualTo(70), "重放不重置忠诚（幂等）。");
        }

        [Test]
        public void test_empty_overrides_no_op()
        {
            var ledger = new GeneralLedger();
            ledger.Set(GeneralState.Fresh(C("char-x"), F("faction-a"), new CityId("city-1"), 60));
            GeneralLifeReconciler.ApplyLore(ledger, LoreOverrides.Empty, 190);
            Assert.That(ledger.Get(C("char-x"))!.Health, Is.EqualTo(GeneralHealth.Hale), "空覆盖不改动。");
        }
    }
}
