using System;
using System.Collections.Generic;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 新手引导调节项（M15 UX §3/§5 Tuning Knobs / ADR-0003 数据驱动）。触发档位来自版本化配置，
    /// <b>勿硬编码回合数</b>。引导为表现层状态，不进 Domain/存档权威体。不可变；构造校验区间合法。
    /// </summary>
    public sealed class OnboardingConfig
    {
        /// <summary>新手前 N 回合自动展开军师建议（≥0；0=关闭自动展开）。</summary>
        public int AutoCouncilRounds { get; }

        public OnboardingConfig(int autoCouncilRounds)
        {
            if (autoCouncilRounds < 0)
                throw new ArgumentOutOfRangeException(nameof(autoCouncilRounds), "自动展开回合数不可为负。");
            AutoCouncilRounds = autoCouncilRounds;
        }

        /// <summary>M15 §5 默认（前 3 回合自动展开军议）。</summary>
        public static OnboardingConfig Default { get; } = new OnboardingConfig(3);
    }

    /// <summary>
    /// 新手引导提示点（M15 UX §3 渐进暴露序「察→谋→备→战」 + §7 Q2 果·长线加重）。
    /// 每点一句 in-world 情境提示——<b>不模态、不替玩家选择</b>（P11）。
    /// </summary>
    public enum OnboardingCue
    {
        /// <summary>察：治理相位——先探敌情。</summary>
        Observe = 0,
        /// <summary>谋：召开军议听取条件化建议。</summary>
        Plan = 1,
        /// <summary>备：设伏并提交计划。</summary>
        Prepare = 2,
        /// <summary>战：时机成熟即开战。</summary>
        Fight = 3,
        /// <summary>果·长线：战果→记功→晋升（§7 Q2 卡点裁决）。</summary>
        OutcomeToCareer = 4,
        /// <summary>果·长线：推进时段→历史在轨上跑。</summary>
        AdvanceToHistory = 5,
        /// <summary>首次失败强化「可继续」（与 story-002 联动）。</summary>
        DefeatCanContinue = 6,
    }

    /// <summary>
    /// 新手引导展示逻辑（epic-028 story-005 / TR-ux-002 / §3/§7 Q2）。纯函数、无副作用——
    /// <b>只据「已见集 + 关闭偏好 + 当前情境」算出此刻该显示的 in-world 提示</b>，不触会话/存档权威态（引导不进 Domain）。
    /// 关闭引导后一律不打扰；已见提示不重复。文案原创（红线），先中文、键化以备本地化。
    /// </summary>
    public static class OnboardingHints
    {
        /// <summary>
        /// 军议是否自动展开（§5）：未关闭引导且当前回合（1 起）在前 <see cref="OnboardingConfig.AutoCouncilRounds"/> 回合内。
        /// N=0 或已关闭 → 恒不自动展开（按键调出）。
        /// </summary>
        public static bool ShouldAutoExpandCouncil(int round, OnboardingConfig config, bool disabled = false)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return !disabled && round >= 1 && round <= config.AutoCouncilRounds;
        }

        /// <summary>
        /// 从候选引导点中筛出<b>未见且未关闭</b>者的文案（按候选给定序）。关闭引导 → 空；已见 → 跳过。
        /// 调用方显示后应对显示项调 <c>seen</c> 登记（PlayerPrefs 级表现偏好，非权威存档）。
        /// </summary>
        public static IReadOnlyList<string> CuesFor(
            IEnumerable<OnboardingCue> candidates, IReadOnlyCollection<OnboardingCue> seen, bool disabled)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            var result = new List<string>();
            if (disabled) return result;
            var seenSet = seen == null ? new HashSet<OnboardingCue>() : new HashSet<OnboardingCue>(seen);
            foreach (OnboardingCue cue in candidates)
                if (!seenSet.Contains(cue)) result.Add(CueText(cue));
            return result;
        }

        /// <summary>某引导点的 in-world 提示文案（原创；沿自然循环序、缘由式，不替选、不报胜率）。</summary>
        public static string CueText(OnboardingCue cue) => cue switch
        {
            OnboardingCue.Observe => "先察敌情——派出侦察，返报需时；敌情只给估计与时效，没有精确真值。",
            OnboardingCue.Plan => "再谋方略——召开军议，军师陈列缘由与条件，不替你定计、不报胜率。",
            OnboardingCue.Prepare => "而后备战——设伏并提交计划；提交是原子承诺，不可反悔。",
            OnboardingCue.Fight => "时机成熟即开战——兵法是条件组合，非按钮；条件齐备方能成型。",
            OnboardingCue.OutcomeToCareer => "战果之后：记功累积功绩与名望，通往晋升——胜负都在续写生涯长线。",
            OnboardingCue.AdvanceToHistory => "推进时段：历史大势自行在轨上推演——唯有你够得着之处，才会因你而分叉。",
            OnboardingCue.DefeatCanContinue => "此战失利并非终局——重整旗鼓，战役继续；这一败也是来路。",
            _ => cue.ToString(),
        };
    }
}
