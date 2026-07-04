using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Presentation.Projections;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>战役主循环相位（M15 UX §1 全循环信息架构）。用于「当前可做的合法动作」过滤（AC-5）。</summary>
    public enum CampaignPhase
    {
        /// <summary>治理（城市日常 + 侦察/军议 + 起草备战）。</summary>
        Governance = 0,
        /// <summary>备战（草稿/提交/待开战）。</summary>
        Preparing = 1,
        /// <summary>战中（已开战，未结算战果）。</summary>
        Battle = 2,
        /// <summary>战后（已出战果，续局）。</summary>
        Aftermath = 3,
    }

    /// <summary>
    /// HUD 相位与可做动作视图（epic-028 story-004 / TR-ux-005 / ADR-0009）。
    /// 纯读会话相位标志，产出「当前相位 + 该相位合法可做动作集」——保证任一相位玩家都看得到下一步能做什么（AC-5）。
    /// 不可变、纯函数。<b>只列合法动作</b>，命令的真正校验仍在 <see cref="CampaignSessionService"/>（UI 不预判吞错）。
    /// </summary>
    public sealed class HudPhaseView
    {
        /// <summary>当前相位。</summary>
        public CampaignPhase Phase { get; }

        /// <summary>该相位可做动作的中文标签集（非空）。</summary>
        public IReadOnlyList<string> AvailableActions { get; }

        private HudPhaseView(CampaignPhase phase, IReadOnlyList<string> actions)
        {
            Phase = phase;
            AvailableActions = actions;
        }

        /// <summary>由会话公共相位标志判定相位并列出该相位合法动作（确定性）。</summary>
        public static HudPhaseView ForSession(CampaignSession s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (s.HasOutcome)
            {
                var actions = new List<string>();
                foreach (ContinuationOption o in s.LastContinuationOptions) actions.Add(ContinuationLabel(o.Kind));
                actions.Add("推进时段（战役继续）");
                return new HudPhaseView(CampaignPhase.Aftermath, actions);
            }

            if (s.HasBattle)
                return new HudPhaseView(CampaignPhase.Battle, new[] { "结算战果" });

            if (s.HasPreparation && (s.CommittedPlan != null || (s.PlanOrders?.Count ?? 0) > 0))
            {
                var actions = new List<string>();
                if (s.CommittedPlan != null) actions.Add("开战");
                else { actions.Add("提交计划"); actions.Add("移除设伏"); }
                return new HudPhaseView(CampaignPhase.Preparing, actions);
            }

            // 治理相位（默认）：城市日常 + 情报 + 起草备战。
            var govern = new List<string> { "推进时段", "征用军粮", "修工事", "安抚" };
            if (s.HasIntel) { govern.Add("侦察"); govern.Add("召开军议"); }
            if (s.HasPreparation) govern.Add("设伏备战");
            return new HudPhaseView(CampaignPhase.Governance, govern);
        }

        private static string ContinuationLabel(ContinuationCommandKind kind) => kind switch
        {
            ContinuationCommandKind.Pursue => "乘胜追击",
            ContinuationCommandKind.Consolidate => "巩固据点",
            ContinuationCommandKind.Regroup => "重整旗鼓",
            ContinuationCommandKind.Accountability => "问责追责",
            ContinuationCommandKind.Retreat => "且战且退",
            ContinuationCommandKind.SueForPeace => "遣使求和",
            _ => kind.ToString(),
        };
    }

    /// <summary>一条治理动作描述（标识 + 中文标签 + 对后续战役条件的<b>因果方向</b>说明）。不可变。</summary>
    public sealed class GovernanceActionDescriptor
    {
        /// <summary>动作标识（与 UXML 按钮 name 对应）。</summary>
        public string ActionId { get; }

        /// <summary>中文动作标签。</summary>
        public string Label { get; }

        /// <summary>因果说明（<b>只说方向 ↑↓，不预测精确数值</b>——无胜率精神同源，TR-ux-001 治理分量）。</summary>
        public string CausalHint { get; }

        internal GovernanceActionDescriptor(string actionId, string label, string causalHint)
        {
            ActionId = actionId;
            Label = label;
            CausalHint = causalHint;
        }
    }

    /// <summary>
    /// 治理面板视图（epic-028 story-004 / TR-ux-001 / GDD_004）：多维账本（<see cref="CityLedgerView"/>，
    /// P6 分列不合并）+ 三治理动作各带因果方向说明。因果文案<b>数据驱动</b>（集中常量表，不散落 Controller）。
    /// 不可变、纯函数。
    /// </summary>
    public sealed class GovernanceActionView
    {
        /// <summary>多维城市账本（粮草/民心/治安/工事分列，无合并总分）。</summary>
        public CityLedgerView Ledger { get; }

        /// <summary>三治理动作（征用军粮/修工事/安抚）各带因果方向说明。</summary>
        public IReadOnlyList<GovernanceActionDescriptor> Actions { get; }

        /// <summary>在办治理事务提示（GDD_004 派人处理→需时见效；一条一句「处理中，约第 X 日完成」，确定性序）。</summary>
        public IReadOnlyList<string> InProgress { get; }

        private GovernanceActionView(
            CityLedgerView ledger, IReadOnlyList<GovernanceActionDescriptor> actions, IReadOnlyList<string> inProgress)
        {
            Ledger = ledger;
            Actions = actions;
            InProgress = inProgress;
        }

        // 因果方向表（数据驱动集中承载；说明「方向」不说明「精确数值预测」，无胜率精神同源）。
        private static readonly IReadOnlyList<GovernanceActionDescriptor> CausalTable = new[]
        {
            new GovernanceActionDescriptor("requisition", "征用军粮", "↑ 备战补给　↓ 城中民心"),
            new GovernanceActionDescriptor("repair-fort", "修工事", "↑ 守城工事强度"),
            new GovernanceActionDescriptor("appease", "安抚", "↑ 城中民心"),
        };

        /// <summary>从会话城市治理态构造（含多维账本 + 三动作因果说明 + 在办事务）。未启用治理时抛。</summary>
        public static GovernanceActionView FromSession(CampaignSession s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (!s.HasCityGovernance) throw new InvalidOperationException("会话未启用城市治理。");

            var inProgress = new List<string>();
            var tasks = new List<PendingGovernanceTask>(s.PendingGovernance);
            tasks.Sort((a, b) =>
            {
                int c = a.CompletionTime.AbsoluteIndex.CompareTo(b.CompletionTime.AbsoluteIndex);
                return c != 0 ? c : ((int)a.Kind).CompareTo((int)b.Kind);
            });
            foreach (PendingGovernanceTask t in tasks)
                inProgress.Add($"{KindLabel(t.Kind)}：处理中，约第 {t.CompletionTime.Day} 日完成");

            return new GovernanceActionView(BuildLedgerView(s), CausalTable, inProgress);
        }

        private static string KindLabel(GovernanceActionKind kind) => kind switch
        {
            GovernanceActionKind.Requisition => "征用军粮",
            GovernanceActionKind.RepairFortification => "修工事",
            GovernanceActionKind.Appease => "安抚",
            _ => kind.ToString(),
        };

        /// <summary>把会话城市态映射为多维账本展示视图（日界短缺为瞬时结算输出，会话不留存 → 0/false）。</summary>
        internal static CityLedgerView BuildLedgerView(CampaignSession s)
        {
            CityEconomyState c = s.CityEconomy!;
            var projection = new CityLedgerProjection(
                c.Id.ToString(), c.Stock, c.Available, c.CivMorale, c.Security,
                c.FortificationCurrent, c.FortificationMax, lastDayShortage: 0, highUnrestRisk: false);
            return new CityLedgerView(projection);
        }
    }

    /// <summary>一条备战命令描述（标识 + 展示文本 + 是否可移除）。承诺后不可移除（不可反悔）。不可变。</summary>
    public sealed class PrepOrderDescriptor
    {
        /// <summary>命令标识。</summary>
        public string OrderId { get; }

        /// <summary>展示文本（命令 + 执行者→目标）。</summary>
        public string Label { get; }

        /// <summary>是否可移除（草稿=可，已承诺=不可）。</summary>
        public bool Removable { get; }

        internal PrepOrderDescriptor(string orderId, string label, bool removable)
        {
            OrderId = orderId;
            Label = label;
            Removable = removable;
        }
    }

    /// <summary>
    /// 备战面板视图（epic-028 story-004 / GDD_009 / TR-prep-001）：草稿命令（可移除）与已提交命令（不可反悔）
    /// <b>视觉区分</b>——<see cref="IsCommitted"/> 供 UXML/USS 切朱批承诺态。不可变、纯函数。
    /// </summary>
    public sealed class PrepPanelView
    {
        /// <summary>草稿命令（未提交，可移除）。</summary>
        public IReadOnlyList<PrepOrderDescriptor> DraftOrders { get; }

        /// <summary>已提交命令（原子承诺后，不可移除）。</summary>
        public IReadOnlyList<PrepOrderDescriptor> CommittedOrders { get; }

        /// <summary>是否已提交（→ 朱批承诺视觉态；提交不可反悔）。</summary>
        public bool IsCommitted { get; }

        /// <summary>状态一句话（草稿 N 条 / 已提交 N 条·不可反悔）。</summary>
        public string StatusLabel { get; }

        private PrepPanelView(
            IReadOnlyList<PrepOrderDescriptor> draft, IReadOnlyList<PrepOrderDescriptor> committed,
            bool isCommitted, string statusLabel)
        {
            DraftOrders = draft;
            CommittedOrders = committed;
            IsCommitted = isCommitted;
            StatusLabel = statusLabel;
        }

        /// <summary>从会话备战态构造（草稿来自 PlanOrders，承诺来自 CommittedPlan）。未启用备战时返回空面板。</summary>
        public static PrepPanelView FromSession(CampaignSession s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (!s.HasPreparation)
                return new PrepPanelView(Array.Empty<PrepOrderDescriptor>(), Array.Empty<PrepOrderDescriptor>(), false, "（本会话未启用战役准备）");

            bool isCommitted = s.CommittedPlan != null;

            // 已承诺后草稿即失效：只呈现权威的承诺清单（不可移除），不再显示残留草稿（提交不可反悔）。
            var draft = new List<PrepOrderDescriptor>();
            if (!isCommitted)
                foreach (PreparedOrder o in s.PlanOrders ?? (IReadOnlyList<PreparedOrder>)Array.Empty<PreparedOrder>())
                    draft.Add(new PrepOrderDescriptor(o.Id.Value, OrderLabel(o), removable: true));

            var committed = new List<PrepOrderDescriptor>();
            if (isCommitted)
                foreach (PreparedOrder o in s.CommittedPlan!.Orders)
                    committed.Add(new PrepOrderDescriptor(o.Id.Value, OrderLabel(o), removable: false));

            string status = isCommitted
                ? $"已提交 {committed.Count} 条——原子承诺，不可反悔。"
                : $"草稿 {draft.Count} 条（未提交，可增减）。";
            return new PrepPanelView(draft, committed, isCommitted, status);
        }

        private static string OrderLabel(PreparedOrder o)
            => $"{o.Id.Value}（{o.Executor.Value} → {o.Target.Value}）";
    }

    /// <summary>一条兵法条件进度（链名 + 已满足/未满足条件分列 + 还差条数）。<b>非可执行命令项</b>——纯状态指示。不可变。</summary>
    public sealed class TacticProgressLine
    {
        /// <summary>兵法链中文名。</summary>
        public string TacticName { get; }

        /// <summary>已满足条件（中文）。</summary>
        public IReadOnlyList<string> Satisfied { get; }

        /// <summary>未满足条件（中文）。</summary>
        public IReadOnlyList<string> Unsatisfied { get; }

        /// <summary>还差条数（0=已成型）。</summary>
        public int RemainingCount { get; }

        /// <summary>是否已成型（全部条件满足）。</summary>
        public bool IsFormed => RemainingCount == 0;

        /// <summary>进度一句话（「还差 N 条」/「已成型」）。</summary>
        public string ProgressLabel => IsFormed ? "已成型" : $"还差 {RemainingCount} 条";

        internal TacticProgressLine(
            string tacticName, IReadOnlyList<string> satisfied, IReadOnlyList<string> unsatisfied, int remainingCount)
        {
            TacticName = tacticName;
            Satisfied = satisfied;
            Unsatisfied = unsatisfied;
            RemainingCount = remainingCount;
        }
    }

    /// <summary>
    /// 兵法条件进度视图（epic-028 story-004 / GDD_010 / TR-battle-002）：对每条兵法链，按已满足条件集给出
    /// 已满足✓/未满足✗ 分列 + 「还差 N 条」。<b>兵法是条件组合，非按钮</b>——本视图<b>刻意无任何「执行/命令/按钮」字段</b>
    /// （负向不变量由 HudCampaignViewModelTests 反射断言）。不可变、纯函数。
    /// </summary>
    public sealed class BattleConditionProgressView
    {
        /// <summary>各兵法链进度（保持配置给定链序，不重排为优劣序）。</summary>
        public IReadOnlyList<TacticProgressLine> Lines { get; }

        private BattleConditionProgressView(IReadOnlyList<TacticProgressLine> lines) => Lines = lines;

        /// <summary>由兵法链配置与已满足条件集构造进度（同输入 → 同输出，确定性）。</summary>
        public static BattleConditionProgressView Build(
            TacticChainConfig chains, IReadOnlyCollection<TacticCondition> satisfied)
        {
            if (chains == null) throw new ArgumentNullException(nameof(chains));
            if (satisfied == null) throw new ArgumentNullException(nameof(satisfied));
            var satisfiedSet = new HashSet<TacticCondition>(satisfied);

            var lines = new List<TacticProgressLine>();
            foreach (TacticChainDefinition chain in chains.Chains)
            {
                var met = new List<string>();
                var unmet = new List<string>();
                foreach (TacticCondition cond in chain.Required)
                {
                    if (satisfiedSet.Contains(cond)) met.Add(ConditionLabel(cond));
                    else unmet.Add(ConditionLabel(cond));
                }
                lines.Add(new TacticProgressLine(TacticName(chain.Tag), met, unmet, unmet.Count));
            }
            return new BattleConditionProgressView(lines);
        }

        private static string TacticName(TacticTag tag) => tag switch
        {
            TacticTag.FeintAmbush => "假退伏击",
            TacticTag.SupplyExhaustion => "断粮疲敌",
            TacticTag.HoldUntilRelief => "守城待变",
            TacticTag.NightRaid => "夜袭",
            _ => tag.ToString(),
        };

        private static string ConditionLabel(TacticCondition c) => c switch
        {
            TacticCondition.ControlledRetreatKeptFormation => "受控撤退保持队形",
            TacticCondition.EnemyPursued => "敌军追击",
            TacticCondition.AmbushSurprise => "伏兵突然性",
            TacticCondition.SupplyLineCut => "切断敌补给线",
            TacticCondition.ShortageReachedGrace => "断粮达宽限时段",
            TacticCondition.EnemyCohesionCrossedThreshold => "敌士气疲劳跨阈",
            TacticCondition.HeldPosition => "守住阵地",
            TacticCondition.ReliefArrived => "外援抵达",
            TacticCondition.SurvivedDeadline => "撑过期限",
            TacticCondition.IsNight => "值夜间",
            TacticCondition.StealthSuccess => "隐蔽成功",
            TacticCondition.DefenderUnaware => "守方未察觉",
            TacticCondition.RaiderDisciplineMet => "袭方军纪达标",
            _ => c.ToString(),
        };
    }

    /// <summary>
    /// 命令错误码 → 中文文案集中映射（epic-028 story-004 / ADR-0009 R-5：命令失败稳定错误码无部分写入）。
    /// UI 按稳定错误码显示原因，<b>不做 UI 侧预判吞掉</b>（AC-5）。文案集中一处，勿散落各 Controller。
    /// </summary>
    public static class CampaignErrorText
    {
        /// <summary>把稳定错误码翻译为玩家文案；未登记码降级为通用提示（不崩 UI）。</summary>
        public static string For(CampaignErrorCode code, string detail = "") => code switch
        {
            CampaignErrorCode.None => string.Empty,
            CampaignErrorCode.NullConfig => "开局配置缺失。",
            CampaignErrorCode.InvalidConfig => "配置非法：" + detail,
            CampaignErrorCode.SessionNotFound => "找不到指定场景。",
            CampaignErrorCode.CityGovernanceDisabled => "本会话未启用城市治理。",
            CampaignErrorCode.InvalidAmount => "数量非法（须为非负，且命令 id 不重复）。",
            CampaignErrorCode.InsufficientStock => "粮草可分配量不足，征用被拒。",
            CampaignErrorCode.FortificationFull => "工事已修满，无需再修。",
            CampaignErrorCode.IntelDisabled => "本会话未启用情报。",
            CampaignErrorCode.UnknownIntelSubject => "侦察对象未登记或非法。",
            CampaignErrorCode.PreparationDisabled => "本会话未启用战役准备。",
            _ => "命令未成功（错误码：" + code + "）。",
        };
    }
}
