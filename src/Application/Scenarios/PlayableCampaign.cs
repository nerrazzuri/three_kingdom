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
        public static readonly FactionId LiJue = new FactionId("faction-lijue");
        public static readonly FactionId ZhangXiu = new FactionId("faction-zhangxiu");
        public static readonly FactionId KongRong = new FactionId("faction-kongrong");
        public static readonly FactionId HanSui = new FactionId("faction-hansui");
        public static readonly FactionId ShiXie = new FactionId("faction-shixie");

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
        public static readonly CityId Juancheng = new CityId("city-juancheng");
        public static readonly CityId Pingyuan = new CityId("city-pingyuan");
        public static readonly CityId Jinyang = new CityId("city-jinyang");
        public static readonly CityId Runan = new CityId("city-runan");
        public static readonly CityId Kuaiji = new CityId("city-kuaiji");
        public static readonly CityId Lujiang = new CityId("city-lujiang");
        public static readonly CityId Xuzhou = new CityId("city-xuzhou");
        public static readonly CityId Jiangling = new CityId("city-jiangling");
        public static readonly CityId Jiangxia = new CityId("city-jiangxia");
        public static readonly CityId Changsha = new CityId("city-changsha");
        public static readonly CityId Jiangzhou = new CityId("city-jiangzhou");
        public static readonly CityId Zitong = new CityId("city-zitong");
        public static readonly CityId Wuwei = new CityId("city-wuwei");
        public static readonly CityId Jicheng = new CityId("city-jicheng");
        public static readonly CityId Changan = new CityId("city-changan");
        public static readonly CityId Luoyang = new CityId("city-luoyang");
        public static readonly CityId Wancheng = new CityId("city-wancheng");
        public static readonly CityId Beihai = new CityId("city-beihai");
        public static readonly CityId Hanyang = new CityId("city-hanyang");
        public static readonly CityId Jiaozhou = new CityId("city-jiaozhou");
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

        /// <summary>取某武将的气质标签（GDD_025 档案）；无档案则空——随将带入战斗发作。</summary>
        private static IReadOnlyList<GeneralTag> TagsOf(CharacterId c)
            => GeneralDossiers.Find(c)?.Tags is { } t ? new List<GeneralTag>(t) : new List<GeneralTag>();

        /// <summary>取某武将的战阵档（GDD_025 档案）；无档案则 null（无加成）。</summary>
        private static CombatTier? ProwessOf(CharacterId c) => GeneralDossiers.Find(c)?.Prowess;

        /// <summary>取某武将的谋略档（GDD_025 档案）；无档案则 null（不放大兵法加成）。</summary>
        private static StrategyTier? StrategyOf(CharacterId c) => GeneralDossiers.Find(c)?.Strategy;

        /// <summary>主将（太守亲征）：统率0.7/武勇0.7/智略0.6，善攻坚 + 携档案气质标签 + 战阵档 + 谋略档（GDD_025）。</summary>
        public OffensiveGeneral LeadGeneral
            => new OffensiveGeneral(Lord, Frac(7, 10), Frac(7, 10), Frac(6, 10), GeneralSpecialty.Siege, TagsOf(Lord), ProwessOf(Lord), StrategyOf(Lord));

        /// <summary>可选副将花名册（GDD_014 僚属；副将 Aide 智略高·善奇袭，利设伏路线）。</summary>
        public IReadOnlyList<OffensiveGeneral> DeputyRoster
            => new[] { new OffensiveGeneral(Aide, Frac(5, 10), Frac(6, 10), Frac(8, 10), GeneralSpecialty.Ambush, TagsOf(Aide), ProwessOf(Aide), StrategyOf(Aide)) };

        /// <summary>目标敌城的<b>真实</b>守备（结算用真值；玩家所见须经情报投影，反全知）。虎牢关：守军600 × 工事1.2。</summary>
        public SiegeDefense DefenseOf(CityId city) => new SiegeDefense(600, Frac(12, 10));

        /// <summary>目标进攻路线地形（#3 逐城/地形：由开局指定——虎牢关隘口/下邳坚城/江夏渡口；决定战场正面区与攻坚难度）。</summary>
        public TerrainKind TerrainOf(CityId city)
            => city == _start.OffensiveTarget ? _start.TargetTerrain : TerrainKind.Fortified;

        /// <summary>可出征目标城清单（GDD_019 §7 选目标；授权门/敌控由运行期按会话控制权投影判定）。取自开局首要目标。</summary>
        public IReadOnlyList<CityId> OffensiveTargetCities => new[] { _start.OffensiveTarget };

        /// <summary>某城的初始控制方（供外交战争约束按目标势力判立场）：目标城→目标势力，治所→玩家势力，余则查世界大盘。</summary>
        public FactionId? DefendingFactionOf(CityId city)
        {
            if (city == _start.OffensiveTarget) return _start.TargetFaction;
            if (city == _start.Capital) return _start.PlayerFaction;
            foreach (SeedFaction w in World)
                foreach ((CityId c, int _) in w.Cities)
                    if (c == city) return w.Faction;
            return null;
        }

        /// <summary>
        /// 初始群雄争霸态（GDD_017，群雄割据世界骨架）：玩家太守 1 城，天下 17 城分于 12 势力——
        /// 玩家支配度低（远未统一），争霸→终局成真正战略盘（修正竖切"一开局即统一"）。城数与世界模型城归属一致。
        /// </summary>
        public Domain.Contention.ContentionState InitialContention()
        {
            var standings = new List<Domain.Contention.PowerStanding>();
            foreach (SeedFaction w in World)
            {
                if (w.Bespoke && !_start.IncludesBespokeSeat) continue;
                standings.Add(new Domain.Contention.PowerStanding(w.Faction, w.Cities.Length));
            }
            return new Domain.Contention.ContentionState(standings.ToArray());
        }

        // ---- 共享天下大盘（17 席世界骨架；#1 数据源，默认剧本与各势力开局共用）。玩家席位由 PlayableStart 指定，仅关系/治所随之重定向。----
        private readonly struct SeedFaction
        {
            public readonly FactionId Faction;
            public readonly CharacterId Lord;
            public readonly RelationToPlayer BaseRelation;   // 相对"汜水关太守"默认剧本的基线立场
            public readonly bool Bespoke;                    // 太守专属独立席（仅默认剧本含）
            public readonly (CityId City, int Garrison)[] Cities;
            public SeedFaction(FactionId faction, string lord, RelationToPlayer baseRelation, bool bespoke, (CityId, int)[] cities)
            {
                Faction = faction;
                Lord = new CharacterId(lord);
                BaseRelation = baseRelation;
                Bespoke = bespoke;
                Cities = cities;
            }
        }

        private static readonly SeedFaction[] World =
        {
            new SeedFaction(Player, "char-player-lord", RelationToPlayer.Self, true, new[] { (Fanshui, 800) }),
            new SeedFaction(Cao, "char-caocao", RelationToPlayer.Neutral, false, new[] { (Xuchang, 900), (Puyang, 600), (Chenliu, 600), (Juancheng, 500) }),
            new SeedFaction(YuanShao, "char-yuanshao", RelationToPlayer.Neutral, false, new[] { (Ye, 900), (Nanpi, 700), (Pingyuan, 500), (Jinyang, 500) }),
            new SeedFaction(Enemy, "char-yuan", RelationToPlayer.Hostile, false, new[] { (Shouchun, 700), (EnemyCity, 600), (Runan, 500) }),
            new SeedFaction(Sun, "char-sunce", RelationToPlayer.Neutral, false, new[] { (Jianye, 700), (Wujun, 500), (Kuaiji, 500), (Lujiang, 400) }),
            new SeedFaction(LiuBei, "char-liubei", RelationToPlayer.Neutral, false, new[] { (Xiaopei, 400) }),
            new SeedFaction(LuBu, "char-lubu", RelationToPlayer.Hostile, false, new[] { (Xiapi, 700), (Xuzhou, 600) }),
            new SeedFaction(LiuBiao, "char-liubiao", RelationToPlayer.Neutral, false, new[] { (Xiangyang, 800), (Jiangling, 600), (Jiangxia, 500), (Changsha, 500) }),
            new SeedFaction(LiuZhang, "char-liuzhang", RelationToPlayer.Neutral, false, new[] { (Chengdu, 800), (Jiangzhou, 500), (Zitong, 400) }),
            new SeedFaction(MaTeng, "char-mateng", RelationToPlayer.Neutral, false, new[] { (Xiliang, 700), (Wuwei, 500) }),
            new SeedFaction(ZhangLu, "char-zhanglu", RelationToPlayer.Neutral, false, new[] { (Hanzhong, 600) }),
            new SeedFaction(GongSun, "char-gongsun", RelationToPlayer.Neutral, false, new[] { (Beiping, 700), (Jicheng, 500) }),
            new SeedFaction(LiJue, "char-lijue", RelationToPlayer.Hostile, false, new[] { (Changan, 700), (Luoyang, 600) }),
            new SeedFaction(ZhangXiu, "char-zhangxiu", RelationToPlayer.Hostile, false, new[] { (Wancheng, 600) }),
            new SeedFaction(KongRong, "char-kongrong", RelationToPlayer.Neutral, false, new[] { (Beihai, 400) }),
            new SeedFaction(HanSui, "char-hansui", RelationToPlayer.Neutral, false, new[] { (Hanyang, 500) }),
            new SeedFaction(ShiXie, "char-shixie", RelationToPlayer.Neutral, false, new[] { (Jiaozhou, 400) }),
        };

        /// <summary>某势力相对本局玩家的立场：玩家席=Self，首要目标=Hostile，其余保基线（非玩家的专属席退为中立）。</summary>
        private RelationToPlayer RelationOf(SeedFaction w)
        {
            if (w.Faction == _start.PlayerFaction) return RelationToPlayer.Self;
            if (w.Faction == _start.TargetFaction) return RelationToPlayer.Hostile;
            return w.BaseRelation == RelationToPlayer.Self ? RelationToPlayer.Neutral : w.BaseRelation;
        }

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

        // ---- 本局玩家席位（#1 运行期身份，取自开局 PlayableStart；默认=汜水关太守）。运行期一律读此，不再硬编码单一场景。----
        private readonly PlayableStart _start;

        /// <summary>本局开局描述（选择屏/HUD）。</summary>
        public PlayableStart Start => _start;
        /// <summary>本局锚点年（公元；GDD_026 纪元起点，默认 190 讨董）。</summary>
        public int AnchorYear => _start.AnchorYear;
        /// <summary>本局玩家所属势力（默认 faction-player）。</summary>
        public FactionId PlayerFaction => _start.PlayerFaction;
        /// <summary>本局玩家治所（默认汜水关）。</summary>
        public CityId PlayerCapital => _start.Capital;
        /// <summary>本局玩家君主/本人（默认 char-player-lord）。</summary>
        public CharacterId PlayerLord => _start.PlayerLord;
        /// <summary>本局首要出征目标城（默认虎牢关）。</summary>
        public CityId PlayerOffensiveTarget => _start.OffensiveTarget;
        /// <summary>本局首要目标所属势力（默认袁术）。</summary>
        public FactionId PlayerTargetFaction => _start.TargetFaction;

        private PlayableCampaign(PlayableStart start)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
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

        /// <summary>构造默认可玩场景（汜水关太守）。</summary>
        public static PlayableCampaign Default() => new PlayableCampaign(PlayableStartCatalog.Default);

        /// <summary>按所选开局构造可玩场景（#1 势力选择；共享天下大盘，仅玩家席位不同）。</summary>
        public static PlayableCampaign ForStart(PlayableStart start) => new PlayableCampaign(start);

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

        private CampaignStartConfig BuildStartConfig()
        {
            // 由共享天下大盘按本局玩家席位生成势力/城归属（#1）：玩家席=Self、目标=Hostile；扮演既有诸侯时不含太守专属席。
            var factions = new List<FactionRecord>();
            var cities = new List<CityOwnership>();
            foreach (SeedFaction w in World)
            {
                if (w.Bespoke && !_start.IncludesBespokeSeat) continue;
                var owned = new List<CityId>();
                foreach ((CityId city, int garrison) in w.Cities)
                {
                    owned.Add(city);
                    cities.Add(new CityOwnership(city, w.Faction, garrison));
                }
                factions.Add(new FactionRecord(w.Faction, w.Lord, SurvivalStatus.Active, RelationOf(w), owned.ToArray()));
            }

            return new CampaignStartConfig(
                scenarioConfigId: _start.Id,
                fingerprint: Fp,
                governorSeed: new CitySeed(_start.PlayerFaction, _start.Capital, _start.CapitalGarrison, 60, 20, new[] { new RetinueMember(Aide, Frac(6, 10)) }),
                startTime: new WorldTime(0, DaySegment.Dawn),
                initialFactions: factions.ToArray(),
                initialCities: cities.ToArray(),
                // 城市治理（M03）：库存100 / 民心60 / 城防20。
                cityEconomy: new CityEconomyState(_start.Capital, stock: 100, reserved: 0, civMorale: 60, security: 50, fortificationCurrent: 20, fortificationMax: 100),
                settlementConfig: new CitySettlementConfig(
                    baseYield: 20, baseCivConsume: 30, baseMaintenance: 10, stockFloor: 0,
                    civMoraleMax: 100, shortageMoralePenalty: Frac(1, 2), unrestShortageThreshold: 50, fortRepairRate: 15),
                populationPressure: FixedPoint.FromInt(1),
                initialLogisticsHolding: 0,
                governanceConfig: new CityGovernanceConfig(Frac(1, 2), 10, 10),
                // 情报（M04）：敌军主力真值（玩家初始未知，须侦察）。
                worldTruth: BuildTruth(),
                playerIntel: new FactionIntel(_start.PlayerFaction),
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
        }

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
            // 完整晋升曲线（太守→上守→刺史→镇将→护军→副都督→大都督→继承基业）：门槛递增，
            // 忠臣晋升线成真长期成长（首阶一战即达，问鼎需数十战功 + 高君主好感）。
            var merit = new[] { 0, 40, 120, 280, 550, 950, 1500, 2400 };
            var renown = new[] { 0, 10, 60, 150, 320, 600, 1000, 1600 };
            var standing = new[]
            {
                FixedPoint.Zero, Frac(1, 100), Frac(15, 100), Frac(30, 100),
                Frac(45, 100), Frac(60, 100), Frac(75, 100), Frac(90, 100),
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
