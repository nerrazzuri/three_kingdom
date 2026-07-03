using System;
using System.Collections.Generic;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Presentation.Runtime
{
    /// <summary>
    /// 复盘屏<b>临时</b>演示战局（epic-028 story-002 evidence 用；story-004 HUD 接真实「备战→开战」流程后移除）。
    /// 在当前会话上按 console harness 已验证的确定性序列走一局：补齐计划→开战→解析→（胜局标满伏击条件）→
    /// 识别兵法→结算战果，返回复盘展示模型。
    /// <b>全程只经 <see cref="CampaignSessionService"/> 命令</b>（ADR-0002/0009——本类零规则，只编排既有用例）；
    /// 固定种子/夹具 → 同分支同结果（ADR-0004）。
    /// </summary>
    public static class BattleReviewDemo
    {
        /// <summary>假退伏击三条件（与 console harness 同源顺序；兵法=条件组合，非按钮）。</summary>
        private static readonly TacticCondition[] FeintAmbushConditions =
        {
            TacticCondition.ControlledRetreatKeptFormation,
            TacticCondition.EnemyPursued,
            TacticCondition.AmbushSurprise,
        };

        /// <summary>
        /// 在 <paramref name="session"/> 上演示一局并结算为 <paramref name="branch"/> 分支，返回复盘模型。
        /// 幂等防重：已有计划/战斗则跳过对应步骤（重复点击不重复添加命令）。
        /// </summary>
        public static BattleReviewView Run(
            CampaignSession session, PlayableCampaign scenario, OutcomeBranch branch, BattleReviewTuning tuning)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));

            var service = new CampaignSessionService();

            // 备战（幂等）：无已提交计划才补齐（草稿→原子提交）。
            if (session.CommittedPlan == null)
            {
                service.AddPlanOrder(session, scenario.AmbushPlan());
                SubmitPlanResult submitted = service.SubmitPlan(session);
                if (!submitted.Committed)
                    throw new InvalidOperationException("演示战局：计划提交被拒（应为已验证夹具，属编程错误）。");
            }

            // 开战（幂等）+ 解析一个阶段（确定性种子）。
            if (!session.HasBattle)
            {
                CampaignCommandResult started = service.StartBattle(
                    session, scenario.Units(), scenario.BattleConfig, scenario.BattleSeed, scenario.TacticChains);
                if (!started.Applied)
                    throw new InvalidOperationException($"演示战局：开战失败（{started.Error}）。");
            }
            var orders = new[]
            {
                new BattleOrder(0, PlayableCampaign.PlayerUnit, BattleOrderType.Engage,
                    targetUnit: PlayableCampaign.EnemyUnit),
            };
            service.ResolveBattlePhase(session, orders);

            // 胜局：标满假退伏击条件。防重经 public 复盘出口判断（会话内部条件集为 internal——
            // 表现层类型边界正确挡住直读；已识别出该兵法 = 三条件已全，跳过再标）。
            if (branch == OutcomeBranch.Victory && !HasTactic(service.RecognizeTactics(session), TacticTag.FeintAmbush))
            {
                foreach (TacticCondition cond in FeintAmbushConditions)
                    service.MarkTacticCondition(session, cond);
            }

            IReadOnlyList<RecognizedTactic> tactics = service.RecognizeTactics(session);

            var context = new OutcomeContext(PlayableCampaign.Player, PlayableCampaign.Fanshui);
            OutcomeContinuation continuation = service.ResolveBattleOutcome(
                session, branch, context, scenario.OutcomeConfig);

            CareerGain? gainPreview = branch == OutcomeBranch.Victory
                ? scenario.Ladder.GainFor(CareerGainSource.CombatVictory)
                : null;
            CareerState career = session.Career.Career;

            return BattleReviewView.From(
                branch, continuation.Consequences.Changes, tactics, continuation.Options,
                gainPreview, career.Merit, career.Renown, tuning);
        }

        private static bool HasTactic(IReadOnlyList<RecognizedTactic> tactics, TacticTag tag)
        {
            foreach (RecognizedTactic t in tactics)
                if (t.Tag == tag) return true;
            return false;
        }
    }
}
