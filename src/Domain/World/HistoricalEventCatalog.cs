using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Configuration;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 已校验的历史事件目录（GDD_015 / ADR-0007 + ADR-0003）。不可变。
    /// 经 <see cref="TryCreate"/> 加载期校验后构造：保证每个事件<b>有前置且有分叉结局</b>、EventId 唯一、
    /// 下游引用均指向目录内事件。校验失败聚合返回稳定错误码（<see cref="ConfigErrorCode"/>），无部分加载。
    /// </summary>
    public sealed class HistoricalEventCatalog
    {
        private readonly Dictionary<EventId, HistoricalEvent> _byId;

        /// <summary>目录内事件（按 EventId 稳定序）。</summary>
        public IReadOnlyList<HistoricalEvent> Events { get; }

        private HistoricalEventCatalog(IReadOnlyList<HistoricalEvent> ordered, Dictionary<EventId, HistoricalEvent> byId)
        {
            Events = ordered;
            _byId = byId;
        }

        /// <summary>按 ID 取事件；不存在返回 null。</summary>
        public HistoricalEvent? Find(EventId id) => _byId.TryGetValue(id, out HistoricalEvent? e) ? e : null;

        /// <summary>
        /// 加载并校验事件集合（ADR-0003 配置校验，TR-world-005）。
        /// 拒绝：缺前置 / 缺分叉结局 / 重复 EventId / 下游引用不存在的事件。
        /// </summary>
        public static Result<HistoricalEventCatalog> TryCreate(IReadOnlyList<HistoricalEvent> events)
        {
            if (events is null) throw new ArgumentNullException(nameof(events));

            var errors = new List<ConfigError>();
            var byId = new Dictionary<EventId, HistoricalEvent>();

            foreach (HistoricalEvent e in events)
            {
                if (byId.ContainsKey(e.Id))
                {
                    errors.Add(new ConfigError(ConfigErrorCode.DuplicateStableId, "历史事件 EventId 重复。", e.Id.Value));
                    continue;
                }
                byId[e.Id] = e;

                if (e.Preconds.Count == 0)
                    errors.Add(new ConfigError(ConfigErrorCode.MissingRequiredField, "历史事件缺前置条件。", e.Id.Value, "Preconds"));
                if (e.DivergenceOutcome is null)
                    errors.Add(new ConfigError(ConfigErrorCode.MissingRequiredField, "历史事件缺分叉结局（不得只有正常结局却允许被破坏）。", e.Id.Value, "DivergenceOutcome"));
            }

            // 下游引用必须指向目录内事件。
            foreach (HistoricalEvent e in events)
                foreach (EventId d in e.Downstream)
                    if (!byId.ContainsKey(d))
                        errors.Add(new ConfigError(ConfigErrorCode.MissingReference, $"下游事件引用不存在：{d.Value}。", e.Id.Value, "Downstream"));

            if (errors.Count > 0) return Result<HistoricalEventCatalog>.Failure(errors);

            var ordered = new List<HistoricalEvent>(events);
            ordered.Sort((a, b) => a.Id.CompareTo(b.Id));
            return Result<HistoricalEventCatalog>.Success(new HistoricalEventCatalog(ordered, byId));
        }
    }
}
