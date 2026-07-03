using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-028 story-005：新手引导 ViewModel/配置（TR-ux-002 / §3/§5/§7 Q2 / ADR-0003）。
    /// 覆盖 AC-1 军议自动显示（前 N 回合，配置驱动）、AC-3 果·长线首次提示（已见不复现）、
    /// AC-6 可关闭且引导不进权威态、配置校验。纯 C# 确定性。
    /// </summary>
    [TestFixture]
    public class OnboardingViewModelTests
    {
        // ---- AC-1：前 N 回合军议自动展开；之后需按键调出；N=0 关闭 ----

        [Test]
        public void test_council_auto_expands_within_first_n_rounds()
        {
            var config = new OnboardingConfig(3);   // N=3
            Assert.That(OnboardingHints.ShouldAutoExpandCouncil(1, config), Is.True, "回合 1 自动展开。");
            Assert.That(OnboardingHints.ShouldAutoExpandCouncil(3, config), Is.True, "回合 3（=N）自动展开。");
            Assert.That(OnboardingHints.ShouldAutoExpandCouncil(4, config), Is.False, "回合 4 需按键调出。");
        }

        [Test]
        public void test_council_auto_expand_disabled_when_n_zero_or_guidance_off()
        {
            Assert.That(OnboardingHints.ShouldAutoExpandCouncil(1, new OnboardingConfig(0)), Is.False,
                "N=0 → 任何回合不自动展开。");
            Assert.That(OnboardingHints.ShouldAutoExpandCouncil(1, OnboardingConfig.Default, disabled: true), Is.False,
                "关闭引导 → 不自动展开。");
        }

        // ---- AC-3：果·长线首次提示；标记已见后不再出现 ----

        [Test]
        public void test_outcome_career_cue_shown_first_time_then_suppressed_after_seen()
        {
            var candidates = new[] { OnboardingCue.OutcomeToCareer };
            var seen = new HashSet<OnboardingCue>();

            IReadOnlyList<string> first = OnboardingHints.CuesFor(candidates, seen, disabled: false);
            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(first[0], Does.Contain("记功"));
            Assert.That(first[0], Does.Contain("晋升"));

            seen.Add(OnboardingCue.OutcomeToCareer);   // 显示后登记已见
            IReadOnlyList<string> again = OnboardingHints.CuesFor(candidates, seen, disabled: false);
            Assert.That(again, Is.Empty, "已见提示不重复。");
        }

        [Test]
        public void test_progressive_cues_follow_observe_plan_prepare_fight_order()
        {
            var candidates = new[]
            {
                OnboardingCue.Observe, OnboardingCue.Plan, OnboardingCue.Prepare, OnboardingCue.Fight,
            };
            IReadOnlyList<string> cues = OnboardingHints.CuesFor(candidates, new HashSet<OnboardingCue>(), disabled: false);
            Assert.That(cues.Count, Is.EqualTo(4), "察→谋→备→战四条按序给出。");
            Assert.That(cues[0], Does.Contain("察"));
            Assert.That(cues[3], Does.Contain("开战"));
        }

        // ---- AC-6：关闭引导 → 无输出 ----

        [Test]
        public void test_all_cues_suppressed_when_guidance_disabled()
        {
            var all = (OnboardingCue[])Enum.GetValues(typeof(OnboardingCue));
            IReadOnlyList<string> cues = OnboardingHints.CuesFor(all, new HashSet<OnboardingCue>(), disabled: true);
            Assert.That(cues, Is.Empty, "关闭引导后一律不打扰。");
        }

        // ---- AC-6：引导不进 Domain——渲染引导不改会话权威态哈希 ----

        [Test]
        public void test_guidance_does_not_affect_authoritative_session_state()
        {
            var service = new CampaignSessionService();
            CampaignSession session = service.StartCampaign(PlayableCampaign.Default().StartConfig).Session!;
            string before = service.CaptureSnapshot(session);

            // 无论关闭与否、无论渲染何种引导，都不触会话权威态。
            OnboardingHints.ShouldAutoExpandCouncil(session.CurrentTime.Day + 1, OnboardingConfig.Default);
            OnboardingHints.CuesFor((OnboardingCue[])Enum.GetValues(typeof(OnboardingCue)),
                new HashSet<OnboardingCue>(), disabled: false);
            OnboardingHints.CuesFor(new[] { OnboardingCue.OutcomeToCareer },
                new HashSet<OnboardingCue>(), disabled: true);

            string after = service.CaptureSnapshot(session);
            Assert.That(after, Is.EqualTo(before), "引导为表现层状态，不改权威会话存档信封。");
        }

        // ---- 配置校验（ADR-0003）：非法 N 被拒 ----

        [Test]
        public void test_negative_auto_council_rounds_rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OnboardingConfig(-1));
        }
    }
}
