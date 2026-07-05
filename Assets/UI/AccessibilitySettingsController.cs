using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Accessibility;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 无障碍设置视觉壳控制器（Presentation 薄壳 / ADR-0002）。把
    /// <see cref="AccessibilitySettingsViewModel"/>（已 dotnet 测试覆盖的不可变变换）绑定到 UXML 控件；
    /// 每次变更即写回 <see cref="AccessibilityRuntime"/>（原子持久）并把设置实时应用到本屏
    /// （文本缩放/色盲/减少动态即时可见，自我演示）。本壳无规则、不碰 gameplay。视觉签核为 ADVISORY。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class AccessibilitySettingsController : MonoBehaviour
    {
        /// <summary>「返回」目标场景（由打开本屏的场景设定；默认回主菜单）。镜像 ZoneBattleSession.ReturnScene。</summary>
        public static string ReturnScene = "MainMenu";

        // 色盲模式中文显示。
        private static readonly System.Collections.Generic.Dictionary<ColorblindMode, string> ColorblindLabels =
            new System.Collections.Generic.Dictionary<ColorblindMode, string>
            {
                [ColorblindMode.None] = "无",
                [ColorblindMode.Protanopia] = "红色盲",
                [ColorblindMode.Deuteranopia] = "绿色盲",
                [ColorblindMode.Tritanopia] = "蓝色盲",
            };

        // HUD 元素中文显示（与 AccessibilityApplier.HudElementNames 同集合）。
        private static readonly System.Collections.Generic.Dictionary<HudElement, string> HudLabels =
            new System.Collections.Generic.Dictionary<HudElement, string>
            {
                [HudElement.TimeBar] = "时辰条",
                [HudElement.OwnLedger] = "己方账本",
                [HudElement.EnemyReport] = "敌情探报",
                [HudElement.AdvisorEntry] = "军师入口",
                [HudElement.CommandTray] = "命令托盘",
                [HudElement.OutcomeChain] = "战果因果链",
            };

        private VisualElement _root;
        private AccessibilitySettingsViewModel _vm;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _vm = AccessibilitySettingsViewModel.Load(AccessibilityRuntime.Store);

            BuildHudToggles();
            Wire("text-scale-cycle", () => Mutate(_vm.CycleTextScale()));
            Wire("colorblind-cycle", () => Mutate(_vm.WithColorblind(NextColorblind(_vm.Colorblind))));

            var reduce = _root.Q<Toggle>("reduce-motion-toggle");
            if (reduce != null) reduce.RegisterValueChangedCallback(_ => Mutate(_vm.ToggleReduceMotion()));

            Wire("back", () => SceneManager.LoadScene(ReturnScene));

            Render();
        }

        private void BuildHudToggles()
        {
            var group = _root.Q<VisualElement>("hud-toggles");
            if (group == null) return;
            group.Clear();
            foreach (var pair in HudLabels)
            {
                HudElement element = pair.Key;
                var toggle = new Toggle(pair.Value) { name = "hud-" + element };
                toggle.RegisterValueChangedCallback(_ => Mutate(_vm.ToggleHudElement(element.ToString())));
                group.Add(toggle);
            }
        }

        private void Mutate(AccessibilitySettingsViewModel next)
        {
            _vm = next;
            SettingsSaveResult result = AccessibilityRuntime.Apply(_vm.Settings); // 原子持久 + 刷新生效值
            ShowStatus(result);
            Render();
        }

        private void Render()
        {
            SetButtonText("text-scale-cycle", _vm.TextScaleLabel);
            SetButtonText("colorblind-cycle", ColorblindLabels[_vm.Colorblind]);

            var reduce = _root.Q<Toggle>("reduce-motion-toggle");
            if (reduce != null) reduce.SetValueWithoutNotify(_vm.ReduceMotion);

            foreach (var element in HudLabels.Keys)
            {
                var toggle = _root.Q<Toggle>("hud-" + element);
                if (toggle != null) toggle.SetValueWithoutNotify(_vm.IsHudElementVisible(element.ToString()));
            }

            // 把设置实时应用到本屏（文本缩放/色盲/减少动态自我演示）。
            AccessibilityApplier.Apply(_root, _vm.Settings);
        }

        private void ShowStatus(SettingsSaveResult result)
        {
            var status = _root.Q<Label>("settings-status");
            if (status == null) return;
            status.text = result.Success ? string.Empty : (result.Reason ?? "保存失败。");
        }

        private static ColorblindMode NextColorblind(ColorblindMode current)
        {
            var options = AccessibilitySettingsViewModel.ColorblindOptions;
            int idx = 0;
            for (int i = 0; i < options.Count; i++)
                if (options[i] == current) { idx = i; break; }
            return options[(idx + 1) % options.Count];
        }

        private void SetButtonText(string name, string text)
        {
            var button = _root.Q<Button>(name);
            if (button != null) button.text = text;
        }

        private void Wire(string name, Action onClick)
        {
            var button = _root.Q<Button>(name);
            if (button != null) button.clicked += onClick;
        }
    }
}
