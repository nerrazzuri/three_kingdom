using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>招揽状态机 + 反全知门（GDD_027 #2 / GDD_020）：未闻名不可见，逐级发觉→接触→确定性招揽结算。</summary>
    [TestFixture]
    public class TalentRecruitmentTests
    {
        private const int Y = 190;
        private static CharacterId C(string id) => new CharacterId(id);
        private static readonly CharacterId Sage = new CharacterId("char-simahui"); // 司马徽·在野

        private static bool PoolHas(System.Collections.Generic.IReadOnlyList<KnownTalent> pool, string id)
        {
            foreach (KnownTalent t in pool) if (t.GeneralId == id) return true;
            return false;
        }

        [Test]
        public void test_anti_omniscience_gate_hides_unknown_talents()
        {
            // Arrange：全新知晓簿——玩家什么都没发觉。
            var book = new TalentKnowledgeBook();
            // Act
            var before = TalentRecruitment.KnownPool(Y, book);
            TalentRecruitment.Reveal(book, Sage, RecruitChannel.Scout, Y);
            var after = TalentRecruitment.KnownPool(Y, book);
            // Assert：未发觉时可见池空（反全知）；发觉后司马徽现身。
            Assert.That(before.Count, Is.EqualTo(0), "未发觉 → 招揽列表为空（不露全部在野将）。");
            Assert.That(PoolHas(after, "char-simahui"), Is.True, "侦察发觉后司马徽入可见池。");
        }

        [Test]
        public void test_discovery_ladder_by_channel()
        {
            // Scout=听闻（可见但不可招）；Visit=接触（可招）。
            var book = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(book, Sage, RecruitChannel.Scout, Y);
            Assert.That(book.DiscoveryOf(Sage), Is.EqualTo(TalentKnowledge.Heard));
            Assert.That(book.CanAttempt(Sage), Is.False, "仅听闻不可招。");
            TalentRecruitment.Reveal(book, Sage, RecruitChannel.Visit, Y);
            Assert.That(book.DiscoveryOf(Sage), Is.EqualTo(TalentKnowledge.Contacted));
            Assert.That(book.CanAttempt(Sage), Is.True, "亲访接触后可招。");
        }

        [Test]
        public void test_attempt_rejected_before_contact()
        {
            var book = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(book, Sage, RecruitChannel.Scout, Y); // 仅听闻
            RecruitAttemptResult r = TalentRecruitment.Attempt(book, Sage, renownTier: 0, offerTier: 3, seed: 1UL);
            Assert.That(r.Accepted, Is.False, "未接触 → 招揽不受理。");
        }

        [Test]
        public void test_generous_offer_recruits_easy_target()
        {
            // Arrange：接触易招者 + 厚待遇 → 应入伙。
            var book = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(book, Sage, RecruitChannel.Visit, Y);
            // Act
            RecruitAttemptResult r = TalentRecruitment.Attempt(book, Sage, renownTier: 3, offerTier: 9, seed: 777UL);
            // Assert
            Assert.That(r.Accepted, Is.True);
            Assert.That(r.Outcome, Is.EqualTo(RecruitOutcome.Joined), "厚待招易招者 → 入伙。");
            Assert.That(book.CanAttempt(Sage), Is.False, "已入伙不再可招。");
        }

        [Test]
        public void test_attempt_is_deterministic()
        {
            // 两簿同态、同种子 → 同结果。
            var b1 = new TalentKnowledgeBook(); TalentRecruitment.Reveal(b1, Sage, RecruitChannel.Visit, Y);
            var b2 = new TalentKnowledgeBook(); TalentRecruitment.Reveal(b2, Sage, RecruitChannel.Visit, Y);
            RecruitOutcome o1 = TalentRecruitment.Attempt(b1, Sage, 0, 2, 424242UL).Outcome;
            RecruitOutcome o2 = TalentRecruitment.Attempt(b2, Sage, 0, 2, 424242UL).Outcome;
            Assert.That(o1, Is.EqualTo(o2), "同态同种子 → 招揽结果确定。");
        }

        [Test]
        public void test_reveal_ignores_non_wandering_general()
        {
            // 在职将（关羽·刘备）不入招揽视野。
            var book = new TalentKnowledgeBook();
            TalentRecruitment.Reveal(book, C("char-guanyu"), RecruitChannel.Visit, Y);
            Assert.That(book.IsKnown(C("char-guanyu")), Is.False, "在职将不入招揽知晓簿。");
        }
    }
}
