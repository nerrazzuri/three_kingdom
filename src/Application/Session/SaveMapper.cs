using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 会话 ↔ 存档快照映射（ADR-0005）。把 <see cref="GameSession"/> 的权威状态投影成版本化
    /// <see cref="SaveSnapshot"/> 的「世界真值段 / 阵营知识段」，及其逆向恢复。
    /// <b>分段隔离</b>：城市/时间/敌方真值入世界真值段；玩家已知敌情入知识段（不交叉，TR-intel-003）。
    /// 本 slice 未使用随机流（侦察未含暴露判定），故随机流段为空。
    /// </summary>
    internal static class SaveMapper
    {
        // 世界真值段键（权威 own/world 状态）。
        private const string KeyTime = "time";
        private const string KeyStock = "city.stock";
        private const string KeyReserved = "city.reserved";
        private const string KeyCivMorale = "city.civMorale";
        private const string KeySecurity = "city.security";
        private const string KeyFort = "city.fort";
        private const string KeyFortMax = "city.fortMax";
        private const string KeyLogistics = "logistics";
        private const string KeyLastShortage = "city.lastShortage";
        private const string KeyUnrest = "city.unrest";
        private const string KeyEnemyTruth = "enemy.truthStrength";
        private const string KeyDiploUsed = "diplo.used";
        private const string KeyDiploResponse = "diplo.response";
        private const string KeyDiploFulfilled = "diplo.fulfilled";
        private const string KeyDiploPendingIndex = "diplo.pendingIndex";
        private const string KeyDiploPendingAmount = "diplo.pendingAmount";
        private const string KeyDiploDelivered = "diplo.delivered";
        private const string KeyRaidLastDay = "raid.lastDay";
        private const string KeyRaidExposed = "raid.lastExposed";
        private const string RngDiplomacy = "diplomacy";
        private const string RngRaid = "raid";

        // 阵营知识段键（玩家已知敌情；不含真值）。
        private const string KeyKnownStrength = "enemy.knownStrength";
        private const string KeyKnownObservedAt = "enemy.knownObservedAt";
        private const string KeyKnownSource = "enemy.knownSource";

        /// <summary>把会话权威状态投影为存档快照。</summary>
        public static SaveSnapshot ToSnapshot(GameSession session, SaveVersion version, ConfigFingerprint fingerprint)
        {
            CityEconomyState city = session.City;

            var worldTruth = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                [KeyTime] = session.CurrentTime.AbsoluteIndex,
                [KeyStock] = city.Stock,
                [KeyReserved] = city.Reserved,
                [KeyCivMorale] = city.CivMorale,
                [KeySecurity] = city.Security,
                [KeyFort] = city.FortificationCurrent,
                [KeyFortMax] = city.FortificationMax,
                [KeyLogistics] = session.Logistics,
                [KeyLastShortage] = session.LastDayShortage,
                [KeyUnrest] = session.HighUnrestRisk ? 1 : 0,
                [KeyEnemyTruth] = session.EnemyTruthStrength,
                [KeyDiploUsed] = session.DiplomacyUsed ? 1 : 0,
                [KeyDiploResponse] = (long)session.DiplomacyResponse,
                [KeyDiploFulfilled] = session.DiplomacyFulfilledRoll ? 1 : 0,
                [KeyDiploPendingIndex] = session.PendingDeliveryIndex,
                [KeyDiploPendingAmount] = session.PendingDeliveryAmount,
                [KeyDiploDelivered] = session.DiplomacyDeliveredAmount,
                [KeyRaidLastDay] = session.LastRaidDay,
                [KeyRaidExposed] = session.LastRaidExposed ? 1 : 0,
            };

            var knowledge = new Dictionary<string, long>(StringComparer.Ordinal);
            if (session.IntelProjection.TryGet(session.Scenario.EnemySubject, out var entry))
            {
                knowledge[KeyKnownStrength] = entry.KnownStrength;
                knowledge[KeyKnownObservedAt] = entry.ObservedAt.AbsoluteIndex;
                knowledge[KeyKnownSource] = (long)entry.Source;
            }

            var rngStreams = new[] { session.CaptureDiplomacyRng(), session.CaptureRaidRng() };
            return new SaveSnapshot(version, fingerprint, rngStreams, worldTruth, knowledge);
        }

        /// <summary>把（已校验/迁移的）快照恢复为新会话。</summary>
        public static GameSession FromSnapshot(SaveSnapshot snapshot, SliceScenario scenario)
        {
            IReadOnlyDictionary<string, long> t = snapshot.WorldTruth;

            WorldTime time = TimeFromIndex(t[KeyTime]);
            var city = new CityEconomyState(
                scenario.InitialCity.Id,
                stock: t[KeyStock],
                reserved: t[KeyReserved],
                civMorale: checked((int)t[KeyCivMorale]),
                security: checked((int)t[KeySecurity]),
                fortificationCurrent: checked((int)t[KeyFort]),
                fortificationMax: checked((int)t[KeyFortMax]));

            IReadOnlyDictionary<string, long> k = snapshot.FactionKnowledge;
            bool hasKnown = k.ContainsKey(KeyKnownStrength);
            int knownStrength = hasKnown ? checked((int)k[KeyKnownStrength]) : 0;
            WorldTime knownObserved = hasKnown ? TimeFromIndex(k[KeyKnownObservedAt]) : time;
            IntelSource knownSource = hasKnown ? (IntelSource)checked((int)k[KeyKnownSource]) : IntelSource.Scouting;

            var session = new GameSession(
                scenario, time, city,
                logistics: t[KeyLogistics],
                lastDayShortage: t[KeyLastShortage],
                highUnrestRisk: t[KeyUnrest] != 0,
                enemyTruthStrength: checked((int)t[KeyEnemyTruth]),
                hasKnownEnemy: hasKnown,
                knownEnemyStrength: knownStrength,
                knownEnemySource: knownSource,
                knownEnemyObservedAt: knownObserved);

            // 恢复外交（含在途交付与随机流位置；读档据 (seed,position) 续判不重抽）。
            session.RestoreDiplomacy(
                used: t[KeyDiploUsed] != 0,
                response: (DiplomaticResponse)checked((int)t[KeyDiploResponse]),
                fulfilledRoll: t[KeyDiploFulfilled] != 0,
                pendingIndex: t[KeyDiploPendingIndex],
                pendingAmount: t[KeyDiploPendingAmount],
                deliveredAmount: t[KeyDiploDelivered],
                rng: FindRng(snapshot, RngDiplomacy, scenario.DiplomacyRngSeed));

            // 恢复袭扰（一日一袭日 + 随机流位置）。
            session.RestoreRaid(
                lastRaidDay: checked((int)t[KeyRaidLastDay]),
                lastExposed: t[KeyRaidExposed] != 0,
                rng: FindRng(snapshot, RngRaid, scenario.RaidRngSeed));

            return session;
        }

        /// <summary>从快照取命名随机流位置；缺失则回落到种子起点（确定性）。</summary>
        private static RngStreamState FindRng(SaveSnapshot snapshot, string name, ulong fallbackSeed)
        {
            foreach (var s in snapshot.RngStreams)
                if (s.Name == name) return s;
            return new RngStreamState(name, fallbackSeed, 0);
        }

        /// <summary>由绝对时间索引重建世界时间（T = Day×SegmentsPerDay + Segment 的逆）。</summary>
        private static WorldTime TimeFromIndex(long absoluteIndex)
        {
            int per = WorldTime.SegmentsPerDay;
            int day = checked((int)(absoluteIndex / per));
            int seg = (int)(absoluteIndex % per);
            return new WorldTime(day, (DaySegment)seg);
        }
    }
}
