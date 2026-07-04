using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;
using WTimeWindow = ThreeKingdom.Domain.World.TimeWindow;
using PrepTimeWindow = ThreeKingdom.Domain.Preparation.TimeWindow;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 「汜水关太守」默认可玩场景（M15 单一数据源，ADR-0003 数据驱动 / ADR-0009 配置驱动开局）。
    /// console harness 与 Unity 表现层<b>共用同一来源</b>（epic-028 story-001：单一场景源，勿复制两份数值）。
    /// 组装一套<b>全 11 循环启用</b>的 <see cref="CampaignStartConfig"/>——城市治理 / 情报 / 军议 / 战役准备 /
    /// 历史世界——并暴露驱动循环运行期所需的卫星配置（晋升梯队 / 叛乱 / 战斗 / 兵法链）与稳定 id。
    /// 全部数值取自各循环已验证的单元测试夹具，合并为一个连贯场景；固定指纹/种子 → 确定性（ADR-0004）。
    /// 不可变：构造一次，之后只读。所有数值在此集中，方法体内无散落魔法数（CON-5 同纪律）。
    /// </summary>
    public sealed class PlayableCampaign
    {
        // ---- 稳定标识（配置与运行期命令共用，单一来源）----

        /// <summary>玩家（太守）势力。</summary>
        public static readonly FactionId Player = new FactionId("faction-player");
        /// <summary>当面之敌（袁军）。</summary>
        public static readonly FactionId Enemy = new FactionId("faction-yuan");
        /// <summary>历史势力（孙吴）——历史事件前置所系。</summary>
        public static readonly FactionId Sun = new FactionId("faction-sun");

        /// <summary>玩家君主。</summary>
        public static readonly CharacterId Lord = new CharacterId("char-player-lord");
        /// <summary>副将 / 军师候选（部曲 + 军议视角）。</summary>
        public static readonly CharacterId Aide = new CharacterId("char-aide");

        /// <summary>所辖城池（汜水关）。</summary>
        public static readonly CityId Fanshui = new CityId("city-fanshui");
        /// <summary>可出征攻打的敌方目标城（虎牢关，曹魏控制；GDD_019 出征目标）。</summary>
        public static readonly CityId EnemyCity = new CityId("city-hulao");
        /// <summary>战役可达区域（隘口，伏击发生地）。</summary>
        public static readonly RegionId Pass = new RegionId("region-pass");
        /// <summary>军粮资源键。</summary>
        public static readonly ResourceKey Grain = new ResourceKey("res-grain");
        /// <summary>授权的战役命令（设伏）。</summary>
        public static readonly OrderId AmbushOrder = new OrderId("order-ambush");
        /// <summary>情报对象（敌军主力）。</summary>
        public static readonly IntelSubjectId EnemyArmy = new IntelSubjectId("subject-enemy-army");

        /// <summary>战斗单位 id。</summary>
        public static readonly BattleUnitId PlayerUnit = new BattleUnitId("unit-player-1");
        public static readonly BattleUnitId EnemyUnit = new BattleUnitId("unit-enemy-1");

        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xCA11AB1EUL);
        private static FixedPoint Frac(int n, int d) => FixedPoint.FromFraction(n, d);

        /// <summary>开局已校验配置（全循环启用）。</summary>
        public CampaignStartConfig StartConfig { get; }

        /// <summary>晋升梯队（GDD_014；低门槛便于 harness 体验晋升）。</summary>
        public PromotionLadderConfig Ladder { get; }

        /// <summary>自立配置（GDD_014）。</summary>
        public RebellionConfig Rebellion { get; }

        /// <summary>战斗配置（GDD_010）。</summary>
        public BattleConfig BattleConfig { get; }

        /// <summary>兵法识别链配置（竖切默认链，含假退伏击机动招式）。</summary>
        public TacticChainConfig TacticChains { get; }

        /// <summary>战果后果配置（GDD_010 §后果 / M07）。</summary>
        public OutcomeConsequenceConfig OutcomeConfig { get; }

        /// <summary>自立后拟建的新势力 id（GDD_014）。</summary>
        public static readonly FactionId RebelFaction = new FactionId("faction-player-rebel");

        /// <summary>太守效力的君主势力（占城 C 归君主直辖时的承接方，ADR-0010）。</summary>
        public static readonly FactionId LordFaction = new FactionId("faction-lord");

        // ---- 出征卫星配置（GDD_019 v2 / ADR-0011）：单一数据源，harness 与 Unity 壳共用，勿复制数值 ----

        /// <summary>六维闭合因果映射配置。</summary>
        public OffensiveSetupConfig OffensiveSetup => OffensiveSetupConfig.Default;

        /// <summary>攻城胜负结算配置（每条兵法条件加成）。</summary>
        public SiegeResolutionConfig SiegeResolution => SiegeResolutionConfig.Default;

        /// <summary>占城归属 C 配置（ADR-0010）。</summary>
        public OccupationConfig Occupation => OccupationConfig.Default;

        /// <summary>出征占城归属判定固定种子（确定性）。</summary>
        public ulong OffensiveSeed => 0xC0FFEEUL;

        /// <summary>主将（太守亲征）：统率0.7/武勇0.7/智略0.6，善攻坚（战斗属性 ADR-0011 D5，与好感解耦）。</summary>
        public OffensiveGeneral LeadGeneral
            => new OffensiveGeneral(Lord, Frac(7, 10), Frac(7, 10), Frac(6, 10), GeneralSpecialty.Siege);

        /// <summary>可选副将花名册（GDD_014 僚属；副将 Aide 智略高·善奇袭，利设伏路线）。</summary>
        public IReadOnlyList<OffensiveGeneral> DeputyRoster
            => new[] { new OffensiveGeneral(Aide, Frac(5, 10), Frac(6, 10), Frac(8, 10), GeneralSpecialty.Ambush) };

        /// <summary>目标敌城的<b>真实</b>守备（结算用真值；玩家所见须经情报投影，反全知）。虎牢关：守军600 × 工事1.2。</summary>
        public SiegeDefense DefenseOf(CityId city) => new SiegeDefense(600, Frac(12, 10));

        /// <summary>目标进攻路线地形（虎牢关=隘口，利设伏；伏兵突然性条件门）。</summary>
        public TerrainKind TerrainOf(CityId city) => TerrainKind.Pass;

        /// <summary>可出征目标城清单（GDD_019 §7 选目标；授权门/敌控由运行期按会话控制权投影判定）。</summary>
        public IReadOnlyList<CityId> OffensiveTargetCities => new[] { EnemyCity };

        /// <summary>守城区域防御战：玩家守军（汜水关；GDD_021 攻守统一，守方视角）。</summary>
        public int DefenseGarrison => 700;

        /// <summary>守城区域防御战：来犯敌军突击兵力（敌AI攻方）。</summary>
        public int EnemyAssaultForce => 500;

        /// <summary>占城后进驻的守军（占城 C 控制权变更的新驻军）。</summary>
        public Garrison ConqueredGarrison => new Garrison(600);

        /// <summary>开战固定种子（确定性）。</summary>
        public ulong BattleSeed => 42UL;

        /// <summary>侦察行程时段（GDD_007 派出→在途→返报；默认约 1 日返报，可调）。</summary>
        public int ScoutLeadSegments => WorldTime.SegmentsPerDay;

        /// <summary>征用军粮办理时段（GDD_004 派人处理→需时见效；约 1 日，可调）。</summary>
        public int RequisitionLeadSegments => WorldTime.SegmentsPerDay;

        /// <summary>修工事办理时段（工程较重；约 1 日，可调）。</summary>
        public int RepairLeadSegments => WorldTime.SegmentsPerDay;

        /// <summary>安抚办理时段（派吏安民较快；约半日，可调）。</summary>
        public int AppeaseLeadSegments => WorldTime.SegmentsPerDay / 2;

        private PlayableCampaign()
        {
            StartConfig = BuildStartConfig();
            Ladder = BuildLadder();
            Rebellion = new RebellionConfig(
                rebelCityMin: 3, rebelRenownMin: 400,
                rebelAffinityMin: Frac(5, 10), defectThreshold: Frac(5, 10),
                loyalRatioHi: Frac(7, 10), loyalRatioMid: Frac(4, 10));
            BattleConfig = new BattleConfig(Frac(15, 10), Frac(1, 1));
            TacticChains = TacticChainConfig.SliceDefault();
            OutcomeConfig = new OutcomeConsequenceConfig(
                reputationLossDefeat: 20, reputationLossRetreat: 8, reputationLossCityLost: 35,
                civMoraleLoss: 15, securityLoss: 20, fortificationDamage: 25, forceAttrition: 600);
        }

        /// <summary>构造默认可玩场景。</summary>
        public static PlayableCampaign Default() => new PlayableCampaign();

        /// <summary>开战玩家+敌方单位（确定性预设；敌弱于守方，留给玩家用杠杆扩大优势）。</summary>
        public IReadOnlyList<BattleUnitState> Units(int playerForce = 1000, int enemyForce = 800)
            => new[]
            {
                new BattleUnitState(PlayerUnit, Player, Pass, playerForce,
                    morale: Frac(7, 10), fatigue: Frac(2, 10), discipline: Frac(6, 10),
                    terrainMod: Frac(1, 1), postureMod: Frac(1, 1), support: Frac(0, 1)),
                new BattleUnitState(EnemyUnit, Enemy, Pass, enemyForce,
                    morale: Frac(7, 10), fatigue: Frac(2, 10), discipline: Frac(6, 10),
                    terrainMod: Frac(1, 1), postureMod: Frac(1, 1), support: Frac(0, 1)),
            };

        /// <summary>一条满足开战前提的准备计划命令（设伏，投入 40 军粮）。</summary>
        public PreparedOrder AmbushPlan()
            => new PreparedOrder(
                AmbushOrder, Aide, Pass, new PrepTimeWindow(0, 2),
                new Dictionary<ResourceKey, long> { [Grain] = 40 }, null);

        // ---- 配置组装（各段取自对应循环的已验证夹具）----

        private static CampaignStartConfig BuildStartConfig()
            => new CampaignStartConfig(
                scenarioConfigId: "scenario-fanshui-playable",
                fingerprint: Fp,
                governorSeed: new CitySeed(Player, Fanshui, 800, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                startTime: new WorldTime(0, DaySegment.Dawn),
                initialFactions: new[]
                {
                    new FactionRecord(Player, Lord, SurvivalStatus.Active, RelationToPlayer.Self, new[] { Fanshui }),
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { EnemyCity }),
                    new FactionRecord(Sun, new CharacterId("char-sunquan"), SurvivalStatus.Active, RelationToPlayer.Neutral, Array.Empty<CityId>()),
                },
                initialCities: new[] { new CityOwnership(Fanshui, Player, 800), new CityOwnership(EnemyCity, Enemy, 600) },
                // 城市治理（M03）：库存100 / 民心60 / 城防20。
                cityEconomy: new CityEconomyState(Fanshui, stock: 100, reserved: 0, civMorale: 60, security: 50, fortificationCurrent: 20, fortificationMax: 100),
                settlementConfig: new CitySettlementConfig(
                    baseYield: 20, baseCivConsume: 30, baseMaintenance: 10, stockFloor: 0,
                    civMoraleMax: 100, shortageMoralePenalty: Frac(1, 2), unrestShortageThreshold: 50, fortRepairRate: 15),
                populationPressure: FixedPoint.FromInt(1),
                initialLogisticsHolding: 0,
                governanceConfig: new CityGovernanceConfig(Frac(1, 2), 10, 10),
                // 情报（M04）：敌军主力真值（玩家初始未知，须侦察）。
                worldTruth: BuildTruth(),
                playerIntel: new FactionIntel(Player),
                intelConfig: BuildIntelConfig(),
                councilSetup: BuildCouncilSetup(),
                // 战役准备（M05）：军粮池 + 隘口可达 + 设伏授权。
                resourcePool: new ResourcePool(new Dictionary<ResourceKey, long> { [Grain] = 100 }),
                preparationConfig: new PreparationConfig(tightResourceMargin: 10),
                reachableRegions: new[] { Pass },
                authorizedOrders: new[] { AmbushOrder },
                // 历史世界（M10）：赤壁→夷陵，前置=孙存活；玩家触及孙吴（够得着，可分叉）。
                historyCatalog: BuildHistory(),
                playerReach: new PlayerReach(new[] { Sun }, Array.Empty<CityId>()),
                divergenceConfig: new DivergencePropagationConfig(2));

        private static WorldTruthLedger BuildTruth()
        {
            var t = new WorldTruthLedger();
            t.Set(new TruthRecord(EnemyArmy, 5000, Enemy));
            return t;
        }

        private static IntelConfig BuildIntelConfig()
            => new IntelConfig(
                new Dictionary<IntelSource, FixedPoint> { [IntelSource.Scouting] = Frac(8, 10), [IntelSource.DirectObservation] = Frac(9, 10) },
                baseError: 0, ttlSegments: 8, baseExposure: Frac(2, 10),
                exposureAlertWeight: Frac(1, 10), exposureSkillWeight: Frac(1, 10));

        private static SessionCouncilSetup BuildCouncilSetup()
        {
            var advisor = new AdvisorPerspective(new AdvisorId("advisor-zhuge"), Frac(8, 10));
            var template = new AdviceTemplate(
                candidateId: "advice-ambush",
                observation: "隘口适于设伏",
                assumption: "若敌将急躁且经此路",
                requiredConditions: new[] { "敌将性烈", "敌军经隘口" },
                risks: new[] { "暴露则反受夹击" },
                referencedSubjects: new[] { EnemyArmy });
            return new SessionCouncilSetup(advisor, new[] { template }, new CouncilConfig(Frac(1, 1)), Frac(7, 10));
        }

        private static HistoricalEventCatalog BuildHistory()
        {
            var chibi = new HistoricalEvent(
                new EventId("evt-chibi"),
                new WTimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(5, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("historical-chibi"),
                new HistoricalOutcome("sun-fell-early"),
                new[] { new EventId("evt-yiling") });
            var yiling = new HistoricalEvent(
                new EventId("evt-yiling"),
                new WTimeWindow(new WorldTime(1, DaySegment.Dawn), new WorldTime(8, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(Sun) },
                new HistoricalOutcome("historical-yiling"),
                new HistoricalOutcome("yiling-diverged"),
                Array.Empty<EventId>());
            return HistoricalEventCatalog.TryCreate(new[] { chibi, yiling }).Value!;
        }

        private static PromotionLadderConfig BuildLadder()
        {
            // 低门槛梯队：阶1（merit40/renown10/standing0.01）一次战功即可达；阶2+ 暂不可达（harness 体验晋升即可）。
            var merit = new[] { 0, 40, 9999, 9999, 9999, 9999, 9999, 9999 };
            var renown = new[] { 0, 10, 9999, 9999, 9999, 9999, 9999, 9999 };
            var standing = new[]
            {
                FixedPoint.Zero, Frac(1, 100), FixedPoint.One, FixedPoint.One,
                FixedPoint.One, FixedPoint.One, FixedPoint.One, FixedPoint.One,
            };
            var gains = new Dictionary<CareerGainSource, CareerGain>
            {
                [CareerGainSource.CombatVictory] = new CareerGain(40, 10, Frac(2, 100)),
                [CareerGainSource.MajorBattleVictory] = new CareerGain(80, 50, Frac(5, 100)),
                [CareerGainSource.LordMissionComplete] = new CareerGain(45, 15, Frac(3, 100)),
                [CareerGainSource.CityGovernance] = new CareerGain(40, 12, Frac(2, 100)),
                [CareerGainSource.RebellionSuppressed] = new CareerGain(60, 40, Frac(4, 100)),
                [CareerGainSource.TalentRecruited] = new CareerGain(20, 35, Frac(2, 100)),
            };
            return new PromotionLadderConfig(merit, renown, standing, gains);
        }
    }
}
