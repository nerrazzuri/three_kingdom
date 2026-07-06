using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>演义事件求值上下文（GDD_027 P8）：纪元盘年 + 当前公元年 + 玩家势力。（区别于 GDD_015 条件历史 HistoricalEvent。）</summary>
    public readonly struct LoreContext
    {
        public int AnchorYear { get; }
        public int CurrentYear { get; }
        public FactionId PlayerFaction { get; }
        public LoreContext(int anchorYear, int currentYear, FactionId playerFaction)
        {
            AnchorYear = anchorYear; CurrentYear = currentYear; PlayerFaction = playerFaction;
        }
    }

    /// <summary>一桩演义招牌事件（GDD_027 P8）：锚定具名武将，按在世/归属/纪元触发。试水期只做招牌事件；引擎可扩。</summary>
    public sealed class LoreEvent
    {
        public string Id { get; }
        public string Name { get; }
        public string Narrative { get; }
        private readonly Func<LoreContext, bool> _trigger;

        public LoreEvent(string id, string name, string narrative, Func<LoreContext, bool> trigger)
        {
            Id = id; Name = name; Narrative = narrative; _trigger = trigger;
        }

        public bool Fires(LoreContext ctx) => _trigger(ctx);
    }

    /// <summary>
    /// 演义事件引擎（GDD_027 P8 试水）：登记招牌事件表，按上下文求值出当轮触发者。事件锚定具名武将（在世/归属驱动）。
    /// 试水先落一桩「桃园结义」；机制验证通过后系统化扩表（更多招牌事件 + 移籍/生死效果）。纯函数，无场景依赖。
    /// 与 GDD_015 条件历史世界模型（Domain.World.HistoricalEvent，因果四元组）分工：此为<b>演义脚本事件</b>，那为世界因果推演。
    /// </summary>
    public static class LoreEvents
    {
        private static bool Alive(string id, int year) => GeneralDossiers.AvailableAt(new CharacterId(id), year);

        /// <summary>招牌事件表（试水期 1 桩；引擎设计为可扩）。</summary>
        public static readonly IReadOnlyList<LoreEvent> All = new[]
        {
            // 桃园结义：玩家扮刘备、讨董之世(≤190)、刘关张俱在 → 义结金兰（效果：三人同场凝聚/士气，接 P6）。
            new LoreEvent(
                id: "event-taoyuan", name: "桃园结义",
                narrative: "念刘备、关羽、张飞，虽然异姓，既结为兄弟，则同心协力，救困扶危；上报国家，下安黎庶。",
                trigger: ctx =>
                    ctx.PlayerFaction == PlayableCampaign.LiuBei
                    && ctx.CurrentYear <= 190
                    && Alive("char-liubei", ctx.CurrentYear)
                    && Alive("char-guanyu", ctx.CurrentYear)
                    && Alive("char-zhangfei", ctx.CurrentYear)),
        };

        /// <summary>某上下文当轮触发的招牌事件（稳定序）。</summary>
        public static IReadOnlyList<LoreEvent> FiredAt(LoreContext ctx)
        {
            var fired = new List<LoreEvent>();
            foreach (LoreEvent e in All) if (e.Fires(ctx)) fired.Add(e);
            return fired;
        }
    }
}
