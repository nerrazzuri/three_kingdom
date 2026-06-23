namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 袭扰敌补给（断粮疲敌）的<b>只读投影 DTO</b>（GDD_010/012 / ADR-0002）。
    /// 派出→在途→见效（非即时）：承载可否派出 / 是否在途 / 预计见效日 / 上次见效结果。
    /// <b>不</b>暴露敌真实兵力（P10：削减效果须经侦察得知）。不可变；不泄露可变聚合。
    /// </summary>
    public sealed class RaidProjection
    {
        /// <summary>当前是否可派出袭扰（无在途 + 粮草足 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>是否有袭扰队在途（已派出未见效）。</summary>
        public bool InFlight { get; }
        /// <summary>袭扰队预计见效的世界日（0 基；-1 = 无在途）。</summary>
        public int ArrivalDay { get; }
        /// <summary>是否有可展示的已见效结果。</summary>
        public bool HasResult { get; }
        /// <summary>上一次已见效袭扰是否暴露（暴露损民心，未暴露则削敌补给）。</summary>
        public bool LastExposed { get; }

        public RaidProjection(bool canDispatch, bool inFlight, int arrivalDay, bool hasResult, bool lastExposed)
        {
            CanDispatch = canDispatch;
            InFlight = inFlight;
            ArrivalDay = arrivalDay;
            HasResult = hasResult;
            LastExposed = lastExposed;
        }
    }
}
