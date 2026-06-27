using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// epic-011 story-005：生涯状态存档 round-trip（Integration）。
    /// 治理 ADR：ADR-0005（版本化 DTO + JSON 端口 + 版本/指纹校验）+ ADR-0002。GDD_014 / TR-career-003。
    /// 覆盖 AC-1/2 round-trip 逐字段+哈希一致、AC-3 读档后续推进确定性、AC-4/5 版本/指纹校验拒绝不部分载入。
    /// </summary>
    [TestFixture]
    public class CareerSaveRoundtripTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId NewState = new FactionId("faction-new");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CharacterId Warden = new CharacterId("char-warden");

        private static readonly SaveVersion CurrentVersion = new SaveVersion(1, 0);
        private static readonly ConfigFingerprint Fingerprint = new ConfigFingerprint(0xABCDEF12UL);
        private static readonly CareerSaveCodec Codec = new CareerSaveCodec();

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CareerSaveState NontrivialState()
        {
            var career = new CareerState(120, 60, Frac(3, 10), Rank.SeniorGovernor, Cao, isUnaffiliated: false);
            var retinue = new RetinueState(
                new[] { new RetinueMember(Aide, Frac(7, 10)), new RetinueMember(Warden, Frac(1, 2)) },
                new[] { new KeyValuePair<OfficeRole, CharacterId>(OfficeRole.Strategist, Aide) });
            var rebellion = new RebellionState(
                new[] { Frac(7, 10), Frac(2, 10) }, Frac(1, 2), RebellionOutcome.PartialFollow, NewState);
            var missions = new LordMissionLog(new[]
            {
                new LordMissionRecord("mission-defend-fanshui", MissionResult.Completed),
                new LordMissionRecord("mission-collect-tax", MissionResult.Failed),
            });
            return new CareerSaveState(CurrentVersion, Fingerprint, new CareerSnapshot(career, retinue), rebellion, missions);
        }

        private static CareerSaveState RoundTrip(CareerSaveState s)
            => Codec.Deserialize(Codec.Serialize(s), CurrentVersion, Fingerprint);

        // ---- AC-1 / AC-2：round-trip 逐字段 + 哈希一致 ----

        [Test]
        public void test_roundtrip_preserves_all_career_fields()
        {
            CareerSaveState original = NontrivialState();
            CareerSaveState loaded = RoundTrip(original);

            CareerState a = original.Snapshot.Career, b = loaded.Snapshot.Career;
            Assert.That(b.Merit, Is.EqualTo(a.Merit));
            Assert.That(b.Renown, Is.EqualTo(a.Renown));
            Assert.That(b.LordStanding, Is.EqualTo(a.LordStanding));
            Assert.That(b.Rank, Is.EqualTo(a.Rank));
            Assert.That(b.Faction, Is.EqualTo(a.Faction));
            Assert.That(b.IsUnaffiliated, Is.EqualTo(a.IsUnaffiliated));

            Assert.That(loaded.Snapshot.Retinue.Members.Count, Is.EqualTo(2));
            Assert.That(loaded.Snapshot.Retinue.Holder(OfficeRole.Strategist), Is.EqualTo(Aide));
            Assert.That(loaded.Rebellion!.Outcome, Is.EqualTo(RebellionOutcome.PartialFollow));
            Assert.That(loaded.Rebellion.NewFaction, Is.EqualTo(NewState));
            Assert.That(loaded.Rebellion.AffinitySnapshot.Count, Is.EqualTo(2));
            Assert.That(loaded.Missions.Records.Count, Is.EqualTo(2));
            Assert.That(loaded.Missions.Records[1].Result, Is.EqualTo(MissionResult.Failed));
        }

        [Test]
        public void test_roundtrip_preserves_state_hash()
        {
            CareerSaveState original = NontrivialState();
            Assert.That(RoundTrip(original).ComputeHash(), Is.EqualTo(original.ComputeHash()));
        }

        [Test]
        public void test_roundtrip_unaffiliated_with_no_rebellion_and_empty_missions()
        {
            var career = new CareerState(40, 20, FixedPoint.Zero, Rank.CityGovernor, faction: null, isUnaffiliated: true);
            var snapshot = new CareerSnapshot(career, RetinueState.Empty);
            var state = new CareerSaveState(CurrentVersion, Fingerprint, snapshot, rebellion: null, LordMissionLog.Empty);

            CareerSaveState loaded = RoundTrip(state);
            Assert.That(loaded.Snapshot.Career.IsUnaffiliated, Is.True);
            Assert.That(loaded.Snapshot.Career.Faction, Is.Null);
            Assert.That(loaded.Rebellion, Is.Null);
            Assert.That(loaded.Missions.Records, Is.Empty);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(state.ComputeHash()));
        }

        [Test]
        public void test_serialization_is_deterministic()
        {
            CareerSaveState s = NontrivialState();
            Assert.That(Codec.Serialize(s), Is.EqualTo(Codec.Serialize(s)));
        }

        // ---- AC-3：读档后续推进确定性 ----

        [Test]
        public void test_advance_after_load_matches_direct_advance()
        {
            var service = new CareerStateService();
            List<CareerCommand> Stream() => new List<CareerCommand>
            {
                new GainMeritCommand(25, 10),
                new AdjustLordStandingCommand(Frac(1, 10)),
                new AssignOfficeCommand(OfficeRole.CityWarden, Warden),
            };

            CareerSnapshot direct = Run(service, NontrivialState().Snapshot, Stream());
            CareerSnapshot afterLoad = Run(service, RoundTrip(NontrivialState()).Snapshot, Stream());

            Assert.That(afterLoad.ComputeHash(), Is.EqualTo(direct.ComputeHash()));
        }

        // ---- AC-4 / AC-5：版本 + 指纹校验，不部分载入 ----

        [Test]
        public void test_newer_version_save_is_rejected()
        {
            var future = new CareerSaveState(new SaveVersion(2, 0), Fingerprint,
                NontrivialState().Snapshot, null, LordMissionLog.Empty);
            string text = Codec.Serialize(future);

            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, CurrentVersion, Fingerprint));
        }

        [Test]
        public void test_fingerprint_mismatch_is_rejected()
        {
            string text = Codec.Serialize(NontrivialState());
            var otherFingerprint = new ConfigFingerprint(0x99999999UL);
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize(text, CurrentVersion, otherFingerprint));
        }

        [Test]
        public void test_corrupt_text_is_rejected()
        {
            Assert.Throws<SaveFormatException>(() => Codec.Deserialize("not-a-valid-save", CurrentVersion, Fingerprint));
        }

        private static CareerSnapshot Run(CareerStateService service, CareerSnapshot start, List<CareerCommand> commands)
        {
            CareerSnapshot s = start;
            foreach (CareerCommand cmd in commands)
            {
                CareerCommandResult r = service.Apply(s, cmd);
                Assert.That(r.Applied, Is.True, $"命令失败：{r.Error} {r.Detail}");
                s = r.Snapshot;
            }
            return s;
        }
    }
}
