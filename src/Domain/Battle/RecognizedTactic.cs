using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 一个被识别的兵法复盘标签（GDD_010 §Formula 10 / TR-battle-002）。
    /// 携带成立所依据的条件（即因果复盘的决定性因素，Top≤5）。<b>事后标签</b>，不含执行语义。不可变。
    /// </summary>
    public sealed class RecognizedTactic
    {
        /// <summary>复盘标签。</summary>
        public TacticTag Tag { get; }

        /// <summary>成立所依据的条件（因果复盘因素，数量 ≤5）。</summary>
        public IReadOnlyList<TacticCondition> MatchedConditions { get; }

        public RecognizedTactic(TacticTag tag, IReadOnlyList<TacticCondition> matchedConditions)
        {
            Tag = tag;
            MatchedConditions = matchedConditions ?? throw new ArgumentNullException(nameof(matchedConditions));
        }
    }
}
