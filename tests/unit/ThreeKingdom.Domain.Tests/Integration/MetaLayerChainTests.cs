using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Application.Career;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.Integration
{
    /// <summary>
    /// Meta 层跨系统链端到端测试（full-game-review ADV-9）。
    /// 验证此前只被分片测试的链路作为<b>一个完整流程</b>跑通：
    /// 守城败 → 生涯转在野 → 失城经 GDD_004 ControlChanged → GDD_015 世界归属投影同步更新。
    /// 这是把 epic-011（生涯/守城后果）与 epic-012（归属投影）接成一条链的集成证明——
    /// 不依赖完整 GameSession 装配（装配属 review BLK-1，另案），但覆盖三系统经事件协同的确定性契约（ADR-0008）。
    /// </summary>
    [TestFixture]
    public class MetaLayerChainTests
    {
        private static readonly FactionId Player = new FactionId("faction-player");
        private static readonly FactionId Enemy = new FactionId("faction-yuan");
        private static readonly CharacterId Lord = new CharacterId("char-player-lord");
        private static readonly CharacterId Aide = new CharacterId("char-aide");
        private static readonly CityId Fanshui = new CityId("city-fanshui");

        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        private static CitySeed Seed()
            => new CitySeed(Player, Fanshui, garrison: 800, fortification: 60, output: 20,
                new[] { new RetinueMember(Aide, Frac(6, 10)) });

        private static WorldState InitialWorld()
            => new WorldState(
                new WorldTime(0, DaySegment.Dawn),
                new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan-lord"), SurvivalStatus.Active, RelationToPlayer.Hostile, Array.Empty<CityId>()),
                },
                new[] { new CityOwnership(Fanshui, Player, 800) },
                Array.Empty<string>(), Array.Empty<string>());

        // ---- 端到端链：守城败 → 在野 + 失城经 004 → 世界投影同步 ----

        [Test]
        public void test_siege_loss_chains_career_wandering_and_world_ownership_via_gdd004()
        {
            // Arrange：单一权威 + 生涯编排 + 世界投影都挂到同一 CityControlAuthority。
            var authority = new CityControlAuthority();
            var campaign = new GovernorCampaignService(authority);          // epic-011：生涯/守城后果
            CareerSnapshot career = campaign.BeginGovernorStart(Seed());     // 登记 Fanshui 归属玩家 + 生涯绑定
            var projection = new WorldCityProjection(InitialWorld(), authority); // epic-012：世界归属投影，订阅 004

            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Player), "开局：城属玩家。");
            Assert.That(projection.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player), "开局：世界投影=玩家。");
            Assert.That(career.Career.IsUnaffiliated, Is.False);

            // Act：守城败 → 经 GovernorCampaignService 一次结算驱动整链。
            var ctx = new SiegeContext(Fanshui, Enemy, new Garrison(500));
            SiegeResolutionResult result = campaign.ResolveSiege(
                career, SiegeOutcome.Fallen, new GovernorStartConfig(30, Frac(1, 10)), ctx);

            // Assert：三系统经同一 ControlChanged 事件协同更新。
            // ① 生涯（epic-011）：罢官转在野，保留部曲。
            Assert.That(result.Snapshot.Career.IsUnaffiliated, Is.True);
            Assert.That(result.Snapshot.Career.Faction, Is.Null);
            Assert.That(result.Snapshot.Retinue.IsMember(Aide), Is.True);
            // ② 控制权权威（GDD_004）：单点写 + 发事件，归属转敌方。
            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Enemy));
            Assert.That(result.ControlChange!.Cause, Is.EqualTo(ChangeCause.SiegeDefenseLost));
            // ③ 世界模型（epic-012）：投影由订阅事件回流，同步到敌方——证明链路贯通。
            Assert.That(projection.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Enemy));
            Assert.That(projection.Current.OwnershipOf(Fanshui)!.Garrison, Is.EqualTo(500));
            // ④ 世界投影与权威最终一致（无双写、无脱节）。
            Assert.That(projection.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(authority.OwnerOf(Fanshui)));
        }

        [Test]
        public void test_siege_win_keeps_ownership_consistent_across_chain()
        {
            var authority = new CityControlAuthority();
            var campaign = new GovernorCampaignService(authority);
            CareerSnapshot career = campaign.BeginGovernorStart(Seed());
            var projection = new WorldCityProjection(InitialWorld(), authority);

            var ctx = new SiegeContext(Fanshui, Enemy, new Garrison(500));
            SiegeResolutionResult result = campaign.ResolveSiege(
                career, SiegeOutcome.Defended, new GovernorStartConfig(30, Frac(1, 10)), ctx);

            // 守城胜：生涯加功绩/信任、归属不变，三系统仍一致。
            Assert.That(result.Snapshot.Career.IsUnaffiliated, Is.False);
            Assert.That(result.Snapshot.Career.Merit, Is.EqualTo(30));
            Assert.That(result.ControlChange, Is.Null);
            Assert.That(authority.OwnerOf(Fanshui), Is.EqualTo(Player));
            Assert.That(projection.Current.OwnershipOf(Fanshui)!.Owner, Is.EqualTo(Player));
        }

        [Test]
        public void test_chain_is_deterministic()
        {
            StateHash Run()
            {
                var authority = new CityControlAuthority();
                var campaign = new GovernorCampaignService(authority);
                CareerSnapshot career = campaign.BeginGovernorStart(Seed());
                var projection = new WorldCityProjection(InitialWorld(), authority);
                var ctx = new SiegeContext(Fanshui, Enemy, new Garrison(500));
                SiegeResolutionResult r = campaign.ResolveSiege(
                    career, SiegeOutcome.Fallen, new GovernorStartConfig(30, Frac(1, 10)), ctx);

                var hasher = new StateHasher();
                r.Snapshot.AppendTo(hasher);
                projection.Current.AppendTo(hasher);
                return hasher.ToHash();
            }
            Assert.That(Run(), Is.EqualTo(Run()));
        }
    }
}
