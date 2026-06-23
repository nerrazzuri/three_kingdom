using System;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 侦察派出展示视图（GDD_007 / ADR-0002）。把 <see cref="ScoutProjection"/> 翻译为中文状态串 +
    /// 派出按钮可用性。体现「派出→在途→返报」非即时（避免即时暴露敌情）。不可变、纯映射。BLOCKING 测试覆盖。
    /// </summary>
    public sealed class ScoutView
    {
        /// <summary>「派出侦察」按钮是否可用（无在途侦察 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>状态文案。</summary>
        public string StatusLabel { get; }

        public ScoutView(ScoutProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            CanDispatch = projection.CanDispatch;
            StatusLabel = projection.InFlight
                ? "侦察队在途——第 " + (projection.ArrivalDay + 1).ToString(CultureInfo.InvariantCulture) + " 日返报"
                : "可派出侦察（返报需时）";
        }
    }
}
