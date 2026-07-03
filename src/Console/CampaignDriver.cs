using System;
using System.Collections.Generic;
using System.IO;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Console
{
    /// <summary>
    /// M15 交互驱动：把单个输入符映射到一条 <see cref="CampaignSessionService"/> 命令，回传人类可读因果反馈。
    /// <b>所有状态变更只经 Application 用例</b>（ADR-0002/0009 接缝执行端），不碰可变 Domain 对象。
    /// 命令分派与反馈是确定性纯逻辑（同会话态 + 同输入 → 同结果），I/O 隔离在 <see cref="Program"/>，便于单测。
    /// 会话由本驱动持有并按命令推进（CampaignSession 为可变会话句柄，经服务方法推进）。
    /// </summary>
    public sealed class CampaignDriver
    {
        private static readonly TacticCondition[] FeintAmbushConditions =
        {
            TacticCondition.ControlledRetreatKeptFormation,
            TacticCondition.EnemyPursued,
            TacticCondition.AmbushSurprise,
        };

        private readonly CampaignSessionService _service;
        private readonly PlayableCampaign _scenario;
        private readonly string _savePath;
        private int _ambushMarkIndex;

        /// <summary>当前会话。</summary>
        public CampaignSession Session { get; private set; }

        /// <summary>玩家是否已请求退出。</summary>
        public bool Quit { get; private set; }

        public CampaignDriver(string? savePath = null)
        {
            _service = new CampaignSessionService();
            _scenario = PlayableCampaign.Default();
            _savePath = savePath ?? "campaign.save";
            CampaignStartResult start = _service.StartCampaign(_scenario.StartConfig);
            if (!start.Started)
                throw new InvalidOperationException($"默认场景开局失败：{start.Error}");
            Session = start.Session!;
        }

        /// <summary>执行一个输入符，返回因果反馈文本。空输入返回空串（仅重绘）。</summary>
        public string Step(string? input)
        {
            string key = (input ?? string.Empty).Trim().ToLowerInvariant();
            if (key.Length == 0) return string.Empty;

            try
            {
                return Dispatch(key);
            }
            catch (Exception ex)
            {
                // 防御：任何 Domain 前置异常转友好反馈，不打断 REPL。
                return $"× 该操作此刻不可用：{ex.Message}";
            }
        }

        private string Dispatch(string key) => key switch
        {
            "1" => AdvanceSegment(),
            "2" => Govern(_service.RequisitionFood(Session, 30), "征用军粮 30（移交后勤、损民心）"),
            "3" => Govern(_service.RepairFortification(Session), "修缮城防"),
            "4" => Govern(_service.Appease(Session), "安抚民心"),
            "5" => Scout(),
            "6" => Convene(),
            "7" => Prepare(),
            "8" => StartBattle(),
            "9" => ResolvePhase(),
            "m" => MarkAmbushCondition(),
            "t" => Recognize(),
            "o" => Outcome(OutcomeBranch.Victory, "胜利"),
            "p" => Outcome(OutcomeBranch.CityLost, "失城"),
            "c" => CareerStep(),
            "r" => Rebellion(),
            "h" => History(),
            "s" => Save(),
            "l" => Load(),
            "?" => CampaignTextView.Menu(),
            "q" => Stop(),
            _ => $"× 未知命令「{key}」。按 [?] 重示菜单。",
        };

        private string AdvanceSegment()
        {
            _service.Advance(Session, 1);
            var t = Session.CurrentTime;
            return $"✓ 时段推进 → 第 {t.Day} 日 {t.Segment}（跨日界则城市日结、敌援/情报时效随之变化）";
        }

        private string Govern(CampaignCommandResult r, string label)
            => r.Applied ? $"✓ {label}" : $"× {label}失败：{r.Error}（{r.Detail}）";

        private string Scout()
        {
            CampaignCommandResult r = _service.Scout(Session, PlayableCampaign.EnemyArmy, IntelSource.Scouting);
            if (!r.Applied) return $"× 侦察失败：{r.Error}（{r.Detail}）";
            return "✓ 侦察归来，敌军主力估值并入你的阵营知识（只见估计值与时效，非真值——反全知）";
        }

        private string Convene()
        {
            CouncilAdviceSet set = _service.ConveneCouncil(Session);
            return CampaignTextView.Council(set);
        }

        private string Prepare()
        {
            CampaignCommandResult add = _service.AddPlanOrder(Session, _scenario.AmbushPlan());
            if (!add.Applied) return $"× 加入设伏命令失败：{add.Error}（{add.Detail}）";
            SubmitPlanResult sub = _service.SubmitPlan(Session);
            if (!sub.Committed)
                return $"× 计划提交被拒（原子，无部分写入）：{DescribeRejection(sub.Validation)}";
            return "✓ 设伏命令已纳入草稿并原子提交为可执行计划（军粮一次性扣减）。可 [8] 开战";
        }

        private static string DescribeRejection(PlanValidationResult validation)
        {
            var reasons = new List<string>();
            foreach (PlanError e in validation.Errors)
                reasons.Add(e.Code + "：" + e.Detail);
            return reasons.Count == 0 ? "（无具体冲突）" : string.Join("；", reasons);
        }

        private string StartBattle()
        {
            CampaignCommandResult r = _service.StartBattle(
                Session, _scenario.Units(), _scenario.BattleConfig, _scenario.BattleSeed, _scenario.TacticChains);
            if (!r.Applied) return $"× 开战失败：{r.Error}（{r.Detail}）。需先 [7] 备战并提交计划";
            _ambushMarkIndex = 0;
            return "✓ 据已提交计划开战（敌方为确定性预设；智能 AI 见战术对手循环）。可 [9] 解析战斗、[m] 标记伏击条件";
        }

        private string ResolvePhase()
        {
            if (!Session.HasBattle) return "× 尚未开战。先 [7] 备战、[8] 开战";
            var orders = new[]
            {
                new BattleOrder(0, PlayableCampaign.PlayerUnit, BattleOrderType.Engage, targetUnit: PlayableCampaign.EnemyUnit),
            };
            BattleResolution res = _service.ResolveBattlePhase(Session, orders);
            return res.Committed
                ? $"✓ 战斗阶段已解析（确定性，{res.Events.Count} 个战斗事件）"
                : $"× 战斗阶段回滚（战斗态不变）：{res.Error}";
        }

        private string MarkAmbushCondition()
        {
            if (!Session.HasBattle) return "× 尚未开战，无法标记战斗中条件";
            if (_ambushMarkIndex >= FeintAmbushConditions.Length)
                return "（假退伏击三条件已全部标记。[t] 识别兵法应可识别出「假退伏击」）";
            TacticCondition cond = FeintAmbushConditions[_ambushMarkIndex++];
            _service.MarkTacticCondition(Session, cond);
            int remain = FeintAmbushConditions.Length - _ambushMarkIndex;
            return $"✓ 已涌现条件：{cond}（假退伏击还差 {remain} 条——兵法是条件组合，非按钮）";
        }

        private string Recognize()
        {
            if (!Session.HasBattle) return "× 尚未开战，无可复盘";
            IReadOnlyList<RecognizedTactic> tactics = _service.RecognizeTactics(Session);
            return CampaignTextView.Tactics(tactics);
        }

        private string Outcome(OutcomeBranch branch, string label)
        {
            var context = new OutcomeContext(PlayableCampaign.Player, PlayableCampaign.Fanshui);
            OutcomeContinuation cont = _service.ResolveBattleOutcome(Session, branch, context, _scenario.OutcomeConfig);
            if (!cont.Writeback.Committed)
                return $"× 战果写回失败（原子，城市态不变）。";
            int n = cont.Options.Count;
            return $"✓ 已结算战果·{label}，后果原子写回。续局选项 {n} 个"
                 + (branch != OutcomeBranch.Victory ? "——失利亦可继续（失败不删档，可重整）" : string.Empty);
        }

        private string CareerStep()
        {
            CareerCommandResult gain = _service.ApplyCareerGain(Session, _scenario.Ladder, CareerGainSource.CombatVictory);
            if (!gain.Applied) return $"× 记功失败：{gain.Error}（{gain.Detail}）";
            CareerState c = Session.Career.Career;
            CareerCommandResult promo = _service.RequestPromotion(Session, _scenario.Ladder);
            string promoLine = promo.Applied
                ? $"晋升至 {Session.Career.Career.Rank}！"
                : $"未达晋升门槛（{promo.Error}）——可经治理/招揽等非战斗功绩继续积累";
            return $"✓ 战功记入：功绩 {c.Merit}/名望 {c.Renown}。{promoLine}";
        }

        private string Rebellion()
        {
            var ctx = new RebellionContext(
                citiesOwned: 1, supplyReady: true, troopsReady: true, lordOppression: false,
                PlayableCampaign.RebelFaction);
            RebellionEligibility elig = _service.CheckRebellionEligibility(Session, _scenario.Rebellion, ctx);
            if (!elig.CanRebel)
                return $"自立检定：未达资格（军事组={elig.MilitaryGroupMet} 民望组={elig.PopularGroupMet} 压迫={elig.OppressionMet}）。"
                     + "三分支任一达成方可自立——实力/民望不足时自立必败，故为硬门槛";
            RebellionResult r = _service.LaunchRebellion(Session, _scenario.Rebellion, ctx);
            return r.Launched
                ? $"✓ 举兵自立，另立新势力 {PlayableCampaign.RebelFaction.Value}"
                : $"× 自立未成：{r.Error}（{r.Detail}）——在野亦为合法续局，不死局";
        }

        private string History()
        {
            IReadOnlyList<HistoryAdvanceResult> results = _service.AdvanceHistory(Session);
            if (results.Count == 0)
                return "（当前无到期历史事件。推进时段 [1] 后再试——历史按演义时间线在轨上跑）";
            var b = new System.Text.StringBuilder();
            b.AppendLine("【历史世界推进】");
            foreach (HistoryAdvanceResult r in results)
            {
                string verb = !r.Fired ? "未触发"
                    : r.Diverged ? "分叉（你已强到改变前置，历史脱稿）"
                    : "按演义正常结局（你够不着或前置成立）";
                b.AppendLine($" · {r.Reason}：{verb}");
            }
            return b.ToString();
        }

        private string Save()
        {
            string snapshot = _service.CaptureSnapshot(Session);
            File.WriteAllText(_savePath, snapshot);
            return $"✓ 已存档至 {_savePath}（确定性快照，全循环态）";
        }

        private string Load()
        {
            if (!File.Exists(_savePath)) return $"× 无存档可读（{_savePath} 不存在）";
            string text = File.ReadAllText(_savePath);
            CampaignStartConfig cfg = _scenario.StartConfig;
            // 数据驱动配置不入存档体（ADR-0009 §R-1），由载入方按指纹回供。
            Session = _service.Restore(
                text, cfg.Fingerprint,
                cfg.SettlementConfig, cfg.GovernanceConfig, cfg.PopulationPressure,
                cfg.IntelConfig, cfg.CouncilSetup, cfg.PreparationConfig,
                cfg.ReachableRegions, cfg.AuthorizedOrders,
                _scenario.BattleConfig, _scenario.TacticChains);
            return $"✓ 已从 {_savePath} 读档恢复会话（版本/指纹校验通过，否则整体拒绝不部分载入）";
        }

        private string Stop()
        {
            Quit = true;
            return "再会。汜水关的故事，下次继续。";
        }
    }
}
