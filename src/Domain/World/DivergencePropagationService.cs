using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 分叉下游重评估传播（GDD_015 / ADR-0007 §3 / TR-world-002）。纯函数、确定性、有界可终止。
    /// 某事件分叉后，按 <b>EventId 稳定序</b>重检其下游前置；下游若<b>够得着且前置被破坏</b>则随之分叉，
    /// 并按 <see cref="DivergencePropagationConfig.SpreadDepth"/> 继续涟漪。"已处理集合"保证无环、有界终止。
    /// <para>
    /// 够不着的下游短路（不脱稿，留在轨）——与 story-002 reachability 门一致，维持"远方便宜"。
    /// </para>
    /// </summary>
    public sealed class DivergencePropagationService
    {
        private readonly IReachPredicate _reach;

        public DivergencePropagationService(IReachPredicate reachPredicate)
        {
            _reach = reachPredicate ?? throw new ArgumentNullException(nameof(reachPredicate));
        }

        public DivergencePropagationService() : this(new SubjectReachPredicate()) { }

        /// <summary>
        /// 从已分叉的 <paramref name="origin"/> 出发，重评估其下游链（稳定序、深度受限、去重）。
        /// </summary>
        public DivergencePropagationResult Propagate(
            HistoricalEventCatalog catalog, HistoricalEvent origin, WorldState world,
            PlayerReach reach, DivergencePropagationConfig config)
        {
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            if (origin is null) throw new ArgumentNullException(nameof(origin));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (reach is null) throw new ArgumentNullException(nameof(reach));
            if (config is null) throw new ArgumentNullException(nameof(config));

            var visited = new HashSet<EventId> { origin.Id }; // 源已分叉，防自引用回环
            var diverged = new List<EventId>();
            WorldState current = world;

            foreach (EventId d in StableSorted(origin.Downstream))
                current = Visit(catalog, d, current, reach, config.SpreadDepth, visited, diverged);

            return new DivergencePropagationResult(current, diverged);
        }

        private WorldState Visit(
            HistoricalEventCatalog catalog, EventId eventId, WorldState world,
            PlayerReach reach, int depthRemaining, HashSet<EventId> visited, List<EventId> diverged)
        {
            if (!visited.Add(eventId)) return world; // 已处理 → 去重，保证有界终止

            HistoricalEvent? e = catalog.Find(eventId);
            if (e is null) return world;                       // 目录已校验下游引用，理论不达
            if (world.IsTriggered(eventId.Value)) return world; // 已结算 → 不重复分叉
            if (!_reach.Reachable(e, reach)) return world;      // 够不着 → 不脱稿（留在轨）
            if (e.AllPreconditionsHold(world)) return world;    // 前置仍成立 → 不分叉

            // 够得着且前置被破坏 → 下游分叉。
            WorldState next = world.WithTriggeredEvent(eventId.Value, diverged: true);
            diverged.Add(eventId);

            if (depthRemaining > 0)
                foreach (EventId d in StableSorted(e.Downstream))
                    next = Visit(catalog, d, next, reach, depthRemaining - 1, visited, diverged);

            return next;
        }

        private static IReadOnlyList<EventId> StableSorted(IReadOnlyList<EventId> ids)
        {
            var list = new List<EventId>(ids);
            list.Sort();   // EventId 实现 IComparable（序数序）——稳定确定性遍历
            return list;
        }
    }
}
