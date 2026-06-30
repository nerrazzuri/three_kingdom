namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// CampaignSession 装配层稳定错误码（ADR-0009 §R-4）。数值稳定（追加新码用新序数，不重排），
    /// 用于跨层契约与回归断言；失败返回稳定码、无部分写入。
    /// </summary>
    public enum CampaignErrorCode
    {
        /// <summary>无错误（成功）。</summary>
        None = 0,

        /// <summary>空配置 / 空入参。</summary>
        NullConfig = 1,

        /// <summary>配置非法（缺字段/越界），加载期被拒。</summary>
        InvalidConfig = 2,

        /// <summary>会话不存在（按 id 查找失败）。</summary>
        SessionNotFound = 3,

        /// <summary>会话未启用城市治理（无城市态），治理命令不适用。</summary>
        CityGovernanceDisabled = 4,

        /// <summary>命令数量非法（负数等）。</summary>
        InvalidAmount = 5,

        /// <summary>库存/可分配量不足，征用被拒（无部分写入）。</summary>
        InsufficientStock = 6,

        /// <summary>工事已满，修复目标无效（无部分写入）。</summary>
        FortificationFull = 7,

        /// <summary>会话未启用情报/军议，情报命令不适用。</summary>
        IntelDisabled = 8,

        /// <summary>侦察对象非法（未在世界真值登记 / 缺必填字段；"侦察全部"非法）。</summary>
        UnknownIntelSubject = 9,
    }
}
