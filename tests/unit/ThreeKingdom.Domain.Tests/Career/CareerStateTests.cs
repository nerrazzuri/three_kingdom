using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// epic-011 story-001：CareerState 权威状态与确定性结算骨架。
    /// 治理 ADR：ADR-0002（四层架构 / 单一写路径）+ ADR-0004（确定性整数/定点 + 状态哈希）。GDD_014 / TR-career-001、005。
    /// 覆盖 AC-1/AC-5 字段与类型、AC-3 唯一写路径+非法操作无部分写入、AC-4 确定性结算（哈希一致 + 顺序敏感）。
    /// </summary>
    [TestFixture]
    public class CareerStateTests
    {
        private static readonly FactionId Lord = new FactionId("faction-cao");
        private static readonly CharacterId Adviser = new CharacterId("char-guo-jia");
        private static readonly CharacterId Warden = new CharacterId("char-li-dian");
        private static readonly CharacterId Outsider = new CharacterId("char-stranger");

        private static FixedPoint Frac(int num, int den) => FixedPoint.FromFraction(num, den);

        private static CareerSnapshot NewGovernorSnapshot(FixedPoint? standing = null)
        {
            var career = CareerState.NewGovernor(Lord, standing ?? FixedPoint.Zero);
            var retinue = new RetinueState(
                new[]
                {
                    new RetinueMember(Adviser, Frac(3, 4)),
                    new RetinueMember(Warden, Frac(1, 2)),
                },
                Array.Empty<KeyValuePair<OfficeRole, CharacterId>>());
            return new CareerSnapshot(career, retinue);
        }

        private static readonly CareerCommandService Service = new CareerCommandService();

        // ---- AC-1 / AC-5：CareerState 字段与类型 ----

        [Test]
        public void test_new_governor_has_expected_fields_and_types()
        {
            CareerState c = CareerState.NewGovernor(Lord, Frac(1, 4));

            Assert.That(c.Merit, Is.EqualTo(0));
            Assert.That(c.Renown, Is.EqualTo(0));
            Assert.That(c.LordStanding, Is.EqualTo(Frac(1, 4)));   // Q16.16 定点，非 float
            Assert.That(c.Rank, Is.EqualTo(Rank.CityGovernor));     // 枚举序数 0
            Assert.That((int)Rank.Successor, Is.EqualTo(7));        // rank 0–7
            Assert.That(c.Faction, Is.EqualTo(Lord));
            Assert.That(c.IsUnaffiliated, Is.False);
        }

        [Test]
        public void test_lord_standing_above_one_is_rejected()
        {
            // lord_standing ∈ [0,1]；上越界构造被拒（无部分写入）。
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CareerState(0, 0, Frac(3, 2), Rank.CityGovernor, Lord, isUnaffiliated: false));
        }

        [Test]
        public void test_lord_standing_below_zero_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CareerState(0, 0, FixedPoint.FromInt(-1), Rank.CityGovernor, Lord, isUnaffiliated: false));
        }

        [Test]
        public void test_negative_merit_construction_is_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CareerState(-1, 0, FixedPoint.Zero, Rank.CityGovernor, Lord, isUnaffiliated: false));
        }

        [Test]
        public void test_unaffiliated_requires_null_faction_and_zero_standing()
        {
            // 在野⇔无所属势力；在野时好感须为 0。
            Assert.Throws<ArgumentException>(
                () => new CareerState(0, 0, FixedPoint.Zero, Rank.CityGovernor, Lord, isUnaffiliated: true));
            Assert.Throws<ArgumentException>(
                () => new CareerState(0, 0, Frac(1, 2), Rank.CityGovernor, faction: null, isUnaffiliated: true));

            var wanderer = new CareerState(0, 0, FixedPoint.Zero, Rank.CityGovernor, faction: null, isUnaffiliated: true);
            Assert.That(wanderer.IsUnaffiliated, Is.True);
            Assert.That(wanderer.Faction, Is.Null);
        }

        [Test]
        public void test_retinue_skeleton_holds_members_affinity_and_office_assignments()
        {
            var retinue = new RetinueState(
                new[] { new RetinueMember(Adviser, Frac(3, 4)) },
                new[] { new KeyValuePair<OfficeRole, CharacterId>(OfficeRole.Strategist, Adviser) });

            Assert.That(retinue.IsMember(Adviser), Is.True);
            Assert.That(retinue.Holder(OfficeRole.Strategist), Is.EqualTo(Adviser));
            Assert.That(retinue.Holder(OfficeRole.CityWarden), Is.Null);
            Assert.That(retinue.Members[0].Affinity, Is.EqualTo(Frac(3, 4)));
        }

        // ---- AC-3：唯一写路径 + 非法操作返回稳定错误码、无部分写入 ----

        [Test]
        public void test_negative_merit_delta_returns_stable_code_and_no_partial_write()
        {
            CareerSnapshot before = NewGovernorSnapshot();
            StateHash hashBefore = before.ComputeHash();

            CareerCommandResult result = Service.Submit(before, new GainMeritCommand(meritDelta: -5, renownDelta: 10));

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.NegativeMeritGain)); // 稳定错误码
            // 无部分写入：name（renown）也未变，整快照逐位不变。
            Assert.That(result.Snapshot.ComputeHash(), Is.EqualTo(hashBefore));
            Assert.That(result.Snapshot.Career.Renown, Is.EqualTo(0));
        }

        [Test]
        public void test_rank_skip_returns_stable_code_and_state_unchanged()
        {
            CareerSnapshot before = NewGovernorSnapshot(); // rank = CityGovernor(0)
            StateHash hashBefore = before.ComputeHash();

            // 越级：从 0 直接申请 ProvincialInspector(2)。
            CareerCommandResult result = Service.Submit(before, new PromoteRankCommand(Rank.ProvincialInspector));

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.RankSkipNotAllowed));
            Assert.That(result.Snapshot.Career.Rank, Is.EqualTo(Rank.CityGovernor));
            Assert.That(result.Snapshot.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_null_command_returns_stable_code_via_application_path()
        {
            CareerSnapshot before = NewGovernorSnapshot();
            CareerCommandResult result = Service.Submit(before, null!);

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.NullCommand));
            Assert.That(result.Snapshot, Is.SameAs(before));
        }

        [Test]
        public void test_assign_office_to_non_member_is_rejected()
        {
            CareerSnapshot before = NewGovernorSnapshot();
            StateHash hashBefore = before.ComputeHash();

            CareerCommandResult result = Service.Submit(before, new AssignOfficeCommand(OfficeRole.CityWarden, Outsider));

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.UnknownRetinueMember));
            Assert.That(result.Snapshot.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_unaffiliated_cannot_promote()
        {
            var career = new CareerState(0, 0, FixedPoint.Zero, Rank.CityGovernor, faction: null, isUnaffiliated: true);
            var before = new CareerSnapshot(career, RetinueState.Empty);

            CareerCommandResult result = Service.Submit(before, new PromoteRankCommand(Rank.SeniorGovernor));

            Assert.That(result.Applied, Is.False);
            Assert.That(result.Error, Is.EqualTo(CareerErrorCode.Unaffiliated));
        }

        [Test]
        public void test_valid_merit_gain_applies_and_accumulates()
        {
            CareerSnapshot before = NewGovernorSnapshot();
            CareerCommandResult r1 = Service.Submit(before, new GainMeritCommand(30, 12));

            Assert.That(r1.Applied, Is.True);
            Assert.That(r1.Error, Is.EqualTo(CareerErrorCode.None));
            Assert.That(r1.Snapshot.Career.Merit, Is.EqualTo(30));
            Assert.That(r1.Snapshot.Career.Renown, Is.EqualTo(12));

            // before 未被原地修改（不可变性 / 单一写路径）。
            Assert.That(before.Career.Merit, Is.EqualTo(0));
        }

        // ---- AC-4：确定性结算（同前态 + 同命令流 → 哈希逐位一致；顺序敏感）----

        [Test]
        public void test_same_state_same_command_stream_yields_identical_hash()
        {
            List<CareerCommand> Stream() => new List<CareerCommand>
            {
                new GainMeritCommand(40, 15),
                new AdjustLordStandingCommand(Frac(1, 4)),
                new PromoteRankCommand(Rank.SeniorGovernor),
                new AssignOfficeCommand(OfficeRole.Strategist, Adviser),
            };

            StateHash hashA = RunStream(NewGovernorSnapshot(), Stream()).ComputeHash();
            StateHash hashB = RunStream(NewGovernorSnapshot(), Stream()).ComputeHash();

            Assert.That(hashA, Is.EqualTo(hashB));
        }

        [Test]
        public void test_command_order_change_yields_different_hash()
        {
            // 同一职位先后任免不同僚属：顺序决定最终持有者 → 哈希不同（顺序敏感性正确）。
            var orderAB = new List<CareerCommand>
            {
                new AssignOfficeCommand(OfficeRole.CityWarden, Adviser),
                new AssignOfficeCommand(OfficeRole.CityWarden, Warden),
            };
            var orderBA = new List<CareerCommand>
            {
                new AssignOfficeCommand(OfficeRole.CityWarden, Warden),
                new AssignOfficeCommand(OfficeRole.CityWarden, Adviser),
            };

            StateHash hashAB = RunStream(NewGovernorSnapshot(), orderAB).ComputeHash();
            StateHash hashBA = RunStream(NewGovernorSnapshot(), orderBA).ComputeHash();

            Assert.That(hashAB, Is.Not.EqualTo(hashBA));
        }

        private static CareerSnapshot RunStream(CareerSnapshot start, List<CareerCommand> commands)
        {
            CareerSnapshot snapshot = start;
            foreach (CareerCommand cmd in commands)
            {
                CareerCommandResult result = Service.Submit(snapshot, cmd);
                Assert.That(result.Applied, Is.True, $"命令 {cmd.GetType().Name} 未应用：{result.Error} {result.Detail}");
                snapshot = result.Snapshot;
            }
            return snapshot;
        }
    }
}
