using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 单区域的地图真值（GDD_003 §Main Rules：地图真值与阵营知识分离）。权威结构——区域真实归属与驻军。
    /// 仅经 <see cref="MapTruth"/> 的权威方法修改；<b>修改真值绝不触碰任何阵营知识</b>（ADR-0002 唯一权威来源）。
    /// </summary>
    public sealed class RegionTruth
    {
        /// <summary>区域。</summary>
        public RegionId Region { get; }

        /// <summary>真实控制方。</summary>
        public FactionId Controller { get; internal set; }

        /// <summary>真实驻军兵力（≥0）。</summary>
        public int Garrison { get; internal set; }

        internal RegionTruth(RegionId region, FactionId controller, int garrison)
        {
            if (garrison < 0) throw new ArgumentOutOfRangeException(nameof(garrison), "驻军不可为负。");
            Region = region;
            Controller = controller;
            Garrison = garrison;
        }
    }

    /// <summary>
    /// 地图真值表（GDD_003 §Data Model）。区域真实归属/驻军的<b>唯一权威来源</b>。
    /// 控制权/驻军变更经此（权威路径）；<b>不</b>自动揭示给任何阵营知识（敌区信息须由侦察更新，AC-2）。
    /// </summary>
    public sealed class MapTruth
    {
        private readonly Dictionary<RegionId, RegionTruth> _truth;

        public MapTruth(IEnumerable<RegionTruth> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            _truth = new Dictionary<RegionId, RegionTruth>();
            foreach (var e in entries)
            {
                if (e == null) throw new ArgumentException("真值条目不可为 null。", nameof(entries));
                if (_truth.ContainsKey(e.Region))
                    throw new ArgumentException($"重复区域真值：{e.Region}。", nameof(entries));
                _truth[e.Region] = e;
            }
        }

        /// <summary>新建一条区域真值（构建期辅助）。</summary>
        public static RegionTruth Entry(RegionId region, FactionId controller, int garrison)
            => new RegionTruth(region, controller, garrison);

        /// <summary>取区域真值；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public RegionTruth Region(RegionId id)
        {
            if (_truth.TryGetValue(id, out var t)) return t;
            throw new KeyNotFoundException($"无区域真值：{id}。");
        }

        /// <summary>权威更改控制方（GDD §Main Rules：控制权改变不自动揭示敌情——本方法不触知识）。</summary>
        public void SetController(RegionId id, FactionId controller) => Region(id).Controller = controller;

        /// <summary>权威更改驻军（不触知识）。</summary>
        public void SetGarrison(RegionId id, int garrison)
        {
            if (garrison < 0) throw new ArgumentOutOfRangeException(nameof(garrison), "驻军不可为负。");
            Region(id).Garrison = garrison;
        }
    }
}
