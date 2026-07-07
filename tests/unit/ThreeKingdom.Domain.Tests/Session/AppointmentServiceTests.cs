using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Appointment;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>任用合法性门（GDD_027 P3）：须在世/非俘/非重创/属玩家麾下（事奉或已招揽），防表现层污染权威态。</summary>
    [TestFixture]
    public class AppointmentServiceTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static CityId City(string id) => new CityId(id);
        private const int Y = 190;
        private static readonly FactionId Shu = PlayableCampaign.LiuBei;
        private static readonly CityId Xiaopei = new CityId("city-xiaopei");

        private static (AppointGate, AppointmentBook) Assign(CharacterId g, GeneralLedger ledger = null, TalentKnowledgeBook talents = null)
            => AppointmentService.Assign(AppointmentBook.Empty(20), Xiaopei, g, Y, Shu, ledger ?? new GeneralLedger(), talents ?? new TalentKnowledgeBook());

        [Test]
        public void test_player_in_service_general_can_be_appointed()
        {
            // 关羽190事奉刘备驻小沛 → 可任用。
            var (gate, _) = Assign(C("char-guanyu"));
            Assert.That(gate, Is.EqualTo(AppointGate.Ok), "麾下在职将可任用。");
        }

        [Test]
        public void test_enemy_general_rejected_as_not_yours()
        {
            // 曹操190事奉曹魏，非玩家（刘备）麾下 → 拒。
            var (gate, _) = Assign(C("char-caocao"));
            Assert.That(gate, Is.EqualTo(AppointGate.NotYours), "非麾下拒。");
        }

        [Test]
        public void test_recruited_wandering_general_can_be_appointed()
        {
            // 司马徽在野，但已招揽入伙 → 可任用。
            var talents = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(talents, C("char-simahui"), RecruitChannel.Visit, Y);
            // 直接置为已入伙（模拟招揽成功）。
            while (talents.OutcomeOf(C("char-simahui")) != RecruitOutcome.Joined)
            {
                var r = TalentRecruitment.Attempt(talents, C("char-simahui"), 3, 9, 777UL);
                if (!r.Accepted) break;
                if (r.Outcome == RecruitOutcome.Joined) break;
                if (r.Outcome == RecruitOutcome.Resented || r.Outcome == RecruitOutcome.Defected) break;
            }
            Assume.That(talents.OutcomeOf(C("char-simahui")), Is.EqualTo(RecruitOutcome.Joined));
            var (gate, _) = Assign(C("char-simahui"), talents: talents);
            Assert.That(gate, Is.EqualTo(AppointGate.Ok), "已招揽入伙者可任用（即便在野出身）。");
        }

        [Test]
        public void test_captive_and_grave_rejected()
        {
            var ledger = new GeneralLedger();
            ledger.Set(GeneralLifeService.Capture(GeneralState.Fresh(C("char-guanyu"), Shu, Xiaopei, 80), new FactionId("faction-cao")));
            Assert.That(Assign(C("char-guanyu"), ledger: ledger).Item1, Is.EqualTo(AppointGate.Captive), "在押拒。");

            var ledger2 = new GeneralLedger();
            ledger2.Set(GeneralState.Fresh(C("char-guanyu"), Shu, Xiaopei, 80).WithHealth(GeneralHealth.Grave));
            Assert.That(Assign(C("char-guanyu"), ledger: ledger2).Item1, Is.EqualTo(AppointGate.Incapacitated), "重创拒。");
        }

        [Test]
        public void test_absent_general_rejected()
        {
            // 姜维202生，190未出世 → 不在世间。
            var (gate, _) = Assign(C("char-jiangwei"));
            Assert.That(gate, Is.EqualTo(AppointGate.Absent), "未出世拒。");
        }
    }
}
