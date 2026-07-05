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
        public void test_calendar_derives_year_and_season_from_week_deterministically()
        {
            var cal = new EraCalendar(startYear: 190);   // 52 周/年, 13 周/季（一步=一周）
            Assert.That(cal.YearOf(new WorldTime(0, DaySegment.Dawn)), Is.EqualTo(190), "开局即锚点年。");
            Assert.That(cal.YearOf(new WorldTime(51, DaySegment.Dawn)), Is.EqualTo(190), "未满 52 周仍在锚点年。");
            Assert.That(cal.YearOf(new WorldTime(52, DaySegment.Dawn)), Is.EqualTo(191), "满 52 周跨一年。");
            Assert.That(cal.YearOf(new WorldTime(520, DaySegment.Dawn)), Is.EqualTo(200), "十年后。");
            // 季：0春/1夏/2秋/3冬（每 13 周一季）。
            Assert.That(cal.SeasonOfYear(new WorldTime(0, DaySegment.Dawn)), Is.EqualTo(0), "春。");
            Assert.That(cal.SeasonOfYear(new WorldTime(13, DaySegment.Dawn)), Is.EqualTo(1), "夏。");
            Assert.That(cal.SeasonOfYear(new WorldTime(39, DaySegment.Dawn)), Is.EqualTo(3), "冬。");
            Assert.That(cal.SeasonOfYear(new WorldTime(52, DaySegment.Dawn)), Is.EqualTo(0), "次年春。");
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

        [Test]
        public void test_runtime_week_season_year_cadence()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            Assert.That(runtime.CurrentYear, Is.EqualTo(190));
            Assert.That(runtime.CurrentSeason, Is.EqualTo(0), "开局春季。");

            runtime.AdvanceSeason();
            Assert.That(runtime.CurrentSeason, Is.EqualTo(1), "过一季 → 夏。");
            Assert.That(runtime.CurrentYear, Is.EqualTo(190), "同年内。");

            runtime.AdvanceYear();
            Assert.That(runtime.CurrentYear, Is.EqualTo(191), "过一年 → 次年。");
            Assert.That(runtime.CurrentSeasonLabel, Is.EqualTo("夏"), "跳整年季相不变。");
        }

        [Test]
        public void test_events_spread_across_life_not_all_at_start()
        {
            // 历史事件按公元年铺开：早年（190–192）不该出现赤壁(208)/夷陵(222)等晚期大事。
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            var early = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < 3; i++)
            {
                runtime.AdvanceYear();
                foreach (EventNoticeView n in runtime.EventNotices()) early.Add(n.OutcomeLabel);
            }
            Assert.That(early.Count, Is.GreaterThan(0), "早年已有开局期事件（桃园/董卓焚洛阳等）。");
            foreach (string lbl in early)
            {
                Assert.That(lbl.Contains("chibi"), Is.False, "赤壁属后期(208)，早年不现。");
                Assert.That(lbl.Contains("yiling"), Is.False, "夷陵属后期(222)，早年不现。");
            }
        }
    }
}
