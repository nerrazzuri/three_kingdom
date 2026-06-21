using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 世界地图（GDD_003 §Data Model：WorldMap）：区域节点 + 路线边构成的拓扑图，<b>视觉坐标不参与 Domain 结算</b>（AC-1）。
    /// 构造时校验（GDD §Failure：重复稳定 ID / 悬空路线 → 结构化拒绝，不建图）：
    /// 区域/路线 ID 唯一；每条路线端点须为已存在区域。构造后不可变；提供按 RouteId 序的邻接遍历（确定性寻路所需）。
    /// </summary>
    public sealed class WorldMap
    {
        private readonly Dictionary<RegionId, Region> _regions;
        private readonly Dictionary<RouteId, Route> _routes;
        private readonly Dictionary<RegionId, List<Route>> _adjacency;

        public WorldMap(IReadOnlyList<Region> regions, IReadOnlyList<Route> routes)
        {
            if (regions == null) throw new ArgumentNullException(nameof(regions));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            _regions = new Dictionary<RegionId, Region>();
            foreach (var region in regions)
            {
                if (region == null) throw new ArgumentException("区域不可为 null。", nameof(regions));
                if (_regions.ContainsKey(region.Id))
                    throw new ArgumentException($"重复区域稳定 ID：{region.Id}。", nameof(regions));
                _regions[region.Id] = region;
            }

            _routes = new Dictionary<RouteId, Route>();
            _adjacency = new Dictionary<RegionId, List<Route>>();
            foreach (var route in routes)
            {
                if (route == null) throw new ArgumentException("路线不可为 null。", nameof(routes));
                if (_routes.ContainsKey(route.Id))
                    throw new ArgumentException($"重复路线稳定 ID：{route.Id}。", nameof(routes));
                if (!_regions.ContainsKey(route.From))
                    throw new ArgumentException($"悬空路线 {route.Id}：起区域 {route.From} 不存在。", nameof(routes));
                if (!_regions.ContainsKey(route.To))
                    throw new ArgumentException($"悬空路线 {route.Id}：终区域 {route.To} 不存在。", nameof(routes));
                _routes[route.Id] = route;
                AddAdjacency(route.From, route);
                if (route.Bidirectional) AddAdjacency(route.To, route);
            }

            // 邻接表按 RouteId 序固定，保证寻路遍历确定（不依赖字典遍历序）。
            foreach (var list in _adjacency.Values)
                list.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private void AddAdjacency(RegionId region, Route route)
        {
            if (!_adjacency.TryGetValue(region, out var list))
            {
                list = new List<Route>();
                _adjacency[region] = list;
            }
            list.Add(route);
        }

        /// <summary>区域数。</summary>
        public int RegionCount => _regions.Count;

        /// <summary>路线数。</summary>
        public int RouteCount => _routes.Count;

        /// <summary>是否含区域。</summary>
        public bool HasRegion(RegionId id) => _regions.ContainsKey(id);

        /// <summary>取区域；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public Region Region(RegionId id)
        {
            if (_regions.TryGetValue(id, out var r)) return r;
            throw new KeyNotFoundException($"无区域：{id}。");
        }

        /// <summary>取路线；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public Route Route(RouteId id)
        {
            if (_routes.TryGetValue(id, out var r)) return r;
            throw new KeyNotFoundException($"无路线：{id}。");
        }

        /// <summary>从 <paramref name="region"/> 可出发的路线（按 RouteId 升序；无则空）。</summary>
        public IReadOnlyList<Route> RoutesFrom(RegionId region)
            => _adjacency.TryGetValue(region, out var list) ? list : (IReadOnlyList<Route>)Array.Empty<Route>();
    }
}
