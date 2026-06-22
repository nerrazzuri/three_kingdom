using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 复盘上下文（GDD_010 §Formula 10）。承载一阶段战役<b>事后</b>已成立的可观察条件集，
    /// 由调用方从解析结果（BattleResolution）、后勤断粮事件、情报侦测等汇集。
    /// 复盘识别<b>只读</b>此集合判定兵法是否涌现——不驱动结算，仅事后解释。不可变。
    /// </summary>
    public sealed class RetrospectiveContext
    {
        private readonly HashSet<TacticCondition> _satisfied;

        public RetrospectiveContext(IEnumerable<TacticCondition> satisfiedConditions)
        {
            if (satisfiedConditions == null) throw new ArgumentNullException(nameof(satisfiedConditions));
            _satisfied = new HashSet<TacticCondition>(satisfiedConditions);
        }

        /// <summary>某条件是否成立。</summary>
        public bool Has(TacticCondition condition) => _satisfied.Contains(condition);

        /// <summary>已成立条件数。</summary>
        public int Count => _satisfied.Count;
    }
}
