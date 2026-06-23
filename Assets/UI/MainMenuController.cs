using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ThreeKingdom.Presentation.Screens;
using ThreeKingdom.Presentation.Intents;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 主菜单视觉壳控制器（Presentation 薄壳 / ADR-0002）。
    /// <b>只</b>把 <see cref="MainMenuViewModel"/> 的只读状态绑定到 UXML，并把按钮点击翻译为
    /// Application 命令（经 <see cref="IntentTranslator"/>）——<b>不</b>含规则、不直接改核心状态。
    /// ViewModel 逻辑已由 dotnet 测试覆盖（BLOCKING）；本壳的视觉由 Editor 截图签核（ADVISORY）。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuController : MonoBehaviour
    {
        private readonly IntentTranslator _translator = new IntentTranslator();
        private MainMenuViewModel _vm;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // slice 演示：无存档槽（真实槽投影由 Application 注入）。
            _vm = MainMenuViewModel.FromSlot(SaveSlotView.Empty("campaign"));
            Render(root);

            // 应用当前无障碍设置（文本缩放/色盲/减少动态；story-005 横切挂接）。
            AccessibilityApplier.Apply(root, AccessibilityRuntime.Current);

            Wire(root, "new-game", () => StartNewGame(root));
            Wire(root, "continue", () => Continue(root));
            Wire(root, "quit", () =>
            {
                _vm = _vm.RequestQuit();
                Render(root);
            });

            // 竖切：「继续」可用性反映真实存档是否存在（默认槽 campaign）。
            var continueBtn = root.Q<Button>("continue");
            if (continueBtn != null) continueBtn.SetEnabled(SessionRuntime.HasSave());
        }

        /// <summary>
        /// 「继续」端到端竖切：经真实持久栈读档恢复会话 → 进入 HUD；失败显示可行动原因（零部分载入）。
        /// </summary>
        private void Continue(VisualElement root)
        {
            if (!SessionRuntime.HasSave()) return;

            SubmitAndRefresh(root, new LoadGameIntent("campaign")); // 接缝：意图→LoadGameCommand 载荷
            if (SessionRuntime.Load(out string reason))
            {
                SceneManager.LoadScene("Hud"); // 进入 HUD（恢复的真实世界状态）
            }
            else
            {
                var error = root.Q<Label>("error");
                if (error != null) error.text = reason; // 可行动原因（不兼容/损坏/指纹不符）
            }
        }

        private void Render(VisualElement root)
        {
            var continueBtn = root.Q<Button>("continue");
            if (continueBtn != null) continueBtn.SetEnabled(_vm.ContinueAvailable);

            var version = root.Q<Label>("slot-version");
            if (version != null) version.text = _vm.ContinueAvailable ? $"存档版本 {_vm.SlotVersionLabel}" : string.Empty;

            var error = root.Q<Label>("error");
            if (error != null) error.text = _vm.State == MainMenuState.LoadError ? (_vm.ErrorReason ?? string.Empty) : string.Empty;
        }

        /// <summary>
        /// 「新游戏」端到端竖切：意图→命令载荷接缝演示 + 真实 Application 开局 + 进入 HUD 场景。
        /// 进 HUD 后 <see cref="HudController"/> 读 <see cref="SessionRuntime"/> 的真实世界状态投影。
        /// </summary>
        private void StartNewGame(VisualElement root)
        {
            SubmitAndRefresh(root, new NewGameIntent()); // 接缝：意图→StartNewGameCommand 载荷（IntentTranslator）
            SessionRuntime.NewGame();                    // 真实 Application 用例：开局至第 0 日黎明
            SceneManager.LoadScene("Hud");               // 进入 HUD（展示真实世界状态，可推进时段）
        }

        private void SubmitAndRefresh(VisualElement root, IUiIntent intent)
        {
            IApplicationCommand command = _translator.Translate(intent);
            // 薄壳只提交命令载荷给 Application（此处 slice 仅记录，证明接缝连通）。
            Debug.Log($"[MainMenu] 意图 {intent.GetType().Name} → 命令 {command.GetType().Name}");
            Render(root);
        }

        private static void Wire(VisualElement root, string name, System.Action onClick)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += onClick;
        }
    }
}
