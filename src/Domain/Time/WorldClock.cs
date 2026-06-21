using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 推进时间命令（technical-preferences：Command 后缀）。封装一次推进的时段数（≥1）。
    /// 时间「仅由推进命令前进」（Control Manifest）——外部不得直接写 <see cref="WorldClock.Current"/>。
    /// </summary>
    public sealed class AdvanceTimeCommand
    {
        /// <summary>本次推进的时段数（≥1）。</summary>
        public int Segments { get; }

        public AdvanceTimeCommand(int segments)
        {
            if (segments < 1)
                throw new ArgumentOutOfRangeException(nameof(segments), "推进时段数须 ≥ 1。");
            Segments = segments;
        }
    }

    /// <summary>一次日界穿越的结算编排（GDD_001）：到达的世界日 + 该日界的固定阶段顺序。</summary>
    public readonly struct DayBoundarySettlement
    {
        /// <summary>本次穿越到达的世界日序号。</summary>
        public int Day { get; }

        /// <summary>该日界的结算阶段顺序（恒为 <see cref="DayBoundaryStages.CanonicalOrder"/>）。</summary>
        public IReadOnlyList<DayBoundaryStage> Stages => DayBoundaryStages.CanonicalOrder;

        public DayBoundarySettlement(int day) => Day = day;
    }

    /// <summary>一次推进的结果：起止时间 + 按时间顺序穿越的日界结算列表。</summary>
    public sealed class AdvanceResult
    {
        private readonly DayBoundarySettlement[] _dayBoundaries;

        /// <summary>推进前时间。</summary>
        public WorldTime From { get; }

        /// <summary>推进后时间。</summary>
        public WorldTime To { get; }

        /// <summary>按时间升序穿越的日界（未跨日则为空）。</summary>
        public IReadOnlyList<DayBoundarySettlement> DayBoundaries => _dayBoundaries;

        internal AdvanceResult(WorldTime from, WorldTime to, IReadOnlyList<DayBoundarySettlement> dayBoundaries)
        {
            From = from;
            To = to;
            var copy = new DayBoundarySettlement[dayBoundaries.Count];
            for (int i = 0; i < dayBoundaries.Count; i++) copy[i] = dayBoundaries[i];
            _dayBoundaries = copy;
        }
    }

    /// <summary>
    /// 世界时钟聚合（GDD_001 / TR-time-001）：持有权威 <see cref="Current"/> 时间，
    /// <b>只能</b>经 <see cref="Apply"/>(<see cref="AdvanceTimeCommand"/>) 前进（确定性、引擎无关）。
    /// 跨日时按全局固定顺序（时间推进 → 环境 → 补给 → 城市 → 状态事件）编排日界结算阶段，
    /// 同输入序列产生同样的时段变化与日界序列（可复现，ADR-0004）。
    /// </summary>
    public sealed class WorldClock
    {
        /// <summary>当前权威时间（外部只读；仅 <see cref="Apply"/> 可推进）。</summary>
        public WorldTime Current { get; private set; }

        public WorldClock(WorldTime start) => Current = start;

        /// <summary>
        /// 应用推进命令：前进时间并返回起止与穿越的日界结算。每跨入一个新世界日产生一次日界结算。
        /// </summary>
        public AdvanceResult Apply(AdvanceTimeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var from = Current;
            var to = from.Advance(command.Segments);

            var crossings = new List<DayBoundarySettlement>();
            for (int day = from.Day + 1; day <= to.Day; day++)
                crossings.Add(new DayBoundarySettlement(day));

            Current = to;
            return new AdvanceResult(from, to, crossings);
        }
    }
}
