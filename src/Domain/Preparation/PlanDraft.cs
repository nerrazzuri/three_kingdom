using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 计划草稿（GDD_009 §Data Model：PlanDraft / TR-prep-001）。
    /// 玩家在 Presentation/Application 侧构建的<b>可变</b>命令集合。<b>不修改任何权威 gameplay state</b>——
    /// 增删命令只改本草稿自身（草稿后权威状态哈希不变）。提交经 <see cref="PlanCommitService"/>，
    /// 全部校验通过才原子生成 <see cref="CommittedPlan"/>（P4 草稿/承诺双态）。
    /// </summary>
    public sealed class PlanDraft
    {
        private readonly List<PreparedOrder> _orders = new List<PreparedOrder>();

        /// <summary>当前草稿命令（只读视图）。</summary>
        public IReadOnlyList<PreparedOrder> Orders => _orders;

        /// <summary>加入一条命令草稿（仅改草稿，无副作用）。同 ID 已存在则抛。</summary>
        public void AddOrder(PreparedOrder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            foreach (PreparedOrder o in _orders)
                if (o.Id == order.Id) throw new InvalidOperationException($"命令 {order.Id} 已存在于草稿。");
            _orders.Add(order);
        }

        /// <summary>移除一条命令草稿（仅改草稿，无副作用）。不存在返回 false。</summary>
        public bool RemoveOrder(OrderId id)
        {
            for (int i = 0; i < _orders.Count; i++)
                if (_orders[i].Id == id) { _orders.RemoveAt(i); return true; }
            return false;
        }

        /// <summary>清空草稿。</summary>
        public void Clear() => _orders.Clear();
    }
}
