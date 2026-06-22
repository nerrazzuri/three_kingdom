namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 断粮后果事件（GDD_012 §Formula 5 / §权威分工 / TR-supply-002）。
    /// 短缺累计达宽限期时由后勤系统<b>发出</b>，携带受影响单位与累计时段。
    /// <para>
    /// <b>单一权威红线</b>：本系统<b>只</b>发出此事件，<b>不</b>就地变更士气/疲劳；
    /// morale/fatigue 的实际施加由 GDD_011 唯一消费此事件（幂等），GDD_010 只读。
    /// 因此本事件为不可变的<b>意图通知</b>，不含也不应用任何状态减益。
    /// </para>
    /// </summary>
    public sealed class SupplyCutoffEvent
    {
        /// <summary>受断粮影响的单位。</summary>
        public UnitId Unit { get; }

        /// <summary>短缺连续累计时段（达宽限期触发本事件）。</summary>
        public int ShortageSegments { get; }

        /// <summary>本时段未满足的需求量（短缺量，≥0）。</summary>
        public long Shortage { get; }

        public SupplyCutoffEvent(UnitId unit, int shortageSegments, long shortage)
        {
            Unit = unit;
            ShortageSegments = shortageSegments;
            Shortage = shortage;
        }
    }
}
