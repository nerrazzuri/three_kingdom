using System;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 开局结果（ADR-0009 §R-4）。不可变。成功携新会话；失败携稳定 <see cref="CampaignErrorCode"/>、无部分写入。
    /// </summary>
    public sealed class CampaignStartResult
    {
        /// <summary>是否成功开局。</summary>
        public bool Started { get; }

        /// <summary>会话（成功时非空）。</summary>
        public CampaignSession? Session { get; }

        /// <summary>错误码（成功为 None）。</summary>
        public CampaignErrorCode Error { get; }

        /// <summary>可解释明细。</summary>
        public string Detail { get; }

        private CampaignStartResult(bool started, CampaignSession? session, CampaignErrorCode error, string detail)
        {
            Started = started;
            Session = session;
            Error = error;
            Detail = detail ?? string.Empty;
        }

        public static CampaignStartResult Success(CampaignSession session)
            => new CampaignStartResult(true, session ?? throw new ArgumentNullException(nameof(session)), CampaignErrorCode.None, string.Empty);

        public static CampaignStartResult Failure(CampaignErrorCode error, string detail)
            => new CampaignStartResult(false, null, error, detail);
    }
}
