using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Conquest
{
    /// <summary>
    /// 出征准备（GDD_019 §4a 六维闭合因果的输入，ADR-0011）。不可变。
    /// 六维：兵力 · 补给 · 将领编成 · 兵种编成 · 布势路线 · 时机/天气，外加地形/侦察门。
    /// 兵法条件<b>不再直接传入</b>——由 <see cref="OffensiveSetupService.Derive"/> 按各维的门确定性派生。
    /// </summary>
    public sealed class OffensivePreparation
    {
        /// <summary>D1 可投入（征募）兵力。</summary>
        public int MusteredTroops { get; }

        /// <summary>D2 补给续航量。</summary>
        public long Supply { get; }

        /// <summary>D3 将领编成（主将/副将/军师随军）。</summary>
        public OffensiveCommand Command { get; }

        /// <summary>D4 兵种编成（份额=条件门与战力契合输入）。</summary>
        public TroopComposition Composition { get; }

        /// <summary>D5 布势路线（要凑成的兵法链）。</summary>
        public ApproachPlan Approach { get; }

        /// <summary>D6 时机/天气窗口。</summary>
        public OffensiveTiming Timing { get; }

        /// <summary>D5 进攻路线地形（伏兵突然性等条件门）。</summary>
        public TerrainKind Terrain { get; }

        /// <summary>D7 目标是否已侦察（反全知门：守方未察觉/伏兵突然性 + 免情报盲区折扣）。</summary>
        public bool Scouted { get; }

        /// <summary>长围承诺时段（长围断粮的时间成本；断粮达宽限门槛 StarveSegments）。</summary>
        public long SiegeSegmentsCommitted { get; }

        /// <summary>构造并校验。兵力/补给/长围时段非负；主将必选（经 OffensiveCommand）；兵种和 ≤ 兵力。</summary>
        public OffensivePreparation(
            int musteredTroops, long supply, OffensiveCommand command, TroopComposition composition,
            ApproachPlan approach, OffensiveTiming timing, TerrainKind terrain = TerrainKind.Fortified,
            bool scouted = false, long siegeSegmentsCommitted = 0)
        {
            if (musteredTroops < 0) throw new ArgumentOutOfRangeException(nameof(musteredTroops), "可投入兵力不可为负。");
            if (supply < 0) throw new ArgumentOutOfRangeException(nameof(supply), "补给不可为负。");
            if (siegeSegmentsCommitted < 0) throw new ArgumentOutOfRangeException(nameof(siegeSegmentsCommitted), "长围时段不可为负。");
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Composition = composition ?? throw new ArgumentNullException(nameof(composition));
            Timing = timing ?? throw new ArgumentNullException(nameof(timing));
            if (Composition.Total > musteredTroops)
                throw new ArgumentException("兵种编成总数不可超过投入兵力。", nameof(composition));

            MusteredTroops = musteredTroops;
            Supply = supply;
            Approach = approach;
            Terrain = terrain;
            Scouted = scouted;
            SiegeSegmentsCommitted = siegeSegmentsCommitted;
        }
    }

    /// <summary>派生出的进攻方战力（GDD_019 F1/F2）。不可变。</summary>
    public sealed class OffensiveForce
    {
        /// <summary>进攻方兵力（准备越足越高）。</summary>
        public int Force { get; }
        /// <summary>进攻方士气（补给/武勇越足越高，封顶）。</summary>
        public FixedPoint Morale { get; }
        /// <summary>随军携入的已满足兵法条件（供 GDD_010 结算加成 + 战后识别）。</summary>
        public IReadOnlyList<TacticCondition> Conditions { get; }

        internal OffensiveForce(int force, FixedPoint morale, IReadOnlyList<TacticCondition> conditions)
        {
            Force = force;
            Morale = morale;
            Conditions = conditions;
        }
    }

    /// <summary>
    /// 闭合因果映射配置（GDD_019 §8 Tuning，数据驱动，ADR-0011）。不可变。全部权威整数/定点（ADR-0004）。
    /// </summary>
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

        // --- 将领（D3）---
        /// <summary>主将统率 → 战力权重（每 1.0 统率的战力）。</summary>
        public FixedPoint CommandForceWeight { get; }
        /// <summary>副将统率 → 战力权重。</summary>
        public FixedPoint DeputyForceWeight { get; }
        /// <summary>副将递减因子 decay∈(0,1]（第 k 名 decay^k，防堆将碾压）。</summary>
        public FixedPoint DeputyDecay { get; }
        /// <summary>主将武勇 → 士气权重。</summary>
        public FixedPoint ValorMoraleWeight { get; }
        /// <summary>智略门槛（伏兵突然性需军师随军或主将智略≥此）。</summary>
        public FixedPoint GuileMin { get; }
        /// <summary>军纪门槛（夜袭隐蔽/军纪需主将统率≥此）。</summary>
        public FixedPoint DisciplineMin { get; }

        // --- 兵种/路线（D4/D5）---
        /// <summary>骑兵份额门槛（追击/机动条件）。</summary>
        public FixedPoint CavalryMinShare { get; }
        /// <summary>步卒份额门槛（正面/长围战力契合）。</summary>
        public FixedPoint InfantryFitShare { get; }
        /// <summary>兵种契合战力加成。</summary>
        public int FitBonus { get; }
        /// <summary>正面强攻战力修正（+，见效快）。</summary>
        public int FrontalForceMod { get; }
        /// <summary>长围断粮战力修正（−，换耗敌）。</summary>
        public int ProtractedForceMod { get; }
        /// <summary>情报盲区战力折扣（未侦察）。</summary>
        public int IntelBlindPenalty { get; }

        // --- 长围（D5 时间成本）---
        /// <summary>长围断粮的补给门槛（能久围）。</summary>
        public long StarveSupplyMin { get; }
        /// <summary>断粮达宽限所需承诺时段。</summary>
        public long StarveSegments { get; }

        public OffensiveSetupConfig(
            int baseForce, FixedPoint forcePerTroop, FixedPoint baseMorale, FixedPoint moralePerStep,
            long supplyStep, FixedPoint maxMorale,
            FixedPoint commandForceWeight, FixedPoint deputyForceWeight, FixedPoint deputyDecay,
            FixedPoint valorMoraleWeight, FixedPoint guileMin, FixedPoint disciplineMin,
            FixedPoint cavalryMinShare, FixedPoint infantryFitShare, int fitBonus,
            int frontalForceMod, int protractedForceMod, int intelBlindPenalty,
            long starveSupplyMin, long starveSegments)
        {
            if (baseForce < 0) throw new ArgumentOutOfRangeException(nameof(baseForce));
            if (supplyStep <= 0) throw new ArgumentOutOfRangeException(nameof(supplyStep), "补给档量须为正。");
            if (deputyDecay <= FixedPoint.Zero || deputyDecay > FixedPoint.One)
                throw new ArgumentOutOfRangeException(nameof(deputyDecay), "副将递减因子须在 (0,1]。");
            BaseForce = baseForce;
            ForcePerTroop = forcePerTroop;
            BaseMorale = baseMorale;
            MoralePerStep = moralePerStep;
            SupplyStep = supplyStep;
            MaxMorale = maxMorale;
            CommandForceWeight = commandForceWeight;
            DeputyForceWeight = deputyForceWeight;
            DeputyDecay = deputyDecay;
            ValorMoraleWeight = valorMoraleWeight;
            GuileMin = guileMin;
            DisciplineMin = disciplineMin;
            CavalryMinShare = cavalryMinShare;
            InfantryFitShare = infantryFitShare;
            FitBonus = fitBonus;
            FrontalForceMod = frontalForceMod;
            ProtractedForceMod = protractedForceMod;
            IntelBlindPenalty = intelBlindPenalty;
            StarveSupplyMin = starveSupplyMin;
            StarveSegments = starveSegments;
        }

        /// <summary>默认（GDD_019 §8 默认值）。</summary>
        public static OffensiveSetupConfig Default { get; } = new OffensiveSetupConfig(
            baseForce: 200, forcePerTroop: FixedPoint.One,
            baseMorale: FixedPoint.FromFraction(5, 10), moralePerStep: FixedPoint.FromFraction(5, 100),
            supplyStep: 50, maxMorale: FixedPoint.One,
            commandForceWeight: FixedPoint.FromInt(100), deputyForceWeight: FixedPoint.FromInt(60),
            deputyDecay: FixedPoint.FromFraction(5, 10), valorMoraleWeight: FixedPoint.FromFraction(1, 10),
            guileMin: FixedPoint.FromFraction(6, 10), disciplineMin: FixedPoint.FromFraction(6, 10),
            cavalryMinShare: FixedPoint.FromFraction(3, 10), infantryFitShare: FixedPoint.FromFraction(5, 10),
            fitBonus: 40, frontalForceMod: 50, protractedForceMod: -80, intelBlindPenalty: 80,
            starveSupplyMin: 200, starveSegments: 8);
    }

    /// <summary>
    /// 闭合因果核心（GDD_019 R3/§4a/§5，ADR-0011）：把玩家<b>六维准备态确定性</b>映射为进攻方战力/士气/携入条件。
    /// 兵力/主将统率→战力；补给/主将武勇→士气（封顶）；兵种/时机/天气/军师/侦察→路线兵法条件能否成型。
    /// <b>准备决定胜负的地基</b>：门齐→触发兵法、以弱胜强；门不齐/裸战→硬碰可能败。纯函数、无随机、整数/定点。
    /// <b>无兵种克制表、无坐标</b>（ADR-0011 负向不变量）。
    /// </summary>
    public sealed class OffensiveSetupService
    {
        public OffensiveForce Derive(OffensivePreparation prep, OffensiveSetupConfig config)
        {
            if (prep == null) throw new ArgumentNullException(nameof(prep));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // --- 战力（F1）---
            long force = config.BaseForce;
            force += (config.ForcePerTroop * FixedPoint.FromInt(prep.MusteredTroops)).RoundToInt();
            force += CommandForce(prep.Command, config);
            force += CompositionFit(prep, config);
            force += ApproachForceMod(prep.Approach, config);
            if (!prep.Scouted) force -= config.IntelBlindPenalty;
            if (force < 0) force = 0;
            int forceInt = force > int.MaxValue ? int.MaxValue : (int)force;

            // --- 士气（F2）---
            long steps = prep.Supply / config.SupplyStep;
            int stepsInt = steps > int.MaxValue ? int.MaxValue : (int)steps;
            FixedPoint morale = config.BaseMorale + config.MoralePerStep * FixedPoint.FromInt(stepsInt);
            morale += prep.Command.Lead.Valor * config.ValorMoraleWeight;
            if (morale > config.MaxMorale) morale = config.MaxMorale;
            if (morale < FixedPoint.Zero) morale = FixedPoint.Zero;

            // --- 兵法条件成型（F3：路线模板 ∩ 门）---
            List<TacticCondition> conditions = FormConditions(prep, config);

            return new OffensiveForce(forceInt, morale, conditions);
        }

        /// <summary>主将统率 + 副将（decay^k 递减）→ 战力（F1 将领项）。</summary>
        private static int CommandForce(OffensiveCommand cmd, OffensiveSetupConfig c)
        {
            int f = (cmd.Lead.Command * c.CommandForceWeight).RoundToInt();
            FixedPoint decay = FixedPoint.One;
            foreach (OffensiveGeneral dep in cmd.Deputies)
            {
                decay = decay * c.DeputyDecay;                 // 第 k 名 → decay^k
                f += (dep.Command * c.DeputyForceWeight * decay).RoundToInt();
            }
            return f;
        }

        /// <summary>兵种×路线契合（F2b，<b>杠杆</b>：匹配给 +FitBonus，否则 0；无克制减益）。</summary>
        private static int CompositionFit(OffensivePreparation p, OffensiveSetupConfig c)
        {
            switch (p.Approach)
            {
                case ApproachPlan.FrontalAssault:
                case ApproachPlan.ProtractedSiege:
                    return Share(p, TroopType.Infantry) >= c.InfantryFitShare ? c.FitBonus : 0;
                case ApproachPlan.FeintLure:
                    return Share(p, TroopType.Cavalry) >= c.CavalryMinShare ? c.FitBonus : 0;
                default:
                    return 0;
            }
        }

        /// <summary>路线战力修正（F1 路线项）。</summary>
        private static int ApproachForceMod(ApproachPlan approach, OffensiveSetupConfig c) => approach switch
        {
            ApproachPlan.FrontalAssault => c.FrontalForceMod,
            ApproachPlan.ProtractedSiege => c.ProtractedForceMod,
            _ => 0,
        };

        /// <summary>某兵种份额（该兵种数 / 总投入；投入 0 → 0）。</summary>
        private static FixedPoint Share(OffensivePreparation p, TroopType t)
            => p.MusteredTroops <= 0 ? FixedPoint.Zero : FixedPoint.FromFraction(p.Composition.Count(t), p.MusteredTroops);

        /// <summary>F3 门表：所选路线的每条目标条件，仅当其门全通过才携入。</summary>
        private static List<TacticCondition> FormConditions(OffensivePreparation p, OffensiveSetupConfig c)
        {
            var list = new List<TacticCondition>();
            OffensiveCommand cmd = p.Command;
            bool guileEnough = cmd.AdvisorAccompanies || cmd.Lead.Guile >= c.GuileMin;
            bool disciplined = cmd.Lead.Command >= c.DisciplineMin;

            switch (p.Approach)
            {
                case ApproachPlan.FeintLure:
                    list.Add(TacticCondition.ControlledRetreatKeptFormation);                       // 路线提供
                    if (Share(p, TroopType.Cavalry) >= c.CavalryMinShare)
                        list.Add(TacticCondition.EnemyPursued);                                      // 骑兵门
                    if (p.Terrain == TerrainKind.Pass && guileEnough && p.Scouted)
                        list.Add(TacticCondition.AmbushSurprise);                                    // 隘口+智谋+侦察门
                    break;

                case ApproachPlan.ProtractedSiege:
                    bool canStarve = p.Supply >= c.StarveSupplyMin;
                    bool starvedLong = canStarve && p.SiegeSegmentsCommitted >= c.StarveSegments;
                    if (canStarve) list.Add(TacticCondition.SupplyLineCut);                          // 补给门
                    if (starvedLong) list.Add(TacticCondition.ShortageReachedGrace);                 // 补给+时间门
                    if (starvedLong) list.Add(TacticCondition.EnemyCohesionCrossedThreshold);        // 前二成立
                    break;

                case ApproachPlan.NightRaid:
                    if (p.Timing.IsNight) list.Add(TacticCondition.IsNight);                         // 时段门
                    if (p.Timing.IsNight && (p.Timing.IsFoggy || disciplined))
                        list.Add(TacticCondition.StealthSuccess);                                    // 雾/军纪门
                    if (p.Timing.IsNight && p.Scouted)
                        list.Add(TacticCondition.DefenderUnaware);                                   // 侦察门
                    if (p.Timing.IsNight && disciplined)
                        list.Add(TacticCondition.RaiderDisciplineMet);                               // 军纪门
                    break;

                case ApproachPlan.FrontalAssault:
                default:
                    break;   // 正面强攻无兵法条件（纯战力）
            }

            return list;
        }
    }
}
