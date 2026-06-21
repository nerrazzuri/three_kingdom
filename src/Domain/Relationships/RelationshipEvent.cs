using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Relationships
{
    /// <summary>
    /// 具名关系事件（GDD_006 §Data Model：RelationshipEvent / §Formula 1）。
    /// 带<b>稳定事件 ID</b>（幂等去重键）；只对<b>知情者</b>生效，改变其对 <see cref="Target"/> 的各维度关系。
    /// 不可变；构造时防御性拷贝知情者与变化量。
    /// </summary>
    public sealed class RelationshipEvent
    {
        private readonly HashSet<CharacterId> _knowers;
        private readonly Dictionary<RelationshipDimension, int> _deltas;

        /// <summary>稳定事件 ID（幂等键，非空）。</summary>
        public string EventId { get; }

        /// <summary>关系指向的对象（知情者对其的关系发生变化）。</summary>
        public CharacterId Target { get; }

        /// <summary>原因标签（诊断/展示用，非空）。</summary>
        public string Reason { get; }

        /// <summary>知情者集合。</summary>
        public IReadOnlyCollection<CharacterId> Knowers => _knowers;

        /// <summary>各维度变化量。</summary>
        public IReadOnlyDictionary<RelationshipDimension, int> Deltas => _deltas;

        public RelationshipEvent(
            string eventId,
            CharacterId target,
            IReadOnlyCollection<CharacterId> knowers,
            IReadOnlyDictionary<RelationshipDimension, int> deltas,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("EventId 不可为空。", nameof(eventId));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason 不可为空。", nameof(reason));
            if (knowers == null) throw new ArgumentNullException(nameof(knowers));
            if (deltas == null) throw new ArgumentNullException(nameof(deltas));

            EventId = eventId;
            Target = target;
            Reason = reason;

            _knowers = new HashSet<CharacterId>(knowers);
            _deltas = new Dictionary<RelationshipDimension, int>();
            foreach (var kv in deltas) _deltas[kv.Key] = kv.Value;
        }
    }
}
