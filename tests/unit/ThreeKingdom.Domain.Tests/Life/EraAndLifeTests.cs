using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Life;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Life
{
    /// <summary>
    /// 纪元日历 + 空降者一生（GDD_026 / ADR-0015）：公元年由抽象日-段纯函数派生；寿命由种子确定性派生，
    /// 寿终为可续的自然落幕。只给人生阶段定性档，不给精确倒计时（反全知延伸至己身）。
    /// </summary>
    [TestFixture]
    public class EraAndLifeTests
    {
        // ---- 纪元日历 ----
        [Test]
        public void test_calendar_derives_year_from_day_deterministically()
        {
            var cal = new EraCalendar(startYear: 190, daysPerYear: 12);
            Assert.That(cal.YearOf(new WorldTime(0, DaySegment.Dawn)), Is.EqualTo(190), "开局即锚点年。");
            Assert.That(cal.YearOf(new WorldTime(11, DaySegment.Night)), Is.EqualTo(190), "未满一年仍在锚点年。");
            Assert.That(cal.YearOf(new WorldTime(12, DaySegment.Dawn)), Is.EqualTo(191), "满 12 日跨一年。");
            Assert.That(cal.YearOf(new WorldTime(120, DaySegment.Dawn)), Is.EqualTo(200), "十年后。");
        }

        // ---- 空降者寿命 ----
        [Test]
        public void test_lifespan_is_deterministic_and_within_band()
        {
            var cfg = ArrivalLifeConfig.Default;   // base 48 ± 7 → [41,55]
            ArrivalLife a = ArrivalLife.Roll(12345UL, 190, cfg);
            ArrivalLife b = ArrivalLife.Roll(12345UL, 190, cfg);
            Assert.That(a.Lifespan, Is.EqualTo(b.Lifespan), "同种子 → 同寿命（存读档一致）。");
            Assert.That(a.Lifespan, Is.InRange(cfg.LifespanBase - cfg.LifespanSpread, cfg.LifespanBase + cfg.LifespanSpread));
            Assert.That(a.EntryAge, Is.EqualTo(20));
            Assert.That(a.DeathYear, Is.EqualTo(190 + a.Lifespan));
            Assert.That(a.DeathAge, Is.EqualTo(20 + a.Lifespan));
        }

        [Test]
        public void test_life_phases_and_death()
        {
            // 寿命固定构造：入场 20、寿 40 → 卒于 230（60 岁）。
            var cfg = ArrivalLifeConfig.Default;
            var life = new ArrivalLife(entryAge: 20, lifespan: 40, startYear: 190, config: cfg);

            Assert.That(life.AgeAt(190), Is.EqualTo(20));
            Assert.That(life.PhaseAt(190), Is.EqualTo(LifePhase.Vigorous), "开局春秋鼎盛。");
            Assert.That(life.PhaseAt(230 - 15), Is.EqualTo(LifePhase.Aging), "大限前 15 年起年事渐高。");
            Assert.That(life.PhaseAt(230 - 5), Is.EqualTo(LifePhase.Twilight), "大限前 5 年起风烛残年。");
            Assert.That(life.IsOver(229), Is.False);
            Assert.That(life.IsOver(230), Is.True, "至卒年即寿终。");
        }

        // ---- 运行期接线 ----
        [Test]
        public void test_runtime_exposes_year_and_life_at_arrival()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();

            Assert.That(runtime.CurrentYear, Is.EqualTo(190), "开局即 190 讨董之世。");
            ArrivalLifeView v = runtime.LifeView();
            Assert.That(v.Year, Is.EqualTo(190));
            Assert.That(v.Age, Is.EqualTo(20), "弱冠入场。");
            Assert.That(v.IsOver, Is.False, "开局未寿终。");
            Assert.That(v.PhaseLabel, Is.EqualTo("春秋鼎盛"));
        }
    }
}
