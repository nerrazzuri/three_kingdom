using System;
using System.Collections.Generic;
using System.Globalization;

namespace ThreeKingdom.Presentation.Accessibility
{
    /// <summary>
    /// 无障碍设置面板的绑定逻辑（ADR-0002，纯 C#）。包裹不可变 <see cref="AccessibilitySettings"/>，
    /// 提供<b>不可变 with 变换</b>（每次返回新 VM，不在原对象上突变），供 UXML 薄壳绑定控件。
    /// 持久经 <see cref="ISettingsStore"/>（与存档分离）。本类逻辑由 dotnet 测试覆盖（BLOCKING）；
    /// 视觉外壳由 Editor 截图签核（ADVISORY）。
    /// </summary>
    public sealed class AccessibilitySettingsViewModel
    {
        /// <summary>文本缩放离散档位（%），循环切换用。</summary>
        public static readonly IReadOnlyList<int> TextScaleSteps = new[] { 100, 125, 150, 175, 200 };

        /// <summary>可选色盲模式（含「无」），下拉绑定用。</summary>
        public static readonly IReadOnlyList<ColorblindMode> ColorblindOptions = new[]
        {
            ColorblindMode.None,
            ColorblindMode.Protanopia,
            ColorblindMode.Deuteranopia,
            ColorblindMode.Tritanopia,
        };

        /// <summary>当前设置（只读快照）。</summary>
        public AccessibilitySettings Settings { get; }

        public AccessibilitySettingsViewModel(AccessibilitySettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>以默认设置起始。</summary>
        public static AccessibilitySettingsViewModel FromDefault()
            => new AccessibilitySettingsViewModel(AccessibilitySettings.Default);

        /// <summary>经 store 加载起始（缺失/损坏回落默认）。</summary>
        public static AccessibilitySettingsViewModel Load(ISettingsStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            return new AccessibilitySettingsViewModel(store.Load().Settings);
        }

        // ---- 绑定用展示字段 ----

        /// <summary>文本缩放展示文字（如「150%」）。</summary>
        public string TextScaleLabel => Settings.TextScalePercent.ToString(CultureInfo.InvariantCulture) + "%";

        /// <summary>当前色盲模式。</summary>
        public ColorblindMode Colorblind => Settings.Colorblind;

        /// <summary>减少动态是否开启。</summary>
        public bool ReduceMotion => Settings.ReduceMotion;

        /// <summary>某 HUD 元素当前是否可见（未记录视为可见）。</summary>
        public bool IsHudElementVisible(string element) => Settings.IsHudElementVisible(element);

        // ---- 不可变变换（返回新 VM）----

        /// <summary>循环切到下一个文本缩放档位（到顶回到最小档）。</summary>
        public AccessibilitySettingsViewModel CycleTextScale()
        {
            int idx = IndexOfNearestStep(Settings.TextScalePercent);
            int next = TextScaleSteps[(idx + 1) % TextScaleSteps.Count];
            return WithTextScale(next);
        }

        /// <summary>设为指定文本缩放（越界由 <see cref="AccessibilitySettings"/> 构造校验拒绝）。</summary>
        public AccessibilitySettingsViewModel WithTextScale(int percent)
            => Rebuild(percent, Settings.Colorblind, Settings.ReduceMotion, Settings.HudVisibility);

        /// <summary>设为指定色盲模式。</summary>
        public AccessibilitySettingsViewModel WithColorblind(ColorblindMode mode)
            => Rebuild(Settings.TextScalePercent, mode, Settings.ReduceMotion, Settings.HudVisibility);

        /// <summary>翻转减少动态开关。</summary>
        public AccessibilitySettingsViewModel ToggleReduceMotion()
            => Rebuild(Settings.TextScalePercent, Settings.Colorblind, !Settings.ReduceMotion, Settings.HudVisibility);

        /// <summary>翻转某 HUD 元素可见性（未记录默认可见 → 翻为隐藏）。</summary>
        public AccessibilitySettingsViewModel ToggleHudElement(string element)
        {
            if (string.IsNullOrWhiteSpace(element)) throw new ArgumentException("元素键不可为空。", nameof(element));
            var map = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var kv in Settings.HudVisibility) map[kv.Key] = kv.Value;
            map[element] = !Settings.IsHudElementVisible(element);
            return Rebuild(Settings.TextScalePercent, Settings.Colorblind, Settings.ReduceMotion, map);
        }

        // ---- 持久 ----

        /// <summary>把当前设置写入 store（原子；失败由结果稳定上报）。</summary>
        public SettingsSaveResult Persist(ISettingsStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            return store.Save(Settings);
        }

        private static int IndexOfNearestStep(int percent)
        {
            // 容忍持久值落在档位之间：取「不小于当前值」的首档；超过最大档则视作最后档（下一步回环到最小）。
            for (int i = 0; i < TextScaleSteps.Count; i++)
                if (TextScaleSteps[i] >= percent) return i;
            return TextScaleSteps.Count - 1;
        }

        private static AccessibilitySettingsViewModel Rebuild(
            int percent, ColorblindMode mode, bool reduceMotion, IReadOnlyDictionary<string, bool> hud)
            => new AccessibilitySettingsViewModel(new AccessibilitySettings(percent, mode, reduceMotion, hud));
    }
}
