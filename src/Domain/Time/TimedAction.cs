using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Time
{
    /// <summary>行动状态（GDD_001 §Data Model：TimedAction.状态）。</summary>
    public enum ActionStatus
    {
        /// <summary>已声明未开始。</summary>
        Pending = 0,

        /// <summary>进行中。</summary>
        InProgress = 1,

        /// <summary>已完成。</summary>
        Completed = 2,

        /// <summary>被中断（已用时间保留）。</summary>
        Interrupted = 3,

        /// <summary>被取消。</summary>
        Cancelled = 4,
    }

    /// <summary>
    /// 有耗时行动（GDD_001 §Data Model：TimedAction）。纯 Domain 值对象，字段完整且可被存档（衔接 TR-time-003；
    /// 实际序列化经 epic-009 显式 DTO）。稳定 ID 确保 round-trip 后事件序一致（ADR-0004/0005）。
    /// </summary>
    public sealed class TimedAction
    {
        /// <summary>行动稳定 ID（非空）。</summary>
        public string ActionId { get; }

        /// <summary>发起者稳定 ID（非空）。</summary>
        public string ActorId { get; }

        /// <summary>开始时间。</summary>
        public WorldTime Start { get; }

        /// <summary>持续时段数（= 行动耗时 cost，≥1）。</summary>
        public int DurationSegments { get; }

        /// <summary>当前状态。</summary>
        public ActionStatus Status { get; }

        /// <summary>是否可中断。</summary>
        public bool Interruptible { get; }

        public TimedAction(string actionId, string actorId, WorldTime start, int durationSegments, ActionStatus status, bool interruptible)
        {
            if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("ActionId 不可为空。", nameof(actionId));
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId 不可为空。", nameof(actorId));
            if (durationSegments < 1) throw new ArgumentOutOfRangeException(nameof(durationSegments), "持续时段数须 ≥ 1。");
            if (!Enum.IsDefined(typeof(ActionStatus), status)) throw new ArgumentOutOfRangeException(nameof(status));
            ActionId = actionId;
            ActorId = actorId;
            Start = start;
            DurationSegments = durationSegments;
            Status = status;
            Interruptible = interruptible;
        }

        /// <summary>完成时间 = 开始 + 持续（GDD §Formula 3：end_time = T(start) + cost）。</summary>
        public WorldTime EndTime => Start.Advance(DurationSegments);

        /// <summary>返回仅状态变更的新实例（值对象不可变；其余字段保持，便于 round-trip 验证）。</summary>
        public TimedAction WithStatus(ActionStatus status)
            => new TimedAction(ActionId, ActorId, Start, DurationSegments, status, Interruptible);

        public override string ToString() => $"{ActionId}({ActorId}) {Start}+{DurationSegments} [{Status}]";
    }

    /// <summary>
    /// 行动耗时计算（GDD_001 §Formula 2：<c>cost = ceil(base × terrain × weather)</c>，结果 ≥ 1）。
    /// 用定点乘法（ADR-0004 禁 float 权威路径）；向上取整经 <c>ceil(x) = -floor(-x)</c>。
    /// </summary>
    public static class ActionCost
    {
        /// <param name="baseCost">基础耗时（≥1 时段，配置）。</param>
        /// <param name="terrainMod">地形修正乘数（&gt;0，配置）。</param>
        /// <param name="weatherMod">天气修正乘数（&gt;0，只减速即 ≥1，配置；本函数不强制以保通用）。</param>
        public static int Compute(int baseCost, FixedPoint terrainMod, FixedPoint weatherMod)
        {
            if (baseCost < 1) throw new ArgumentOutOfRangeException(nameof(baseCost), "基础耗时须 ≥ 1。");
            if (terrainMod <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(terrainMod), "地形修正须 > 0。");
            if (weatherMod <= FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(weatherMod), "天气修正须 > 0。");

            FixedPoint product = FixedPoint.FromInt(baseCost) * terrainMod * weatherMod;
            int cost = CeilToInt(product);
            return cost < 1 ? 1 : cost; // 任何行动至少推进 1 时段
        }

        /// <summary>向上取整：ceil(x) = -floor(-x)（FixedPoint 仅提供 FloorToInt）。</summary>
        internal static int CeilToInt(FixedPoint value) => -((-value).FloorToInt());
    }
}
