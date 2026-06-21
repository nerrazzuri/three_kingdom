// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 所有平衡值数据驱动，绝不硬编码于方法体（设计锁 / ADR-0003）
// Date: 2026-06-21

using TkSlice.Domain.Numerics;

namespace TkSlice.Domain.Config
{
    /// <summary>
    /// 切片不可变平衡配置。量产中由 ScriptableObject 编辑期 → 构建时转不可变（ADR-0003）；
    /// 切片直接以一个不可变对象承载，方法体不得出现魔法数。
    /// 全部权威数值用整数或定点 Fixed。
    /// </summary>
    public sealed class SiegeConfig
    {
        // --- 战斗结算（GDD_010）---
        /// <summary>士气对战力的权重。combat_power 因子之一。</summary>
        public Fixed MoraleWeight { get; init; } = Fixed.FromFraction(35, 100);
        /// <summary>疲劳对战力的负权重。</summary>
        public Fixed FatigueWeight { get; init; } = Fixed.FromFraction(25, 100);
        /// <summary>军纪对战力的权重。</summary>
        public Fixed DisciplineWeight { get; init; } = Fixed.FromFraction(20, 100);
        /// <summary>补给充足度对战力的权重。</summary>
        public Fixed SupplyWeight { get; init; } = Fixed.FromFraction(20, 100);
        /// <summary>守城工事对防守方战力的加成上限系数。</summary>
        public Fixed FortificationBonus { get; init; } = Fixed.FromFraction(60, 100);
        /// <summary>伏击突然性乘性加成（GDD_010 §4，受条件链门控）。</summary>
        public Fixed AmbushMultiplier { get; init; } = Fixed.FromFraction(60, 100);
        /// <summary>被伏击方因措手不及损失的战力比例（突然性压制，GDD_010 §4）。</summary>
        public Fixed AmbushDefenderPenalty { get; init; } = Fixed.FromFraction(45, 100);
        /// <summary>判定胜负所需的战力比优势阈值（power_ratio ≥ 此值 → 决定性胜）。</summary>
        public Fixed DecisiveRatio { get; init; } = Fixed.FromFraction(130, 100);

        // --- 断粮传导（GDD_012 §5 + GDD_011）---
        /// <summary>断粮后果触发的宽限时段数。</summary>
        public int SupplyGracePeriod { get; init; } = 3;
        /// <summary>每个短缺时段 supply_state 的衰减量。</summary>
        public Fixed SupplyDecayPerSegment { get; init; } = Fixed.FromFraction(12, 100);
        /// <summary>断粮事件对 unit_morale 的每时段惩罚（GDD_011 唯一施加点）。</summary>
        public Fixed StarveMoralePenalty { get; init; } = Fixed.FromFraction(8, 100);
        /// <summary>断粮事件对 fatigue 的每时段增量（GDD_011 唯一施加点）。</summary>
        public Fixed StarveFatigueGain { get; init; } = Fixed.FromFraction(6, 100);
        /// <summary>士气低于此阈值时部队开始溃逃减员（GDD_011 失败态）。</summary>
        public Fixed DesertMoraleThreshold { get; init; } = Fixed.FromFraction(40, 100);
        /// <summary>溃逃每时段流失的兵力比例。</summary>
        public Fixed DesertRatePerSegment { get; init; } = Fixed.FromFraction(6, 100);

        // --- 袭扰断粮线（双边博弈：袭扰强度 vs 敌军护卫/补给推进）---
        /// <summary>单支袭扰队提供的切断强度。</summary>
        public Fixed RaidStrengthPerUnit { get; init; } = Fixed.FromFraction(30, 100);
        /// <summary>敌军粮道护卫强度（基线防护；袭扰须压过它才易得手）。</summary>
        public Fixed EnemyEscortStrength { get; init; } = Fixed.FromFraction(50, 100);
        /// <summary>切断概率对（袭扰强度−护卫）优势的斜率。</summary>
        public Fixed CutContestSlope { get; init; } = Fixed.FromFraction(80, 100);
        /// <summary>切断失败的时段，敌军补给车队推进回补的补给量。</summary>
        public Fixed EnemyResupplyRestore { get; init; } = Fixed.FromFraction(10, 100);
        /// <summary>守城方因抽调袭扰队产生的工事削弱（每支袭扰队）。</summary>
        public Fixed RaidGarrisonWeaken { get; init; } = Fixed.FromFraction(7, 100);

        // --- 敌军自身援军（两边都有援军；制造消耗赛时间压力）---
        /// <summary>敌军自己的援军抵达时段（拖过此刻则敌军回血）。</summary>
        public int EnemyReinforceSegment { get; init; } = 14;
        /// <summary>敌军援军兵力。</summary>
        public int EnemyReinforceTroops { get; init; } = 300;
        /// <summary>敌军援军抵达时的士气回升。</summary>
        public Fixed EnemyReinforceMorale { get; init; } = Fixed.FromFraction(15, 100);

        // --- 情报雾（GDD_007 真值/知识分离）---
        /// <summary>侦察后情报置信度。</summary>
        public Fixed ScoutConfidence { get; init; } = Fixed.FromFraction(85, 100);
        /// <summary>侦察估计的最大误差幅度（置信越低、估计越糊）。</summary>
        public Fixed ScoutErrorBand { get; init; } = Fixed.FromFraction(12, 100);
        /// <summary>每时段情报置信度的衰减（情报会过时）。</summary>
        public Fixed IntelDecayPerSegment { get; init; } = Fixed.FromFraction(7, 100);
        /// <summary>开局对敌情的初始（模糊）置信度。</summary>
        public Fixed IntelInitialConfidence { get; init; } = Fixed.FromFraction(30, 100);

        // --- 假退伏击（GDD_005 性格 + GDD_010 §4 + GDD_011 军纪）---
        /// <summary>压住佯退、使其像真溃退所需的军纪下限（低于则弄假成真，变真溃逃）。</summary>
        public Fixed FeignDisciplineThreshold { get; init; } = Fixed.FromFraction(50, 100);
        /// <summary>敌将追击意愿基线。</summary>
        public Fixed PursuitBase { get; init; } = Fixed.FromFraction(30, 100);
        /// <summary>鲁莽倾向对追击意愿的权重。</summary>
        public Fixed PursuitRecklessWeight { get; init; } = Fixed.FromFraction(60, 100);
        /// <summary>伏击疑虑（暴露）对追击意愿的负权重。</summary>
        public Fixed PursuitSuspicionWeight { get; init; } = Fixed.FromFraction(70, 100);
        /// <summary>敌将决定追击的意愿阈值。</summary>
        public Fixed PursuitThreshold { get; init; } = Fixed.FromFraction(50, 100);
        /// <summary>伏兵每等待一时段累积的暴露（越久越易被识破）。</summary>
        public Fixed AmbushExposurePerSegment { get; init; } = Fixed.FromFraction(12, 100);

        // --- 外交受控入口（GDD_012 §8）---
        /// <summary>守城每时段对城市民心/外交压力的累积（守城待变的代价）。</summary>
        public Fixed SiegePressurePerSegment { get; init; } = Fixed.FromFraction(3, 100);
        /// <summary>外援基础响应分。</summary>
        public Fixed DiplomacyBaseGrant { get; init; } = Fixed.FromFraction(35, 100);
        /// <summary>声望对响应分的权重。</summary>
        public Fixed DiplomacyStandingWeight { get; init; } = Fixed.FromFraction(40, 100);
        /// <summary>承诺代价对响应分的权重。</summary>
        public Fixed DiplomacyCostWeight { get; init; } = Fixed.FromFraction(25, 100);
        /// <summary>接受请求的响应分阈值。</summary>
        public Fixed DiplomacyAcceptThreshold { get; init; } = Fixed.FromFraction(55, 100);
        /// <summary>外援交付延迟（时段），绝非即到。</summary>
        public int DiplomacyCommitLead { get; init; } = 4;
        /// <summary>背约概率（r &lt; betray_risk → 背约/迟到）。</summary>
        public Fixed DiplomacyBetrayRisk { get; init; } = Fixed.FromFraction(20, 100);
        /// <summary>援军交付的兵力数。</summary>
        public int DiplomacyReliefTroops { get; init; } = 550;

        public static SiegeConfig Default() => new SiegeConfig();
    }
}
