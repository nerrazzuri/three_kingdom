using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Characters
{
    /// <summary>
    /// 战阵档（GDD_025）：武将带兵作战的<b>杀伤强度粗档</b>——隐藏、不显示为数字。5 级，各对应一个系数<b>区间</b>。
    /// 每次结算种子化<b>右偏</b>抽取系数（低端常见、高端罕见 → 偶有"神勇爆发"）。区间互相重叠：低档偶尔能碰高档垫底，
    /// 但期望值随档递增，长期强弱分明。玩家靠名声/战场表现判断，看不到档次数字。
    /// </summary>
    public enum CombatTier
    {
        /// <summary>羸弱：纯文士，带兵杀伤最低。</summary>
        Feeble = 0,
        /// <summary>寻常：文官/守成带兵。</summary>
        Ordinary = 1,
        /// <summary>勇健：合格战将。</summary>
        Sturdy = 2,
        /// <summary>骁锐：一时之勇（周仓/廖化级）。</summary>
        Valiant = 3,
        /// <summary>绝世：万人敌（吕布/关羽/赵云级）。</summary>
        Peerless = 4,
    }

    /// <summary>
    /// 战阵档 → 杀伤系数（GDD_025，<b>确定性</b>右偏抽取，ADR-0004）。定点 Q16.16，注入式种子随机（非 float/掷骰）。
    /// 系数 = 低 + (高−低)·u^2（k=2：期望约落区间前 1/3，高端罕见）。
    /// </summary>
    public static class CombatProwess
    {
        private static FixedPoint F(int n) => FixedPoint.FromFraction(n, 100);

        /// <summary>某档的系数区间 [低, 高]（重叠——强弱是概率优势非绝对墙）。</summary>
        public static (FixedPoint Min, FixedPoint Max) Range(CombatTier tier) => tier switch
        {
            CombatTier.Peerless => (F(130), F(175)),
            CombatTier.Valiant => (F(110), F(145)),
            CombatTier.Sturdy => (F(95), F(120)),
            CombatTier.Ordinary => (F(85), F(105)),
            _ => (F(75), F(90)),   // Feeble
        };

        /// <summary>
        /// 种子化右偏抽取一次杀伤系数。<paramref name="rng"/> 为注入式确定性流（战斗种子+回合+支队派生）→ 同局可复现。
        /// u=NextUnit∈[0,1)；skew=u²（k=2）令低端常见、高端罕见。
        /// </summary>
        public static FixedPoint Roll(CombatTier tier, IDeterministicRandom rng)
        {
            (FixedPoint min, FixedPoint max) = Range(tier);
            FixedPoint u = rng.NextUnit();
            FixedPoint skew = u * u;                 // k=2 右偏
            return min + (max - min) * skew;
        }
    }
}
