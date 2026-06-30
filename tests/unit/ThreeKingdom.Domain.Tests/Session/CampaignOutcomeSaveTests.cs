using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using F = ThreeKingdom.Domain.Tests.Session.OutcomeFixture;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-020 story-004：后果续局态存读档 + 确定性（Integration / Assembly）。
    /// 治理 ADR：ADR-0005（存档 round-trip）+ ADR-0004（确定性）。TR-outcome-001/002。
    /// 覆盖：写回态 round-trip；续局态 round-trip；哈希一致；确定性链。
    /// </summary>
    [TestFixture]
    public class CampaignOutcomeSaveTests
    {
        private static CampaignSessionService Service => F.Service;

        // 后果会话含城市治理态，Restore 须提供城市配置。
        private static CampaignSession Restore(string text)
            => Service.Restore(text, F.Fp,
                settlementConfig: F.SettlementConfig(), governanceConfig: F.GovernanceConfig(),
                populationPressure: FixedPoint.FromInt(1));

        // ---- AC-1: 后果写回态 round-trip 一致 ----

        [Test]
        public void test_writeback_state_roundtrip()
        {
            CampaignSession s = F.NewSession(F.CityState(morale: 60, fortCur: 40));
            Service.ResolveBattleOutcome(s, OutcomeBranch.Defeat, F.Ctx(), F.OutcomeCfg());

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.CityEconomy!.CivMorale, Is.EqualTo(s.CityEconomy!.CivMorale));
            Assert.That(loaded.CityEconomy!.FortificationCurrent, Is.EqualTo(s.CityEconomy!.FortificationCurrent));
        }

        // ---- AC-2: 最近续局态 round-trip ----

        [Test]
        public void test_continuation_state_roundtrip()
        {
            CampaignSession s = F.NewSession();
            Service.ResolveBattleOutcome(s, OutcomeBranch.CityLost, F.Ctx(), F.OutcomeCfg());

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.HasOutcome, Is.True);
            Assert.That(loaded.LastOutcomeBranch, Is.EqualTo(OutcomeBranch.CityLost));
            Assert.That(loaded.LastContinuationOptions.Select(o => o.Kind),
                Is.EqualTo(s.LastContinuationOptions.Select(o => o.Kind)), "续局选项 round-trip");
        }

        // ---- AC-3: round-trip 哈希一致 ----

        [Test]
        public void test_outcome_roundtrip_preserves_hash()
        {
            CampaignSession s = F.NewSession();
            Service.ResolveBattleOutcome(s, OutcomeBranch.Defeat, F.Ctx(), F.OutcomeCfg());
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }

        // ---- AC-4: 存档不中断确定性链 ----

        [Test]
        public void test_save_at_midpoint_does_not_break_determinism_chain()
        {
            // 直推：后果写回。
            CampaignSession direct = F.NewSession();
            Service.ResolveBattleOutcome(direct, OutcomeBranch.Defeat, F.Ctx(), F.OutcomeCfg());
            StateHash directHash = direct.ComputeHash();

            // 切割：存读档 → 后果写回。
            CampaignSession s = F.NewSession();
            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));
            Service.ResolveBattleOutcome(loaded, OutcomeBranch.Defeat, F.Ctx(), F.OutcomeCfg());

            Assert.That(loaded.ComputeHash(), Is.EqualTo(directHash), "存档切割点不影响后果写回确定性");
        }

        // ---- 无后果态向后兼容 ----

        [Test]
        public void test_no_outcome_session_roundtrip()
        {
            CampaignSession s = F.NewSession();   // 未写回后果
            StateHash before = s.ComputeHash();

            CampaignSession loaded = Restore(Service.CaptureSnapshot(s));

            Assert.That(loaded.HasOutcome, Is.False);
            Assert.That(loaded.ComputeHash(), Is.EqualTo(before));
        }
    }
}
