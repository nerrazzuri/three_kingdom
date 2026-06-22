namespace ThreeKingdom.Domain.Diplomacy
{
    /// <summary>
    /// 受控外交请求类型（GDD_012 §8.4：slice 三选一受控入口）。
    /// 每种作用于 slice 的一个维度：兵力 / 后勤 / 时间——不扩张为完整天下外交。
    /// </summary>
    public enum DiplomaticRequestType
    {
        /// <summary>求援：到达时生成援军，作用于兵力（喂 GDD_010）。</summary>
        Reinforcement = 0,

        /// <summary>求粮：到达时交付外部补给，作用于后勤（喂 GDD_012 §1 守恒）。</summary>
        Supply = 1,

        /// <summary>求时限：改变敌军压力期限的剩余，作用于时间（喂 GDD_001 §5）。</summary>
        Deadline = 2,
    }

    /// <summary>外势力响应（GDD_012 §8.1：确定性结构化响应而非单值）。</summary>
    public enum DiplomaticResponse
    {
        /// <summary>拒绝：grant_score 低于附条件阈值。</summary>
        Rejected = 0,

        /// <summary>附条件：grant_score 在附条件与接受阈值之间，需满足额外承诺。</summary>
        Conditional = 1,

        /// <summary>接受：grant_score 达接受阈值，进入延迟交付与兑现判定。</summary>
        Accepted = 2,
    }

    /// <summary>兑现/背约结果原因（GDD_012 §8.3 / §Failure Cases：可解释 gameplay 结果，非黑箱）。</summary>
    public enum DiplomaticOutcomeReason
    {
        /// <summary>兑现：按时交付。</summary>
        Fulfilled = 0,

        /// <summary>未接受：请求被拒或仅附条件，未进入交付。</summary>
        NotAccepted = 1,

        /// <summary>玩家违反承诺前提，外援取消。</summary>
        PlayerBreached = 2,

        /// <summary>交付路线被永久切断，转为运输失败。</summary>
        RoutePermanentlyCut = 3,

        /// <summary>外势力背约/迟到/缩水（确定性随机流判定）。</summary>
        BetrayedByForeignPower = 4,
    }
}
