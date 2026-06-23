using ThreeKingdom.Domain.Diplomacy;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 外交求粮的<b>只读投影 DTO</b>（GDD_012 §8 / ADR-0002）。承载是否已求援、响应、兑现判定、
    /// 在途交付（到达日 + 规模）与已抵达量，供 HUD 显示外交状态。不可变；不泄露可变聚合。
    /// </summary>
    public sealed class DiplomacyProjection
    {
        /// <summary>是否已发起求援（受控入口，一局一次）。</summary>
        public bool Used { get; }
        /// <summary>外势力响应（接受/附条件/拒绝）。</summary>
        public DiplomaticResponse Response { get; }
        /// <summary>是否兑现（接受后随机流判定，未背约）。</summary>
        public bool Fulfilled { get; }
        /// <summary>在途交付到达日（0 基；-1 = 无在途/已抵达）。</summary>
        public int PendingArrivalDay { get; }
        /// <summary>在途交付规模（0 = 无）。</summary>
        public long PendingAmount { get; }
        /// <summary>已抵达入城的援粮量（&gt;0 = 已交付）。</summary>
        public long DeliveredAmount { get; }

        public DiplomacyProjection(
            bool used, DiplomaticResponse response, bool fulfilled,
            int pendingArrivalDay, long pendingAmount, long deliveredAmount)
        {
            Used = used;
            Response = response;
            Fulfilled = fulfilled;
            PendingArrivalDay = pendingArrivalDay;
            PendingAmount = pendingAmount;
            DeliveredAmount = deliveredAmount;
        }
    }
}
