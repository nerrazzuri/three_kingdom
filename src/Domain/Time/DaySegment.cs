namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 世界日内的时段（GDD_001：MVP 四时段 黎明/白昼/黄昏/夜间）。
    /// 枚举序数即时段在日内的次序（0..SEGMENTS_PER_DAY-1），用于绝对时间线性化 T = d×SEGMENTS_PER_DAY + s。
    /// </summary>
    public enum DaySegment
    {
        /// <summary>黎明。</summary>
        Dawn = 0,

        /// <summary>白昼。</summary>
        Day = 1,

        /// <summary>黄昏。</summary>
        Dusk = 2,

        /// <summary>夜间。</summary>
        Night = 3,
    }
}
