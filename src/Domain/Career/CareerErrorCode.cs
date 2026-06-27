namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 生涯命令稳定错误码（GDD_014 §Failure Cases：「非法操作被拒，返回稳定错误码，无部分写入」/ TR-career-005）。
    /// 数值稳定（追加新码用新序数，不重排现有码），用于跨层契约与回归断言。
    /// </summary>
    public enum CareerErrorCode
    {
        /// <summary>无错误（成功）。</summary>
        None = 0,

        /// <summary>空命令（Application 前置校验）。</summary>
        NullCommand = 1,

        /// <summary>功绩增量为负（merit 为单调累积里程碑闸，不可负增）。</summary>
        NegativeMeritGain = 2,

        /// <summary>名望增量为负（renown 为单调累积值，不可负增）。</summary>
        NegativeRenownGain = 3,

        /// <summary>越级晋升（晋升须逐级 rank→rank+1，目标非紧邻上一阶）。</summary>
        RankSkipNotAllowed = 4,

        /// <summary>已达最高阶（<see cref="Rank.Successor"/>），无可晋升目标。</summary>
        AlreadyAtMaxRank = 5,

        /// <summary>在野状态：无君主，不可执行晋升 / 君主好感类命令。</summary>
        Unaffiliated = 6,

        /// <summary>任免对象不在僚属列表（无法授予官职位）。</summary>
        UnknownRetinueMember = 7,

        /// <summary>晋升门槛未达（merit/renown/lord_standing 任一不足；story-002 配置门槛判定）。</summary>
        PromotionThresholdNotMet = 8,

        /// <summary>自立发动条件未满足（story-003）。</summary>
        RebellionConditionNotMet = 9,
    }
}
