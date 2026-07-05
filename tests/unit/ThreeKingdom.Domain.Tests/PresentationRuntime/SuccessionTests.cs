using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 一生的闭环·传承（GDD_026 R6）：空降者寿终后由子嗣续局——同世界、同治所、生涯延续，新一世自当前公元年弱冠起。
    /// 寿终是可续的自然落幕（非 game-over），执掌者换代而天下照旧流转。
    /// </summary>
    [TestFixture]
    public class SuccessionTests
    {
        [Test]
        public void test_cannot_succeed_before_death()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            Assert.That(rt.IsLifeOver, Is.False, "开局未寿终。");
            Assert.That(rt.Generation, Is.EqualTo(0), "开局为第一世空降者。");
            Assert.Throws<InvalidOperationException>(() => rt.SucceedHeir(), "未寿终不能传承。");
        }

        [Test]
        public void test_life_ends_and_heir_continues_same_world()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();

            // 跳时·过 70 年 → 必越大限（寿命上限 55 年）。
            for (int i = 0; i < 70; i++) rt.AdvanceYear();
            Assert.That(rt.IsLifeOver, Is.True, "数十年后空降者寿终。");
            int deathYear = rt.CurrentYear;

            ArrivalLifeView heir = rt.SucceedHeir();
            Assert.That(rt.Generation, Is.EqualTo(1), "子嗣接掌为第二世。");
            Assert.That(heir.Age, Is.EqualTo(20), "子嗣弱冠接掌。");
            Assert.That(heir.IsOver, Is.False, "新一世方始，未寿终。");
            Assert.That(rt.CurrentYear, Is.EqualTo(deathYear), "同世界同年延续——只是执掌者换了一代。");
            Assert.That(rt.IsLifeOver, Is.False, "传承后重回在世。");
        }
    }
}
