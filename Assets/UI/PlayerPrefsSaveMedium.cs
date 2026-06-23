using UnityEngine;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// <see cref="ISaveMedium"/> 的 Unity 侧实现，以 <see cref="PlayerPrefs"/> 为后端
    /// （slice：跨 Play 会话持久存档；量产期 Infrastructure 改文件系统 + 原子 rename）。
    /// 原子写回的<b>编排</b>仍在纯 C# 的 <c>SaveRepository</c>（已单测）；本类只是介质适配。
    /// 原子改名经键值搬移 + 即时 <c>Save</c>。
    /// </summary>
    public sealed class PlayerPrefsSaveMedium : ISaveMedium
    {
        private const string Prefix = "tk.save.";

        public bool Exists(string name) => PlayerPrefs.HasKey(Prefix + name);

        public string Read(string name) => PlayerPrefs.HasKey(Prefix + name) ? PlayerPrefs.GetString(Prefix + name) : null;

        public void Write(string name, string content)
        {
            PlayerPrefs.SetString(Prefix + name, content);
            PlayerPrefs.Save();
        }

        public void Move(string from, string to)
        {
            string value = PlayerPrefs.GetString(Prefix + from);
            PlayerPrefs.SetString(Prefix + to, value);
            PlayerPrefs.DeleteKey(Prefix + from);
            PlayerPrefs.Save();
        }

        public void Delete(string name)
        {
            PlayerPrefs.DeleteKey(Prefix + name);
            PlayerPrefs.Save();
        }
    }
}
