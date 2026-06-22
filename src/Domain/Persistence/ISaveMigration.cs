namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 单步逐版迁移（ADR-0005）：把 <see cref="From"/> 版本的快照升级为 <see cref="To"/> 版本。
    /// 实现须为<b>纯函数</b>：只读入参、产出升级后的<b>新</b>快照（不就地修改），且新快照版本 = <see cref="To"/>。
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>迁移起始版本。</summary>
        SaveVersion From { get; }

        /// <summary>迁移目标版本（须高于 <see cref="From"/>）。</summary>
        SaveVersion To { get; }

        /// <summary>对副本应用迁移，返回 <see cref="To"/> 版本的新快照。失败应抛异常（由迁移器捕获，保留原档）。</summary>
        SaveSnapshot Apply(SaveSnapshot snapshot);
    }
}
