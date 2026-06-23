using ThreeKingdom.Presentation.Accessibility;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// slice 运行期当前无障碍设置的单一来源（三屏共读）。首次访问从 <see cref="ISettingsStore"/> 加载
    /// （缺失/损坏回落默认）；设置面板更新即写回并刷新 <see cref="Current"/>，使后续打开的屏读到新值。
    /// 纯进程内状态（非 gameplay 权威），无 MonoBehaviour 依赖。
    /// </summary>
    public static class AccessibilityRuntime
    {
        private static ISettingsStore _store;
        private static AccessibilitySettings _current;

        /// <summary>注入的持久 store（默认 PlayerPrefs 介质）。</summary>
        public static ISettingsStore Store
        {
            get => _store ??= new SettingsStore(new PlayerPrefsSettingsMedium());
            set { _store = value; _current = null; }
        }

        /// <summary>当前生效设置（首访从 store 加载）。</summary>
        public static AccessibilitySettings Current => _current ??= Store.Load().Settings;

        /// <summary>更新并持久（面板提交用）；返回保存结果供 UI 显示成败。</summary>
        public static SettingsSaveResult Apply(AccessibilitySettings settings)
        {
            var result = Store.Save(settings);
            if (result.Success) _current = settings; // 仅成功才切换生效值
            return result;
        }
    }
}
