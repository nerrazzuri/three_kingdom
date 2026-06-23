using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThreeKingdom.Unity.EditorTools
{
    /// <summary>
    /// 表现层 UXML/USS 预览窗（仅 Editor）。供视觉/无障碍签核（ADVISORY）——
    /// 无需进入 Play 模式或搭 Scene 即可在 Editor 内加载三屏视觉壳并截图。
    /// 不参与运行时；不依赖 Domain 逻辑（逻辑已由 dotnet 测试覆盖 BLOCKING）。
    /// </summary>
    public sealed class UxmlPreviewWindow : EditorWindow
    {
        private static readonly string[] Screens = { "MainMenu", "Hud", "PauseMenu" };
        private int _selected;

        [MenuItem("三国/UXML 视觉壳预览")]
        public static void Open()
        {
            var window = GetWindow<UxmlPreviewWindow>();
            window.titleContent = new GUIContent("UXML 预览");
            window.minSize = new Vector2(640, 480);
            window.Reload();
        }

        private void CreateGUI() => Reload();

        private void Reload()
        {
            rootVisualElement.Clear();

            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 28 } };
            foreach (var screen in Screens)
            {
                string captured = screen;
                var button = new Button(() => { _selected = System.Array.IndexOf(Screens, captured); LoadScreen(); }) { text = screen };
                toolbar.Add(button);
            }
            rootVisualElement.Add(toolbar);

            var host = new VisualElement { name = "preview-host", style = { flexGrow = 1 } };
            rootVisualElement.Add(host);
            LoadScreen();
        }

        private void LoadScreen()
        {
            var host = rootVisualElement.Q<VisualElement>("preview-host");
            if (host == null) return;
            host.Clear();

            string name = Screens[Mathf.Clamp(_selected, 0, Screens.Length - 1)];
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/UI/{name}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"Assets/UI/{name}.uss");

            if (tree == null)
            {
                host.Add(new Label($"未找到 Assets/UI/{name}.uxml"));
                return;
            }

            tree.CloneTree(host);
            if (style != null) host.styleSheets.Add(style);
        }
    }
}
