using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 君主任务接入运行期（GDD_014 / W5）：君主主动派任务 → 玩家去做 → 完成累积功绩通往晋升。
    /// 补齐"太守生涯循环"曾缺的脊梁——君主从被动变主动。
    /// </summary>
    [TestFixture]
    public class LordMissionRuntimeTests
    {
        [Test]
        public void test_lord_mission_offered_with_view_at_start()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();

            LordMission m = rt.CurrentMission();
            Assert.That(m, Is.Not.Null, "君主开局即有所命。");
            Assert.That(m.DeadlineYear, Is.EqualTo(rt.CurrentYear + 3), "期限=当前年+3。");

            LordMissionView v = rt.CurrentMissionView();
            Assert.That(new[] { "讨伐", "守土", "献纳" }, Does.Contain(v.TypeLabel));
            Assert.That(v.Order.Length, Is.GreaterThan(8), "有一句中文君命。");
        }

        [Test]
        public void test_completing_missions_accrues_career_merit_over_years()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            int merit0 = rt.CareerView().Merit;

            // 数年间：每年评估当前任务并结算，完成则换新（守土守到期即成、献纳粮足即成）。
            for (int i = 0; i < 20; i++) { rt.CheckMission(); rt.AdvanceYear(); }
            rt.CheckMission();

            Assert.That(rt.CareerView().Merit, Is.GreaterThan(merit0),
                "数年间完成若干君主任务 → 功绩累积（生涯循环因君主任务转起来）。");
        }

        [Test]
        public void test_check_mission_returns_defined_progress()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            Assert.That(System.Enum.IsDefined(typeof(MissionProgress), rt.CheckMission()), Is.True);
        }

        // ---- 两处收尾：献纳实扣粮 + 失败损名望 ----

        [Test]
        public void test_levy_grain_deducts_stock_and_rejects_when_short()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            var svc = new CampaignSessionService();
            long stock0 = rt.Session.CityEconomy!.Stock;

            Assert.That(svc.LevyGrain(rt.Session, 30), Is.True, "库存足 → 扣粮成功。");
            Assert.That(rt.Session.CityEconomy!.Stock, Is.EqualTo(stock0 - 30), "实扣 30 石。");
            Assert.That(svc.LevyGrain(rt.Session, 100000), Is.False, "库存不足 → 拒。");
            Assert.That(rt.Session.CityEconomy!.Stock, Is.EqualTo(stock0 - 30), "失败不改库存（无部分写入）。");
        }

        [Test]
        public void test_renown_penalty_reduces_renown_floored_at_zero()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            var svc = new CampaignSessionService();
            PromotionLadderConfig ladder = PlayableCampaign.Default().Ladder;

            svc.ApplyCareerGain(rt.Session, ladder, CareerGainSource.MajorBattleVictory);   // 先得些名望
            int r0 = rt.CareerView().Renown;
            Assert.That(r0, Is.GreaterThan(0), "已积名望。");

            svc.PenalizeRenown(rt.Session, 20);
            Assert.That(rt.CareerView().Renown, Is.EqualTo(System.Math.Max(0, r0 - 20)), "名望减 20（下限 0）。");

            svc.PenalizeRenown(rt.Session, 100000);
            Assert.That(rt.CareerView().Renown, Is.EqualTo(0), "重罚亦不为负。");
        }
    }
}
