using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using ThreeKingdom.Unity.UI;

namespace ThreeKingdom.Unity.EditorTools
{
    /// <summary>
    /// 程序化生成 slice 三屏 Scene + 共享 PanelSettings（仅 Editor）。
    /// 让 Unity 用正确 API 写场景/资源 YAML（避免手写 GUID 出错）。
    /// 每个场景挂 UIDocument（指向对应 UXML + 共享 PanelSettings）+ 对应 Controller，
    /// 进 Play 即渲染该屏。视觉/交互签核为 ADVISORY（须 graphics 模式 Editor）。
    /// </summary>
    public static class SliceSceneBuilder
    {
        private const string PanelSettingsPath = "Assets/UI/SlicePanelSettings.asset";
        private const string ThemePath = "Assets/UI/SliceTheme.tss";
        private const string SceneDir = "Assets/Scenes";

        private struct ScreenDef
        {
            public string Name;
            public string Uxml;
            public Type Controller;
        }

        private static readonly ScreenDef[] Screens =
        {
            new ScreenDef { Name = "MainMenu", Uxml = "Assets/UI/MainMenu.uxml", Controller = typeof(MainMenuController) },
            new ScreenDef { Name = "Hud", Uxml = "Assets/UI/Hud.uxml", Controller = typeof(HudController) },
            new ScreenDef { Name = "PauseMenu", Uxml = "Assets/UI/PauseMenu.uxml", Controller = typeof(PauseMenuController) },
            // story-005 无障碍设置面板：自我演示屏（改设置即时应用文本缩放/色盲/减少动态到本屏）。
            new ScreenDef { Name = "AccessibilitySettings", Uxml = "Assets/UI/AccessibilitySettings.uxml", Controller = typeof(AccessibilitySettingsController) },
        };

        /// <summary>菜单与 batchmode -executeMethod 共用入口。</summary>
        [MenuItem("三国/构建 Slice 场景")]
        public static void BuildAll()
        {
            EnsurePanelSettings(); // 确保资源存在；引用在循环内逐场景重新加载（见下）
            Directory.CreateDirectory(SceneDir);

            var scenePaths = new List<string>();
            foreach (var screen in Screens)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                // PanelSettings 引用须在每个新场景内重新加载：NewScene(Single) 会卸载场景域，
                // 循环外捕获的同一引用在后续迭代可能失效（曾致末屏 m_PanelSettings=None 不渲染）。
                var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
                if (panel == null)
                    throw new InvalidOperationException($"PanelSettings 缺失：{PanelSettingsPath}（{screen.Name} 将无法渲染）。");

                var go = new GameObject("UI");
                var doc = go.AddComponent<UIDocument>();
                doc.panelSettings = panel;
                doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(screen.Uxml);
                go.AddComponent(screen.Controller);

                // UI Toolkit 运行时输入需要 EventSystem（旧 Input Manager → StandaloneInputModule）。
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

                string path = $"{SceneDir}/{screen.Name}.unity";
                EditorSceneManager.SaveScene(scene, path);
                scenePaths.Add(path);
                Debug.Log($"[SliceSceneBuilder] 已生成场景 {path}");
            }

            // 写入 Build Settings（MainMenu 为首场景）。
            var buildScenes = new EditorBuildSettingsScene[scenePaths.Count];
            for (int i = 0; i < scenePaths.Count; i++)
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SliceSceneBuilder] 完成：{scenePaths.Count} 场景 + PanelSettings + Build Settings。");
        }

        private static PanelSettings EnsurePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }

            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (theme != null) panel.themeStyleSheet = theme;
            else Debug.LogWarning("[SliceSceneBuilder] 未找到 SliceTheme.tss，PanelSettings 暂无主题（控件用内置后备外观）。");

            EditorUtility.SetDirty(panel);
            return panel;
        }
    }
}
