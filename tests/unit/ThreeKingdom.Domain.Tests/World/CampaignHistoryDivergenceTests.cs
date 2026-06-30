using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.World;
using H = ThreeKingdom.Domain.Tests.World.CampaignHistoryStateTests;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-023 story-003：玩家触及分叉 + 下游传播（Integration / Assembly）。
    /// 治理 ADR：ADR-0007（分叉下游重评估）+ ADR-0004（确定性）。TR-world-002。
    /// 覆盖：够得着 + 前置被破坏 → 分叉；下游传播；确定性。
    /// </summary>
    [TestFixture]
    public class CampaignHistoryDivergenceTests
    {
        private static CampaignSessionService Service => H.Service;

        // ---- AC-1: 够得着 + 前置破坏 → 分叉 ----

        [Test]
        public void test_reachable_broken_precondition_diverges()
        {
            // 孙已灭（前置破坏）+ 触及孙（够得着）→ 赤壁分叉。
            CampaignSession s = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());

            IReadOnlyList<HistoryAdvanceResult> results = Service.AdvanceHistory(s);

            Assert.That(s.World.IsTriggered(H.Chibi.Value), Is.True);
            Assert.That(s.World.IsDiverged(H.Chibi.Value), Is.True, "够得着+前置破坏→分叉");
            Assert.That(results.Any(r => r.Diverged), Is.True);
        }

        // ---- AC-2: 分叉下游传播重评估 ----

        [Test]
        public void test_divergence_propagates_downstream()
        {
            // 赤壁分叉 → 下游夷陵被传播重评估（脱稿深度≥1）。
            CampaignSession s = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());

            Service.AdvanceHistory(s);

            // 下游夷陵经分叉传播标记（DivergedEvents 含下游或其态变化）。
            Assert.That(s.World.IsDiverged(H.Chibi.Value), Is.True, "源分叉");
            // 传播后下游夷陵在分叉集（脱稿深度 2 覆盖一层下游）。
            Assert.That(s.World.IsDiverged(H.Yiling.Value), Is.True, "下游传播重评估");
        }

        // ---- AC-3: 够得着但前置成立 → 不分叉（对照）----

        [Test]
        public void test_reachable_precondition_held_no_divergence()
        {
            CampaignSession s = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(s);

            Assert.That(s.World.IsTriggered(H.Chibi.Value), Is.True);
            Assert.That(s.World.IsDiverged(H.Chibi.Value), Is.False, "前置成立不分叉");
        }

        // ---- AC-4: 分叉确定性 ----

        [Test]
        public void test_divergence_is_deterministic()
        {
            CampaignSession a = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            CampaignSession b = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(a);
            Service.AdvanceHistory(b);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同序列同分叉走向");
        }
    }
}
