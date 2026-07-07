using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.ZoneBattle;

namespace ThreeKingdom.Application.Battle
{
    /// <summary>
    /// 出征六维准备 → 区域部署桥（GDD_021 R2 / ADR-0012 D7）：把 <see cref="OffensivePreparation"/>（GDD_019 六维）
    /// 转为攻方支队并按<b>布势路线</b>分派到战场区域——单选布势路线升级为多区域部署。守方由守备生成分区布防。
    /// 时机/天气/侦察 → <see cref="ZoneBattleContext"/>（反全知门）。纯装配（不拥规则）。
    /// </summary>
    public sealed class OffensiveDeploymentPlanner
    {
        /// <summary>由六维准备 + 派生士气构造攻方分区支队（布势路线决定分派）。</summary>
        public IReadOnlyList<Detachment> PlanAttacker(OffensivePreparation prep, FixedPoint morale, BattleField field)
        {
            if (prep == null) throw new ArgumentNullException(nameof(prep));
            int total = prep.MusteredTroops;
            int cav = prep.Composition.Count(TroopType.Cavalry);
            int inf = Math.Max(0, total - cav);
            OffensiveGeneral lead = prep.Command.Lead;
            FixedPoint fatigue = FixedPoint.FromFraction(2, 10);

            var dets = new List<Detachment>();
            switch (prep.Approach)
            {
                case ApproachPlan.FeintLure:
                    if (cav > 0)
                        dets.Add(Make("atk-flank", BattleSide.Attacker, lead, Cavalry(cav), cav, morale, fatigue, Posture.Feint, BattleField.Flank));
                    if (inf > 0)
                        dets.Add(Make("atk-front", BattleSide.Attacker, cav > 0 ? null : lead, Infantry(inf), inf, morale, fatigue, Posture.Assault, BattleField.Front));
                    break;

                case ApproachPlan.ProtractedSiege:
                    int siege = total / 2, siegeFront = total - siege;
                    if (siege > 0) dets.Add(Make("atk-supply", BattleSide.Attacker, lead, Infantry(siege), siege, morale, fatigue, Posture.Hold, BattleField.Supply));
                    if (siegeFront > 0) dets.Add(Make("atk-front", BattleSide.Attacker, null, Infantry(siegeFront), siegeFront, morale, fatigue, Posture.Assault, BattleField.Front));
                    break;

                case ApproachPlan.NightRaid:
                    int raid = total / 2, raidFront = total - raid;
                    if (raid > 0) dets.Add(Make("atk-cover", BattleSide.Attacker, lead, Infantry(raid), raid, morale, fatigue, Posture.Assault, BattleField.Cover));
                    if (raidFront > 0) dets.Add(Make("atk-front", BattleSide.Attacker, null, Infantry(raidFront), raidFront, morale, fatigue, Posture.Assault, BattleField.Front));
                    break;

                case ApproachPlan.FrontalAssault:
                default:
                    dets.Add(new Detachment(new DetachmentId("atk-front"), BattleSide.Attacker, lead, prep.Composition,
                        Math.Max(total, prep.Composition.Total), morale, fatigue, Posture.Assault, BattleField.Front));
                    break;
            }
            if (dets.Count == 0)   // 极端：无兵，仍放一支空前军以成军（会速溃）。
                dets.Add(Make("atk-front", BattleSide.Attacker, lead, TroopComposition.None, 0, morale, fatigue, Posture.Assault, BattleField.Front));
            return dets;
        }

        /// <summary>由守备（守军 + 工事）构造守方分区布防：主力守正面，一部护粮道。</summary>
        public IReadOnlyList<Detachment> PlanDefender(SiegeDefense defense, FixedPoint morale, BattleField field)
            => PlanDefender(defense, morale, field, SubversionEffect.None, null);

        /// <summary>同上 + 人心杠杆施计效果（无守将版，向后兼容）。</summary>
        public IReadOnlyList<Detachment> PlanDefender(SiegeDefense defense, FixedPoint morale, BattleField field, SubversionEffect subversion)
            => PlanDefender(defense, morale, field, subversion, null);

        /// <summary>
        /// 守方分区布防（GDD_027 #3：<b>守将进战斗</b>）：主力守正面、一部护粮道，并按守城将<b>标签择位</b>——
        /// 善守/铁骨镇正面（压杀伤右偏），诡谋/远图护粮道（反伏击/识破敌预设）。守将携标签/战阵档/谋略档入结算，
        /// 使"打曹仁守的樊城"≠"打无名守军"。<paramref name="defenders"/> 为该城守将（空/null=无名守军，退回原纯守军行为）。
        /// 先应用人心杠杆施计效果（GDD_024 F3 接缝）：有效守军=守军×(1−倒戈比)；守方开战士气 += 士气增量 + 军纪增量。
        /// </summary>
        public IReadOnlyList<Detachment> PlanDefender(
            SiegeDefense defense, FixedPoint morale, BattleField field, SubversionEffect subversion,
            IReadOnlyList<OffensiveGeneral>? defenders)
        {
            if (defense == null) throw new ArgumentNullException(nameof(defense));
            subversion = subversion ?? SubversionEffect.None;

            int garrison = defense.Garrison;
            if (subversion.GarrisonDefectRatio > FixedPoint.Zero)
                garrison = (FixedPoint.FromInt(garrison) * (FixedPoint.One - subversion.GarrisonDefectRatio)).RoundToInt();
            garrison = Math.Max(0, garrison);
            morale = (morale + subversion.DefenderMoraleDelta + subversion.DefenderDisciplineDelta)
                .Clamp(FixedPoint.Zero, FixedPoint.One);

            OffensiveGeneral? front = PickDefenderFront(defenders);
            OffensiveGeneral? supply = PickDefenderSupply(defenders, front);

            int frontG = garrison - garrison / 3;
            int supplyG = garrison - frontG;
            FixedPoint fatigue = FixedPoint.FromFraction(1, 10);

            var dets = new List<Detachment>();
            if (frontG > 0) dets.Add(Make("def-front", BattleSide.Defender, front, Infantry(frontG), frontG, morale, fatigue, Posture.Hold, BattleField.Front));
            if (supplyG > 0) dets.Add(Make("def-supply", BattleSide.Defender, supply, Infantry(supplyG), supplyG, morale, fatigue, Posture.Hold, BattleField.Supply));
            if (dets.Count == 0) dets.Add(Make("def-front", BattleSide.Defender, front, TroopComposition.None, 0, morale, fatigue, Posture.Hold, BattleField.Front));
            return dets;
        }

        /// <summary>择守正面之将：善守/铁骨优先（镇守压阵），否则战阵档最高（猛将当关）。</summary>
        private static OffensiveGeneral? PickDefenderFront(IReadOnlyList<OffensiveGeneral>? gs)
        {
            if (gs == null) return null;
            OffensiveGeneral? best = null; int bestKey = int.MinValue;
            foreach (OffensiveGeneral g in gs)
            {
                int key = (g.HasTag(GeneralTag.Defender) || g.HasTag(GeneralTag.IronBones)) ? 1000 : 0;
                key += g.Prowess.HasValue ? (int)g.Prowess.Value : 0;
                if (key > bestKey) { bestKey = key; best = g; }
            }
            return best;
        }

        /// <summary>择护粮道之将：诡谋/远图优先（反伏击·识破），否则谋略档最高；避开已镇正面者。</summary>
        private static OffensiveGeneral? PickDefenderSupply(IReadOnlyList<OffensiveGeneral>? gs, OffensiveGeneral? exclude)
        {
            if (gs == null) return null;
            OffensiveGeneral? best = null; int bestKey = int.MinValue;
            foreach (OffensiveGeneral g in gs)
            {
                if (exclude != null && g.Character.Equals(exclude.Character)) continue;
                int key = (g.HasTag(GeneralTag.Cunning) || g.HasTag(GeneralTag.Strategist)) ? 1000 : 0;
                key += g.Strategy.HasValue ? (int)g.Strategy.Value : 0;
                if (key > bestKey) { bestKey = key; best = g; }
            }
            return best;
        }

        /// <summary>六维时机/侦察 → 战斗上下文（反全知门）。晴天=干燥→火攻天时门（DryField）。</summary>
        public ZoneBattleContext ContextFrom(OffensivePreparation prep)
            => new ZoneBattleContext(prep.Timing.IsNight, prep.Timing.IsFoggy, prep.Scouted,
                isDry: prep.Timing.Weather == WeatherType.Clear);

        private static Detachment Make(
            string id, BattleSide side, OffensiveGeneral? general, TroopComposition comp, int strength,
            FixedPoint morale, FixedPoint fatigue, Posture posture, ZoneId at)
            => new Detachment(new DetachmentId(id), side, general, comp, strength, morale, fatigue, posture, at);

        private static TroopComposition Cavalry(int n) => new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = n });
        private static TroopComposition Infantry(int n) => TroopComposition.AllInfantry(n);
    }
}
