using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 会话用例服务（ADR-0002 状态变更协议 2：Application 执行命令、推进 Domain、产出投影）。
    /// 这是 Presentation→Application 接缝的<b>执行端</b>：表现层调用本服务的用例方法（开局/推进时段/侦察/取投影），
    /// 服务编排 <see cref="GameSession"/> 聚合并返回<b>只读投影</b>。确定性——同一会话同一操作序列产生同一结果
    /// （ADR-0004）。无副作用泄露：表现层拿不到可变聚合，只拿投影 DTO。
    /// </summary>
    public sealed class SessionService
    {
        /// <summary>开新局：以 slice 默认场景构造会话（世界第 0 日黎明 + 己方城市 + 敌方真值）。</summary>
        public GameSession NewGame() => new GameSession(SliceScenario.Default());

        /// <summary>推进会话 <paramref name="segments"/> 个时段（默认 1），返回推进后的世界状态投影。</summary>
        public WorldStatusProjection Advance(GameSession session, int segments = 1)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            int daysCrossed = session.Advance(segments);
            return WorldStatus(session, daysCrossed);
        }

        /// <summary>取当前世界状态投影（不推进；DaysCrossedLastAdvance=0）。</summary>
        public WorldStatusProjection Project(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return WorldStatus(session, 0);
        }

        /// <summary>取己方城市账本投影（GDD_004）。</summary>
        public CityLedgerProjection ProjectCity(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            CityEconomyState c = session.City;
            return new CityLedgerProjection(
                c.Id.ToString(), c.Stock, c.Available, c.CivMorale, c.Security,
                c.FortificationCurrent, c.FortificationMax, session.LastDayShortage, session.HighUnrestRisk);
        }

        /// <summary>取敌方情报的只读投影（GDD_007；结构上不含真值）。</summary>
        public IntelProjection ProjectIntel(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return session.IntelProjection;
        }

        /// <summary>取一局目标/胜负投影（守城待变：守至援军 = 胜；民心崩溃 = 败）。</summary>
        public ObjectiveProjection ProjectObjective(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return new ObjectiveProjection(
                session.CurrentTime.Day, session.Scenario.ReliefDay, session.Outcome, session.DefeatReason);
        }

        /// <summary>求粮（受控外交入口，一局一次），返回更新后的外交投影。</summary>
        public DiplomacyProjection RequestAid(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            session.RequestAid();
            return ProjectDiplomacy(session);
        }

        /// <summary>召开军议（GDD_008）并返回建议集 + 当前知识快照（供过时判定）。</summary>
        public (CouncilAdviceSet? Set, KnowledgeSnapshotId Snapshot) Convene(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            session.Convene();
            return (session.LastAdvice, session.CurrentKnowledgeSnapshotId);
        }

        /// <summary>取最近军议建议集 + 当前知识快照（未召开则 Set 为 null）。</summary>
        public (CouncilAdviceSet? Set, KnowledgeSnapshotId Snapshot) ProjectCouncil(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return (session.LastAdvice, session.CurrentKnowledgeSnapshotId);
        }

        /// <summary>取人物花名册投影（GDD_005 §3 关键人物；静态场景数据）。</summary>
        public RosterProjection ProjectRoster(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var list = new List<CharacterProjection>();
            foreach (CharacterState c in session.Scenario.Roster)
                list.Add(new CharacterProjection(
                    c.Identity, c.Role.ToString(),
                    c.Capabilities.Level(CapabilityDomain.Command),
                    c.Capabilities.Level(CapabilityDomain.Valor),
                    c.Capabilities.Level(CapabilityDomain.Strategy),
                    c.Capabilities.Level(CapabilityDomain.Governance),
                    c.Capabilities.Level(CapabilityDomain.Diplomacy),
                    (int)c.Health.Level));
            return new RosterProjection(list);
        }

        /// <summary>取外交求粮投影（GDD_012 §8）。</summary>
        public DiplomacyProjection ProjectDiplomacy(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            long idx = session.PendingDeliveryIndex;
            int arrivalDay = idx >= 0 ? (int)(idx / WorldTime.SegmentsPerDay) : -1;
            return new DiplomacyProjection(
                session.DiplomacyUsed, session.DiplomacyResponse, session.DiplomacyFulfilledRoll,
                arrivalDay, session.PendingDeliveryAmount, session.DiplomacyDeliveredAmount);
        }

        /// <summary>侦察敌方并返回更新后的情报投影。</summary>
        public IntelProjection Scout(GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            session.Scout();
            return session.IntelProjection;
        }

        private static WorldStatusProjection WorldStatus(GameSession session, int daysCrossed)
        {
            WorldTime t = session.CurrentTime;
            return new WorldStatusProjection(t.Day, t.Segment, t.AbsoluteIndex, daysCrossed);
        }
    }
}
