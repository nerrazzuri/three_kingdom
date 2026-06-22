using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 侦测状态（GDD_010 §Data Model：DetectionState / §Formula 4）。
    /// 记录「某阵营对某单位」的侦测等级。突然性来自双方认知差（非按钮）。可克隆以支持原子回滚。
    /// </summary>
    public sealed class DetectionState
    {
        private readonly Dictionary<(FactionId Observer, BattleUnitId Target), Awareness> _awareness;

        public DetectionState() => _awareness = new Dictionary<(FactionId, BattleUnitId), Awareness>();

        private DetectionState(Dictionary<(FactionId, BattleUnitId), Awareness> source)
            => _awareness = new Dictionary<(FactionId, BattleUnitId), Awareness>(source);

        /// <summary>某阵营对某单位的侦测等级（无记录视为未察觉）。</summary>
        public Awareness Of(FactionId observer, BattleUnitId target)
            => _awareness.TryGetValue((observer, target), out Awareness a) ? a : Awareness.Unaware;

        /// <summary>设置侦测等级。</summary>
        public void Set(FactionId observer, BattleUnitId target, Awareness awareness)
            => _awareness[(observer, target)] = awareness;

        /// <summary>深拷贝（用于解析的工作副本，异常回滚时丢弃）。</summary>
        public DetectionState Clone() => new DetectionState(_awareness);
    }
}
