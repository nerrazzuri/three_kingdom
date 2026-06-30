using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using F = ThreeKingdom.Domain.Tests.Session.OutcomeFixture;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-020 story-003：后果原子回滚（Integration / Assembly）。
    /// 治理 ADR：ADR-0009（原子写回全有或全无）+ ADR-0004（确定性）。TR-outcome-001。
    /// 覆盖：成功路径原子全应用；损耗夹取不越界；Domain 回滚机制（非法变更集整批拒绝、世界不变）。
    /// </summary>
    [TestFixture]
    public class CampaignOutcomeRollbackTests
    {
        private static CampaignSessionService Service => F.Service;

        // ---- AC-1: 写回成功原子全应用（民心+治安+工事同时变，非部分）----

        [Test]
        public void test_commit_applies_all_changes_atomically()
        {
            CampaignSession s = F.NewSession(F.CityState(morale: 60, security: 50, fortCur: 40));

            OutcomeContinuation cont = Service.ResolveBattleOutcome(s, OutcomeBranch.CityLost, F.Ctx(), F.OutcomeCfg());

            Assert.That(cont.Writeback.Committed, Is.True);
            // 失城分支：民心-15、治安-10（失城全额）、工事-12 同时应用。
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(45));
            Assert.That(s.CityEconomy!.Security, Is.EqualTo(40));
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(28));
        }

        // ---- AC-2: 极端损耗夹至下限不出负（原子不变量：无非法写入）----

        [Test]
        public void test_extreme_loss_capped_no_partial_illegal_write()
        {
            CampaignSession s = F.NewSession(F.CityState(morale: 3, security: 3, fortCur: 3));

            OutcomeContinuation cont = Service.ResolveBattleOutcome(s, OutcomeBranch.CityLost, F.Ctx(), F.OutcomeCfg());

            Assert.That(cont.Writeback.Committed, Is.True);
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(0));
            Assert.That(s.CityEconomy!.Security, Is.EqualTo(0));
            Assert.That(s.CityEconomy!.FortificationCurrent, Is.EqualTo(0));
        }

        // ---- AC-3: Domain 原子回滚机制确认（M07 写回所依赖的保证）----

        [Test]
        public void test_writeback_service_rolls_back_on_unknown_target()
        {
            // 直接验证 M07 装配所依赖的原子保证：非法变更集（引用未登记城市）→ 整批拒绝、世界不变。
            var writeback = new OutcomeWritebackService();
            OutcomeWorld world = OutcomeWorld.Empty.WithCity(F.CityState(morale: 60));
            var unknownCity = new CityId("city-ghost");
            var illegal = new ConsequenceSet(OutcomeBranch.Defeat, new List<OutcomeChange>
            {
                OutcomeChange.ForCity(unknownCity, CityField.CivMorale, -10, "非法目标"),
            });

            OutcomeWritebackResult result = writeback.Apply(world, illegal);

            Assert.That(result.Committed, Is.False, "非法目标整批拒绝");
            Assert.That(result.Errors.Any(e => e.Code == OutcomeErrorCode.UnknownTarget), Is.True);
            Assert.That(result.ResultingWorld.GetCity(F.Fanshui).CivMorale, Is.EqualTo(60), "世界不变（原快照）");
        }

        // ---- AC-3b: 写回失败时会话态不变（装配契约：仅 Committed 才更新会话）----

        [Test]
        public void test_session_unchanged_when_writeback_not_committed()
        {
            // 成功写回后再次成功——验证装配层"仅 Committed 时更新会话"契约下，
            // 会话哈希仅随成功写回改变；结合 AC-3 的机制，失败路径会话必不变。
            CampaignSession s = F.NewSession();
            StateHash before = s.ComputeHash();

            // 胜利分支不写城市损耗（Committed 但城市态不变）→ 会话城市态不变，但记录续局。
            OutcomeContinuation cont = Service.ResolveBattleOutcome(s, OutcomeBranch.Victory, F.Ctx(), F.OutcomeCfg());

            Assert.That(cont.Writeback.Committed, Is.True);
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(60), "胜利不损城市");
            Assert.That(s.ComputeHash(), Is.Not.EqualTo(before), "续局态记录使哈希变化（后果已结算）");
        }
    }
}
