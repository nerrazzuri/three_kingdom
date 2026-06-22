namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 战果分支（gdd-010 §后果）。胜、败、撤退、失城均为<b>分支结算</b>，而非单一胜负开关。
    /// 战败延续（撤退/失城/问责）的可继续路径由 epic-008 Story 002 保证——
    /// 失败<b>必须</b>产生可继续状态（强制设计锁）。
    /// </summary>
    public enum OutcomeBranch
    {
        /// <summary>胜利：守住/达成目标。</summary>
        Victory = 0,

        /// <summary>主动撤退：保存实力、放弃当前交战。</summary>
        Retreat = 1,

        /// <summary>失城：据点易手，但势力延续。</summary>
        CityLost = 2,

        /// <summary>败北（未撤亦未失城的失利，如野战受挫）：问责/重整延续。</summary>
        Defeat = 3,
    }
}
