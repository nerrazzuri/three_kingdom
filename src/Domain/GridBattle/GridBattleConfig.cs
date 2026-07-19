namespace ThreeKingdom.Domain.GridBattle
{
    /// <summary>
    /// 格子战斗数据驱动配置（ADR-0018 / GDD-028 §7）。全整数——乘子用百分比（dmg*pct/100），
    /// 无 float 于权威路径（ADR-0004）。<see cref="Default"/> 为原型验证值；正式版按平衡套件调。
    /// </summary>
    public sealed class GridBattleConfig
    {
        // netstandard2.1 无 init 访问器（缺 IsExternalInit）——用只读自动属性 + 字段初始化保持不可变。
        // 兵种速度（格/段）与射程（切比雪夫）。兵种差异非克制三角（ADR-0011 D8 护栏）。
        public int CavalrySpeed { get; } = 3;
        public int InfantrySpeed { get; } = 1;
        public int ArcherSpeed { get; } = 1;
        public int CavalryRange { get; } = 1;
        public int InfantryRange { get; } = 1;
        public int ArcherRange { get; } = 2;

        // 涌现倍率（百分比）。
        /// <summary>伏击倍率（林中相邻隘口内敌），百分比。</summary>
        public int AmbushPct { get; } = 250;
        /// <summary>夜袭倍率（夜段近战），百分比。</summary>
        public int NightPct { get; } = 125;

        // 补给硬约束（GDD-028 §3.8 / §4）。
        public int MaxSupply { get; } = 100;
        /// <summary>补给充足阈（≥ 回血）。</summary>
        public int SupplyHighThreshold { get; } = 50;
        /// <summary>补给告急阈（&lt; 叛逃 + 伤亡放大）。</summary>
        public int SupplyLowThreshold { get; } = 25;
        /// <summary>守住己仓每段补给回升。</summary>
        public int SupplyReplenish { get; } = 5;
        /// <summary>己仓被焚/被断每段补给下滑。</summary>
        public int SupplyDrain { get; } = 7;
        /// <summary>火攻焚敌仓一次性补给损失。</summary>
        public int GranaryBurnLoss { get; } = 38;
        /// <summary>补给充足每段回血。</summary>
        public int RecoverPerSegment { get; } = 5;
        /// <summary>补给告急每段叛逃减员。</summary>
        public int DesertPerSegment { get; } = 6;
        /// <summary>补给告急方受伤放大倍率（强攻数倍伤亡），百分比。</summary>
        public int StarvingDamagePct { get; } = 300;

        /// <summary>兵种移动速度（格/段）。</summary>
        public int SpeedOf(TroopKind kind) => kind switch
        {
            TroopKind.Cavalry => CavalrySpeed,
            TroopKind.Infantry => InfantrySpeed,
            TroopKind.Archer => ArcherSpeed,
            _ => 1,
        };

        /// <summary>兵种攻击射程（切比雪夫格）。</summary>
        public int RangeOf(TroopKind kind) => kind switch
        {
            TroopKind.Cavalry => CavalryRange,
            TroopKind.Infantry => InfantryRange,
            TroopKind.Archer => ArcherRange,
            _ => 1,
        };

        /// <summary>原型验证的默认配置。</summary>
        public static GridBattleConfig Default() => new GridBattleConfig();
    }
}
