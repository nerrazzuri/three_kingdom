using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Map
{
    /// <summary>
    /// 知识来源（对齐 epic-005 情报四层契约 TR-intel-001：来源决定可信层级）。
    /// 时效/置信衰减权威归 GDD_007（本层只记 <see cref="RegionKnowledge.ObservedTime"/>）。
    /// </summary>
    public enum KnowledgeSource
    {
        /// <summary>己方领地（事实层）。</summary>
        OwnTerritory = 0,

        /// <summary>亲自直接观察（事实层）。</summary>
        DirectObservation = 1,

        /// <summary>侦察报告（推测层）。</summary>
        Scouting = 2,

        /// <summary>传闻（最低可信层）。</summary>
        Rumor = 3,
    }

    /// <summary>
    /// 阵营对单区域的<b>已知</b>信息（GDD_003 §Data Model：MapKnowledge）。仅含阵营合法知道的字段 +
    /// 最后观察时间 + 来源——<b>不引用、不携带任何真值</b>。不可变值。
    /// </summary>
    public readonly struct RegionKnowledge
    {
        /// <summary>区域。</summary>
        public RegionId Region { get; }

        /// <summary>已知控制方。</summary>
        public FactionId KnownController { get; }

        /// <summary>已知驻军（观察时的估计/记录值，非真值引用）。</summary>
        public int KnownGarrison { get; }

        /// <summary>最后观察时间（事实 vs 推测判定复用 GDD_007，不在此另立 TTL）。</summary>
        public WorldTime ObservedTime { get; }

        /// <summary>信息来源层级。</summary>
        public KnowledgeSource Source { get; }

        public RegionKnowledge(RegionId region, FactionId knownController, int knownGarrison, WorldTime observedTime, KnowledgeSource source)
        {
            if (knownGarrison < 0) throw new ArgumentOutOfRangeException(nameof(knownGarrison), "已知驻军不可为负。");
            if (!Enum.IsDefined(typeof(KnowledgeSource), source)) throw new ArgumentOutOfRangeException(nameof(source));
            Region = region;
            KnownController = knownController;
            KnownGarrison = knownGarrison;
            ObservedTime = observedTime;
            Source = source;
        }
    }

    /// <summary>
    /// 阵营知识表（GDD_003 / TR-map-003）。与 <see cref="MapTruth"/> 独立存储。
    /// 仅经 <see cref="ApplyObservation"/>（侦察/观察）更新——敌区信息<b>只能由侦察更新</b>，
    /// 不随控制权变更自动揭示（AC-2）。<see cref="Project"/> 导出只读投影供显示层（不触真值，AC-3）。
    /// </summary>
    public sealed class FactionKnowledge
    {
        private readonly Dictionary<RegionId, RegionKnowledge> _known;

        /// <summary>所属阵营。</summary>
        public FactionId Faction { get; }

        public FactionKnowledge(FactionId faction)
        {
            Faction = faction;
            _known = new Dictionary<RegionId, RegionKnowledge>();
        }

        /// <summary>是否已知某区域。</summary>
        public bool Knows(RegionId region) => _known.ContainsKey(region);

        /// <summary>尝试取已知信息。</summary>
        public bool TryGet(RegionId region, out RegionKnowledge knowledge) => _known.TryGetValue(region, out knowledge);

        /// <summary>
        /// 应用一次观察（侦察/直接观察产出），写入或覆盖该区域知识。<b>只更新知识</b>，不接触真值。
        /// </summary>
        public void ApplyObservation(RegionKnowledge observation) => _known[observation.Region] = observation;

        /// <summary>导出只读知识投影（快照），供显示层使用——结构上仅含知识字段，无真值。</summary>
        public MapKnowledgeProjection Project() => new MapKnowledgeProjection(_known.Values);
    }

    /// <summary>
    /// 阵营知识的只读投影（供 UI/显示层）。从 <see cref="FactionKnowledge"/> 快照构造，
    /// 结构上<b>不含真值字段</b>（control-manifest：显示层用只读投影，禁读真值）。构造后不可变。
    /// </summary>
    public sealed class MapKnowledgeProjection
    {
        private readonly Dictionary<RegionId, RegionKnowledge> _entries;

        internal MapKnowledgeProjection(IEnumerable<RegionKnowledge> entries)
        {
            _entries = new Dictionary<RegionId, RegionKnowledge>();
            foreach (var e in entries) _entries[e.Region] = e;
        }

        /// <summary>已知区域数。</summary>
        public int Count => _entries.Count;

        /// <summary>投影是否含某区域（仅当阵营已知）。</summary>
        public bool Contains(RegionId region) => _entries.ContainsKey(region);

        /// <summary>尝试取投影中的已知信息。</summary>
        public bool TryGet(RegionId region, out RegionKnowledge knowledge) => _entries.TryGetValue(region, out knowledge);
    }
}
