using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 战场（GDD_021 R1 / ADR-0012 D2）：命名区域集 + <b>邻接图</b>（决定支队可调动去向），<b>非坐标网格</b>。
    /// MVP 固定 5 区模板（<see cref="Default"/>），<b>预留场景注入自定义战场</b>（构造函数接受任意区域/邻接，数据驱动）。
    /// 不可变。区域按 id 规范序；邻接为无向、以规范无序对存储 → 确定性哈希。
    /// </summary>
    public sealed class BattleField
    {
        private readonly List<Zone> _zones;                          // 按 ZoneId 序数排序
        private readonly Dictionary<string, Zone> _byId;
        private readonly HashSet<string> _adjacency;                 // 规范无序对 "a|b"（a<b 序数）

        /// <summary>区域集（按 id 规范序）。</summary>
        public IReadOnlyList<Zone> Zones => _zones;

        public BattleField(IReadOnlyList<Zone> zones, IReadOnlyList<(ZoneId, ZoneId)> adjacencies)
        {
            if (zones == null || zones.Count == 0) throw new ArgumentException("战场至少一个区域。", nameof(zones));

            _byId = new Dictionary<string, Zone>(StringComparer.Ordinal);
            foreach (Zone z in zones)
            {
                if (_byId.ContainsKey(z.Id.Value)) throw new ArgumentException($"区域 id 重复：{z.Id.Value}", nameof(zones));
                _byId[z.Id.Value] = z;
            }

            _zones = new List<Zone>(zones);
            _zones.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));

            _adjacency = new HashSet<string>(StringComparer.Ordinal);
            if (adjacencies != null)
                foreach ((ZoneId a, ZoneId b) in adjacencies)
                {
                    if (!_byId.ContainsKey(a.Value) || !_byId.ContainsKey(b.Value))
                        throw new ArgumentException("邻接引用了不存在的区域。", nameof(adjacencies));
                    if (a == b) continue;                            // 自邻忽略
                    _adjacency.Add(Key(a.Value, b.Value));
                }
        }

        private static string Key(string a, string b)
            => string.CompareOrdinal(a, b) <= 0 ? a + "|" + b : b + "|" + a;

        /// <summary>是否含某区。</summary>
        public bool Contains(ZoneId id) => id.Value != null && _byId.ContainsKey(id.Value);

        /// <summary>取某区（不存在则抛）。</summary>
        public Zone ZoneOf(ZoneId id)
            => _byId.TryGetValue(id.Value ?? string.Empty, out Zone? z) ? z : throw new KeyNotFoundException($"无此区域：{id}");

        /// <summary>两区是否相邻（调动合法性用；无向）。</summary>
        public bool AreAdjacent(ZoneId a, ZoneId b)
            => a.Value != null && b.Value != null && a != b && _adjacency.Contains(Key(a.Value, b.Value));

        /// <summary>某区的相邻区（按 id 规范序）。</summary>
        public IReadOnlyList<ZoneId> Neighbors(ZoneId id)
        {
            var result = new List<ZoneId>();
            foreach (Zone z in _zones)
                if (z.Id != id && AreAdjacent(id, z.Id)) result.Add(z.Id);
            return result;
        }

        internal void AppendTo(StateHasher hasher)
        {
            hasher.Append(_zones.Count);
            foreach (Zone z in _zones) z.AppendTo(hasher);
            var keys = new List<string>(_adjacency);
            keys.Sort(StringComparer.Ordinal);
            hasher.Append(keys.Count);
            foreach (string k in keys) ZoneHashing.AppendString(hasher, k);
        }

        // ---- 默认 5 区战场模板（GDD_021 §16 MVP；场景自定义为 Future）----

        /// <summary>正面关城（攻坚硬碰）。</summary>
        public static readonly ZoneId Front = new ZoneId("zone-front");
        /// <summary>侧翼隘口（设伏诱敌）。</summary>
        public static readonly ZoneId Flank = new ZoneId("zone-flank");
        /// <summary>敌粮道（长围断粮）。</summary>
        public static readonly ZoneId Supply = new ZoneId("zone-supply");
        /// <summary>遮蔽高地/林（隐蔽夜袭）。</summary>
        public static readonly ZoneId Cover = new ZoneId("zone-cover");
        /// <summary>预备后方（预备队/退路）。</summary>
        public static readonly ZoneId Reserve = new ZoneId("zone-reserve");

        /// <summary>
        /// 默认战场（虎牢关攻/汜水关守通用模板）：5 区 + 邻接图。各区条件禀赋对应现有兵法链（复用，零新造）。
        /// </summary>
        public static BattleField Default()
        {
            var zones = new[]
            {
                new Zone(Front, TerrainKind.Fortified, new[]
                {
                    // 城门正面可诈降赚城（黄盖诈降）：诈降禀赋。
                    TacticCondition.SurrenderFeigned, TacticCondition.EnemyLuredOpen, TacticCondition.StrikeFromWithin,
                }, softCapacity: 800),
                new Zone(Flank, TerrainKind.Pass, new[]
                {
                    TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued, TacticCondition.AmbushSurprise,
                }, softCapacity: 400),
                new Zone(Supply, TerrainKind.Plain, new[]
                {
                    TacticCondition.SupplyLineCut, TacticCondition.ShortageReachedGrace, TacticCondition.EnemyCohesionCrossedThreshold,
                    // 粮营易燃（乌巢烧粮/赤壁烧船）：火攻禀赋。
                    TacticCondition.DryField, TacticCondition.EnemyExposedToFire, TacticCondition.FireIgnited,
                    // 粮道近水低地（水淹七军）：水攻禀赋。
                    TacticCondition.EnemyInLowGround, TacticCondition.WaterworksHeld, TacticCondition.FloodReleased,
                }, softCapacity: 300),
                new Zone(Cover, TerrainKind.Cover, new[]
                {
                    TacticCondition.IsNight, TacticCondition.StealthSuccess, TacticCondition.DefenderUnaware, TacticCondition.RaiderDisciplineMet,
                    // 林莽/连营易燃（火烧连营）：火攻禀赋。
                    TacticCondition.DryField, TacticCondition.EnemyExposedToFire, TacticCondition.FireIgnited,
                    // 掩护侧翼利机动伏援（围点打援）：禀赋。
                    TacticCondition.PointBesieged, TacticCondition.ReliefIntercepted, TacticCondition.AmbushOnRoute,
                }, softCapacity: 300),
                new Zone(Reserve, TerrainKind.Plain, Array.Empty<TacticCondition>(), softCapacity: 500),
            };
            var adj = new[]
            {
                (Front, Flank), (Front, Cover), (Front, Reserve),
                (Flank, Supply), (Cover, Supply), (Cover, Reserve),
            };
            return new BattleField(zones, adj);
        }
    }
}
