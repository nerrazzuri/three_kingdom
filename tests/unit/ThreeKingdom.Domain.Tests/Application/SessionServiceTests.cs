using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// EPIC_010 竖切：Application 会话用例服务（BLOCKING）。治理 ADR：ADR-0002（用例编排）+ ADR-0004（确定性）。
    /// 证明 Presentation→Application→Domain 接缝：开局→推进→只读投影，确定性可复现。
    /// </summary>
    [TestFixture]
    public class SessionServiceTests
    {
        [Test]
        public void test_new_game_starts_at_day_zero_dawn()
        {
            var service = new SessionService();

            var projection = service.Project(service.NewGame());

            Assert.That(projection.Day, Is.EqualTo(0));
            Assert.That(projection.Segment, Is.EqualTo(DaySegment.Dawn));
            Assert.That(projection.AbsoluteIndex, Is.EqualTo(0));
            Assert.That(projection.DaysCrossedLastAdvance, Is.EqualTo(0));
        }

        [Test]
        public void test_advance_one_segment_moves_dawn_to_day_without_crossing()
        {
            var service = new SessionService();
            var session = service.NewGame();

            var projection = service.Advance(session, 1);

            Assert.That(projection.Segment, Is.EqualTo(DaySegment.Day));
            Assert.That(projection.Day, Is.EqualTo(0));
            Assert.That(projection.DaysCrossedLastAdvance, Is.EqualTo(0), "同日内推进不应跨日界。");
        }

        [Test]
        public void test_advance_four_segments_crosses_into_next_day()
        {
            var service = new SessionService();
            var session = service.NewGame(); // 第 0 日黎明

            var projection = service.Advance(session, WorldTime.SegmentsPerDay); // 整整一天

            Assert.That(projection.Day, Is.EqualTo(1));
            Assert.That(projection.Segment, Is.EqualTo(DaySegment.Dawn));
            Assert.That(projection.DaysCrossedLastAdvance, Is.EqualTo(1), "跨入第 1 日应报告 1 次日界穿越。");
        }

        [Test]
        public void test_advance_sequence_is_deterministic()
        {
            var service = new SessionService();
            var a = service.NewGame();
            var b = service.NewGame();

            foreach (var step in new[] { 1, 3, 2, 4 })
            {
                service.Advance(a, step);
                service.Advance(b, step);
            }
            var pa = service.Project(a);
            var pb = service.Project(b);

            Assert.That(pa.AbsoluteIndex, Is.EqualTo(pb.AbsoluteIndex), "同一推进序列必产生同一权威时间（ADR-0004）。");
            Assert.That(pa.Day, Is.EqualTo(pb.Day));
            Assert.That(pa.Segment, Is.EqualTo(pb.Segment));
        }

        [Test]
        public void test_project_after_advance_reports_no_fresh_crossing()
        {
            var service = new SessionService();
            var session = service.NewGame();
            service.Advance(session, WorldTime.SegmentsPerDay); // 此次跨日

            var projection = service.Project(session); // 仅取投影，未再推进

            Assert.That(projection.Day, Is.EqualTo(1));
            Assert.That(projection.DaysCrossedLastAdvance, Is.EqualTo(0), "Project 不推进，不应报告日界穿越。");
        }

        [Test]
        public void test_advance_null_session_throws()
        {
            var service = new SessionService();

            Assert.That(() => service.Advance(null!, 1), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
