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
using ThreeKingdom.Domain.Talent;
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
        /// <summary>可出征攻打的敌方目标城（虎牢关，袁术控制；GDD_019 出征目标）。</summary>
        public static readonly CityId EnemyCity = new CityId("city-hulao");

        // ---- 群雄割据世界骨架（2026-07-05 内容扩充）：让争霸→终局有真实天下（玩家仍为汜水关太守，运行期耦合不变）。----
        // 各势力 + 君主 + 城；争霸初盘与世界模型城归属一致。
        public static readonly FactionId Cao = new FactionId("faction-cao");
        public static readonly FactionId YuanShao = new FactionId("faction-yuanshao");
        public static readonly FactionId LiuBei = new FactionId("faction-liubei");
        public static readonly FactionId LuBu = new FactionId("faction-lubu");
        public static readonly FactionId LiuBiao = new FactionId("faction-liubiao");
        public static readonly FactionId MaTeng = new FactionId("faction-mateng");
        public static readonly FactionId LiuZhang = new FactionId("faction-liuzhang");
        public static readonly FactionId ZhangLu = new FactionId("faction-zhanglu");
        public static readonly FactionId GongSun = new FactionId("faction-gongsun");

        public static readonly CityId Xuchang = new CityId("city-xuchang");
        public static readonly CityId Puyang = new CityId("city-puyang");
        public static readonly CityId Chenliu = new CityId("city-chenliu");
        public static readonly CityId Ye = new CityId("city-ye");
        public static readonly CityId Nanpi = new CityId("city-nanpi");
        public static readonly CityId Xiaopei = new CityId("city-xiaopei");
        public static readonly CityId Jianye = new CityId("city-jianye");
        public static readonly CityId Wujun = new CityId("city-wujun");
        public static readonly CityId Shouchun = new CityId("city-shouchun");
        public static readonly CityId Xiapi = new CityId("city-xiapi");
        public static readonly CityId Xiangyang = new CityId("city-xiangyang");
        public static readonly CityId Xiliang = new CityId("city-xiliang");
        public static readonly CityId Chengdu = new CityId("city-chengdu");
        public static readonly CityId Hanzhong = new CityId("city-hanzhong");
        public static readonly CityId Beiping = new CityId("city-beiping");
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

        /// <summary>某城的初始控制方（供外交战争约束按目标势力判立场；MVP 场景映射）。</summary>
        public FactionId? DefendingFactionOf(CityId city)
        {
            if (city == EnemyCity) return Enemy;
            if (city == Fanshui) return Player;
            return null;
        }

        /// <summary>
        /// 初始群雄争霸态（GDD_017，群雄割据世界骨架）：玩家太守 1 城，天下 17 城分于 12 势力——
        /// 玩家支配度低（远未统一），争霸→终局成真正战略盘（修正竖切"一开局即统一"）。城数与世界模型城归属一致。
        /// </summary>
        public Domain.Contention.ContentionState InitialContention()
            => new Domain.Contention.ContentionState(new[]
            {
                new Domain.Contention.PowerStanding(Player, 1),      // 汜水关
                new Domain.Contention.PowerStanding(Cao, 3),         // 许昌/濮阳/陈留
                new Domain.Contention.PowerStanding(YuanShao, 2),   // 邺城/南皮
                new Domain.Contention.PowerStanding(Enemy, 2),      // 袁术：虎牢关/寿春
                new Domain.Contention.PowerStanding(Sun, 2),        // 孙吴：建业/吴郡
                new Domain.Contention.PowerStanding(LiuBiao, 1),    // 襄阳
                new Domain.Contention.PowerStanding(LiuBei, 1),     // 小沛
                new Domain.Contention.PowerStanding(LuBu, 1),       // 下邳
                new Domain.Contention.PowerStanding(MaTeng, 1),     // 西凉
                new Domain.Contention.PowerStanding(LiuZhang, 1),   // 成都
                new Domain.Contention.PowerStanding(ZhangLu, 1),    // 汉中
                new Domain.Contention.PowerStanding(GongSun, 1),    // 北平
            });

        /// <summary>守城区域防御战：玩家守军（汜水关；GDD_021 攻守统一，守方视角）。</summary>
        public int DefenseGarrison => 700;

        /// <summary>守城区域防御战：来犯敌军突击兵力（敌AI攻方）。</summary>
        public int EnemyAssaultForce => 500;

        // ---- 人才招揽（GDD_020）：目录含登场时间窗（反全历史）；单一数据源 ----

        /// <summary>卧龙（智略绝顶·善奇袭；晚登场——第 3 日后才出世，志向低·阻力高，难招）。</summary>
        public static readonly TalentId Wolong = new TalentId("talent-wolong");
        /// <summary>骁将（统率武勇高·善攻坚；开局即在，志向高·易招）。</summary>
        public static readonly TalentId Xiaojiang = new TalentId("talent-xiaojiang");
        /// <summary>能吏（内政/后勤·善辎重；第 1 日后登场，中庸）。</summary>
        public static readonly TalentId Nengli = new TalentId("talent-nengli");

        /// <summary>人才目录（数据驱动，含各人时间窗/属性/志向）。</summary>
        public TalentRoster TalentRoster => new TalentRoster(new[]
        {
            new TalentProfile(Wolong, new CharacterId("char-wolong"), Frac(9, 10), Frac(3, 10), FixedPoint.One,
                GeneralSpecialty.Ambush, willingness: Frac(2, 10), reluctance: Frac(5, 10), appearFrom: new WorldTime(3, DaySegment.Dawn)),
            new TalentProfile(Xiaojiang, new CharacterId("char-xiaojiang"), Frac(8, 10), Frac(9, 10), Frac(4, 10),
                GeneralSpecialty.Siege, willingness: Frac(7, 10), reluctance: Frac(1, 10), appearFrom: new WorldTime(0, DaySegment.Dawn)),
            new TalentProfile(Nengli, new CharacterId("char-nengli"), Frac(5, 10), Frac(3, 10), Frac(7, 10),
                GeneralSpecialty.Logistics, willingness: Frac(5, 10), reluctance: Frac(3, 10), appearFrom: new WorldTime(1, DaySegment.Dawn)),
        });

        /// <summary>招揽概率映射配置。</summary>
        public TalentRecruitmentConfig TalentRecruit => TalentRecruitmentConfig.Default;

        /// <summary>招揽种子基（确定性）。</summary>
        public ulong TalentSeed => 0x7A1E27UL;

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
                    new FactionRecord(Enemy, new CharacterId("char-yuan"), SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { EnemyCity, Shouchun }),
                    new FactionRecord(Sun, new CharacterId("char-sunquan"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Jianye, Wujun }),
                    new FactionRecord(Cao, new CharacterId("char-caocao"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Xuchang, Puyang, Chenliu }),
                    new FactionRecord(YuanShao, new CharacterId("char-yuanshao"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Ye, Nanpi }),
                    new FactionRecord(LiuBei, new CharacterId("char-liubei"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Xiaopei }),
                    new FactionRecord(LuBu, new CharacterId("char-lubu"), SurvivalStatus.Active, RelationToPlayer.Hostile, new[] { Xiapi }),
                    new FactionRecord(LiuBiao, new CharacterId("char-liubiao"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Xiangyang }),
                    new FactionRecord(MaTeng, new CharacterId("char-mateng"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Xiliang }),
                    new FactionRecord(LiuZhang, new CharacterId("char-liuzhang"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Chengdu }),
                    new FactionRecord(ZhangLu, new CharacterId("char-zhanglu"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Hanzhong }),
                    new FactionRecord(GongSun, new CharacterId("char-gongsun"), SurvivalStatus.Active, RelationToPlayer.Neutral, new[] { Beiping }),
                },
                initialCities: new[]
                {
                    new CityOwnership(Fanshui, Player, 800), new CityOwnership(EnemyCity, Enemy, 600), new CityOwnership(Shouchun, Enemy, 500),
                    new CityOwnership(Jianye, Sun, 700), new CityOwnership(Wujun, Sun, 500),
                    new CityOwnership(Xuchang, Cao, 900), new CityOwnership(Puyang, Cao, 600), new CityOwnership(Chenliu, Cao, 600),
                    new CityOwnership(Ye, YuanShao, 800), new CityOwnership(Nanpi, YuanShao, 600),
                    new CityOwnership(Xiaopei, LiuBei, 400), new CityOwnership(Xiapi, LuBu, 700),
                    new CityOwnership(Xiangyang, LiuBiao, 700), new CityOwnership(Xiliang, MaTeng, 600),
                    new CityOwnership(Chengdu, LiuZhang, 800), new CityOwnership(Hanzhong, ZhangLu, 500), new CityOwnership(Beiping, GongSun, 500),
                },
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
            // 够不着的天下大事（袁术称帝）：玩家圈（触孙权）不及袁术 → 只作通报 + 主角心里话（GDD_015 事件分级）。
            // 够不着的天下大事（袁术称帝）：玩家圈（触孙权）不及袁术 → 只作通报 + 主角心里话（GDD_015 事件分级）。
            var yuanshu = new HistoricalEvent(
                new EventId("evt-yuanshu-emperor"),
                new WTimeWindow(new WorldTime(0, DaySegment.Dawn), new WorldTime(4, DaySegment.Dawn)),
                new[] { Precondition.FactionAliveOf(new FactionId("faction-yuanshu")) },
                new HistoricalOutcome("yuanshu-declares-emperor"),
                new HistoricalOutcome("yuanshu-emperor-averted"),
                Array.Empty<EventId>());

            // 演义主线事件网（够不着→通报+心里话，随人设着色；前置势力皆不在玩家圈内）。
            HistoricalEvent Notable(string id, int start, int end, FactionId precond, string outcome, string diverged)
                => new HistoricalEvent(new EventId(id),
                    new WTimeWindow(new WorldTime(start, DaySegment.Dawn), new WorldTime(end, DaySegment.Dawn)),
                    new[] { Precondition.FactionAliveOf(precond) },
                    new HistoricalOutcome(outcome), new HistoricalOutcome(diverged), Array.Empty<EventId>());

            HistoricalEvent taoyuan = Notable("evt-taoyuan", 0, 3, LiuBei, "taoyuan-oath", "taoyuan-averted");
            HistoricalEvent dongBurns = Notable("evt-dong-burns", 0, 3, new FactionId("faction-dong"), "dong-zhuo-burns-luoyang", "luoyang-spared");
            HistoricalEvent wangyun = Notable("evt-wangyun-plot", 1, 4, LuBu, "wang-yun-chain-plot", "dong-zhuo-survives");
            HistoricalEvent caoEmperor = Notable("evt-cao-emperor", 2, 6, Cao, "cao-cao-controls-emperor", "emperor-free");
            HistoricalEvent guandu = Notable("evt-guandu", 3, 7, YuanShao, "guandu-cao-wins", "yuanshao-prevails");
            HistoricalEvent lubuEnd = Notable("evt-lubu-executed", 3, 7, LuBu, "lubu-executed", "lubu-survives");
            HistoricalEvent sangu = Notable("evt-sangu", 4, 9, LiuBei, "liu-bei-recruits-zhuge", "zhuge-declines");
            HistoricalEvent guanyu = Notable("evt-guanyu-jingzhou", 6, 12, LiuBei, "guan-yu-loses-jingzhou", "jingzhou-held");

            return HistoricalEventCatalog.TryCreate(new[]
            {
                chibi, yiling, yuanshu,
                taoyuan, dongBurns, wangyun, caoEmperor, guandu, lubuEnd, sangu, guanyu,
            }).Value!;
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
