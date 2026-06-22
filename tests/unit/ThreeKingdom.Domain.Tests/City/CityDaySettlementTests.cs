using System;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.City
{
    /// <summary>
    /// epic-004 story-001：城市日界产耗结算与资源守恒。
    /// 治理 ADR：ADR-0004（确定性整数/定点）+ ADR-0003（数据驱动配置）。GDD_004 / TR-city-001/002。
    /// 覆盖 AC-1 同源库存 + 移交无双计、AC-2 日界稳定顺序、AC-3 下限夹取不出负、AC-4 守恒恒等。
    /// </summary>
    [TestFixture]
    public class CityDaySettlementTests
    {
        private static readonly CityId City = new CityId("city-fanshui");

        private static CityEconomyState State(
            long stock, long reserved = 0, int civMorale = 60, int security = 50,
            int fortCur = 80, int fortMax = 100)
            => new CityEconomyState(City, stock, reserved, civMorale, security, fortCur, fortMax);

        private static CitySettlementConfig Config(
            long baseYield = 20, long baseCivConsume = 30, long baseMaintenance = 5,
            long stockFloor = 0, int civMoraleMax = 100, int shortagePenaltyNum = 1, int shortagePenaltyDen = 2,
            long unrestThreshold = 10, int fortRepairRate = 15)
            => new CitySettlementConfig(
                baseYield, baseCivConsume, baseMaintenance, stockFloor, civMoraleMax,
                FixedPoint.FromFraction(shortagePenaltyNum, shortagePenaltyDen), unrestThreshold, fortRepairRate);

        private static readonly CityDaySettlementService Service = new CityDaySettlementService();

        // ---- AC-2: 日界按稳定顺序结算 ----

        [Test]
        public void test_city_settlement_ledger_follows_canonical_stage_order()
        {
            // Arrange
            var state = State(stock: 100, reserved: 20);
            var config = Config();

            // Act
            var result = Service.Settle(state, logisticsHolding: 0, config, FixedPoint.One);

            // Assert: 账本阶段序列恰为 承诺→产入→消耗→短缺后果→工事/治安
            var stages = result.Ledger.Select(e => e.Stage).ToArray();
            Assert.That(stages, Is.EqualTo(CityDaySettlementStages.CanonicalOrder.ToArray()));
        }

        [Test]
        public void test_city_settlement_stage_order_is_deterministic_across_runs()
        {
            // Arrange
            var state = State(stock: 70, reserved: 10);
            var config = Config();

            // Act：同输入两次结算
            var a = Service.Settle(state, 5, config, FixedPoint.FromFraction(3, 2));
            var b = Service.Settle(state, 5, config, FixedPoint.FromFraction(3, 2));

            // Assert：确定性——库存/消耗/短缺/民心/后勤完全一致
            Assert.That(b.EndState.Stock, Is.EqualTo(a.EndState.Stock));
            Assert.That(b.Consumed, Is.EqualTo(a.Consumed));
            Assert.That(b.Shortage, Is.EqualTo(a.Shortage));
            Assert.That(b.EndState.CivMorale, Is.EqualTo(a.EndState.CivMorale));
            Assert.That(b.EndLogisticsHolding, Is.EqualTo(a.EndLogisticsHolding));
        }

        // ---- AC-1: 同一权威库存 + 移交后勤不双计 ----

        [Test]
        public void test_requisition_transfer_moves_food_to_logistics_without_double_count()
        {
            // Arrange：库存 100，已承诺军粮 40，后勤已持 10；无产入无消耗以隔离移交
            var state = State(stock: 100, reserved: 40);
            var config = Config(baseYield: 0, baseCivConsume: 0, baseMaintenance: 0);
            long totalFoodBefore = state.Stock + 10;

            // Act
            var result = Service.Settle(state, logisticsHolding: 10, config, FixedPoint.Zero);

            // Assert：移交量单一转移——城市扣 40、后勤增 40、总量守恒、城市不再保留
            Assert.That(result.Transferred, Is.EqualTo(40));
            Assert.That(result.EndState.Stock, Is.EqualTo(60));
            Assert.That(result.EndLogisticsHolding, Is.EqualTo(50));
            Assert.That(result.EndState.Reserved, Is.EqualTo(0));
            long totalFoodAfter = result.EndState.Stock + result.EndLogisticsHolding;
            Assert.That(totalFoodAfter, Is.EqualTo(totalFoodBefore), "移交不得双计：城市+后勤总量守恒。");
        }

        // ---- AC-4: 守恒恒等 产入 − 消耗 − 转移 = 库存差 ----

        [Test]
        public void test_conservation_identity_holds_with_yield_consume_and_transfer()
        {
            // Arrange：同时含产入/消耗/转移
            var state = State(stock: 50, reserved: 15);
            var config = Config(baseYield: 20, baseCivConsume: 30, baseMaintenance: 5, stockFloor: 0);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert：恒等式成立
            Assert.That(result.ConservationHolds, Is.True);
            Assert.That(result.Yield - result.Consumed - result.Transferred,
                Is.EqualTo(result.EndState.Stock - result.StartStock));
        }

        [Test]
        public void test_conservation_holds_when_floor_clamp_creates_shortage()
        {
            // Arrange：库存触下限——消耗被夹取，差额转短缺，恒等仍以实际扣减计
            var state = State(stock: 10);
            var config = Config(baseYield: 0, baseCivConsume: 20, baseMaintenance: 20, stockFloor: 5);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert：实际消耗=5（10→5），短缺=15，恒等成立
            Assert.That(result.Consumed, Is.EqualTo(5));
            Assert.That(result.Shortage, Is.EqualTo(15));
            Assert.That(result.ConservationHolds, Is.True);
        }

        [Test]
        public void test_conservation_holds_with_zero_yield_and_zero_demand()
        {
            // Arrange：零产入、零需求边界
            var state = State(stock: 80, reserved: 0);
            var config = Config(baseYield: 0, baseCivConsume: 0, baseMaintenance: 0);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.Zero);

            // Assert：库存不变，恒等成立
            Assert.That(result.EndState.Stock, Is.EqualTo(80));
            Assert.That(result.ConservationHolds, Is.True);
        }

        // ---- AC-3: 资源不低于合法下限，不出负 ----

        [Test]
        public void test_consumption_clamps_at_stock_floor_and_records_shortage()
        {
            // Arrange：需求远超可消耗，库存须停在下限
            var state = State(stock: 30);
            var config = Config(baseYield: 0, baseCivConsume: 100, baseMaintenance: 100, stockFloor: 8);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert：库存=下限 8，不出负；短缺=需求−实际消耗
            Assert.That(result.EndState.Stock, Is.EqualTo(8));
            Assert.That(result.EndState.Stock, Is.GreaterThanOrEqualTo(config.StockFloor));
            Assert.That(result.Consumed, Is.EqualTo(22));
            Assert.That(result.Shortage, Is.EqualTo(78));
        }

        [Test]
        public void test_stock_never_goes_negative_even_when_already_at_floor()
        {
            // Arrange：库存已等于下限，再有需求也不出负、不补齐
            var state = State(stock: 5);
            var config = Config(baseYield: 0, baseCivConsume: 50, baseMaintenance: 50, stockFloor: 5);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert：消耗 0，库存维持下限，恒等成立
            Assert.That(result.Consumed, Is.EqualTo(0));
            Assert.That(result.EndState.Stock, Is.EqualTo(5));
            Assert.That(result.ConservationHolds, Is.True);
        }

        // ---- 短缺后果 / 工事（顺序末阶段的可观测结果）----

        [Test]
        public void test_shortage_reduces_morale_and_flags_high_unrest_over_threshold()
        {
            // Arrange：短缺 15 > 阈值 10；k_shortage=0.5 → 民心 −8（round(7.5)）
            var state = State(stock: 10, civMorale: 60);
            var config = Config(baseYield: 0, baseCivConsume: 25, baseMaintenance: 25, stockFloor: 0,
                shortagePenaltyNum: 1, shortagePenaltyDen: 2, unrestThreshold: 10);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert
            Assert.That(result.Shortage, Is.EqualTo(15));
            Assert.That(result.EndState.CivMorale, Is.EqualTo(52));
            Assert.That(result.HighUnrestRisk, Is.True);
        }

        [Test]
        public void test_fortification_repair_bounded_by_max_in_final_stage()
        {
            // Arrange：工事 80/100，修复速率 15 → 仅修到 95（不越上限），且为末阶段
            var state = State(stock: 50, fortCur: 80, fortMax: 100);
            var config = Config(fortRepairRate: 30);

            // Act
            var result = Service.Settle(state, 0, config, FixedPoint.One);

            // Assert：修到上限 100（min(20, 30)=20），不越界
            Assert.That(result.EndState.FortificationCurrent, Is.EqualTo(100));
            var lastEntry = result.Ledger[result.Ledger.Count - 1];
            Assert.That(lastEntry.Stage, Is.EqualTo(CityDaySettlementStage.FortificationSecurity));
            Assert.That(lastEntry.FortDelta, Is.EqualTo(20));
        }

        // ---- 构造不变量（无部分写入）----

        [Test]
        public void test_state_rejects_reserved_exceeding_stock()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CityEconomyState(City, stock: 10, reserved: 20, civMorale: 50, security: 50,
                    fortificationCurrent: 0, fortificationMax: 0));
        }

        [Test]
        public void test_config_rejects_negative_yield()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Config(baseYield: -1));
        }

        [Test]
        public void test_settle_rejects_negative_population_pressure()
        {
            var state = State(stock: 50);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Service.Settle(state, 0, Config(), FixedPoint.FromInt(-1)));
        }
    }
}
