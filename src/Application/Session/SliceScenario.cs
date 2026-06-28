using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
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
    /// slice 会话的<b>场景组装器</b>：读取不可变数据源 <see cref="SliceScenarioData"/> 并构造 Domain 聚合
    /// （ADR-0003：平衡数值集中于数据，逻辑不硬编码）。会话编排逻辑（<see cref="GameSession"/>）只读这些属性，
    /// 本类方法体内<b>无魔法数字</b>——全部字面值来自注入的数据源（收尾 CON-5：数据/逻辑分离）。
    /// 量产期可由 ScriptableObject→不可变配置管线产出 <see cref="SliceScenarioData"/> 替换 <see cref="SliceScenarioData.Default"/>。
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

        // ---- 人物花名册（关键人物，GDD_005）----
        /// <summary>关键人物（主将/军师/外勤武将/敌将；展示其能力/性格/职责/健康）。</summary>
        public IReadOnlyList<CharacterState> Roster { get; }

        // ---- 袭扰敌补给（断粮疲敌第二取胜路线，GDD_010/012）----
        /// <summary>一次袭扰消耗的城内粮草（资源代价）。</summary>
        public long RaidStockCost { get; }
        /// <summary>袭扰成功对敌真实兵力的削减（断其粮道，疲敌）。</summary>
        public int RaidEnemyDamage { get; }
        /// <summary>袭扰暴露基础概率（[0,1] 定点）。</summary>
        public FixedPoint RaidExposureBase { get; }
        /// <summary>袭扰者能力对暴露概率的折减权重（≥0）。</summary>
        public FixedPoint RaidSkillWeight { get; }
        /// <summary>袭扰者能力（[0,1]，取外勤武勇归一）。</summary>
        public FixedPoint RaidCapability { get; }
        /// <summary>袭扰暴露时的民心损耗（袭扰队受挫）。</summary>
        public int RaidExposureMoralePenalty { get; }
        /// <summary>敌兵力降至此阈值及以下 → 敌疲退兵（断粮疲敌取胜）。</summary>
        public int EnemyWithdrawThreshold { get; }
        /// <summary>袭扰判定随机流种子（确定性，位置可存档）。</summary>
        public ulong RaidRngSeed { get; }
        /// <summary>袭扰队往返见效所需时段数（≥1；营地距离，非点击即到）。</summary>
        public int RaidLeadSegments { get; }

        // ---- 侦察行军时延（GDD_007；派出→在途→返报，非即时暴露）----
        /// <summary>侦察队往返返报所需时段数（≥1；营地距离，避免即时暴露敌情）。</summary>
        public int ScoutLeadSegments { get; }

        // ---- 假退伏击（第三取胜路线，GDD_010；一次性高风险决战赌注）----
        /// <summary>设伏诱敌的工事代价（示弱开口诱敌，降工事；非战斗投入）。</summary>
        public int AmbushFortCost { get; }
        /// <summary>设伏→诱敌→发动所需时段数（≥1）。</summary>
        public int AmbushLeadSegments { get; }
        /// <summary>敌将是否性烈易诱（成立的非战斗前提：来自敌将性格，花名册可见）。不成立则诱敌必败。</summary>
        public bool EnemyGeneralRash { get; }
        /// <summary>伏击得手对敌真实兵力的重创（早发动可一举击溃；晚发动敌已壮大仅重挫）。</summary>
        public int AmbushSuccessDamage { get; }
        /// <summary>伏击暴露/失败基础概率（[0,1]）。</summary>
        public FixedPoint AmbushExposureBase { get; }
        /// <summary>主将统御对伏击成败的折减权重（≥0）。</summary>
        public FixedPoint AmbushSkillWeight { get; }
        /// <summary>设伏主将能力（[0,1]，取守将统御归一）。</summary>
        public FixedPoint AmbushCapability { get; }
        /// <summary>伏击失败的民心损耗（示弱失策，军心受挫）。</summary>
        public int AmbushFailMoralePenalty { get; }
        /// <summary>伏击得手的民心提振（大捷鼓舞）。</summary>
        public int AmbushSuccessMoraleBonus { get; }
        /// <summary>伏击判定随机流种子（确定性，位置可存档）。</summary>
        public ulong AmbushRngSeed { get; }

        /// <summary>
        /// 从不可变数据源组装 slice 场景（ADR-0003）。本构造仅做<b>数据→Domain 聚合</b>的映射组装，无平衡数字。
        /// </summary>
        public SliceScenario(SliceScenarioData data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            // ---- 时间 ----
            Start = data.Start;
            ReliefDay = data.ReliefDay;

            // ---- 城市 ----
            InitialCity = new CityEconomyState(
                id: new CityId(data.CityId),
                stock: data.CityStock,
                reserved: data.CityReserved,
                civMorale: data.CityCivMorale,
                security: data.CitySecurity,
                fortificationCurrent: data.CityFortCurrent,
                fortificationMax: data.CityFortMax);
            CityConfig = new CitySettlementConfig(
                baseYield: data.BaseYield,
                baseCivConsume: data.BaseCivConsume,
                baseMaintenance: data.BaseMaintenance,
                stockFloor: data.StockFloor,
                civMoraleMax: data.CivMoraleMax,
                shortageMoralePenalty: data.ShortageMoralePenalty,
                unrestShortageThreshold: data.UnrestShortageThreshold,
                fortRepairRate: data.FortRepairRate);
            InitialLogistics = data.InitialLogistics;
            PopulationPressure = data.PopulationPressure;

            // ---- 情报 ----
            PlayerFaction = new FactionId(data.PlayerFactionId);
            EnemyFaction = new FactionId(data.EnemyFactionId);
            EnemySubject = new IntelSubjectId(data.EnemySubjectId);
            EnemyInitialStrength = data.EnemyInitialStrength;
            EnemyReinforcePerDay = data.EnemyReinforcePerDay;

            // ---- 外交 ----
            DiplomacyPower = new ForeignPowerId(data.DiplomacyPowerId);
            DiplomacyConfig = new DiplomacyConfig(
                baseGrant: data.DiploBaseGrant,
                weightStanding: data.DiploWeightStanding,
                weightCost: data.DiploWeightCost,
                weightPressure: data.DiploWeightPressure,
                acceptThreshold: data.DiploAcceptThreshold,
                conditionalThreshold: data.DiploConditionalThreshold,
                costNormalizer: data.DiploCostNormalizer,
                commitLeadSegments: data.DiploCommitLeadSegments,
                betrayRiskBase: data.DiploBetrayRiskBase,
                betrayPressureWeight: data.DiploBetrayPressureWeight,
                betrayalStandingPenalty: data.DiploBetrayalStandingPenalty);
            DiplomacyPledgeCost = data.DiplomacyPledgeCost;
            DiplomacySupplyAmount = data.DiplomacySupplyAmount;
            DiplomacyStanding = data.DiplomacyStanding;
            DiplomacyPressure = data.DiplomacyPressure;
            DiplomacyRngSeed = data.DiplomacyRngSeed;

            // ---- 军议 ----
            Advisor = new AdvisorPerspective(new AdvisorId(data.AdvisorId), data.AdvisorCapability);
            CouncilConfig = new CouncilConfig(data.GapDetectionWeight);
            KnownClaimConfidence = data.KnownClaimConfidence;
            var enemyRef = new[] { EnemySubject };
            var advice = new List<AdviceTemplate>(data.AdviceSpecs.Count);
            foreach (SliceScenarioData.AdviceSpec spec in data.AdviceSpecs)
                advice.Add(new AdviceTemplate(
                    spec.CandidateId, spec.Observation, spec.Assumption,
                    spec.RequiredConditions, spec.Risks, enemyRef));
            AdviceTemplates = advice;

            // ---- 袭扰敌补给 ----
            RaidStockCost = data.RaidStockCost;
            RaidEnemyDamage = data.RaidEnemyDamage;
            RaidExposureBase = data.RaidExposureBase;
            RaidSkillWeight = data.RaidSkillWeight;
            RaidCapability = data.RaidCapability;
            RaidExposureMoralePenalty = data.RaidExposureMoralePenalty;
            EnemyWithdrawThreshold = data.EnemyWithdrawThreshold;
            RaidRngSeed = data.RaidRngSeed;
            RaidLeadSegments = data.RaidLeadSegments;

            // ---- 侦察行军时延 ----
            ScoutLeadSegments = data.ScoutLeadSegments;

            // ---- 假退伏击 ----
            AmbushFortCost = data.AmbushFortCost;
            AmbushLeadSegments = data.AmbushLeadSegments;
            EnemyGeneralRash = data.EnemyGeneralRash;
            AmbushSuccessDamage = data.AmbushSuccessDamage;
            AmbushExposureBase = data.AmbushExposureBase;
            AmbushSkillWeight = data.AmbushSkillWeight;
            AmbushCapability = data.AmbushCapability;
            AmbushFailMoralePenalty = data.AmbushFailMoralePenalty;
            AmbushSuccessMoraleBonus = data.AmbushSuccessMoraleBonus;
            AmbushRngSeed = data.AmbushRngSeed;

            // ---- 人物花名册 ----
            var roster = new List<CharacterState>(data.CharacterSpecs.Count);
            foreach (SliceScenarioData.CharacterSpec spec in data.CharacterSpecs)
                roster.Add(MakeCharacter(spec));
            Roster = roster;
        }

        // 人物构造助手：能力五域百分值 + 性格倾向（十分制→[-1,1] 定点）+ 健康。
        private static CharacterState MakeCharacter(SliceScenarioData.CharacterSpec spec)
        {
            IReadOnlyList<int> caps = spec.Capabilities;
            var capMap = new Dictionary<CapabilityDomain, int>
            {
                [CapabilityDomain.Command] = caps[0],
                [CapabilityDomain.Valor] = caps[1],
                [CapabilityDomain.Strategy] = caps[2],
                [CapabilityDomain.Governance] = caps[3],
                [CapabilityDomain.Diplomacy] = caps[4],
            };
            var traitMap = new Dictionary<PersonalityTrait, FixedPoint>();
            foreach (var (trait, ten) in spec.Traits)
                traitMap[trait] = FixedPoint.FromFraction(ten, 10);

            FixedPoint factor = spec.Health == HealthLevel.Healthy ? FixedPoint.One
                : spec.Health == HealthLevel.Injured ? FixedPoint.FromFraction(7, 10)
                : FixedPoint.Zero;

            return new CharacterState(
                new CharacterId(spec.Identity), spec.Identity,
                new CapabilitySet(capMap), new PersonalityProfile(traitMap),
                new HealthState(spec.Health, factor), new RoleId(spec.RoleId));
        }

        /// <summary>slice 默认场景（从 <see cref="SliceScenarioData.Default"/> 组装；确定性初值）。</summary>
        public static SliceScenario Default() => new SliceScenario(SliceScenarioData.Default);
    }
}
