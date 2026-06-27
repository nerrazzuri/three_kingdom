using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 可达性谓词（ADR-0007 §2：reachability 触发门）。判定玩家势力圈是否触及事件前置主体。
    /// 够不着即短路——不评估前置、不跑 AI（"早期历史便宜"的来源）。
    /// </summary>
    public interface IReachPredicate
    {
        /// <summary>玩家势力圈 <paramref name="reach"/> 是否触及事件 <paramref name="e"/> 的前置主体。</summary>
        bool Reachable(HistoricalEvent e, PlayerReach reach);
    }

    /// <summary>
    /// 默认可达性谓词（MVP）：事件任一前置的主体势力或主体城池被玩家圈触及，则视为够得着。
    /// 单一战役框够用；全图主体拓扑判定属 Future Scope。
    /// </summary>
    public sealed class SubjectReachPredicate : IReachPredicate
    {
        public bool Reachable(HistoricalEvent e, PlayerReach reach)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));
            if (reach is null) throw new ArgumentNullException(nameof(reach));
            foreach (Precondition p in e.Preconds)
            {
                if (reach.Touches(p.SubjectFaction)) return true;
                if (p.SubjectCity.HasValue && reach.Touches(p.SubjectCity.Value)) return true;
            }
            return false;
        }
    }
}
