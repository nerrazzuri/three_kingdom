namespace ThreeKingdom.Domain.Battle
{
    /// <summary>战役事件类型（GDD_010 §Data Model：BattleEvent）。</summary>
    public enum BattleEventType
    {
        /// <summary>交战发生。</summary>
        Engagement = 0,

        /// <summary>伤亡。</summary>
        Casualty = 1,
    }

    /// <summary>
    /// 战役事件（GDD_010 §Data Model：BattleEvent）。带稳定序号——事件序参与确定性哈希。
    /// 不可变。
    /// </summary>
    public sealed class BattleEvent
    {
        /// <summary>稳定序号（发布顺序，确定性）。</summary>
        public int Sequence { get; }

        /// <summary>事件类型。</summary>
        public BattleEventType Type { get; }

        /// <summary>关联单位。</summary>
        public BattleUnitId Unit { get; }

        /// <summary>可解释明细。</summary>
        public string Detail { get; }

        public BattleEvent(int sequence, BattleEventType type, BattleUnitId unit, string detail)
        {
            Sequence = sequence;
            Type = type;
            Unit = unit;
            Detail = detail ?? string.Empty;
        }
    }
}
