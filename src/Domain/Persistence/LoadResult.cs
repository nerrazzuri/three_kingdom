namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>加载校验错误码（稳定枚举，供 main-menu/pause 错误态显示可行动原因，TR-save-003）。</summary>
    public enum LoadErrorCode
    {
        /// <summary>无错误。</summary>
        None = 0,

        /// <summary>槽为空（无存档）。</summary>
        SlotEmpty = 1,

        /// <summary>存档文本损坏/截断/字段非法（含必需分段缺失）。</summary>
        Corrupted = 2,

        /// <summary>存档版本高于当前，不兼容（不静默降级、不部分载入）。</summary>
        IncompatibleNewer = 3,

        /// <summary>配置指纹与当前不符（配置已变，加载不安全）。</summary>
        FingerprintMismatch = 4,

        /// <summary>迁移链无法把旧档升级到当前版本。</summary>
        MigrationFailed = 5,
    }

    /// <summary>
    /// 加载结果（TR-save-003）。失败时 <see cref="Snapshot"/> 为 null 且<b>不</b>载入当前会话（零部分载入）；
    /// <see cref="Reason"/> 给出可行动原因。不可变。
    /// </summary>
    public sealed class LoadResult
    {
        /// <summary>是否加载成功。</summary>
        public bool Succeeded { get; }

        /// <summary>已校验（必要时已迁移）的快照（成功非空）。</summary>
        public SaveSnapshot? Snapshot { get; }

        /// <summary>错误码。</summary>
        public LoadErrorCode Error { get; }

        /// <summary>可行动原因（供 UI 错误态）。</summary>
        public string Reason { get; }

        private LoadResult(bool ok, SaveSnapshot? snapshot, LoadErrorCode error, string reason)
        {
            Succeeded = ok;
            Snapshot = snapshot;
            Error = error;
            Reason = reason ?? string.Empty;
        }

        internal static LoadResult Success(SaveSnapshot snapshot) => new LoadResult(true, snapshot, LoadErrorCode.None, string.Empty);
        internal static LoadResult Failure(LoadErrorCode error, string reason) => new LoadResult(false, null, error, reason);
    }
}
