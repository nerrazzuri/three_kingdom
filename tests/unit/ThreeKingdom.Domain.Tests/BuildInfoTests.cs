using NUnit.Framework;
using ThreeKingdom.Domain;

namespace ThreeKingdom.Domain.Tests
{
    /// <summary>
    /// 框架确认示例测试（epic-001 story-001）。证明：纯 C# Domain 可在无 Unity 运行时下经 dotnet test 单测，
    /// 测试框架（NUnit）装配可用。待真实 Domain story 落地后由其单元测试取代。
    /// </summary>
    [TestFixture]
    public class BuildInfoTests
    {
        [Test]
        public void DomainMarker_is_stable()
        {
            Assert.That(BuildInfo.DomainMarker, Is.EqualTo("three-kingdom-domain"));
        }

        [Test]
        public void Echo_is_deterministic()
        {
            Assert.That(BuildInfo.Echo("汜水"), Is.EqualTo("汜水"));
            Assert.That(BuildInfo.Echo("汜水"), Is.EqualTo(BuildInfo.Echo("汜水")));
        }
    }
}
