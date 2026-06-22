using System;
using System.Collections.Generic;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>HUD 情境（hud.md §5 五态）。</summary>
    public enum HudContext
    {
        /// <summary>生活观察。</summary>
        DailyObservation = 0,
        /// <summary>判断布局。</summary>
        JudgmentLayout = 1,
        /// <summary>行动承诺。</summary>
        ActionCommit = 2,
        /// <summary>战争应变。</summary>
        WarResponse = 3,
        /// <summary>战果延续。</summary>
        OutcomeReview = 4,
    }

    /// <summary>HUD 元素（hud.md §4）。</summary>
    public enum HudElement
    {
        /// <summary>时间条。</summary>
        TimeBar = 0,
        /// <summary>己方账本（完整真值）。</summary>
        OwnLedger = 1,
        /// <summary>敌方探报（仅推测，无真值）。</summary>
        EnemyReport = 2,
        /// <summary>军师入口（条件化建议，无最优解）。</summary>
        AdvisorEntry = 3,
        /// <summary>命令托盘。</summary>
        CommandTray = 4,
        /// <summary>战果因果链。</summary>
        OutcomeChain = 5,
    }

    /// <summary>
    /// HUD 情境→可见元素集的展示投影（hud.md §5/§12 / ADR-0002）。
    /// 每个情境<b>只</b>显示规定元素；<b>全屏模态</b>（军议/暂停/读档）→ 元素集<b>为空</b>（隐去全部 HUD）。
    /// 确定性：同情境 → 同元素集。不可变。
    /// </summary>
    public sealed class HudContextView
    {
        private static readonly IReadOnlyDictionary<HudContext, HudElement[]> Map =
            new Dictionary<HudContext, HudElement[]>
            {
                [HudContext.DailyObservation] = new[] { HudElement.TimeBar, HudElement.OwnLedger },
                [HudContext.JudgmentLayout] = new[] { HudElement.TimeBar, HudElement.OwnLedger, HudElement.EnemyReport, HudElement.AdvisorEntry },
                [HudContext.ActionCommit] = new[] { HudElement.TimeBar, HudElement.OwnLedger, HudElement.CommandTray },
                [HudContext.WarResponse] = new[] { HudElement.TimeBar, HudElement.OwnLedger, HudElement.EnemyReport, HudElement.CommandTray },
                [HudContext.OutcomeReview] = new[] { HudElement.OwnLedger, HudElement.OutcomeChain },
            };

        private readonly HashSet<HudElement> _visible;

        /// <summary>当前情境。</summary>
        public HudContext Context { get; }

        /// <summary>全屏模态是否激活（激活则隐去全部 HUD）。</summary>
        public bool ModalActive { get; }

        /// <summary>可见元素集（模态激活时为空；否则按情境规定）。</summary>
        public IReadOnlyCollection<HudElement> VisibleElements => _visible;

        private HudContextView(HudContext context, bool modalActive, HashSet<HudElement> visible)
        {
            Context = context;
            ModalActive = modalActive;
            _visible = visible;
        }

        /// <summary>构造某情境的 HUD（modalActive=true 则隐去全部元素）。</summary>
        public static HudContextView ForContext(HudContext context, bool modalActive = false)
        {
            if (!Map.TryGetValue(context, out var elements))
                throw new ArgumentOutOfRangeException(nameof(context));
            var set = modalActive ? new HashSet<HudElement>() : new HashSet<HudElement>(elements);
            return new HudContextView(context, modalActive, set);
        }

        /// <summary>是否显示某元素。</summary>
        public bool Shows(HudElement element) => _visible.Contains(element);
    }
}
