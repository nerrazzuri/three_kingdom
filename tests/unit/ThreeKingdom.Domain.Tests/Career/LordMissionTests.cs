using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.Tests.Career
{
    /// <summary>
    /// 君主任务派发 + 评估（GDD_014 / W5）：君主主动派讨伐/守土/献纳，完成累积功绩通往晋升、逾期损名望。
    /// 确定性纯函数——同种子同任务；据世界情势评估进度。
    /// </summary>
    [TestFixture]
    public class LordMissionTests
    {
        private static readonly LordMissionService Svc = new LordMissionService();
        private static readonly LordMissionConfig Cfg = LordMissionConfig.Default;
        private static CityId City(string id) => new CityId(id);
        private static readonly IReadOnlyList<CityId> Targets = new[] { City("city-hulao"), City("city-xiapi") };
        private static readonly CityId Cap = City("city-fanshui");

        [Test]
        public void test_generation_is_deterministic()
        {
            LordMission a = Svc.Generate(Rank.CityGovernor, 190, Targets, Cap, 42UL, Cfg);
            LordMission b = Svc.Generate(Rank.CityGovernor, 190, Targets, Cap, 42UL, Cfg);
            Assert.That(a.Id, Is.EqualTo(b.Id));
            Assert.That(a.Type, Is.EqualTo(b.Type));
            Assert.That(a.DeadlineYear, Is.EqualTo(193), "期限=当前年+3。");
        }

        [Test]
        public void test_reward_scales_with_rank()
        {
            // 找到同类型（守土）在两官阶下的酬劳对比：官越高酬越丰。
            LordMission low = Svc.Generate(Rank.CityGovernor, 190, System.Array.Empty<CityId>(), Cap, 5UL, Cfg);
            LordMission high = Svc.Generate(Rank.GrandCommander, 190, System.Array.Empty<CityId>(), Cap, 5UL, Cfg);
            Assert.That(high.RewardMerit, Is.GreaterThan(low.RewardMerit), "官越高任越重、酬越丰。");
        }

        [Test]
        public void test_no_targets_never_subjugate()
        {
            for (ulong s = 0; s < 30; s++)
            {
                LordMission m = Svc.Generate(Rank.CityGovernor, 190, System.Array.Empty<CityId>(), Cap, s, Cfg);
                Assert.That(m.Type, Is.Not.EqualTo(MissionType.Subjugate), "无可攻目标 → 不派讨伐。");
            }
        }

        [Test]
        public void test_subjugate_completes_on_owning_target()
        {
            var m = new LordMission("m", MissionType.Subjugate, City("city-hulao"), 0, 190, 193, 80, 50, 20);
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(true, 0)), Is.EqualTo(MissionProgress.Completed), "占目标城 → 完成。");
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(false, 0)), Is.EqualTo(MissionProgress.Pending), "未占且未逾期 → 进行中。");
            Assert.That(Svc.Evaluate(m, 194, new MissionContext(false, 0)), Is.EqualTo(MissionProgress.Failed), "逾期未占 → 失败。");
        }

        [Test]
        public void test_defend_fails_on_lost_city_completes_at_deadline()
        {
            var m = new LordMission("m", MissionType.Defend, Cap, 0, 190, 193, 60, 40, 20);
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(false, 0)), Is.EqualTo(MissionProgress.Failed), "失守即败。");
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(true, 0)), Is.EqualTo(MissionProgress.Pending), "守着未到期 → 进行中。");
            Assert.That(Svc.Evaluate(m, 193, new MissionContext(true, 0)), Is.EqualTo(MissionProgress.Completed), "守到期限 → 完成。");
        }

        [Test]
        public void test_tribute_completes_on_delivered_grain()
        {
            var m = new LordMission("m", MissionType.Tribute, null, 100, 190, 193, 45, 30, 20);
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(false, 100)), Is.EqualTo(MissionProgress.Completed), "缴足 → 完成。");
            Assert.That(Svc.Evaluate(m, 191, new MissionContext(false, 50)), Is.EqualTo(MissionProgress.Pending), "未缴足且未逾期 → 进行中。");
            Assert.That(Svc.Evaluate(m, 194, new MissionContext(false, 50)), Is.EqualTo(MissionProgress.Failed), "逾期未缴足 → 失败。");
        }
    }
}
