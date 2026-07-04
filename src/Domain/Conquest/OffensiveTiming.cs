using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 出征时机/天气窗口（GDD_019 D6，何时发起）：发起时段 + 当前天气。不可变。
    /// 门若干路线条件：夜袭需夜间时段（<see cref="IsNight"/>）；隐蔽成功需雾（或军纪足）。
    /// 时机不对 → 对应条件不成型（确定性，无随机）。
    /// </summary>
    public sealed class OffensiveTiming
    {
        /// <summary>发起时段。</summary>
        public DaySegment Segment { get; }

        /// <summary>发起时天气（GDD_002）。</summary>
        public WeatherType Weather { get; }

        public OffensiveTiming(DaySegment segment, WeatherType weather)
        {
            Segment = segment;
            Weather = weather;
        }

        /// <summary>是否夜间（夜袭条件门）。</summary>
        public bool IsNight => Segment == DaySegment.Night;

        /// <summary>是否有雾（隐蔽成功条件门之一）。</summary>
        public bool IsFoggy => Weather == WeatherType.Fog;
    }
}
