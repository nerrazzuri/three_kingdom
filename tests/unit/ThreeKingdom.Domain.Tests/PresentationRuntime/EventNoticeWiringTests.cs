using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Domain.World;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 天下事件通报接入推进（GDD_015 A3）：运行期 Advance 触发到期历史事件 → 按可达性 + 人设产通报流。
    /// 切身事件（赤壁，玩家触及孙权）入通报为 Personal；背景事件不打扰；心里话口吻随人设（单测见 EventReflectionTests）。
    /// </summary>
    [TestFixture]
    public class EventNoticeWiringTests
    {
        [Test]
        public void test_advance_fires_history_and_surfaces_notice()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            runtime.Advance(2);   // 越过赤壁窗口起点（第0日黎明）→ 触发
            IReadOnlyList<EventNoticeView> notices = runtime.EventNotices();
            Assert.That(notices.Count, Is.GreaterThan(0), "推进触发历史事件 → 通报流产出（事件在轨推演，接入运行期）。");
        }

        [Test]
        public void test_unreachable_event_surfaces_persona_monologue()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            runtime.Advance(2);   // 袁术称帝（够不着）在早期窗口触发 → 通报 + 主角心里话
            bool anyMonologue = false;
            foreach (EventNoticeView n in runtime.EventNotices())
                if (n.Tier == NoticeTier.Notable && n.HasMonologue && n.Text.Length > 0) anyMonologue = true;
            Assert.That(anyMonologue, Is.True, "够不着的天下大事（袁术称帝）→ 通报带主角心里话（随人设着色）。");
        }

        [Test]
        public void test_romance_event_network_surfaces_multiple_monologues_over_timeline()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            int monologues = 0;
            var seen = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < 60; i++)   // 沿时间线推进（覆盖各事件时间窗）
            {
                runtime.Advance(1);
                foreach (EventNoticeView n in runtime.EventNotices())
                    if (n.HasMonologue) { monologues++; seen.Add(n.OutcomeLabel); }
            }
            Assert.That(monologues, Is.GreaterThanOrEqualTo(5), "演义主线多条事件在时间线上触发 → 心里话通报（事件网可玩）。");
            Assert.That(seen.Count, Is.GreaterThanOrEqualTo(5), "触发的是多条不同事件（非同一条重复）。");
        }

        [Test]
        public void test_notices_refresh_and_clear_each_advance()
        {
            var runtime = new CampaignRuntime(new InMemorySaveMedium());
            runtime.NewGame();
            // 推进足够多，触发完目录内全部事件（赤壁+夷陵）。
            for (int i = 0; i < 40; i++) runtime.Advance(1);
            // 事件既已触发（幂等），后续推进不再产新通报 → 通报流清空（每次推进刷新）。
            runtime.Advance(1);
            Assert.That(runtime.EventNotices(), Is.Empty, "事件耗尽后推进 → 通报流清空（每次推进刷新，不残留）。");
        }
    }
}
