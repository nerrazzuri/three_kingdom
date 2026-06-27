using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 历史事件四元组（GDD_015 / ADR-0007）。不可变定义。
    /// <c>{时间窗 + 前置条件谓词 + 正常结局 + 分叉结局 + 下游事件引用}</c>。
    /// <para>
    /// diverged 标志属 <see cref="WorldState"/>（可变运行态），<b>不</b>属此定义。
    /// 本类型为<b>候选</b>表示：允许空前置 / 缺分叉结局，以便 <see cref="HistoricalEventCatalog"/>
    /// 在加载期聚合校验并拒绝（ADR-0003：缺前置或缺分叉分支的事件被拒）。
    /// </para>
    /// </summary>
    public sealed class HistoricalEvent
    {
        private readonly Precondition[] _preconds;
        private readonly EventId[] _downstream;

        /// <summary>事件 ID。</summary>
        public EventId Id { get; }

        /// <summary>时间窗。</summary>
        public TimeWindow Window { get; }

        /// <summary>前置条件（可为空——加载期校验会拒绝空前置事件）。</summary>
        public IReadOnlyList<Precondition> Preconds => _preconds;

        /// <summary>正常结局（非空）。</summary>
        public HistoricalOutcome NormalOutcome { get; }

        /// <summary>分叉结局（可为 null——加载期校验会拒绝缺分叉分支的事件）。</summary>
        public HistoricalOutcome? DivergenceOutcome { get; }

        /// <summary>下游事件引用（依赖本事件，按稳定序重评估属 story-003）。</summary>
        public IReadOnlyList<EventId> Downstream => _downstream;

        public HistoricalEvent(
            EventId id,
            TimeWindow window,
            IReadOnlyList<Precondition> preconds,
            HistoricalOutcome normalOutcome,
            HistoricalOutcome? divergenceOutcome,
            IReadOnlyList<EventId> downstream)
        {
            if (preconds is null) throw new ArgumentNullException(nameof(preconds));
            if (downstream is null) throw new ArgumentNullException(nameof(downstream));
            Id = id;
            Window = window;
            NormalOutcome = normalOutcome ?? throw new ArgumentNullException(nameof(normalOutcome));
            DivergenceOutcome = divergenceOutcome;

            var p = new Precondition[preconds.Count];
            for (int i = 0; i < preconds.Count; i++)
                p[i] = preconds[i] ?? throw new ArgumentException("前置不可含 null。", nameof(preconds));
            _preconds = p;

            var d = new EventId[downstream.Count];
            for (int i = 0; i < downstream.Count; i++) d[i] = downstream[i];
            _downstream = d;
        }

        /// <summary>全部前置在给定世界态下是否成立（确定性，AND 语义）。</summary>
        public bool AllPreconditionsHold(WorldState world)
        {
            foreach (Precondition p in _preconds)
                if (!p.Evaluate(world)) return false;
            return true;
        }
    }
}
