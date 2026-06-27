using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 一名核心部曲/僚属及其对玩家的好感（GDD_014 §Data Model：RetinueState「列表及其好感（引用 GDD_006）」）。
    /// 不可变。好感为 Q16.16 定点 ∈[0,1]（权威路径禁 float，ADR-0004）；
    /// 其权威来源为 GDD_006 RelationshipState，本骨架按快照值持有，story-005 存档纳入「好感快照」。
    /// </summary>
    public sealed class RetinueMember
    {
        /// <summary>僚属人物 ID（GDD_005 稳定身份）。</summary>
        public CharacterId Character { get; }

        /// <summary>对玩家好感（定点 ∈[0,1]）。</summary>
        public FixedPoint Affinity { get; }

        /// <summary>构造并校验好感范围。越界即抛，无部分写入。</summary>
        public RetinueMember(CharacterId character, FixedPoint affinity)
        {
            if (affinity < FixedPoint.Zero || affinity > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(affinity), "僚属好感须在 [0,1]。");
            Character = character;
            Affinity = affinity;
        }
    }
}
