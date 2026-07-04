using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// 官职撤职 + 派系不满（GDD_014 官职任免 B6）：撤职去任免、前任因失位好感降（喂忠诚经营）；
    /// 无人在任的官职撤职 → 稳定错误码，无部分写入。
    /// </summary>
    [TestFixture]
    public class OfficeDismissalTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private static readonly CharacterId Warden = new CharacterId("char-warden");
        private readonly CareerStateService _svc = new CareerStateService();

        private static CareerSnapshot Snapshot()
        {
            var retinue = new RetinueState(
                new[] { new RetinueMember(Warden, F(7, 10)) },
                new[] { new KeyValuePair<OfficeRole, CharacterId>(OfficeRole.CityWarden, Warden) });
            return new CareerSnapshot(new CareerState(0, 100, F(3, 10), Rank.CityGovernor, new ThreeKingdom.Domain.Map.FactionId("faction-lord"), false), retinue);
        }

        private static FixedPoint Aff(RetinueState r, CharacterId c)
        {
            foreach (RetinueMember m in r.Members) if (m.Character == c) return m.Affinity;
            return FixedPoint.FromInt(-1);
        }

        [Test]
        public void test_dismiss_removes_office_and_drops_affinity()
        {
            CareerCommandResult r = _svc.Apply(Snapshot(), new DismissOfficeCommand(OfficeRole.CityWarden, F(2, 10)));
            Assert.That(r.Applied, Is.True);
            Assert.That(r.Snapshot.Retinue.Holder(OfficeRole.CityWarden), Is.Null, "撤职 → 该官职位空缺。");
            Assert.That(r.Snapshot.Retinue.IsMember(Warden), Is.True, "撤职不逐人（仍在僚属）。");
            Assert.That(Aff(r.Snapshot.Retinue, Warden).Raw, Is.EqualTo(F(5, 10).Raw), "前任派系不满 → 好感降（0.7−0.2=0.5）。");
        }

        [Test]
        public void test_dismiss_empty_office_fails_stably()
        {
            CareerCommandResult r = _svc.Apply(Snapshot(), new DismissOfficeCommand(OfficeRole.Strategist, F(2, 10)));
            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CareerErrorCode.NoOfficeHolder), "无人在任 → 稳定错误码。");
            Assert.That(r.Snapshot.Retinue.Holder(OfficeRole.CityWarden), Is.EqualTo(Warden), "失败无部分写入（原任免不变）。");
        }
    }
}
