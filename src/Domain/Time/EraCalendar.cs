using System;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 纪元日历（GDD_026 R2 / ADR-0015 D1）：在既有 <see cref="WorldTime"/> 之上叠公元纪年层。
    /// <b>时间尺度（2026-07-05 定）</b>：世界地图一步 = <b>一周</b>（<see cref="WorldTime.Day"/> 即"周"序，时段仅战斗内用）；
    /// <b>一季 = 13 周、一年 = 52 周 = 4 季</b>。争霸/历史事件/寿命按季/年推进；<c>当前年 = 起始年 + 周/52</c>。
    /// <b>纯函数、确定性</b>（ADR-0004）：不新增权威状态、不引随机；纪元完全由 <see cref="WorldTime"/> 派生 → 存读档一致。
    /// </summary>
    public sealed class EraCalendar
    {
        /// <summary>起始公元年（锚点年，如 190）。</summary>
        public int StartYear { get; }
        /// <summary>一年折合周数（旋钮，默认 52）。</summary>
        public int WeeksPerYear { get; }
        /// <summary>一季折合周数（旋钮，默认 13）。</summary>
        public int WeeksPerSeason { get; }

        public EraCalendar(int startYear, int weeksPerYear = 52, int weeksPerSeason = 13)
        {
            if (weeksPerYear <= 0) throw new ArgumentOutOfRangeException(nameof(weeksPerYear), "一年折合周数须为正。");
            if (weeksPerSeason <= 0) throw new ArgumentOutOfRangeException(nameof(weeksPerSeason), "一季折合周数须为正。");
            StartYear = startYear;
            WeeksPerYear = weeksPerYear;
            WeeksPerSeason = weeksPerSeason;
        }

        /// <summary>某时点已历周数（自开局；WorldTime.Day 即周序）。</summary>
        public int WeeksElapsed(WorldTime t) => t.Day;

        /// <summary>某时点已历季数（自开局）。</summary>
        public int SeasonsElapsed(WorldTime t) => t.Day / WeeksPerSeason;

        /// <summary>某时点已历公元年数（自开局）。</summary>
        public int YearsElapsed(WorldTime t) => t.Day / WeeksPerYear;

        /// <summary>某时点的公元年。</summary>
        public int YearOf(WorldTime t) => StartYear + YearsElapsed(t);

        /// <summary>某时点在当年的第几季（0=春,1=夏,2=秋,3=冬）。</summary>
        public int SeasonOfYear(WorldTime t) => (t.Day / WeeksPerSeason) % 4;

        /// <summary>某时点在当年的第几周（0 起）。</summary>
        public int WeekOfYear(WorldTime t) => t.Day % WeeksPerYear;

        /// <summary>把某公元年折算为自锚点起的周序（供历史事件按年铺开；早于锚点则 0）。</summary>
        public int WeekIndexOfYear(int year) => Math.Max(0, (year - StartYear) * WeeksPerYear);
    }
}
