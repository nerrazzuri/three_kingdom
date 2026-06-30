using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Outcome;
using F = ThreeKingdom.Domain.Tests.Session.OutcomeFixture;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-020 story-002：四分支续局选项（Integration / Assembly）。
    /// 治理 ADR：ADR-0002（失败必须产生可继续状态）+ ADR-0009（装配）。TR-outcome-002。
    /// 覆盖：胜/撤退/失城/败北各有续局；任一败局至少一条合法可继续（失败不切死局）；确定性。
    /// </summary>
    [TestFixture]
    public class CampaignContinuationOptionsTests
    {
        private static CampaignSessionService Service => F.Service;

        private static OutcomeContinuation Resolve(OutcomeBranch branch)
            => Service.ResolveBattleOutcome(F.NewSession(), branch, F.Ctx(), F.OutcomeCfg());

        // ---- AC-1: 胜利分支续局选项 ----

        [Test]
        public void test_victory_options_include_pursue_or_consolidate()
        {
            OutcomeContinuation cont = Resolve(OutcomeBranch.Victory);
            Assert.That(cont.Options.Any(o => o.Kind == ContinuationCommandKind.Pursue
                                           || o.Kind == ContinuationCommandKind.Consolidate), Is.True);
        }

        // ---- AC-2: 失城分支含求和 + 兜底 ----

        [Test]
        public void test_city_lost_options_include_sue_for_peace_and_fallback()
        {
            OutcomeContinuation cont = Resolve(OutcomeBranch.CityLost);
            Assert.That(cont.Options.Any(o => o.Kind == ContinuationCommandKind.SueForPeace), Is.True, "失城含求和");
            Assert.That(cont.Options.Any(o => o.Kind == ContinuationCommandKind.Regroup
                                           || o.Kind == ContinuationCommandKind.Accountability), Is.True, "含兜底");
        }

        // ---- AC-3: 败北分支兜底 ----

        [Test]
        public void test_defeat_options_include_regroup_and_accountability()
        {
            OutcomeContinuation cont = Resolve(OutcomeBranch.Defeat);
            Assert.That(cont.Options.Any(o => o.Kind == ContinuationCommandKind.Regroup), Is.True);
            Assert.That(cont.Options.Any(o => o.Kind == ContinuationCommandKind.Accountability), Is.True);
        }

        // ---- AC-4: 任一败局必非空可继续（失败不切死局，TR-outcome-002 核心）----

        [Test]
        public void test_all_defeat_branches_have_playable_continuation()
        {
            foreach (OutcomeBranch branch in new[] { OutcomeBranch.Defeat, OutcomeBranch.CityLost, OutcomeBranch.Retreat })
            {
                OutcomeContinuation cont = Resolve(branch);
                Assert.That(cont.Options.Count, Is.GreaterThan(0), $"{branch} 至少一条续局");
                Assert.That(cont.IsPlayable, Is.True, $"{branch} 可继续（不切死局）");
            }
        }

        [Test]
        public void test_commander_captured_still_has_continuation()
        {
            var ctx = new OutcomeContext(F.Player, city: F.Fanshui, commander: F.Aide, commanderCaptured: true);
            OutcomeContinuation cont = Service.ResolveBattleOutcome(F.NewSession(), OutcomeBranch.Defeat, ctx, F.OutcomeCfg());

            Assert.That(cont.Options.Count, Is.GreaterThan(0), "主将被俘极端败局仍有续局");
        }

        // ---- AC-5: 续局选项存会话 + 确定性 ----

        [Test]
        public void test_options_stored_in_session_and_deterministic()
        {
            CampaignSession a = F.NewSession();
            CampaignSession b = F.NewSession();
            Service.ResolveBattleOutcome(a, OutcomeBranch.CityLost, F.Ctx(), F.OutcomeCfg());
            Service.ResolveBattleOutcome(b, OutcomeBranch.CityLost, F.Ctx(), F.OutcomeCfg());

            Assert.That(a.HasOutcome, Is.True);
            Assert.That(a.LastOutcomeBranch, Is.EqualTo(OutcomeBranch.CityLost));
            Assert.That(a.LastContinuationOptions.Select(o => o.Kind),
                Is.EqualTo(b.LastContinuationOptions.Select(o => o.Kind)), "续局选项确定性");
        }
    }
}
