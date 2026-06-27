namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 势力对玩家的战略态势关系（GDD_015 §Data Model：FactionRecord「对玩家关系」）。
    /// 此为势力级（战略尺度）态势，区别于 GDD_006 的人物级好感——不得混用。
    /// </summary>
    public enum RelationToPlayer
    {
        /// <summary>玩家自身所属势力。</summary>
        Self = 0,

        /// <summary>盟友。</summary>
        Allied = 1,

        /// <summary>中立。</summary>
        Neutral = 2,

        /// <summary>敌对。</summary>
        Hostile = 3,
    }
}
