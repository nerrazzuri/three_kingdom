using System;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 区域节点（GDD_003 §Data Model：Region）。本 story 持其拓扑与容量契约（地形/控制/设施留各 epic）。
    /// 容量门控见 <see cref="CanAccept"/>（GDD §Formula 3）。不可变值。
    /// </summary>
    public sealed class Region
    {
        /// <summary>区域稳定 ID。</summary>
        public RegionId Id { get; }

        /// <summary>区域容量（≥0；0 表示不可驻留）。</summary>
        public int Capacity { get; }

        public Region(RegionId id, int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "区域容量不可为负。");
            Id = id;
            Capacity = capacity;
        }

        /// <summary>
        /// 容量门控（GDD §Formula 3：can_enter = occ + n ≤ cap）。超容返回 false（不静默叠加）。
        /// </summary>
        public bool CanAccept(int currentOccupancy, int incoming)
        {
            if (currentOccupancy < 0) throw new ArgumentOutOfRangeException(nameof(currentOccupancy), "当前占用不可为负。");
            if (incoming < 0) throw new ArgumentOutOfRangeException(nameof(incoming), "进入数不可为负。");
            return (long)currentOccupancy + incoming <= Capacity;
        }

        public override string ToString() => $"Region({Id}) cap={Capacity}";
    }
}
