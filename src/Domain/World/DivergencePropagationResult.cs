using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.World
{
    /// <summary>分叉传播结果（ADR-0007 §3）。不可变。携传播后世界态与按处理（EventId 稳定）序的下游分叉列表。</summary>
    public sealed class DivergencePropagationResult
    {
        /// <summary>传播后的世界态（下游分叉已置标）。</summary>
        public WorldState World { get; }

        /// <summary>本次因传播而分叉的下游事件（按 EventId 稳定序）。</summary>
        public IReadOnlyList<EventId> DivergedDownstream { get; }

        public DivergencePropagationResult(WorldState world, IReadOnlyList<EventId> divergedDownstream)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            DivergedDownstream = divergedDownstream ?? throw new ArgumentNullException(nameof(divergedDownstream));
        }
    }
}
