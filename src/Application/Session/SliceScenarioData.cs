using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// slice 场景的<b>不可变配置数据源</b>（ADR-0003：平衡数值集中于数据，逻辑不硬编码）。
    /// 这是 slice 全部开局禀赋/链条/人物字面值的<b>唯一来源</b>——<see cref="SliceScenario"/> 只读本数据并组装
    /// Domain 聚合，其方法体内无魔法数字。量产期可由 ScriptableObject→不可变配置管线产出本类型实例替换 <see cref="Default"/>。
    /// 本类型为纯数据：仅字段，无规则逻辑（数据/逻辑分离，收尾 CON-5）。
    /// </summary>
    public sealed class SliceScenarioData
    {
        // ---- 时间 ----
        /// <summary>开局世界时间。</summary>
        public WorldTime Start { get; }
        /// <summary>援军抵达日（0 基日序号）。</summary>
        public int ReliefDay { get; }

        // ---- 城市（GDD_004）----
        public string CityId { get; }
        public long CityStock { get; }
        public long CityReserved { get; }
        public int CityCivMorale { get; }
        public int CitySecurity { get; }
        public int CityFortCurrent { get; }
        public int CityFortMax { get; }
        public long BaseYield { get; }
        public long BaseCivConsume { get; }
        public long BaseMaintenance { get; }
        public long StockFloor { get; }
        public int CivMoraleMax { get; }
        public FixedPoint ShortageMoralePenalty { get; }
        public long UnrestShortageThreshold { get; }
        public int FortRepairRate { get; }
        public long InitialLogistics { get; }
        public FixedPoint PopulationPressure { get; }

        // ---- 情报（GDD_007）----
        public string PlayerFactionId { get; }
        public string EnemyFactionId { get; }
        public string EnemySubjectId { get; }
        public int EnemyInitialStrength { get; }
        public int EnemyReinforcePerDay { get; }

        // ---- 外交（GDD_012 §8）----
        public string DiplomacyPowerId { get; }
        public FixedPoint DiploBaseGrant { get; }
        public FixedPoint DiploWeightStanding { get; }
        public FixedPoint DiploWeightCost { get; }
        public FixedPoint DiploWeightPressure { get; }
        public FixedPoint DiploAcceptThreshold { get; }
        public FixedPoint DiploConditionalThreshold { get; }
        public long DiploCostNormalizer { get; }
        public int DiploCommitLeadSegments { get; }
        public FixedPoint DiploBetrayRiskBase { get; }
        public FixedPoint DiploBetrayPressureWeight { get; }
        public int DiploBetrayalStandingPenalty { get; }
        public long DiplomacyPledgeCost { get; }
        public long DiplomacySupplyAmount { get; }
        public FixedPoint DiplomacyStanding { get; }
        public FixedPoint DiplomacyPressure { get; }
        public ulong DiplomacyRngSeed { get; }

        // ---- 军议（GDD_008）----
        public string AdvisorId { get; }
        public FixedPoint AdvisorCapability { get; }
        public FixedPoint GapDetectionWeight { get; }
        public FixedPoint KnownClaimConfidence { get; }
        /// <summary>条件化建议字面值（敌情主题由 <see cref="SliceScenario"/> 统一注入 <see cref="EnemySubjectId"/>）。</summary>
        public IReadOnlyList<AdviceSpec> AdviceSpecs { get; }

        // ---- 人物花名册（GDD_005）----
        public IReadOnlyList<CharacterSpec> CharacterSpecs { get; }

        // ---- 袭扰敌补给（断粮疲敌，GDD_010/012）----
        public long RaidStockCost { get; }
        public int RaidEnemyDamage { get; }
        public FixedPoint RaidExposureBase { get; }
        public FixedPoint RaidSkillWeight { get; }
        public FixedPoint RaidCapability { get; }
        public int RaidExposureMoralePenalty { get; }
        public int EnemyWithdrawThreshold { get; }
        public ulong RaidRngSeed { get; }
        public int RaidLeadSegments { get; }

        // ---- 侦察行军时延（GDD_007）----
        public int ScoutLeadSegments { get; }

        // ---- 假退伏击（GDD_010）----
        public int AmbushFortCost { get; }
        public int AmbushLeadSegments { get; }
        public bool EnemyGeneralRash { get; }
        public int AmbushSuccessDamage { get; }
        public FixedPoint AmbushExposureBase { get; }
        public FixedPoint AmbushSkillWeight { get; }
        public FixedPoint AmbushCapability { get; }
        public int AmbushFailMoralePenalty { get; }
        public int AmbushSuccessMoraleBonus { get; }
        public ulong AmbushRngSeed { get; }

        private SliceScenarioData()
        {
            // ---- 时间 ----
            Start = new WorldTime(0, DaySegment.Dawn);
            ReliefDay = 8; // 第 9 日（0 基 8）援军抵达 = 胜；守不到则可能民心崩溃失城。

            // ---- 城市 ----
            CityId = "汜水关";
            CityStock = 300;
            CityReserved = 0;
            CityCivMorale = 70;
            CitySecurity = 55;
            CityFortCurrent = 60;
            CityFortMax = 100;
            BaseYield = 40;
            BaseCivConsume = 70;
            BaseMaintenance = 20;
            StockFloor = 80;
            CivMoraleMax = 100;
            ShortageMoralePenalty = FixedPoint.FromInt(1);
            UnrestShortageThreshold = 30;
            FortRepairRate = 5;
            InitialLogistics = 0;
            PopulationPressure = FixedPoint.One;

            // ---- 情报 ----
            PlayerFactionId = "玩家势力";
            EnemyFactionId = "曹魏";
            EnemySubjectId = "敌前锋";
            EnemyInitialStrength = 1000;
            EnemyReinforcePerDay = 120;

            // ---- 外交（求粮，GDD_012 §8）：静态背景外势力，延迟交付 + 可背约（确定性随机流）。----
            DiplomacyPowerId = "江东";
            DiploBaseGrant = FixedPoint.FromFraction(2, 5);          // 0.4
            DiploWeightStanding = FixedPoint.FromFraction(1, 2);     // 0.5
            DiploWeightCost = FixedPoint.FromFraction(3, 10);        // 0.3
            DiploWeightPressure = FixedPoint.FromFraction(2, 5);     // 0.4
            DiploAcceptThreshold = FixedPoint.FromFraction(3, 5);    // 0.6
            DiploConditionalThreshold = FixedPoint.FromFraction(2, 5); // 0.4
            DiploCostNormalizer = 100;
            DiploCommitLeadSegments = WorldTime.SegmentsPerDay * 2;  // 两日后抵达
            DiploBetrayRiskBase = FixedPoint.FromFraction(1, 5);     // 0.2
            DiploBetrayPressureWeight = FixedPoint.FromFraction(3, 10); // 0.3
            DiploBetrayalStandingPenalty = 5;
            DiplomacyPledgeCost = 50;
            DiplomacySupplyAmount = 120;        // 兑现则到达时入城粮草
            DiplomacyStanding = FixedPoint.FromFraction(3, 5);       // 0.6
            DiplomacyPressure = FixedPoint.FromFraction(1, 5);       // 0.2
            DiplomacyRngSeed = 0xD17_0ACE_2026UL;

            // ---- 军议（GDD_008）：三条条件化建议，依据敌情主题；并列呈现，无最优解。----
            AdvisorId = "随军军师";
            AdvisorCapability = FixedPoint.FromFraction(7, 10);      // adv_cap 0.7
            GapDetectionWeight = FixedPoint.One;
            KnownClaimConfidence = FixedPoint.FromFraction(1, 2);    // 0.5 已侦察=依据中等
            AdviceSpecs = new List<AdviceSpec>
            {
                new AdviceSpec(
                    "断粮疲敌",
                    "敌前锋深入，补给线拉长。",
                    "若敌补给可被持续袭扰，其战力随时日衰减。",
                    new[] { "需查明敌补给路线与护卫强度", "需投入袭扰兵力且承担暴露风险" },
                    new[] { "袭扰队可能被反伏", "敌可能改道补给" }),
                new AdviceSpec(
                    "守城待变",
                    "援军定于第 9 日抵达。",
                    "若粮草民心可支撑至援军，则不必决战。",
                    new[] { "需粮草撑至援军日", "可向外求粮缓解" },
                    new[] { "久守民心易崩", "敌或在援军前强攻" }),
                new AdviceSpec(
                    "假退伏击",
                    "敌将性烈，易受诱。",
                    "若示弱诱敌冒进，可于隘口设伏。",
                    new[] { "需摸清敌将性格与追击倾向", "需预设伏兵与退路" },
                    new[] { "诱敌不成反失城门", "伏击暴露则两面受敌" }),
            };

            // ---- 袭扰敌补给（断粮疲敌，第二取胜路线）----
            RaidStockCost = 25;
            RaidEnemyDamage = 320;                              // 数次成功可压过敌每日 +120 增援，断粮快于守城
            RaidExposureBase = FixedPoint.FromFraction(11, 20); // 0.55
            RaidSkillWeight = FixedPoint.FromFraction(2, 5);    // 0.4
            RaidCapability = FixedPoint.FromFraction(80, 100);  // 外勤武勇 0.8
            RaidExposureMoralePenalty = 6;
            EnemyWithdrawThreshold = 400;                       // 敌力≤400 → 疲敝退兵（胜）
            RaidRngSeed = 0x5A1D_2026_0001UL;
            RaidLeadSegments = WorldTime.SegmentsPerDay;         // 袭扰队往返约一日见效
            ScoutLeadSegments = 2;                              // 侦察返报约半日（4 时段/日）

            // ---- 假退伏击（第三取胜路线）----
            AmbushFortCost = 25;                                // 示弱开口，降工事（投入/风险）
            AmbushLeadSegments = WorldTime.SegmentsPerDay;       // 设伏诱敌约一日发动
            EnemyGeneralRash = true;                            // 敌将·夏侯烈 性烈（花名册 Risk 高）→ 可诱
            AmbushSuccessDamage = 760;                          // 早发动可一举击溃（计入发动日敌增援后仍压至退兵阈值）
            AmbushExposureBase = FixedPoint.FromFraction(13, 20); // 0.65 基础失败/暴露
            AmbushSkillWeight = FixedPoint.FromFraction(1, 2);  // 0.5
            AmbushCapability = FixedPoint.FromFraction(78, 100); // 守将统御 0.78 → 净失败率≈0.26
            AmbushFailMoralePenalty = 18;                       // 失败重挫民心（高风险）
            AmbushSuccessMoraleBonus = 12;
            AmbushRngSeed = 0xC0FFEEUL; // 基线种子：首次诱敌得手（净失败率≈0.26，此流首掷 0.79）

            // ---- 人物花名册（GDD_005）：关键四人（数据驱动，原创角色，守红线①）。----
            CharacterSpecs = new List<CharacterSpec>
            {
                new CharacterSpec("守将", "守将·秦烈", new[] { 78, 82, 55, 60, 40 }, HealthLevel.Healthy,
                    new[] { (PersonalityTrait.Discipline, 6), (PersonalityTrait.Honor, 5) }),
                new CharacterSpec("军师", "军师·陈疏", new[] { 50, 30, 85, 70, 65 }, HealthLevel.Healthy,
                    new[] { (PersonalityTrait.Patience, 7), (PersonalityTrait.Risk, -3) }),
                new CharacterSpec("外勤", "校尉·方武", new[] { 60, 80, 45, 35, 30 }, HealthLevel.Injured,
                    new[] { (PersonalityTrait.Risk, 6), (PersonalityTrait.Discipline, -2) }),
                new CharacterSpec("敌将", "敌将·夏侯烈", new[] { 75, 88, 40, 30, 25 }, HealthLevel.Healthy,
                    new[] { (PersonalityTrait.Risk, 8), (PersonalityTrait.Patience, -6) }),
            };
        }

        /// <summary>slice 默认数据源（确定性字面值，单一来源）。</summary>
        public static SliceScenarioData Default { get; } = new SliceScenarioData();

        /// <summary>
        /// 条件化建议字面值（GDD_008）。纯数据；敌情主题引用由 <see cref="SliceScenario"/> 统一注入。
        /// </summary>
        public sealed class AdviceSpec
        {
            public string CandidateId { get; }
            public string Observation { get; }
            public string Assumption { get; }
            public IReadOnlyList<string> RequiredConditions { get; }
            public IReadOnlyList<string> Risks { get; }

            public AdviceSpec(
                string candidateId, string observation, string assumption,
                IReadOnlyList<string> requiredConditions, IReadOnlyList<string> risks)
            {
                CandidateId = candidateId;
                Observation = observation;
                Assumption = assumption;
                RequiredConditions = requiredConditions;
                Risks = risks;
            }
        }

        /// <summary>
        /// 人物字面值（GDD_005）。能力为五域百分值（Command/Valor/Strategy/Governance/Diplomacy），
        /// 性格为十分制倾向（[-10,10] → [-1,1] 定点，由 <see cref="SliceScenario"/> 归一）。纯数据。
        /// </summary>
        public sealed class CharacterSpec
        {
            public string RoleId { get; }
            public string Identity { get; }
            public IReadOnlyList<int> Capabilities { get; }
            public HealthLevel Health { get; }
            public IReadOnlyList<(PersonalityTrait Trait, int TenScale)> Traits { get; }

            public CharacterSpec(
                string roleId, string identity, IReadOnlyList<int> capabilities,
                HealthLevel health, IReadOnlyList<(PersonalityTrait, int)> traits)
            {
                RoleId = roleId;
                Identity = identity;
                Capabilities = capabilities;
                Health = health;
                Traits = traits;
            }
        }
    }
}
