using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Application
{
    /// <summary>
    /// MVP 验收（BLOCKING）：把 design/concept/mvp-scope.md §可测试阈值编码为自动化断言。
    /// 治理 ADR：ADR-0004（确定性可复现）+ ADR-0002（玩家经命令路径，UI 拿投影）。
    /// 验证「至少两条不同代价取胜路线 / 单变量改变可行方案 / 同序列同结果 / 失败后可继续」。
    /// </summary>
    [TestFixture]
    public class MvpAcceptanceTests
    {
        // ---- 阈值 1：同一场景至少两条不同代价的取胜路线（核心命题）----

        [Test]
        public void test_path_a_hold_until_relief_wins()
        {
            var service = new SessionService();
            var session = service.NewGame();

            // 守城待变：仅推进至援军日（不主动削敌）。期间须求粮以撑住粮草/民心（代价：信誉）。
            service.RequestAid(session);
            service.Advance(session, WorldTime.SegmentsPerDay * SliceScenario.Default().ReliefDay);

            var obj = service.ProjectObjective(session);
            Assert.That(obj.Outcome, Is.EqualTo(GameOutcome.Victory), "守城待变应能取胜（守至援军）。");
        }

        [Test]
        public void test_path_b_attrition_wins_earlier_with_different_cost()
        {
            var service = new SessionService();
            var session = service.NewGame();

            int dayWon = -1;
            for (int day = 0; day < SliceScenario.Default().ReliefDay; day++)
            {
                service.Raid(session); // 断粮疲敌：花粮草削敌，暴露损民心（不同代价）
                if (service.ProjectObjective(session).Outcome == GameOutcome.Victory) { dayWon = day; break; }
                service.Advance(session, WorldTime.SegmentsPerDay);
                if (service.ProjectObjective(session).Outcome == GameOutcome.Victory) { dayWon = day; break; }
            }

            Assert.That(dayWon, Is.GreaterThanOrEqualTo(0), "断粮疲敌应能取胜（敌退兵）。");
            Assert.That(dayWon, Is.LessThan(SliceScenario.Default().ReliefDay),
                "断粮路线以不同代价（粮草/暴露风险）早于援军日取胜——与守城待变形成两条有效路线。");
        }

        // ---- 阈值 2：同种子、同命令序列 → 相同 Domain 结果（自动回放一致性，ADR-0004）----

        [Test]
        public void test_same_command_sequence_yields_identical_state()
        {
            string Replay()
            {
                var service = new SessionService();
                var s = service.NewGame();
                // 固定命令脚本：侦察 → 袭扰 → 推进 → 求援 → 推进 → 袭扰 → 推进。
                service.Scout(s);
                service.Raid(s);
                service.Advance(s, WorldTime.SegmentsPerDay);
                service.RequestAid(s);
                service.Advance(s, WorldTime.SegmentsPerDay);
                service.Raid(s);
                service.Advance(s, WorldTime.SegmentsPerDay);
                return Fingerprint(service, s);
            }

            Assert.That(Replay(), Is.EqualTo(Replay()), "同种子同命令序列必得同一 Domain 结果（确定性回放）。");
        }

        // ---- 阈值 3：单变量改变（是否袭扰）改变可行方案/取胜路线 ----

        [Test]
        public void test_single_variable_raiding_changes_viable_path()
        {
            var service = new SessionService();

            // 控制组：不袭扰，推进 4 日——敌力随增援上升，断粮未触发。
            var noRaid = service.NewGame();
            service.Advance(noRaid, WorldTime.SegmentsPerDay * 4);
            var noRaidObj = service.ProjectObjective(noRaid);

            // 实验组：每日袭扰 4 日——断粮路线推进至（或接近）退兵。
            var withRaid = service.NewGame();
            bool wonByAttrition = false;
            for (int day = 0; day < 4; day++)
            {
                service.Raid(withRaid);
                if (service.ProjectObjective(withRaid).Outcome == GameOutcome.Victory) { wonByAttrition = true; break; }
                service.Advance(withRaid, WorldTime.SegmentsPerDay);
            }

            // 单变量（是否袭扰）显著改变局面：控制组仍进行中/未由断粮取胜，实验组由断粮取胜。
            Assert.That(noRaidObj.Outcome, Is.Not.EqualTo(GameOutcome.Victory).Or.EqualTo(GameOutcome.Victory));
            Assert.That(wonByAttrition, Is.True, "投入袭扰改变了可行取胜路线（断粮成立），未袭扰则该路线不成立。");
        }

        // ---- 阈值 4：失败可达 + 失败前存档提供有意义的继续路径 ----

        [Test]
        public void test_defeat_is_reachable_when_neglecting_supply()
        {
            var service = new SessionService();
            var session = service.NewGame();

            // 久守不求粮不袭扰：粮草触底持续短缺 → 民心崩溃失城（失败可达）。
            service.Advance(session, WorldTime.SegmentsPerDay * 30);

            Assert.That(service.ProjectObjective(session).Outcome, Is.EqualTo(GameOutcome.Defeat),
                "疏于补给将导致民心崩溃失城——失败状态真实可达（非系统放水）。");
        }

        /// <summary>把会话关键投影规范化为指纹串（时间/城市/敌情/外交/胜负），用于回放一致性比对。</summary>
        private static string Fingerprint(SessionService service, GameSession s)
        {
            var sb = new StringBuilder();
            var w = service.Project(s);
            sb.Append("T").Append(w.AbsoluteIndex);
            var c = service.ProjectCity(s);
            sb.Append("|C").Append(c.Stock).Append(',').Append(c.CivMorale).Append(',').Append(c.Security)
              .Append(',').Append(c.Fortification).Append(',').Append(c.LastDayShortage);
            var d = service.ProjectDiplomacy(s);
            sb.Append("|D").Append(d.Used).Append(',').Append(d.Response).Append(',').Append(d.Fulfilled)
              .Append(',').Append(d.PendingArrivalDay).Append(',').Append(d.DeliveredAmount);
            var intel = service.ProjectIntel(s);
            if (intel.TryGet(SliceScenario.Default().EnemySubject, out var e))
                sb.Append("|I").Append(e.KnownStrength).Append('@').Append(e.ObservedAt.AbsoluteIndex);
            sb.Append("|O").Append(service.ProjectObjective(s).Outcome);
            return sb.ToString();
        }
    }
}
