using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Supply;

namespace ThreeKingdom.Domain.Tests.Supply
{
    /// <summary>
    /// epic-004 story-002：三持有者补给守恒与路线断粮传导。
    /// 治理 ADR：ADR-0004（确定性整数/定点）。GDD_012 / TR-supply-001/002。
    /// 覆盖 AC-1 三持有者守恒无双计、AC-2 断粮经路线拓扑判定（非按钮）、
    /// AC-3 时段累积发事件、AC-4 不立即崩溃（渐进）、AC-5 单一权威（只发事件不改士气）。
    /// </summary>
    [TestFixture]
    public class SupplyConservationCutoffTests
    {
        private static readonly SupplySettlementService Service = new SupplySettlementService();
        private static readonly UnitId Unit = new UnitId("unit-vanguard");

        private static SupplyChainState State(
            long cityStock = 0, long convoyLoad = 0, long unitCarried = 0,
            long consumed = 0, long lost = 0, int shortageSegments = 0)
            => new SupplyChainState(cityStock, convoyLoad, unitCarried, consumed, lost, shortageSegments);

        private static SupplyConfig Config(int grace = 3, int lossNum = 1, int lossDen = 20)
            => new SupplyConfig(grace, FixedPoint.FromFraction(lossNum, lossDen));

        // ---- AC-1: 三持有者守恒，移交无双计 ----

        [Test]
        public void test_dispatch_convoy_moves_food_from_city_without_double_count()
        {
            // Arrange
            var state = State(cityStock: 1000, convoyLoad: 0);
            long totalBefore = state.GrandTotal;

            // Act：拨 400 入运输
            var after = Service.DispatchConvoy(state, 400);

            // Assert：城市 −400、在途 +400、同批不双计、总量守恒
            Assert.That(after.CityStock, Is.EqualTo(600));
            Assert.That(after.ConvoyLoad, Is.EqualTo(400));
            Assert.That(after.GrandTotal, Is.EqualTo(totalBefore));
        }

        [Test]
        public void test_dispatch_exceeding_city_stock_is_rejected_without_deduction()
        {
            var state = State(cityStock: 100);
            Assert.Throws<InvalidOperationException>(() => Service.DispatchConvoy(state, 200));
        }

        [Test]
        public void test_grand_total_conserved_across_dispatch_loss_and_consume()
        {
            // Arrange：城市/在途/携行三类齐备
            var state = State(cityStock: 1000, convoyLoad: 0, unitCarried: 100);
            var config = Config(grace: 3, lossNum: 1, lossDen: 20);
            long total = state.GrandTotal; // 1100

            // Act：拨运 → 在途损耗 → 单位消耗（路线通）
            var s1 = Service.DispatchConvoy(state, 400);
            var s2 = Service.ApplyTransitLoss(s1, config, FixedPoint.FromFraction(7, 5)); // env=1.4
            var seg = Service.SettleSegment(s2, Unit, demand: 150, routeDeliverable: true, config);

            // Assert：每步守恒总量恒定（消耗/损耗计入 sink，不凭空增减）
            Assert.That(s1.GrandTotal, Is.EqualTo(total));
            Assert.That(s2.GrandTotal, Is.EqualTo(total));
            Assert.That(seg.State.GrandTotal, Is.EqualTo(total));
        }

        [Test]
        public void test_transit_loss_accounts_lost_quantity_for_conservation()
        {
            // Arrange：在途 500，损耗率 0.05 × 环境 1.4 = 0.07 → 留存 465，损耗 35
            var state = State(convoyLoad: 500);
            var config = Config(lossNum: 1, lossDen: 20);

            // Act
            var after = Service.ApplyTransitLoss(state, config, FixedPoint.FromFraction(7, 5));

            // Assert
            Assert.That(after.ConvoyLoad, Is.EqualTo(465));
            Assert.That(after.Lost, Is.EqualTo(35));
            Assert.That(after.GrandTotal, Is.EqualTo(state.GrandTotal));
        }

        // ---- AC-2: 断粮经路线拓扑切断判定（非按钮）----

        [Test]
        public void test_supply_deliverable_only_when_no_route_in_chain_is_severed()
        {
            var chain = new[] { new RouteId("r1"), new RouteId("r2"), new RouteId("r3") };

            Assert.That(RouteSupplyLink.IsDeliverable(chain, Array.Empty<RouteId>()), Is.True);
            Assert.That(RouteSupplyLink.IsDeliverable(chain, new[] { new RouteId("r2") }), Is.False);
        }

        [Test]
        public void test_empty_supply_chain_is_rejected()
        {
            Assert.Throws<ArgumentException>(
                () => RouteSupplyLink.IsDeliverable(Array.Empty<RouteId>(), Array.Empty<RouteId>()));
        }

        // ---- AC-3 + AC-4: 时段累积、达宽限期发事件、不立即崩溃 ----

        [Test]
        public void test_cutoff_does_not_emit_event_before_grace_period()
        {
            // Arrange：路线切断、无携行、grace=3
            var state = State(convoyLoad: 500, unitCarried: 0);
            var config = Config(grace: 3);

            // Act：第 1、2 时段断粮
            var seg1 = Service.SettleSegment(state, Unit, demand: 100, routeDeliverable: false, config);
            var seg2 = Service.SettleSegment(seg1.State, Unit, demand: 100, routeDeliverable: false, config);

            // Assert：短缺累积但未达宽限期——紧张、无事件（不立即崩溃）
            Assert.That(seg1.Shortage, Is.EqualTo(100));
            Assert.That(seg1.State.ShortageSegments, Is.EqualTo(1));
            Assert.That(seg1.Status, Is.EqualTo(SupplyStatusLevel.Strained));
            Assert.That(seg1.CutoffEvent, Is.Null);
            Assert.That(seg2.State.ShortageSegments, Is.EqualTo(2));
            Assert.That(seg2.CutoffEvent, Is.Null);
        }

        [Test]
        public void test_cutoff_emits_event_when_shortage_reaches_grace_period()
        {
            // Arrange
            var state = State(convoyLoad: 500, unitCarried: 0);
            var config = Config(grace: 3);

            // Act：连续 3 时段断粮
            var s = state;
            SupplySegmentResult seg = null!;
            for (int i = 0; i < 3; i++)
            {
                seg = Service.SettleSegment(s, Unit, demand: 100, routeDeliverable: false, config);
                s = seg.State;
            }

            // Assert：达宽限期 → 断粮、发事件，事件携带累计时段与短缺
            Assert.That(seg.State.ShortageSegments, Is.EqualTo(3));
            Assert.That(seg.Status, Is.EqualTo(SupplyStatusLevel.Cutoff));
            Assert.That(seg.CutoffEvent, Is.Not.Null);
            Assert.That(seg.CutoffEvent!.ShortageSegments, Is.EqualTo(3));
            Assert.That(seg.CutoffEvent.Unit, Is.EqualTo(Unit));
        }

        [Test]
        public void test_resupply_resets_shortage_accumulation()
        {
            // Arrange：先断粮累计 2 时段，再恢复补给（路线通 + 在途有粮）
            var config = Config(grace: 3);
            var s = State(convoyLoad: 500, unitCarried: 0);
            s = Service.SettleSegment(s, Unit, 100, routeDeliverable: false, config).State;
            s = Service.SettleSegment(s, Unit, 100, routeDeliverable: false, config).State;
            Assert.That(s.ShortageSegments, Is.EqualTo(2));

            // Act：恢复——路线通，在途交付满足需求
            var recovered = Service.SettleSegment(s, Unit, 100, routeDeliverable: true, config);

            // Assert：短缺清零、充足、无事件
            Assert.That(recovered.Shortage, Is.EqualTo(0));
            Assert.That(recovered.State.ShortageSegments, Is.EqualTo(0));
            Assert.That(recovered.Status, Is.EqualTo(SupplyStatusLevel.Sufficient));
            Assert.That(recovered.CutoffEvent, Is.Null);
        }

        // ---- AC-5: 单一权威——只发事件，本系统不改士气/疲劳 ----

        [Test]
        public void test_cutoff_consequence_is_emitted_as_event_for_gdd011_to_consume()
        {
            // Arrange：grace=0，断粮即发事件（事件是唯一传导途径）
            var state = State(unitCarried: 0);
            var config = Config(grace: 0);

            // Act
            var seg = Service.SettleSegment(state, Unit, demand: 50, routeDeliverable: false, config);

            // Assert：后果以事件形式发出，供 GDD_011 消费；本结果不含任何士气/疲劳字段
            Assert.That(seg.CutoffEvent, Is.Not.Null);
            Assert.That(seg.CutoffEvent!.Shortage, Is.EqualTo(50));
            Assert.That(seg.State, Is.InstanceOf<SupplyChainState>(),
                "结算只产出补给链状态 + 事件；士气/疲劳由 GDD_011 唯一施加。");
        }

        // ---- 先携行后交付 + 确定性 ----

        [Test]
        public void test_carried_consumed_before_delivered_supply()
        {
            // Arrange：携行 60，需求 100，在途 500，路线通 → 先耗携行 60，再取在途 40
            var state = State(convoyLoad: 500, unitCarried: 60);
            var config = Config();

            // Act
            var seg = Service.SettleSegment(state, Unit, demand: 100, routeDeliverable: true, config);

            // Assert
            Assert.That(seg.Consumed, Is.EqualTo(100));
            Assert.That(seg.Shortage, Is.EqualTo(0));
            Assert.That(seg.State.UnitCarried, Is.EqualTo(0));      // 携行先耗尽
            Assert.That(seg.State.ConvoyLoad, Is.EqualTo(460));     // 在途仅取 40
        }

        [Test]
        public void test_segment_settlement_is_deterministic()
        {
            var state = State(cityStock: 200, convoyLoad: 300, unitCarried: 50);
            var config = Config();

            var a = Service.SettleSegment(state, Unit, 120, routeDeliverable: true, config);
            var b = Service.SettleSegment(state, Unit, 120, routeDeliverable: true, config);

            Assert.That(b.Consumed, Is.EqualTo(a.Consumed));
            Assert.That(b.Shortage, Is.EqualTo(a.Shortage));
            Assert.That(b.State.ConvoyLoad, Is.EqualTo(a.State.ConvoyLoad));
            Assert.That(b.State.GrandTotal, Is.EqualTo(a.State.GrandTotal));
        }

        // ---- 构造不变量 ----

        [Test]
        public void test_state_rejects_negative_holder()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => State(cityStock: -1));
        }
    }
}
