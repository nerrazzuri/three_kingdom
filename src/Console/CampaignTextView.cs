using System.Collections.Generic;
using System.Text;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Console
{
    /// <summary>
    /// 把 <see cref="CampaignSession"/> 的只读投影渲染为文本（M15 表现层）。<b>纯函数</b>：无 I/O、无副作用、
    /// 不持状态——同一会话态恒产同一字符串，便于单测。仅读 Application/Domain 暴露的只读出口（反全知：
    /// 情报只读玩家知识 <see cref="CampaignSession.PlayerKnowledge"/>，绝不读真值）。float/double 仅用于
    /// 非权威显示换算（ADR-0004 允许 Presentation 用浮点）。
    /// </summary>
    public static class CampaignTextView
    {
        /// <summary>Q16.16 定点 → 显示用小数（仅展示，非权威）。</summary>
        private static double Dec(FixedPoint v) => v.Raw / 65536.0;

        private static string Segment(DaySegment s) => s switch
        {
            DaySegment.Dawn => "黎明",
            DaySegment.Day => "白昼",
            DaySegment.Dusk => "黄昏",
            DaySegment.Night => "夜晚",
            _ => s.ToString(),
        };

        private static string RankName(Rank r) => r switch
        {
            Rank.CityGovernor => "城池太守",
            Rank.SeniorGovernor => "资深太守",
            Rank.ProvincialInspector => "州刺史",
            Rank.RegionalGeneral => "四方中郎将",
            Rank.GuardianGeneral => "镇国将军",
            Rank.DeputyCommander => "副都督",
            Rank.GrandCommander => "大都督",
            Rank.Successor => "继承基业",
            _ => r.ToString(),
        };

        /// <summary>主状态面板。</summary>
        public static string Status(CampaignSession s)
        {
            var b = new StringBuilder();
            WorldTime t = s.CurrentTime;
            b.AppendLine("════════════════════════════════════════════");
            b.AppendLine($" 汜水关太守 · 第 {t.Day} 日 {Segment(t.Segment)}");
            b.AppendLine("════════════════════════════════════════════");

            CareerState c = s.Career.Career;
            string faction = c.IsUnaffiliated ? "在野" : (c.Faction?.Value ?? "—");
            b.AppendLine($" 生涯：{RankName(c.Rank)} | 功绩 {c.Merit} | 名望 {c.Renown} | 君主好感 {Dec(c.LordStanding):0.00} | 归属 {faction}");

            if (s.HasCityGovernance)
            {
                var ce = s.CityEconomy!;
                b.AppendLine($" 城池：粮 {ce.Stock} | 民心 {ce.CivMorale} | 治安 {ce.Security} | 城防 {ce.FortificationCurrent}/{ce.FortificationMax} | 后勤 {s.LogisticsHolding}");
            }

            if (s.HasIntel)
                b.AppendLine($" 情报：{IntelLine(s.PlayerKnowledge!, t)}");

            if (s.HasPreparation)
            {
                int orders = s.PlanOrders?.Count ?? 0;
                string committed = s.CommittedPlan != null ? "已提交" : "未提交";
                b.AppendLine($" 战役准备：草稿命令 {orders} 条 | 计划 {committed}");
            }

            if (s.HasBattle)
                b.AppendLine(" 战斗：进行中（标记伏击条件后可复盘识别兵法）");

            if (s.HasOutcome)
                b.AppendLine($" 上一战果：{Branch(s.LastOutcomeBranch!.Value)} | 可继续选项 {s.LastContinuationOptions.Count} 个");

            if (s.HasHistory)
            {
                int triggered = s.World.TriggeredEvents.Count;
                int diverged = s.World.DivergedEvents.Count;
                b.AppendLine($" 历史世界：事件已触发 {triggered} 起，其中分叉 {diverged} 起");
            }

            return b.ToString();
        }

        private static string IntelLine(IntelProjection k, WorldTime now)
        {
            if (k.Count == 0) return "敌情未明（尚未侦察）";
            var b = new StringBuilder();
            bool first = true;
            foreach (IntelKnowledgeEntry e in k.Entries)
            {
                if (!first) b.Append("；");
                int ageSegs = (now.Day - e.ObservedAt.Day) * WorldTime.SegmentsPerDay + ((int)now.Segment - (int)e.ObservedAt.Segment);
                b.Append($"{e.Subject.Value}≈{e.KnownStrength}（{e.Source}，{ageSegs} 段前）");
                first = false;
            }
            return b.ToString();
        }

        private static string Branch(OutcomeBranch br) => br switch
        {
            OutcomeBranch.Victory => "胜利",
            OutcomeBranch.Retreat => "撤退",
            OutcomeBranch.CityLost => "失城",
            OutcomeBranch.Defeat => "败北",
            _ => br.ToString(),
        };

        /// <summary>军议建议集（GDD_008：缘由/条件/风险/缺失情报/置信；不排优劣、不报成功率）。</summary>
        public static string Council(CouncilAdviceSet? set)
        {
            if (set == null || set.Advice.Count == 0) return "（军师无可呈条件化建议）";
            var b = new StringBuilder();
            b.AppendLine("【军议】军师只陈缘由与条件，不替你定计、不报胜率：");
            int i = 1;
            foreach (AdviceStatement a in set.Advice)
            {
                b.AppendLine($" {i}. 缘由：{a.Observation}（{a.Assumption}）");
                b.AppendLine($"    所需条件：{string.Join("、", a.RequiredConditions)}");
                b.AppendLine($"    风险：{string.Join("、", a.Risks)}");
                if (a.MissingIntel.Count > 0)
                {
                    var miss = new List<string>();
                    foreach (IntelSubjectId m in a.MissingIntel) miss.Add(m.Value);
                    b.AppendLine($"    待证情报：{string.Join("、", miss)}");
                }
                b.AppendLine($"    完整度/置信：{Dec(a.Confidence):0.00}");
                i++;
            }
            return b.ToString();
        }

        /// <summary>复盘识别出的兵法（事后涌现，非按钮）。</summary>
        public static string Tactics(IReadOnlyList<RecognizedTactic> tactics)
        {
            if (tactics.Count == 0) return "（条件未全，未识别出成型兵法——兵法是条件组合，非按钮）";
            var b = new StringBuilder();
            b.AppendLine("【复盘】识别出的兵法：");
            foreach (RecognizedTactic rt in tactics)
                b.AppendLine($" · {TagName(rt.Tag)}（满足条件 {rt.MatchedConditions.Count} 条）");
            return b.ToString();
        }

        private static string TagName(TacticTag tag) => tag switch
        {
            TacticTag.FeintAmbush => "假退伏击",
            TacticTag.SupplyExhaustion => "断粮疲敌",
            TacticTag.HoldUntilRelief => "守城待变",
            TacticTag.NightRaid => "夜袭",
            _ => tag.ToString(),
        };

        /// <summary>命令菜单。</summary>
        public static string Menu()
            => "命令： [1]推进时段 [2]征用军粮 [3]修缮城防 [4]安抚民心 [5]侦察敌情 [6]召开军议\n"
             + "       [7]备战设伏并提交 [8]开战 [9]解析战斗 [m]标记伏击条件 [t]识别兵法\n"
             + "       [o]结算战果·胜 [p]结算战果·败 [c]记功并申请晋升 [r]检定自立 [h]推进历史\n"
             + "       [s]存档 [l]读档 [?]重示菜单 [q]退出";
    }
}
