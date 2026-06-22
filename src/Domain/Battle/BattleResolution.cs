using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 阶段解析结果（GDD_010 §Formula 1/2 / TR-battle-001/003）。
    /// 成功：携带新快照、有序事件与确定性状态哈希。
    /// 回滚：携带<b>原快照</b>（异常时整个原子阶段回滚，无半结算态）+ 错误说明。不可变。
    /// </summary>
    public sealed class BattleResolution
    {
        /// <summary>是否提交（true=阶段成功推进；false=异常已原子回滚）。</summary>
        public bool Committed { get; }

        /// <summary>结算后快照（成功为新态；回滚为原态）。</summary>
        public BattleSnapshot State { get; }

        /// <summary>有序事件（成功时；回滚为空）。</summary>
        public IReadOnlyList<BattleEvent> Events { get; }

        /// <summary>确定性状态哈希（成功时；回滚为原态哈希）。</summary>
        public StateHash Hash { get; }

        /// <summary>错误说明（回滚时非空）。</summary>
        public string? Error { get; }

        private BattleResolution(bool committed, BattleSnapshot state, IReadOnlyList<BattleEvent> events, StateHash hash, string? error)
        {
            Committed = committed;
            State = state;
            Events = events;
            Hash = hash;
            Error = error;
        }

        public static BattleResolution Commit(BattleSnapshot state, IReadOnlyList<BattleEvent> events, StateHash hash)
            => new BattleResolution(true, state, events, hash, null);

        public static BattleResolution Rollback(BattleSnapshot original, StateHash hash, string error)
            => new BattleResolution(false, original, Array.Empty<BattleEvent>(), hash, error);
    }
}
