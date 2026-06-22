using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 校验上下文（GDD_009 §Formula 1：可达/资源/权限的外部事实快照）。
    /// 由调用方从地图（可达性）、城市/后勤（可承诺资源）与权限（已授权命令）汇集；
    /// 校验器据此做纯函数判定（不反向耦合各源系统）。不可变。
    /// </summary>
    public sealed class PreparationContext
    {
        private readonly HashSet<RegionId> _reachable;
        private readonly Dictionary<ResourceKey, long> _available;
        private readonly HashSet<OrderId> _authorized;

        public PreparationContext(
            IEnumerable<RegionId> reachableRegions,
            IReadOnlyDictionary<ResourceKey, long> availableResources,
            IEnumerable<OrderId> authorizedOrders)
        {
            if (reachableRegions == null) throw new ArgumentNullException(nameof(reachableRegions));
            if (availableResources == null) throw new ArgumentNullException(nameof(availableResources));
            if (authorizedOrders == null) throw new ArgumentNullException(nameof(authorizedOrders));

            _reachable = new HashSet<RegionId>(reachableRegions);
            _available = new Dictionary<ResourceKey, long>();
            foreach (KeyValuePair<ResourceKey, long> kv in availableResources)
            {
                if (kv.Value < 0) throw new ArgumentOutOfRangeException(nameof(availableResources), "可承诺资源不可为负。");
                _available[kv.Key] = kv.Value;
            }
            _authorized = new HashSet<OrderId>(authorizedOrders);
        }

        /// <summary>目标区域是否可达（GDD_003）。</summary>
        public bool IsReachable(RegionId region) => _reachable.Contains(region);

        /// <summary>某资源当前可承诺量（无记录视为 0）。</summary>
        public long Available(ResourceKey key) => _available.TryGetValue(key, out long v) ? v : 0L;

        /// <summary>该命令的执行者是否具备执行权限（GDD_005/006）。</summary>
        public bool IsAuthorized(OrderId order) => _authorized.Contains(order);
    }
}
