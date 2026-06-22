using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Preparation
{
    /// <summary>
    /// 可承诺资源池（GDD_009：权威资源 state 的提交侧视图 / TR-prep-001）。
    /// 不可变：提交成功才经 <see cref="Deduct"/> 产生新池（原子），失败则原池不变（全有或全无）。
    /// 草稿编辑<b>绝不</b>触及本池——以 <see cref="Hash"/> 可验证草稿零副作用。资源为权威整数。
    /// </summary>
    public sealed class ResourcePool
    {
        private readonly SortedDictionary<ResourceKey, long> _resources;

        public ResourcePool(IReadOnlyDictionary<ResourceKey, long> resources)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            _resources = new SortedDictionary<ResourceKey, long>(Comparer<ResourceKey>.Default);
            foreach (KeyValuePair<ResourceKey, long> kv in resources)
            {
                if (kv.Value < 0) throw new ArgumentOutOfRangeException(nameof(resources), "资源量不可为负。");
                _resources[kv.Key] = kv.Value;
            }
        }

        /// <summary>某资源当前量（无记录视为 0）。</summary>
        public long Get(ResourceKey key) => _resources.TryGetValue(key, out long v) ? v : 0L;

        /// <summary>导出可承诺量快照（供校验上下文使用）。</summary>
        public IReadOnlyDictionary<ResourceKey, long> AsAvailable()
            => new Dictionary<ResourceKey, long>(_resources);

        /// <summary>
        /// 原子扣减需求并返回新池（单一事务）。任一资源不足抛——调用方须先经校验保证可提交，
        /// 故正常路径不会触发（防御性，杜绝静默部分写入）。
        /// </summary>
        public ResourcePool Deduct(IReadOnlyDictionary<ResourceKey, long> needs)
        {
            if (needs == null) throw new ArgumentNullException(nameof(needs));
            var next = new Dictionary<ResourceKey, long>(_resources);
            foreach (KeyValuePair<ResourceKey, long> kv in needs)
            {
                long current = next.TryGetValue(kv.Key, out long v) ? v : 0L;
                long remaining = current - kv.Value;
                if (remaining < 0) throw new InvalidOperationException($"资源 {kv.Key} 不足，拒绝扣减（应已被校验拦截）。");
                next[kv.Key] = remaining;
            }
            return new ResourcePool(next);
        }

        /// <summary>确定性状态哈希（规范化排序顺序无关；用于证明草稿零副作用，ADR-0004）。</summary>
        public StateHash Hash()
        {
            var hasher = new StateHasher();
            foreach (KeyValuePair<ResourceKey, long> kv in _resources)
            {
                foreach (char c in kv.Key.Value) hasher.Append((int)c);
                hasher.Append(kv.Value);
            }
            return hasher.ToHash();
        }
    }
}
