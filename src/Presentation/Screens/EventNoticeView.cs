using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 天下事件通报展示（GDD_015 事件分级）。够不着的"可述"事件带主角心里话（口吻随人设）；
    /// 切身事件指向完整事件处理。玩家侧只见文本，无数字。不可变、纯映射。
    /// </summary>
    public sealed class EventNoticeView
    {
        /// <summary>事件结局标签（内部标识）。</summary>
        public string OutcomeLabel { get; }
        /// <summary>分级（Personal 切身 / Notable 可述带心里话 / Background 背景）。</summary>
        public NoticeTier Tier { get; }
        /// <summary>展示文本：Notable→主角心里话；Personal→切身提示。</summary>
        public string Text { get; }
        /// <summary>是否为带心里话的可述通报。</summary>
        public bool HasMonologue { get; }

        public EventNoticeView(EventReflection reflection)
        {
            OutcomeLabel = reflection.OutcomeLabel;
            Tier = reflection.Tier;
            HasMonologue = reflection.HasMonologue;
            Text = reflection.Tier == NoticeTier.Personal
                ? "天下有变，事关于你——详见事件。"
                : reflection.Monologue;
        }
    }
}
