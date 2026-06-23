using UnityEngine;
using ThreeKingdom.Presentation.Accessibility;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// <see cref="ISettingsMedium"/> 的 Unity 侧实现，以 <see cref="PlayerPrefs"/> 为后端
    /// （与存档分离：无障碍设置不是 gameplay 权威状态）。原子改名经键值搬移 + 即时 <c>Save</c>。
    /// 原子写回的<b>编排</b>仍在纯 C# 的 <see cref="SettingsStore"/>（已单测），本类只是介质适配。
    /// </summary>
    public sealed class PlayerPrefsSettingsMedium : ISettingsMedium
    {
        private const string Prefix = "tk.settings.";

        public bool Exists(string key) => PlayerPrefs.HasKey(Prefix + key);

        public string Read(string key) => PlayerPrefs.HasKey(Prefix + key) ? PlayerPrefs.GetString(Prefix + key) : null;

        public void Write(string key, string content)
        {
            PlayerPrefs.SetString(Prefix + key, content);
            PlayerPrefs.Save();
        }

        public void Move(string from, string to)
        {
            string value = PlayerPrefs.GetString(Prefix + from);
            PlayerPrefs.SetString(Prefix + to, value);
            PlayerPrefs.DeleteKey(Prefix + from);
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(Prefix + key);
            PlayerPrefs.Save();
        }
    }
}
