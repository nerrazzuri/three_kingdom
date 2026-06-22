namespace ThreeKingdom.Domain.Intel
{
    /// <summary>
    /// 情报来源（GDD_007：报告含来源；来源决定可靠性而非真实概率）。
    /// S1 仅承载来源标识；置信度/时效衰减由 Story 002 在此基础上派生。
    /// 与地图层 <c>ThreeKingdom.Domain.Map.KnowledgeSource</c> 契约一致（同序：直接观察 &gt; 侦察 &gt; 传闻）。
    /// </summary>
    public enum IntelSource
    {
        /// <summary>直接观察（最高可靠性）。</summary>
        DirectObservation = 1,

        /// <summary>侦察（中等可靠性）。</summary>
        Scouting = 2,

        /// <summary>传闻（最低可靠性）。</summary>
        Rumor = 3,

        /// <summary>缴获文书（可靠但可能过时）。</summary>
        Captured = 4,
    }
}
