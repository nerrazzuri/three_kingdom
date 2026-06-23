using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// EPIC_010 竖切：世界状态展示视图（BLOCKING）。治理 ADR：ADR-0002（Presentation 翻译只读投影为 UI 文案）。
    /// 覆盖日标签 +1、时段中文映射、合成时辰标签、跨日提示。
    /// </summary>
    [TestFixture]
    public class WorldStatusViewTests
    {
        private static WorldStatusView ViewOf(int day, DaySegment seg, int crossed = 0)
            => new WorldStatusView(new WorldStatusProjection(day, seg, day * WorldTime.SegmentsPerDay + (int)seg, crossed));

        [Test]
        public void test_day_zero_renders_as_first_day()
        {
            Assert.That(ViewOf(0, DaySegment.Dawn).DayLabel, Is.EqualTo("第 1 日"));
            Assert.That(ViewOf(2, DaySegment.Dawn).DayLabel, Is.EqualTo("第 3 日"));
        }

        [Test]
        public void test_segments_map_to_chinese_labels()
        {
            Assert.That(ViewOf(0, DaySegment.Dawn).SegmentLabel, Is.EqualTo("黎明"));
            Assert.That(ViewOf(0, DaySegment.Day).SegmentLabel, Is.EqualTo("白昼"));
            Assert.That(ViewOf(0, DaySegment.Dusk).SegmentLabel, Is.EqualTo("黄昏"));
            Assert.That(ViewOf(0, DaySegment.Night).SegmentLabel, Is.EqualTo("夜间"));
        }

        [Test]
        public void test_time_label_composes_day_and_segment()
        {
            Assert.That(ViewOf(0, DaySegment.Dawn).TimeLabel, Is.EqualTo("第 1 日 · 黎明"));
        }

        [Test]
        public void test_cross_day_notice_present_only_when_crossed()
        {
            var crossed = ViewOf(1, DaySegment.Dawn, crossed: 1);
            Assert.That(crossed.CrossedDay, Is.True);
            Assert.That(crossed.CrossDayNotice, Does.Contain("跨入新一日"));

            var same = ViewOf(0, DaySegment.Day, crossed: 0);
            Assert.That(same.CrossedDay, Is.False);
            Assert.That(same.CrossDayNotice, Is.Empty);
        }

        [Test]
        public void test_null_projection_throws()
        {
            Assert.That(() => new WorldStatusView(null!), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
