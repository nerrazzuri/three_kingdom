using System;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.World
{
    /// <summary>前置条件谓词类别（GDD_015 / ADR-0007：事件四元组之前置）。</summary>
    public enum PreconditionKind
    {
        /// <summary>某势力仍存续（历史正常前提，如"孙权未灭"）。</summary>
        FactionAlive = 0,

        /// <summary>某城归属某势力（如"荆州属刘表"）。</summary>
        CityOwnedBy = 1,
    }

    /// <summary>
    /// 历史事件前置条件（数据驱动、可确定性求值，ADR-0007 + ADR-0003）。不可变值。
    /// <see cref="Evaluate"/> 读 <see cref="WorldState"/> 判前置是否成立；reachability 判定用其"前置主体"
    /// （<see cref="SubjectFaction"/>/<see cref="SubjectCity"/>）确定玩家势力圈是否触及。
    /// </summary>
    public sealed class Precondition
    {
        /// <summary>谓词类别。</summary>
        public PreconditionKind Kind { get; }

        /// <summary>前置主体势力（FactionAlive 为该势力；CityOwnedBy 为期望归属势力）。</summary>
        public FactionId SubjectFaction { get; }

        /// <summary>前置主体城池（仅 CityOwnedBy 适用；FactionAlive 为 null）。</summary>
        public CityId? SubjectCity { get; }

        private Precondition(PreconditionKind kind, FactionId subjectFaction, CityId? subjectCity)
        {
            Kind = kind;
            SubjectFaction = subjectFaction;
            SubjectCity = subjectCity;
        }

        /// <summary>构造"某势力存续"前置。</summary>
        public static Precondition FactionAliveOf(FactionId faction)
            => new Precondition(PreconditionKind.FactionAlive, faction, null);

        /// <summary>构造"某城归属某势力"前置。</summary>
        public static Precondition CityOwnedByOf(CityId city, FactionId owner)
            => new Precondition(PreconditionKind.CityOwnedBy, owner, city);

        /// <summary>在给定世界态下求值：前置是否成立（确定性）。</summary>
        public bool Evaluate(WorldState world)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            switch (Kind)
            {
                case PreconditionKind.FactionAlive:
                    FactionRecord? f = world.FactionById(SubjectFaction);
                    return f != null && f.Survival == SurvivalStatus.Active;
                case PreconditionKind.CityOwnedBy:
                    CityOwnership? c = world.OwnershipOf(SubjectCity!.Value);
                    return c != null && c.Owner.HasValue && c.Owner.Value == SubjectFaction;
                default:
                    throw new InvalidOperationException($"未知前置类别：{Kind}。");
            }
        }
    }
}
