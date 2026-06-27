using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 条件历史世界模型的权威状态（GDD_015 §Data Model：WorldState / TR-world-001 / ADR-0002 + ADR-0004）。
    /// 不可变聚合：当前世界时间（GDD_001）、各势力记录、各城归属只读投影、已触发/已分叉历史事件集合。
    /// 推进经 <see cref="WorldProgressionService"/> 产出新实例，<b>不</b>就地修改；纳入状态哈希（确定性，存档奠基）。
    /// <para>
    /// 时间唯一来自 <see cref="WorldTime"/>（不依赖帧率/Unity 时间）。城池归属为<b>只读反映</b>，
    /// 写权威在 GDD_004（story-004 订阅接入）；历史事件触发逻辑属 story-002，本骨架仅持有集合与确定性时间推进。
    /// </para>
    /// </summary>
    public sealed class WorldState
    {
        private readonly FactionRecord[] _factions;       // 按 FactionId 序数升序
        private readonly CityOwnership[] _cities;          // 按 CityId 序数升序
        private readonly string[] _triggeredEvents;        // 已触发事件 id，序数升序去重
        private readonly string[] _divergedEvents;         // 已分叉事件 id，序数升序去重

        /// <summary>当前世界时间（GDD_001）。</summary>
        public WorldTime CurrentTime { get; }

        /// <summary>各势力记录（按 ID 序数升序）。</summary>
        public IReadOnlyList<FactionRecord> Factions => _factions;

        /// <summary>各城归属只读投影（按 City ID 序数升序）。</summary>
        public IReadOnlyList<CityOwnership> Cities => _cities;

        /// <summary>已触发历史事件 id 集合（序数升序）。</summary>
        public IReadOnlyList<string> TriggeredEvents => _triggeredEvents;

        /// <summary>已分叉历史事件 id 集合（序数升序）。</summary>
        public IReadOnlyList<string> DivergedEvents => _divergedEvents;

        /// <summary>
        /// 构造世界状态。集合各自规范排序去重；事件集合允许为空（单势力初态合法）。
        /// </summary>
        public WorldState(
            WorldTime currentTime,
            IReadOnlyList<FactionRecord> factions,
            IReadOnlyList<CityOwnership> cities,
            IReadOnlyCollection<string> triggeredEvents,
            IReadOnlyCollection<string> divergedEvents)
        {
            if (factions is null) throw new ArgumentNullException(nameof(factions));
            if (cities is null) throw new ArgumentNullException(nameof(cities));
            if (triggeredEvents is null) throw new ArgumentNullException(nameof(triggeredEvents));
            if (divergedEvents is null) throw new ArgumentNullException(nameof(divergedEvents));

            CurrentTime = currentTime;

            var sortedFactions = new List<FactionRecord>(factions);
            sortedFactions.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));
            for (int i = 1; i < sortedFactions.Count; i++)
                if (sortedFactions[i].Id == sortedFactions[i - 1].Id)
                    throw new ArgumentException($"势力记录重复：{sortedFactions[i].Id}。", nameof(factions));
            _factions = sortedFactions.ToArray();

            var sortedCities = new List<CityOwnership>(cities);
            sortedCities.Sort((a, b) => string.CompareOrdinal(a.City.Value, b.City.Value));
            for (int i = 1; i < sortedCities.Count; i++)
                if (sortedCities[i].City == sortedCities[i - 1].City)
                    throw new ArgumentException($"城池归属投影重复：{sortedCities[i].City}。", nameof(cities));
            _cities = sortedCities.ToArray();

            _triggeredEvents = SortedDistinct(triggeredEvents, nameof(triggeredEvents));
            _divergedEvents = SortedDistinct(divergedEvents, nameof(divergedEvents));
        }

        /// <summary>查询某势力记录；不存在返回 null。</summary>
        public FactionRecord? FactionById(FactionId id)
        {
            foreach (FactionRecord f in _factions)
                if (f.Id == id) return f;
            return null;
        }

        /// <summary>查询某城归属只读投影；不存在返回 null。</summary>
        public CityOwnership? OwnershipOf(CityId city)
        {
            foreach (CityOwnership c in _cities)
                if (c.City == city) return c;
            return null;
        }

        /// <summary>产出时间推进后的新状态（仅供推进服务使用；其余字段不变，维持不可变与单一推进路径）。</summary>
        internal WorldState WithTime(WorldTime newTime)
            => new WorldState(newTime, _factions, _cities, _triggeredEvents, _divergedEvents);

        /// <summary>该事件 id 是否已触发。</summary>
        public bool IsTriggered(string eventId)
        {
            foreach (string s in _triggeredEvents)
                if (string.Equals(s, eventId, StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>该事件 id 是否已分叉。</summary>
        public bool IsDiverged(string eventId)
        {
            foreach (string s in _divergedEvents)
                if (string.Equals(s, eventId, StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>
        /// 产出"已触发某历史事件"后的新状态（仅供历史推进服务使用，story-002）。
        /// <paramref name="diverged"/>=true 时同时计入已分叉集合。调用方须先确认未触发（构造去重，重复将抛）。
        /// </summary>
        internal WorldState WithTriggeredEvent(string eventId, bool diverged)
        {
            var triggered = new List<string>(_triggeredEvents) { eventId };
            var divergedSet = new List<string>(_divergedEvents);
            if (diverged) divergedSet.Add(eventId);
            return new WorldState(CurrentTime, _factions, _cities, triggered, divergedSet);
        }

        /// <summary>
        /// 以规范顺序追加到状态哈希（ADR-0004）。顺序：时间.AbsoluteIndex → 势力数 + 各势力
        /// → 城池数 + 各城(City 长度+字符, owner 有无+长度+字符, 守备) → 已触发集合 → 已分叉集合。
        /// </summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append(CurrentTime.AbsoluteIndex);

            hasher.Append(_factions.Length);
            foreach (FactionRecord f in _factions) f.AppendTo(hasher);

            hasher.Append(_cities.Length);
            foreach (CityOwnership c in _cities)
            {
                AppendString(hasher, c.City.Value);
                hasher.Append(c.Owner.HasValue);
                AppendString(hasher, c.Owner.HasValue ? c.Owner.Value.Value : string.Empty);
                hasher.Append(c.Garrison);
            }

            AppendStringSet(hasher, _triggeredEvents);
            AppendStringSet(hasher, _divergedEvents);
        }

        /// <summary>计算本世界状态的确定性哈希。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            AppendTo(hasher);
            return hasher.ToHash();
        }

        private static string[] SortedDistinct(IReadOnlyCollection<string> source, string paramName)
        {
            var list = new List<string>(source.Count);
            foreach (string s in source)
            {
                if (string.IsNullOrWhiteSpace(s))
                    throw new ArgumentException("事件 id 不可为空或空白。", paramName);
                list.Add(s);
            }
            list.Sort(StringComparer.Ordinal);
            for (int i = 1; i < list.Count; i++)
                if (string.Equals(list[i], list[i - 1], StringComparison.Ordinal))
                    throw new ArgumentException($"事件 id 重复：{list[i]}。", paramName);
            return list.ToArray();
        }

        private static void AppendStringSet(StateHasher hasher, string[] set)
        {
            hasher.Append(set.Length);
            foreach (string s in set) AppendString(hasher, s);
        }

        private static void AppendString(StateHasher hasher, string value)
        {
            hasher.Append(value.Length);
            foreach (char ch in value) hasher.Append((int)ch);
        }
    }
}
