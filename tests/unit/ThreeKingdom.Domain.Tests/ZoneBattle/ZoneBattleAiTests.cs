using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Domain.Tests.ZoneBattle
{
    /// <summary>
    /// S5 敌方区域AI（GDD_021 R6 / ADR-0013，落地 GDD_016）：确定性种子选择 + 反全知（隐蔽伏兵不可见）+
    /// 增援受威胁区 + 同规则不作弊（只相邻）+ 渐进记忆。
    /// </summary>
    [TestFixture]
    public class ZoneBattleAiTests
    {
        private static FixedPoint F(int n, int d) => FixedPoint.FromFraction(n, d);
        private readonly EnemyZoneAiService _ai = new EnemyZoneAiService();

        private static Detachment Det(string id, BattleSide side, ZoneId at, int strength)
            => new Detachment(new DetachmentId(id), side, null, TroopComposition.AllInfantry(strength),
                strength, F(7, 10), F(2, 10), Posture.Assault, at);

        // 玩家=攻方；AI=守方。
        private static ZoneBattleState State(IReadOnlyList<Detachment> dets, IReadOnlyList<ZoneEngagementState>? eng = null)
            => new ZoneBattleState(BattleField.Default(), dets, eng ?? Array.Empty<ZoneEngagementState>(),
                new BattleClock(1, 6), BattleSide.Attacker, seed: 99UL);

        [Test]
        public void test_ai_decision_is_deterministic()
        {
            ZoneBattleState s = State(new[]
            {
                Det("p1", BattleSide.Attacker, BattleField.Front, 600),
                Det("e1", BattleSide.Defender, BattleField.Reserve, 300),
                Det("e2", BattleSide.Defender, BattleField.Cover, 200),
            });
            StateHash h1 = _ai.Decide(s, BattleSide.Defender, ZoneBattleConfig.Default, EnemyAiConfig.Default).Hash();
            StateHash h2 = _ai.Decide(s, BattleSide.Defender, ZoneBattleConfig.Default, EnemyAiConfig.Default).Hash();
            Assert.That(h2, Is.EqualTo(h1), "同态 → 同决策（种子确定性，可复现）。");
        }

        [Test]
        public void test_ai_reinforces_visibly_threatened_front()
        {
            // 玩家重兵压正面（可见）→ 守方AI 把预备队调向正面（预备与正面相邻）。
            var sharp = new EnemyAiConfig(null, 30, 20, 40, 25, 15, sharpness: 6);
            ZoneBattleState s = State(new[]
            {
                Det("p1", BattleSide.Attacker, BattleField.Front, 5000),
                Det("e1", BattleSide.Defender, BattleField.Reserve, 300),
            });
            ZoneBattleState after = _ai.Decide(s, BattleSide.Defender, ZoneBattleConfig.Default, sharp);
            Detachment e1 = after.TryGet(new DetachmentId("e1"))!;
            Assert.That(e1.InTransit, Is.True, "受威胁 → AI 调动预备队。");
            Assert.That(e1.TransitTarget, Is.EqualTo(BattleField.Front), "增援可见受威胁的正面。");
        }

        [Test]
        public void test_ai_cannot_see_hidden_ambush()
        {
            // 玩家伏兵在侧翼蓄势（AmbushCharge>0），AI 不在侧翼 → AI 视野中侧翼无敌情（反全知）。
            var flankCharging = new ZoneEngagementState(BattleField.Flank, ambushCharge: 1, starveTurns: 0, formed: null);
            ZoneBattleState s = State(new[]
            {
                Det("p-ambush", BattleSide.Attacker, BattleField.Flank, 400),
                Det("e1", BattleSide.Defender, BattleField.Front, 300),
            }, new[] { flankCharging });

            AiWorldView view = AiWorldView.BuildFor(s, BattleSide.Defender);
            Assert.That(view.VisibleEnemyIn(BattleField.Flank), Is.EqualTo(0), "蓄势伏兵 + AI 不在场 → 敌隐蔽不可见。");
            Assert.That(view.VisibleEnemyIn(BattleField.Front), Is.EqualTo(0), "正面无攻方支队。");
        }

        [Test]
        public void test_ai_sees_ambush_zone_once_it_contests_it()
        {
            var flankCharging = new ZoneEngagementState(BattleField.Flank, ambushCharge: 1, starveTurns: 0, formed: null);
            ZoneBattleState s = State(new[]
            {
                Det("p-ambush", BattleSide.Attacker, BattleField.Flank, 400),
                Det("e1", BattleSide.Defender, BattleField.Flank, 300),   // AI 在侧翼 → 接触
            }, new[] { flankCharging });
            AiWorldView view = AiWorldView.BuildFor(s, BattleSide.Defender);
            Assert.That(view.VisibleEnemyIn(BattleField.Flank), Is.EqualTo(400), "AI 在场接触 → 伏兵暴露、可见。");
        }

        [Test]
        public void test_ai_never_moves_to_nonadjacent_zone()
        {
            ZoneBattleState s = State(new[]
            {
                Det("p1", BattleSide.Attacker, BattleField.Supply, 800),
                Det("e1", BattleSide.Defender, BattleField.Reserve, 300),   // 预备与敌粮道不相邻
            });
            ZoneBattleState after = _ai.Decide(s, BattleSide.Defender, ZoneBattleConfig.Default, EnemyAiConfig.Default);
            Detachment e1 = after.TryGet(new DetachmentId("e1"))!;
            if (e1.InTransit)
                Assert.That(after.Field.AreAdjacent(e1.Location, e1.TransitTarget!.Value), Is.True,
                    "AI 调动只限相邻（同规则不作弊，无瞬移）。");
            Assert.Pass();
        }

        [Test]
        public void test_ai_updates_memory_with_visible_enemy()
        {
            ZoneBattleState s = State(new[]
            {
                Det("p1", BattleSide.Attacker, BattleField.Front, 700),
                Det("e1", BattleSide.Defender, BattleField.Reserve, 300),
            });
            ZoneBattleState after = _ai.Decide(s, BattleSide.Defender, ZoneBattleConfig.Default, EnemyAiConfig.Default);
            Assert.That(after.Memory.LastVisible(BattleField.Front), Is.EqualTo(700), "记忆记下可见敌情（供趋势判断）。");
        }
    }
}
