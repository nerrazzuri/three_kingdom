using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 结算上下文：本次战果涉及的阵营、据点、主将与极端败局标志。
    /// </summary>
    public sealed class OutcomeContext
    {
        /// <summary>己方阵营（名声写回目标）。</summary>
        public FactionId Faction { get; }

        /// <summary>涉及城市（可空：野战无城）。</summary>
        public CityId? City { get; }

        /// <summary>主将（可空：减员写回目标）。</summary>
        public CharacterId? Commander { get; }

        /// <summary>主将是否被俘（极端败局；不影响「仍可继续」的保证）。</summary>
        public bool CommanderCaptured { get; }

        public OutcomeContext(FactionId faction, CityId? city = null, CharacterId? commander = null, bool commanderCaptured = false)
        {
            Faction = faction;
            City = city;
            Commander = commander;
            CommanderCaptured = commanderCaptured;
        }
    }

    /// <summary>
    /// 可玩失败延续服务（gdd-010 §后果/失败延续 / ADR-0002 / 强制设计锁「失败必须产生可继续状态」）。
    /// <list type="bullet">
    ///   <item>胜/败/撤退/失城<b>均为分支结算</b>，各自生成不同变更集，<b>共用</b> Story 001 原子写回路径。</item>
    ///   <item>损失幅度数据驱动（<see cref="OutcomeConsequenceConfig"/>），并以当前值上限夹取——不写出负值。</item>
    ///   <item>任一败局至少给出一条合法可继续命令；<see cref="OutcomeContinuation"/> 构造时断言非空（败局不切死局）。</item>
    /// </list>
    /// 纯函数确定性：同 (world, branch, context, config) → 同变更集 → 同写回哈希 + 同命令集。
    /// </summary>
    public sealed class FailureContinuationService
    {
        private readonly OutcomeWritebackService _writeback = new OutcomeWritebackService();

        /// <summary>结算一个战果分支：生成变更集 → 原子写回 → 给出合法可继续命令。</summary>
        public OutcomeContinuation Resolve(OutcomeWorld world, OutcomeBranch branch, OutcomeContext context, OutcomeConsequenceConfig config)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var consequences = BuildConsequences(world, branch, context, config);
            OutcomeWritebackResult result = _writeback.Apply(world, consequences);
            IReadOnlyList<ContinuationOption> options = LegalContinuations(branch, context);

            return new OutcomeContinuation(branch, consequences, result, options);
        }

        /// <summary>是否存在 ≥1 合法可继续命令（强制设计锁的可断言形式）。</summary>
        public bool HasPlayableContinuation(OutcomeBranch branch, OutcomeContext context)
            => LegalContinuations(branch, context).Count > 0;

        // —— 变更集（分支差异在此，写回路径相同）——
        private static ConsequenceSet BuildConsequences(OutcomeWorld world, OutcomeBranch branch, OutcomeContext ctx, OutcomeConsequenceConfig cfg)
        {
            var changes = new List<OutcomeChange>();

            long repLoss = branch switch
            {
                OutcomeBranch.Victory => 0,
                OutcomeBranch.Retreat => cfg.ReputationLossRetreat,
                OutcomeBranch.CityLost => cfg.ReputationLossCityLost,
                OutcomeBranch.Defeat => cfg.ReputationLossDefeat,
                _ => 0,
            };
            if (repLoss != 0 && world.HasReputation(ctx.Faction))
                changes.Add(OutcomeChange.ForReputation(ctx.Faction, -repLoss, $"{branch} 名声受损"));

            if (ctx.City.HasValue && world.HasCity(ctx.City.Value) && branch != OutcomeBranch.Victory)
            {
                var c = world.GetCity(ctx.City.Value);
                long secLoss = branch == OutcomeBranch.CityLost ? cfg.SecurityLoss : cfg.SecurityLoss / 2;
                AddCapped(changes, ctx.City.Value, CityField.CivMorale, c.CivMorale, cfg.CivMoraleLoss, "民心动摇");
                AddCapped(changes, ctx.City.Value, CityField.Security, c.Security, secLoss, "治安受损");
                AddCapped(changes, ctx.City.Value, CityField.Fortification, c.FortificationCurrent, cfg.FortificationDamage, "工事损毁");
            }

            if (ctx.Commander.HasValue && world.HasCharacter(ctx.Commander.Value)
                && (branch == OutcomeBranch.Retreat || branch == OutcomeBranch.Defeat || branch == OutcomeBranch.CityLost))
            {
                long vit = world.GetCharacterVitality(ctx.Commander.Value);
                AddCappedChar(changes, ctx.Commander.Value, vit, cfg.ForceAttrition, "撤退减员");
            }

            return new ConsequenceSet(branch, changes);
        }

        private static void AddCapped(List<OutcomeChange> changes, CityId city, CityField field, long current, long magnitude, string reason)
        {
            long loss = Math.Min(current, magnitude); // 不写出负值（守住原子写回不变量）
            if (loss > 0) changes.Add(OutcomeChange.ForCity(city, field, -loss, reason));
        }

        private static void AddCappedChar(List<OutcomeChange> changes, CharacterId ch, long current, long magnitude, string reason)
        {
            long loss = Math.Min(current, magnitude);
            if (loss > 0) changes.Add(OutcomeChange.ForCharacter(ch, -loss, reason));
        }

        // —— 合法可继续命令（任一分支非空；败局至少撤退/失城/问责一路）——
        private static IReadOnlyList<ContinuationOption> LegalContinuations(OutcomeBranch branch, OutcomeContext ctx)
        {
            var options = new List<ContinuationOption>();
            switch (branch)
            {
                case OutcomeBranch.Victory:
                    options.Add(new ContinuationOption(ContinuationCommandKind.Pursue, "乘胜追击扩大战果"));
                    options.Add(new ContinuationOption(ContinuationCommandKind.Consolidate, "巩固据点修整"));
                    break;
                case OutcomeBranch.Retreat:
                    options.Add(new ContinuationOption(ContinuationCommandKind.Retreat, "且战且退保存实力"));
                    break;
                case OutcomeBranch.CityLost:
                    options.Add(new ContinuationOption(ContinuationCommandKind.SueForPeace, "据点既失，遣使求和争取喘息"));
                    break;
                case OutcomeBranch.Defeat:
                    break;
            }

            // 通用兜底：任何分支（含主将被俘的极端败局）始终可重整与问责 → 保证非空。
            if (branch != OutcomeBranch.Victory)
            {
                options.Add(new ContinuationOption(ContinuationCommandKind.Regroup,
                    ctx.CommanderCaptured ? "主将被俘，余部由副将重整" : "收拢余部重整旗鼓"));
                options.Add(new ContinuationOption(ContinuationCommandKind.Accountability, "复盘败因，问责追责"));
            }

            return options;
        }
    }
}
