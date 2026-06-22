using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 战役快照（GDD_010 §Data Model：BattleState / §Formula 1）。
    /// 单位状态 + 侦测 + 配置指纹，构成确定性哈希的输入之一。提供深拷贝以支持
    /// 阶段原子回滚（在工作副本上推进，异常即丢弃，原快照不变）。
    /// </summary>
    public sealed class BattleSnapshot
    {
        private readonly Dictionary<BattleUnitId, BattleUnitState> _units;

        /// <summary>侦测状态。</summary>
        public DetectionState Detection { get; }

        /// <summary>配置指纹（GDD_010 §Formula 1 哈希输入）。</summary>
        public string ConfigFingerprint { get; }

        public BattleSnapshot(IEnumerable<BattleUnitState> units, DetectionState detection, string configFingerprint)
        {
            if (units == null) throw new ArgumentNullException(nameof(units));
            Detection = detection ?? throw new ArgumentNullException(nameof(detection));
            ConfigFingerprint = configFingerprint ?? string.Empty;
            _units = new Dictionary<BattleUnitId, BattleUnitState>();
            foreach (BattleUnitState u in units) _units[u.Id] = u;
        }

        /// <summary>全部单位（只读）。</summary>
        public IReadOnlyCollection<BattleUnitState> Units => _units.Values;

        /// <summary>取单位；不存在抛。</summary>
        public BattleUnitState Unit(BattleUnitId id)
        {
            if (!_units.TryGetValue(id, out BattleUnitState? u))
                throw new KeyNotFoundException($"无单位 {id}。");
            return u;
        }

        /// <summary>是否存在单位。</summary>
        public bool Has(BattleUnitId id) => _units.ContainsKey(id);
    }
}
