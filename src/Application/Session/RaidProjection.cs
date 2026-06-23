namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 袭扰敌补给（断粮疲敌）的<b>只读投影 DTO</b>（GDD_010/012 / ADR-0002）。
    /// 只承载是否可袭扰 + 上次袭扰结果（执行/暴露），<b>不</b>暴露敌真实兵力（P10：削减效果须经侦察得知）。
    /// 不可变；不泄露可变聚合。
    /// </summary>
    public sealed class RaidProjection
    {
        /// <summary>本日是否可袭扰（一日一袭 + 粮草足 + 局未终）。</summary>
        public bool CanRaid { get; }
        /// <summary>上一次操作是否真正执行了袭扰（false = 不可袭扰被忽略）。</summary>
        public bool LastPerformed { get; }
        /// <summary>上一次袭扰是否暴露（暴露损民心，未暴露则削敌补给）。</summary>
        public bool LastExposed { get; }

        public RaidProjection(bool canRaid, bool lastPerformed, bool lastExposed)
        {
            CanRaid = canRaid;
            LastPerformed = lastPerformed;
            LastExposed = lastExposed;
        }
    }
}
