using System;
using System.Globalization;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Presentation.Projections
{
    /// <summary>
    /// 袭扰敌补给（断粮疲敌）展示视图（GDD_010/012 / ADR-0002）。把 <see cref="RaidProjection"/> 翻译为
    /// 中文状态串 + 派出按钮可用性。体现「派出→在途→见效」非即时；不暴露敌真值（P10：效果须经侦察得知）。
    /// 不可变、纯映射。BLOCKING 测试覆盖。
    /// </summary>
    public sealed class RaidView
    {
        /// <summary>「派出袭扰」按钮是否可用（无在途袭扰 + 粮草足 + 局未终）。</summary>
        public bool CanDispatch { get; }
        /// <summary>状态文案。</summary>
        public string StatusLabel { get; }

        public RaidView(RaidProjection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            CanDispatch = projection.CanDispatch;

            if (projection.InFlight)
                StatusLabel = "袭扰队在途——第 " + (projection.ArrivalDay + 1).ToString(CultureInfo.InvariantCulture) + " 日见效";
            else if (projection.HasResult)
                StatusLabel = projection.LastExposed
                    ? "上次袭扰暴露，袭扰队受挫（民心受损）"
                    : "上次袭扰得手，敌补给受创（再探可知敌情变化）";
            else if (!projection.CanDispatch)
                StatusLabel = "粮草不足，无法派出袭扰";
            else
                StatusLabel = "可派出袭扰（花粮草、有暴露风险，见效需时）";
        }
    }
}
