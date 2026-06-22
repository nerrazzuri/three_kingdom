using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Relationships;

namespace ThreeKingdom.Domain.Tests.Outcome
{
    /// <summary>
    /// epic-008 story-001：跨系统变更集校验与原子写回。
    /// 治理 ADR：ADR-0004（变更集→校验→原子写回，无半结算态）+ ADR-0002（各权威系统独占写）。
    /// 覆盖 AC-1 原子写回（任一非法 → 整批回滚、零部分写入）、AC-2 守恒（跨系统总量恒等 + 同战果同哈希）、
    /// AC-5 确定性（同变更集 → 同最终状态哈希）。
    /// </summary>
    [TestFixture]
    public class AtomicWritebackTests
    {
        private static readonly OutcomeWritebackService Service = new OutcomeWritebackService();

        private static readonly CityId Fanshui = new CityId("city-fanshui");
        private static readonly FactionId Liu = new FactionId("faction-liu");
        private static readonly CharacterId Guan = new CharacterId("char-guan");
        private static readonly CharacterId Zhang = new CharacterId("char-zhang");

        private static CityEconomyState City(long stock = 1000, long reserved = 0, int civMorale = 50, int security = 50, int fort = 60, int fortMax = 100)
            => new CityEconomyState(Fanshui, stock, reserved, civMorale, security, fort, fortMax);

        private static OutcomeWorld World()
            => OutcomeWorld.Empty
                .WithCity(City())
                .WithReputation(Liu, 100)
                .WithCharacter(Guan, 3000)
                .WithRelationship(new RelationshipKey(Guan, Zhang, RelationshipDimension.Trust), 20);

        // ---- AC-1: 含非法目标 → 整批回滚，零部分写入 ----

        [Test]
        public void test_outcome_change_set_with_illegal_target_rolls_back_entire_batch()
        {
            var world = World();
            var hashBefore = world.ComputeHash();

            // 一条合法（名声 +30）+ 一条非法（库存 −2000 < 0）。
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForReputation(Liu, 30, "守城得胜"),
                OutcomeChange.ForCity(Fanshui, CityField.Stock, -2000, "战耗"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.False);
            Assert.That(result.HasError(OutcomeErrorCode.NegativeResult), Is.True, "返回稳定错误码。");
            // 零部分写入：合法的那条名声变更也不得生效；哈希与字段均不变。
            Assert.That(result.ResultingWorld.GetReputation(Liu), Is.EqualTo(100));
            Assert.That(result.ResultingWorld.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_unknown_target_is_rejected_with_stable_code()
        {
            var world = World();
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForReputation(new FactionId("faction-unknown"), 10, "无此阵营"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.False);
            Assert.That(result.HasError(OutcomeErrorCode.UnknownTarget), Is.True);
        }

        [Test]
        public void test_multiple_partially_illegal_changes_aggregate_errors_and_write_nothing()
        {
            var world = World();
            var hashBefore = world.ComputeHash();
            var set = new ConsequenceSet(OutcomeBranch.Defeat, new[]
            {
                OutcomeChange.ForCity(Fanshui, CityField.Security, -100, "治安崩坏"),      // 50−100 <0
                OutcomeChange.ForCity(Fanshui, CityField.Fortification, 100, "工事超界"),   // 60+100 >100
                OutcomeChange.ForCharacter(Guan, -5000, "全军覆没"),                        // 3000−5000 <0
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.False);
            Assert.That(result.Errors.Count, Is.GreaterThanOrEqualTo(3), "聚合全部非法项。");
            Assert.That(result.HasError(OutcomeErrorCode.NegativeResult), Is.True);
            Assert.That(result.HasError(OutcomeErrorCode.FortificationOutOfRange), Is.True);
            Assert.That(result.ResultingWorld.ComputeHash(), Is.EqualTo(hashBefore));
        }

        // ---- AC-1 边界：同字段多条变更先聚合，最终值合法即可提交 ----

        [Test]
        public void test_same_field_deltas_aggregate_before_invariant_check()
        {
            var world = World();
            // 库存 1000 −800 然后 +300 → 净 −500 → 最终 500（合法）。
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForCity(Fanshui, CityField.Stock, -800, "战耗"),
                OutcomeChange.ForCity(Fanshui, CityField.Stock, 300, "缴获回补"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.True);
            Assert.That(result.ResultingWorld.GetCity(Fanshui).Stock, Is.EqualTo(500));
        }

        // ---- AC-2: 守恒（同键净额须为 0，否则拒绝；满足则跨系统总量恒等）----

        [Test]
        public void test_conservation_group_must_net_to_zero()
        {
            var world = World();
            // 城市拨出军粮 −400，但只有 300 进入后勤同源池 → 净 −100，凭空消失，拒绝。
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForCity(Fanshui, CityField.Stock, -400, "拨给军队", "grain-transfer"),
                OutcomeChange.ForReputation(Liu, 300, "误记为名声", "grain-transfer"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.False);
            Assert.That(result.HasError(OutcomeErrorCode.ConservationViolation), Is.True);
        }

        [Test]
        public void test_conserved_transfer_commits_and_total_is_invariant()
        {
            var world = World();
            long totalBefore = world.GetCity(Fanshui).Stock + world.GetCharacterVitality(Guan);

            // 城市拨出 400 粮，等量计入人物（部队携行）→ 同键净 0，守恒。
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForCity(Fanshui, CityField.Stock, -400, "拨给守军", "grain-move"),
                OutcomeChange.ForCharacter(Guan, 400, "守军受领军粮", "grain-move"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.True);
            long totalAfter = result.ResultingWorld.GetCity(Fanshui).Stock + result.ResultingWorld.GetCharacterVitality(Guan);
            Assert.That(totalAfter, Is.EqualTo(totalBefore), "跨系统总量恒等（无凭空增减）。");
        }

        // ---- AC-5: 确定性（同战果 → 同变更集 → 同最终哈希；与构造无关）----

        [Test]
        public void test_same_change_set_yields_same_final_hash()
        {
            IEnumerable<OutcomeChange> Build() => new[]
            {
                OutcomeChange.ForReputation(Liu, 25, "胜"),
                OutcomeChange.ForCity(Fanshui, CityField.CivMorale, 10, "民心振奋"),
                OutcomeChange.ForRelationship(Guan, Zhang, RelationshipDimension.Trust, 15, "并肩死守"),
            };

            var r1 = Service.Apply(World(), new ConsequenceSet(OutcomeBranch.Victory, Build()));
            var r2 = Service.Apply(World(), new ConsequenceSet(OutcomeBranch.Victory, Build()));

            Assert.That(r1.Committed, Is.True);
            Assert.That(r2.Committed, Is.True);
            Assert.That(r2.ResultHash, Is.EqualTo(r1.ResultHash));
        }

        [Test]
        public void test_relationship_writeback_is_directional_and_clamped()
        {
            var world = World();
            var set = new ConsequenceSet(OutcomeBranch.Victory, new[]
            {
                OutcomeChange.ForRelationship(Guan, Zhang, RelationshipDimension.Trust, 500, "超量增益"),
            });

            var result = Service.Apply(world, set);

            Assert.That(result.Committed, Is.True);
            // 方向性：Guan→Zhang 被 clamp 到 100；反向 Zhang→Guan 不受影响（中性 0）。
            Assert.That(result.ResultingWorld.GetRelationship(new RelationshipKey(Guan, Zhang, RelationshipDimension.Trust)), Is.EqualTo(100));
            Assert.That(result.ResultingWorld.GetRelationship(new RelationshipKey(Zhang, Guan, RelationshipDimension.Trust)), Is.EqualTo(0));
        }
    }
}
