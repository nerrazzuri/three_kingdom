using System;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-013 story-005：目标循环端到端 + 确定性哈希（Integration / Assembly，epic-013 总验收）。
    /// 治理 ADR：ADR-0004（确定性）+ ADR-0009（R-6 + CD 护栏）。TR-session-004/005。
    /// 串起 S1-S4：开局→战果→后果(004/015/014)→存档 round-trip→续推；确定性 + 失败可继续 + CD 护栏①⑥。
    /// </summary>
    [TestFixture]
    public class CampaignTargetLoopE2ETests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CampaignStartConfig Config()
            => new CampaignStartConfig(
                "scenario-fanshui-siege", Fp,
                new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) });

        private static readonly CampaignSessionService Service = new CampaignSessionService();

        // 代偿路径产出的战果（断粮疲敌，CD 护栏①：携 ≤5 决定性因素）。
        private static BattleOutcomeSummary StarveOutcome()
            => new BattleOutcomeSummary(SiegeOutcome.Fallen,
                new[] { "敌补给断3时段", "守军疲劳跨阈", "城门示弱诱敌失败" });

        // ---- 目标循环端到端 ----

        private static StateHash RunTargetLoop()
        {
            // 开局（S1）
            CampaignSession s = Service.StartCampaign(Config()).Session!;
            // 推进数日（S2）
            Service.Advance(s, 4);
            // 战果（代偿，携 CausalTrace）→ 后果原子写回 004/015/014（S3）
            BattleOutcomeSummary battle = StarveOutcome();
            CampaignCommandResult c = Service.ResolveSiege(s, battle.Outcome, new GovernorStartConfig(30, Frac(1, 10)),
                new SiegeContext(Fanshui, Enemy, new Garrison(500)));
            Assert.That(c.Applied, Is.True);
            // 存档 round-trip（S4）
            CampaignSession restored = Service.Restore(Service.CaptureSnapshot(s), Fp);
            // 失败可继续：续推
            Service.Advance(restored, 2);
            return restored.ComputeHash();
        }

        [Test]
        public void test_target_loop_end_to_end_is_deterministic()
        {
            Assert.That(RunTargetLoop(), Is.EqualTo(RunTargetLoop())); // 同种子+同命令流→同哈希
        }

        [Test]
        public void test_siege_loss_loop_is_continuable_after_save_restore()
        {
            CampaignSession s = Service.StartCampaign(Config()).Session!;
            Service.ResolveSiege(s, SiegeOutcome.Fallen, new GovernorStartConfig(30, Frac(1, 10)),
                new SiegeContext(Fanshui, Enemy, new Garrison(500)));
            CampaignSession restored = Service.Restore(Service.CaptureSnapshot(s), Fp);

            // 失败后会话进入合法可继续态（在野），且仍可推进。
            Assert.That(restored.Career.Career.IsUnaffiliated, Is.True);
            Assert.That(restored.World.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy));
            Assert.DoesNotThrow(() => Service.Advance(restored, 1));
        }

        // ---- CD 护栏 ----

        [Test]
        public void test_battle_outcome_carries_readable_causal_trace()
        {
            BattleOutcomeSummary b = StarveOutcome();
            Assert.That(b.DecisiveFactors.Count, Is.LessThanOrEqualTo(5)); // 护栏①：≤5 可读因素
            Assert.That(b.DecisiveFactors, Is.Not.Empty);
            Assert.Throws<ArgumentException>(() =>
                new BattleOutcomeSummary(SiegeOutcome.Defended, new[] { "1", "2", "3", "4", "5", "6" })); // >5 拒
        }

        [Test]
        public void test_compensation_and_b3_paths_share_outcome_schema()
        {
            // 护栏⑥：代偿路径与（mock）完整 B3 路径产出同一 BattleOutcomeSummary schema → B3 深化纯叠加。
            BattleOutcomeSummary compensation = StarveOutcome();
            BattleOutcomeSummary mockB3 = new BattleOutcomeSummary(SiegeOutcome.Fallen,
                new[] { "战区伏击得手", "敌主力溃" }); // 假设来自完整战役命令层
            Assert.That(compensation.GetType(), Is.EqualTo(mockB3.GetType()));
            // 两者皆可驱动同一后果写回路径（同 SiegeOutcome 契约）。
            CampaignSession a = Service.StartCampaign(Config()).Session!;
            CampaignSession bb = Service.StartCampaign(Config()).Session!;
            Assert.That(Service.ResolveSiege(a, compensation.Outcome, new GovernorStartConfig(0, FixedPoint.Zero), new SiegeContext(Fanshui, Enemy, new Garrison(500))).Applied, Is.True);
            Assert.That(Service.ResolveSiege(bb, mockB3.Outcome, new GovernorStartConfig(0, FixedPoint.Zero), new SiegeContext(Fanshui, Enemy, new Garrison(500))).Applied, Is.True);
        }
    }
}
