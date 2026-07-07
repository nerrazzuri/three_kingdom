using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>E4.3 武将任命 AI（ADR-0013）：据城册按标签推荐内政/守将/军师/先锋——AI 与玩家共用"会用人"逻辑。</summary>
    [TestFixture]
    public class GeneralAssignmentServiceTests
    {
        private static CharacterId C(string id) => new CharacterId(id);

        [Test]
        public void test_populated_city_fills_roles_without_double_use()
        {
            // 小沛（刘备190）有关羽张飞等 → 各职有人。
            CityAssignment a = GeneralAssignmentService.Recommend(new CityId("city-xiaopei"), 190);
            Assert.That(a.DefenderLead.HasValue, Is.True, "有守将。");
            Assert.That(a.Vanguard.HasValue, Is.True, "有先锋。");
            Assert.That(a.Governor.HasValue, Is.True, "有内政官。");
            Assert.That(a.Advisor.HasValue, Is.True, "有军师。");
            Assert.That(a.DefenderLead!.Value.Value, Is.Not.EqualTo(a.Vanguard!.Value.Value), "守将与先锋不重复用人。");
        }

        [Test]
        public void test_empty_city_yields_no_assignment()
        {
            CityAssignment a = GeneralAssignmentService.Recommend(new CityId("city-does-not-exist"), 190);
            Assert.That(a.DefenderLead.HasValue, Is.False, "空城无守将。");
            Assert.That(a.Vanguard.HasValue, Is.False);
        }

        [Test]
        public void test_deterministic()
        {
            var a = GeneralAssignmentService.Recommend(new CityId("city-ye"), 190);
            var b = GeneralAssignmentService.Recommend(new CityId("city-ye"), 190);
            Assert.That(a.DefenderLead?.Value, Is.EqualTo(b.DefenderLead?.Value), "任命推荐确定性。");
        }
    }
}
