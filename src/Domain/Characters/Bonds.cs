using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>一对武将之间的羁绊（GDD_025 R4）：无序对 + 类型。同场触发协同（血脉/师徒/知己）或崩解（仇怨）。</summary>
    public sealed class Bond
    {
        public CharacterId A { get; }
        public CharacterId B { get; }
        public BondType Type { get; }

        public Bond(CharacterId a, CharacterId b, BondType type) { A = a; B = b; Type = type; }

        /// <summary>某两人是否即本羁绊之两端（无序）。</summary>
        public bool Links(CharacterId x, CharacterId y)
            => (A == x && B == y) || (A == y && B == x);

        /// <summary>某人是否为本羁绊一端。</summary>
        public bool Involves(CharacterId x) => A == x || B == x;
    }

    /// <summary>羁绊效果配置（GDD_025 R4 §12，可版本化）。</summary>
    public sealed class BondConfig
    {
        /// <summary>每对同场协同羁绊（血脉/师徒/知己）的士气加成。</summary>
        public FixedPoint SynergyBonus { get; }
        /// <summary>每对同场仇怨的士气惩罚（互相牵制）。</summary>
        public FixedPoint FeudPenalty { get; }
        /// <summary>士气增减上下限（防多羁绊叠加失衡）。</summary>
        public FixedPoint Cap { get; }
        /// <summary>崩解·狂怒：血脉/知己在同场阵亡 → 余者悲愤死战，士气骤升幅度（GDD_025 R4）。</summary>
        public FixedPoint CollapseRage { get; }

        public BondConfig(FixedPoint synergyBonus, FixedPoint feudPenalty, FixedPoint cap, FixedPoint collapseRage)
        {
            SynergyBonus = synergyBonus;
            FeudPenalty = feudPenalty;
            Cap = cap;
            CollapseRage = collapseRage;
        }

        /// <summary>默认：协同 +8%/对、仇怨 −12%/对、封顶 ±30%、崩解狂怒 +18%。</summary>
        public static BondConfig Default { get; } = new BondConfig(
            FixedPoint.FromFraction(8, 100), FixedPoint.FromFraction(12, 100),
            FixedPoint.FromFraction(30, 100), FixedPoint.FromFraction(18, 100));
    }

    /// <summary>
    /// 羁绊效果（GDD_025 R4，<b>确定性纯函数</b>）：同一阵营在场武将之间的羁绊 → 士气修正。
    /// 血脉/师徒/知己同场<b>协同</b>（士气升，并肩死战）；仇怨同场<b>互扣</b>（貌合神离，士气损）。
    /// 崩解（一人亡则余者狂怒不可控）为动态效果，接后续战中死亡事件（此处先给同场静态协同/互扣）。
    /// </summary>
    public sealed class BondEffectService
    {
        /// <summary>
        /// 同阵营在场武将的<b>羁绊士气乘数</b>：Σ(协同对 +SynergyBonus / 仇怨对 −FeudPenalty)，封顶后 1+Σ。
        /// 只计"两端皆在场"的羁绊。无羁绊则中性 1.0。
        /// </summary>
        public FixedPoint SideBondMorale(IReadOnlyList<CharacterId> present, IReadOnlyList<Bond> bonds, BondConfig config)
        {
            if (present == null || present.Count < 2 || bonds == null || bonds.Count == 0) return FixedPoint.One;
            BondConfig cfg = config ?? BondConfig.Default;

            FixedPoint sum = FixedPoint.Zero;
            foreach (Bond b in bonds)
            {
                if (!BothPresent(present, b)) continue;
                sum += b.Type == BondType.Feud ? -cfg.FeudPenalty : cfg.SynergyBonus;
            }
            sum = sum.Clamp(-cfg.Cap, cfg.Cap);
            return FixedPoint.One + sum;
        }

        /// <summary>
        /// 崩解·狂怒（GDD_025 R4）：<paramref name="survivor"/> 是否会因 <paramref name="fallen"/> 阵亡而悲愤死战——
        /// 唯<b>血脉/知己</b>之交触发（生死与共者亡则余者狂怒）；师徒/仇怨不触发。
        /// </summary>
        public bool IsRousedByFall(CharacterId survivor, CharacterId fallen, IReadOnlyList<Bond> bonds)
        {
            if (bonds == null) return false;
            foreach (Bond b in bonds)
                if (b.Links(survivor, fallen) && (b.Type == BondType.Blood || b.Type == BondType.Kindred))
                    return true;
            return false;
        }

        private static bool BothPresent(IReadOnlyList<CharacterId> present, Bond b)
        {
            bool a = false, c = false;
            foreach (CharacterId id in present)
            {
                if (id == b.A) a = true;
                if (id == b.B) c = true;
            }
            return a && c;
        }
    }
}
