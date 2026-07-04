using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>出征相关中文标签集中映射（表现层，不影响权威 id）。</summary>
    public static class OffensiveText
    {
        /// <summary>布势路线中文名。</summary>
        public static string Approach(ApproachPlan a) => a switch
        {
            ApproachPlan.FrontalAssault => "正面强攻",
            ApproachPlan.FeintLure => "假退诱敌",
            ApproachPlan.ProtractedSiege => "长围断粮",
            ApproachPlan.NightRaid => "夜袭",
            _ => a.ToString(),
        };

        /// <summary>兵种中文名。</summary>
        public static string Troop(TroopType t) => t switch
        {
            TroopType.Infantry => "步卒",
            TroopType.Cavalry => "骑兵",
            TroopType.Archer => "弓弩",
            TroopType.Marine => "水军",
            _ => t.ToString(),
        };

        /// <summary>时段中文名。</summary>
        public static string Segment(DaySegment s) => s switch
        {
            DaySegment.Dawn => "黎明",
            DaySegment.Day => "白昼",
            DaySegment.Dusk => "黄昏",
            DaySegment.Night => "夜间",
            _ => s.ToString(),
        };

        /// <summary>天气中文名。</summary>
        public static string Weather(WeatherType w) => w switch
        {
            WeatherType.Clear => "晴",
            WeatherType.Overcast => "阴",
            WeatherType.Rain => "雨",
            WeatherType.Fog => "雾",
            _ => w.ToString(),
        };

        /// <summary>兵法条件中文名（与战况进度同源）。</summary>
        public static string Condition(TacticCondition c) => c switch
        {
            TacticCondition.ControlledRetreatKeptFormation => "受控撤退保持队形",
            TacticCondition.EnemyPursued => "敌军追击",
            TacticCondition.AmbushSurprise => "伏兵突然性",
            TacticCondition.SupplyLineCut => "切断敌补给线",
            TacticCondition.ShortageReachedGrace => "断粮达宽限时段",
            TacticCondition.EnemyCohesionCrossedThreshold => "敌士气疲劳跨阈",
            TacticCondition.HeldPosition => "守住阵地",
            TacticCondition.ReliefArrived => "外援抵达",
            TacticCondition.SurvivedDeadline => "撑过期限",
            TacticCondition.IsNight => "值夜间",
            TacticCondition.StealthSuccess => "隐蔽成功",
            TacticCondition.DefenderUnaware => "守方未察觉",
            TacticCondition.RaiderDisciplineMet => "袭方军纪达标",
            _ => c.ToString(),
        };

        /// <summary>授权门结果中文说明。</summary>
        public static string Gate(OffensiveGateResult g) => g switch
        {
            OffensiveGateResult.Authorized => "可攻（授权·敌控城）",
            OffensiveGateResult.NotAuthorized => "未授权（须君主受命）",
            OffensiveGateResult.NotEnemyControlled => "非敌控（无主/未登记）",
            OffensiveGateResult.OwnCity => "己方城（不可攻）",
            _ => g.ToString(),
        };

        /// <summary>某布势路线意图凑成的目标兵法条件集（供缺失提示，与 Domain F3 门表意图一致）。</summary>
        public static IReadOnlyList<TacticCondition> ApproachTargets(ApproachPlan a) => a switch
        {
            ApproachPlan.FeintLure => new[]
            {
                TacticCondition.ControlledRetreatKeptFormation, TacticCondition.EnemyPursued, TacticCondition.AmbushSurprise,
            },
            ApproachPlan.ProtractedSiege => new[]
            {
                TacticCondition.SupplyLineCut, TacticCondition.ShortageReachedGrace, TacticCondition.EnemyCohesionCrossedThreshold,
            },
            ApproachPlan.NightRaid => new[]
            {
                TacticCondition.IsNight, TacticCondition.StealthSuccess, TacticCondition.DefenderUnaware, TacticCondition.RaiderDisciplineMet,
            },
            _ => Array.Empty<TacticCondition>(),
        };
    }

    /// <summary>一个可出征目标（城 + 授权门状态）。不可变。</summary>
    public sealed class OffensiveTargetLine
    {
        /// <summary>目标城权威 id。</summary>
        public string CityId { get; }
        /// <summary>目标城中文名。</summary>
        public string CityLabel { get; }
        /// <summary>授权门结果。</summary>
        public OffensiveGateResult Gate { get; }
        /// <summary>门结果中文说明。</summary>
        public string GateLabel => OffensiveText.Gate(Gate);
        /// <summary>是否可攻（门通过）。</summary>
        public bool CanAttack => Gate == OffensiveGateResult.Authorized;

        internal OffensiveTargetLine(string cityId, string cityLabel, OffensiveGateResult gate)
        {
            CityId = cityId;
            CityLabel = cityLabel;
            Gate = gate;
        }
    }

    /// <summary>出征选目标视图（GDD_019 §7 选目标 + R1/R2 授权门，反全知只读控制权投影）。不可变。</summary>
    public sealed class OffensiveTargetsView
    {
        /// <summary>候选目标（含门状态；不可攻的也列出并说明原因，AC-5 看得到为什么）。</summary>
        public IReadOnlyList<OffensiveTargetLine> Targets { get; }
        /// <summary>是否存在可攻目标。</summary>
        public bool AnyAttackable { get; }
        /// <summary>是否已受君主出征授权（未授权则先请缨）。</summary>
        public bool Authorized { get; }

        internal OffensiveTargetsView(IReadOnlyList<OffensiveTargetLine> targets, bool authorized)
        {
            Targets = targets;
            Authorized = authorized;
            bool any = false;
            foreach (OffensiveTargetLine t in targets) if (t.CanAttack) { any = true; break; }
            AnyAttackable = any;
        }
    }

    /// <summary>
    /// 出征计划草稿（GDD_019 §4a 六维·ADR-0011 D7 发起前临时态，不入存档）。<b>可变 builder</b>，只经 CampaignRuntime 修改。
    /// 兵法条件不在此设定——由发起时 <see cref="OffensiveSetupService"/> 按门确定性派生。
    /// </summary>
    public sealed class OffensivePlan
    {
        /// <summary>目标城。</summary>
        public CityId Target { get; }
        /// <summary>D1 投入兵力。</summary>
        public int Muster { get; set; }
        /// <summary>D2 随军补给。</summary>
        public long Supply { get; set; }
        /// <summary>D3 主将。</summary>
        public OffensiveGeneral Lead { get; set; }
        /// <summary>D3 副将（有序）。</summary>
        public List<OffensiveGeneral> Deputies { get; } = new List<OffensiveGeneral>();
        /// <summary>D3 军师是否随军。</summary>
        public bool Advisor { get; set; }
        /// <summary>D4 兵种编成（各兵种数；空=默认全步卒）。</summary>
        public Dictionary<TroopType, int> Composition { get; } = new Dictionary<TroopType, int>();
        /// <summary>D5 布势路线。</summary>
        public ApproachPlan Approach { get; set; }
        /// <summary>D6 发起时段。</summary>
        public DaySegment Segment { get; set; }
        /// <summary>D6 发起天气。</summary>
        public WeatherType Weather { get; set; }
        /// <summary>长围承诺时段（长围断粮时间成本）。</summary>
        public long SiegeSegments { get; set; }

        /// <summary>以场景默认组建草稿（主将=太守亲征；正面强攻；当前时段/晴）。</summary>
        public OffensivePlan(
            CityId target, OffensiveGeneral defaultLead, int defaultMuster, long defaultSupply, DaySegment segment)
        {
            Target = target;
            Lead = defaultLead ?? throw new ArgumentNullException(nameof(defaultLead));
            Muster = defaultMuster;
            Supply = defaultSupply;
            Approach = ApproachPlan.FrontalAssault;
            Segment = segment;
            Weather = WeatherType.Clear;
        }

        /// <summary>组装为不可变出征准备（消费终态）。地形/侦察由发起方（Runtime）按场景/情报注入。</summary>
        public OffensivePreparation Build(TerrainKind terrain, bool scouted)
        {
            int assigned = 0;
            foreach (int v in Composition.Values) assigned = checked(assigned + v);
            TroopComposition comp = assigned == 0 ? TroopComposition.AllInfantry(Muster) : new TroopComposition(Composition);
            return new OffensivePreparation(
                Muster, Supply, new OffensiveCommand(Lead, Deputies, Advisor),
                comp, Approach, new OffensiveTiming(Segment, Weather), terrain, scouted, SiegeSegments);
        }
    }

    /// <summary>
    /// 出征计划预览（GDD_019 R3 闭合因果可见性）：草稿六维摘要 + <b>预计派生</b>的战力/士气/成型条件 +
    /// 所选路线<b>尚缺</b>的条件提示（军师风格，<b>无胜率</b>，AC-5）。不可变、纯函数。
    /// </summary>
    public sealed class OffensivePlanView
    {
        /// <summary>目标城中文名。</summary>
        public string TargetLabel { get; }
        /// <summary>六维摘要行（兵力/补给/主将/副将/军师/兵种/路线/时机）。</summary>
        public IReadOnlyList<string> Summary { get; }
        /// <summary>预计进攻方战力（准备越足越高）。</summary>
        public int ForcePreview { get; }
        /// <summary>预计士气（定点值文本）。</summary>
        public string MoraleLabel { get; }
        /// <summary>预计成型的兵法条件（中文）。</summary>
        public IReadOnlyList<string> FormingConditions { get; }
        /// <summary>所选路线尚缺的目标条件（提示玩家还差什么，无胜率）。</summary>
        public IReadOnlyList<string> MissingConditions { get; }
        /// <summary>是否已侦察目标（反全知：影响突袭类条件与情报盲区折扣）。</summary>
        public bool Scouted { get; }

        private OffensivePlanView(
            string targetLabel, IReadOnlyList<string> summary, int force, string morale,
            IReadOnlyList<string> forming, IReadOnlyList<string> missing, bool scouted)
        {
            TargetLabel = targetLabel;
            Summary = summary;
            ForcePreview = force;
            MoraleLabel = morale;
            FormingConditions = forming;
            MissingConditions = missing;
            Scouted = scouted;
        }

        /// <summary>由草稿 + 预计派生战力构造（Runtime 传入 dry-run 的 <see cref="OffensiveForce"/>）。</summary>
        public static OffensivePlanView FromPlan(OffensivePlan plan, OffensiveForce preview, bool scouted)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (preview == null) throw new ArgumentNullException(nameof(preview));

            var summary = new List<string>
            {
                $"投入兵力：{plan.Muster}",
                $"随军补给：{plan.Supply}",
                $"主将：{DisplayNames.Of(plan.Lead.Character.Value)}",
            };
            if (plan.Deputies.Count > 0)
            {
                var names = new List<string>();
                foreach (OffensiveGeneral d in plan.Deputies) names.Add(DisplayNames.Of(d.Character.Value));
                summary.Add("副将：" + string.Join("、", names));
            }
            summary.Add("军师随军：" + (plan.Advisor ? "是" : "否"));
            if (plan.Composition.Count > 0)
            {
                var comp = new List<string>();
                foreach (KeyValuePair<TroopType, int> kv in plan.Composition) comp.Add($"{OffensiveText.Troop(kv.Key)}{kv.Value}");
                summary.Add("兵种：" + string.Join("、", comp));
            }
            else summary.Add("兵种：步卒（未细分）");
            summary.Add("布势路线：" + OffensiveText.Approach(plan.Approach));
            summary.Add($"时机：{OffensiveText.Segment(plan.Segment)}·{OffensiveText.Weather(plan.Weather)}");
            summary.Add("侦察：" + (scouted ? "已探明敌情" : "未侦察（摸黑攻城·无突袭之利）"));

            var forming = new List<string>();
            var formingSet = new HashSet<TacticCondition>(preview.Conditions);
            foreach (TacticCondition c in preview.Conditions) forming.Add(OffensiveText.Condition(c));

            var missing = new List<string>();
            foreach (TacticCondition c in OffensiveText.ApproachTargets(plan.Approach))
                if (!formingSet.Contains(c)) missing.Add(OffensiveText.Condition(c));

            return new OffensivePlanView(
                DisplayNames.Of(plan.Target.Value), summary, preview.Force, preview.Morale.ToString(),
                forming, missing, scouted);
        }
    }

    /// <summary>
    /// 出征结果视图（GDD_019 §7 输出 / 占城 C / 失败可继续）：被门拒 / 战败退兵 / 破城占城归属。不可变、纯函数。<b>无胜率</b>。
    /// </summary>
    public sealed class OffensiveResultView
    {
        /// <summary>是否通过授权门并出征。</summary>
        public bool Launched { get; }
        /// <summary>是否进入区域战斗、尚未分胜负（多回合作战进行中）。</summary>
        public bool BattleInProgress { get; }
        /// <summary>是否破城取胜。</summary>
        public bool Victory { get; }
        /// <summary>一句话结论。</summary>
        public string ConclusionLabel { get; }
        /// <summary>占城归属说明（仅取胜时非空）。</summary>
        public string OwnershipLabel { get; }
        /// <summary>后果注记（占城计数/自立倾向/记功；或失败可继续）。</summary>
        public IReadOnlyList<string> Notes { get; }
        /// <summary>本战成型的兵法条件（复盘，中文）。</summary>
        public IReadOnlyList<string> Tactics { get; }

        private OffensiveResultView(
            bool launched, bool battleInProgress, bool victory, string conclusion, string ownership,
            IReadOnlyList<string> notes, IReadOnlyList<string> tactics)
        {
            Launched = launched;
            BattleInProgress = battleInProgress;
            Victory = victory;
            ConclusionLabel = conclusion;
            OwnershipLabel = ownership;
            Notes = notes;
            Tactics = tactics;
        }

        /// <summary>出征已发起、进入区域战斗（多回合作战，未分胜负）。</summary>
        public static OffensiveResultView Started()
            => new OffensiveResultView(true, true, false, "出征已发起——进入战场，排兵布阵、逐回合决胜。", string.Empty,
                new[] { "指挥各区支队、推进回合，破敌正面即克城。" }, Array.Empty<string>());

        /// <summary>破城取胜后的占城归属结算收口（区域战斗胜局）。</summary>
        public static OffensiveResultView Victorious(ConquestResult conquest, IReadOnlyList<string>? tactics = null)
        {
            bool toPlayer = conquest.Verdict == OwnershipVerdict.GrantToPlayer;
            string ownership = toPlayer
                ? "破城！此城归你直辖。"
                : "破城！然君主收归直辖——你得功绩名望，战果却被夺，自立之心渐起。";
            var notes = new List<string> { $"累计占城：{conquest.ConquestCount} 座" };
            if (conquest.CareerApplied) notes.Add("战功记入——晋阶门槛推进。");
            if (!toPlayer) notes.Add($"自立倾向累积至 {conquest.RebellionLean}（战果屡被夺 → 更易/更想自立）。");
            return new OffensiveResultView(true, false, true, "破城取胜！", ownership, notes, tactics ?? Array.Empty<string>());
        }

        /// <summary>攻城未克退兵（区域战斗败局；失败可继续，红线）。</summary>
        public static OffensiveResultView Defeated()
            => new OffensiveResultView(true, false, false, "攻城未克——折兵退兵。", string.Empty,
                new[] { "战役继续：可再备战、改守、或转攻他城（失败不切死局）。" }, Array.Empty<string>());

        /// <summary>由 Application 出征结果构造展示模型。</summary>
        public static OffensiveResultView FromResult(OffensiveResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var tactics = new List<string>();
            if (result.Force != null)
                foreach (TacticCondition c in result.Force.Conditions) tactics.Add(OffensiveText.Condition(c));

            if (!result.Launched)
                return new OffensiveResultView(false, false, false,
                    "未能出征：" + OffensiveText.Gate(result.Gate), string.Empty,
                    new[] { "调整目标或先请君主授权，再出征。" }, tactics);

            if (!result.Victory)
                return new OffensiveResultView(true, false, false,
                    "攻城未克——折兵退兵。", string.Empty,
                    new[] { "战役继续：可再备战、改守、或转攻他城（失败不切死局）。" }, tactics);

            ConquestResult c2 = result.Conquest!;
            bool toPlayer = c2.Verdict == OwnershipVerdict.GrantToPlayer;
            string ownership = toPlayer
                ? "破城！此城归你直辖。"
                : "破城！然君主收归直辖——你得功绩名望，战果却被夺，自立之心渐起。";
            var notes = new List<string> { $"累计占城：{c2.ConquestCount} 座" };
            if (c2.CareerApplied) notes.Add("战功记入——晋阶门槛推进。");
            if (!toPlayer) notes.Add($"自立倾向累积至 {c2.RebellionLean}（战果屡被夺 → 更易/更想自立）。");

            return new OffensiveResultView(true, false, true, "破城取胜！", ownership, notes, tactics);
        }
    }
}
