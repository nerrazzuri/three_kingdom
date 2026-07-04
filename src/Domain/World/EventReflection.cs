namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 事件通报（GDD_015 事件分级的产出）。不可变。切身事件走完整结算（<see cref="Tier"/>=Personal）；
    /// 够不着的可述事件带 <see cref="Monologue"/>（<b>随主角人设着色的心里话</b>，纯丰富体验）；
    /// 背景事件仅记世界事实、不打扰玩家。玩家侧只见通报文本与心里话，无任何数字。
    /// </summary>
    public sealed class EventReflection
    {
        /// <summary>触发的事件结局标签。</summary>
        public string OutcomeLabel { get; }
        /// <summary>通报分级。</summary>
        public NoticeTier Tier { get; }
        /// <summary>主角心里话（仅 Notable 非空；口吻随人设）。</summary>
        public string Monologue { get; }

        public EventReflection(string outcomeLabel, NoticeTier tier, string monologue)
        {
            OutcomeLabel = outcomeLabel ?? string.Empty;
            Tier = tier;
            Monologue = monologue ?? string.Empty;
        }

        /// <summary>是否有心里话（Notable 且有台词）。</summary>
        public bool HasMonologue => Tier == NoticeTier.Notable && Monologue.Length > 0;
    }
}
