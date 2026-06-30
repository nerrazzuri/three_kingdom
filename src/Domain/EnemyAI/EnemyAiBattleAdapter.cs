using System;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>
    /// 敌方 AI 决策 → 战区命令适配（ADR-0006 / TR-ai-004 / M08 接入 M06 战斗）。
    /// 把 <see cref="StrategicAction"/> 映射为 <see cref="BattleOrder"/>，驱动 M06 战斗阶段解析，
    /// 替代 M06 的确定性预设敌方命令。AI 随机经注入 <see cref="IDeterministicRandom"/>（与战斗种子同源），
    /// 纳入同一状态哈希与重放契约（ADR-0004）；AI 经 <see cref="AiWorldView"/> 决策——不读战斗真值（反全知贯穿）。
    /// </summary>
    public sealed class EnemyAiBattleAdapter
    {
        private readonly EnemyAiService _ai;

        public EnemyAiBattleAdapter(EnemyAiService? ai = null) => _ai = ai ?? new EnemyAiService();

        /// <summary>
        /// 决策并映射为战区命令。AI 经 <paramref name="view"/>（阵营知识，非真值）决策 → 映射 BattleOrder。
        /// 返回决策记录（复盘/错误信念可读）与对应命令。
        /// </summary>
        public (DecisionRecord Decision, BattleOrder Order) DecideOrder(
            AiWorldView view, PersonalityProfile personality, ScorerConfig config, FixedPoint temperature,
            IDeterministicRandom rng, int sequence, BattleUnitId actor, BattleUnitId targetUnit)
        {
            DecisionRecord decision = _ai.Decide(view, personality, config, temperature, rng);
            BattleOrder order = ToBattleOrder(decision.Selected, sequence, actor, targetUnit);
            return (decision, order);
        }

        /// <summary>战术动作 → 战区命令映射（确定性，纯函数）。</summary>
        public static BattleOrder ToBattleOrder(StrategicAction action, int sequence, BattleUnitId actor, BattleUnitId targetUnit)
            => action switch
            {
                StrategicAction.Pursue => new BattleOrder(sequence, actor, BattleOrderType.Engage, targetUnit: targetUnit),
                StrategicAction.Retreat => new BattleOrder(sequence, actor, BattleOrderType.Retreat),
                StrategicAction.Hold => new BattleOrder(sequence, actor, BattleOrderType.Hold),
                StrategicAction.FeintLure => new BattleOrder(sequence, actor, BattleOrderType.Conceal),   // 诱敌：隐蔽设伏前置
                _ => new BattleOrder(sequence, actor, BattleOrderType.Hold),
            };
    }
}
