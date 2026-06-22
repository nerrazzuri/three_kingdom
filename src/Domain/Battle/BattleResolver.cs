using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 确定性战役解析器（GDD_010 §Formula 1/2 / TR-battle-001/003 / ADR-0004）。
    /// 按稳定管线（验证→移动→侦测→交战→损耗→士气→触发→发布）逐步推进；命令流按稳定序号
    /// 排序。同初始快照 + 配置指纹 + 种子 + 有序命令流 → 同事件与状态哈希。
    /// 在<b>工作副本</b>上推进，任一步异常即丢弃副本、原子回滚到原快照（无半结算态）。
    /// 全程定点/整数，无 float、无隐式随机。
    /// </summary>
    public sealed class BattleResolver
    {
        /// <summary>解析一个战役阶段。</summary>
        public BattleResolution ResolvePhase(BattleSnapshot snapshot, IReadOnlyList<BattleOrder> orders, ulong seed, BattleConfig config)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var sorted = new List<BattleOrder>(orders);
            sorted.Sort((a, b) => a.Sequence != b.Sequence ? a.Sequence.CompareTo(b.Sequence) : a.Actor.CompareTo(b.Actor));

            try
            {
                var units = new Dictionary<BattleUnitId, BattleUnitState>();
                foreach (BattleUnitState u in snapshot.Units) units[u.Id] = u;
                DetectionState detection = snapshot.Detection.Clone();
                var events = new List<BattleEvent>();

                // 管线按 BattlePhasePipeline.CanonicalOrder 顺序推进（每步纯函数式更新工作副本）。
                StepValidate(sorted, units);
                StepMove(sorted, units);
                StepDetect(sorted, units, detection);
                StepEngageAndCasualty(units, detection, config, events);
                // 士气步：本系统只读 GDD_011 已结算值，不改（GDD_010 §8）。触发/撤退步：S2 条件链。
                AssignSequences(events);

                var newSnapshot = new BattleSnapshot(units.Values, detection, snapshot.ConfigFingerprint);
                StateHash hash = HashState(newSnapshot, seed, sorted);
                return BattleResolution.Commit(newSnapshot, events, hash);
            }
            catch (Exception ex)
            {
                // 原子回滚：丢弃工作副本，返回未改动的原快照。
                StateHash origHash = HashState(snapshot, seed, sorted);
                return BattleResolution.Rollback(snapshot, origHash, ex.Message);
            }
        }

        private static void StepValidate(List<BattleOrder> orders, Dictionary<BattleUnitId, BattleUnitState> units)
        {
            foreach (BattleOrder o in orders)
            {
                if (!units.ContainsKey(o.Actor)) throw new InvalidOperationException($"命令执行者 {o.Actor} 不存在。");
                if (o.Type == BattleOrderType.Move && o.TargetRegion == null) throw new InvalidOperationException($"移动命令缺目标区域。");
                if ((o.Type == BattleOrderType.Scout || o.Type == BattleOrderType.Engage) && o.TargetUnit == null)
                    throw new InvalidOperationException($"侦察/接战命令缺目标单位。");
            }
        }

        private static void StepMove(List<BattleOrder> orders, Dictionary<BattleUnitId, BattleUnitState> units)
        {
            foreach (BattleOrder o in orders)
                if (o.Type == BattleOrderType.Move)
                    units[o.Actor] = units[o.Actor].WithRegion(o.TargetRegion!.Value);
        }

        private static void StepDetect(List<BattleOrder> orders, Dictionary<BattleUnitId, BattleUnitState> units, DetectionState detection)
        {
            foreach (BattleOrder o in orders)
                if (o.Type == BattleOrderType.Scout)
                {
                    BattleUnitId target = o.TargetUnit!.Value;
                    if (!units.ContainsKey(target)) throw new KeyNotFoundException($"侦察目标 {target} 不存在。");
                    detection.Set(units[o.Actor].Faction, target, Awareness.Confirmed);
                }
        }

        private static void StepEngageAndCasualty(
            Dictionary<BattleUnitId, BattleUnitState> units, DetectionState detection, BattleConfig config, List<BattleEvent> events)
        {
            // 按区域聚合，稳定 ID 序两两配对敌对单位（确定性接触规则，非点击先后）。
            var byRegion = new SortedDictionary<RegionId, List<BattleUnitState>>(Comparer<RegionId>.Default);
            foreach (BattleUnitState u in units.Values)
            {
                if (!byRegion.TryGetValue(u.Region, out List<BattleUnitState>? list)) byRegion[u.Region] = list = new List<BattleUnitState>();
                list.Add(u);
            }

            var casualties = new Dictionary<BattleUnitId, int>();
            foreach (KeyValuePair<RegionId, List<BattleUnitState>> kv in byRegion)
            {
                List<BattleUnitState> list = kv.Value;
                list.Sort((a, b) => a.Id.CompareTo(b.Id));
                for (int i = 0; i < list.Count; i++)
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        BattleUnitState a = list[i], b = list[j];
                        if (a.Faction == b.Faction) continue;

                        FixedPoint cpA = CombatMath.CombatPower(a), cpB = CombatMath.CombatPower(b);
                        int casB = CombatMath.Casualty(cpA, cpB, b.Force, config.CasualtyCurve,
                            CombatMath.AmbushBonus(CombatMath.IsSurprise(detection, a, b), config));
                        int casA = CombatMath.Casualty(cpB, cpA, a.Force, config.CasualtyCurve,
                            CombatMath.AmbushBonus(CombatMath.IsSurprise(detection, b, a), config));
                        Accumulate(casualties, a.Id, casA);
                        Accumulate(casualties, b.Id, casB);
                        events.Add(new BattleEvent(0, BattleEventType.Engagement, a.Id, $"交战 {a.Id} vs {b.Id} @ {kv.Key}"));
                    }
            }

            // 一次性应用伤亡（自交战前兵力计算，顺序无关）。
            var hurt = new List<KeyValuePair<BattleUnitId, int>>(casualties);
            hurt.Sort((x, y) => x.Key.CompareTo(y.Key));
            foreach (KeyValuePair<BattleUnitId, int> c in hurt)
                if (c.Value > 0)
                {
                    BattleUnitState u = units[c.Key];
                    units[c.Key] = u.WithForce(Math.Max(0, u.Force - c.Value));
                    events.Add(new BattleEvent(0, BattleEventType.Casualty, c.Key, $"{c.Key} 伤亡 {c.Value}"));
                }
        }

        private static void Accumulate(Dictionary<BattleUnitId, int> map, BattleUnitId id, int delta)
            => map[id] = (map.TryGetValue(id, out int cur) ? cur : 0) + delta;

        /// <summary>发布步：按 (类型, 单位) 稳定序赋事件序号（确定性事件序）。</summary>
        private static void AssignSequences(List<BattleEvent> events)
        {
            events.Sort((a, b) => a.Type != b.Type ? a.Type.CompareTo(b.Type) : a.Unit.CompareTo(b.Unit));
            for (int i = 0; i < events.Count; i++)
            {
                BattleEvent e = events[i];
                events[i] = new BattleEvent(i, e.Type, e.Unit, e.Detail);
            }
        }

        /// <summary>确定性状态哈希（§Formula 1）：指纹 ‖ 种子 ‖ 有序命令 ‖ 结果单位态。</summary>
        private static StateHash HashState(BattleSnapshot snapshot, ulong seed, List<BattleOrder> orderedOrders)
        {
            var h = new StateHasher();
            AppendString(h, snapshot.ConfigFingerprint);
            h.Append(seed);
            foreach (BattleOrder o in orderedOrders)
            {
                h.Append(o.Sequence);
                AppendString(h, o.Actor.Value);
                h.Append((int)o.Type);
            }

            var sortedUnits = new List<BattleUnitState>(snapshot.Units);
            sortedUnits.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (BattleUnitState u in sortedUnits)
            {
                AppendString(h, u.Id.Value);
                AppendString(h, u.Region.Value);
                h.Append(u.Force);
                h.Append(u.Morale).Append(u.Fatigue).Append(u.Discipline);
            }
            return h.ToHash();
        }

        private static void AppendString(StateHasher h, string s)
        {
            foreach (char c in s) h.Append((int)c);
            h.Append(s.Length);
        }
    }
}
