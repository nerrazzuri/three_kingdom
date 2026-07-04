using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 延迟治理（GDD_004 太守派人处理→需时见效，非即时）：下令记为在办，推进到完成时刻才应用效果。
    /// 覆盖 在办不即效 / 未到不生效 / 到时生效 / 非法下令拒绝 / 存档 round-trip / 确定性。
    /// 用安抚（半日=2 时段，不跨日 → 不触城市日结，断言干净）。
    /// </summary>
    [TestFixture]
    public class CampaignGovernanceDelayTests
    {
        private readonly CampaignSessionService _service = new CampaignSessionService();

        private CampaignSession NewSession() => _service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;

        private const int AppeaseLead = 2;   // 半日

        [Test]
        public void test_dispatch_appease_is_pending_not_immediate()
        {
            var s = NewSession();
            int moraleBefore = s.CityEconomy!.CivMorale;

            CampaignCommandResult r = _service.DispatchAppease(s, AppeaseLead);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.PendingGovernance.Count, Is.EqualTo(1), "记为在办一件。");
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(moraleBefore), "下令即时不改民心（非即时）。");
        }

        [Test]
        public void test_governance_not_applied_before_completion()
        {
            var s = NewSession();
            int moraleBefore = s.CityEconomy!.CivMorale;
            _service.DispatchAppease(s, AppeaseLead);

            _service.Advance(s, AppeaseLead - 1);   // 未到完成时刻，且不跨日

            Assert.That(s.PendingGovernance.Count, Is.EqualTo(1), "未到完成仍在办。");
            Assert.That(s.CityEconomy!.CivMorale, Is.EqualTo(moraleBefore));
        }

        [Test]
        public void test_governance_applied_after_completion()
        {
            var s = NewSession();
            int moraleBefore = s.CityEconomy!.CivMorale;
            _service.DispatchAppease(s, AppeaseLead);

            _service.Advance(s, AppeaseLead);   // 到完成时刻（半日，不跨日 → 仅安抚生效）

            Assert.That(s.PendingGovernance.Count, Is.EqualTo(0), "完成后移出在办。");
            Assert.That(s.CityEconomy!.CivMorale, Is.GreaterThan(moraleBefore), "完成后民心提升。");
        }

        [Test]
        public void test_dispatch_requisition_over_stock_rejected_at_dispatch()
        {
            var s = NewSession();
            CampaignCommandResult r = _service.DispatchRequisition(s, 1_000_000, 4);

            Assert.That(r.Applied, Is.False);
            Assert.That(r.Error, Is.EqualTo(CampaignErrorCode.InsufficientStock));
            Assert.That(s.PendingGovernance.Count, Is.EqualTo(0), "非法下令不入在办（零写入）。");
        }

        [Test]
        public void test_pending_governance_round_trips_through_save()
        {
            var s = NewSession();
            int moraleBefore = s.CityEconomy!.CivMorale;
            _service.DispatchAppease(s, AppeaseLead);

            string saved = _service.CaptureSnapshot(s);
            CampaignSession restored = Restore(saved);

            Assert.That(restored.PendingGovernance.Count, Is.EqualTo(1), "在办治理存读档保留。");
            Assert.That(restored.PendingGovernance[0].Kind, Is.EqualTo(GovernanceActionKind.Appease));

            _service.Advance(restored, AppeaseLead);
            Assert.That(restored.PendingGovernance.Count, Is.EqualTo(0));
            Assert.That(restored.CityEconomy!.CivMorale, Is.GreaterThan(moraleBefore), "读档后推进正常见效。");
        }

        [Test]
        public void test_governance_dispatch_is_deterministic()
        {
            var a = NewSession();
            var b = NewSession();
            _service.DispatchAppease(a, AppeaseLead);
            _service.DispatchAppease(b, AppeaseLead);
            _service.Advance(a, AppeaseLead);
            _service.Advance(b, AppeaseLead);

            Assert.That(b.CityEconomy!.CivMorale, Is.EqualTo(a.CityEconomy!.CivMorale), "同序列同民心。");
        }

        private CampaignSession Restore(string text)
        {
            CampaignStartConfig cfg = PlayableCampaign.Default().StartConfig;
            return _service.Restore(
                text, cfg.Fingerprint,
                settlementConfig: cfg.SettlementConfig, governanceConfig: cfg.GovernanceConfig,
                populationPressure: cfg.PopulationPressure,
                intelConfig: cfg.IntelConfig, councilSetup: cfg.CouncilSetup,
                prepConfig: cfg.PreparationConfig,
                reachableRegions: cfg.ReachableRegions, authorizedOrders: cfg.AuthorizedOrders);
        }
    }
}
