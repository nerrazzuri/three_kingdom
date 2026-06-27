namespace ThreeKingdom.Domain.World
{
    /// <summary>势力存续状态（GDD_015 §Data Model：FactionRecord「存续状态」）。</summary>
    public enum SurvivalStatus
    {
        /// <summary>存续（君主在位、有领土或残部）。</summary>
        Active = 0,

        /// <summary>已灭（势力覆灭，退出棋盘）。</summary>
        Destroyed = 1,
    }
}
