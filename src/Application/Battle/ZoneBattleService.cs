using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Application.Battle
{
    /// <summary>一回合编排结果（新态 + 涌现事件 + 终局）。不可变。</summary>
    public sealed class ZoneBattleRoundResult
    {
        public ZoneBattleState State { get; }
        public IReadOnlyList<string> Emergences { get; }
        public ZoneBattleOutcome Outcome { get; }

        public ZoneBattleRoundResult(ZoneBattleState state, IReadOnlyList<string> emergences, ZoneBattleOutcome outcome)
        {
            State = state;
            Emergences = emergences;
            Outcome = outcome;
        }
    }

    /// <summary>
    /// 区域战斗编排（Application，ADR-0009 装配 / GDD_021 R3/R7 攻守统一）：把 Domain 引擎组成一回合循环——
    /// <b>玩家调整（命令，调用方在本调用前经 <see cref="ZoneCommandService"/> 已应用）→ 敌AI → 同步结算 → 终局判定</b>。
    /// 只编排不拥规则（规则在 Domain）。确定性：同态+同命令序+同配置 → 同结果。
    /// </summary>
    public sealed class ZoneBattleService
    {
        private readonly RoundResolutionService _rounds = new RoundResolutionService();
        private readonly EnemyZoneAiService _ai = new EnemyZoneAiService();
        private readonly ZoneBattleOutcomeService _outcome = new ZoneBattleOutcomeService();

        /// <summary>开战：以战场 + 双方支队建初始态（回合 1，空交战/记忆）。</summary>
        public ZoneBattleState Start(
            BattleField field, IReadOnlyList<Detachment> detachments, BattleSide playerSide, int maxRounds, ulong seed)
            => new ZoneBattleState(field, detachments, Array.Empty<ZoneEngagementState>(),
                new BattleClock(1, maxRounds), playerSide, seed);

        /// <summary>
        /// 推进一回合：敌AI（非玩家阵营）决策 → 同步结算 → 终局判定。玩家命令须在调用<b>前</b>应用于 <paramref name="state"/>。
        /// </summary>
        public ZoneBattleRoundResult ResolveRound(
            ZoneBattleState state, ZoneBattleContext context, ZoneBattleConfig config, EnemyAiConfig aiConfig)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            BattleSide aiSide = state.PlayerSide == BattleSide.Attacker ? BattleSide.Defender : BattleSide.Attacker;

            ZoneBattleState afterAi = _ai.Decide(state, aiSide, config, aiConfig);   // ② 敌AI
            RoundResolution res = _rounds.ResolveRound(afterAi, context, config);     // ③ 同步结算
            ZoneBattleOutcome outcome = _outcome.Evaluate(res.State);                 // ④ 终局
            return new ZoneBattleRoundResult(res.State, res.Emergences, outcome);
        }
    }
}
