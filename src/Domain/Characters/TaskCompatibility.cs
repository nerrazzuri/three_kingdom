using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>任务种类（GDD_005：同一时段只能承担兼容任务）。MVP 集。</summary>
    public enum TaskKind
    {
        /// <summary>侦察。</summary>
        Scouting = 0,

        /// <summary>防守。</summary>
        Defending = 1,

        /// <summary>攻城。</summary>
        Besieging = 2,

        /// <summary>治理。</summary>
        Governing = 3,

        /// <summary>交涉。</summary>
        Negotiating = 4,

        /// <summary>休整。</summary>
        Resting = 5,
    }

    /// <summary>
    /// 任务兼容性策略（GDD_005 §Formula 4 no_conflict）：配置驱动的冲突对集合。
    /// 同一时段两任务冲突 → 不可同时承担（准备系统拒绝，不静默合并）。无序对、自反可配。不可变、确定性。
    /// </summary>
    public sealed class TaskConflictPolicy
    {
        private readonly HashSet<(TaskKind, TaskKind)> _conflicts;

        /// <param name="conflictingPairs">冲突任务对（无序；(a,b) 与 (b,a) 等价）。</param>
        public TaskConflictPolicy(IReadOnlyList<(TaskKind, TaskKind)> conflictingPairs)
        {
            if (conflictingPairs == null) throw new ArgumentNullException(nameof(conflictingPairs));
            _conflicts = new HashSet<(TaskKind, TaskKind)>();
            foreach (var pair in conflictingPairs)
                _conflicts.Add(Normalize(pair.Item1, pair.Item2));
        }

        // 无序对规范化：按枚举序数排列，使 (a,b) 与 (b,a) 命中同一键。
        private static (TaskKind, TaskKind) Normalize(TaskKind a, TaskKind b)
            => (int)a <= (int)b ? (a, b) : (b, a);

        /// <summary>两任务种类是否冲突。</summary>
        public bool Conflicts(TaskKind a, TaskKind b) => _conflicts.Contains(Normalize(a, b));

        /// <summary>
        /// 在已承担 <paramref name="current"/> 的前提下能否再承担 <paramref name="next"/>：
        /// 与任一现任务冲突即不可（GDD §Formula 4）。
        /// </summary>
        public bool CanAssign(IEnumerable<TaskKind> current, TaskKind next)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            foreach (var t in current)
                if (Conflicts(t, next)) return false;
            return true;
        }
    }
}
