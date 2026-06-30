using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.World;
using H = ThreeKingdom.Domain.Tests.World.CampaignHistoryStateTests;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-023 story-002：历史事件按时间窗触发（够不着前置短路）（Integration / Assembly）。
    /// 治理 ADR：ADR-0007（reachability 门）+ ADR-0004（确定性）。TR-world-002。
    /// 覆盖：到期触发；够不着前置短路恒正常结局；够得着前置成立正常；触发态写 world。
    /// </summary>
    [TestFixture]
    public class CampaignHistoryTriggerTests
    {
        private static CampaignSessionService Service => H.Service;

        // ---- AC-1: 到期事件触发（够得着 + 前置成立 → 正常结局）----

        [Test]
        public void test_due_event_fires_normal_when_preconditions_hold()
        {
            // 孙存活（前置成立）+ 触及 → 正常历史结局。
            CampaignSession s = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());

            IReadOnlyList<HistoryAdvanceResult> results = Service.AdvanceHistory(s);

            Assert.That(results.Any(r => r.Fired), Is.True, "到期事件触发");
            Assert.That(s.World.IsTriggered(H.Chibi.Value), Is.True, "触发态写入 world");
            Assert.That(results.First(r => r.Fired).Diverged, Is.False, "前置成立→正常结局非分叉");
        }

        // ---- AC-2: 够不着前置短路恒成立（reachability 门，TR-world-002 核心）----

        [Test]
        public void test_unreachable_event_fires_normal_short_circuit()
        {
            // 孙已灭（前置本不成立）但够不着（reach=None）→ 短路前置，正常结局（早期历史便宜）。
            CampaignSession s = H.NewSession(sunAlive: false, reach: ThreeKingdom.Domain.World.PlayerReach.None);

            IReadOnlyList<HistoryAdvanceResult> results = Service.AdvanceHistory(s);

            HistoryAdvanceResult chibi = results.First(r => true);
            Assert.That(s.World.IsTriggered(H.Chibi.Value), Is.True, "够不着也照常触发");
            Assert.That(s.World.IsDiverged(H.Chibi.Value), Is.False, "够不着前置短路→不分叉（继续历史）");
        }

        // ---- AC-3: 重复推进不重复触发 ----

        [Test]
        public void test_already_triggered_not_refired()
        {
            CampaignSession s = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(s);
            IReadOnlyList<HistoryAdvanceResult> second = Service.AdvanceHistory(s);

            Assert.That(second.Count, Is.EqualTo(0), "已触发事件不重复触发");
        }

        // ---- AC-4: 触发确定性 ----

        [Test]
        public void test_trigger_is_deterministic()
        {
            CampaignSession a = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());
            CampaignSession b = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(a);
            Service.AdvanceHistory(b);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同序列同历史走向");
        }
    }
}
