namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立结局分支（GDD_014 §Formula 3：关系网决定）。由发动时好感快照的 loyal_ratio 确定性判定。
    /// </summary>
    public enum RebellionOutcome
    {
        /// <summary>全员拥立：文武集体拥立，完整继承城池/兵力/人才，成立新势力。</summary>
        FullSupport = 0,

        /// <summary>部分跟随：半数跟随，部分旧臣归降，损失部分兵力城池，开局需平乱御敌。</summary>
        PartialFollow = 1,

        /// <summary>众叛亲离：城池倒戈、兵力溃散，仅带少量亲卫沦为流浪势力（合法可继续，非卡死）。</summary>
        Abandoned = 2,
    }
}
