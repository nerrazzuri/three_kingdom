using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 世界推进服务（GDD_015 §Main Rules：「世界推进只由世界时间前进驱动」/ TR-world-001 / ADR-0004）。
    /// 纯函数、确定性：接受前态 + 推进时段数，产出时间前进后的<b>新</b> <see cref="WorldState"/>。
    /// <para>
    /// 本骨架仅推进时间，其余态势不变。历史事件触发（到时间窗→查前置→触发/分叉）属 story-002，
    /// 将在此服务内接入——本服务即该逻辑的单一注入点，故现在固化「时间是唯一推进驱动」契约。
    /// </para>
    /// </summary>
    public sealed class WorldProgressionService
    {
        /// <summary>
        /// 推进 <paramref name="segments"/> 个时段（GDD_001 时段）。
        /// </summary>
        /// <param name="before">前态（非空）。</param>
        /// <param name="segments">推进时段数（≥0；0 为不推进，恒等返回等价前态）。</param>
        public WorldState Advance(WorldState before, int segments)
        {
            if (before is null) throw new ArgumentNullException(nameof(before));
            if (segments < 0) throw new ArgumentOutOfRangeException(nameof(segments), "推进时段数不可为负。");

            // 时间为唯一推进驱动（确定性，ADR-0004）。story-002 将在此后插入历史事件触发评估。
            return before.WithTime(before.CurrentTime.Advance(segments));
        }
    }
}
