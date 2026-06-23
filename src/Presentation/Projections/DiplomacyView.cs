using System;
using System.Globalization;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Diplomacy;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 外交求粮展示视图（GDD_012 §8 / ADR-0002）。把 <see cref="DiplomacyProjection"/> 翻译为中文状态串：
    /// 未求援 / 拒绝 / 附条件 / 背约（代价已付）/ 在途（第 N 日抵达）/ 已抵达。
    /// 体现「非点击即到 + 可背约 + 代价不返还」设计锁。不可变、纯映射。逻辑由 dotnet 测试覆盖（BLOCKING）。
    /// </summary>
    public sealed class DiplomacyView
    {
        /// <summary>「求援」按钮是否可用（未用过才可用，受控一局一次）。</summary>
        public bool CanRequest { get; }
        /// <summary>外交状态文案。</summary>
        public string StatusLabel { get; }

        public DiplomacyView(DiplomacyProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            CanRequest = !projection.Used;

            if (!projection.Used)
            {
                StatusLabel = "可向江东求粮（一局一次）";
            }
            else if (projection.DeliveredAmount > 0)
            {
                StatusLabel = "援粮已抵达（粮草 +" + projection.DeliveredAmount.ToString(CultureInfo.InvariantCulture) + "）";
            }
            else if (projection.Response == DiplomaticResponse.Accepted && projection.Fulfilled && projection.PendingArrivalDay >= 0)
            {
                StatusLabel = "江东已许援粮 " + projection.PendingAmount.ToString(CultureInfo.InvariantCulture)
                    + "，第 " + (projection.PendingArrivalDay + 1).ToString(CultureInfo.InvariantCulture) + " 日抵达";
            }
            else if (projection.Response == DiplomaticResponse.Accepted && !projection.Fulfilled)
            {
                StatusLabel = "江东背约，援粮未至（代价已付）";
            }
            else if (projection.Response == DiplomaticResponse.Conditional)
            {
                StatusLabel = "江东仅附条件应允，未发援粮";
            }
            else
            {
                StatusLabel = "江东拒绝了求粮";
            }
        }
    }
}
