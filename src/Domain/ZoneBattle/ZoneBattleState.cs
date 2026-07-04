using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 区域战斗权威态（GDD_021 / ADR-0012 D1）：战场 + 全部支队（含所在区）+ 各区交战累积态 + 回合钟 + 玩家阵营 + 种子。
    /// 不可变；回合推进/命令产出<b>新</b>实例（纯函数变换，S3/S4）。确定性哈希（ADR-0004）：
    /// 支队按 (Side, Id) 规范序、区域按 id 规范序遍历。
    /// </summary>
    public sealed class ZoneBattleState
    {
        private readonly List<Detachment> _detachments;                      // 规范序 (Side, Id)
        private readonly Dictionary<string, Detachment> _byId;
        private readonly Dictionary<string, ZoneEngagementState> _engagements; // key=zone id

        /// <summary>战场（区域图）。</summary>
        public BattleField Field { get; }
        /// <summary>全部支队（规范序 (Side, Id)）。</summary>
        public IReadOnlyList<Detachment> Detachments => _detachments;
        /// <summary>回合钟。</summary>
        public BattleClock Clock { get; }
        /// <summary>玩家控制的阵营（另一方由敌AI驱动，攻守统一）。</summary>
        public BattleSide PlayerSide { get; }
        /// <summary>敌AI确定性随机种子基（ADR-0006/0013）。</summary>
        public ulong Seed { get; }
        /// <summary>敌AI渐进记忆（ADR-0013 D6；入态/存档/哈希）。</summary>
        public EnemyAiMemory Memory { get; }

        public ZoneBattleState(
            BattleField field, IReadOnlyList<Detachment> detachments,
            IReadOnlyList<ZoneEngagementState> engagements, BattleClock clock, BattleSide playerSide, ulong seed,
            EnemyAiMemory? memory = null)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            PlayerSide = playerSide;
            Seed = seed;
            Memory = memory ?? EnemyAiMemory.Empty;

            _byId = new Dictionary<string, Detachment>(StringComparer.Ordinal);
            _detachments = new List<Detachment>(detachments ?? Array.Empty<Detachment>());
            foreach (Detachment d in _detachments)
            {
                if (!field.Contains(d.Location))
                    throw new ArgumentException($"支队 {d.Id} 所在区 {d.Location} 不在战场。", nameof(detachments));
                if (d.TransitTarget != null && !field.Contains(d.TransitTarget.Value))
                    throw new ArgumentException($"支队 {d.Id} 调动目标 {d.TransitTarget} 不在战场。", nameof(detachments));
                if (_byId.ContainsKey(d.Id.Value)) throw new ArgumentException($"支队 id 重复：{d.Id.Value}", nameof(detachments));
                _byId[d.Id.Value] = d;
            }
            _detachments.Sort(CompareDetachments);

            // 交战态须覆盖所有区（缺者补空态）；忽略非战场区。
            _engagements = new Dictionary<string, ZoneEngagementState>(StringComparer.Ordinal);
            if (engagements != null)
                foreach (ZoneEngagementState e in engagements)
                    if (field.Contains(e.Zone)) _engagements[e.Zone.Value] = e;
            foreach (Zone z in field.Zones)
                if (!_engagements.ContainsKey(z.Id.Value)) _engagements[z.Id.Value] = ZoneEngagementState.Empty(z.Id);
        }

        private static int CompareDetachments(Detachment a, Detachment b)
        {
            int s = ((int)a.Side).CompareTo((int)b.Side);
            return s != 0 ? s : string.CompareOrdinal(a.Id.Value, b.Id.Value);
        }

        /// <summary>某阵营的支队（规范序）。</summary>
        public IReadOnlyList<Detachment> DetachmentsOf(BattleSide side)
        {
            var list = new List<Detachment>();
            foreach (Detachment d in _detachments) if (d.Side == side) list.Add(d);
            return list;
        }

        /// <summary>某区的支队（含在途——结算方按 InTransit 自行排除）。</summary>
        public IReadOnlyList<Detachment> DetachmentsIn(ZoneId zone)
        {
            var list = new List<Detachment>();
            foreach (Detachment d in _detachments) if (d.Location == zone) list.Add(d);
            return list;
        }

        /// <summary>取某支队（不存在返回 null）。</summary>
        public Detachment? TryGet(DetachmentId id)
            => _byId.TryGetValue(id.Value ?? string.Empty, out Detachment? d) ? d : null;

        /// <summary>某区交战态。</summary>
        public ZoneEngagementState EngagementOf(ZoneId zone)
            => _engagements.TryGetValue(zone.Value ?? string.Empty, out ZoneEngagementState? e) ? e : ZoneEngagementState.Empty(zone);

        /// <summary>某阵营<b>未溃散</b>支队的总兵力（溃散=兵尽或士气崩，不计入）。</summary>
        public long TotalStrength(BattleSide side)
        {
            long sum = 0;
            foreach (Detachment d in _detachments) if (d.Side == side && !d.IsBroken) sum += d.Strength;
            return sum;
        }

        /// <summary>某阵营是否已无战力（全部溃散）。</summary>
        public bool IsRouted(BattleSide side) => TotalStrength(side) <= 0;

        // ---- 不可变更新（回合/命令产出新态；S3/S4 用）----

        /// <summary>以新支队集产出新态（战场/交战/钟/阵营/种子/记忆不变）。</summary>
        public ZoneBattleState WithDetachments(IReadOnlyList<Detachment> detachments)
            => new ZoneBattleState(Field, detachments, EngagementList(), Clock, PlayerSide, Seed, Memory);

        /// <summary>以新交战态集产出新态。</summary>
        public ZoneBattleState WithEngagements(IReadOnlyList<ZoneEngagementState> engagements)
            => new ZoneBattleState(Field, _detachments, engagements, Clock, PlayerSide, Seed, Memory);

        /// <summary>以新支队集 + 新交战态 + 新钟产出新态（回合结算一次性写回）。</summary>
        public ZoneBattleState With(IReadOnlyList<Detachment> detachments, IReadOnlyList<ZoneEngagementState> engagements, BattleClock clock)
            => new ZoneBattleState(Field, detachments, engagements, clock, PlayerSide, Seed, Memory);

        /// <summary>以新敌AI记忆产出新态。</summary>
        public ZoneBattleState WithMemory(EnemyAiMemory memory)
            => new ZoneBattleState(Field, _detachments, EngagementList(), Clock, PlayerSide, Seed, memory);

        private IReadOnlyList<ZoneEngagementState> EngagementList()
        {
            var list = new List<ZoneEngagementState>();
            foreach (Zone z in Field.Zones) list.Add(_engagements[z.Id.Value]);
            return list;
        }

        /// <summary>确定性状态哈希（ADR-0004；支队 (Side,Id) 序、区域 id 序）。</summary>
        public StateHash Hash()
        {
            var hasher = new StateHasher();
            Field.AppendTo(hasher);
            hasher.Append(_detachments.Count);
            foreach (Detachment d in _detachments) d.AppendTo(hasher);
            foreach (Zone z in Field.Zones) _engagements[z.Id.Value].AppendTo(hasher);   // 区域规范序
            Clock.AppendTo(hasher);
            hasher.Append((int)PlayerSide);
            hasher.Append(Seed);
            Memory.AppendTo(hasher);
            return hasher.ToHash();
        }
    }
}
