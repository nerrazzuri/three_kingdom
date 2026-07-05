using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Domain.World;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 天下事件通报接入推进（GDD_015 A3 / GDD_026）：历史事件按<b>公元年</b>在一生里铺开——玩家沿季/年推进逐年遇之。
    /// 切身事件（赤壁，玩家触及孙权）入通报；够不着的大事带主角心里话（随人设着色）。
    /// </summary>
    [TestFixture]
    public class EventNoticeWiringTests
    {
        [Test]
        public void test_advancing_years_fires_history_and_surfaces_notice()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            // 过头一年 → 190 年的开局事件（桃园/董卓焚洛阳）触发。
            runtime.AdvanceYear();
            Assert.That(runtime.EventNotices().Count, Is.GreaterThan(0), "推进跨年触发历史事件 → 通报流产出。");
        }

        [Test]
        public void test_unreachable_event_surfaces_persona_monologue_over_life()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            bool anyMonologue = false;
            for (int i = 0; i < 12 && !anyMonologue; i++)   // 190→~202，越过袁术称帝(197)等够不着大事
            {
                runtime.AdvanceYear();
                foreach (EventNoticeView n in runtime.EventNotices())
                    if (n.Tier == NoticeTier.Notable && n.HasMonologue && n.Text.Length > 0) anyMonologue = true;
            }
            Assert.That(anyMonologue, Is.True, "够不着的天下大事 → 通报带主角心里话（随人设着色）。");
        }

        [Test]
        public void test_romance_event_network_surfaces_multiple_monologues_over_timeline()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            int monologues = 0;
            var seen = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < 40; i++)   // 沿一生时间线逐年推进（190→230，覆盖演义主线各年）
            {
                runtime.AdvanceYear();
                foreach (EventNoticeView n in runtime.EventNotices())
                    if (n.HasMonologue) { monologues++; seen.Add(n.OutcomeLabel); }
            }
            Assert.That(monologues, Is.GreaterThanOrEqualTo(5), "演义主线多条事件沿一生触发 → 心里话通报（事件网可玩）。");
            Assert.That(seen.Count, Is.GreaterThanOrEqualTo(5), "触发的是多条不同事件（非同一条重复）。");
        }

        [Test]
        public void test_notices_clear_after_events_exhausted()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            // 推进过全部事件年（末为夷陵 222）→ 事件耗尽。
            for (int i = 0; i < 45; i++) runtime.AdvanceYear();
            // 事件既已触发（幂等），后续推进不再产新通报 → 通报流清空（每次推进刷新）。
            runtime.AdvanceYear();
            Assert.That(runtime.EventNotices(), Is.Empty, "事件耗尽后推进 → 通报流清空（每次推进刷新，不残留）。");
        }
    }
}
