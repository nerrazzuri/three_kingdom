using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// slice 会话的<b>数据驱动初始配置 + 场景设定</b>（ADR-0003：平衡数值集中于配置，逻辑不硬编码）。
    /// 这是把分散的平衡数字收拢到<b>单一来源</b>的场景工厂——会话编排逻辑（<see cref="GameSession"/>）只读这些值，
    /// 方法体内无魔法数字。量产期可由 ScriptableObject→不可变配置管线替换本工厂（ADR-0003）。
    /// </summary>
    public sealed class SliceScenario
    {
        // ---- 时间 ----
        /// <summary>开局世界时间（第 0 日黎明）。</summary>
        public WorldTime Start { get; }

        // ---- 城市（己方账本，GDD_004）----
        /// <summary>开局城市经济状态。</summary>
        public CityEconomyState InitialCity { get; }
        /// <summary>城市日界结算配置。</summary>
        public CitySettlementConfig CityConfig { get; }
        /// <summary>开局后勤持有军粮。</summary>
        public long InitialLogistics { get; }
        /// <summary>人口压力系数（定点）。</summary>
        public FixedPoint PopulationPressure { get; }

        // ---- 情报（敌情探报，GDD_007）----
        /// <summary>玩家阵营。</summary>
        public FactionId PlayerFaction { get; }
        /// <summary>敌方阵营（真值归属）。</summary>
        public FactionId EnemyFaction { get; }
        /// <summary>敌方侦察主题。</summary>
        public IntelSubjectId EnemySubject { get; }
        /// <summary>敌方开局真实兵力。</summary>
        public int EnemyInitialStrength { get; }
        /// <summary>敌方每日增援（真值漂移；玩家不再侦察则情报随时间过时）。</summary>
        public int EnemyReinforcePerDay { get; }

        private SliceScenario()
        {
            Start = new WorldTime(0, DaySegment.Dawn);

            InitialCity = new CityEconomyState(
                id: new CityId("汜水关"),
                stock: 300,
                reserved: 0,
                civMorale: 70,
                security: 55,
                fortificationCurrent: 60,
                fortificationMax: 100);
            CityConfig = new CitySettlementConfig(
                baseYield: 40,
                baseCivConsume: 70,
                baseMaintenance: 20,
                stockFloor: 80,
                civMoraleMax: 100,
                shortageMoralePenalty: FixedPoint.FromInt(1),
                unrestShortageThreshold: 30,
                fortRepairRate: 5);
            InitialLogistics = 0;
            PopulationPressure = FixedPoint.One;

            PlayerFaction = new FactionId("玩家势力");
            EnemyFaction = new FactionId("曹魏");
            EnemySubject = new IntelSubjectId("敌前锋");
            EnemyInitialStrength = 1000;
            EnemyReinforcePerDay = 120;
        }

        /// <summary>slice 默认场景（确定性初值）。</summary>
        public static SliceScenario Default() => new SliceScenario();
    }
}
