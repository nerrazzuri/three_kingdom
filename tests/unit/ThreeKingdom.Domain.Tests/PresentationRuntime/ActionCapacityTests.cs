using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 行动容量节流（GDD_014 / 2026-07-05：手下有限，同时能办的差事有限，随官阶增长）：
    /// 非体力点数——与"时长制"互补（时长管多久见效、容量管同时几件）。满员则拒，办完则腾出人手。
    /// </summary>
    [TestFixture]
    public class ActionCapacityTests
    {
        [Test]
        public void test_governor_starts_with_two_action_slots()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            Assert.That(rt.ActionCapacity, Is.EqualTo(2), "太守（rank0）手令上限 2。");
            Assert.That(rt.ActionsInFlight, Is.EqualTo(0), "开局无在办差事。");
            Assert.That(rt.HasFreeAgent, Is.True);
        }

        [Test]
        public void test_dispatch_fills_slots_then_rejects_when_full()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();

            Assert.That(rt.Repair().Applied, Is.True, "第一件差事：修工事。");
            Assert.That(rt.Appease().Applied, Is.True, "第二件：安抚。");
            Assert.That(rt.ActionsInFlight, Is.EqualTo(2), "两件在办，满员。");

            CampaignCommandResult third = rt.Requisition(20);
            Assert.That(third.Applied, Is.False, "满员 → 第三件被拒。");
            Assert.That(third.Error, Is.EqualTo(CampaignErrorCode.NoFreeAgent), "稳定错误码：手下已满。");
            Assert.That(rt.ScoutEnemy().Applied, Is.False, "侦察同样占人手，满员亦拒。");
        }

        [Test]
        public void test_slots_free_up_after_tasks_resolve_over_time()
        {
            var rt = new CampaignRuntime(new InMemorySaveMedium());
            rt.NewGame();
            rt.Repair();
            rt.Appease();
            Assert.That(rt.HasFreeAgent, Is.False, "满员。");

            // 推进数周 → 在办差事陆续办成、腾出人手。
            for (int i = 0; i < 6; i++) rt.AdvanceWeek();
            Assert.That(rt.HasFreeAgent, Is.True, "差事办完 → 人手腾出，可再遣。");
            Assert.That(rt.Requisition(20).Applied, Is.True);
        }
    }
}
