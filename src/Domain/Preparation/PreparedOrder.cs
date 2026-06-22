using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 一条准备命令（GDD_009 §Data Model：PreparedOrder）。
    /// 含执行者、目标、时间窗、资源需求与命令依赖。<b>基础命令</b>——不含「伏兵成功」等结果命令。
    /// 不可变值对象（草稿中以集合形式编辑；构造校验非空与非负需求）。
    /// </summary>
    public sealed class PreparedOrder
    {
        /// <summary>命令 ID（依赖图节点）。</summary>
        public OrderId Id { get; }

        /// <summary>执行者（GDD_005 人物）。</summary>
        public CharacterId Executor { get; }

        /// <summary>目标区域（GDD_003；可达性校验对象）。</summary>
        public RegionId Target { get; }

        /// <summary>时间窗（占用冲突校验对象）。</summary>
        public TimeWindow Window { get; }

        /// <summary>资源需求（按资源类型，数量 ≥0）。</summary>
        public IReadOnlyDictionary<ResourceKey, long> ResourceNeeds { get; }

        /// <summary>本命令依赖的前置命令（须先完成；构成依赖图的边）。</summary>
        public IReadOnlyList<OrderId> Dependencies { get; }

        public PreparedOrder(
            OrderId id,
            CharacterId executor,
            RegionId target,
            TimeWindow window,
            IReadOnlyDictionary<ResourceKey, long>? resourceNeeds = null,
            IReadOnlyList<OrderId>? dependencies = null)
        {
            Id = id;
            Executor = executor;
            Target = target;
            Window = window;

            var needs = new Dictionary<ResourceKey, long>();
            if (resourceNeeds != null)
                foreach (KeyValuePair<ResourceKey, long> kv in resourceNeeds)
                {
                    if (kv.Value < 0) throw new ArgumentOutOfRangeException(nameof(resourceNeeds), "资源需求不可为负。");
                    needs[kv.Key] = kv.Value;
                }
            ResourceNeeds = needs;
            Dependencies = dependencies ?? Array.Empty<OrderId>();
        }
    }
}
