using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// epic-012 story-004：城池归属只读投影（订阅 GDD_004 控制权变更）。
    /// 治理 ADR：ADR-0008（归属唯一权威 GDD_004 + 事件）+ ADR-0007（世界模型只读反映）。GDD_015 / TR-world-003。
    /// 覆盖 AC-1 订阅同步、AC-2 不独立写（编译级无写 API）、AC-3 并发裁定按序单点、AC-4 与权威最终一致。
    /// </summary>
    [TestFixture]
    public class WorldCityProjectionTests
    {
        private static readonly FactionId Cao = new FactionId("faction-cao");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly FactionId Third = new FactionId("faction-liu");
        private static readonly CharacterId CaoCao = new CharacterId("char-caocao");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

        private static (WorldCityProjection proj, CityControlAuthority authority) NewProjection()
        {
            var authority = new CityControlAuthority();
            authority.RegisterInitial(Fanshui, Cao, new Garrison(800));
            var world = new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[] { new FactionRecord(Cao, CaoCao, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }) },
                new[] { new CityOwnership(Fanshui, Cao, 800) },
                Array.Empty<string>(), Array.Empty<string>());
            return (new WorldCityProjection(world, authority), authority);
        }

        // ---- AC-1：订阅同步 ----

        [Test]
        public void test_projection_syncs_on_control_changed_event()
        {
            (WorldCityProjection proj, CityControlAuthority authority) = NewProjection();

            authority.RequestControlChange(Fanshui, Enemy, new Garrison(500), ChangeCause.SiegeConquest);

            CityOwnership? o = proj.Current.OwnershipOf(Fanshui);
            Assert.That(o!.Owner, Is.EqualTo(Enemy));
            Assert.That(o.Garrison, Is.EqualTo(500));
        }

        [Test]
        public void test_successive_changes_converge_to_last_value()
        {
            (WorldCityProjection proj, CityControlAuthority authority) = NewProjection();

            authority.RequestControlChange(Fanshui, Enemy, new Garrison(500), ChangeCause.SiegeConquest);
            authority.RequestControlChange(Fanshui, Third, new Garrison(300), ChangeCause.HistoricalDivergence);

            CityOwnership? o = proj.Current.OwnershipOf(Fanshui);
            Assert.That(o!.Owner, Is.EqualTo(Third)); // 最终一致：最后一次事件
            Assert.That(o.Garrison, Is.EqualTo(300));
        }

        // ---- AC-2：世界模型不独立写归属（编译级无写 API）----

        [Test]
        public void test_projection_exposes_no_public_ownership_writer()
        {
            // 归属唯一更新路径是订阅事件；投影不得暴露任何 public 写**归属**方法（AdvanceTime 是时间驱动，非归属写）。
            foreach (MethodInfo m in typeof(WorldCityProjection)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName))
            {
                string n = m.Name;
                bool ownershipWriter = n.IndexOf("Owner", StringComparison.Ordinal) >= 0
                    || n.StartsWith("Set", StringComparison.Ordinal)
                    || n.StartsWith("Write", StringComparison.Ordinal);
                Assert.That(ownershipWriter, Is.False, $"WorldCityProjection 不应暴露 public 写归属方法：{n}（归属只经订阅事件）。");
            }
        }

        [Test]
        public void test_owner_change_flows_through_authority_not_direct_write()
        {
            // 模拟历史结局 owner_change：经 GDD_004 RequestControlChange 发起，投影由事件回流更新。
            (WorldCityProjection proj, CityControlAuthority authority) = NewProjection();
            Assert.That(proj.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Cao));

            authority.RequestControlChange(Fanshui, Enemy, new Garrison(400), ChangeCause.HistoricalDivergence);

            Assert.That(proj.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy));
        }

        // ---- AC-3：并发裁定按 GDD_001 全局序由 004 单点结算 ----

        [Test]
        public void test_concurrent_contest_resolved_in_order_deterministically()
        {
            // 历史事件与玩家战役同争一城：按日界全局序（调用序）由 004 单点结算，结果确定。
            FactionId Run()
            {
                (WorldCityProjection proj, CityControlAuthority authority) = NewProjection();
                // 同一日界：先历史分叉夺城，后玩家战役夺回（全局序裁定）。
                authority.RequestControlChange(Fanshui, Enemy, new Garrison(500), ChangeCause.HistoricalDivergence);
                authority.RequestControlChange(Fanshui, Cao, new Garrison(600), ChangeCause.SiegeConquest);
                return proj.Current.OwnershipOf(Fanshui)!.Owner!.Value;
            }
            Assert.That(Run(), Is.EqualTo(Cao));     // 单一最终归属
            Assert.That(Run(), Is.EqualTo(Run()));   // 可复现
        }

        // ---- AC-4：与权威最终一致 ----

        [Test]
        public void test_projection_matches_authority_after_changes()
        {
            (WorldCityProjection proj, CityControlAuthority authority) = NewProjection();
            authority.RequestControlChange(Fanshui, Enemy, new Garrison(500), ChangeCause.SiegeConquest);

            Assert.That(proj.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(authority.OwnerOf(Fanshui)));
            Assert.That(proj.Current.OwnershipOf(Fanshui)!.Garrison, Is.EqualTo(authority.GarrisonOf(Fanshui)!.Value.Value));
        }
    }
}
