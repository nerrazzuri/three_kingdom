using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 计划提交服务（GDD_009 §Formula 1/3 / TR-prep-001 / ADR-0002）。
    /// 承载 SubmitDeploymentPlan 的原子事务：先经 <see cref="PlanValidator"/> 全量校验，
    /// 任一硬冲突 → 失败、资源池<b>原样不变</b>、返回稳定错误码（零部分写入）；
    /// 全部通过 → <b>一次性</b>扣减资源并生成不可变 <see cref="CommittedPlan"/>（全有或全无）。
    /// 纯函数：不就地修改输入池/草稿，新状态以返回值表达。
    /// </summary>
    public sealed class PlanCommitService
    {
        private readonly PlanValidator _validator = new PlanValidator();

        /// <summary>
        /// 提交计划草稿（GDD_009 §Formula 1/3）。
        /// </summary>
        /// <param name="draft">计划草稿（不被修改）。</param>
        /// <param name="pool">当前可承诺资源池（不被修改；成功时返回扣减后的新池）。</param>
        /// <param name="reachableRegions">可达区域（GDD_003）。</param>
        /// <param name="authorizedOrders">已授权命令（GDD_005/006）。</param>
        /// <param name="config">校验配置。</param>
        public SubmitPlanResult Submit(
            PlanDraft draft,
            ResourcePool pool,
            IEnumerable<RegionId> reachableRegions,
            IEnumerable<OrderId> authorizedOrders,
            PreparationConfig config)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var context = new PreparationContext(reachableRegions, pool.AsAvailable(), authorizedOrders);
            PlanValidationResult validation = _validator.Validate(draft.Orders, context, config);

            // 任一硬冲突 → 全单拒绝，资源池不变（全有或全无，无部分写入）。
            if (!validation.CanCommit)
                return SubmitPlanResult.Failure(pool, validation);

            // 校验通过 → 一次性原子扣减并生成承诺快照。
            var totalNeeds = AggregateNeeds(draft.Orders);
            ResourcePool committedPool = pool.Deduct(totalNeeds);
            var plan = new CommittedPlan(draft.Orders, totalNeeds);
            return SubmitPlanResult.Success(plan, committedPool, validation);
        }

        private static IReadOnlyDictionary<ResourceKey, long> AggregateNeeds(IReadOnlyList<PreparedOrder> orders)
        {
            var totals = new Dictionary<ResourceKey, long>();
            foreach (PreparedOrder o in orders)
                foreach (KeyValuePair<ResourceKey, long> kv in o.ResourceNeeds)
                    totals[kv.Key] = (totals.TryGetValue(kv.Key, out long cur) ? cur : 0L) + kv.Value;
            return totals;
        }
    }
}
