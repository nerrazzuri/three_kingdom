using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Battle;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Numerics;
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

        /// <summary>由出征六维准备开战（攻方经部署桥分区、守方由守备布防；玩家=攻方）。</summary>
        public static ZoneBattleRuntime FromOffensive(
            OffensivePreparation prep, FixedPoint morale, int garrison, ulong seed, int maxRounds = 6)
        {
            var field = BattleField.Default();
            var planner = new OffensiveDeploymentPlanner();
            var dets = new List<Detachment>();
            dets.AddRange(planner.PlanAttacker(prep, morale, field));
            dets.AddRange(planner.PlanDefender(new SiegeDefense(garrison, FixedPoint.FromFraction(12, 10)), FixedPoint.FromFraction(7, 10), field));
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

        /// <summary>推进一回合（敌AI决策 → 同步结算 → 终局判定）；返回战后展示投影。已终局则原样返回。</summary>
        public ZoneBattleView ResolveRound()
        {
            if (!IsOver)
            {
                ZoneBattleRoundResult r = _service.ResolveRound(_state, _context, _config, _aiConfig);
                _state = r.State;
                _lastEmergences = new List<string>(r.Emergences);
                _outcome = r.Outcome;
            }
            return View();
        }

        /// <summary>当前战斗展示投影（各区态势 + 上回合涌现 + 终局）。</summary>
        public ZoneBattleView View() => ZoneBattleView.FromState(_state, _outcome, _lastEmergences);
    }
}
