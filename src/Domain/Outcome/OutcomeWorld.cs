using System;
using System.Collections.Generic;
using System.Text;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Relationships;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>方向性关系键 <c>(from, to, dim)</c>（A→B 与 B→A 独立，P6 多维不合并）。</summary>
    public readonly struct RelationshipKey : IEquatable<RelationshipKey>
    {
        /// <summary>来源人物。</summary>
        public CharacterId From { get; }

        /// <summary>目标人物。</summary>
        public CharacterId To { get; }

        /// <summary>关系维度。</summary>
        public RelationshipDimension Dim { get; }

        public RelationshipKey(CharacterId from, CharacterId to, RelationshipDimension dim)
        {
            From = from; To = to; Dim = dim;
        }

        public bool Equals(RelationshipKey other) => From == other.From && To == other.To && Dim == other.Dim;
        public override bool Equals(object? obj) => obj is RelationshipKey other && Equals(other);
        public override int GetHashCode()
        {
            int h = From.GetHashCode();
            h = (h * 397) ^ To.GetHashCode();
            h = (h * 397) ^ (int)Dim;
            return h;
        }
    }

    /// <summary>
    /// 后果写回作用域内的跨系统权威状态快照（gdd-010 §后果 / ADR-0002）。
    /// <b>不可变</b>聚合：城市经济、阵营名声、人物计量、方向性关系四类权威状态的只读集合。
    /// 后果结算<b>只生成新实例</b>（<see cref="OutcomeWritebackService"/>），从不就地修改——
    /// 失败时调用方持有的原快照原样不变（零部分写入，确定性可复盘 ADR-0004）。
    /// </summary>
    public sealed class OutcomeWorld
    {
        private readonly Dictionary<CityId, CityEconomyState> _cities;
        private readonly Dictionary<FactionId, long> _reputation;
        private readonly Dictionary<CharacterId, long> _characterVitality;
        private readonly Dictionary<RelationshipKey, int> _relationships;

        internal OutcomeWorld(
            Dictionary<CityId, CityEconomyState> cities,
            Dictionary<FactionId, long> reputation,
            Dictionary<CharacterId, long> characterVitality,
            Dictionary<RelationshipKey, int> relationships)
        {
            _cities = cities;
            _reputation = reputation;
            _characterVitality = characterVitality;
            _relationships = relationships;
        }

        /// <summary>空世界（无任何状态）。</summary>
        public static OutcomeWorld Empty => new OutcomeWorld(
            new Dictionary<CityId, CityEconomyState>(),
            new Dictionary<FactionId, long>(),
            new Dictionary<CharacterId, long>(),
            new Dictionary<RelationshipKey, int>());

        /// <summary>登记一座城市的初始权威状态（返回新实例；不修改原实例）。</summary>
        public OutcomeWorld WithCity(CityEconomyState city)
        {
            if (city == null) throw new ArgumentNullException(nameof(city));
            var cities = new Dictionary<CityId, CityEconomyState>(_cities) { [city.Id] = city };
            return new OutcomeWorld(cities, Copy(_reputation), Copy(_characterVitality), Copy(_relationships));
        }

        /// <summary>登记一个阵营名声初值。</summary>
        public OutcomeWorld WithReputation(FactionId faction, long score)
        {
            var rep = Copy(_reputation); rep[faction] = score;
            return new OutcomeWorld(Copy(_cities), rep, Copy(_characterVitality), Copy(_relationships));
        }

        /// <summary>登记一个人物计量初值（≥0）。</summary>
        public OutcomeWorld WithCharacter(CharacterId character, long vitality)
        {
            if (vitality < 0) throw new ArgumentOutOfRangeException(nameof(vitality), "人物计量不可为负。");
            var v = Copy(_characterVitality); v[character] = vitality;
            return new OutcomeWorld(Copy(_cities), Copy(_reputation), v, Copy(_relationships));
        }

        /// <summary>登记一个关系维度初值（刻度内）。</summary>
        public OutcomeWorld WithRelationship(RelationshipKey key, int value)
        {
            var r = Copy(_relationships); r[key] = RelationshipScale.Clamp(value);
            return new OutcomeWorld(Copy(_cities), Copy(_reputation), Copy(_characterVitality), r);
        }

        /// <summary>是否含某城市。</summary>
        public bool HasCity(CityId id) => _cities.ContainsKey(id);
        /// <summary>取城市状态；不存在抛 <see cref="KeyNotFoundException"/>。</summary>
        public CityEconomyState GetCity(CityId id) => _cities[id];

        /// <summary>是否含某阵营名声。</summary>
        public bool HasReputation(FactionId id) => _reputation.ContainsKey(id);
        /// <summary>取阵营名声。</summary>
        public long GetReputation(FactionId id) => _reputation[id];

        /// <summary>是否含某人物计量。</summary>
        public bool HasCharacter(CharacterId id) => _characterVitality.ContainsKey(id);
        /// <summary>取人物计量。</summary>
        public long GetCharacterVitality(CharacterId id) => _characterVitality[id];

        /// <summary>是否含某关系键。</summary>
        public bool HasRelationship(RelationshipKey key) => _relationships.ContainsKey(key);
        /// <summary>取关系维度值；未记录则中性值。</summary>
        public int GetRelationship(RelationshipKey key)
            => _relationships.TryGetValue(key, out int v) ? v : RelationshipScale.Neutral;

        // —— 内部：供写回服务读取工作副本 ——
        internal Dictionary<CityId, CityEconomyState> CitiesCopy() => Copy(_cities);
        internal Dictionary<FactionId, long> ReputationCopy() => Copy(_reputation);
        internal Dictionary<CharacterId, long> CharacterVitalityCopy() => Copy(_characterVitality);
        internal Dictionary<RelationshipKey, int> RelationshipsCopy() => Copy(_relationships);

        private static Dictionary<TK, TV> Copy<TK, TV>(Dictionary<TK, TV> src) => new Dictionary<TK, TV>(src);

        /// <summary>
        /// 确定性状态哈希（ADR-0004）：以规范顺序遍历四类状态。
        /// 城市按 id 序数升序、名声按阵营序数升序、人物按 id 序数升序、关系按 (from,to,dim) 序数升序。
        /// 同一内容（与登记/写回顺序无关）→ 同一哈希；任一字段变化 → 哈希变化。
        /// </summary>
        public StateHash ComputeHash()
        {
            var h = new StateHasher();

            var cityIds = new List<CityId>(_cities.Keys);
            cityIds.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            h.Append(cityIds.Count);
            foreach (var id in cityIds)
            {
                var c = _cities[id];
                AppendString(h, id.Value);
                h.Append(c.Stock); h.Append(c.Reserved);
                h.Append(c.CivMorale); h.Append(c.Security);
                h.Append(c.FortificationCurrent); h.Append(c.FortificationMax);
            }

            var facs = new List<FactionId>(_reputation.Keys);
            facs.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            h.Append(facs.Count);
            foreach (var f in facs) { AppendString(h, f.Value); h.Append(_reputation[f]); }

            var chars = new List<CharacterId>(_characterVitality.Keys);
            chars.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            h.Append(chars.Count);
            foreach (var ch in chars) { AppendString(h, ch.Value); h.Append(_characterVitality[ch]); }

            var rels = new List<RelationshipKey>(_relationships.Keys);
            rels.Sort((a, b) =>
            {
                int byFrom = string.CompareOrdinal(a.From.Value, b.From.Value);
                if (byFrom != 0) return byFrom;
                int byTo = string.CompareOrdinal(a.To.Value, b.To.Value);
                if (byTo != 0) return byTo;
                return ((int)a.Dim).CompareTo((int)b.Dim);
            });
            h.Append(rels.Count);
            foreach (var k in rels)
            {
                AppendString(h, k.From.Value); AppendString(h, k.To.Value);
                h.Append((int)k.Dim); h.Append(_relationships[k]);
            }

            return h.ToHash();
        }

        private static void AppendString(StateHasher h, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            h.Append(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) h.Append(bytes[i]);
        }
    }
}
