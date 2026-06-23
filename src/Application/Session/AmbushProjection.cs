namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 假退伏击的<b>只读投影 DTO</b>（GDD_010 / ADR-0002）。一次性决战赌注：设伏→在途→发动。
    /// 承载可否设伏 / 是否在途 / 预计发动日 / 是否已结算 / 是否得手。<b>不</b>暴露敌真实兵力（P10）。不可变。
    /// </summary>
    public sealed class AmbushProjection
    {
        /// <summary>当前是否可设伏诱敌（一局一次 + 无在途 + 工事足示弱 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>是否有伏击在途（已设伏未发动）。</summary>
        public bool InFlight { get; }
        /// <summary>伏击预计发动的世界日（0 基；-1 = 无在途）。</summary>
        public int ArrivalDay { get; }
        /// <summary>是否已结算（发动过）。</summary>
        public bool Resolved { get; }
        /// <summary>是否得手（结算且诱敌成立未暴露）。</summary>
        public bool Succeeded { get; }

        public AmbushProjection(bool canDispatch, bool inFlight, int arrivalDay, bool resolved, bool succeeded)
        {
            CanDispatch = canDispatch;
            InFlight = inFlight;
            ArrivalDay = arrivalDay;
            Resolved = resolved;
            Succeeded = succeeded;
        }
    }
}
