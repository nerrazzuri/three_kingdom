using System;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 路线边（GDD_003 §Data Model：Route）。连接两区域，含方向性与基础耗时。不可变值。
    /// 双向路线才可能产生相向接触（GDD §Formula 5）。
    /// </summary>
    public sealed class Route
    {
        /// <summary>路线稳定 ID。</summary>
        public RouteId Id { get; }

        /// <summary>起区域。</summary>
        public RegionId From { get; }

        /// <summary>终区域。</summary>
        public RegionId To { get; }

        /// <summary>是否双向通行（false 即仅 From→To）。</summary>
        public bool Bidirectional { get; }

        /// <summary>基础通行耗时（≥1 时段，配置）。</summary>
        public int BaseTime { get; }

        public Route(RouteId id, RegionId from, RegionId to, bool bidirectional, int baseTime)
        {
            if (from == to)
                throw new ArgumentException("路线起终区域不可相同。", nameof(to));
            if (baseTime < 1)
                throw new ArgumentOutOfRangeException(nameof(baseTime), "路线基础耗时须 ≥ 1。");
            Id = id;
            From = from;
            To = to;
            Bidirectional = bidirectional;
            BaseTime = baseTime;
        }

        /// <summary>是否可从 <paramref name="region"/> 沿本路线出发（双向则两端皆可，单向仅 From）。</summary>
        public bool TraversableFrom(RegionId region)
            => From == region || (Bidirectional && To == region);

        /// <summary>给定出发端，返回另一端。出发端非本路线端点抛 <see cref="ArgumentException"/>。</summary>
        public RegionId Other(RegionId region)
        {
            if (From == region) return To;
            if (To == region) return From;
            throw new ArgumentException($"区域 {region} 非路线 {Id} 的端点。", nameof(region));
        }

        public override string ToString() => $"Route({Id}) {From}{(Bidirectional ? "<->" : "->")}{To} t={BaseTime}";
    }
}
