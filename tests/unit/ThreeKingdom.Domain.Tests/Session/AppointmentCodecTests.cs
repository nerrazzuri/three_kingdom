using NUnit.Framework;
using ThreeKingdom.Domain.Appointment;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>任用簿持久化（GDD_027 P3 / ADR-0005）：城册调拨无损 round-trip + 向后兼容空。</summary>
    [TestFixture]
    public class AppointmentCodecTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);
        private readonly AppointmentCodec _codec = new AppointmentCodec();

        [Test]
        public void test_round_trip_preserves_assignments()
        {
            // Arrange：两城若干调拨。
            AppointmentBook book = AppointmentBook.Empty(20);
            book = book.Assign(City("city-xiaopei"), C("char-guanyu")).Book;
            book = book.Assign(City("city-xiaopei"), C("char-zhangfei")).Book;
            book = book.Assign(City("city-xuzhou"), C("char-chendao")).Book;

            // Act
            string text = _codec.Serialize(book);
            AppointmentBook back = _codec.Deserialize(text);

            // Assert
            Assert.That(back.CityCap, Is.EqualTo(20));
            Assert.That(back.Roster(City("city-xiaopei")).Count, Is.EqualTo(2), "小沛 2 员保留。");
            Assert.That(back.CityOf(C("char-guanyu")), Is.EqualTo("city-xiaopei"), "关羽任用城保留。");
            Assert.That(back.CityOf(C("char-chendao")), Is.EqualTo("city-xuzhou"), "陈到任用城保留。");
        }

        [Test]
        public void test_empty_and_null_decode_to_empty_book()
        {
            Assert.That(_codec.Deserialize(null).Cities().Count, Is.EqualTo(0), "旧存档无此段 → 空簿。");
            Assert.That(_codec.Deserialize("").Cities().Count, Is.EqualTo(0));
            Assert.That(_codec.Deserialize(_codec.Serialize(AppointmentBook.Empty())).Cities().Count, Is.EqualTo(0));
        }
    }
}
