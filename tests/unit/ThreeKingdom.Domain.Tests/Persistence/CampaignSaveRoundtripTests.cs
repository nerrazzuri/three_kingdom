using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Persistence
{
    /// <summary>
    /// 战役存档统一信封 round-trip（full-game-review ADV-10 / GDD_013 / ADR-0005）。
    /// 验证生涯段 + 世界段纳入<b>单一信封</b>、一处版本/指纹校验、整体 round-trip 一致、不兼容整体拒绝。
    /// </summary>
    [TestFixture]
    public class CampaignSaveRoundtripTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Xuchang = new CityId("city-xuchang");

        private static readonly SaveVersion Version = new SaveVersion(1, 0);
        private static readonly ConfigFingerprint Fingerprint = new ConfigFingerprint(0xC0FFEEUL);
        private static readonly CampaignSaveCodec Codec = new CampaignSaveCodec();

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignSaveState State()
        {
            var career = new CareerSaveState(
                Version, Fingerprint,
                new CareerSnapshot(
                    new CareerState(80, 40, Frac(3, 10), Rank.SeniorGovernor, Cao, false),
                    new RetinueState(new[] { new RetinueMember(Aide, Frac(6, 10)) }, Array.Empty<System.Collections.Generic.KeyValuePair<OfficeRole, CharacterId>>())),
                rebellion: null,
                new LordMissionLog(new[] { new LordMissionRecord("mission-defend", MissionResult.Completed) }));

            var world = new WorldSaveState(
                Version, Fingerprint,
                new WorldState(
                    new WorldTime(3, DaySegment.Dawn),
                    new[] { new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Xuchang }) },
                    new[] { new CityOwnership(Xuchang, Cao, 800) },
                    new[] { "evt-guandu" }, new[] { "evt-guandu" }));

            return new CampaignSaveState(Version, Fingerprint, career, world);
        }

        [Test]
        public void test_unified_envelope_roundtrip_preserves_both_segments()
        {
            CampaignSaveState original = State();
            CampaignSaveState loaded = Codec.Deserialize(Codec.Serialize(original), Version, Fingerprint);

            // 生涯段
            Assert.That(loaded.Career.Snapshot.Career.Merit, Is.EqualTo(80));
            Assert.That(loaded.Career.Snapshot.Career.Rank, Is.EqualTo(Rank.SeniorGovernor));
            Assert.That(loaded.Career.Missions.Records[0].MissionId, Is.EqualTo("mission-defend"));
            // 世界段
            Assert.That(loaded.World.World.CurrentTime, Is.EqualTo(new WorldTime(3, DaySegment.Dawn)));
            Assert.That(loaded.World.World.IsDiverged("evt-guandu"), Is.True);
            Assert.That(loaded.World.World.OwnershipOf(Xuchang)!.Owner, Is.EqualTo(Cao));
            // 整体哈希一致
            Assert.That(loaded.ComputeHash(), Is.EqualTo(original.ComputeHash()));
        }

        [Test]
        public void test_serialization_is_deterministic()
        {
            Assert.That(Codec.Serialize(State()), Is.EqualTo(Codec.Serialize(State())));
        }

        [Test]
        public void test_newer_envelope_version_rejected_wholesale()
        {
            var future = new CampaignSaveState(new SaveVersion(2, 0), Fingerprint, State().Career, State().World);
            string text = Codec.Serialize(future);
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, Version, Fingerprint));
        }

        [Test]
        public void test_fingerprint_mismatch_rejected_wholesale()
        {
            string text = Codec.Serialize(State());
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, Version, new ConfigFingerprint(0xBADUL)));
        }

        [Test]
        public void test_corrupt_envelope_rejected()
        {
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize("not-a-campaign-save", Version, Fingerprint));
        }
    }
}
