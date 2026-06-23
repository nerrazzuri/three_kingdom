using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// EPIC_010 竖切续（Phase B6）：外交求粮展示视图（BLOCKING）。治理 ADR：ADR-0002 + GDD_012 §8。
    /// 覆盖未求援/在途/背约/拒绝/已抵达各状态的中文文案与按钮可用性。
    /// </summary>
    [TestFixture]
    public class DiplomacyViewTests
    {
        [Test]
        public void test_not_requested_allows_request()
        {
            var view = new DiplomacyView(new DiplomacyProjection(false, DiplomaticResponse.Rejected, false, -1, 0, 0));

            Assert.That(view.CanRequest, Is.True);
            Assert.That(view.StatusLabel, Does.Contain("求粮"));
        }

        [Test]
        public void test_accepted_pending_shows_arrival_day_and_amount()
        {
            var view = new DiplomacyView(new DiplomacyProjection(true, DiplomaticResponse.Accepted, true, 2, 120, 0));

            Assert.That(view.CanRequest, Is.False);
            Assert.That(view.StatusLabel, Does.Contain("120"));
            Assert.That(view.StatusLabel, Does.Contain("第 3 日"));
        }

        [Test]
        public void test_betrayed_shows_cost_paid()
        {
            var view = new DiplomacyView(new DiplomacyProjection(true, DiplomaticResponse.Accepted, false, -1, 0, 0));

            Assert.That(view.StatusLabel, Does.Contain("背约"));
            Assert.That(view.StatusLabel, Does.Contain("代价已付"));
        }

        [Test]
        public void test_rejected_shows_rejection()
        {
            var view = new DiplomacyView(new DiplomacyProjection(true, DiplomaticResponse.Rejected, false, -1, 0, 0));

            Assert.That(view.StatusLabel, Does.Contain("拒绝"));
        }

        [Test]
        public void test_delivered_shows_arrived()
        {
            var view = new DiplomacyView(new DiplomacyProjection(true, DiplomaticResponse.Accepted, true, -1, 0, 120));

            Assert.That(view.StatusLabel, Does.Contain("抵达"));
            Assert.That(view.StatusLabel, Does.Contain("120"));
        }
    }
}
