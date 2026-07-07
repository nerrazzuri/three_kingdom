using System;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>存档硬化（A3）：伴生槽写失败不静默吞（Save 返 false）；读坏档各自降级为空不牵连主档（robust）。</summary>
    [TestFixture]
    public class SaveHardeningTests
    {
        private static CharacterId C(string id) => new CharacterId(id);

        /// <summary>伴生槽写必失败的介质（主档正常）。</summary>
        private sealed class CompanionFailMedium : ISaveMedium
        {
            private readonly ISaveMedium _inner;
            public CompanionFailMedium(ISaveMedium inner) { _inner = inner; }
            public bool Exists(string name) => _inner.Exists(name);
            public string? Read(string name) => _inner.Read(name);
            public void Move(string from, string to) => _inner.Move(from, to);
            public void Delete(string name) => _inner.Delete(name);
            public void Write(string name, string content)
            {
                if (name.Contains(".generals") || name.Contains(".talents") || name.Contains(".appoint"))
                    throw new InvalidOperationException("伴生槽写失败（注入）。");
                _inner.Write(name, content);
            }
        }

        [Test]
        public void test_save_returns_false_when_companion_slot_write_fails()
        {
            // Arrange
            var inner = new InMemorySaveMedium();
            var rt = new CampaignRuntime(new CompanionFailMedium(inner), slot: "sv");
            rt.NewGame();

            // Act
            bool ok = rt.Save();

            // Assert：伴生槽失败 → Save 报未完全存档（不再静默 true）；主档仍在。
            Assert.That(ok, Is.False, "伴生槽写失败 → Save 返回 false。");
            Assert.That(inner.Exists("sv"), Is.True, "主存档已就位（伴生失败不损主档）。");
        }

        [Test]
        public void test_load_degrades_gracefully_on_corrupt_companion_blob()
        {
            // Arrange：正常存一份带知晓态的档。
            var medium = new InMemorySaveMedium();
            var rt1 = new CampaignRuntime(medium, slot: "sv");
            rt1.NewGame();
            rt1.DiscoverTalent(C("char-simahui"), RecruitChannel.Visit);
            Assert.That(rt1.Save(), Is.True, "正常存档成功。");

            // 破坏知晓簿伴生槽（半写/损坏）。
            medium.Write("sv.talents", "GARBAGE\nnot-a-number\nrubbish");

            // Act：另起运行期载入。
            var rt2 = new CampaignRuntime(medium, slot: "sv");
            bool loaded = rt2.Load(out string reason);

            // Assert：主档载入成功；坏的知晓簿降级为空，不崩、不牵连主档。
            Assert.That(loaded, Is.True, $"坏伴生槽不牵连主档载入（reason: {reason}）。");
            Assert.That(rt2.Talents.Entries.Count, Is.EqualTo(0), "坏知晓簿降级为空。");
        }
    }
}
