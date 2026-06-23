using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Presentation.Accessibility;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-005：无障碍设置持久（可测逻辑，BLOCKING）。治理 ADR：ADR-0002 + ADR-0005 原子写模式。
    /// 覆盖 AC-4 跨会话 round-trip、原子写回（写失败保留旧）、损坏/缺失优雅回落默认。
    /// </summary>
    [TestFixture]
    public class AccessibilitySettingsStoreTests
    {
        /// <summary>纯内存设置介质（可注入写故障）：模拟原子改名原语。</summary>
        private sealed class InMemorySettingsMedium : ISettingsMedium
        {
            private readonly Dictionary<string, string> _data = new Dictionary<string, string>(StringComparer.Ordinal);

            /// <summary>下一次 Write 抛异常（模拟磁盘写满）。</summary>
            public bool FailNextWrite { get; set; }

            public bool Exists(string key) => _data.ContainsKey(key);

            public string? Read(string key) => _data.TryGetValue(key, out var v) ? v : null;

            public void Write(string key, string content)
            {
                if (FailNextWrite) { FailNextWrite = false; throw new InvalidOperationException("模拟写失败。"); }
                _data[key] = content;
            }

            public void Move(string from, string to)
            {
                if (!_data.TryGetValue(from, out var v)) throw new InvalidOperationException("源键不存在。");
                _data[to] = v;
                _data.Remove(from);
            }

            public void Delete(string key) => _data.Remove(key);

            /// <summary>直接植入正式键内容（绕过编排，用于构造损坏数据）。</summary>
            public void Seed(string key, string content) => _data[key] = content;
        }

        // ---- AC-4: 跨会话 round-trip ----

        [Test]
        public void test_settings_store_save_then_load_round_trips_all_fields()
        {
            var medium = new InMemorySettingsMedium();
            var store = new SettingsStore(medium);
            var settings = new AccessibilitySettings(150, ColorblindMode.Tritanopia, true,
                new Dictionary<string, bool> { ["EnemyReport"] = false });

            var saveResult = store.Save(settings);
            var loadResult = store.Load();

            Assert.That(saveResult.Success, Is.True);
            Assert.That(loadResult.WasReset, Is.False, "存在持久值时不应回落默认。");
            Assert.That(loadResult.Settings.TextScalePercent, Is.EqualTo(150));
            Assert.That(loadResult.Settings.Colorblind, Is.EqualTo(ColorblindMode.Tritanopia));
            Assert.That(loadResult.Settings.ReduceMotion, Is.True);
            Assert.That(loadResult.Settings.IsHudElementVisible("EnemyReport"), Is.False);
        }

        // ---- 缺失 → 回落默认 ----

        [Test]
        public void test_load_missing_settings_falls_back_to_default()
        {
            var store = new SettingsStore(new InMemorySettingsMedium());

            var result = store.Load();

            Assert.That(result.WasReset, Is.True);
            Assert.That(result.Reason, Is.Not.Null);
            Assert.That(result.Settings.TextScalePercent, Is.EqualTo(AccessibilitySettings.MinTextScale));
            Assert.That(result.Settings.Colorblind, Is.EqualTo(ColorblindMode.None));
        }

        // ---- 损坏 → 回落默认（不抛、不砸档）----

        [Test]
        public void test_load_corrupted_settings_falls_back_to_default_without_throwing()
        {
            var medium = new InMemorySettingsMedium();
            medium.Seed(SettingsStore.DefaultKey, "完全不是设置文本");
            var store = new SettingsStore(medium);

            var result = store.Load();

            Assert.That(result.WasReset, Is.True, "损坏的设置文件应回落默认而非抛异常。");
            Assert.That(result.Settings.TextScalePercent, Is.EqualTo(AccessibilitySettings.MinTextScale));
        }

        // ---- 原子写回：写失败保留上一份有效设置 ----

        [Test]
        public void test_save_failure_preserves_previous_valid_settings()
        {
            var medium = new InMemorySettingsMedium();
            var store = new SettingsStore(medium);
            store.Save(new AccessibilitySettings(125, ColorblindMode.None, false)); // 既有有效设置

            medium.FailNextWrite = true;
            var failed = store.Save(new AccessibilitySettings(200, ColorblindMode.Protanopia, true));

            Assert.That(failed.Success, Is.False);
            Assert.That(failed.Reason, Is.Not.Null);
            // 正式键未被破坏——仍是上一份有效设置。
            var reloaded = store.Load();
            Assert.That(reloaded.WasReset, Is.False);
            Assert.That(reloaded.Settings.TextScalePercent, Is.EqualTo(125));
            Assert.That(reloaded.Settings.Colorblind, Is.EqualTo(ColorblindMode.None));
            // 临时键已清理（不残留半写）。
            Assert.That(medium.Exists(SettingsStore.DefaultKey + ".tmp"), Is.False);
        }
    }
}
