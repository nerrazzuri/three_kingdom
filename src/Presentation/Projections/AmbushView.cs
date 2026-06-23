using System;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 假退伏击展示视图（GDD_010 / ADR-0002）。把 <see cref="AmbushProjection"/> 翻译为中文状态串 +
    /// 设伏按钮可用性。一次性决战赌注：设伏→在途→发动（非即时）；不暴露敌真值（P10）。不可变、纯映射。
    /// </summary>
    public sealed class AmbushView
    {
        /// <summary>「设伏诱敌」按钮是否可用（一局一次 + 无在途 + 工事足 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>状态文案。</summary>
        public string StatusLabel { get; }

        public AmbushView(AmbushProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            CanDispatch = projection.CanDispatch;

            if (projection.InFlight)
                StatusLabel = "已示弱诱敌，伏兵待发——第 "
                    + (projection.ArrivalDay + 1).ToString(CultureInfo.InvariantCulture) + " 日发动";
            else if (projection.Resolved)
                StatusLabel = projection.Succeeded
                    ? "伏击得手，大破敌军（再探可知敌情变化）"
                    : "诱敌不成，示弱失策（民心受挫、城防露怯）";
            else if (!projection.CanDispatch)
                StatusLabel = "暂不可设伏（工事不足或已用过此计）";
            else
                StatusLabel = "可设伏诱敌（一局一次；敌将性烈方可诱，降工事、风险高、回报大）";
        }
    }
}
