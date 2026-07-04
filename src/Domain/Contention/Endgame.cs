using System;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Contention
{
    /// <summary>终局状态（GDD_018）。</summary>
    public enum EndgameStatus
    {
        Ongoing = 0,           // 争霸继续
        PlayerUnifies = 1,     // 统一天下（胜）
        PlayerEliminated = 2,  // 覆灭（负）
    }

    /// <summary>终局配置（GDD_018 §Balancing）：统一所需支配度阈值 [0,1]。不可变。</summary>
    public sealed class EndgameConfig
    {
        public FixedPoint UnificationThreshold { get; }

        public EndgameConfig(FixedPoint unificationThreshold)
        {
            if (unificationThreshold <= FixedPoint.Zero || unificationThreshold > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(unificationThreshold), "统一阈值须在 (0,1]。");
            UnificationThreshold = unificationThreshold;
        }

        /// <summary>默认：支配度过半（&gt;1/2）即统一。</summary>
        public static EndgameConfig Default { get; } = new EndgameConfig(FixedPoint.FromFraction(1, 2));
    }

    /// <summary>
    /// 统一终局判定（GDD_018 R1-R4，确定性纯函数）：玩家领城归零→覆灭；支配度达阈或群雄尽灭→统一；否则继续。
    /// 同争霸态 → 同判（可复现）。
    /// </summary>
    public sealed class EndgameService
    {
        public EndgameStatus Evaluate(ContentionState state, FactionId player, EndgameConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (state.CitiesOf(player) <= 0) return EndgameStatus.PlayerEliminated;   // 覆灭优先

            // 群雄尽灭（存续的非玩家势力为空）→ 统一。
            bool rivalsRemain = false;
            foreach (FactionId f in state.AlivePowers()) if (f != player) { rivalsRemain = true; break; }
            if (!rivalsRemain) return EndgameStatus.PlayerUnifies;

            // 支配度达阈 → 统一。
            if (state.Dominance(player) >= config.UnificationThreshold) return EndgameStatus.PlayerUnifies;

            return EndgameStatus.Ongoing;
        }
    }
}
