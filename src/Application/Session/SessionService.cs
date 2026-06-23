using System;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 会话用例服务（ADR-0002 状态变更协议 2：Application 执行命令、推进 Domain、产出投影）。
    /// 这是 Presentation→Application 接缝的<b>执行端</b>：表现层调用本服务的用例方法（开局/推进/投影），
    /// 服务编排 Domain（<see cref="WorldClock"/>）并返回<b>只读投影</b>。确定性——同一会话同一推进序列产生同一结果
    /// （ADR-0004）。无副作用泄露：表现层拿不到可变聚合，只拿 <see cref="WorldStatusProjection"/>。
    /// </summary>
    public sealed class SessionService
    {
        /// <summary>开新局：世界时间起于第 0 日黎明（确定性初值）。</summary>
        public GameSession NewGame() => new GameSession(new WorldTime(0, DaySegment.Dawn));

        /// <summary>推进会话 <paramref name="segments"/> 个时段（默认 1），返回推进后的世界状态投影。</summary>
        public WorldStatusProjection Advance(GameSession session, int segments = 1)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            AdvanceResult result = session.Advance(segments);
            return Project(session, result.DayBoundaries.Count);
        }

        /// <summary>取当前世界状态投影（不推进；DaysCrossedLastAdvance=0）。</summary>
        public WorldStatusProjection Project(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return Project(session, 0);
        }

        private static WorldStatusProjection Project(GameSession session, int daysCrossed)
        {
            WorldTime t = session.CurrentTime;
            return new WorldStatusProjection(t.Day, t.Segment, t.AbsoluteIndex, daysCrossed);
        }
    }
}
