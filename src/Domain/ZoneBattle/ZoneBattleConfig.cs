using System;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 区域战斗配置（GDD_021 §11 Balancing，数据驱动，ADR-0012）。不可变。全部权威整数/定点（ADR-0004）。
    /// 含条件门槛 + 累积回合门 + 交战/减员/姿态/地形系数。
    /// </summary>
    public sealed class ZoneBattleConfig
    {
        // ---- 条件门槛（复用 ADR-0011 语义）----
        /// <summary>骑兵份额门槛（追击条件）。</summary>
        public FixedPoint CavalryMinShare { get; }
        /// <summary>智略门槛（伏兵突然性）。</summary>
        public FixedPoint GuileMin { get; }
        /// <summary>军纪门槛（夜袭隐蔽/军纪，取主将统率）。</summary>
        public FixedPoint DisciplineMin { get; }
        /// <summary>伏兵蓄势所需连续隐蔽回合。</summary>
        public int AmbushChargeRounds { get; }
        /// <summary>断粮达宽限所需连续切断回合。</summary>
        public int StarveRounds { get; }

        // ---- 交战/结算 ----
        /// <summary>每满足一条兵法条件的战力加成。</summary>
        public FixedPoint ConditionBonusEach { get; }
        /// <summary>基础减员率（均势时败方每回合损失比例，定点）。实际败方损失随战力比放大（封顶 <see cref="AttritionCap"/>）。</summary>
        public FixedPoint AttritionRate { get; }
        /// <summary>败方减员率上限（战力悬殊时不至瞬间全灭却能快速崩解）。</summary>
        public FixedPoint AttritionCap { get; }
        /// <summary>败方每回合士气跌幅。</summary>
        public FixedPoint MoraleDropOnLoss { get; }
        /// <summary>每回合基础疲劳增幅（实际增幅 = 基础 × 姿态疲劳倍率）。</summary>
        public FixedPoint FatiguePerRound { get; }
        /// <summary>主攻姿态战力乘数（最高——猛攻速破，但疲劳累积最快）。</summary>
        public FixedPoint AssaultMod { get; }
        /// <summary>守姿态战力乘数（居中偏防——依托工事久持，疲劳累积最慢）。</summary>
        public FixedPoint HoldMod { get; }
        /// <summary>佯攻姿态战力乘数（最低——示弱诱敌/掩护蓄势）。</summary>
        public FixedPoint FeintMod { get; }
        /// <summary>疲劳战力权重（有效战力 = 名义 ×(1 − 疲劳×此值)；令疲劳成真代价，久战侵蚀战力）。</summary>
        public FixedPoint FatiguePowerWeight { get; }
        /// <summary>主攻疲劳倍率（&gt;1：猛攻耗力快）。</summary>
        public FixedPoint AssaultFatigueMul { get; }
        /// <summary>坚守疲劳倍率（&lt;1：据守省力）。</summary>
        public FixedPoint HoldFatigueMul { get; }
        /// <summary>佯攻疲劳倍率（居中）。</summary>
        public FixedPoint FeintFatigueMul { get; }
        /// <summary>涌现兵法一次性士气冲击（对被打击方）。</summary>
        public FixedPoint EmergenceMoraleShock { get; }
        /// <summary>守方在坚固地形（城门/关隘正面）的城防战力加成（守城之利：破坚城须真优势而非均势，强化 W5）。</summary>
        public FixedPoint FortifiedDefenseBonus { get; }
        /// <summary>兵种×地形战力杠杆幅度（W4 #11 / ADR-0011 杠杆非克制）：合地形之兵种按其份额增益、逆地形（如隘口之骑）受抑。</summary>
        public FixedPoint TroopTerrainBonus { get; }

        public ZoneBattleConfig(
            FixedPoint cavalryMinShare, FixedPoint guileMin, FixedPoint disciplineMin,
            int ambushChargeRounds, int starveRounds,
            FixedPoint conditionBonusEach, FixedPoint attritionRate, FixedPoint attritionCap, FixedPoint moraleDropOnLoss,
            FixedPoint fatiguePerRound, FixedPoint assaultMod, FixedPoint holdMod, FixedPoint feintMod,
            FixedPoint fatiguePowerWeight, FixedPoint assaultFatigueMul, FixedPoint holdFatigueMul, FixedPoint feintFatigueMul,
            FixedPoint emergenceMoraleShock, FixedPoint fortifiedDefenseBonus, FixedPoint troopTerrainBonus)
        {
            if (ambushChargeRounds < 1) throw new ArgumentOutOfRangeException(nameof(ambushChargeRounds));
            if (starveRounds < 1) throw new ArgumentOutOfRangeException(nameof(starveRounds));
            CavalryMinShare = cavalryMinShare;
            GuileMin = guileMin;
            DisciplineMin = disciplineMin;
            AmbushChargeRounds = ambushChargeRounds;
            StarveRounds = starveRounds;
            ConditionBonusEach = conditionBonusEach;
            AttritionRate = attritionRate;
            AttritionCap = attritionCap;
            MoraleDropOnLoss = moraleDropOnLoss;
            FatiguePerRound = fatiguePerRound;
            AssaultMod = assaultMod;
            HoldMod = holdMod;
            FeintMod = feintMod;
            FatiguePowerWeight = fatiguePowerWeight;
            AssaultFatigueMul = assaultFatigueMul;
            HoldFatigueMul = holdFatigueMul;
            FeintFatigueMul = feintFatigueMul;
            EmergenceMoraleShock = emergenceMoraleShock;
            FortifiedDefenseBonus = fortifiedDefenseBonus;
            TroopTerrainBonus = troopTerrainBonus;
        }

        /// <summary>
        /// 兵种×地形战力乘数（W4 #11 / ADR-0011 <b>杠杆非克制</b>）：合地形之兵种按其份额增益、逆地形之兵种受抑——
        /// 平原利骑冲、渡口利水战、隘口/林莽骑不得展而利步、坚城步战守成。返回 [0.5,1.5] 附近的乘数（中性 1.0）。
        /// </summary>
        public FixedPoint TroopTerrainMul(TroopComposition comp, TerrainKind terrain)
        {
            if (comp == null || comp.Total <= 0) return FixedPoint.One;
            FixedPoint Share(TroopType t) => FixedPoint.FromFraction(comp.Count(t), comp.Total);
            FixedPoint cav = Share(TroopType.Cavalry);
            FixedPoint marine = Share(TroopType.Marine);
            FixedPoint inf = Share(TroopType.Infantry);
            FixedPoint half = TroopTerrainBonus * FixedPoint.FromFraction(1, 2);

            FixedPoint fav, pen;
            switch (terrain)
            {
                case TerrainKind.Plain: fav = TroopTerrainBonus * cav; pen = FixedPoint.Zero; break;                // 平原利骑冲
                case TerrainKind.Ford: fav = TroopTerrainBonus * marine; pen = TroopTerrainBonus * cav; break;      // 渡口利水·骑难渡
                case TerrainKind.Pass: fav = TroopTerrainBonus * inf; pen = TroopTerrainBonus * cav; break;         // 隘口步战·骑不得展
                case TerrainKind.Cover: fav = TroopTerrainBonus * inf; pen = TroopTerrainBonus * cav; break;        // 林莽步隐·骑受阻
                case TerrainKind.Fortified: fav = TroopTerrainBonus * inf; pen = half * cav; break;                 // 坚城步战守成
                default: fav = FixedPoint.Zero; pen = FixedPoint.Zero; break;
            }
            return FixedPoint.One + fav - pen;
        }

        /// <summary>某姿态战力乘数。</summary>
        public FixedPoint PostureMod(Posture p) => p switch
        {
            Posture.Assault => AssaultMod,
            Posture.Hold => HoldMod,
            Posture.Feint => FeintMod,
            _ => FixedPoint.One,
        };

        /// <summary>某姿态疲劳倍率。</summary>
        public FixedPoint PostureFatigueMul(Posture p) => p switch
        {
            Posture.Assault => AssaultFatigueMul,
            Posture.Hold => HoldFatigueMul,
            Posture.Feint => FeintFatigueMul,
            _ => FixedPoint.One,
        };

        /// <summary>疲劳侵蚀后的有效战力乘数 (1 − 疲劳×权重)，下限 0（疲劳∈[0,1]、权重≤1 时自然 ≥0）。</summary>
        public FixedPoint FatiguePowerMul(FixedPoint fatigue)
        {
            FixedPoint m = FixedPoint.One - fatigue * FatiguePowerWeight;
            return m < FixedPoint.Zero ? FixedPoint.Zero : m;
        }

        /// <summary>
        /// 默认（GDD_021 §11 平衡定值，2026-07-04 打磨）：
        /// 姿态从"纯战力乘数"升级为<b>速攻 vs 久持</b>权衡——主攻战力最高但疲劳最快、坚守居中但省力；
        /// 疲劳纳入战力（久战侵蚀）。攻方受回合上限时压（须速破），令三姿态皆无占优。
        /// </summary>
        public static ZoneBattleConfig Default { get; } = new ZoneBattleConfig(
            cavalryMinShare: FixedPoint.FromFraction(3, 10),
            guileMin: FixedPoint.FromFraction(6, 10),
            disciplineMin: FixedPoint.FromFraction(6, 10),
            ambushChargeRounds: 2, starveRounds: 2,
            conditionBonusEach: FixedPoint.FromFraction(15, 100),
            attritionRate: FixedPoint.FromFraction(2, 10),
            attritionCap: FixedPoint.FromFraction(6, 10),
            moraleDropOnLoss: FixedPoint.FromFraction(1, 10),
            fatiguePerRound: FixedPoint.FromFraction(5, 100),
            assaultMod: FixedPoint.FromFraction(125, 100),
            holdMod: FixedPoint.FromFraction(110, 100),
            feintMod: FixedPoint.FromFraction(75, 100),
            fatiguePowerWeight: FixedPoint.FromFraction(5, 10),
            assaultFatigueMul: FixedPoint.FromInt(2),
            holdFatigueMul: FixedPoint.FromFraction(1, 2),
            feintFatigueMul: FixedPoint.One,
            emergenceMoraleShock: FixedPoint.FromFraction(3, 10),
            fortifiedDefenseBonus: FixedPoint.FromFraction(35, 100),
            troopTerrainBonus: FixedPoint.FromFraction(15, 100));
    }

    /// <summary>
    /// 战斗全局上下文（GDD_021 D6/D7：时机/天气/侦察，battle-wide）。不可变。
    /// 来自出征六维准备（时机 → 夜/雾；侦察 → 反全知门）或守城场景。
    /// </summary>
    public sealed class ZoneBattleContext
    {
        /// <summary>是否夜间（夜袭条件门）。</summary>
        public bool IsNight { get; }
        /// <summary>是否有雾（隐蔽成功门之一）。</summary>
        public bool IsFoggy { get; }
        /// <summary>攻方是否已侦察目标（反全知：突袭类条件门 + 免情报盲区）。</summary>
        public bool AttackerScouted { get; }

        /// <summary>是否干燥天时（晴，无雨无雾）——火攻条件门（DryField）。</summary>
        public bool IsDry { get; }

        public ZoneBattleContext(bool isNight, bool isFoggy, bool attackerScouted, bool isDry = false)
        {
            IsNight = isNight;
            IsFoggy = isFoggy;
            AttackerScouted = attackerScouted;
            IsDry = isDry;
        }

        /// <summary>白昼、已侦察的中性上下文（isDry 默认 false：火攻须由天时显式开启，避免默认场景意外起火）。</summary>
        public static ZoneBattleContext Default { get; } = new ZoneBattleContext(false, false, true, isDry: false);

        internal void AppendTo(StateHasher hasher)
            => hasher.Append(IsNight).Append(IsFoggy).Append(AttackerScouted).Append(IsDry);
    }
}
