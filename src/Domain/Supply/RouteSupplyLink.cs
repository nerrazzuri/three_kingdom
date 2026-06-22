using System;
using System.Collections.Generic;
using System.Linq;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 补给路线链路判定（GDD_012 §Main Rules：补给路线是 GDD_003 路线的有序链 / TR-supply-002）。
    /// 断粮<b>由拓扑切断派生</b>——只要补给链中任一路线被切断则交付中断，
    /// <b>不存在「断粮成功」按钮</b>（玩家不能直接把交付设为失败，只能切断实际路线）。
    /// 纯静态判定，确定性。
    /// </summary>
    public static class RouteSupplyLink
    {
        /// <summary>
        /// 判定补给链是否可交付：路线序列中所有路线均未被切断方为通（GDD_012 §Main Rules）。
        /// </summary>
        /// <param name="routeSequence">运输沿 GDD_003 路线的有序链（非空）。</param>
        /// <param name="severedRoutes">当前被切断的路线集合（实际拓扑事实，非按钮）。</param>
        /// <returns>链路全通返回 true；任一段被切断返回 false。</returns>
        public static bool IsDeliverable(IReadOnlyList<RouteId> routeSequence, IReadOnlyCollection<RouteId> severedRoutes)
        {
            if (routeSequence == null) throw new ArgumentNullException(nameof(routeSequence));
            if (severedRoutes == null) throw new ArgumentNullException(nameof(severedRoutes));
            if (routeSequence.Count == 0) throw new ArgumentException("补给路线链不可为空。", nameof(routeSequence));

            foreach (RouteId route in routeSequence)
                if (severedRoutes.Contains(route))
                    return false;
            return true;
        }
    }
}
