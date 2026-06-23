using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Diplomacy;
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

        /// <summary>援军抵达日（守住至此日 = 胜；0 基日序号，展示为「第 ReliefDay+1 日」）。守城待变核心目标。</summary>
        public int ReliefDay { get; }

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

        // ---- 外交（求粮受控入口，GDD_012 §8）----
        /// <summary>目标外势力。</summary>
        public ForeignPowerId DiplomacyPower { get; }
        /// <summary>外交平衡配置。</summary>
        public DiplomacyConfig DiplomacyConfig { get; }
        /// <summary>求援承诺代价。</summary>
        public long DiplomacyPledgeCost { get; }
        /// <summary>求粮兑现到达时入城的粮草量。</summary>
        public long DiplomacySupplyAmount { get; }
        /// <summary>对外势力声望 standing（[0,1]）。</summary>
        public FixedPoint DiplomacyStanding { get; }
        /// <summary>敌方反向外交压力（[0,1]）。</summary>
        public FixedPoint DiplomacyPressure { get; }
        /// <summary>外交兑现判定随机流种子（确定性，位置可存档）。</summary>
        public ulong DiplomacyRngSeed { get; }

        // ---- 军议（军师条件化建议，GDD_008）----
        /// <summary>军师视角（能力影响缺口发现与置信）。</summary>
        public AdvisorPerspective Advisor { get; }
        /// <summary>军议平衡配置。</summary>
        public CouncilConfig CouncilConfig { get; }
        /// <summary>条件化建议模板（数据驱动，覆盖 slice 三链）。</summary>
        public IReadOnlyList<AdviceTemplate> AdviceTemplates { get; }
        /// <summary>已知主题的依据置信（slice：已侦察即中等可靠，[0,1] 定点）。</summary>
        public FixedPoint KnownClaimConfidence { get; }

        private SliceScenario()
        {
            Start = new WorldTime(0, DaySegment.Dawn);
            ReliefDay = 8; // 第 9 日（0 基 8）援军抵达 = 胜；守不到则可能民心崩溃失城。

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

            // 外交（求粮，GDD_012 §8）：静态背景外势力，延迟交付 + 可背约（确定性随机流）。
            DiplomacyPower = new ForeignPowerId("江东");
            DiplomacyConfig = new DiplomacyConfig(
                baseGrant: FixedPoint.FromFraction(2, 5),         // 0.4
                weightStanding: FixedPoint.FromFraction(1, 2),    // 0.5
                weightCost: FixedPoint.FromFraction(3, 10),       // 0.3
                weightPressure: FixedPoint.FromFraction(2, 5),    // 0.4
                acceptThreshold: FixedPoint.FromFraction(3, 5),   // 0.6
                conditionalThreshold: FixedPoint.FromFraction(2, 5), // 0.4
                costNormalizer: 100,
                commitLeadSegments: WorldTime.SegmentsPerDay * 2, // 两日后抵达
                betrayRiskBase: FixedPoint.FromFraction(1, 5),    // 0.2
                betrayPressureWeight: FixedPoint.FromFraction(3, 10), // 0.3
                betrayalStandingPenalty: 5);
            DiplomacyPledgeCost = 50;
            DiplomacySupplyAmount = 120;        // 兑现则到达时入城粮草
            DiplomacyStanding = FixedPoint.FromFraction(3, 5);    // 0.6
            DiplomacyPressure = FixedPoint.FromFraction(1, 5);    // 0.2
            DiplomacyRngSeed = 0xD17_0ACE_2026UL;

            // 军议（GDD_008）：三条条件化建议，依据敌情主题；并列呈现，无最优解。
            Advisor = new AdvisorPerspective(new AdvisorId("随军军师"), FixedPoint.FromFraction(7, 10)); // adv_cap 0.7
            CouncilConfig = new CouncilConfig(gapDetectionWeight: FixedPoint.One);
            KnownClaimConfidence = FixedPoint.FromFraction(1, 2); // 0.5 已侦察=依据中等
            var enemyRef = new[] { EnemySubject };
            AdviceTemplates = new List<AdviceTemplate>
            {
                new AdviceTemplate(
                    "断粮疲敌",
                    "敌前锋深入，补给线拉长。",
                    "若敌补给可被持续袭扰，其战力随时日衰减。",
                    new[] { "需查明敌补给路线与护卫强度", "需投入袭扰兵力且承担暴露风险" },
                    new[] { "袭扰队可能被反伏", "敌可能改道补给" },
                    enemyRef),
                new AdviceTemplate(
                    "守城待变",
                    "援军定于第 9 日抵达。",
                    "若粮草民心可支撑至援军，则不必决战。",
                    new[] { "需粮草撑至援军日", "可向外求粮缓解" },
                    new[] { "久守民心易崩", "敌或在援军前强攻" },
                    enemyRef),
                new AdviceTemplate(
                    "假退伏击",
                    "敌将性烈，易受诱。",
                    "若示弱诱敌冒进，可于隘口设伏。",
                    new[] { "需摸清敌将性格与追击倾向", "需预设伏兵与退路" },
                    new[] { "诱敌不成反失城门", "伏击暴露则两面受敌" },
                    enemyRef),
            };
        }

        /// <summary>slice 默认场景（确定性初值）。</summary>
        public static SliceScenario Default() => new SliceScenario();
    }
}
