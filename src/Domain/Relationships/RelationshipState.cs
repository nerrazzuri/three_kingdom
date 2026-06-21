using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Relationships
{
    /// <summary>
    /// 关系状态（GDD_006 §Data Model：RelationshipState / TR-relationship-001）。
    /// <b>方向性多维</b>：键 (from, to, dimension) 独立存储——A→B 与 B→A 互不影响，可不对称（AC-1）。
    /// 变化只经 <see cref="ApplyEvent"/>，按事件稳定 ID <b>幂等</b>去重（同事件重复应用不叠加，AC-2）。
    /// <b>不</b>提供任何单一综合好感值（P6 多维不合并，AC-5）。
    /// </summary>
    public sealed class RelationshipState
    {
        private readonly Dictionary<(CharacterId From, CharacterId To, RelationshipDimension Dim), int> _values
            = new Dictionary<(CharacterId, CharacterId, RelationshipDimension), int>();

        private readonly HashSet<string> _appliedEvents = new HashSet<string>();

        /// <summary>取 from 对 to 在 dim 维度的值；未记录则为中性值。</summary>
        public int Get(CharacterId from, CharacterId to, RelationshipDimension dim)
            => _values.TryGetValue((from, to, dim), out int v) ? v : RelationshipScale.Neutral;

        /// <summary>某事件 ID 是否已结算。</summary>
        public bool HasApplied(string eventId) => _appliedEvents.Contains(eventId);

        /// <summary>
        /// 应用一个具名事件（GDD §Formula 1）：仅对知情者，按维度变化量 clamp 更新其对 target 的关系。
        /// 幂等——若该事件 ID 已结算则直接跳过（返回 false），不重复叠加。
        /// </summary>
        /// <returns>true 表示本次实际结算；false 表示因幂等被跳过。</returns>
        public bool ApplyEvent(RelationshipEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            if (!_appliedEvents.Add(ev.EventId)) return false; // 幂等：已结算

            foreach (var knower in ev.Knowers)
            {
                foreach (var delta in ev.Deltas)
                {
                    var key = (knower, ev.Target, delta.Key);
                    int current = _values.TryGetValue(key, out int v) ? v : RelationshipScale.Neutral;
                    _values[key] = RelationshipScale.Clamp((long)current + delta.Value);
                }
            }
            return true;
        }
    }
}
