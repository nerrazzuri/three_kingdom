using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>人才知晓簿持久化（GDD_027 #2 / ADR-0005）：反全知发觉进度无损 round-trip + 向后兼容空。</summary>
    [TestFixture]
    public class TalentKnowledgeCodecTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private const int Y = 190;

        [Test]
        public void test_round_trip_preserves_discovery_and_attempts()
        {
            // Arrange：发觉两人才 + 一次招揽尝试。
            var book = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(book, C("char-simahui"), RecruitChannel.Visit, Y);   // 接触
            TalentRecruitment.Reveal(book, C("char-cuizhouping"), RecruitChannel.Scout, Y); // 听闻
            TalentRecruitment.Attempt(book, C("char-simahui"), 0, 1, 42UL);                // 一次尝试

            // Act
            string text = TalentKnowledgeCodec.Serialize(book);
            TalentKnowledgeBook back = TalentKnowledgeCodec.Deserialize(text);

            // Assert
            Assert.That(back.DiscoveryOf(C("char-simahui")), Is.EqualTo(book.DiscoveryOf(C("char-simahui"))), "接触态保留。");
            Assert.That(back.AttemptsOf(C("char-simahui")), Is.EqualTo(book.AttemptsOf(C("char-simahui"))), "尝试次数保留。");
            Assert.That(back.OutcomeOf(C("char-simahui")), Is.EqualTo(book.OutcomeOf(C("char-simahui"))), "末次结果保留。");
            Assert.That(back.IsKnown(C("char-cuizhouping")), Is.True, "听闻态保留（反全知发觉不丢）。");
        }

        [Test]
        public void test_empty_and_null_decode_to_empty_book()
        {
            Assert.That(TalentKnowledgeCodec.Deserialize(null).Entries.Count, Is.EqualTo(0), "旧存档无此段 → 空簿（向后兼容）。");
            Assert.That(TalentKnowledgeCodec.Deserialize("").Entries.Count, Is.EqualTo(0));
            Assert.That(TalentKnowledgeCodec.Deserialize(TalentKnowledgeCodec.Serialize(new TalentKnowledgeBook())).Entries.Count, Is.EqualTo(0));
        }
    }
}
