namespace ThreeKingdom.Presentation.Accessibility
{
    /// <summary>
    /// 无障碍设置介质端口（ADR-0002）：抽象命名键的读写与<b>原子改名</b>原语。
    /// 与存档介质（<c>ISaveMedium</c>）<b>分离</b>——设置不是 gameplay 权威状态，独立持久。
    /// 量产期由 Unity 侧以 PlayerPrefs/文件系统实现；原子写回的<b>编排</b>由
    /// <see cref="SettingsStore"/> 实现，可纯内存单测。
    /// </summary>
    public interface ISettingsMedium
    {
        /// <summary>正式键是否已存在内容。</summary>
        bool Exists(string key);

        /// <summary>读取键内容；不存在返回 null。</summary>
        string? Read(string key);

        /// <summary>写入一个键（可能失败抛异常；正式键的不破坏性由编排保证）。</summary>
        void Write(string key, string content);

        /// <summary>原子改名 from→to（覆盖 to）。契约：要么完全成功，要么 to 保持原值（不产生半写）。</summary>
        void Move(string from, string to);

        /// <summary>删除一个键（清理临时键用；不存在则无操作）。</summary>
        void Delete(string key);
    }
}
