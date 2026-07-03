using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// 战果复盘屏 ViewModel（epic-028 story-002 / TR-ux-001 因果 + TR-ux-004 续局）。
    /// 验证：默认折叠一句话结论 + 一键展开（2026-07-03 Q3 裁决）；主因素 ≤5 截取；兵法条件/成型兵法列出；
    /// 四分支各 ≥1 续局选项且全文无「游戏结束/删档/胜率」；败类分支明示可继续；长线记功提示；
    /// 同输入渲染恒等 + 展开/折叠往返内容不变（TR-ux-005）。
    /// 治理 ADR：ADR-0002（只读投影）+ ADR-0004（确定性）。
    /// </summary>
    [TestFixture]
    public class BattleReviewViewModelTests
    {
        private static readonly CityId City = new CityId("city-test");

        /// <summary>构造 N 条权威变更（因素输入夹具；Reason/Delta 由 Domain 类型承载）。</summary>
        private static IReadOnlyList<OutcomeChange> Changes(int count)
        {
            var list = new List<OutcomeChange>();
            for (int i = 0; i < count; i++)
                list.Add(OutcomeChange.ForCity(City, CityField.CivMorale, -(i + 1), $"因素{i + 1}：战后民心波动"));
            return list;
        }

        private static IReadOnlyList<RecognizedTactic> AmbushTactic()
            => new[]
            {
                new RecognizedTactic(TacticTag.FeintAmbush, new[]
                {
                    TacticCondition.ControlledRetreatKeptFormation,
                    TacticCondition.EnemyPursued,
                    TacticCondition.AmbushSurprise,
                }),
            };

        private static IReadOnlyList<ContinuationOption> Options()
            => new[] { new ContinuationOption(ContinuationCommandKind.Regroup, "收拢部众，重整旗鼓") };

        [Test]
        public void test_review_defaults_collapsed_with_one_line_conclusion()
        {
            // Arrange / Act
            BattleReviewView view = BattleReviewView.From(
                OutcomeBranch.Victory, Changes(3), AmbushTactic(), Options(),
                gainPreview: null, currentMerit: 0, currentRenown: 0, BattleReviewTuning.Default);

            // Assert：默认折叠——只见结论，因素不可见；结论含分支+首要原因（成型兵法优先）。
            Assert.That(view.IsExpanded, Is.False);
            Assert.That(view.VisibleFactorLines, Is.Empty);
            Assert.That(view.ConclusionLabel, Does.Contain("胜利"));
            Assert.That(view.ConclusionLabel, Does.Contain("假退伏击"));
            Assert.That(view.ToggleLabel, Is.EqualTo("展开因果"));
        }

        [Test]
        public void test_expand_reveals_at_most_max_factors()
        {
            // Arrange：7 条变更 > 上限 5。
            BattleReviewView view = BattleReviewView.From(
                OutcomeBranch.Defeat, Changes(7), new RecognizedTactic[0], Options(),
                null, 0, 0, BattleReviewTuning.Default);

            // Act
            BattleReviewView expanded = view.Expand();

            // Assert：≤5 主因素（权威顺序前 5 条）；折叠态计数一致（展开不改内容）。
            Assert.That(expanded.IsExpanded, Is.True);
            Assert.That(expanded.VisibleFactorLines.Count, Is.EqualTo(5));
            Assert.That(expanded.VisibleFactorLines[0], Does.Contain("因素1"));
            Assert.That(expanded.FactorCount, Is.EqualTo(view.FactorCount));
            Assert.That(expanded.ToggleLabel, Is.EqualTo("收起因果"));
        }

        [Test]
        public void test_expanded_lists_tactic_conditions_and_recognized_tactic()
        {
            // Arrange / Act
            BattleReviewView view = BattleReviewView.From(
                OutcomeBranch.Victory, Changes(2), AmbushTactic(), Options(),
                null, 0, 0, BattleReviewTuning.Default).Expand();

            // Assert：成型兵法名 + 满足条件数（TR-ux-001）。
            Assert.That(view.TacticLines.Count, Is.EqualTo(1));
            Assert.That(view.TacticLines[0], Does.Contain("假退伏击"));
            Assert.That(view.TacticLines[0], Does.Contain("3 条"));
        }

        [Test]
        public void test_no_tactic_recognized_states_condition_principle()
        {
            // Arrange / Act：条件不全 → 无成型兵法，明示原则而非空白。
            BattleReviewView view = BattleReviewView.From(
                OutcomeBranch.Defeat, Changes(1), new RecognizedTactic[0], Options(),
                null, 0, 0, BattleReviewTuning.Default);

            // Assert
            Assert.That(view.TacticLines[0], Does.Contain("条件组合"));
        }

        [Test]
        public void test_four_branches_each_offer_continuation_and_no_gameover_wording()
        {
            // Arrange / Act：四分支在真实会话上各演示一局（真实后果写回路径）。
            foreach (OutcomeBranch branch in new[]
                     { OutcomeBranch.Victory, OutcomeBranch.Retreat, OutcomeBranch.CityLost, OutcomeBranch.Defeat })
            {
                CampaignSession session = NewSession();
                BattleReviewView view = ScriptedBattle.Run(
                    session, PlayableCampaign.Default(), branch, BattleReviewTuning.Default);

                // Assert：每分支 ≥1 续局选项（TR-ux-004）；全文无禁用措辞（AC-2/AC-4 反例清单）。
                Assert.That(view.Options.Count, Is.GreaterThanOrEqualTo(1), $"分支 {branch} 无续局选项");
                string all = AllText(view.Expand());
                Assert.That(all, Does.Not.Contain("游戏结束"), $"分支 {branch}");
                Assert.That(all, Does.Not.Contain("删档"), $"分支 {branch}");
                Assert.That(all, Does.Not.Contain("Game Over"), $"分支 {branch}");
                Assert.That(all, Does.Not.Contain("胜率"), $"分支 {branch}");
                Assert.That(all, Does.Not.Contain("成功率"), $"分支 {branch}");
                if (branch != OutcomeBranch.Victory)
                    Assert.That(view.ContinuationNotice, Does.Contain("可继续"), $"分支 {branch} 未明示可继续");
            }
        }

        [Test]
        public void test_career_hint_shows_gain_and_current_values()
        {
            // Arrange / Act：胜局演示（记功预览来自梯队配置 CombatVictory=+40/+10）。
            CampaignSession session = NewSession();
            BattleReviewView view = ScriptedBattle.Run(
                session, PlayableCampaign.Default(), OutcomeBranch.Victory, BattleReviewTuning.Default);

            // Assert：长线提示含增量与当前值（果→长线引导，Q2 卡点裁决）。
            Assert.That(view.CareerHintLabel, Does.Contain("+40"));
            Assert.That(view.CareerHintLabel, Does.Contain("+10"));
            Assert.That(view.CareerHintLabel, Does.Contain("晋升"));
        }

        [Test]
        public void test_render_is_deterministic_and_toggle_roundtrip_keeps_content()
        {
            // Arrange
            BattleReviewView a = BattleReviewView.From(
                OutcomeBranch.Victory, Changes(4), AmbushTactic(), Options(),
                null, 40, 10, BattleReviewTuning.Default);
            BattleReviewView b = BattleReviewView.From(
                OutcomeBranch.Victory, Changes(4), AmbushTactic(), Options(),
                null, 40, 10, BattleReviewTuning.Default);

            // Act
            BattleReviewView roundTrip = a.Expand().Collapse().Expand();

            // Assert：同输入两次构造逐字段相等（渲染恒等）；展开→折叠→展开内容不变。
            Assert.That(AllText(a.Expand()), Is.EqualTo(AllText(b.Expand())));
            Assert.That(AllText(roundTrip), Is.EqualTo(AllText(a.Expand())));
        }

        [Test]
        public void test_demo_battle_same_branch_is_deterministic_across_sessions()
        {
            // Arrange / Act：两个独立新会话跑同分支演示。
            BattleReviewView first = ScriptedBattle.Run(
                NewSession(), PlayableCampaign.Default(), OutcomeBranch.Victory, BattleReviewTuning.Default);
            BattleReviewView second = ScriptedBattle.Run(
                NewSession(), PlayableCampaign.Default(), OutcomeBranch.Victory, BattleReviewTuning.Default);

            // Assert：同配置+同分支 → 逐字段相同复盘（ADR-0004）。
            Assert.That(AllText(second.Expand()), Is.EqualTo(AllText(first.Expand())));
        }

        private static CampaignSession NewSession()
        {
            CampaignStartResult r = new CampaignSessionService().StartCampaign(PlayableCampaign.Default().StartConfig);
            Assert.That(r.Started, Is.True, "前置：场景开局应成功。");
            return r.Session!;
        }

        /// <summary>拼接全部可见文本（禁用措辞扫描面 + 恒等比较面）。</summary>
        private static string AllText(BattleReviewView view)
        {
            var b = new StringBuilder();
            b.AppendLine(view.BranchLabel).AppendLine(view.ConclusionLabel).AppendLine(view.ToggleLabel);
            foreach (string line in view.VisibleFactorLines) b.AppendLine(line);
            foreach (string line in view.TacticLines) b.AppendLine(line);
            foreach (BattleReviewOptionView o in view.Options) b.AppendLine(o.KindLabel).AppendLine(o.Reason);
            b.AppendLine(view.ContinuationNotice).AppendLine(view.CareerHintLabel);
            return b.ToString();
        }
    }
}
