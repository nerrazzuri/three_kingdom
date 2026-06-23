using UnityEngine;
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

            Wire(root, "new-game", () => SubmitAndRefresh(root, new NewGameIntent()));
            Wire(root, "continue", () => SubmitAndRefresh(root, new LoadGameIntent("campaign")));
            Wire(root, "quit", () =>
            {
                _vm = _vm.RequestQuit();
                Render(root);
            });
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
