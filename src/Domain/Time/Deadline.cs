using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>期限到期后果类型（GDD_001 §Data Model：Deadline.后果类型）。</summary>
    public enum DeadlineConsequence
    {
        /// <summary>失败（目标不可达）。</summary>
        Fail = 0,

        /// <summary>惩罚（可继续但有代价）。</summary>
        Penalty = 1,

        /// <summary>升级（触发后续事件）。</summary>
        Escalate = 2,
    }

    /// <summary>
    /// 期限（GDD_001 §Data Model：Deadline）。纯 Domain 值对象，字段完整可被存档（衔接 TR-time-003）。
    /// 剩余时间 <c>remaining = T(deadline) − T(now)</c>（GDD §Formula 5）；≤0 即到期（错过期限是可玩后果，非技术错误）。
    /// </summary>
    public sealed class Deadline
    {
        /// <summary>目标稳定 ID（非空）。</summary>
        public string TargetId { get; }

        /// <summary>到期时间。</summary>
        public WorldTime DueTime { get; }

        /// <summary>到期后果类型。</summary>
        public DeadlineConsequence Consequence { get; }

        public Deadline(string targetId, WorldTime dueTime, DeadlineConsequence consequence)
        {
            if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("TargetId 不可为空。", nameof(targetId));
            if (!Enum.IsDefined(typeof(DeadlineConsequence), consequence)) throw new ArgumentOutOfRangeException(nameof(consequence));
            TargetId = targetId;
            DueTime = dueTime;
            Consequence = consequence;
        }

        /// <summary>相对 <paramref name="now"/> 的剩余时段数（可为负，表示已逾期）。</summary>
        public long RemainingSegments(WorldTime now) => DueTime.AbsoluteIndex - now.AbsoluteIndex;

        /// <summary>是否已到期（剩余 ≤ 0）。</summary>
        public bool IsExpired(WorldTime now) => RemainingSegments(now) <= 0;

        public override string ToString() => $"Deadline({TargetId})@{DueTime}[{Consequence}]";
    }

    /// <summary>
    /// 取消行动的时间损失策略（GDD_001 §Formula 6：<c>time_lost = ceil(elapsed × cancel_loss)</c>）。
    /// 已消耗时间按比例不返还；用定点乘法（ADR-0004 禁 float 权威路径）。
    /// </summary>
    public static class CancellationPolicy
    {
        /// <param name="elapsedSegments">取消时已消耗的时段数（≥0）。</param>
        /// <param name="cancelLoss">不返还比例（0.0–1.0，配置）。</param>
        public static int ComputeTimeLost(int elapsedSegments, FixedPoint cancelLoss)
        {
            if (elapsedSegments < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSegments), "已消耗时段数不可为负。");
            if (cancelLoss < FixedPoint.Zero || cancelLoss > FixedPoint.FromInt(1))
                throw new ArgumentOutOfRangeException(nameof(cancelLoss), "取消损失比例须 ∈ [0,1]。");

            FixedPoint lost = FixedPoint.FromInt(elapsedSegments) * cancelLoss;
            return ActionCost.CeilToInt(lost);
        }
    }
}
