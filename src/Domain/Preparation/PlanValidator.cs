using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 计划校验器（GDD_009 §Formula 1/2 / TR-prep-002 / ADR-0004）。
    /// 纯函数、确定性：检测五类硬冲突（占用/资源/可达/权限/循环依赖）并区分软风险。
    /// 错误一次性聚合（非首个即停）；遍历按稳定 OrderId 序，平局确定。命令依赖图须为 DAG，
    /// 用拓扑排序检环。校验<b>只读</b>不改任何状态（提交事务在 Story 001）。
    /// </summary>
    public sealed class PlanValidator
    {
        /// <summary>校验一组命令（GDD_009 §Formula 1：任一硬冲突阻断提交，全部聚合返回）。</summary>
        public PlanValidationResult Validate(
            IReadOnlyList<PreparedOrder> orders, PreparationContext context, PreparationConfig config)
        {
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var sorted = new List<PreparedOrder>(orders);
            sorted.Sort((a, b) => a.Id.CompareTo(b.Id)); // 稳定遍历序（确定性）

            var errors = new List<PlanError>();
            var risks = new List<PlanRisk>();

            CheckResources(sorted, context, config, errors, risks);
            CheckTimeOverlap(sorted, errors);
            CheckReachabilityAndAuthority(sorted, context, errors);
            CheckCycles(sorted, errors);

            return new PlanValidationResult(errors, risks);
        }

        /// <summary>资源门控（§Formula 1 resource_short）：need&gt;avail 即错误；恰好够/余量薄列风险。</summary>
        private static void CheckResources(
            List<PreparedOrder> orders, PreparationContext context, PreparationConfig config,
            List<PlanError> errors, List<PlanRisk> risks)
        {
            var totalNeed = new SortedDictionary<ResourceKey, long>(Comparer<ResourceKey>.Default);
            foreach (PreparedOrder o in orders)
                foreach (KeyValuePair<ResourceKey, long> kv in o.ResourceNeeds)
                    totalNeed[kv.Key] = (totalNeed.TryGetValue(kv.Key, out long cur) ? cur : 0L) + kv.Value;

            foreach (KeyValuePair<ResourceKey, long> kv in totalNeed)
            {
                long avail = context.Available(kv.Key);
                if (kv.Value > avail)
                    errors.Add(new PlanError(PlanErrorCode.ResourceShortage, $"资源 {kv.Key} 需求 {kv.Value} 超过可承诺 {avail}。"));
                else if (kv.Value > 0 && avail - kv.Value <= config.TightResourceMargin)
                    risks.Add(new PlanRisk(PlanRiskCode.TightResource, $"资源 {kv.Key} 余量薄（可承诺 {avail}，需求 {kv.Value}）。"));
            }
        }

        /// <summary>占用冲突（§Formula 1 time_overlap）：同一执行者两命令时间窗重叠。</summary>
        private static void CheckTimeOverlap(List<PreparedOrder> orders, List<PlanError> errors)
        {
            for (int i = 0; i < orders.Count; i++)
                for (int j = i + 1; j < orders.Count; j++)
                    if (orders[i].Executor == orders[j].Executor && orders[i].Window.Overlaps(orders[j].Window))
                        errors.Add(new PlanError(PlanErrorCode.TimeOverlap,
                            $"执行者 {orders[i].Executor} 命令 {orders[i].Id} 与 {orders[j].Id} 时间窗重叠。"));
        }

        /// <summary>可达 + 权限门控（§Formula 1 unreachable / no_authority）。</summary>
        private static void CheckReachabilityAndAuthority(
            List<PreparedOrder> orders, PreparationContext context, List<PlanError> errors)
        {
            foreach (PreparedOrder o in orders)
            {
                if (!context.IsReachable(o.Target))
                    errors.Add(new PlanError(PlanErrorCode.Unreachable, $"命令 {o.Id} 目标 {o.Target} 不可达。"));
                if (!context.IsAuthorized(o.Id))
                    errors.Add(new PlanError(PlanErrorCode.NoAuthority, $"命令 {o.Id} 执行者 {o.Executor} 无权限。"));
            }
        }

        /// <summary>循环依赖检测（§Formula 2）：Kahn 拓扑排序，无法全部出队即存在环。</summary>
        private static void CheckCycles(List<PreparedOrder> orders, List<PlanError> errors)
        {
            var ids = new HashSet<OrderId>();
            foreach (PreparedOrder o in orders) ids.Add(o.Id);

            var indegree = new SortedDictionary<OrderId, int>(Comparer<OrderId>.Default);
            var dependents = new Dictionary<OrderId, List<OrderId>>(); // dep -> 依赖它的命令
            foreach (PreparedOrder o in orders) indegree[o.Id] = 0;

            foreach (PreparedOrder o in orders)
                foreach (OrderId dep in o.Dependencies)
                {
                    if (!ids.Contains(dep)) continue; // 跳过指向未知命令的边
                    indegree[o.Id] += 1;
                    if (!dependents.TryGetValue(dep, out List<OrderId>? list)) dependents[dep] = list = new List<OrderId>();
                    list.Add(o.Id);
                }

            var queue = new List<OrderId>();
            foreach (KeyValuePair<OrderId, int> kv in indegree) if (kv.Value == 0) queue.Add(kv.Key);

            int processed = 0;
            while (queue.Count > 0)
            {
                queue.Sort(); // 稳定出队序
                OrderId node = queue[0];
                queue.RemoveAt(0);
                processed++;
                if (dependents.TryGetValue(node, out List<OrderId>? deps))
                    foreach (OrderId d in deps)
                        if (--indegree[d] == 0) queue.Add(d);
            }

            if (processed < ids.Count)
                errors.Add(new PlanError(PlanErrorCode.CyclicDependency, $"命令依赖图存在环（{ids.Count - processed} 个命令无法拓扑排序）。"));
        }
    }
}
