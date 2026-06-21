using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Map
{
    /// <summary>
    /// epic-002 story-005：地图真值与阵营知识分离（Integration）。
    /// 治理 ADR：ADR-0002（真值与阵营知识分离；唯一权威来源；显示层只读投影不触真值）。GDD_003 / TR-map-003。
    /// 覆盖 AC-1 真值/知识独立结构、AC-2 敌区只能由侦察更新（控制权变更不自动揭示）、AC-3 投影只读且无真值字段。
    /// </summary>
    [TestFixture]
    public class TruthKnowledgeSplitTests
    {
        private static RegionId R(string s) => new RegionId(s);
        private static FactionId F(string s) => new FactionId(s);

        private const string Home = "luoyang";
        private const string Enemy = "chengdu";

        private static MapTruth NewTruth()
            => new MapTruth(new[]
            {
                MapTruth.Entry(R(Home), F("wei"), 10),
                MapTruth.Entry(R(Enemy), F("shu"), 500), // 未侦察的敌区真值
            });

        // ---- AC-1 / AC-3：真值与知识独立；投影无真值泄露 ----

        [Test]
        public void Projection_does_not_leak_unscouted_enemy_truth()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));

            var projection = wei.Project();

            Assert.That(projection.Contains(R(Enemy)), Is.False, "未侦察敌区不应出现在投影中");
            Assert.That(projection.TryGet(R(Enemy), out _), Is.False);
            // 真值仍独立存在且未受影响
            Assert.That(truth.Region(R(Enemy)).Garrison, Is.EqualTo(500));
        }

        // ---- AC-2：控制权变更不自动揭示 ----

        [Test]
        public void Control_change_does_not_auto_reveal_to_knowledge()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));

            // 敌区控制权刚变更为 wei（真值权威路径）
            truth.SetController(R(Enemy), F("wei"));

            // 知识/投影仍不含该区——不自动揭示（须侦察）
            Assert.That(wei.Knows(R(Enemy)), Is.False);
            Assert.That(wei.Project().Contains(R(Enemy)), Is.False);
            Assert.That(truth.Region(R(Enemy)).Controller, Is.EqualTo(F("wei"))); // 真值确实已变
        }

        // ---- AC-2：侦察更新知识，真值不变 ----

        [Test]
        public void Scouting_updates_only_knowledge_truth_unchanged()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));
            var now = new WorldTime(2, DaySegment.Day);

            var obs = ScoutingService.Observe(truth, wei, R(Enemy), now, KnowledgeSource.Scouting);

            // 知识已更新
            Assert.That(wei.Knows(R(Enemy)), Is.True);
            Assert.That(wei.TryGet(R(Enemy), out var known), Is.True);
            Assert.That(known.KnownController, Is.EqualTo(F("shu")));
            Assert.That(known.KnownGarrison, Is.EqualTo(500));
            Assert.That(known.ObservedTime, Is.EqualTo(now));
            Assert.That(known.Source, Is.EqualTo(KnowledgeSource.Scouting));
            Assert.That(obs.Region, Is.EqualTo(R(Enemy)));

            // 真值未变
            Assert.That(truth.Region(R(Enemy)).Garrison, Is.EqualTo(500));
            Assert.That(truth.Region(R(Enemy)).Controller, Is.EqualTo(F("shu")));
        }

        [Test]
        public void Scouted_region_appears_in_projection_with_known_fields()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));
            ScoutingService.Observe(truth, wei, R(Enemy), new WorldTime(1, DaySegment.Dawn), KnowledgeSource.Scouting);

            var projection = wei.Project();

            Assert.That(projection.Contains(R(Enemy)), Is.True);
            Assert.That(projection.TryGet(R(Enemy), out var k), Is.True);
            Assert.That(k.KnownGarrison, Is.EqualTo(500));
        }

        [Test]
        public void Stale_knowledge_persists_after_truth_changes_until_rescouted()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));
            ScoutingService.Observe(truth, wei, R(Enemy), new WorldTime(1, DaySegment.Dawn), KnowledgeSource.Scouting);

            // 真值之后变化（敌增兵），但未重新侦察
            truth.SetGarrison(R(Enemy), 800);

            wei.TryGet(R(Enemy), out var known);
            Assert.That(known.KnownGarrison, Is.EqualTo(500), "旧知识保留，真值变化不自动同步（须重新侦察）");

            // 重新侦察后知识刷新
            ScoutingService.Observe(truth, wei, R(Enemy), new WorldTime(3, DaySegment.Day), KnowledgeSource.Scouting);
            wei.TryGet(R(Enemy), out var refreshed);
            Assert.That(refreshed.KnownGarrison, Is.EqualTo(800));
        }

        [Test]
        public void Projection_is_a_snapshot_independent_of_later_updates()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));

            var early = wei.Project();
            ScoutingService.Observe(truth, wei, R(Enemy), new WorldTime(1, DaySegment.Dawn), KnowledgeSource.Scouting);

            Assert.That(early.Contains(R(Enemy)), Is.False, "先前导出的投影是快照，不受后续侦察影响");
            Assert.That(wei.Project().Contains(R(Enemy)), Is.True);
        }

        [Test]
        public void Two_factions_keep_independent_knowledge()
        {
            var truth = NewTruth();
            var wei = new FactionKnowledge(F("wei"));
            var shu = new FactionKnowledge(F("shu"));

            ScoutingService.Observe(truth, wei, R(Enemy), new WorldTime(1, DaySegment.Dawn), KnowledgeSource.Scouting);

            Assert.That(wei.Knows(R(Enemy)), Is.True);
            Assert.That(shu.Knows(R(Enemy)), Is.False, "另一阵营知识独立，不受影响");
        }

        [Test]
        public void Truth_rejects_duplicate_region_and_negative_garrison()
        {
            Assert.Throws<ArgumentException>(() => new MapTruth(new[]
            {
                MapTruth.Entry(R(Home), F("wei"), 1),
                MapTruth.Entry(R(Home), F("shu"), 1),
            }));
            Assert.Throws<ArgumentOutOfRangeException>(() => MapTruth.Entry(R(Home), F("wei"), -1));
        }
    }
}
