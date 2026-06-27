using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 势力记录（GDD_015 §Data Model：FactionRecord / TR-world-001）。不可变。
    /// 持有势力 id、君主、存续状态、领有城池、对玩家关系。构造时校验不变量，失败即抛、无部分写入。
    /// <para>
    /// <b>领有城池</b>为势力侧反映（与 <see cref="CityOwnership"/> 的城侧只读投影一致），按 <see cref="CityId"/>
    /// 序数升序规范化以保证哈希字节序稳定（ADR-0004）。
    /// </para>
    /// </summary>
    public sealed class FactionRecord
    {
        private readonly CityId[] _ownedCities; // 已按序数升序、去重

        /// <summary>势力稳定 ID。</summary>
        public FactionId Id { get; }

        /// <summary>君主人物 ID（存续势力非空；已灭势力可为 null）。</summary>
        public CharacterId? Lord { get; }

        /// <summary>存续状态。</summary>
        public SurvivalStatus Survival { get; }

        /// <summary>对玩家战略关系。</summary>
        public RelationToPlayer Relation { get; }

        /// <summary>领有城池（按 ID 序数升序，稳定遍历）。</summary>
        public IReadOnlyList<CityId> OwnedCities => _ownedCities;

        /// <summary>构造势力记录。</summary>
        public FactionRecord(
            FactionId id,
            CharacterId? lord,
            SurvivalStatus survival,
            RelationToPlayer relation,
            IReadOnlyList<CityId> ownedCities)
        {
            if (ownedCities is null) throw new ArgumentNullException(nameof(ownedCities));
            if (!Enum.IsDefined(typeof(SurvivalStatus), survival))
                throw new ArgumentOutOfRangeException(nameof(survival), "未定义的存续状态。");
            if (!Enum.IsDefined(typeof(RelationToPlayer), relation))
                throw new ArgumentOutOfRangeException(nameof(relation), "未定义的对玩家关系。");
            if (survival == SurvivalStatus.Active && lord == null)
                throw new ArgumentException("存续势力须有君主。", nameof(lord));

            var sorted = new List<CityId>(ownedCities);
            sorted.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == sorted[i - 1])
                    throw new ArgumentException($"领有城池重复：{sorted[i]}。", nameof(ownedCities));
            }

            Id = id;
            Lord = lord;
            Survival = survival;
            Relation = relation;
            _ownedCities = sorted.ToArray();
        }

        /// <summary>
        /// 以规范顺序追加到状态哈希（ADR-0004）。顺序：id → lord(有无标志+长度+字符)
        /// → (int)存续 → (int)关系 → 城池数 → 各城(长度+字符)。
        /// </summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            AppendString(hasher, Id.Value);
            hasher.Append(Lord.HasValue);
            AppendString(hasher, Lord.HasValue ? Lord.Value.Value : string.Empty);
            hasher.Append((int)Survival);
            hasher.Append((int)Relation);
            hasher.Append(_ownedCities.Length);
            foreach (CityId c in _ownedCities) AppendString(hasher, c.Value);
        }

        private static void AppendString(StateHasher hasher, string value)
        {
            hasher.Append(value.Length);
            foreach (char ch in value) hasher.Append((int)ch);
        }
    }
}
