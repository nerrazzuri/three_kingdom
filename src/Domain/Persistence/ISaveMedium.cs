namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 存档介质端口（ADR-0002 + ADR-0005）：抽象命名槽的读写与<b>原子改名</b>原语。
    /// 量产期由 Infrastructure 以文件系统实现（临时文件 + 原子 rename）；Domain 仅依赖此接口，
    /// 原子写回的<b>编排</b>（先写临时槽、再原子改名到正式槽）由 <see cref="SaveRepository"/> 实现，可纯内存单测。
    /// </summary>
    public interface ISaveMedium
    {
        /// <summary>正式槽是否已存在有效内容。</summary>
        bool Exists(string name);

        /// <summary>读取槽内容；不存在返回 null。</summary>
        string? Read(string name);

        /// <summary>写入一个槽（可能失败抛异常，如磁盘写满；正式槽的不破坏性由编排保证）。</summary>
        void Write(string name, string content);

        /// <summary>原子改名 from→to（覆盖 to）。契约：要么完全成功，要么 to 保持原值（不产生半写）。</summary>
        void Move(string from, string to);

        /// <summary>删除一个槽（清理临时文件用；不存在则无操作）。</summary>
        void Delete(string name);
    }
}
