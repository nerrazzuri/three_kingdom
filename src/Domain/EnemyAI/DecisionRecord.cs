using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>
    /// 敌方 AI 决策记录（ADR-0006 §1/§3 / TR-ai-003）。决策定局后的不可变产物：
    /// 选中动作 + 缘由码 + 全部候选评分 + AI 感知的敌军兵力（<b>错误信念可读</b>——供玩家复盘 / LLM 下游装饰）。
    /// <para>LLM（后续）<b>只读</b>本记录产出台词/战报，不回写状态、不入哈希（ADR-0006 §3）。</para>
    /// </summary>
    public sealed class DecisionRecord
    {
        /// <summary>选中的动作。</summary>
        public StrategicAction Selected { get; }

        /// <summary>选择缘由码（使决策依据对玩家可读）。</summary>
        public AiReasonCode Reason { get; }

        /// <summary>全部候选动作评分（含被可行性门淘汰者，供复盘）。</summary>
        public IReadOnlyList<ScoredAction> Candidates { get; }

        /// <summary>AI 当时感知的敌军兵力（来自情报，<b>可能与真值不同</b>——错误信念可读）。</summary>
        public int PerceivedEnemyForce { get; }

        public DecisionRecord(StrategicAction selected, AiReasonCode reason, IReadOnlyList<ScoredAction> candidates, int perceivedEnemyForce)
        {
            Selected = selected;
            Reason = reason;
            Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
            PerceivedEnemyForce = perceivedEnemyForce;
        }
    }
}
