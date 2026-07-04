using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 被挖角结算结果（GDD_014 忠诚经营）。不可变。<see cref="Left"/>=true 表示该僚属叛离，
    /// <see cref="State"/> 为移除后新态；否则原态不变。<see cref="Chance"/> 供测试/复盘（玩家侧不呈现数字）。
    /// </summary>
    public sealed class PoachResult
    {
        /// <summary>被挖走的僚属。</summary>
        public CharacterId Member { get; }
        /// <summary>是否叛离。</summary>
        public bool Left { get; }
        /// <summary>结算后部曲态（叛离则已移除该员并撤其官职；否则原态）。</summary>
        public RetinueState State { get; }
        /// <summary>被挖角概率（[0,1]；忠者不可挖时为 0）。</summary>
        public FixedPoint Chance { get; }

        public PoachResult(CharacterId member, bool left, RetinueState state, FixedPoint chance)
        {
            Member = member;
            Left = left;
            State = state;
            Chance = chance;
        }
    }
}
