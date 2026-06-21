using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>
    /// 时段跨越结算阶段（GDD_001 §Main Rules：耗尽时段预算跨入下一世界时段，触发天气/补给/疲劳结算）。
    /// 枚举序数即固定顺序：天气 → 补给 → 疲劳。
    /// </summary>
    public enum SegmentSettlementStage
    {
        /// <summary>天气（环境派生修正）。</summary>
        Weather = 0,

        /// <summary>补给（后勤消耗）。</summary>
        Supply = 1,

        /// <summary>疲劳（士气/疲劳累积）。</summary>
        Fatigue = 2,
    }

    /// <summary>时段跨越结算阶段的权威固定顺序。</summary>
    public static class SegmentSettlementStages
    {
        /// <summary>规范顺序：天气 → 补给 → 疲劳。</summary>
        public static readonly IReadOnlyList<SegmentSettlementStage> CanonicalOrder = new[]
        {
            SegmentSettlementStage.Weather,
            SegmentSettlementStage.Supply,
            SegmentSettlementStage.Fatigue,
        };
    }

    /// <summary>
    /// 一次时段跨越：到达的世界时间 + 该时段结算阶段（天气/补给/疲劳）+ 若同时跨日则附带日界结算（复用 Story 001 编排）。
    /// </summary>
    public readonly struct SegmentCrossing
    {
        private readonly DayBoundarySettlement[] _dayBoundaries;

        /// <summary>跨越后到达的世界时间。</summary>
        public WorldTime To { get; }

        /// <summary>时段结算阶段（恒为 <see cref="SegmentSettlementStages.CanonicalOrder"/>）。</summary>
        public IReadOnlyList<SegmentSettlementStage> SegmentStages => SegmentSettlementStages.CanonicalOrder;

        /// <summary>若此次时段推进同时跨日，附带的日界结算（否则为空）。</summary>
        public IReadOnlyList<DayBoundarySettlement> DayBoundaries => _dayBoundaries;

        internal SegmentCrossing(WorldTime to, IReadOnlyList<DayBoundarySettlement> dayBoundaries)
        {
            To = to;
            var copy = new DayBoundarySettlement[dayBoundaries.Count];
            for (int i = 0; i < dayBoundaries.Count; i++) copy[i] = dayBoundaries[i];
            _dayBoundaries = copy;
        }
    }

    /// <summary>一次战斗阶段消耗的结果：跨越的世界时段数、各时段跨越（按时间序）、当前时段内剩余阶段数。</summary>
    public sealed class BattlePhaseResult
    {
        private readonly SegmentCrossing[] _crossings;

        /// <summary>本次消耗跨越的世界时段数（可为 0）。</summary>
        public int SegmentsAdvanced { get; }

        /// <summary>按时间升序的时段跨越列表。</summary>
        public IReadOnlyList<SegmentCrossing> Crossings => _crossings;

        /// <summary>消耗后停留在当前世界时段内的战斗阶段数（0 ≤ x &lt; PhaseBudget）。</summary>
        public int RemainingPhasesInSegment { get; }

        internal BattlePhaseResult(int segmentsAdvanced, IReadOnlyList<SegmentCrossing> crossings, int remaining)
        {
            SegmentsAdvanced = segmentsAdvanced;
            var copy = new SegmentCrossing[crossings.Count];
            for (int i = 0; i < crossings.Count; i++) copy[i] = crossings[i];
            _crossings = copy;
            RemainingPhasesInSegment = remaining;
        }
    }

    /// <summary>
    /// 战斗时钟（GDD_001 §Formula 4 / TR-time-002）：嵌套于世界时间之上，按<b>配置驱动</b>的时段预算消耗战斗阶段。
    /// <c>segments_consumed = floor(phases_elapsed ÷ PhaseBudget)</c>——每耗尽 PhaseBudget 个战斗阶段确定性跨入
    /// 下一世界时段（经 <see cref="WorldClock"/> 推进，复用 Story 001 日界编排），并触发天气/补给/疲劳结算。
    /// 预算为整数（来自 epic-001 配置管线，禁硬编码 / 禁 float 计时，ADR-0004）。
    /// </summary>
    public sealed class BattleClock
    {
        private readonly WorldClock _worldClock;

        /// <summary>每个世界时段容纳的战斗阶段数（≥1，配置驱动）。</summary>
        public int PhaseBudget { get; }

        /// <summary>当前世界时段内已消耗的战斗阶段数（0 ≤ x &lt; PhaseBudget）。</summary>
        public int PhasesInCurrentSegment { get; private set; }

        /// <summary>当前权威世界时间（由底层世界时钟提供）。</summary>
        public WorldTime Current => _worldClock.Current;

        /// <param name="worldClock">底层世界时钟（提供时间权威与日界编排）。</param>
        /// <param name="phaseBudget">时段预算（≥1，来自配置）。</param>
        public BattleClock(WorldClock worldClock, int phaseBudget)
        {
            _worldClock = worldClock ?? throw new ArgumentNullException(nameof(worldClock));
            if (phaseBudget < 1)
                throw new ArgumentOutOfRangeException(nameof(phaseBudget), "时段预算须 ≥ 1。");
            PhaseBudget = phaseBudget;
        }

        /// <summary>
        /// 消耗 <paramref name="phases"/> 个战斗阶段。累积超过预算的部分按 floor 除法确定性跨入相应数量的世界时段，
        /// 余数留在当前时段。单次超额自然跨越多个时段（GDD §Formula 4）。
        /// </summary>
        public BattlePhaseResult ConsumePhases(int phases)
        {
            if (phases < 1)
                throw new ArgumentOutOfRangeException(nameof(phases), "消耗的战斗阶段数须 ≥ 1。");

            int total = checked(PhasesInCurrentSegment + phases);
            int segments = total / PhaseBudget;
            PhasesInCurrentSegment = total % PhaseBudget;

            var crossings = new List<SegmentCrossing>(segments);
            for (int i = 0; i < segments; i++)
            {
                var advance = _worldClock.Apply(new AdvanceTimeCommand(1));
                crossings.Add(new SegmentCrossing(advance.To, advance.DayBoundaries));
            }

            return new BattlePhaseResult(segments, crossings, PhasesInCurrentSegment);
        }
    }
}
