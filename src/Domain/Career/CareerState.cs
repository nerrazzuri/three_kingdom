using System;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>
    /// 太守生涯权威状态（GDD_014 §Data Model：CareerState / TR-career-001 / ADR-0002 + ADR-0004）。
    /// 不可变聚合：功绩、名望、君主好感、官阶、所属势力、在野标志；构造时校验全部不变量，
    /// 失败即抛、无部分写入。状态变更经 <see cref="CareerStateService"/>（命令路径）产出新实例，<b>不</b>就地修改。
    /// <para>
    /// <b>权威路径禁 float</b>（ADR-0004）：merit/renown 为权威整数，lord_standing 为 Q16.16 定点 <see cref="FixedPoint"/>。
    /// 纳入状态哈希（<see cref="AppendTo"/>），为确定性结算（TR-career-001）与存档 round-trip（story-005）奠基。
    /// </para>
    /// <para>
    /// <b>在野语义</b>：<see cref="IsUnaffiliated"/> 为真时 <see cref="Faction"/> 为 null（无所属君主），
    /// 且 <see cref="LordStanding"/> 恒为 0（无君主则无好感）。两者互为充要，构造时强制一致。
    /// </para>
    /// </summary>
    public sealed class CareerState
    {
        /// <summary>累计功绩（权威整数，≥0，单调累积里程碑闸——GDD_014 N10）。</summary>
        public int Merit { get; }

        /// <summary>名望（权威整数，≥0，单调累积）。</summary>
        public int Renown { get; }

        /// <summary>君主好感（Q16.16 定点，∈[0,1]）。在野时恒为 0。</summary>
        public FixedPoint LordStanding { get; }

        /// <summary>当前官阶（GDD_014 官阶梯队序数）。</summary>
        public Rank Rank { get; }

        /// <summary>所属势力（在野时为 null）。</summary>
        public FactionId? Faction { get; }

        /// <summary>在野标志（无所属君主）。与 <see cref="Faction"/>==null 互为充要。</summary>
        public bool IsUnaffiliated { get; }

        /// <summary>定点 0（lord_standing 下界）。</summary>
        private static readonly FixedPoint StandingMin = FixedPoint.Zero;

        /// <summary>定点 1（lord_standing 上界）。</summary>
        private static readonly FixedPoint StandingMax = FixedPoint.One;

        /// <summary>
        /// 构造生涯状态并校验不变量。任一不变量违反即抛，无部分写入。
        /// </summary>
        /// <param name="merit">累计功绩，≥0。</param>
        /// <param name="renown">名望，≥0。</param>
        /// <param name="lordStanding">君主好感，∈[0,1]；在野时须为 0。</param>
        /// <param name="rank">当前官阶。</param>
        /// <param name="faction">所属势力；在野时须为 null。</param>
        /// <param name="isUnaffiliated">在野标志；须与 <paramref name="faction"/>==null 一致。</param>
        public CareerState(
            int merit,
            int renown,
            FixedPoint lordStanding,
            Rank rank,
            FactionId? faction,
            bool isUnaffiliated)
        {
            if (merit < 0) throw new ArgumentOutOfRangeException(nameof(merit), "功绩不可为负。");
            if (renown < 0) throw new ArgumentOutOfRangeException(nameof(renown), "名望不可为负。");
            if (lordStanding < StandingMin || lordStanding > StandingMax)
                throw new ArgumentOutOfRangeException(nameof(lordStanding), "君主好感须在 [0,1]。");
            if (!Enum.IsDefined(typeof(Rank), rank))
                throw new ArgumentOutOfRangeException(nameof(rank), "未定义的官阶。");
            if (isUnaffiliated != (faction == null))
                throw new ArgumentException("在野标志须与所属势力一致：在野⇔无所属势力。", nameof(isUnaffiliated));
            if (isUnaffiliated && lordStanding != StandingMin)
                throw new ArgumentException("在野（无君主）时君主好感须为 0。", nameof(lordStanding));

            Merit = merit;
            Renown = renown;
            LordStanding = lordStanding;
            Rank = rank;
            Faction = faction;
            IsUnaffiliated = isUnaffiliated;
        }

        /// <summary>
        /// 创建太守开局生涯状态（rank=城池太守，merit/renown=0，依附指定势力）。
        /// </summary>
        /// <param name="faction">所属君主势力。</param>
        /// <param name="initialLordStanding">初始君主好感（默认 0）。</param>
        public static CareerState NewGovernor(FactionId faction, FixedPoint initialLordStanding)
            => new CareerState(0, 0, initialLordStanding, Rank.CityGovernor, faction, isUnaffiliated: false);

        /// <summary>
        /// 按字段差量产出新实例（仅供命令服务使用；保持不可变与单一写路径）。
        /// 仅覆盖本骨架会变更的数值字段（merit/renown/lord_standing/rank）；势力归属与在野转换
        /// 属自立线（story-003），届时以专门的状态转换方法表达，不经此重载（避免 null 哨兵歧义）。
        /// </summary>
        internal CareerState With(
            int? merit = null,
            int? renown = null,
            FixedPoint? lordStanding = null,
            Rank? rank = null)
            => new CareerState(
                merit ?? Merit,
                renown ?? Renown,
                lordStanding ?? LordStanding,
                rank ?? Rank,
                Faction,
                IsUnaffiliated);

        /// <summary>
        /// 自立成立：脱离原君主、建立/归属自有势力，成为该势力之主（story-003）。
        /// 不再有上级君主，故 lord_standing 归零；merit/renown/rank 保留（结局后玩法简化，MVP）。
        /// </summary>
        internal CareerState IntoOwnFaction(FactionId newFaction)
            => new CareerState(Merit, Renown, FixedPoint.Zero, Rank, newFaction, isUnaffiliated: false);

        /// <summary>
        /// 沦为流浪势力（自立众叛亲离 / 君主已灭转在野，story-003/004）：无所属、无君主好感，
        /// 合法可继续状态（非卡死）。merit/renown/rank 保留。
        /// </summary>
        internal CareerState IntoWandering()
            => new CareerState(Merit, Renown, FixedPoint.Zero, Rank, faction: null, isUnaffiliated: true);

        /// <summary>
        /// 以规范顺序追加权威字段到状态哈希（ADR-0004）。顺序固定：
        /// merit → renown → lord_standing.Raw → (int)rank → isUnaffiliated → faction（长度+序数字符，在野为长度 0）。
        /// </summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append(Merit);
            hasher.Append(Renown);
            hasher.Append(LordStanding);
            hasher.Append((int)Rank);
            hasher.Append(IsUnaffiliated);
            string factionValue = Faction.HasValue ? Faction.Value.Value : string.Empty;
            hasher.Append(factionValue.Length);
            foreach (char c in factionValue) hasher.Append((int)c);
        }

        /// <summary>计算本状态的确定性哈希（便捷封装；多聚合组合哈希用 <see cref="CareerSnapshot"/>）。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            AppendTo(hasher);
            return hasher.ToHash();
        }
    }
}
