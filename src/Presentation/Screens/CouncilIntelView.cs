using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 军议/敌情屏调节项（M15 UX §2.3 置信/时效契约 · §7 Q1 裁决——表现层数值集中承载，不散落 UI 代码，CON-5 同纪律）。
    /// <para>
    /// 定性置信按两个阈值分三档（低/中/高），阈值<b>数据驱动</b>（勿硬编码映射边界，控制清单 guardrail）。
    /// 时效「过时」阈值<b>不在此</b>——与 <c>IntelConfig.TtlSegments</c> 同源（读同一配置），由调用方传入 ViewModel，
    /// 避免另立常量漂移。不可变；构造校验阈值区间合法。
    /// </para>
    /// </summary>
    public sealed class CouncilIntelTuning
    {
        /// <summary>低/中 分界：<c>confidence &lt; LowCeiling → 低</c>。定点 [0,1]。</summary>
        public FixedPoint LowCeiling { get; }

        /// <summary>中/高 分界：<c>LowCeiling ≤ confidence &lt; HighCeiling → 中；≥ HighCeiling → 高</c>。定点 [0,1]。</summary>
        public FixedPoint HighCeiling { get; }

        /// <summary>低档文字标签。</summary>
        public string LowLabel { get; }

        /// <summary>中档文字标签。</summary>
        public string MidLabel { get; }

        /// <summary>高档文字标签。</summary>
        public string HighLabel { get; }

        public CouncilIntelTuning(
            FixedPoint lowCeiling, FixedPoint highCeiling,
            string lowLabel = "低", string midLabel = "中", string highLabel = "高")
        {
            RequireUnit(lowCeiling, nameof(lowCeiling));
            RequireUnit(highCeiling, nameof(highCeiling));
            if (highCeiling <= lowCeiling)
                throw new ArgumentException("中/高分界须严格大于低/中分界。", nameof(highCeiling));

            LowCeiling = lowCeiling;
            HighCeiling = highCeiling;
            LowLabel = lowLabel ?? throw new ArgumentNullException(nameof(lowLabel));
            MidLabel = midLabel ?? throw new ArgumentNullException(nameof(midLabel));
            HighLabel = highLabel ?? throw new ArgumentNullException(nameof(highLabel));
        }

        /// <summary>M15 §7 Q1 裁决默认（0.4 / 0.7 → 低/中/高）。</summary>
        public static CouncilIntelTuning Default { get; } =
            new CouncilIntelTuning(FixedPoint.FromFraction(2, 5), FixedPoint.FromFraction(7, 10));

        /// <summary>
        /// 把<b>依据置信</b>（定点 [0,1]，<b>非</b>成功率）映射为定性档标签。
        /// 边界归属明确且稳定：<c>== LowCeiling → 中</c>（非「低」，因非严格小于）；<c>== HighCeiling → 高</c>。
        /// </summary>
        public string Band(FixedPoint confidence)
        {
            if (confidence < LowCeiling) return LowLabel;
            if (confidence < HighCeiling) return MidLabel;
            return HighLabel;
        }

        private static void RequireUnit(FixedPoint value, string name)
        {
            if (value < FixedPoint.Zero || value > FixedPoint.One)
                throw new ArgumentOutOfRangeException(name, "分界须在 [0,1]。");
        }
    }

    /// <summary>
    /// 单条军师建议的<b>战役屏</b>展示模型（GDD_008 / P11 无最优解 / ADR-0002 / TR-ux-002）。
    /// 只呈现观察/假设/所需条件/风险/缺失情报 + <b>定性</b>置信档（低/中/高）。
    /// <para>
    /// <b>负向不变量（结构性）</b>：本类型<b>刻意不含</b>成功率数值、最优/推荐标记、排序分值——
    /// <see cref="ConfidenceLabel"/> 是依据可靠性的定性标签，<b>非</b>成功概率（由 CouncilIntelViewModelTests 反射断言）。
    /// </para>
    /// 不可变。
    /// </summary>
    public sealed class CampaignAdviceView
    {
        /// <summary>候选标识（路线名，<b>非</b>排名）。</summary>
        public string CandidateLabel { get; }

        /// <summary>观察（事实陈述）。</summary>
        public string Observation { get; }

        /// <summary>假设（须成立的前提）。</summary>
        public string Assumption { get; }

        /// <summary>所需条件。</summary>
        public IReadOnlyList<string> RequiredConditions { get; }

        /// <summary>主要风险。</summary>
        public IReadOnlyList<string> Risks { get; }

        /// <summary>缺失情报（展示文本）。</summary>
        public IReadOnlyList<string> MissingIntel { get; }

        /// <summary>定性置信档（低/中/高），<b>非</b>成功率、<b>无</b>小数。</summary>
        public string ConfidenceLabel { get; }

        private CampaignAdviceView(
            string candidate, string observation, string assumption,
            IReadOnlyList<string> required, IReadOnlyList<string> risks,
            IReadOnlyList<string> missing, string confidenceLabel)
        {
            CandidateLabel = candidate;
            Observation = observation;
            Assumption = assumption;
            RequiredConditions = required;
            Risks = risks;
            MissingIntel = missing;
            ConfidenceLabel = confidenceLabel;
        }

        /// <summary>从军师建议构造（不排优劣、小数置信经调节项映射为定性档，绝不暴露成功率）。</summary>
        internal static CampaignAdviceView FromStatement(AdviceStatement s, CouncilIntelTuning tuning)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var missing = new List<string>();
            foreach (IntelSubjectId m in s.MissingIntel) missing.Add(DisplayNames.Of(m.Value));
            return new CampaignAdviceView(
                DisplayNames.Of(s.CandidateId), s.Observation, s.Assumption,
                new List<string>(s.RequiredConditions),
                new List<string>(s.Risks),
                missing,
                tuning.Band(s.Confidence));
        }
    }

    /// <summary>
    /// 军议屏展示模型（epic-028 story-003 / TR-ux-002/003 / GDD_008 §Formula 3/4）。
    /// <para>
    /// 建议<b>并列</b>呈现，<b>无</b>全局排名或「最佳方案」标记；绑定召开时的知识快照——当前知识快照变化后
    /// <see cref="IsStale"/> 为真并给出重开提示（<b>不</b>静默重算，玩家仍可依旧建议行事，决策自由 P11）。
    /// 纯函数构造：同建议集 + 同当前快照 + 同调节项 → 逐字段恒等（ADR-0004 / TR-ux-005）。不可变。
    /// </para>
    /// </summary>
    public sealed class CampaignCouncilView
    {
        /// <summary>并列建议（保持 Domain 给定顺序，不重排为优劣序）。</summary>
        public IReadOnlyList<CampaignAdviceView> Advice { get; }

        /// <summary>是否相对当前知识快照已过时（侦察改变知识后为真）。</summary>
        public bool IsStale { get; }

        /// <summary>过时提示（过时=显式徽标文案 + 重开建议；不过时为空串）。不禁用旧建议。</summary>
        public string StaleNotice
            => IsStale ? "情报已更新，此军议基于召开时的旧情报——建议重新召开军议。" : string.Empty;

        private CampaignCouncilView(IReadOnlyList<CampaignAdviceView> advice, bool isStale)
        {
            Advice = advice;
            IsStale = isStale;
        }

        /// <summary>从军议建议集构造（按当前知识快照判定过时；小数置信经调节项定性化）。</summary>
        public static CampaignCouncilView FromSet(
            CouncilAdviceSet set, KnowledgeSnapshotId currentSnapshot, CouncilIntelTuning tuning)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            if (tuning == null) throw new ArgumentNullException(nameof(tuning));
            var views = new List<CampaignAdviceView>();
            foreach (AdviceStatement s in set.Advice) views.Add(CampaignAdviceView.FromStatement(s, tuning));
            return new CampaignCouncilView(views, set.IsStaleAgainst(currentSnapshot));
        }
    }

    /// <summary>
    /// 单条敌情的<b>战役屏</b>展示模型（GDD_007 / P1 不完全信息核心设计锁 / ADR-0002 / TR-ux-003）。
    /// <b>只</b>由阵营知识投影（<see cref="IntelProjection"/>，结构上不含真值）派生——故本类型<b>无任何权威真值字段</b>，
    /// 只有报告估计值/来源/时效（负向不变量由 CouncilIntelViewModelTests 反射断言）。不可变。
    /// </summary>
    public sealed class CampaignEnemyIntelView
    {
        /// <summary>主题标识（展示文本）。</summary>
        public string SubjectLabel { get; }

        /// <summary>估计兵力（<b>报告值</b>，非真值；来自 <see cref="IntelKnowledgeEntry.KnownStrength"/>）。</summary>
        public int EstimatedStrength { get; }

        /// <summary>来源标签（可靠性提示）。</summary>
        public string SourceLabel { get; }

        /// <summary>观测已历时段数（now − 观测时刻，非负）。</summary>
        public long AgeSegments { get; }

        /// <summary>时效标签（「N 段前」；本时段获报为特例文案）。</summary>
        public string ObservedAgoLabel { get; }

        /// <summary>是否已超时效告警阈值（age &gt; ttl）——UI 高亮为过时。</summary>
        public bool IsStale { get; }

        private CampaignEnemyIntelView(
            string subjectLabel, int estimatedStrength, string sourceLabel,
            long ageSegments, string observedAgoLabel, bool isStale)
        {
            SubjectLabel = subjectLabel;
            EstimatedStrength = estimatedStrength;
            SourceLabel = sourceLabel;
            AgeSegments = ageSegments;
            ObservedAgoLabel = observedAgoLabel;
            IsStale = isStale;
        }

        /// <summary>
        /// 从单条知识条目构造（只搬运阵营合法字段，绝不接触真值）。时效相对 <paramref name="now"/> 计算，
        /// 过时阈值 <paramref name="ttlSegments"/> 与 IntelConfig 同源（由调用方传入）。
        /// </summary>
        internal static CampaignEnemyIntelView FromEntry(IntelKnowledgeEntry entry, WorldTime now, int ttlSegments)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            long age = now.AbsoluteIndex - entry.ObservedAt.AbsoluteIndex;
            if (age < 0) age = 0;   // now 早于观测（异常）→ 视为本时段，不呈现负时效
            string ago = age <= 0 ? "本时段获报" : age + " 段前";
            bool stale = age > ttlSegments;
            return new CampaignEnemyIntelView(
                DisplayNames.Of(entry.Subject.Value), entry.KnownStrength, SourceText(entry.Source), age, ago, stale);
        }

        private static string SourceText(IntelSource source) => source switch
        {
            IntelSource.Scouting => "斥候",
            IntelSource.DirectObservation => "目视",
            _ => source.ToString(),
        };
    }

    /// <summary>
    /// 敌情屏展示模型（epic-028 story-003 / TR-ux-003 / GDD_007）：从只读投影派生的一组探报条目。
    /// 条目按主题标签序数升序排列，保证<b>确定性</b>（同投影+同 now+同 ttl → 同展示模型，ADR-0004）。不可变。
    /// </summary>
    public sealed class CampaignEnemyIntelPanelView
    {
        /// <summary>探报条目（只读，确定性排序）。</summary>
        public IReadOnlyList<CampaignEnemyIntelView> Entries { get; }

        /// <summary>在途侦察提示（GDD_007 派出→在途→返报；一条一句「约第 X 日返报」，确定性排序，无任何数值）。</summary>
        public IReadOnlyList<string> InTransit { get; }

        /// <summary>是否尚无任何敌情<b>且无在途侦察</b>（须派出侦察）。</summary>
        public bool IsEmpty => Entries.Count == 0 && InTransit.Count == 0;

        /// <summary>空敌情面板（会话未启用情报时的安全降级，避免 UI 因 null 崩溃）。</summary>
        public static CampaignEnemyIntelPanelView Empty { get; } =
            new CampaignEnemyIntelPanelView(Array.Empty<CampaignEnemyIntelView>(), Array.Empty<string>());

        private CampaignEnemyIntelPanelView(IReadOnlyList<CampaignEnemyIntelView> entries, IReadOnlyList<string> inTransit)
        {
            Entries = entries;
            InTransit = inTransit;
        }

        /// <summary>
        /// 从阵营情报投影构造敌情面板（仅探报，无真值）。<paramref name="ttlSegments"/> 须与 IntelConfig 同源（≥1）；
        /// <paramref name="pending"/> 为在途侦察（尚未返报），只呈现「在途·约第 X 日返报」，<b>不</b>含任何数值。
        /// </summary>
        public static CampaignEnemyIntelPanelView FromProjection(
            IntelProjection projection, WorldTime now, int ttlSegments, IReadOnlyList<PendingScout>? pending = null)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            if (ttlSegments < 1)
                throw new ArgumentOutOfRangeException(nameof(ttlSegments), "时效阈值须≥1（与 IntelConfig.TtlSegments 同源）。");
            var list = new List<CampaignEnemyIntelView>();
            foreach (IntelKnowledgeEntry entry in projection.Entries)
                list.Add(CampaignEnemyIntelView.FromEntry(entry, now, ttlSegments));
            list.Sort((a, b) => string.CompareOrdinal(a.SubjectLabel, b.SubjectLabel));

            var transit = new List<string>();
            if (pending != null)
            {
                var ordered = new List<PendingScout>(pending);
                ordered.Sort((a, b) =>
                {
                    int c = a.ArrivalTime.AbsoluteIndex.CompareTo(b.ArrivalTime.AbsoluteIndex);
                    return c != 0 ? c : string.CompareOrdinal(a.Subject.Value, b.Subject.Value);
                });
                foreach (PendingScout p in ordered)
                    transit.Add($"{DisplayNames.Of(p.Subject.Value)}：侦察兵在途，约第 {p.ArrivalTime.Day} 日返报");
            }
            return new CampaignEnemyIntelPanelView(list, transit);
        }
    }
}
