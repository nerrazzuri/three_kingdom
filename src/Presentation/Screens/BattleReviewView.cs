using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Outcome;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 战果复盘屏调节项（M15 UX §5 Tuning Knobs 的表现层集中承载——数值不散落 UI 代码，CON-5 同纪律）。
    /// 2026-07-03 用户裁决：因果链<b>默认折叠</b>（一句话结论，一键展开）· 主因素 <b>≤5</b>。
    /// </summary>
    public sealed class BattleReviewTuning
    {
        /// <summary>展开态最多显示的主因素数（M15 AC-1：≤5）。</summary>
        public int MaxFactors { get; }

        /// <summary>复盘初始是否折叠（2026-07-03 裁决：默认折叠）。</summary>
        public bool DefaultCollapsed { get; }

        public BattleReviewTuning(int maxFactors, bool defaultCollapsed)
        {
            if (maxFactors < 1) throw new ArgumentOutOfRangeException(nameof(maxFactors), "主因素数须 ≥1。");
            MaxFactors = maxFactors;
            DefaultCollapsed = defaultCollapsed;
        }

        /// <summary>M15 裁决默认值（≤5 因素 · 默认折叠）。</summary>
        public static BattleReviewTuning Default { get; } = new BattleReviewTuning(5, true);
    }

    /// <summary>一条续局选项展示（中文种类标签 + Domain 缘由；只读）。</summary>
    public sealed class BattleReviewOptionView
    {
        /// <summary>续局命令种类（Domain 枚举，供后续 story 接真实命令分派）。</summary>
        public ContinuationCommandKind Kind { get; }

        /// <summary>中文种类标签。</summary>
        public string KindLabel { get; }

        /// <summary>可读缘由（来自权威 <see cref="ContinuationOption.Reason"/>）。</summary>
        public string Reason { get; }

        internal BattleReviewOptionView(ContinuationCommandKind kind, string kindLabel, string reason)
        {
            Kind = kind;
            KindLabel = kindLabel;
            Reason = reason;
        }
    }

    /// <summary>
    /// 战果复盘屏展示模型（epic-028 story-002 / TR-ux-001 因果契约 + TR-ux-004 续局契约）。
    /// <para>
    /// <b>折叠态</b>：一句话结论（分支 + 首要原因）。<b>展开态</b>：≤N 主因素因果链（因素=权威
    /// <see cref="OutcomeChange"/> 的 Reason/Delta，UI 不补算）+ 满足的兵法条件与成型兵法
    /// + 续局选项（任何分支保证 ≥1，无「终局」措辞）+ 长线记功提示（果→长线，Q2 卡点裁决）。
    /// 不可变；<see cref="Expand"/>/<see cref="Collapse"/> 产出新实例、共享同一权威数据
    /// （同态渲染恒等且展开/折叠不改内容，ADR-0004/TR-ux-005）。
    /// 全部文案原创（红线）；<b>绝不</b>出现成功率数字/「游戏结束」（AC-2/AC-4 反例清单）。
    /// </para>
    /// </summary>
    public sealed class BattleReviewView
    {
        private static readonly IReadOnlyDictionary<OutcomeBranch, string> BranchLabels =
            new Dictionary<OutcomeBranch, string>
            {
                [OutcomeBranch.Victory] = "胜利",
                [OutcomeBranch.Retreat] = "撤退",
                [OutcomeBranch.CityLost] = "失城",
                [OutcomeBranch.Defeat] = "败北",
            };

        private static readonly IReadOnlyDictionary<ContinuationCommandKind, string> KindLabels =
            new Dictionary<ContinuationCommandKind, string>
            {
                [ContinuationCommandKind.Pursue] = "乘胜追击",
                [ContinuationCommandKind.Consolidate] = "巩固据点",
                [ContinuationCommandKind.Regroup] = "重整旗鼓",
                [ContinuationCommandKind.Accountability] = "问责追责",
                [ContinuationCommandKind.Retreat] = "且战且退",
                [ContinuationCommandKind.SueForPeace] = "遣使求和",
            };

        private static readonly IReadOnlyDictionary<TacticTag, string> TacticNames =
            new Dictionary<TacticTag, string>
            {
                [TacticTag.FeintAmbush] = "假退伏击",
                [TacticTag.SupplyExhaustion] = "断粮疲敌",
                [TacticTag.HoldUntilRelief] = "守城待变",
            };

        /// <summary>完整主因素文案（权威顺序，已截取 ≤ 上限；展开/折叠共享此数据）。</summary>
        private readonly IReadOnlyList<string> _factorLines;

        /// <summary>战果分支中文标签。</summary>
        public string BranchLabel { get; }

        /// <summary>折叠态一句话结论（分支 + 首要原因）。</summary>
        public string ConclusionLabel { get; }

        /// <summary>因果链当前是否展开。</summary>
        public bool IsExpanded { get; }

        /// <summary>展开/收起按钮文案。</summary>
        public string ToggleLabel => IsExpanded ? "收起因果" : "展开因果";

        /// <summary>当前可见的主因素文案（折叠=空；展开=全部，≤ 调节项上限）。</summary>
        public IReadOnlyList<string> VisibleFactorLines
            => IsExpanded ? _factorLines : Array.Empty<string>();

        /// <summary>主因素总数（与展开状态无关——「展开与否内容不变」的可断言面）。</summary>
        public int FactorCount => _factorLines.Count;

        /// <summary>兵法复盘（满足的条件数与成型兵法；条件不全时明示「兵法是条件组合，非按钮」）。</summary>
        public IReadOnlyList<string> TacticLines { get; }

        /// <summary>续局选项（任何分支保证非空——强制设计锁「失败可继续」）。</summary>
        public IReadOnlyList<BattleReviewOptionView> Options { get; }

        /// <summary>续局提示（败类分支明示「可继续，非终局」——扭转「输了=重来」预期）。</summary>
        public string ContinuationNotice { get; }

        /// <summary>长线记功提示（果→长线引导，2026-07-03 Q2 卡点裁决；数据来自梯队配置与当前生涯态）。</summary>
        public string CareerHintLabel { get; }

        private BattleReviewView(
            string branchLabel, string conclusion, bool expanded,
            IReadOnlyList<string> factorLines, IReadOnlyList<string> tacticLines,
            IReadOnlyList<BattleReviewOptionView> options, string continuationNotice, string careerHint)
        {
            BranchLabel = branchLabel;
            ConclusionLabel = conclusion;
            IsExpanded = expanded;
            _factorLines = factorLines;
            TacticLines = tacticLines;
            Options = options;
            ContinuationNotice = continuationNotice;
            CareerHintLabel = careerHint;
        }

        /// <summary>
        /// 从权威投影构造复盘（初始展开态由调节项决定）。
        /// 因素=<paramref name="changes"/> 的 Reason/Delta（按权威顺序截取前 N 条，不重排、不补算）；
        /// 首要原因=成型兵法优先（条件涌现是核心幻想），否则首条变更缘由。
        /// <paramref name="options"/> 为空违反强制设计锁，抛异常（Domain 已保证非空，此处双保险）。
        /// </summary>
        public static BattleReviewView From(
            OutcomeBranch branch,
            IReadOnlyList<OutcomeChange> changes,
            IReadOnlyList<RecognizedTactic> tactics,
            IReadOnlyList<ContinuationOption> options,
            CareerGain? gainPreview, int currentMerit, int currentRenown,
            BattleReviewTuning tuning)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            if (tactics == null) throw new ArgumentNullException(nameof(tactics));
            if (options == null || options.Count == 0)
                throw new InvalidOperationException("强制设计锁违反：复盘无任何续局选项（失败必须可继续）。");
            if (tuning == null) throw new ArgumentNullException(nameof(tuning));

            string branchLabel = Label(BranchLabels, branch);

            string primary = tactics.Count > 0
                ? $"成型兵法「{Label(TacticNames, tactics[0].Tag)}」"
                : changes.Count > 0 ? changes[0].Reason : "战场态势";
            string conclusion = $"{branchLabel}——首要原因：{primary}";

            var lines = new List<string>();
            for (int i = 0; i < changes.Count && i < tuning.MaxFactors; i++)
            {
                OutcomeChange c = changes[i];
                lines.Add($"{c.Reason}（{(c.Delta >= 0 ? "+" : string.Empty)}{c.Delta}）");
            }

            var tacticLines = new List<string>();
            if (tactics.Count == 0)
            {
                tacticLines.Add("条件未全，未识别出成型兵法——兵法是条件组合，非按钮。");
            }
            else
            {
                foreach (RecognizedTactic rt in tactics)
                    tacticLines.Add($"成型兵法「{Label(TacticNames, rt.Tag)}」（满足条件 {rt.MatchedConditions.Count} 条）");
            }

            var optionViews = new List<BattleReviewOptionView>();
            foreach (ContinuationOption o in options)
                optionViews.Add(new BattleReviewOptionView(o.Kind, Label(KindLabels, o.Kind), o.Reason));

            string notice = branch == OutcomeBranch.Victory
                ? "乘胜可续——选一条续局之路。"
                : "此战失利，但可继续——非终局，选一条续局之路，重整再来。";

            string careerHint = gainPreview != null
                ? $"此战可记功：功绩 +{gainPreview.Merit} · 名望 +{gainPreview.Renown}（当前 功绩 {currentMerit} · 名望 {currentRenown}）——记功后计入晋升门槛。"
                : $"此战无战功可记（当前 功绩 {currentMerit} · 名望 {currentRenown}）——治理、招揽等非战斗之路同样累积晋升资历。";

            return new BattleReviewView(
                branchLabel, conclusion, !tuning.DefaultCollapsed,
                lines, tacticLines, optionViews, notice, careerHint);
        }

        /// <summary>中文标签查表；未登记的新枚举值降级为枚举名（Domain 扩枚举不崩 UI，仅缺译名）。</summary>
        private static string Label<TKey>(IReadOnlyDictionary<TKey, string> labels, TKey key) where TKey : notnull
            => labels.TryGetValue(key, out string? label) ? label : key.ToString()!;

        /// <summary>展开因果链（共享同一权威数据；内容不因展开而变，只改可见性）。</summary>
        public BattleReviewView Expand()
            => new BattleReviewView(BranchLabel, ConclusionLabel, true,
                _factorLines, TacticLines, Options, ContinuationNotice, CareerHintLabel);

        /// <summary>收起因果链（回到一句话结论；数据保留，可再展开）。</summary>
        public BattleReviewView Collapse()
            => new BattleReviewView(BranchLabel, ConclusionLabel, false,
                _factorLines, TacticLines, Options, ContinuationNotice, CareerHintLabel);
    }
}
