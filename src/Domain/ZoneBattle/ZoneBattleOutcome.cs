using System;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>区域战斗终局（GDD_021 R3 核心循环终止）。</summary>
    public enum ZoneBattleOutcome
    {
        /// <summary>未分胜负，继续下一回合。</summary>
        Ongoing = 0,
        /// <summary>攻方胜（守方溃散/破城）。</summary>
        AttackerVictory = 1,
        /// <summary>守方胜（攻方溃散或超时未克退兵）。</summary>
        DefenderVictory = 2,
    }

    /// <summary>
    /// 终局判定（GDD_021 R3 / ADR-0012，确定性纯函数）：
    /// 守方溃散 → 攻方破城；攻方溃散 → 守方胜；<b>超时未克 → 守方胜、攻方退兵</b>（失败可继续，红线）。
    /// </summary>
    public sealed class ZoneBattleOutcomeService
    {
        public ZoneBattleOutcome Evaluate(ZoneBattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.IsRouted(BattleSide.Attacker)) return ZoneBattleOutcome.DefenderVictory;
            if (state.IsRouted(BattleSide.Defender)) return ZoneBattleOutcome.AttackerVictory;

            // 破正面即破城：守方正面无在场支队而攻方在正面（攻破城门）。
            bool attackerAtFront = AnyActive(state, BattleSide.Attacker, BattleField.Front);
            bool defenderAtFront = AnyActive(state, BattleSide.Defender, BattleField.Front);
            if (attackerAtFront && !defenderAtFront) return ZoneBattleOutcome.AttackerVictory;

            if (state.Clock.IsExpired) return ZoneBattleOutcome.DefenderVictory;   // 攻方未能及时破城 → 退兵
            return ZoneBattleOutcome.Ongoing;
        }

        private static bool AnyActive(ZoneBattleState state, BattleSide side, ZoneId zone)
        {
            foreach (Detachment d in state.DetachmentsIn(zone))
                if (d.Side == side && !d.InTransit && !d.IsBroken) return true;
            return false;
        }
    }
}
