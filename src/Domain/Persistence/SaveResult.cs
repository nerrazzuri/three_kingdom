namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>存档写入错误码（稳定枚举，供 UI 显示可行动原因）。</summary>
    public enum SaveErrorCode
    {
        /// <summary>无错误。</summary>
        None = 0,

        /// <summary>写入临时槽失败（如磁盘写满）；正式存档未被破坏。</summary>
        TempWriteFailed = 1,

        /// <summary>原子改名失败；正式存档保持上一份有效内容。</summary>
        CommitFailed = 2,
    }

    /// <summary>
    /// 存档写入结果（TR-save-001）。失败时<b>保证</b>正式存档仍为上一份有效内容（原子写不破坏）。不可变。
    /// </summary>
    public sealed class SaveResult
    {
        /// <summary>是否成功提交。</summary>
        public bool Succeeded { get; }

        /// <summary>错误码（成功为 <see cref="SaveErrorCode.None"/>）。</summary>
        public SaveErrorCode Error { get; }

        /// <summary>可读细节。</summary>
        public string Detail { get; }

        private SaveResult(bool succeeded, SaveErrorCode error, string detail)
        {
            Succeeded = succeeded;
            Error = error;
            Detail = detail ?? string.Empty;
        }

        internal static SaveResult Success() => new SaveResult(true, SaveErrorCode.None, string.Empty);
        internal static SaveResult Failure(SaveErrorCode error, string detail) => new SaveResult(false, error, detail);
    }
}
