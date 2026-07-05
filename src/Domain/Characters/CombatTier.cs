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
    /// 谋略档 → 计谋系数（GDD_025，与战阵档对称）：乘进<b>已成型兵法的战力加成</b>——诸葛之计比马谡同计更狠，
    /// 偶有"一计定乾坤"。同 5 级 + 区间 + 右偏抽取；与战阵档<b>独立</b>（吕布可绝世武·愚钝谋）。
    /// </summary>
    public enum StrategyTier
    {
        /// <summary>愚钝：有勇无谋（吕布/许褚级）。</summary>
        Dull = 0,
        /// <summary>寻常：粗通韬略。</summary>
        Plain = 1,
        /// <summary>机敏：临机有策（荀彧/鲁肃级）。</summary>
        Sharp = 2,
        /// <summary>智略：奇谋百出（郭嘉/贾诩级）。</summary>
        Adept = 3,
        /// <summary>经天纬地：算无遗策（诸葛/周瑜/司马级）。</summary>
        Master = 4,
    }

    /// <summary>
    /// 档 → 系数（GDD_025，<b>确定性</b>右偏抽取，ADR-0004）。定点 Q16.16，注入式种子随机（非 float/掷骰）。
    /// 系数 = 低 + (高−低)·u²（k=2：期望约落区间前 1/3，高端罕见）。战阵档/谋略档共用此区间数学（等级 0..4）。
    /// </summary>
    public static class CombatProwess
    {
        private static FixedPoint F(int n) => FixedPoint.FromFraction(n, 100);

        /// <summary>某等级（0..4）的系数区间 [低, 高]（重叠——强弱是概率优势非绝对墙）。</summary>
        public static (FixedPoint Min, FixedPoint Max) RangeAt(int level) => level switch
        {
            4 => (F(130), F(175)),
            3 => (F(110), F(145)),
            2 => (F(95), F(120)),
            1 => (F(85), F(105)),
            _ => (F(75), F(90)),   // 0
        };

        /// <summary>战阵档系数区间。</summary>
        public static (FixedPoint Min, FixedPoint Max) Range(CombatTier tier) => RangeAt((int)tier);

        /// <summary>种子化右偏抽取某等级系数（u=NextUnit；skew=u² 令低端常见、高端罕见）。同流可复现。</summary>
        public static FixedPoint RollAt(int level, IDeterministicRandom rng)
        {
            (FixedPoint min, FixedPoint max) = RangeAt(level);
            FixedPoint u = rng.NextUnit();
            FixedPoint skew = u * u;                 // k=2 右偏
            return min + (max - min) * skew;
        }

        /// <summary>战阵档 → 杀伤系数（右偏抽取）。</summary>
        public static FixedPoint Roll(CombatTier tier, IDeterministicRandom rng) => RollAt((int)tier, rng);

        /// <summary>谋略档 → 计谋系数（右偏抽取，乘进兵法条件加成）。</summary>
        public static FixedPoint RollStrategy(StrategyTier tier, IDeterministicRandom rng) => RollAt((int)tier, rng);
    }
}
