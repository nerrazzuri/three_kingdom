using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Subversion
{
    /// <summary>
    /// 施计结算产出（GDD_024 §6）。不可变。含结果类型、战斗接缝效果、成功度（供 UI/测试，无胜率呈现给玩家），
    /// 及副作用标志（<see cref="Exposed"/> 反噬时暴露情报，供 Application 收紧侦察门 / 记守将怨恨）。
    /// </summary>
    public sealed class SubversionOutcome
    {
        /// <summary>结果类型。</summary>
        public SubversionResult Result { get; }
        /// <summary>战斗接缝效果（无效/门不齐 → <see cref="SubversionEffect.None"/>）。</summary>
        public SubversionEffect Effect { get; }
        /// <summary>结算用成功度 s（[0,1]，供测试与内部；玩家侧只给条件/风险，不给数字）。</summary>
        public FixedPoint Chance { get; }
        /// <summary>是否暴露（反噬时 true：守将怨你↑、该城侦察门收紧，R4）。</summary>
        public bool Exposed { get; }

        public SubversionOutcome(SubversionResult result, SubversionEffect effect, FixedPoint chance, bool exposed)
        {
            Result = result;
            Effect = effect ?? SubversionEffect.None;
            Chance = chance;
            Exposed = exposed;
        }

        internal void AppendTo(StateHasher hasher)
        {
            hasher.Append((int)Result).Append(Chance).Append(Exposed);
            Effect.AppendTo(hasher);
        }
    }
}
