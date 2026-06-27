using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 自立状态（GDD_014 §Data Model：RebellionState / TR-career-002）。不可变。
    /// 记录自立触发标志、发动时<b>好感快照</b>、结局分支与新势力初始态。
    /// <para>
    /// 好感快照在发动瞬间固化（<see cref="AffinitySnapshot"/>，按定点值升序），分支据此判定；
    /// <b>发动后好感变动不回溯改分支</b>（GDD_014 §Edge Cases 快照隔离）。纳入存档哈希（story-005）。
    /// </para>
    /// </summary>
    public sealed class RebellionState
    {
        private readonly FixedPoint[] _affinitySnapshot; // 升序

        /// <summary>自立已触发。</summary>
        public bool Triggered { get; }

        /// <summary>发动时僚属好感快照（升序，确定性遍历）。</summary>
        public IReadOnlyList<FixedPoint> AffinitySnapshot => _affinitySnapshot;

        /// <summary>忠诚比率 loyal_ratio（定点 ∈[0,1]，由快照与 defect_threshold 算得）。</summary>
        public FixedPoint LoyalRatio { get; }

        /// <summary>结局分支。</summary>
        public RebellionOutcome Outcome { get; }

        /// <summary>新势力 id（拥立/部分跟随时非空；众叛亲离沦为流浪时为 null）。</summary>
        public FactionId? NewFaction { get; }

        public RebellionState(
            IReadOnlyList<FixedPoint> affinitySnapshot,
            FixedPoint loyalRatio,
            RebellionOutcome outcome,
            FactionId? newFaction)
        {
            if (affinitySnapshot is null) throw new ArgumentNullException(nameof(affinitySnapshot));
            if (!Enum.IsDefined(typeof(RebellionOutcome), outcome))
                throw new ArgumentOutOfRangeException(nameof(outcome), "未定义的自立结局。");

            var sorted = new List<FixedPoint>(affinitySnapshot);
            sorted.Sort((a, b) => a.CompareTo(b));
            _affinitySnapshot = sorted.ToArray();

            Triggered = true;
            LoyalRatio = loyalRatio;
            Outcome = outcome;
            NewFaction = newFaction;
        }

        /// <summary>以规范顺序追加到状态哈希（ADR-0004）。顺序：触发标志 → ratio → (int)结局 → 新势力(有无+串) → 快照。</summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append(Triggered);
            hasher.Append(LoyalRatio);
            hasher.Append((int)Outcome);
            hasher.Append(NewFaction.HasValue);
            string fv = NewFaction.HasValue ? NewFaction.Value.Value : string.Empty;
            hasher.Append(fv.Length);
            foreach (char c in fv) hasher.Append((int)c);
            hasher.Append(_affinitySnapshot.Length);
            foreach (FixedPoint a in _affinitySnapshot) hasher.Append(a);
        }
    }
}
