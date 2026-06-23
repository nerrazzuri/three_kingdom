using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Presentation.Accessibility;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-005：无障碍设置面板绑定逻辑（可测逻辑，BLOCKING）。治理 ADR：ADR-0002。
    /// 覆盖文本缩放循环档位、各开关不可变变换、色盲设定、persist→load 一致。
    /// </summary>
    [TestFixture]
    public class AccessibilitySettingsViewModelTests
    {
        private sealed class InMemorySettingsMedium : ISettingsMedium
        {
            private readonly Dictionary<string, string> _data = new Dictionary<string, string>(StringComparer.Ordinal);
            public bool Exists(string key) => _data.ContainsKey(key);
            public string? Read(string key) => _data.TryGetValue(key, out var v) ? v : null;
            public void Write(string key, string content) => _data[key] = content;
            public void Move(string from, string to) { _data[to] = _data[from]; _data.Remove(from); }
            public void Delete(string key) => _data.Remove(key);
        }

        [Test]
        public void test_cycle_text_scale_steps_through_and_wraps_to_minimum()
        {
            var vm = AccessibilitySettingsViewModel.FromDefault();
            Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(100));

            vm = vm.CycleTextScale(); Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(125));
            vm = vm.CycleTextScale(); Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(150));
            vm = vm.CycleTextScale(); Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(175));
            vm = vm.CycleTextScale(); Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(200));
            vm = vm.CycleTextScale(); Assert.That(vm.Settings.TextScalePercent, Is.EqualTo(100), "到顶应回环到最小档。");
        }

        [Test]
        public void test_transforms_are_immutable_and_leave_source_unchanged()
        {
            var original = AccessibilitySettingsViewModel.FromDefault();

            var mutated = original.ToggleReduceMotion();

            Assert.That(original.ReduceMotion, Is.False, "原 VM 不应被突变。");
            Assert.That(mutated.ReduceMotion, Is.True);
        }

        [Test]
        public void test_with_colorblind_sets_mode()
        {
            var vm = AccessibilitySettingsViewModel.FromDefault().WithColorblind(ColorblindMode.Deuteranopia);

            Assert.That(vm.Colorblind, Is.EqualTo(ColorblindMode.Deuteranopia));
        }

        [Test]
        public void test_toggle_hud_element_flips_default_visible_to_hidden()
        {
            var vm = AccessibilitySettingsViewModel.FromDefault();
            Assert.That(vm.IsHudElementVisible("EnemyReport"), Is.True, "未记录默认可见。");

            var toggled = vm.ToggleHudElement("EnemyReport");

            Assert.That(toggled.IsHudElementVisible("EnemyReport"), Is.False);
            Assert.That(toggled.ToggleHudElement("EnemyReport").IsHudElementVisible("EnemyReport"), Is.True, "再翻回可见。");
        }

        [Test]
        public void test_persist_then_load_round_trips_through_store()
        {
            var store = new SettingsStore(new InMemorySettingsMedium());
            var edited = AccessibilitySettingsViewModel.FromDefault()
                .WithTextScale(175)
                .WithColorblind(ColorblindMode.Protanopia)
                .ToggleReduceMotion()
                .ToggleHudElement("TimeBar");

            var saveResult = edited.Persist(store);
            var reloaded = AccessibilitySettingsViewModel.Load(store);

            Assert.That(saveResult.Success, Is.True);
            Assert.That(reloaded.Settings.TextScalePercent, Is.EqualTo(175));
            Assert.That(reloaded.Colorblind, Is.EqualTo(ColorblindMode.Protanopia));
            Assert.That(reloaded.ReduceMotion, Is.True);
            Assert.That(reloaded.IsHudElementVisible("TimeBar"), Is.False);
        }
    }
}
