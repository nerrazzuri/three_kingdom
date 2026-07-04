namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 历史事件对玩家的通报分级（GDD_015 事件分级：够得着走完整事件、够不着走轻量通报，
    /// 破"全演义事件网内容爆炸"的成本墙——无关事件近零成本，仍以"心里话"反哺生涯抉择）。
    /// </summary>
    public enum NoticeTier
    {
        /// <summary>切身：够得着（涉及玩家/前置成立/已分叉）→ 走完整事件与分叉结算。</summary>
        Personal = 0,

        /// <summary>可述：够不着但值得一提（如他雄称帝）→ 通报 + 主角心里话（可埋生涯种子）。</summary>
        Notable = 1,

        /// <summary>背景：够不着且琐碎 → 仅记为世界事实，不打扰玩家。</summary>
        Background = 2,
    }
}
