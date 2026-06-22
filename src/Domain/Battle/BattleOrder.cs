using System;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 基础战役命令（GDD_010 §Data Model：BattleOrder）。
    /// 表达动作不保证结果。<see cref="Sequence"/> 为稳定提交序号——命令流按此排序，
    /// 保证有序命令流的确定性解析（TR-battle-001）。不可变。
    /// </summary>
    public sealed class BattleOrder
    {
        /// <summary>稳定提交序号（命令流排序键，确定性）。</summary>
        public int Sequence { get; }

        /// <summary>执行单位。</summary>
        public BattleUnitId Actor { get; }

        /// <summary>命令类型。</summary>
        public BattleOrderType Type { get; }

        /// <summary>目标区域（Move 用；否则可为 null）。</summary>
        public RegionId? TargetRegion { get; }

        /// <summary>目标单位（Scout/Engage 用；否则可为 null）。</summary>
        public BattleUnitId? TargetUnit { get; }

        public BattleOrder(int sequence, BattleUnitId actor, BattleOrderType type,
            RegionId? targetRegion = null, BattleUnitId? targetUnit = null)
        {
            if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence), "提交序号不可为负。");
            Sequence = sequence;
            Actor = actor;
            Type = type;
            TargetRegion = targetRegion;
            TargetUnit = targetUnit;
        }
    }
}
