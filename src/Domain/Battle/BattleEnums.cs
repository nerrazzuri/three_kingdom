using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>战役单位 ID（GDD_010：BattleUnitState）。序数比较，非空。</summary>
    public readonly struct BattleUnitId : IEquatable<BattleUnitId>, IComparable<BattleUnitId>
    {
        /// <summary>标识字符串（序数语义）。</summary>
        public string Value { get; }

        public BattleUnitId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("BattleUnitId 不可为空或空白。", nameof(value));
            Value = value;
        }

        public int CompareTo(BattleUnitId other) => string.CompareOrdinal(Value, other.Value);
        public bool Equals(BattleUnitId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is BattleUnitId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public static bool operator ==(BattleUnitId a, BattleUnitId b) => a.Equals(b);
        public static bool operator !=(BattleUnitId a, BattleUnitId b) => !a.Equals(b);
        public override string ToString() => Value ?? "<null>";
    }

    /// <summary>侦测状态（GDD_010 §Formula 4：突然性来自双方认知差，非按钮）。</summary>
    public enum Awareness
    {
        /// <summary>未察觉。</summary>
        Unaware = 0,

        /// <summary>怀疑。</summary>
        Suspect = 1,

        /// <summary>确认。</summary>
        Confirmed = 2,
    }

    /// <summary>基础命令类型（GDD_010 §Main Rules：命令表达动作，不保证结果）。</summary>
    public enum BattleOrderType
    {
        /// <summary>移动到目标区域。</summary>
        Move = 0,

        /// <summary>坚守。</summary>
        Hold = 1,

        /// <summary>接战。</summary>
        Engage = 2,

        /// <summary>侦察（更新对目标的侦测）。</summary>
        Scout = 3,

        /// <summary>受控撤退。</summary>
        Retreat = 4,

        /// <summary>隐蔽待命。</summary>
        Conceal = 5,
    }

    /// <summary>
    /// 阶段解析管线步骤（GDD_010 §Formula 2 / TR-battle-003）。
    /// 枚举序数即权威固定顺序：验证→移动→侦测→交战→损耗→士气→触发→发布。
    /// </summary>
    public enum BattlePhaseStep
    {
        Validate = 0,
        Move = 1,
        Detect = 2,
        Engage = 3,
        Casualty = 4,
        Cohesion = 5,
        Trigger = 6,
        Publish = 7,
    }

    /// <summary>阶段管线的权威固定顺序。</summary>
    public static class BattlePhasePipeline
    {
        /// <summary>规范解析顺序（不可变）。</summary>
        public static readonly IReadOnlyList<BattlePhaseStep> CanonicalOrder = new[]
        {
            BattlePhaseStep.Validate, BattlePhaseStep.Move, BattlePhaseStep.Detect, BattlePhaseStep.Engage,
            BattlePhaseStep.Casualty, BattlePhaseStep.Cohesion, BattlePhaseStep.Trigger, BattlePhaseStep.Publish,
        };
    }
}
