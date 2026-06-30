using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Numerics;
using H = ThreeKingdom.Domain.Tests.World.CampaignHistoryStateTests;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-023 story-004：历史世界态存读档 + 同序列同走向（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档）+ ADR-0004（确定性）。TR-world-006/002。
    /// 覆盖：triggered/diverged round-trip（world 段）；哈希一致；同行动序列同走向。
    /// </summary>
    [TestFixture]
    public class CampaignHistorySaveTests
    {
        private static CampaignSessionService Service => H.Service;

        // ---- AC-1: 触发态 round-trip（triggered 在 world 段）----

        [Test]
        public void test_triggered_state_roundtrip()
        {
            CampaignSession s = H.NewSession(sunAlive: true, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(s);   // 触发赤壁

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), H.Fp);

            Assert.That(loaded.World.IsTriggered(H.Chibi.Value), Is.True, "触发态 round-trip（world 段）");
        }

        // ---- AC-2: 分叉态 round-trip（diverged 在 world 段，TR-world-006）----

        [Test]
        public void test_diverged_state_roundtrip()
        {
            CampaignSession s = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(s);   // 赤壁分叉 + 下游传播

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), H.Fp);

            Assert.That(loaded.World.IsDiverged(H.Chibi.Value), Is.True, "分叉态 round-trip");
            Assert.That(loaded.World.IsDiverged(H.Yiling.Value), Is.True, "下游分叉态 round-trip");
        }

        // ---- AC-3: round-trip 哈希一致 ----

        [Test]
        public void test_history_roundtrip_preserves_hash()
        {
            CampaignSession s = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(s);
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Service.Restore(Service.CaptureSnapshot(s), H.Fp);

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-4: 同一行动序列同一历史走向（TR-world-002）----

        [Test]
        public void test_same_action_sequence_same_history()
        {
            CampaignSession a = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            CampaignSession b = H.NewSession(sunAlive: false, reach: H.ReachTouchingSun());
            Service.AdvanceHistory(a);
            Service.AdvanceHistory(b);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()), "同行动序列 → 同历史走向");
        }
    }
}
