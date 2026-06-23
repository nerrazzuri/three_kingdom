namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 侦察派出状态的<b>只读投影 DTO</b>（GDD_007 / ADR-0002）。派出→在途→返报（非即时暴露）：
    /// 承载可否派出 / 是否在途 / 预计返报日。情报内容本身经 <see cref="IntelProjection"/> / 敌情视图呈现。
    /// 不可变。
    /// </summary>
    public sealed class ScoutProjection
    {
        /// <summary>当前是否可派出侦察（无在途侦察 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>是否有侦察队在途（已派出未返报）。</summary>
        public bool InFlight { get; }
        /// <summary>侦察队预计返报的世界日（0 基；-1 = 无在途）。</summary>
        public int ArrivalDay { get; }

        public ScoutProjection(bool canDispatch, bool inFlight, int arrivalDay)
        {
            CanDispatch = canDispatch;
            InFlight = inFlight;
            ArrivalDay = arrivalDay;
        }
    }
}
