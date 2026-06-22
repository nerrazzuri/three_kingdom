using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Council
{
    /// <summary>
    /// 军师视角（GDD_008 §Data Model：AdvisorPerspective）。
    /// 能力影响发现矛盾/缺口的质量与建议置信（§Formula 1/2），但不赋予全知。不可变。
    /// </summary>
    public sealed class AdvisorPerspective
    {
        /// <summary>军师 ID。</summary>
        public AdvisorId Advisor { get; }

        /// <summary>军师相关能力 adv_cap（[0,1] 归一化，来自 GDD_005）。</summary>
        public FixedPoint Capability { get; }

        public AdvisorPerspective(AdvisorId advisor, FixedPoint capability)
        {
            if (capability < FixedPoint.Zero || capability > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(capability), "军师能力须在 [0,1]。");
            Advisor = advisor;
            Capability = capability;
        }
    }
}
