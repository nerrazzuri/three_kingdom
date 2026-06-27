using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-012 story-001：WorldState 权威状态与确定性推进骨架。
    /// 治理 ADR：ADR-0002（四层 / 单一推进路径）+ ADR-0004（确定性时间推进 + 状态哈希）。GDD_015 / TR-world-001。
    /// 覆盖 AC-1/AC-2 字段、AC-3/AC-4 确定性推进（哈希一致 + 不同序列哈希异 + 输入序无关）、AC-5 归属只读（编译级）。
    /// </summary>
    [TestFixture]
    public class WorldStateTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Sun = new FactionId("faction-sun");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CharacterId SunQuan = new CharacterId("char-sunquan");
        private static readonly CityId Xuchang = new CityId("city-xuchang");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

        private static readonly WorldProgressionService Progression = new WorldProgressionService();

        private static WorldState Seed(WorldTime? time = null)
        {
            var factions = new[]
            {
                new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Xuchang }),
                new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { Fanshui }),
            };
            var cities = new[]
            {
                new CityOwnership(Xuchang, Cao, garrison: 800),
                new CityOwnership(Fanshui, Sun, garrison: 500),
            };
            return new WorldState(
                time ?? new WorldTime(0, DaySegment.Dawn),
                factions, cities,
                triggeredEvents: Array.Empty<string>(),
                divergedEvents: Array.Empty<string>());
        }

        // ---- AC-1 / AC-2：字段齐全 ----

        [Test]
        public void test_world_state_holds_time_factions_cities_and_event_sets()
        {
            WorldState w = Seed();

            Assert.That(w.CurrentTime, Is.EqualTo(new WorldTime(0, DaySegment.Dawn)));
            Assert.That(w.Factions.Count, Is.EqualTo(2));
            Assert.That(w.Cities.Count, Is.EqualTo(2));
            Assert.That(w.TriggeredEvents, Is.Empty);
            Assert.That(w.DivergedEvents, Is.Empty);

            FactionRecord? cao = w.FactionById(Cao);
            Assert.That(cao, Is.Not.Null);
            Assert.That(cao!.Lord, Is.EqualTo(CaoCao));
            Assert.That(cao.Survival, Is.EqualTo(SurvivalStatus.Active));
            Assert.That(cao.Relation, Is.EqualTo(RelationToPlayer.Self));
            Assert.That(cao.OwnedCities, Has.Member(Xuchang));

            CityOwnership? xc = w.OwnershipOf(Xuchang);
            Assert.That(xc, Is.Not.Null);
            Assert.That(xc!.Owner, Is.EqualTo(Cao));
            Assert.That(xc.Garrison, Is.EqualTo(800));
        }

        [Test]
        public void test_single_faction_empty_events_initial_state_is_valid()
        {
            var w = new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>()) },
                Array.Empty<CityOwnership>(),
                Array.Empty<string>(),
                Array.Empty<string>());

            Assert.That(w.Factions.Count, Is.EqualTo(1));
            Assert.That(w.Cities, Is.Empty);
        }

        [Test]
        public void test_active_faction_without_lord_is_rejected()
        {
            Assert.Throws<ArgumentException>(
                () => new FactionRecord(Cao, lord: null, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>()));
        }

        [Test]
        public void test_destroyed_faction_may_have_no_lord()
        {
            var fallen = new FactionRecord(Sun, lord: null, SurvivalStatus.Destroyed, RelationToPlayer.Neutral, Array.Empty<CityId>());
            Assert.That(fallen.Survival, Is.EqualTo(SurvivalStatus.Destroyed));
            Assert.That(fallen.Lord, Is.Null);
        }

        [Test]
        public void test_negative_garrison_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CityOwnership(Xuchang, Cao, garrison: -1));
        }

        // ---- AC-3 / AC-4：确定性推进 ----

        [Test]
        public void test_same_state_same_advance_sequence_yields_identical_hash()
        {
            WorldState a = Advance(Seed(), 1, 2, 3);
            WorldState b = Advance(Seed(), 1, 2, 3);

            Assert.That(a.ComputeHash(), Is.EqualTo(b.ComputeHash()));
            Assert.That(a.CurrentTime, Is.EqualTo(b.CurrentTime));
        }

        [Test]
        public void test_different_advance_sequence_yields_different_hash()
        {
            // 不同行动序列（推进总量不同）→ 不同世界态 → 哈希不同。
            WorldState shorter = Advance(Seed(), 1, 1);   // +2 段
            WorldState longer = Advance(Seed(), 1, 2, 3); // +6 段

            Assert.That(shorter.ComputeHash(), Is.Not.EqualTo(longer.ComputeHash()));
        }

        [Test]
        public void test_hash_is_independent_of_input_collection_order()
        {
            // 势力/城池以不同输入顺序构造同一逻辑态 → 规范排序后哈希一致（确定性字节序）。
            var forward = new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Xuchang }),
                    new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { Fanshui }),
                },
                new[] { new CityOwnership(Xuchang, Cao, 800), new CityOwnership(Fanshui, Sun, 500) },
                Array.Empty<string>(), Array.Empty<string>());

            var reversed = new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { Fanshui }),
                    new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Xuchang }),
                },
                new[] { new CityOwnership(Fanshui, Sun, 500), new CityOwnership(Xuchang, Cao, 800) },
                Array.Empty<string>(), Array.Empty<string>());

            Assert.That(forward.ComputeHash(), Is.EqualTo(reversed.ComputeHash()));
        }

        [Test]
        public void test_zero_segment_advance_preserves_hash()
        {
            WorldState w = Seed();
            WorldState same = Progression.Advance(w, 0);
            Assert.That(same.ComputeHash(), Is.EqualTo(w.ComputeHash()));
        }

        [Test]
        public void test_negative_advance_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Progression.Advance(Seed(), -1));
        }

        // ---- AC-5：城池归属只读（编译级，无写 API）----

        [Test]
        public void test_city_ownership_owner_has_no_public_setter()
        {
            // 归属只读投影：Owner/Garrison/City 无 public setter，世界推进路径无法直接写归属（TR-world-003）。
            foreach (string prop in new[] { "Owner", "Garrison", "City" })
            {
                PropertyInfo? p = typeof(CityOwnership).GetProperty(prop);
                Assert.That(p, Is.Not.Null, $"属性 {prop} 应存在。");
                Assert.That(p!.CanWrite, Is.False, $"{prop} 不应有 public setter（只读投影）。");
            }
        }

        [Test]
        public void test_world_state_exposes_no_public_ownership_mutator()
        {
            // WorldState 不得暴露任何 public 写归属/势力的方法（更新须经 story-004 订阅 + 重建）。
            foreach (MethodInfo m in typeof(WorldState).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                string name = m.Name;
                bool looksMutating = name.StartsWith("Set", StringComparison.Ordinal)
                    || name.StartsWith("Add", StringComparison.Ordinal)
                    || name.StartsWith("Remove", StringComparison.Ordinal);
                Assert.That(looksMutating, Is.False, $"WorldState 不应暴露 public 变更方法：{name}。");
            }
        }

        private static WorldState Advance(WorldState start, params int[] steps)
        {
            WorldState w = start;
            foreach (int s in steps) w = Progression.Advance(w, s);
            return w;
        }
    }
}
