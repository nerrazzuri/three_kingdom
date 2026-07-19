using System;
using ThreeKingdom.Domain.Battle;

namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>
    /// 格子战斗中的一支部队（GDD-028 §3.2）。位姿/兵力在引擎内可变（Advance 于克隆态上推进），
    /// 对外只读。速度/射程由兵种经 <see cref="GridBattleConfig"/> 派生（数据驱动）。
    /// </summary>
    public sealed class GridUnit
    {
        /// <summary>部队 ID（复用 Battle.BattleUnitId，序数比较，用于确定性规范序）。</summary>
        public BattleUnitId Id { get; }
        /// <summary>所属方。</summary>
        public GridSide Side { get; }
        /// <summary>兵种。</summary>
        public TroopKind Kind { get; }
        /// <summary>显示名（表现用，不影响权威结算）。</summary>
        public string Name { get; }
        /// <summary>当前格。</summary>
        public GridCoord Position { get; internal set; }
        /// <summary>当前兵力（≤0 即溃灭/叛散）。</summary>
        public int Strength { get; internal set; }
        /// <summary>兵力上限（回血封顶）。</summary>
        public int MaxStrength { get; }
        /// <summary>单体攻击力（整数）。</summary>
        public int Attack { get; }
        /// <summary>目的地（null=未指派/据守）。</summary>
        public GridCoord? Destination { get; internal set; }

        /// <summary>是否存活。</summary>
        public bool Alive => Strength > 0;
        /// <summary>是否行军中（有目的地且未抵达）。</summary>
        public bool EnRoute => Destination.HasValue && Destination.Value != Position;

        public GridUnit(BattleUnitId id, GridSide side, TroopKind kind, GridCoord position,
            int strength, int maxStrength, int attack, GridCoord? destination = null, string? name = null)
        {
            if (strength < 0) throw new ArgumentOutOfRangeException(nameof(strength));
            if (maxStrength <= 0) throw new ArgumentOutOfRangeException(nameof(maxStrength));
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack));
            Id = id;
            Side = side;
            Kind = kind;
            Position = position;
            Strength = strength;
            MaxStrength = maxStrength;
            Attack = attack;
            Destination = destination;
            Name = string.IsNullOrEmpty(name) ? id.Value : name!;
        }

        /// <summary>深拷贝（供 <see cref="GridBattleState.Clone"/> 用）。</summary>
        public GridUnit Clone()
            => new GridUnit(Id, Side, Kind, Position, Strength, MaxStrength, Attack, Destination, Name);
    }
}
