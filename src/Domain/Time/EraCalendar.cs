using System;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 纪元日历（GDD_026 R2 / ADR-0015 D1）：在既有抽象「日-段」<see cref="WorldTime"/> 之上叠一层<b>公元年锚</b>——
    /// <c>当前年 = 起始年 + 已历年数</c>，已历年数由粗粒度 <see cref="WorldTime.Day"/> 按 <see cref="DaysPerYear"/> 折算。
    /// <b>纯函数、确定性</b>（ADR-0004）：不新增权威状态、不引随机；年份完全由 <see cref="WorldTime"/> 派生 → 存读档一致。
    /// 历史事件（GDD_015）与空降者寿命（<see cref="Life.ArrivalLife"/>）按此判年。
    /// </summary>
    public sealed class EraCalendar
    {
        /// <summary>起始公元年（锚点年，如 190）。</summary>
        public int StartYear { get; }

        /// <summary>一公元年折合多少「日」（旋钮，GDD_026 §12；暂定 12——把细粒度日-段与粗粒度年解耦）。</summary>
        public int DaysPerYear { get; }

        public EraCalendar(int startYear, int daysPerYear = 12)
        {
            if (daysPerYear <= 0) throw new ArgumentOutOfRangeException(nameof(daysPerYear), "一年折合日数须为正。");
            StartYear = startYear;
            DaysPerYear = daysPerYear;
        }

        /// <summary>某时点已历公元年数（自开局）。</summary>
        public int YearsElapsed(WorldTime t) => t.Day / DaysPerYear;

        /// <summary>某时点的公元年。</summary>
        public int YearOf(WorldTime t) => StartYear + YearsElapsed(t);
    }
}
