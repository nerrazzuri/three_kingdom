using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>寻路结果（GDD_003 §Formula 2）。无路径时 <see cref="HasPath"/>=false（GDD §Failure：无合法路径不移动）。</summary>
    public sealed class PathResult
    {
        private static readonly RouteId[] NoRoutes = Array.Empty<RouteId>();
        private static readonly RegionId[] NoRegions = Array.Empty<RegionId>();

        /// <summary>是否找到路径。</summary>
        public bool HasPath { get; }

        /// <summary>路径上的路线序列（起→讫；起讫相同则为空）。</summary>
        public IReadOnlyList<RouteId> Routes { get; }

        /// <summary>路径上的区域序列（含起讫）。</summary>
        public IReadOnlyList<RegionId> Regions { get; }

        /// <summary>路径总耗时（各路线耗时之和）。</summary>
        public int TotalCost { get; }

        private PathResult(bool hasPath, IReadOnlyList<RouteId> routes, IReadOnlyList<RegionId> regions, int totalCost)
        {
            HasPath = hasPath;
            Routes = routes;
            Regions = regions;
            TotalCost = totalCost;
        }

        /// <summary>无路径。</summary>
        public static PathResult None() => new PathResult(false, NoRoutes, NoRegions, 0);

        internal static PathResult Found(IReadOnlyList<RouteId> routes, IReadOnlyList<RegionId> regions, int totalCost)
            => new PathResult(true, routes, regions, totalCost);
    }

    /// <summary>
    /// 确定性寻路（GDD_003 §Formula 2 / TR-map-001,002 / ADR-0004）：整数代价 Dijkstra。
    /// 确定性来源：候选节点按 (距离, RegionId 序) 选取；邻接按 RouteId 序遍历；
    /// 等代价路径<b>平局按路线稳定 ID 序列字典序</b>破除（保证同图 + 同起讫 → 同路径，可重放）。
    /// </summary>
    public sealed class Pathfinder
    {
        /// <summary>
        /// 求 <paramref name="from"/> 到 <paramref name="to"/> 的最小代价路径。
        /// <paramref name="costOf"/> 为各路线耗时（默认 <see cref="Route.BaseTime"/>；传入可纳入天气/负载已结算修正）。
        /// 不连通返回 <see cref="PathResult.None"/>。
        /// </summary>
        public static PathResult FindPath(WorldMap map, RegionId from, RegionId to, Func<Route, int>? costOf = null)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (!map.HasRegion(from)) throw new ArgumentException($"起区域不存在：{from}。", nameof(from));
            if (!map.HasRegion(to)) throw new ArgumentException($"讫区域不存在：{to}。", nameof(to));
            costOf ??= r => r.BaseTime;

            if (from == to)
                return PathResult.Found(Array.Empty<RouteId>(), new[] { from }, 0);

            var dist = new Dictionary<RegionId, int> { [from] = 0 };
            var bestPath = new Dictionary<RegionId, List<RouteId>> { [from] = new List<RouteId>() };
            var visited = new HashSet<RegionId>();

            while (TryPickNearest(dist, visited, out RegionId u, out int du))
            {
                visited.Add(u);
                if (u == to) break;

                foreach (var route in map.RoutesFrom(u))
                {
                    var v = route.Other(u);
                    if (visited.Contains(v)) continue;

                    int cost = costOf(route);
                    if (cost < 1) throw new InvalidOperationException($"路线 {route.Id} 耗时须 ≥ 1。");
                    int nd = du + cost;

                    var candidate = new List<RouteId>(bestPath[u]) { route.Id };
                    if (!dist.TryGetValue(v, out int dv) || nd < dv || (nd == dv && LexLess(candidate, bestPath[v])))
                    {
                        dist[v] = nd;
                        bestPath[v] = candidate;
                    }
                }
            }

            if (!dist.TryGetValue(to, out int total))
                return PathResult.None();

            var routes = bestPath[to];
            var regions = new List<RegionId> { from };
            var current = from;
            foreach (var rid in routes)
            {
                current = map.Route(rid).Other(current);
                regions.Add(current);
            }
            return PathResult.Found(routes.ToArray(), regions.ToArray(), total);
        }

        /// <summary>选未访问且距离最小者；同距离按 RegionId 序（确定）。</summary>
        private static bool TryPickNearest(Dictionary<RegionId, int> dist, HashSet<RegionId> visited, out RegionId best, out int bestDist)
        {
            bool found = false;
            best = default;
            bestDist = int.MaxValue;
            foreach (var kv in dist)
            {
                if (visited.Contains(kv.Key)) continue;
                if (!found || kv.Value < bestDist || (kv.Value == bestDist && kv.Key.CompareTo(best) < 0))
                {
                    found = true;
                    bestDist = kv.Value;
                    best = kv.Key;
                }
            }
            return found;
        }

        /// <summary>路线 ID 序列字典序比较：a 严格小于 b。</summary>
        private static bool LexLess(List<RouteId> a, List<RouteId> b)
        {
            int n = Math.Min(a.Count, b.Count);
            for (int i = 0; i < n; i++)
            {
                int c = a[i].CompareTo(b[i]);
                if (c != 0) return c < 0;
            }
            return a.Count < b.Count;
        }
    }
}
