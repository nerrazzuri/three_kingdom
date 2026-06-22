using System;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Relationships;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>后果写回的目标系统种类（各权威系统独占写自身状态，ADR-0002）。</summary>
    public enum OutcomeTargetKind
    {
        /// <summary>城市经济状态（GDD_004 / TR-city-001）。</summary>
        City = 0,

        /// <summary>阵营名声（带符号，可正可负）。</summary>
        Reputation = 1,

        /// <summary>人物状态（此处建模为兵力/健康度等非负计量，TR-character-001）。</summary>
        Character = 2,

        /// <summary>方向性关系维度（GDD_006 / TR-relationship-001，按刻度 clamp）。</summary>
        Relationship = 3,
    }

    /// <summary>可写回的城市字段（各自有不变量）。</summary>
    public enum CityField
    {
        /// <summary>粮食库存（≥0）。</summary>
        Stock = 0,

        /// <summary>城市民心（≥0）。</summary>
        CivMorale = 1,

        /// <summary>治安（≥0）。</summary>
        Security = 2,

        /// <summary>工事当前值（0..FortificationMax）。</summary>
        Fortification = 3,
    }

    /// <summary>
    /// 单条跨系统变更意图（gdd-010 §后果 / systems-index「后果结算」契约）。
    /// 不可变的 <c>(target, field, delta, reason)</c> 意图——本身<b>不</b>直接修改任何权威状态；
    /// 由 <see cref="OutcomeWritebackService"/> 在全量校验通过后<b>统一原子</b>写回（ADR-0004）。
    /// <para>
    /// <see cref="ConservationKey"/>：可选守恒分组。共享同一守恒键的全部变更其 <see cref="Delta"/> 之和
    /// <b>必须为 0</b>（无凭空增减）——例如「城市拨出军粮 −X」与「后勤收到军粮 +X」同键互抵（TR-city-001 守恒）。
    /// </para>
    /// </summary>
    public sealed class OutcomeChange
    {
        /// <summary>目标系统种类。</summary>
        public OutcomeTargetKind Kind { get; }

        /// <summary>变化量（带符号；关系/名声可负，城市/人物计量校验后须满足非负不变量）。</summary>
        public long Delta { get; }

        /// <summary>变更原因（非空，供因果复盘 hud.md P5）。</summary>
        public string Reason { get; }

        /// <summary>守恒分组键（可空）。同键变更之和须为 0。</summary>
        public string? ConservationKey { get; }

        // —— 各 Kind 的目标键（仅相关字段有效）——
        /// <summary>城市目标 ID（Kind=City）。</summary>
        public CityId City { get; }

        /// <summary>城市字段（Kind=City）。</summary>
        public CityField Field { get; }

        /// <summary>阵营 ID（Kind=Reputation）。</summary>
        public FactionId Faction { get; }

        /// <summary>人物 ID（Kind=Character）。</summary>
        public CharacterId Character { get; }

        /// <summary>关系来源（Kind=Relationship）。</summary>
        public CharacterId RelFrom { get; }

        /// <summary>关系目标（Kind=Relationship）。</summary>
        public CharacterId RelTo { get; }

        /// <summary>关系维度（Kind=Relationship）。</summary>
        public RelationshipDimension RelDim { get; }

        private OutcomeChange(
            OutcomeTargetKind kind, long delta, string reason, string? conservationKey,
            CityId city, CityField field, FactionId faction, CharacterId character,
            CharacterId relFrom, CharacterId relTo, RelationshipDimension relDim)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("变更原因不可为空。", nameof(reason));
            if (conservationKey != null && conservationKey.Length == 0)
                throw new ArgumentException("守恒键若提供则不可为空字符串。", nameof(conservationKey));

            Kind = kind;
            Delta = delta;
            Reason = reason;
            ConservationKey = conservationKey;
            City = city;
            Field = field;
            Faction = faction;
            Character = character;
            RelFrom = relFrom;
            RelTo = relTo;
            RelDim = relDim;
        }

        /// <summary>城市字段变更（Kind=City）。</summary>
        public static OutcomeChange ForCity(CityId city, CityField field, long delta, string reason, string? conservationKey = null)
            => new OutcomeChange(OutcomeTargetKind.City, delta, reason, conservationKey,
                city, field, default, default, default, default, default);

        /// <summary>阵营名声变更（Kind=Reputation，带符号）。</summary>
        public static OutcomeChange ForReputation(FactionId faction, long delta, string reason, string? conservationKey = null)
            => new OutcomeChange(OutcomeTargetKind.Reputation, delta, reason, conservationKey,
                default, default, faction, default, default, default, default);

        /// <summary>人物计量变更（Kind=Character；写回后须 ≥0）。</summary>
        public static OutcomeChange ForCharacter(CharacterId character, long delta, string reason, string? conservationKey = null)
            => new OutcomeChange(OutcomeTargetKind.Character, delta, reason, conservationKey,
                default, default, default, character, default, default, default);

        /// <summary>方向性关系维度变更（Kind=Relationship；写回时 clamp 到刻度）。</summary>
        public static OutcomeChange ForRelationship(
            CharacterId from, CharacterId to, RelationshipDimension dim, long delta, string reason)
            => new OutcomeChange(OutcomeTargetKind.Relationship, delta, reason, null,
                default, default, default, default, from, to, dim);
    }
}
