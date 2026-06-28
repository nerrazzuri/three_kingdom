using System;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.World
{
    /// <summary>抽象结算结局类别（GDD_015 §AI Requirements / ADR-0007 §4）。</summary>
    public enum AbstractOutcomeKind
    {
        /// <summary>攻方占据（争夺城易主给攻方）。</summary>
        AttackerTakes = 0,

        /// <summary>守方守住（归属不变）。</summary>
        DefenderHolds = 1,
    }

    /// <summary>
    /// 一场玩家不在场势力混战的抽象结算输入（ADR-0007 §4）。不可变值。
    /// 精度只需"不出戏"，按势力体量/态势 + 注入随机加权——非逐单位。
    /// </summary>
    public readonly struct ContestContext
    {
        /// <summary>被争夺城池。</summary>
        public CityId ContestedCity { get; }

        /// <summary>攻方态势加成（定点，≥0；地形/突然性等的抽象，1=无加成）。</summary>
        public FixedPoint AttackerBias { get; }

        /// <summary>攻方占据后该城守备。</summary>
        public Garrison ResultingGarrison { get; }

        public ContestContext(CityId contestedCity, FixedPoint attackerBias, Garrison resultingGarrison)
        {
            if (attackerBias < FixedPoint.Zero) throw new ArgumentOutOfRangeException(nameof(attackerBias), "攻方加成不可为负。");
            ContestedCity = contestedCity;
            AttackerBias = attackerBias;
            ResultingGarrison = resultingGarrison;
        }
    }

    /// <summary>
    /// 抽象结算结局（ADR-0007 §4）。不可变。携胜负方与是否易主；<b>不</b>直接写城池归属——
    /// 易主须经 GDD_004 控制权变更落地（ADR-0008，见 story-004）。纳入状态哈希（确定性）。
    /// </summary>
    public sealed class AbstractOutcome
    {
        public AbstractOutcomeKind Kind { get; }
        public FactionId Winner { get; }
        public FactionId Loser { get; }
        public CityId ContestedCity { get; }

        /// <summary>归属是否变化（攻方占据为 true；守方守住为 false）。</summary>
        public bool OwnershipChanged { get; }

        public AbstractOutcome(AbstractOutcomeKind kind, FactionId winner, FactionId loser, CityId contestedCity, bool ownershipChanged)
        {
            Kind = kind;
            Winner = winner;
            Loser = loser;
            ContestedCity = contestedCity;
            OwnershipChanged = ownershipChanged;
        }

        /// <summary>以规范顺序追加到状态哈希。</summary>
        public void AppendTo(StateHasher hasher)
        {
            if (hasher is null) throw new ArgumentNullException(nameof(hasher));
            hasher.Append((int)Kind);
            AppendStr(hasher, Winner.Value);
            AppendStr(hasher, Loser.Value);
            AppendStr(hasher, ContestedCity.Value);
            hasher.Append(OwnershipChanged);
        }

        private static void AppendStr(StateHasher h, string s)
        {
            h.Append(s.Length);
            foreach (char c in s) h.Append((int)c);
        }
    }

    /// <summary>
    /// 抽象结算器（ADR-0007 §4）。玩家不在场的势力混战不跑完整 GDD_010 战役，
    /// 由本接口按势力体量/态势 + <b>注入</b>确定性随机产出结局。随机仅经 <see cref="IDeterministicRandom"/>（ADR-0004）。
    /// </summary>
    public interface IAbstractResolver
    {
        AbstractOutcome Resolve(FactionRecord attacker, FactionRecord defender, ContestContext ctx, IDeterministicRandom rng);
    }

    /// <summary>
    /// 体量加权抽象结算器（MVP，ADR-0007 §4 / ADR-0004）。势力体量以领有城池数为代理 + 攻方态势加成，
    /// 抽注入随机单位值按强弱比定占据/守住。确定性：同输入 + 同 rng 位置 → 同结局。零旁路随机、零 float 权威外泄。
    /// </summary>
    public sealed class StrengthAbstractResolver : IAbstractResolver
    {
        public AbstractOutcome Resolve(FactionRecord attacker, FactionRecord defender, ContestContext ctx, IDeterministicRandom rng)
        {
            if (attacker is null) throw new ArgumentNullException(nameof(attacker));
            if (defender is null) throw new ArgumentNullException(nameof(defender));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // 体量代理：领有城池数 +1（避免 0）；攻方按态势加成放大。定点权威路径，无 float。
            FixedPoint attackerStrength = FixedPoint.FromInt(attacker.OwnedCities.Count + 1) * ctx.AttackerBias;
            FixedPoint defenderStrength = FixedPoint.FromInt(defender.OwnedCities.Count + 1);
            FixedPoint total = attackerStrength + defenderStrength;

            // 攻方占据概率 = attackerStrength / total；抽 [0,1) 注入随机决定。
            FixedPoint takeThreshold = total == FixedPoint.Zero ? FixedPoint.Zero : attackerStrength / total;
            FixedPoint roll = rng.NextUnit();

            bool attackerTakes = roll < takeThreshold;
            return attackerTakes
                ? new AbstractOutcome(AbstractOutcomeKind.AttackerTakes, attacker.Id, defender.Id, ctx.ContestedCity, ownershipChanged: true)
                : new AbstractOutcome(AbstractOutcomeKind.DefenderHolds, defender.Id, attacker.Id, ctx.ContestedCity, ownershipChanged: false);
        }
    }

    /// <summary>
    /// 抽象结算适用范围策略（ADR-0007 §4 边界）。玩家<b>够得着</b>的势力行动由 GDD_016 战略层驱动；
    /// <b>够不着</b>（双方均不在玩家势力圈）才用抽象结算——省算力且不与 016 重叠。
    /// </summary>
    public static class AbstractContestPolicy
    {
        /// <summary>该场争夺是否应走抽象结算（双方均够不着）。</summary>
        public static bool ShouldResolveAbstractly(FactionRecord a, FactionRecord b, PlayerReach reach)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));
            if (reach is null) throw new ArgumentNullException(nameof(reach));
            return !reach.Touches(a.Id) && !reach.Touches(b.Id);
        }
    }
}
