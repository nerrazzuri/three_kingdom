using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Subversion;
using ThreeKingdom.Domain.ZoneBattle;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>
    /// 区域战斗可玩运行期（GDD_021 §12 / ADR-0012；Presentation 纯 C# 接缝，dotnet 可测）：
    /// 持一局 <see cref="ZoneBattleState"/>，暴露<b>战中调整（调动/改姿态）→ 推进回合（敌AI+结算）→ 只读投影</b>。
    /// 一切变更经 Domain/Application 服务（ADR-0002）；无 UnityEngine 依赖。
    /// </summary>
    public sealed class ZoneBattleRuntime
    {
        private readonly ZoneBattleService _service = new ZoneBattleService();
        private readonly ZoneCommandService _commands = new ZoneCommandService();
        private readonly EnemyZoneAiService _autoAi = new EnemyZoneAiService();
        private readonly ZoneBattleContext _context;
        private readonly ZoneBattleConfig _config;
        private readonly EnemyAiConfig _aiConfig;

        private ZoneBattleState _state;
        private ZoneBattleOutcome _outcome = ZoneBattleOutcome.Ongoing;
        private List<string> _lastEmergences = new List<string>();

        public ZoneBattleRuntime(
            ZoneBattleState start, ZoneBattleContext context, ZoneBattleConfig? config = null, EnemyAiConfig? aiConfig = null)
        {
            _state = start ?? throw new ArgumentNullException(nameof(start));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? ZoneBattleConfig.Default;
            _aiConfig = aiConfig ?? EnemyAiConfig.Default;
        }

        /// <summary>
        /// 由出征六维准备开战（攻方经部署桥分区、守方由守备布防；玩家=攻方）。
        /// 可选 <paramref name="subversion"/>：战前<b>人心杠杆</b>施计效果（GDD_024）——改守方士气/军纪/有效守军。
        /// </summary>
        public static ZoneBattleRuntime FromOffensive(
            OffensivePreparation prep, FixedPoint morale, int garrison, ulong seed, int maxRounds = 6,
            SubversionEffect? subversion = null, IReadOnlyList<OffensiveGeneral>? defenders = null)
        {
            var field = BattleFieldCatalog.ForTerrain(prep.Terrain);   // #3 逐城/地形战场：按目标城地形选正面区
            var planner = new OffensiveDeploymentPlanner();
            var dets = new List<Detachment>();

            // 羁绊同场（GDD_025 R4 / T4）：并肩之将（血脉/师徒/知己）协同增士气，仇怨同场则互扣。作用于攻方初始士气。
            var present = new List<CharacterId> { prep.Command.Lead.Character };
            foreach (OffensiveGeneral dep in prep.Command.Deputies) present.Add(dep.Character);
            FixedPoint bondMul = new BondEffectService().SideBondMorale(
                present, ThreeKingdom.Application.Scenarios.GeneralBonds.Among(present), BondConfig.Default);
            FixedPoint attackerMorale = (morale * bondMul).Clamp(FixedPoint.Zero, FixedPoint.One);

            dets.AddRange(planner.PlanAttacker(prep, attackerMorale, field));
            dets.AddRange(planner.PlanDefender(
                new SiegeDefense(garrison, FixedPoint.FromFraction(12, 10)), FixedPoint.FromFraction(7, 10), field,
                subversion ?? SubversionEffect.None, defenders));
            ZoneBattleState start = new ZoneBattleService().Start(field, dets, BattleSide.Attacker, maxRounds, seed);
            return new ZoneBattleRuntime(start, planner.ContextFrom(prep));
        }

        /// <summary>
        /// 自包含演示战斗（Unity 战斗屏入口 / 端到端验证）：假退诱敌·600 兵含骑 300·已侦察，攻虎牢关守军 300。
        /// 与 console/Unity 共用同一引擎（无 UnityEngine 依赖）。
        /// </summary>
        public static ZoneBattleRuntime Demo(ulong seed = 20260704UL)
        {
            var lead = new OffensiveGeneral(new CharacterId("char-player-lord"), FixedPoint.FromFraction(7, 10), FixedPoint.FromFraction(7, 10), FixedPoint.FromFraction(8, 10));
            var comp = new TroopComposition(new Dictionary<TroopType, int> { [TroopType.Cavalry] = 300, [TroopType.Infantry] = 300 });
            var prep = new OffensivePreparation(
                600, 300, new OffensiveCommand(lead), comp, ApproachPlan.FeintLure,
                new OffensiveTiming(ThreeKingdom.Domain.Time.DaySegment.Day, ThreeKingdom.Domain.Environment.WeatherType.Clear),
                TerrainKind.Pass, scouted: true);
            FixedPoint morale = new OffensiveSetupService().Derive(prep, OffensiveSetupConfig.Default).Morale;
            return FromOffensive(prep, morale, garrison: 300, seed: seed);
        }

        /// <summary>当前战斗态（只读）。</summary>
        public ZoneBattleState State => _state;
        /// <summary>玩家阵营。</summary>
        public BattleSide PlayerSide => _state.PlayerSide;
        /// <summary>当前终局。</summary>
        public ZoneBattleOutcome Outcome => _outcome;
        /// <summary>是否已分胜负。</summary>
        public bool IsOver => _outcome != ZoneBattleOutcome.Ongoing;

        /// <summary>调动己方支队到相邻区（战中调整；返回命令结果，失败含稳定错误码）。战斗已终局则不动。</summary>
        public ZoneCommandResult MoveDetachment(string detachmentId, string zoneId)
        {
            if (IsOver) return ZoneCommandResult.Failure(ZoneCommandError.None, _state, "战斗已结束。");
            ZoneCommandResult r = _commands.MoveDetachment(_state, PlayerSide, new DetachmentId(detachmentId), new ZoneId(zoneId));
            if (r.Applied) _state = r.State;
            return r;
        }

        /// <summary>改己方支队姿态（主攻/佯攻/守）。</summary>
        public ZoneCommandResult SetPosture(string detachmentId, Posture posture)
        {
            if (IsOver) return ZoneCommandResult.Failure(ZoneCommandError.None, _state, "战斗已结束。");
            ZoneCommandResult r = _commands.SetPosture(_state, PlayerSide, new DetachmentId(detachmentId), posture);
            if (r.Applied) _state = r.State;
            return r;
        }

        private readonly System.Collections.Generic.HashSet<string> _fallenRoused = new System.Collections.Generic.HashSet<string>();
        private readonly ThreeKingdom.Domain.Characters.BondEffectService _bondSvc = new ThreeKingdom.Domain.Characters.BondEffectService();

        /// <summary>
        /// 羁绊崩解（GDD_025 R4）：本回合有血脉/知己之将阵亡（支队溃散）→ 同阵营与其生死之交的余将<b>狂怒死战</b>，
        /// 士气骤升（BondConfig.CollapseRage）。每位阵亡者只触发一次。确定性纯数据变换。
        /// </summary>
        private ZoneBattleState ApplyBondCollapse(ZoneBattleState state)
        {
            var fallen = new System.Collections.Generic.List<(ThreeKingdom.Domain.Characters.CharacterId G, BattleSide Side)>();
            foreach (Detachment d in state.Detachments)
                if (d.IsBroken && d.General != null && d.General.Character.Value != null && _fallenRoused.Add(d.General.Character.Value))
                    fallen.Add((d.General.Character, d.Side));
            if (fallen.Count == 0) return state;

            var bonds = ThreeKingdom.Application.Scenarios.GeneralBonds.All;
            FixedPoint rage = ThreeKingdom.Domain.Characters.BondConfig.Default.CollapseRage;
            var list = new System.Collections.Generic.List<Detachment>();
            bool changed = false;
            foreach (Detachment d in state.Detachments)
            {
                Detachment cur = d;
                if (!d.IsBroken && d.General != null)
                    foreach ((ThreeKingdom.Domain.Characters.CharacterId g, BattleSide side) in fallen)
                        if (side == d.Side && _bondSvc.IsRousedByFall(d.General.Character, g, bonds))
                        {
                            cur = cur.WithCombat(cur.Strength, (cur.Morale + rage).Clamp(FixedPoint.Zero, FixedPoint.One), cur.Fatigue);
                            changed = true;
                        }
                list.Add(cur);
            }
            return changed ? state.WithDetachments(list) : state;
        }

        /// <summary>推进一回合（敌AI决策 → 同步结算 → 终局判定）；返回战后展示投影。已终局则原样返回。</summary>
        public ZoneBattleView ResolveRound()
        {
            if (!IsOver)
            {
                ZoneBattleRoundResult r = _service.ResolveRound(_state, _context, _config, _aiConfig);
                _state = ApplyBondCollapse(r.State);   // 羁绊崩解：血脉/知己同场阵亡 → 余者狂怒死战（GDD_025 R4）
                _lastEmergences = new List<string>(r.Emergences);
                _outcome = r.Outcome;
            }
            return View();
        }

        /// <summary>
        /// AI 代打一回合（GDD_021：玩家可选亲自打或挂 AI）：<b>玩家方也由 AI 决策</b>（同一套角色感知 AI，
        /// 受同规则、不作弊）→ 再走标准回合（敌AI + 结算）。胜负仍由部署/对阵决定——<b>代打不保证赢</b>。
        /// </summary>
        public ZoneBattleView AutoResolveRound()
        {
            if (IsOver) return View();
            _state = _autoAi.Decide(_state, PlayerSide, _config, _aiConfig);   // AI 替玩家指挥
            return ResolveRound();                                             // 敌AI + 同步结算
        }

        /// <summary>AI 代打至终局（挂机）：双方皆 AI，确定性，胜负由准备/对阵决定（可能赢也可能输）。</summary>
        public ZoneBattleView AutoResolve()
        {
            int guard = 0;
            while (!IsOver && guard++ < 100) AutoResolveRound();
            return View();
        }

        /// <summary>当前战斗展示投影（各区态势 + 上回合涌现 + 终局）。</summary>
        public ZoneBattleView View() => ZoneBattleView.FromState(_state, _outcome, _lastEmergences, _context.AttackerScouted);
    }
}
