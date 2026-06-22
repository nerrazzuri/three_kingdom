using NUnit.Framework;
using ThreeKingdom.Presentation.Screens;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-003：HUD 情境/通知/因果链（可测逻辑，BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0004。覆盖 AC-1 情境→元素集 + 全屏模态隐去 HUD、
    /// AC-6 因果链跳过终值不变、AC-7 通知合并/临界绕队/并发上限。
    /// </summary>
    [TestFixture]
    public class HudViewModelTests
    {
        // ---- AC-1: 情境→元素集 + 模态隐去 ----

        [Test]
        public void test_each_context_shows_only_its_prescribed_elements()
        {
            var judgment = HudContextView.ForContext(HudContext.JudgmentLayout);
            Assert.That(judgment.Shows(HudElement.EnemyReport), Is.True);
            Assert.That(judgment.Shows(HudElement.AdvisorEntry), Is.True);
            Assert.That(judgment.Shows(HudElement.CommandTray), Is.False, "判断布局态无命令托盘。");

            var daily = HudContextView.ForContext(HudContext.DailyObservation);
            Assert.That(daily.Shows(HudElement.EnemyReport), Is.False, "生活观察态无敌方探报。");
        }

        [Test]
        public void test_fullscreen_modal_hides_all_hud_elements()
        {
            var modal = HudContextView.ForContext(HudContext.WarResponse, modalActive: true);
            Assert.That(modal.VisibleElements, Is.Empty, "全屏模态隐去全部 HUD。");
        }

        [Test]
        public void test_context_element_set_is_deterministic()
        {
            var a = HudContextView.ForContext(HudContext.ActionCommit);
            var b = HudContextView.ForContext(HudContext.ActionCommit);
            Assert.That(new List<HudElement>(b.VisibleElements), Is.EquivalentTo(new List<HudElement>(a.VisibleElements)));
        }

        // ---- AC-6: 因果链跳过终值不变 ----

        [Test]
        public void test_causal_chain_skip_equals_stepwise_final_value()
        {
            var chain = CausalChainView.From(1000, new[]
            {
                new CausalStep("正面硬守劣势", -300),
                new CausalStep("断粮疲敌", -200),
                new CausalStep("守城工事加成", 150),
            });
            long expected = 1000 - 300 - 200 + 150;

            // 逐步展开到底。
            var stepwise = chain;
            while (!stepwise.IsFullyRevealed) stepwise = stepwise.RevealNext();

            // 整体跳过。
            var skipped = chain.SkipToEnd();

            Assert.That(chain.FinalValue, Is.EqualTo(expected));
            Assert.That(stepwise.RevealedRunningValue, Is.EqualTo(expected));
            Assert.That(skipped.RevealedRunningValue, Is.EqualTo(expected), "跳过与逐步终值一致。");
            Assert.That(skipped.FinalValue, Is.EqualTo(stepwise.FinalValue));
        }

        // ---- AC-7: 通知合并/临界绕队/并发上限 ----

        [Test]
        public void test_same_kind_within_window_merges()
        {
            var feed = new NotificationFeed();
            feed.Push(new Notification("supply", 1000, false));
            feed.Push(new Notification("supply", 1300, false)); // 300ms < 500ms → 合并

            Assert.That(feed.ActiveToasts.Count, Is.EqualTo(1));
            Assert.That(feed.ActiveToasts[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void test_same_kind_beyond_window_does_not_merge()
        {
            var feed = new NotificationFeed();
            feed.Push(new Notification("supply", 1000, false));
            feed.Push(new Notification("supply", 1600, false)); // 600ms > 500ms → 不合并

            Assert.That(feed.ActiveToasts.Count, Is.EqualTo(2));
        }

        [Test]
        public void test_critical_notification_bypasses_concurrency_cap()
        {
            var feed = new NotificationFeed();
            feed.Push(new Notification("a", 0, false));
            feed.Push(new Notification("b", 0, false));
            feed.Push(new Notification("c", 0, false)); // 3 非临界已满
            feed.Push(new Notification("deadline", 0, true)); // 临界绕队列 + 绕上限

            Assert.That(feed.ActiveToasts.Count, Is.EqualTo(4));
            Assert.That(feed.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void test_non_critical_beyond_cap_queues_and_releases()
        {
            var feed = new NotificationFeed();
            feed.Push(new Notification("a", 0, false));
            feed.Push(new Notification("b", 0, false));
            feed.Push(new Notification("c", 0, false));
            feed.Push(new Notification("d", 0, false)); // 超并发上限 → 入队

            Assert.That(feed.ActiveToasts.Count, Is.EqualTo(3));
            Assert.That(feed.PendingCount, Is.EqualTo(1));

            feed.Dismiss(feed.ActiveToasts[0]); // 缓和：消除一条
            Assert.That(feed.Release(), Is.True);
            Assert.That(feed.PendingCount, Is.EqualTo(0));
        }
    }
}
