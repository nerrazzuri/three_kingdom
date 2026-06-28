using System;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.City
{
    /// <summary>城池守备值（ADR-0008 控制权契约：CityControlChanged 携带 garrison）。非负。</summary>
    public readonly struct Garrison : IEquatable<Garrison>
    {
        /// <summary>守备数（≥0）。</summary>
        public int Value { get; }

        public Garrison(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "守备不可为负。");
            Value = value;
        }

        public bool Equals(Garrison other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is Garrison other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    /// <summary>控制权变更来源（ADR-0008：三条来源路径皆为「请求」非「写入」）。</summary>
    public enum ChangeCause
    {
        /// <summary>守城失败失城（GDD_014 生涯发起）。</summary>
        SiegeDefenseLost = 0,

        /// <summary>战役夺城（GDD_010 战果发起）。</summary>
        SiegeConquest = 1,

        /// <summary>历史事件结局 owner_change（GDD_015 发起）。</summary>
        HistoricalDivergence = 2,

        /// <summary>生涯夺城（GDD_014 发起）。</summary>
        CareerConquest = 3,

        /// <summary>玩家不在场的抽象结算混战易主（GDD_015 抽象结算器发起）。</summary>
        AbstractContest = 4,
    }

    /// <summary>
    /// 城池控制权变更事件（ADR-0008）。GDD_004 城市系统<b>独占发布</b>；
    /// 其余系统（GDD_015 世界模型 / GDD_014 生涯 / 情报 / UI）只订阅，绝不直接写 city.owner。不可变。
    /// </summary>
    public sealed class CityControlChanged
    {
        public CityId City { get; }
        public FactionId NewOwner { get; }
        public Garrison Garrison { get; }
        public ChangeCause Cause { get; }

        public CityControlChanged(CityId city, FactionId newOwner, Garrison garrison, ChangeCause cause)
        {
            City = city;
            NewOwner = newOwner;
            Garrison = garrison;
            Cause = cause;
        }
    }

    /// <summary>
    /// 城池控制权权威接口（ADR-0008）。<b>仅 GDD_004 城市系统实现</b>。
    /// 其余系统经 <see cref="RequestControlChange"/> <b>发起</b>变更（请求，非写入）；由实现校验后写权威态并发布
    /// <see cref="CityControlChanged"/>。读当前归属经 <see cref="OwnerOf"/>；订阅经 <see cref="Subscribe"/>。
    /// </summary>
    public interface ICityControlAuthority
    {
        /// <summary>当前归属（城未登记返回 null）。</summary>
        FactionId? OwnerOf(CityId city);

        /// <summary>当前守备（城未登记返回 null）。</summary>
        Garrison? GarrisonOf(CityId city);

        /// <summary>发起控制权变更：校验后写权威态 + 发布事件。城未登记则抛（须先登记开局归属）。</summary>
        void RequestControlChange(CityId city, FactionId newOwner, Garrison garrison, ChangeCause cause);

        /// <summary>订阅控制权变更事件。</summary>
        void Subscribe(Action<CityControlChanged> handler);
    }
}
