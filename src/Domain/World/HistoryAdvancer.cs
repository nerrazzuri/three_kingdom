using System;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 历史事件推进器（GDD_015 / ADR-0007 §2 reachability 触发门 / TR-world-002）。纯函数、确定性、无随机。
    /// 到达事件时间窗时按可达性短路选择结局：够不着→正常（不评估前置）；够得着且前置成立→正常；
    /// 够得着且前置被破坏→置 diverged + 分叉结局。下游重评估属 story-003，本服务不做。
    /// </summary>
    public sealed class HistoryAdvancer
    {
        private readonly IReachPredicate _reach;

        /// <summary>注入可达性谓词（默认 <see cref="SubjectReachPredicate"/>）。</summary>
        public HistoryAdvancer(IReachPredicate reachPredicate)
        {
            _reach = reachPredicate ?? throw new ArgumentNullException(nameof(reachPredicate));
        }

        public HistoryAdvancer() : this(new SubjectReachPredicate()) { }

        /// <summary>
        /// 处理事件 <paramref name="eventId"/> 的时间窗到达。事件不在目录抛（调用方应只传目录内事件）。
        /// 已触发 → 幂等短路（不重复触发）。返回推进后世界态与触发结局。
        /// </summary>
        public HistoryAdvanceResult OnTimeWindowEnter(
            HistoricalEventCatalog catalog, EventId eventId, WorldState world, PlayerReach reach)
        {
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (reach is null) throw new ArgumentNullException(nameof(reach));

            HistoricalEvent e = catalog.Find(eventId)
                ?? throw new ArgumentException($"事件不在目录：{eventId}。", nameof(eventId));

            if (world.IsTriggered(eventId.Value))
                return new HistoryAdvanceResult(false, false, FireReason.AlreadyTriggered, null, world);

            // ① 够不着 → 正常结局，短路不评估前置（"早期历史便宜"）。
            if (!_reach.Reachable(e, reach))
                return FireNormal(e, world, FireReason.NormalUnreachable);

            // ② 够得着且前置全成立 → 正常结局。
            if (e.AllPreconditionsHold(world))
                return FireNormal(e, world, FireReason.NormalPreconditionsHeld);

            // ③ 够得着且前置被破坏 → 置 diverged + 分叉结局。
            HistoricalOutcome divergence = e.DivergenceOutcome
                ?? throw new InvalidOperationException(
                    $"事件 {eventId} 缺分叉结局——应在 HistoricalEventCatalog.TryCreate 加载期被拒，不应抵达此处。");
            WorldState diverged = world.WithTriggeredEvent(eventId.Value, diverged: true);
            return new HistoryAdvanceResult(true, true, FireReason.Diverged, divergence, diverged);
        }

        private static HistoryAdvanceResult FireNormal(HistoricalEvent e, WorldState world, FireReason reason)
        {
            WorldState next = world.WithTriggeredEvent(e.Id.Value, diverged: false);
            return new HistoryAdvanceResult(true, false, reason, e.NormalOutcome, next);
        }
    }
}
