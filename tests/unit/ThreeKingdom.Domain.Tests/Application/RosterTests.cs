using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切续（Phase B7）：人物花名册（BLOCKING）。治理 ADR：ADR-0002 + GDD_005。
    /// 覆盖关键人物投影 + 中文展示（身份/职责/健康/能力五域），能力为量表读值非解锁门槛。
    /// </summary>
    [TestFixture]
    public class RosterTests
    {
        [Test]
        public void test_roster_projects_four_key_characters()
        {
            var service = new SessionService();

            var roster = service.ProjectRoster(service.NewGame());

            Assert.That(roster.Characters, Has.Count.EqualTo(4), "主将/军师/外勤/敌将。");
        }

        [Test]
        public void test_roster_view_renders_identity_role_health_and_capabilities()
        {
            var service = new SessionService();
            var roster = service.ProjectRoster(service.NewGame());

            var view = new RosterView(roster);

            Assert.That(view.Characters, Has.Count.EqualTo(4));
            var first = view.Characters[0];
            Assert.That(first.Title, Does.Contain("秦烈"));
            Assert.That(first.Title, Does.Contain("健康"));
            Assert.That(first.Capabilities, Does.Contain("统 78"));
            Assert.That(first.Capabilities, Does.Contain("武 82"));
        }

        [Test]
        public void test_injured_character_shows_injured_label()
        {
            var service = new SessionService();
            var view = new RosterView(service.ProjectRoster(service.NewGame()));

            // 校尉·方武 设为轻伤。
            bool foundInjured = false;
            foreach (var c in view.Characters)
                if (c.Title.Contains("方武") && c.Title.Contains("轻伤")) foundInjured = true;
            Assert.That(foundInjured, Is.True);
        }
    }
}
