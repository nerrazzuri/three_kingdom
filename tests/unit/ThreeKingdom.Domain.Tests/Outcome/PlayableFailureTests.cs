using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Outcome;

namespace ThreeKingdom.Domain.Tests.Outcome
{
    /// <summary>
    /// epic-008 story-002：可玩失败延续（撤退/失城/问责分支）。
    /// 治理 ADR：ADR-0002（失败为分支，经 Command 路径）+ 强制设计锁「失败必须产生可继续状态」。
    /// 覆盖 AC-1 失败后世界完整 + ≥1 合法可继续命令、AC-2 不同败因写回不同变更集且均可继续、
    /// 分支结算（非单一胜负开关）、确定性。
    /// </summary>
    [TestFixture]
    public class PlayableFailureTests
    {
        private static readonly FailureContinuationService Service = new FailureContinuationService();

        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly FactionId Liu = new FactionId("faction-liu");
        private static readonly CharacterId Guan = new CharacterId("char-guan");

        private static OutcomeConsequenceConfig Config()
            => new OutcomeConsequenceConfig(
                reputationLossDefeat: 20,
                reputationLossRetreat: 8,
                reputationLossCityLost: 35,
                civMoraleLoss: 15,
                securityLoss: 20,
                fortificationDamage: 25,
                forceAttrition: 600);

        private static OutcomeWorld World()
            => OutcomeWorld.Empty
                .WithCity(new CityEconomyState(Fanshui, 1000, 0, 50, 50, 60, 100))
                .WithReputation(Liu, 100)
                .WithCharacter(Guan, 3000);

        private static OutcomeContext Ctx(bool captured = false)
            => new OutcomeContext(Liu, Fanshui, Guan, captured);

        // ---- AC-1: 失败后世界完整 + 存在 ≥1 合法可继续命令 ----

        [Test]
        public void test_defeat_outcome_yields_complete_world_and_at_least_one_continuation()
        {
            var continuation = Service.Resolve(World(), OutcomeBranch.Defeat, Ctx(), Config());

            Assert.That(continuation.Writeback.Committed, Is.True, "失败结算仍原子写回成功（世界完整）。");
            Assert.That(continuation.Options.Count, Is.GreaterThanOrEqualTo(1), "败局存在 ≥1 合法可继续命令。");
            Assert.That(continuation.IsPlayable, Is.True);
            // 名声受损但世界仍在（非空白界面）。
            Assert.That(continuation.Writeback.ResultingWorld.GetReputation(Liu), Is.EqualTo(80));
        }

        [Test]
        public void test_worst_case_city_lost_and_commander_captured_is_still_playable()
        {
            // 极端败局：失城 + 主将被俘 —— 仍须可继续（问责/重整）。
            var continuation = Service.Resolve(World(), OutcomeBranch.CityLost, Ctx(captured: true), Config());

            Assert.That(continuation.IsPlayable, Is.True);
            Assert.That(continuation.HasOption(ContinuationCommandKind.Accountability), Is.True);
            Assert.That(continuation.HasOption(ContinuationCommandKind.Regroup), Is.True);
        }

        [Test]
        public void test_all_failure_branches_have_playable_continuation()
        {
            foreach (var branch in new[] { OutcomeBranch.Retreat, OutcomeBranch.CityLost, OutcomeBranch.Defeat })
                Assert.That(Service.HasPlayableContinuation(branch, Ctx()), Is.True, $"{branch} 须可继续。");
        }

        // ---- AC-2: 不同败因 → 不同变更集，均可继续 ----

        [Test]
        public void test_retreat_and_city_lost_write_different_change_sets_both_continuable()
        {
            var retreat = Service.Resolve(World(), OutcomeBranch.Retreat, Ctx(), Config());
            var cityLost = Service.Resolve(World(), OutcomeBranch.CityLost, Ctx(), Config());

            // 撤退名声损失 8 → 92；失城损失 35 → 65（分支差异）。
            Assert.That(retreat.Writeback.ResultingWorld.GetReputation(Liu), Is.EqualTo(92));
            Assert.That(cityLost.Writeback.ResultingWorld.GetReputation(Liu), Is.EqualTo(65));
            // 写回后世界哈希不同（不同变更集）。
            Assert.That(cityLost.Writeback.ResultHash, Is.Not.EqualTo(retreat.Writeback.ResultHash));
            // 各自的可继续命令不同：撤退含 Retreat，失城含 SueForPeace。
            Assert.That(retreat.HasOption(ContinuationCommandKind.Retreat), Is.True);
            Assert.That(cityLost.HasOption(ContinuationCommandKind.SueForPeace), Is.True);
            Assert.That(retreat.IsPlayable && cityLost.IsPlayable, Is.True);
        }

        // ---- 分支结算（非单一胜负开关）：胜局亦延续 ----

        [Test]
        public void test_victory_is_a_branch_that_also_continues_the_loop()
        {
            var victory = Service.Resolve(World(), OutcomeBranch.Victory, Ctx(), Config());

            Assert.That(victory.IsPlayable, Is.True);
            Assert.That(victory.HasOption(ContinuationCommandKind.Pursue), Is.True);
            // 胜局不损名声。
            Assert.That(victory.Writeback.ResultingWorld.GetReputation(Liu), Is.EqualTo(100));
        }

        // ---- 损失上限夹取：不写出负值（守住原子写回不变量）----

        [Test]
        public void test_loss_is_capped_to_current_and_never_writes_negative()
        {
            // 主将计量仅 400 < 减员 600 → 夹取到 400，写回后 0 而非负。
            var world = OutcomeWorld.Empty
                .WithReputation(Liu, 100)
                .WithCharacter(Guan, 400)
                .WithCity(new CityEconomyState(Fanshui, 1000, 0, 5, 5, 10, 100));
            var continuation = Service.Resolve(world, OutcomeBranch.Defeat, Ctx(), Config());

            Assert.That(continuation.Writeback.Committed, Is.True);
            Assert.That(continuation.Writeback.ResultingWorld.GetCharacterVitality(Guan), Is.EqualTo(0));
            Assert.That(continuation.Writeback.ResultingWorld.GetCity(Fanshui).CivMorale, Is.EqualTo(0));
        }

        // ---- 确定性 ----

        [Test]
        public void test_same_inputs_yield_same_writeback_hash()
        {
            var a = Service.Resolve(World(), OutcomeBranch.CityLost, Ctx(), Config());
            var b = Service.Resolve(World(), OutcomeBranch.CityLost, Ctx(), Config());
            Assert.That(b.Writeback.ResultHash, Is.EqualTo(a.Writeback.ResultHash));
        }
    }
}
