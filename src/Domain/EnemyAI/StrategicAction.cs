namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>
    /// 敌方 AI 候选战术动作（GDD_016 §MVP / ADR-0006）。MVP 战术层 3–4 动作；
    /// 战略层多日计划属后续。动作只是候选，最终由效用评分 + 种子 softmax 选择。
    /// </summary>
    public enum StrategicAction
    {
        /// <summary>追击：扩大战果（需敌可追）。</summary>
        Pursue = 0,

        /// <summary>撤退：保存实力（需有退路）。</summary>
        Retreat = 1,

        /// <summary>坚守：据守不出（通用兜底，通常可行）。</summary>
        Hold = 2,

        /// <summary>诱敌：假退设伏（需性格倾向 + 地形）。</summary>
        FeintLure = 3,
    }

    /// <summary>
    /// 决策缘由码（ADR-0006 §1/§3）：使 AI 的选择依据（含其错误信念）对玩家复盘<b>可读</b>，
    /// 而非黑箱。供 DecisionRecord 携带；LLM 下游（后续）据此产出台词，不回写状态。
    /// </summary>
    public enum AiReasonCode
    {
        /// <summary>明显最优：选中动作效用显著高于其余（非抖动）。</summary>
        TopUtility = 0,

        /// <summary>抖动：选中与次优效用接近，由种子 softmax 抖动选出（看似随机）。</summary>
        SoftmaxJitter = 1,

        /// <summary>性格主导：性格倾向显著抬高该动作效用（如急躁→追击）。</summary>
        PersonalityBias = 2,

        /// <summary>唯一可行：可行性门淘汰其余，仅此动作可行。</summary>
        OnlyFeasible = 3,
    }
}
