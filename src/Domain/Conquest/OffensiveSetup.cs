using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 出征准备快照（GDD_019 R3 闭合因果的输入）：可投入兵力、补给续航、备战计划派生的已满足兵法条件。不可变。
    /// </summary>
    public sealed class OffensivePreparation
    {
        /// <summary>可投入（征募）兵力。</summary>
        public int MusteredTroops { get; }
        /// <summary>补给续航量。</summary>
        public long Supply { get; }
        /// <summary>备战计划已派生的兵法条件（设伏/断粮/分进…）。</summary>
        public IReadOnlyList<TacticCondition> PlannedConditions { get; }

        public OffensivePreparation(int musteredTroops, long supply, IReadOnlyList<TacticCondition> plannedConditions)
        {
            if (musteredTroops < 0) throw new ArgumentOutOfRangeException(nameof(musteredTroops), "可投入兵力不可为负。");
            if (supply < 0) throw new ArgumentOutOfRangeException(nameof(supply), "补给不可为负。");
            MusteredTroops = musteredTroops;
            Supply = supply;
            PlannedConditions = plannedConditions ?? Array.Empty<TacticCondition>();
        }
    }

    /// <summary>派生出的进攻方战力（GDD_019 F1/F2）。不可变。</summary>
    public sealed class OffensiveForce
    {
        /// <summary>进攻方兵力（准备越足越高）。</summary>
        public int Force { get; }
        /// <summary>进攻方士气（补给越足越高，封顶）。</summary>
        public FixedPoint Morale { get; }
        /// <summary>随军携入的已满足兵法条件（供 GDD_010 事后识别）。</summary>
        public IReadOnlyList<TacticCondition> Conditions { get; }

        internal OffensiveForce(int force, FixedPoint morale, IReadOnlyList<TacticCondition> conditions)
        {
            Force = force;
            Morale = morale;
            Conditions = conditions;
        }
    }

    /// <summary>闭合因果映射配置（GDD_019 §Tuning，数据驱动）。不可变。</summary>
    public sealed class OffensiveSetupConfig
    {
        /// <summary>基础兵力（裸战底）。</summary>
        public int BaseForce { get; }
        /// <summary>每征募 1 兵 → 战力（定点，≥0）。</summary>
        public FixedPoint ForcePerTroop { get; }
        /// <summary>基础士气。</summary>
        public FixedPoint BaseMorale { get; }
        /// <summary>补给每满一档 → 士气增量。</summary>
        public FixedPoint MoralePerStep { get; }
        /// <summary>补给一档的量（&gt;0）。</summary>
        public long SupplyStep { get; }
        /// <summary>士气上限。</summary>
        public FixedPoint MaxMorale { get; }

        public OffensiveSetupConfig(
            int baseForce, FixedPoint forcePerTroop, FixedPoint baseMorale,
            FixedPoint moralePerStep, long supplyStep, FixedPoint maxMorale)
        {
            if (baseForce < 0) throw new ArgumentOutOfRangeException(nameof(baseForce));
            if (supplyStep <= 0) throw new ArgumentOutOfRangeException(nameof(supplyStep), "补给档量须为正。");
            BaseForce = baseForce;
            ForcePerTroop = forcePerTroop;
            BaseMorale = baseMorale;
            MoralePerStep = moralePerStep;
            SupplyStep = supplyStep;
            MaxMorale = maxMorale;
        }

        /// <summary>默认（每兵=1 战力；补给每 50 一档 +0.05 士气，封顶 1.0）。</summary>
        public static OffensiveSetupConfig Default { get; } = new OffensiveSetupConfig(
            baseForce: 200, forcePerTroop: FixedPoint.One,
            baseMorale: FixedPoint.FromFraction(5, 10), moralePerStep: FixedPoint.FromFraction(5, 100),
            supplyStep: 50, maxMorale: FixedPoint.One);
    }

    /// <summary>
    /// 闭合因果核心（GDD_019 R3/F1/F2）：把玩家准备态<b>确定性、单调</b>映射为进攻方战力——
    /// 兵力越多 → 战力越高；补给越足 → 士气越高（封顶）；备战计划的兵法条件随军携入。
    /// <b>准备决定胜负的地基</b>：准备足→强攻/触发兵法；裸战→基础战力硬碰（可能败）。纯函数、无随机。
    /// </summary>
    public sealed class OffensiveSetupService
    {
        public OffensiveForce Derive(OffensivePreparation prep, OffensiveSetupConfig config)
        {
            if (prep == null) throw new ArgumentNullException(nameof(prep));
            if (config == null) throw new ArgumentNullException(nameof(config));

            int force = checked(config.BaseForce + (config.ForcePerTroop * FixedPoint.FromInt(prep.MusteredTroops)).RoundToInt());

            long steps = prep.Supply / config.SupplyStep;   // 整除档数
            int stepsInt = steps > int.MaxValue ? int.MaxValue : (int)steps;
            FixedPoint morale = config.BaseMorale + config.MoralePerStep * FixedPoint.FromInt(stepsInt);
            if (morale > config.MaxMorale) morale = config.MaxMorale;

            return new OffensiveForce(force, morale, new List<TacticCondition>(prep.PlannedConditions));
        }
    }
}
