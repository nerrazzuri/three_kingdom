using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.GridBattle;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.GridBattle
{
    /// <summary>
    /// 格子战术战斗引擎（ADR-0018 / GDD-028）验收：确定性 · 寻路 · 兵种定速 · 伏击 · 火攻断粮 ·
    /// 补给回血/叛逃/伤亡倍率 · 半路遭遇 · 段序交战 · 终局 · 命令校验 · 哈希 round-trip。
    /// </summary>
    [TestFixture]
    public class GridBattleEngineTests
    {
        private static readonly GridBattleConfig Cfg = GridBattleConfig.Default();

        private static BattleUnitId U(string s) => new BattleUnitId(s);
        private static GridCoord C(int x, int y) => new GridCoord(x, y);

        private static GridUnit Unit(string id, GridSide side, TroopKind kind, int x, int y, int str, int atk, GridCoord? dest = null)
            => new GridUnit(U(id), side, kind, C(x, y), str, str, atk, dest);

        private static GridBattleState State(BattleGrid grid, int pSup, int eSup, params GridUnit[] units)
            => new GridBattleState(grid, units, new WorldTime(0, DaySegment.Dawn), pSup, eSup);

        // 开阔小场（含 G/g；角落远离交战），用于运动/补给隔离测试。
        private static BattleGrid OpenGrid() => BattleGrid.FromRows(new[]
        {
            "G........",
            ".........",
            ".........",
            "........g",
        });

        // AC-1 确定性：同初态+同命令序，连推 → 同哈希；且原态不被 Advance 改动。
        [Test]
        public void test_determinism_same_input_same_hash()
        {
            var grid = OpenGrid();
            long RunFinalHash()
            {
                var s = State(grid, 100, 100,
                    Unit("p1", GridSide.Player, TroopKind.Cavalry, 0, 1, 100, 20, C(6, 1)),
                    Unit("e1", GridSide.Enemy, TroopKind.Infantry, 8, 2, 100, 18, C(2, 2)));
                for (int i = 0; i < 6; i++) s = GridBattleEngine.Advance(s, Cfg).State;
                return s.Hash();
            }
            Assert.That(RunFinalHash(), Is.EqualTo(RunFinalHash()), "同一初态+命令序须产出同一状态哈希（可回放）");
        }

        [Test]
        public void test_advance_does_not_mutate_original()
        {
            var s = State(OpenGrid(), 100, 100,
                Unit("p1", GridSide.Player, TroopKind.Cavalry, 0, 1, 100, 20, C(6, 1)),
                Unit("e1", GridSide.Enemy, TroopKind.Infantry, 8, 2, 100, 18));
            long before = s.Hash();
            GridBattleEngine.Advance(s, Cfg);
            Assert.That(s.Hash(), Is.EqualTo(before), "Advance 须在克隆上推进，原态不变");
        }

        // AC-2 寻路：绕山、经缺口；绝不落在山格。
        [Test]
        public void test_pathfinding_routes_around_mountain()
        {
            var grid = BattleGrid.FromRows(new[]
            {
                "G.......g",
                ".........",
                "...^^^...",
                ".........",
                ".........",
            });
            var p = Unit("p1", GridSide.Player, TroopKind.Cavalry, 4, 1, 100, 10, C(4, 4));
            var s = State(grid, 30, 30, p, Unit("e", GridSide.Enemy, TroopKind.Infantry, 0, 4, 100, 5));
            for (int i = 0; i < 8 && s.UnitAt(C(4, 4)) == null; i++)
            {
                s = GridBattleEngine.Advance(s, Cfg).State;
                foreach (var u in s.Units)
                    Assert.That(grid.TerrainAt(u.Position), Is.Not.EqualTo(TerrainKind.Mountain), "单位不得落在山格");
            }
            Assert.That(s.UnitAt(C(4, 4))?.Id, Is.EqualTo(U("p1")), "应绕山抵达目的地");
        }

        // AC-3 兵种定速：骑兵一段即到，步兵未到。
        [Test]
        public void test_cavalry_faster_than_infantry()
        {
            var s = State(OpenGrid(), 30, 30,
                Unit("cav", GridSide.Player, TroopKind.Cavalry, 0, 0, 100, 10, C(3, 0)),
                Unit("inf", GridSide.Player, TroopKind.Infantry, 0, 2, 100, 10, C(3, 2)),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 8, 3, 100, 5));
            s = GridBattleEngine.Advance(s, Cfg).State;
            Assert.That(s.UnitAt(C(3, 0))?.Id, Is.EqualTo(U("cav")), "骑兵速3应一段到达 3 格外目的地");
            Assert.That(s.UnitAt(C(3, 2)), Is.Null, "步兵速1一段不应到达 3 格外");
        }

        // AC-4 伏击：林中相邻隘口内敌 → 伤害 ×AmbushPct（250）。
        [Test]
        public void test_ambush_from_forest_into_pass_multiplies_damage()
        {
            var grid = BattleGrid.FromRows(new[]
            {
                "G...g",
                ".FP..",
                ".....",
            });
            var s = State(grid, 30, 30,
                Unit("amb", GridSide.Player, TroopKind.Infantry, 1, 1, 200, 20),   // 林 (1,1)
                Unit("victim", GridSide.Enemy, TroopKind.Infantry, 2, 1, 200, 1)); // 隘 (2,1)，攻低避免反杀干扰
            int before = 200;
            s = GridBattleEngine.Advance(s, Cfg).State;
            int after = s.UnitAt(C(2, 1))!.Strength;
            Assert.That(before - after, Is.EqualTo(20 * Cfg.AmbushPct / 100), "林中入隘伏击应 ×2.5 = 50");
        }

        [Test]
        public void test_no_ambush_on_open_ground()
        {
            var grid = BattleGrid.FromRows(new[] { "G...g", ".....", "....." });
            var s = State(grid, 30, 30,
                Unit("a", GridSide.Player, TroopKind.Infantry, 1, 1, 200, 20),
                Unit("b", GridSide.Enemy, TroopKind.Infantry, 2, 1, 200, 1));
            s = GridBattleEngine.Advance(s, Cfg).State;
            Assert.That(200 - s.UnitAt(C(2, 1))!.Strength, Is.EqualTo(20), "平地相邻应为基础伤害，无伏击");
        }

        // AC-5 火攻断粮：贴敌粮仓 + 白昼 → 焚仓、敌补给暴跌。
        [Test]
        public void test_fire_attack_burns_enemy_granary()
        {
            var grid = BattleGrid.FromRows(new[] { "G...g", "....." });
            var s = State(grid, 100, 100,
                Unit("raider", GridSide.Player, TroopKind.Cavalry, 3, 0, 100, 10), // 贴敌仓 (4,0)
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 0, 1, 100, 5));      // 远离
            s = GridBattleEngine.Advance(s, Cfg).State; // Dawn→Day（白昼可火攻）
            Assert.That(s.EnemyGranaryBurned, Is.True, "贴敌仓于白昼应焚仓");
            Assert.That(s.EnemySupply, Is.LessThanOrEqualTo(100 - Cfg.GranaryBurnLoss), "焚仓后敌补给应暴跌");
        }

        [Test]
        public void test_no_fire_without_unit_at_granary()
        {
            var grid = BattleGrid.FromRows(new[] { "G...g", "....." });
            var s = State(grid, 100, 100,
                Unit("p", GridSide.Player, TroopKind.Infantry, 0, 0, 100, 10),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 1, 1, 100, 5));
            s = GridBattleEngine.Advance(s, Cfg).State;
            Assert.That(s.EnemyGranaryBurned, Is.False, "无兵贴敌仓不应焚仓");
        }

        // AC-6 补给硬约束：告急→叛逃减员；告急方受伤 ×StarvingPct。
        [Test]
        public void test_low_supply_causes_desertion()
        {
            var s = State(OpenGrid(), 10, 30,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 1, 100, 10),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 8, 3, 100, 5));
            s = GridBattleEngine.Advance(s, Cfg).State;
            Assert.That(s.UnitAt(C(1, 1))!.Strength, Is.EqualTo(100 - Cfg.DesertPerSegment), "补给告急应逐段叛逃减员");
        }

        [Test]
        public void test_starving_side_takes_multiplied_casualties()
        {
            var grid = BattleGrid.FromRows(new[] { "G...g", "....." });
            // 告急方(玩家 supply=10)被敌近战：受伤应 ×StarvingPct。
            var starve = State(grid, 10, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 0, 500, 1),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 500, 20));
            int pAfterStarve = GridBattleEngine.Advance(starve, Cfg).State.UnitAt(C(1, 0))!.Strength;
            // 对照：补给正常(30)。
            var ok = State(grid, 30, 40,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 0, 500, 1),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 500, 20));
            int pAfterOk = GridBattleEngine.Advance(ok, Cfg).State.UnitAt(C(1, 0))!.Strength;
            int starveTaken = 500 - pAfterStarve, okTaken = 500 - pAfterOk;
            Assert.That(okTaken, Is.EqualTo(20), "对照：补给正常受伤=敌攻20，无倍率无叛逃");
            // 告急同段两效果并发：战斗受伤 ×3(=60) + 叛逃减员(=6) = 66。
            Assert.That(starveTaken, Is.EqualTo(20 * Cfg.StarvingDamagePct / 100 + Cfg.DesertPerSegment),
                "告急方：受伤×3(60) 叠加叛逃(6) = 66");
        }

        [Test]
        public void test_high_supply_recovers_strength()
        {
            var s = State(OpenGrid(), 100, 30,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 1, 90, 10), // 未满，可回血
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 8, 3, 100, 5));
            // MaxStrength=90（构造时 max=str），已满不涨——改用未满：手工造 max>str。
            var p = new GridUnit(U("p"), GridSide.Player, TroopKind.Infantry, C(1, 1), 80, 100, 10);
            var e = Unit("e", GridSide.Enemy, TroopKind.Infantry, 8, 3, 100, 5);
            var s2 = State(OpenGrid(), 100, 30, p, e);
            s2 = GridBattleEngine.Advance(s2, Cfg).State;
            Assert.That(s2.UnitAt(C(1, 1))!.Strength, Is.EqualTo(80 + Cfg.RecoverPerSegment), "补给充足应逐段回血");
        }

        // AC-7 半路遭遇：行军中新贴敌 → 段结果列出。
        [Test]
        public void test_enroute_encounter_is_surfaced()
        {
            var grid = BattleGrid.FromRows(new[] { "G....g", "......", "......" });
            var s = State(grid, 30, 30,
                Unit("cav", GridSide.Player, TroopKind.Cavalry, 0, 0, 100, 10, C(5, 0)),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 100, 5));
            var r = GridBattleEngine.Advance(s, Cfg);
            Assert.That(r.Encounters, Does.Contain(U("cav")), "行军中新贴敌应作为半路遭遇列出");
        }

        // AC-8 终局：敌全灭 → 玩家胜。
        [Test]
        public void test_enemy_eliminated_yields_player_victory()
        {
            var grid = BattleGrid.FromRows(new[] { "G...g", "....." });
            var s = State(grid, 30, 30,
                Unit("p", GridSide.Player, TroopKind.Infantry, 1, 0, 200, 30),
                Unit("e", GridSide.Enemy, TroopKind.Infantry, 2, 0, 5, 1)); // 弱敌
            var r = GridBattleEngine.Advance(s, Cfg);
            Assert.That(r.Outcome, Is.EqualTo(GridBattleOutcome.PlayerVictory));
            Assert.That(r.State.UnitAt(C(2, 0)), Is.Null, "溃灭者应被剔除");
        }

        // AC-9 命令校验：越界/山地/未知单位拒绝；合法接受。
        [Test]
        public void test_set_destination_validation()
        {
            var grid = BattleGrid.FromRows(new[] { "G.^.g", "....." });
            var s = State(grid, 30, 30, Unit("p", GridSide.Player, TroopKind.Infantry, 0, 0, 100, 10));
            Assert.That(GridBattleEngine.SetDestination(s, U("p"), C(2, 0)), Is.EqualTo(GridCommandResult.DestinationImpassable), "山地目的地应拒绝");
            Assert.That(GridBattleEngine.SetDestination(s, U("p"), C(9, 9)), Is.EqualTo(GridCommandResult.DestinationOutOfBounds), "越界目的地应拒绝");
            Assert.That(GridBattleEngine.SetDestination(s, U("ghost"), C(1, 0)), Is.EqualTo(GridCommandResult.UnitNotFound), "未知单位应拒绝");
            Assert.That(GridBattleEngine.SetDestination(s, U("p"), C(1, 1)), Is.EqualTo(GridCommandResult.Ok), "合法目的地应接受");
            Assert.That(s.UnitAt(C(0, 0))!.Destination, Is.EqualTo(C(1, 1)));
        }

        // AC-10 哈希 round-trip：克隆同哈希（存档一致性基座）。
        [Test]
        public void test_clone_preserves_hash()
        {
            var s = State(OpenGrid(), 70, 40,
                Unit("p", GridSide.Player, TroopKind.Archer, 1, 1, 80, 22, C(5, 1)),
                Unit("e", GridSide.Enemy, TroopKind.Cavalry, 7, 2, 90, 24));
            Assert.That(s.Clone().Hash(), Is.EqualTo(s.Hash()), "克隆态哈希须与原态一致");
        }
    }
}
