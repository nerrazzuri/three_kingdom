using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>区域战斗哈希小工具（字符串规范追加：长度 + 各 char，与既有聚合同款）。</summary>
    internal static class ZoneHashing
    {
        internal static void AppendString(StateHasher hasher, string value)
        {
            hasher.Append(value?.Length ?? 0);
            if (value == null) return;
            foreach (char ch in value) hasher.Append((int)ch);
        }
    }

    /// <summary>
    /// 战场区域（GDD_021 R1，<b>非坐标</b>）：命名空间单位，载地形 + 条件禀赋（此区可涌现哪些兵法条件）+ 软容量。
    /// 邻接关系存于 <see cref="BattleField"/>（区域本身只是描述子）。不可变。<b>无 position/grid/facing 字段</b>（ADR-0012 D2）。
    /// </summary>
    public sealed class Zone
    {
        /// <summary>区域稳定 id。</summary>
        public ZoneId Id { get; }

        /// <summary>地形（伏兵/隐蔽等条件门用）。</summary>
        public TerrainKind Terrain { get; }

        /// <summary>条件禀赋：此区<b>可能</b>涌现的兵法条件（实际成型仍须门齐，S3）。规范序（枚举序）。</summary>
        public IReadOnlyList<TacticCondition> Affordances { get; }

        /// <summary>软容量（部署超此不禁止，但拥挤有代价——S3/平衡）。</summary>
        public int SoftCapacity { get; }

        public Zone(ZoneId id, TerrainKind terrain, IReadOnlyList<TacticCondition>? affordances, int softCapacity)
        {
            if (id.Value is null) throw new ArgumentException("Zone.Id 不可为空。", nameof(id));
            if (softCapacity < 0) throw new ArgumentOutOfRangeException(nameof(softCapacity), "软容量不可为负。");
            Id = id;
            Terrain = terrain;
            SoftCapacity = softCapacity;

            // 规范化条件禀赋：去重 + 按枚举序稳定（确定性哈希）。
            var seen = new SortedSet<int>();
            if (affordances != null) foreach (TacticCondition c in affordances) seen.Add((int)c);
            var list = new List<TacticCondition>();
            foreach (int v in seen) list.Add((TacticCondition)v);
            Affordances = list;
        }

        /// <summary>此区是否禀赋某条件（可涌现）。</summary>
        public bool Affords(TacticCondition condition)
        {
            foreach (TacticCondition c in Affordances) if (c == condition) return true;
            return false;
        }

        internal void AppendTo(StateHasher hasher)
        {
            ZoneHashing.AppendString(hasher, Id.Value);
            hasher.Append((int)Terrain);
            hasher.Append(SoftCapacity);
            hasher.Append(Affordances.Count);
            foreach (TacticCondition c in Affordances) hasher.Append((int)c);
        }
    }
}
