using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Intel;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 延迟侦察（GDD_007 派出→在途→返报，非即时）：派出记为在途，推进到返报时刻才并入知识。
    /// 覆盖 在途不即知 / 未到不返报 / 到时返报 / 确定性 / 存档 round-trip。
    /// </summary>
    [TestFixture]
    public class CampaignScoutDelayTests
    {
        private static readonly PlayableCampaign Scenario = PlayableCampaign.Default();
        private readonly CampaignSessionService _service = new CampaignSessionService();

        // 每局用<b>全新</b>场景：StartConfig 内含可变 FactionIntel，共用会跨会话串知识（用例隔离）。
        private CampaignSession NewSession() => _service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;

        private const int Lead = 4;   // 1 日 = 4 时段

        [Test]
        public void test_dispatch_scout_is_pending_not_immediately_known()
        {
            var s = NewSession();

            CampaignCommandResult r = _service.DispatchScout(s, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);

            Assert.That(r.Applied, Is.True);
            Assert.That(s.PendingScouts.Count, Is.EqualTo(1), "记为在途一支。");
            Assert.That(s.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.False, "派出即时不产生数字（非即时）。");
        }

        [Test]
        public void test_scout_not_returned_before_lead_elapses()
        {
            var s = NewSession();
            _service.DispatchScout(s, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);

            _service.Advance(s, Lead - 1);   // 未到返报时刻

            Assert.That(s.PendingScouts.Count, Is.EqualTo(1), "未到返报仍在途。");
            Assert.That(s.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.False);
        }

        [Test]
        public void test_scout_returns_after_lead_elapses()
        {
            var s = NewSession();
            _service.DispatchScout(s, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);

            _service.Advance(s, Lead);   // 到返报时刻

            Assert.That(s.PendingScouts.Count, Is.EqualTo(0), "返报后移出在途。");
            Assert.That(s.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.True, "返报后敌情入知识。");
        }

        [Test]
        public void test_delayed_scout_is_deterministic()
        {
            var a = NewSession();
            var b = NewSession();
            _service.DispatchScout(a, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);
            _service.DispatchScout(b, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);
            _service.Advance(a, Lead);
            _service.Advance(b, Lead);

            a.PlayerKnowledge!.TryGet(PlayableCampaign.EnemyArmy, out IntelKnowledgeEntry ea);
            b.PlayerKnowledge!.TryGet(PlayableCampaign.EnemyArmy, out IntelKnowledgeEntry eb);
            Assert.That(eb.KnownStrength, Is.EqualTo(ea.KnownStrength), "同序列同报告值。");
            Assert.That(eb.ObservedAt.AbsoluteIndex, Is.EqualTo(ea.ObservedAt.AbsoluteIndex), "同返报时刻。");
        }

        [Test]
        public void test_pending_scout_round_trips_through_save()
        {
            var s = NewSession();
            _service.DispatchScout(s, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);

            string saved = _service.CaptureSnapshot(s);
            CampaignSession restored = Restore(saved);

            Assert.That(restored.PendingScouts.Count, Is.EqualTo(1), "在途侦察存读档保留。");
            Assert.That(restored.PendingScouts[0].Subject.Value, Is.EqualTo(PlayableCampaign.EnemyArmy.Value));

            // 读档后继续推进到返报 → 正常返报入知识（在途状态完整恢复）。
            _service.Advance(restored, Lead);
            Assert.That(restored.PendingScouts.Count, Is.EqualTo(0));
            Assert.That(restored.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.True);
        }

        [Test]
        public void test_two_sessions_from_same_config_have_independent_intel()
        {
            // 回归「重开新游戏串知识」：同一 StartConfig 开两局，一局侦察返报，另一局不应受影响。
            PlayableCampaign scenario = PlayableCampaign.Default();

            CampaignSession first = _service.StartCampaign(scenario.StartConfig).Session!;
            _service.DispatchScout(first, PlayableCampaign.EnemyArmy, IntelSource.Scouting, Lead);
            _service.Advance(first, Lead);
            Assert.That(first.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.True, "first 局已侦得敌情。");

            CampaignSession second = _service.StartCampaign(scenario.StartConfig).Session!;
            Assert.That(second.PlayerKnowledge!.Knows(PlayableCampaign.EnemyArmy), Is.False,
                "同配置另开一局不应继承上一局的情报（每局独立情报层）。");
        }

        private CampaignSession Restore(string text)
        {
            CampaignStartConfig cfg = Scenario.StartConfig;
            return _service.Restore(
                text, cfg.Fingerprint,
                settlementConfig: cfg.SettlementConfig, governanceConfig: cfg.GovernanceConfig,
                populationPressure: cfg.PopulationPressure,
                intelConfig: cfg.IntelConfig, councilSetup: cfg.CouncilSetup,
                prepConfig: cfg.PreparationConfig,
                reachableRegions: cfg.ReachableRegions, authorizedOrders: cfg.AuthorizedOrders,
                battleConfig: Scenario.BattleConfig, tacticChains: Scenario.TacticChains);
        }
    }
}
