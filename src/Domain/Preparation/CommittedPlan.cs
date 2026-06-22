using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 已承诺计划（GDD_009 §Data Model：CommittedPlan / TR-prep-001）。
    /// 提交成功后的<b>不可变</b>快照：命令集合 + 已承诺资源总量。占用已同步反映到资源池
    /// （不可静默回退；取消按行动状态处理，见 GDD_009 §Formula 5，本 story 外）。
    /// </summary>
    public sealed class CommittedPlan
    {
        /// <summary>已承诺的命令（不可变快照）。</summary>
        public IReadOnlyList<PreparedOrder> Orders { get; }

        /// <summary>本次承诺扣减的资源总量（按资源类型）。</summary>
        public IReadOnlyDictionary<ResourceKey, long> CommittedResources { get; }

        public CommittedPlan(IReadOnlyList<PreparedOrder> orders, IReadOnlyDictionary<ResourceKey, long> committedResources)
        {
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (committedResources == null) throw new ArgumentNullException(nameof(committedResources));
            Orders = new List<PreparedOrder>(orders);
            CommittedResources = new Dictionary<ResourceKey, long>(committedResources);
        }
    }
}
