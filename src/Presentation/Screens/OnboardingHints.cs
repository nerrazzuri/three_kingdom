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
        /// <summary>出征：六维准备决定胜负（备足以势破城，裸战多退兵）。</summary>
        PreparationDecides = 7,
        /// <summary>人心杠杆：攻城亦可攻心（离间/策反/流言撬动战局，非替代备战）。</summary>
        MindLever = 8,
        /// <summary>主角人设：开局性情各异，天下事件引出心里话（丰富代入，去留仍由你）。</summary>
        PersonaIntro = 9,
        /// <summary>纪元一生（GDD_026）：空降者寿数有限，按季按年历经史事、老去、传承。</summary>
        ArrivalLife = 10,
        /// <summary>选城起家（GDD_026）：任取非治所城为太守，该城将佐归你。</summary>
        GovernorStart = 11,
        /// <summary>兵种地形（W4）：兵种须合地利，编成随目标城地形而配。</summary>
        TroopTerrain = 12,
        /// <summary>羁绊（GDD_025 R4）：并肩之将协同、宿敌互扣，用人须看其交谊与嫌隙。</summary>
        Bonds = 13,
        /// <summary>被灭续局（GDD_026 R9）：城破被俘可归顺/投奔东山再起，唯身死才终。</summary>
        DefeatCaptive = 14,
        /// <summary>自立无退路（GDD_026 R9.1）：叛主独立若败，必被俘处死，无归顺投奔之路。</summary>
        RebellionNoReturn = 15,
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
            OnboardingCue.PreparationDecides => "出征胜负先在案头定——兵力、补给、将领、兵种、布势、时机六维备足，方能以势破城；裸战强攻，多半退兵。",
            OnboardingCue.MindLever => "攻城亦可攻心——先探明守将嫌隙，再离间、策反、散布流言，令其未战先乱。但攻心是撬动，非替代备战。",
            OnboardingCue.PersonaIntro => "你自有性情——或雄心、或忠义、或务实、或谨慎。天下风云入眼，心里话随你本心；何去何从，仍由你定。",
            OnboardingCue.ArrivalLife => "你是空降此世之人，寿数有限——按周行事、按季按年推演；赤壁、官渡诸事将在你一生里逐年展开，待你老去，再由子嗣续写。",
            OnboardingCue.GovernorStart => "任取一城为太守，该城将佐尽听调遣；君主亲镇的治所不可选。你名义奉其君主，安做臣属或伺机自立，皆在你。",
            OnboardingCue.TroopTerrain => "兵种要合地利——骑利平原冲杀、水军利渡口、隘口与坚城则步战守成；出征前看目标城地形，配好编成。",
            OnboardingCue.Bonds => "并肩之将各有羁绊——刘关张同场则士气相协，宿敌同列则貌合神离。用人须看他与谁生死之交、与谁有隙。",
            OnboardingCue.DefeatCaptive => "势力覆灭并非死局——城破被俘，或归顺新主、或投奔他家东山再起（未必人人肯收）；唯身死才终这一世。",
            OnboardingCue.RebellionNoReturn => "自立是无退路之赌——一旦叛主独立，他日若败，必被俘处死，再无归顺投奔之路。成则问鼎，败则族灭。",
            _ => cue.ToString(),
        };
    }
}
