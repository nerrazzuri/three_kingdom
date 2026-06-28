namespace ThreeKingdom.Application.Session
{
    /// <summary>会话命令结果（ADR-0009 §R-4）。不可变。失败携稳定 <see cref="CampaignErrorCode"/>、无部分写入。</summary>
    public sealed class CampaignCommandResult
    {
        /// <summary>是否应用。</summary>
        public bool Applied { get; }

        /// <summary>错误码（成功为 None）。</summary>
        public CampaignErrorCode Error { get; }

        /// <summary>可解释明细。</summary>
        public string Detail { get; }

        private CampaignCommandResult(bool applied, CampaignErrorCode error, string detail)
        {
            Applied = applied;
            Error = error;
            Detail = detail ?? string.Empty;
        }

        public static CampaignCommandResult Success() => new CampaignCommandResult(true, CampaignErrorCode.None, string.Empty);
        public static CampaignCommandResult Failure(CampaignErrorCode error, string detail) => new CampaignCommandResult(false, error, detail);
    }
}
