using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>武将人生台账持久化（ADR-0017 / ADR-0005）：GeneralLedgerCodec 无损 round-trip + 向后兼容空。</summary>
    [TestFixture]
    public class GeneralLedgerCodecTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private readonly GeneralLedgerCodec _codec = new GeneralLedgerCodec();

        [Test]
        public void test_round_trip_preserves_all_state()
        {
            // Arrange：含主君/驻城/忠诚/负伤/被俘/多记忆的一将 + 一无主将。
            var ledger = new GeneralLedger();
            var s1 = new GeneralState(C("char-guanyu"), new FactionId("faction-liubei"), new CityId("city-jiangling"),
                42, GeneralHealth.Wounded, 30, new FactionId("faction-sun"),
                new[] { new MemoryEvent(MemoryKind.Betrayed, C("char-mifang"), 219, 2), new MemoryEvent(MemoryKind.Rescued, C("char-liubei"), 200, 1) });
            var s2 = GeneralState.Fresh(C("char-x"), null, null, 60);
            ledger.Set(s1); ledger.Set(s2);

            // Act
            string text = _codec.Serialize(ledger);
            GeneralLedger back = _codec.Deserialize(text);

            // Assert
            Assert.That(back.Count, Is.EqualTo(2));
            GeneralState r = back.Get(C("char-guanyu"))!;
            Assert.That(r.Faction!.Value.Value, Is.EqualTo("faction-liubei"));
            Assert.That(r.Location!.Value.Value, Is.EqualTo("city-jiangling"));
            Assert.That(r.Loyalty, Is.EqualTo(42));
            Assert.That(r.Health, Is.EqualTo(GeneralHealth.Wounded));
            Assert.That(r.Fatigue, Is.EqualTo(30));
            Assert.That(r.CaptiveOf!.Value.Value, Is.EqualTo("faction-sun"));
            Assert.That(r.Memories.Count, Is.EqualTo(2));
            Assert.That(r.MemoryWeight(MemoryKind.Betrayed), Is.EqualTo(2));
            GeneralState r2 = back.Get(C("char-x"))!;
            Assert.That(r2.Faction.HasValue, Is.False, "无主将 null 归属 round-trip。");
        }

        [Test]
        public void test_canonical_output_is_deterministic()
        {
            var a = new GeneralLedger(); a.Set(GeneralState.Fresh(C("char-b"), null, null, 50)); a.Set(GeneralState.Fresh(C("char-a"), null, null, 70));
            var b = new GeneralLedger(); b.Set(GeneralState.Fresh(C("char-a"), null, null, 70)); b.Set(GeneralState.Fresh(C("char-b"), null, null, 50));
            Assert.That(_codec.Serialize(a), Is.EqualTo(_codec.Serialize(b)), "按 id 稳定序 → 规范输出与插入序无关。");
        }

        [Test]
        public void test_empty_and_null_decode_to_empty_ledger()
        {
            Assert.That(_codec.Deserialize(null).Count, Is.EqualTo(0), "无此段的旧存档 → 空台账（向后兼容）。");
            Assert.That(_codec.Deserialize("").Count, Is.EqualTo(0));
            Assert.That(_codec.Deserialize(_codec.Serialize(new GeneralLedger())).Count, Is.EqualTo(0), "空台账 round-trip 仍空。");
        }
    }
}
