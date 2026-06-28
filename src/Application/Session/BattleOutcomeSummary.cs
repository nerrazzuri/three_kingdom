using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Career;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 战役结果摘要——M00 消费的稳定 BattleOutcome 契约（ADR-0009 §CD 护栏 ⑥ 契约冻结）。不可变。
    /// 携胜负 + <b>≤5 决定性因素 CausalTrace</b>（CD 护栏 ①：结算可读，非黑盒）。
    /// <para>
    /// 代偿路径（断粮/守城待变）与未来完整 GDD_010 战役命令层（B3）<b>产出同一摘要 schema</b>——
    /// 使 B3 深化为纯叠加、零 M00 返工（§5b.1 裁决 + 护栏 ⑥）。
    /// </para>
    /// </summary>
    public sealed class BattleOutcomeSummary
    {
        /// <summary>守城胜负。</summary>
        public SiegeOutcome Outcome { get; }

        /// <summary>≤5 决定性因素（可读复盘，CD 护栏 ①）。</summary>
        public IReadOnlyList<string> DecisiveFactors { get; }

        public BattleOutcomeSummary(SiegeOutcome outcome, IReadOnlyList<string> decisiveFactors)
        {
            if (decisiveFactors is null) throw new ArgumentNullException(nameof(decisiveFactors));
            if (decisiveFactors.Count > 5)
                throw new ArgumentException("决定性因素须 ≤5（CD 护栏：战果可读，非黑盒）。", nameof(decisiveFactors));
            var arr = new string[decisiveFactors.Count];
            for (int i = 0; i < decisiveFactors.Count; i++)
                arr[i] = decisiveFactors[i] ?? throw new ArgumentException("决定性因素不可含 null。", nameof(decisiveFactors));
            Outcome = outcome;
            DecisiveFactors = arr;
        }
    }
}
