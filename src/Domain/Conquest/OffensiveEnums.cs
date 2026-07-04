namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 兵种（GDD_019 D4 兵种编成，<b>杠杆非克制</b>）。枚举序数即规范遍历顺序（确定性，ADR-0004）。
    /// 每种兵在匹配地形/时机/路线时<b>强化特定兵法条件 + 小幅战力契合</b>，<b>绝无</b>兵种间克制减益（ADR-0011 D3）。
    /// </summary>
    public enum TroopType
    {
        /// <summary>步卒（攻坚/长围主力）。</summary>
        Infantry = 0,
        /// <summary>骑兵（机动/追击，利平原）。</summary>
        Cavalry = 1,
        /// <summary>弓弩（远程压制，利隘口守势）。</summary>
        Archer = 2,
        /// <summary>水军（水战/渡口——非陆战场景，模型预留）。</summary>
        Marine = 3,
    }

    /// <summary>
    /// 将领专长（GDD_019 D3）：降低对应布势路线兵法条件的成型门槛（本 MVP 以枚举标注，
    /// 具体降门逻辑随平衡接入；当前 Derive 主要用统率/武勇/智略三属性，专长供展示与后续扩展）。
    /// </summary>
    public enum GeneralSpecialty
    {
        /// <summary>无专长。</summary>
        None = 0,
        /// <summary>善奇袭（诱敌/夜袭）。</summary>
        Ambush = 1,
        /// <summary>善攻坚（正面强攻/长围）。</summary>
        Siege = 2,
        /// <summary>善骑战（机动/追击）。</summary>
        Cavalry = 3,
        /// <summary>善辎重（补给/久围）。</summary>
        Logistics = 4,
    }

    /// <summary>
    /// 布势路线（GDD_019 D5，<b>阵型的正确落法·非坐标</b>）：择一即声明要凑成的那条兵法链（复用 GDD_010 TacticTag）。
    /// <b>数据模型无 position/grid/facing 字段</b>——摆的是条件不是坐标（ADR-0011 D2 / AC-3c 负向不变量）。
    /// </summary>
    public enum ApproachPlan
    {
        /// <summary>正面强攻：纯战力硬碰，无兵法条件（裸战底），步卒契合、见效快。</summary>
        FrontalAssault = 0,
        /// <summary>假退诱敌（= FeintAmbush）：目标条件 受控撤退保形 / 敌军追击 / 伏兵突然性。</summary>
        FeintLure = 1,
        /// <summary>长围断粮（= SupplyExhaustion）：目标条件 切断补给 / 断粮达宽限 / 敌士气疲劳跨阈。</summary>
        ProtractedSiege = 2,
        /// <summary>夜袭（= NightRaid）：目标条件 值夜 / 隐蔽成功 / 守方未察觉 / 袭方军纪达标。</summary>
        NightRaid = 3,
    }

    /// <summary>
    /// 出征路线地形（GDD_019 D5 布势门）：某些兵法条件（伏兵突然性）须特定地形方成型。
    /// 由目标城/进攻路线的场景数据给定（数据驱动）。
    /// </summary>
    public enum TerrainKind
    {
        /// <summary>平原（利骑战机动）。</summary>
        Plain = 0,
        /// <summary>隘口（利设伏）。</summary>
        Pass = 1,
        /// <summary>渡口（利水战）。</summary>
        Ford = 2,
        /// <summary>坚城（攻坚地形）。</summary>
        Fortified = 3,
        /// <summary>遮蔽高地/林（利隐蔽/夜袭；GDD_021 区域战斗，末尾追加保序）。</summary>
        Cover = 4,
    }
}
