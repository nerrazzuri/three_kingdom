using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>历史事件触发后的处置（GDD_015 / ADR-0007 §2 三分支）。</summary>
    public enum FireReason
    {
        /// <summary>未触发：事件已先前触发（幂等短路）。</summary>
        AlreadyTriggered = 0,

        /// <summary>够不着 → 正常结局（前置恒成立，未评估前置）。</summary>
        NormalUnreachable = 1,

        /// <summary>够得着且前置全成立 → 正常结局。</summary>
        NormalPreconditionsHeld = 2,

        /// <summary>够得着且前置被破坏 → 分叉结局（置 diverged）。</summary>
        Diverged = 3,
    }

    /// <summary>
    /// 历史事件推进结果（ADR-0007）。不可变。携带推进后世界态、触发的结局与处置原因。
    /// </summary>
    public sealed class HistoryAdvanceResult
    {
        /// <summary>是否实际触发了事件（AlreadyTriggered 时为 false）。</summary>
        public bool Fired { get; }

        /// <summary>是否走了分叉分支。</summary>
        public bool Diverged { get; }

        /// <summary>处置原因。</summary>
        public FireReason Reason { get; }

        /// <summary>触发的结局（未触发时为 null）。</summary>
        public HistoricalOutcome? FiredOutcome { get; }

        /// <summary>推进后的世界态（未触发时为原态，未变）。</summary>
        public WorldState World { get; }

        public HistoryAdvanceResult(bool fired, bool diverged, FireReason reason, HistoricalOutcome? firedOutcome, WorldState world)
        {
            Fired = fired;
            Diverged = diverged;
            Reason = reason;
            FiredOutcome = firedOutcome;
            World = world ?? throw new ArgumentNullException(nameof(world));
        }
    }
}
