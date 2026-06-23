using System.Collections.Generic;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Accessibility;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 把 <see cref="AccessibilitySettings"/>（已由 dotnet 测试覆盖的表现层模型）应用到任一屏的 root：
    /// 文本缩放 / 色盲调色 / 减少动态 经 USS class 切换（规则在 <c>SliceTheme.tss</c> 全局生效），
    /// HUD 元素可见性经直接显隐（与情境可见性<b>复合</b>：只<b>额外隐藏</b>用户关闭的元素，绝不强制显示，
    /// 故不会覆盖情境层的隐藏）。薄壳工具，不含 gameplay 规则（ADR-0002）。视觉签核为 ADVISORY。
    /// </summary>
    public static class AccessibilityApplier
    {
        /// <summary>可由用户控制可见性的 HUD 元素 → UXML 元素名（hud.md §10.5）。</summary>
        public static readonly IReadOnlyDictionary<HudElement, string> HudElementNames =
            new Dictionary<HudElement, string>
            {
                [HudElement.TimeBar] = "time-bar",
                [HudElement.OwnLedger] = "own-ledger",
                [HudElement.EnemyReport] = "enemy-report",
                [HudElement.AdvisorEntry] = "advisor-entry",
                [HudElement.CommandTray] = "command-tray",
                [HudElement.OutcomeChain] = "outcome-chain",
            };

        // 切换前需清除的全部互斥 class（避免叠加残留）。
        private static readonly string[] AllTextScaleClasses =
            { "text-scale-100", "text-scale-125", "text-scale-150", "text-scale-175", "text-scale-200" };

        private static readonly string[] AllColorblindClasses =
            { "cb-none", "cb-protanopia", "cb-deuteranopia", "cb-tritanopia" };

        private const string ReduceMotionClass = "reduce-motion";

        /// <summary>把设置应用到给定 root（可重复调用，幂等）。</summary>
        public static void Apply(VisualElement root, AccessibilitySettings settings)
        {
            if (root == null || settings == null) return;

            // 文本缩放：互斥 class，对应 SliceTheme.tss 的 font-size 百分比。
            foreach (var c in AllTextScaleClasses) root.RemoveFromClassList(c);
            root.AddToClassList("text-scale-" + settings.TextScalePercent);

            // 色盲：互斥 class，供调色板/纹样冗余（冗余通道见 StatusChannels）。
            foreach (var c in AllColorblindClasses) root.RemoveFromClassList(c);
            root.AddToClassList("cb-" + settings.Colorblind.ToString().ToLowerInvariant());

            // 减少动态：class 关闭 USS transition；信息字段不依赖动效（无信息丢失）。
            root.EnableInClassList(ReduceMotionClass, settings.ReduceMotion);

            // HUD 可见性：只额外隐藏用户关闭的元素（与情境层复合，不强制显示）。
            foreach (var pair in HudElementNames)
            {
                var element = root.Q<VisualElement>(pair.Value);
                if (element == null) continue; // 非 HUD 屏无此元素，跳过
                if (!settings.IsHudElementVisible(pair.Key.ToString()))
                    element.style.display = DisplayStyle.None;
            }
        }
    }
}
