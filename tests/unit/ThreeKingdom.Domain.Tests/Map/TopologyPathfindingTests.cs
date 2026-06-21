using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Tests.Map
{
    /// <summary>
    /// epic-002 story-004：区域/路线拓扑与确定性寻路。
    /// 治理 ADR：ADR-0004（确定性、平局稳定 ID 序、坐标不参与结算）。GDD_003 / TR-map-001/002。
    /// 覆盖 AC-1 拓扑图（无像素坐标）、AC-2 确定性寻路（平局按 RouteId）、AC-3 容量门控、AC-4 相向接触判定。
    /// </summary>
    [TestFixture]
    public class TopologyPathfindingTests
    {
        private static RegionId R(string s) => new RegionId(s);
        private static RouteId Rt(string s) => new RouteId(s);

        // A-B-D 与 A-C-D 双路径，各 5 耗时（等代价分叉）
        private static WorldMap DiamondMap()
        {
            var regions = new[]
            {
                new Region(R("A"), 10), new Region(R("B"), 10),
                new Region(R("C"), 10), new Region(R("D"), 10),
            };
            var routes = new[]
            {
                new Route(Rt("r1"), R("A"), R("B"), true, 2),
                new Route(Rt("r2"), R("B"), R("D"), true, 3),
                new Route(Rt("r3"), R("A"), R("C"), true, 2),
                new Route(Rt("r4"), R("C"), R("D"), true, 3),
            };
            return new WorldMap(regions, routes);
        }

        // ---- AC-1：拓扑图构造与校验 ----

        [Test]
        public void Map_builds_topology_without_coordinates()
        {
            var map = DiamondMap();
            Assert.That(map.RegionCount, Is.EqualTo(4));
            Assert.That(map.RouteCount, Is.EqualTo(4));
            Assert.That(map.RoutesFrom(R("A")).Select(r => r.Id.Value), Is.EqualTo(new[] { "r1", "r3" }));
        }

        [Test]
        public void Map_rejects_dangling_route()
        {
            var regions = new[] { new Region(R("A"), 1) };
            var routes = new[] { new Route(Rt("r1"), R("A"), R("ghost"), true, 1) };
            Assert.Throws<ArgumentException>(() => new WorldMap(regions, routes));
        }

        [Test]
        public void Map_rejects_duplicate_stable_ids()
        {
            var regions = new[] { new Region(R("A"), 1), new Region(R("A"), 2) };
            Assert.Throws<ArgumentException>(() => new WorldMap(regions, Array.Empty<Route>()));
        }

        [Test]
        public void Route_and_region_reject_illegal_values()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Region(R("A"), -1));        // 负容量
            Assert.Throws<ArgumentOutOfRangeException>(() => new Route(Rt("r"), R("A"), R("B"), true, 0)); // 负/零耗时
            Assert.Throws<ArgumentException>(() => new Route(Rt("r"), R("A"), R("A"), true, 1)); // 自环
        }

        // ---- AC-2：确定性寻路 ----

        [Test]
        public void FindPath_equal_cost_fork_breaks_tie_by_route_id()
        {
            var map = DiamondMap();
            var result = Pathfinder.FindPath(map, R("A"), R("D"));

            Assert.That(result.HasPath, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(5));
            // r1,r2 与 r3,r4 等代价 → 平局取字典序小者 [r1,r2]
            Assert.That(result.Routes.Select(r => r.Value), Is.EqualTo(new[] { "r1", "r2" }));
            Assert.That(result.Regions.Select(r => r.Value), Is.EqualTo(new[] { "A", "B", "D" }));
        }

        [Test]
        public void FindPath_is_deterministic_across_runs()
        {
            var map = DiamondMap();
            var a = Pathfinder.FindPath(map, R("A"), R("D")).Routes.Select(r => r.Value).ToArray();
            var b = Pathfinder.FindPath(map, R("A"), R("D")).Routes.Select(r => r.Value).ToArray();
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void FindPath_same_region_is_empty_zero_cost()
        {
            var map = DiamondMap();
            var result = Pathfinder.FindPath(map, R("A"), R("A"));
            Assert.That(result.HasPath, Is.True);
            Assert.That(result.Routes, Is.Empty);
            Assert.That(result.TotalCost, Is.EqualTo(0));
        }

        [Test]
        public void FindPath_disconnected_returns_no_path()
        {
            var regions = new[] { new Region(R("A"), 1), new Region(R("B"), 1), new Region(R("island"), 1) };
            var routes = new[] { new Route(Rt("r1"), R("A"), R("B"), true, 1) };
            var map = new WorldMap(regions, routes);

            var result = Pathfinder.FindPath(map, R("A"), R("island"));
            Assert.That(result.HasPath, Is.False);
            Assert.That(result.Routes, Is.Empty);
        }

        [Test]
        public void FindPath_custom_cost_can_change_chosen_path()
        {
            var map = DiamondMap();
            // 自定义代价让经 C 的路线更便宜 → 选 r3,r4
            int Cost(Route r) => (r.Id == Rt("r3") || r.Id == Rt("r4")) ? 1 : 10;
            var result = Pathfinder.FindPath(map, R("A"), R("D"), Cost);

            Assert.That(result.Routes.Select(r => r.Value), Is.EqualTo(new[] { "r3", "r4" }));
            Assert.That(result.TotalCost, Is.EqualTo(2));
        }

        [Test]
        public void FindPath_respects_route_direction()
        {
            var regions = new[] { new Region(R("A"), 1), new Region(R("B"), 1) };
            var routes = new[] { new Route(Rt("r1"), R("A"), R("B"), false, 1) }; // 仅 A→B
            var map = new WorldMap(regions, routes);

            Assert.That(Pathfinder.FindPath(map, R("A"), R("B")).HasPath, Is.True);
            Assert.That(Pathfinder.FindPath(map, R("B"), R("A")).HasPath, Is.False); // 单向，反向不可达
        }

        // ---- AC-3：容量门控 ----

        [Test]
        public void Region_capacity_gate_blocks_overflow_allows_exact_fill()
        {
            var region = new Region(R("camp"), 5);
            Assert.That(region.CanAccept(4, 1), Is.True);   // 恰好满容
            Assert.That(region.CanAccept(4, 2), Is.False);  // 超容
            Assert.That(region.CanAccept(0, 5), Is.True);
            Assert.Throws<ArgumentOutOfRangeException>(() => region.CanAccept(-1, 1));
        }

        // ---- AC-4：相向移动接触判定 ----

        [Test]
        public void Route_contact_when_bidirectional_and_progress_sum_reaches_one()
        {
            var route = new Route(Rt("r1"), R("A"), R("B"), true, 3);
            Assert.That(RouteContact.Occurs(route, FixedPoint.FromFraction(6, 10), FixedPoint.FromFraction(5, 10)), Is.True);  // 1.1 ≥ 1
            Assert.That(RouteContact.Occurs(route, FixedPoint.FromFraction(1, 2), FixedPoint.FromFraction(1, 2)), Is.True);    // 恰好 1.0
            Assert.That(RouteContact.Occurs(route, FixedPoint.FromFraction(4, 10), FixedPoint.FromFraction(5, 10)), Is.False); // 0.9 < 1
        }

        [Test]
        public void Route_contact_never_on_one_way_route()
        {
            var oneWay = new Route(Rt("r1"), R("A"), R("B"), false, 3);
            Assert.That(RouteContact.Occurs(oneWay, FixedPoint.One, FixedPoint.One), Is.False);
        }

        [Test]
        public void Route_contact_rejects_progress_out_of_range()
        {
            var route = new Route(Rt("r1"), R("A"), R("B"), true, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => RouteContact.Occurs(route, FixedPoint.FromInt(2), FixedPoint.Zero));
        }

        // ---- 路线通行耗时 ----

        [Test]
        public void RouteCost_applies_ceil_and_minimum_one()
        {
            // base=3, unit=1.0, weather=1.3, load=1.2 → ceil(≈4.68)=5
            int cost = RouteCost.Compute(3, FixedPoint.One, FixedPoint.FromFraction(13, 10), FixedPoint.FromFraction(6, 5));
            Assert.That(cost, Is.EqualTo(5));
        }

        [Test]
        public void RouteCost_rejects_non_positive_inputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RouteCost.Compute(0, FixedPoint.One, FixedPoint.One, FixedPoint.One));
            Assert.Throws<ArgumentOutOfRangeException>(() => RouteCost.Compute(1, FixedPoint.Zero, FixedPoint.One, FixedPoint.One));
        }
    }
}
