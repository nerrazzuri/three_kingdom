using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 暂停菜单视觉壳控制器（Presentation 薄壳 / ADR-0002）。
    /// 绑定 <see cref="PauseMenuViewModel"/>：保存经端口、失败显示可行动原因；有草稿时退出/读取先经 P9。
    /// 状态机逻辑已由 dotnet 测试覆盖（BLOCKING）；本壳的暗化/静止/键鼠由 Editor 验证（ADVISORY）。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private bool _hasDraft;
        private PauseMenuViewModel _vm;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _vm = PauseMenuViewModel.Open(_hasDraft);
            Render(root);

            // 无障碍横切挂接（story-005）：文本缩放/色盲/减少动态。
            AccessibilityApplier.Apply(root, AccessibilityRuntime.Current);

            // 继续游戏 / 设置：场景流导航（暂停菜单为独立场景）。
            Wire(root, "resume", () => SceneManager.LoadScene("Hud"));
            Wire(root, "settings", () =>
            {
                AccessibilitySettingsController.ReturnScene = "PauseMenu";  // 返回回到暂停菜单
                SceneManager.LoadScene("AccessibilitySettings");
            });

            Wire(root, "quit", () =>
            {
                _vm = _vm.RequestExitOrLoad();
                Render(root);
                // 无草稿阻挡则退出到主菜单（有草稿时先由 draft-prompt 提示处置，不导航）。
                if (!_vm.RequiresDraftDisposition) SceneManager.LoadScene("MainMenu");
            });
            Wire(root, "load", () =>
            {
                _vm = _vm.RequestExitOrLoad();
                Render(root);
            });
        }

        private void Render(VisualElement root)
        {
            var draft = root.Q<Label>("draft-prompt");
            if (draft != null)
                draft.text = _vm.RequiresDraftDisposition ? "有未提交的草稿：保留或丢弃后再继续。" : string.Empty;

            var error = root.Q<Label>("save-error");
            if (error != null)
                error.text = _vm.State == PauseMenuState.SaveError ? (_vm.ErrorReason ?? string.Empty) : string.Empty;
        }

        private static void Wire(VisualElement root, string name, System.Action onClick)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += onClick;
        }
    }
}
