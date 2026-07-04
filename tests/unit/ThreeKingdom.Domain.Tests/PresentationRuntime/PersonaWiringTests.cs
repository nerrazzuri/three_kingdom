using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Domain.World;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 主角人设接入运行期（GDD_015 事件分级）：开局赋人设 + 展示视图；人设由会话 id 确定性派生，
    /// 存读档一致（无需新存档字段）。给天下事件"心里话"着色，纯丰富体验。
    /// </summary>
    [TestFixture]
    public class PersonaWiringTests
    {
        private InMemorySaveMedium _medium = null!;
        private CampaignRuntime _runtime = null!;

        [SetUp]
        public void SetUp()
        {
            _medium = new InMemorySaveMedium();
            _runtime = new CampaignRuntime(_medium);
        }

        [Test]
        public void test_persona_assigned_with_display_view()
        {
            _runtime.NewGame();
            PersonaView v = _runtime.PersonaView();
            Assert.That(Enum.IsDefined(typeof(ProtagonistPersona), v.Persona), Is.True, "赋予合法人设。");
            Assert.That(v.Name, Is.Not.Empty, "有中文名。");
            Assert.That(v.Description, Is.Not.Empty, "有性情描述。");
        }

        [Test]
        public void test_persona_stable_across_save_reload()
        {
            _runtime.NewGame();
            ProtagonistPersona before = _runtime.Persona;
            Assert.That(_runtime.Save(), Is.True);

            var reloaded = new CampaignRuntime(_medium);
            Assert.That(reloaded.Load(out _), Is.True);
            Assert.That(reloaded.Persona, Is.EqualTo(before), "人设由会话 id 派生 → 存读档一致。");
        }
    }
}
