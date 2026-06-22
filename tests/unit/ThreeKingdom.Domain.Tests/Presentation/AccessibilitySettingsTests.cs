using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Presentation.Accessibility;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-005：无障碍设置 + 多通道冗余（可测逻辑，BLOCKING）。
    /// 治理 ADR：ADR-0002。覆盖 AC-4 设置持久 round-trip + 非法值拒绝、AC-2 信息不靠颜色单一通道、
    /// AC-3 减少动态无信息丢失。
    /// </summary>
    [TestFixture]
    public class AccessibilitySettingsTests
    {
        // ---- AC-4: 设置持久 round-trip ----

        [Test]
        public void test_settings_serialize_parse_round_trip()
        {
            var settings = new AccessibilitySettings(150, ColorblindMode.Deuteranopia, true,
                new Dictionary<string, bool> { ["EnemyReport"] = false, ["TimeBar"] = true });

            var restored = AccessibilitySettings.Parse(settings.Serialize());

            Assert.That(restored.TextScalePercent, Is.EqualTo(150));
            Assert.That(restored.Colorblind, Is.EqualTo(ColorblindMode.Deuteranopia));
            Assert.That(restored.ReduceMotion, Is.True);
            Assert.That(restored.IsHudElementVisible("EnemyReport"), Is.False);
            Assert.That(restored.IsHudElementVisible("TimeBar"), Is.True);
            Assert.That(restored.IsHudElementVisible("Unlisted"), Is.True, "未记录视为可见。");
        }

        [Test]
        public void test_invalid_text_scale_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AccessibilitySettings(80, ColorblindMode.None, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AccessibilitySettings(300, ColorblindMode.None, false));
        }

        [Test]
        public void test_corrupted_settings_text_is_rejected()
        {
            Assert.Throws<FormatException>(() => AccessibilitySettings.Parse("garbage"));
        }

        // ---- AC-2: 信息不靠颜色单一通道 ----

        [Test]
        public void test_distinct_statuses_remain_distinguishable_without_color()
        {
            // 两个状态即使颜色被色盲削弱（设为相同颜色 token），去色签名仍须不同。
            var supplyCut = new StatusChannels("red", "triangle", "断粮");
            var deadline = new StatusChannels("red", "diamond", "期限");

            Assert.That(deadline.NonColorSignature, Is.Not.EqualTo(supplyCut.NonColorSignature),
                "去色后凭形状/文字仍可唯一区分。");
        }

        // ---- AC-3: 减少动态无信息丢失 ----

        [Test]
        public void test_reduce_motion_disables_animation_but_keeps_info()
        {
            var on = new AccessibilitySettings(100, ColorblindMode.None, reduceMotion: true);
            var off = new AccessibilitySettings(100, ColorblindMode.None, reduceMotion: false);

            Assert.That(on.AnimationEnabled, Is.False, "减少动态停用动效。");
            Assert.That(off.AnimationEnabled, Is.True);
            // 信息字段（HUD 可见性/缩放/色盲）不受动效开关影响——无信息丢失。
            Assert.That(on.IsHudElementVisible("OwnLedger"), Is.True);
        }
    }
}
