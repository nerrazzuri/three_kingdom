using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-012 story-006：WorldState 存档 round-trip（Integration）。
    /// 治理 ADR：ADR-0005（版本化 DTO + 禁 Unity 序列化 + 版本/指纹校验）+ ADR-0002。GDD_015 / TR-world-006。
    /// 覆盖 AC-1/2 round-trip 逐字段+diverged 集合一致、AC-3 读档后续推进确定性、AC-4/5 版本/指纹校验拒不部分载入。
    /// </summary>
    [TestFixture]
    public class WorldSaveRoundtripTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Sun = new FactionId("faction-sun");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CharacterId SunQuan = new CharacterId("char-sunquan");
        private static readonly CityId Xuchang = new CityId("city-xuchang");
        private static readonly CityId Jianye = new CityId("city-jianye");

        private static readonly SaveVersion CurrentVersion = new SaveVersion(1, 0);
        private static readonly ConfigFingerprint Fingerprint = new ConfigFingerprint(0x1234CAFEUL);
        private static readonly WorldSaveCodec Codec = new WorldSaveCodec();
        private static readonly WorldProgressionService Progression = new WorldProgressionService();

        private static WorldState NontrivialWorld()
            => new WorldState(
                new WorldTime(2, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Xuchang }),
                    new FactionRecord(Sun, SunQuan, SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { Jianye }),
                },
                new[] { new CityOwnership(Xuchang, Cao, 800), new CityOwnership(Jianye, Sun, 500) },
                triggeredEvents: new[] { "evt-guandu", "evt-chibi" },
                divergedEvents: new[] { "evt-chibi" });

        private static WorldSaveState State(WorldState w) => new WorldSaveState(CurrentVersion, Fingerprint, w);
        private static WorldSaveState RoundTrip(WorldSaveState s)
            => Codec.Deserialize(Codec.Serialize(s), CurrentVersion, Fingerprint);

        // ---- AC-1 / AC-2：round-trip 逐字段 + diverged 集合 + 哈希 ----

        [Test]
        public void test_roundtrip_preserves_world_fields()
        {
            WorldSaveState original = State(NontrivialWorld());
            WorldState w = RoundTrip(original).World;

            Assert.That(w.CurrentTime, Is.EqualTo(new WorldTime(2, DaySegment.Dawn)));
            Assert.That(w.Factions.Count, Is.EqualTo(2));
            FactionRecord? sun = w.FactionById(Sun);
            Assert.That(sun!.Lord, Is.EqualTo(SunQuan));
            Assert.That(sun.Relation, Is.EqualTo(RelationToPlayer.Hostile));
            Assert.That(sun.OwnedCities, Has.Member(Jianye));
            Assert.That(w.OwnershipOf(Xuchang)!.Owner, Is.EqualTo(Cao));
            Assert.That(w.OwnershipOf(Xuchang)!.Garrison, Is.EqualTo(800));
            Assert.That(w.IsTriggered("evt-guandu"), Is.True);
            Assert.That(w.IsDiverged("evt-chibi"), Is.True);
            Assert.That(w.IsDiverged("evt-guandu"), Is.False);
        }

        [Test]
        public void test_roundtrip_preserves_state_hash()
        {
            WorldSaveState original = State(NontrivialWorld());
            Assert.That(RoundTrip(original).ComputeHash(), Is.EqualTo(original.ComputeHash()));
        }

        [Test]
        public void test_roundtrip_empty_events_world()
        {
            var w = new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, Array.Empty<CityId>()) },
                Array.Empty<CityOwnership>(), Array.Empty<string>(), Array.Empty<string>());

            WorldState loaded = RoundTrip(State(w)).World;
            Assert.That(loaded.TriggeredEvents, Is.Empty);
            Assert.That(loaded.DivergedEvents, Is.Empty);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(w.ComputeHash()));
        }

        [Test]
        public void test_serialization_is_deterministic()
        {
            WorldSaveState s = State(NontrivialWorld());
            Assert.That(Codec.Serialize(s), Is.EqualTo(Codec.Serialize(s)));
        }

        // ---- AC-3：读档后续推进确定性 ----

        [Test]
        public void test_advance_after_load_matches_direct_advance()
        {
            WorldState direct = Progression.Advance(Progression.Advance(NontrivialWorld(), 1), 2);
            WorldState loaded = RoundTrip(State(NontrivialWorld())).World;
            WorldState afterLoad = Progression.Advance(Progression.Advance(loaded, 1), 2);

            Assert.That(afterLoad.ComputeHash(), Is.EqualTo(direct.ComputeHash()));
        }

        // ---- AC-4 / AC-5：版本 + 指纹校验，不部分载入 ----

        [Test]
        public void test_newer_version_save_is_rejected()
        {
            var future = new WorldSaveState(new SaveVersion(2, 0), Fingerprint, NontrivialWorld());
            string text = Codec.Serialize(future);
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, CurrentVersion, Fingerprint));
        }

        [Test]
        public void test_fingerprint_mismatch_is_rejected()
        {
            string text = Codec.Serialize(State(NontrivialWorld()));
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, CurrentVersion, new ConfigFingerprint(0xDEADUL)));
        }

        [Test]
        public void test_corrupt_text_is_rejected()
        {
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize("garbage-not-a-save", CurrentVersion, Fingerprint));
        }
    }
}
