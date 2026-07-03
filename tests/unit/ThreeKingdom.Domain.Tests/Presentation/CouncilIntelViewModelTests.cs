using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-028 story-003：军议/敌情屏 ViewModel（TR-ux-002/003 / GDD_007/008 / §7 Q1 裁决）。
    /// 覆盖 AC-1 无胜率、AC-2 定性置信档 + 边界、AC-3/4 时效「N 段前」+ 过时（快照绑定不静默重算）、
    /// AC-5 类型层反全知、AC-6 渲染恒等。纯 C# 展示模型，无引擎依赖、确定性（无随机/时间依赖）。
    /// </summary>
    [TestFixture]
    public class CouncilIntelViewModelTests
    {
        private static readonly AdvisorId Advisor = new AdvisorId("advisor-zhuge");
        private static readonly IntelSubjectId Subject = new IntelSubjectId("subject-enemy-army");
        private static readonly FactionId Player = new FactionId("faction-player");

        private static AdviceStatement Advice(string candidate, FixedPoint confidence)
            => new AdviceStatement(
                Advisor, candidate, "敌前锋深入，补给线拉长。", "若敌补给可被持续袭扰，其战力随时日衰减。",
                new[] { "需查明敌补给路线" }, new[] { "袭扰队可能被反伏" },
                Array.Empty<IntelSubjectId>(), confidence);

        private static IntelProjection ProjectionAt(int reportedStrength, WorldTime observedAt)
        {
            var intel = new FactionIntel(Player);
            intel.ApplyReport(new IntelReport(Subject, Player, reportedStrength, IntelSource.Scouting, observedAt));
            return intel.Project();
        }

        // ---- AC-2：小数置信 → 定性档（低/中/高），无小数/百分号 ----

        [Test]
        public void test_council_view_maps_confidence_to_qualitative_band()
        {
            var set = new CouncilAdviceSet(new KnowledgeSnapshotId("snap-1"), new[]
            {
                Advice("低置信路线", FixedPoint.FromFraction(1, 5)),    // 0.20 → 低
                Advice("中置信路线", FixedPoint.FromFraction(11, 20)),  // 0.55 → 中
                Advice("高置信路线", FixedPoint.FromFraction(9, 10)),   // 0.90 → 高
            });

            var view = CampaignCouncilView.FromSet(set, new KnowledgeSnapshotId("snap-1"), CouncilIntelTuning.Default);

            Assert.That(view.Advice[0].ConfidenceLabel, Is.EqualTo("低"));
            Assert.That(view.Advice[1].ConfidenceLabel, Is.EqualTo("中"));
            Assert.That(view.Advice[2].ConfidenceLabel, Is.EqualTo("高"));
            foreach (var a in view.Advice)
            {
                Assert.That(a.ConfidenceLabel, Does.Not.Contain("0."), "档位为定性标签，不含小数。");
                Assert.That(a.ConfidenceLabel, Does.Not.Contain("%"), "档位不含百分号。");
            }
        }

        // ---- AC-2 边界：== 低界 → 中；== 高界 → 高（归属确定且锁定） ----

        [Test]
        public void test_council_view_confidence_band_boundaries_are_locked()
        {
            var tuning = CouncilIntelTuning.Default;   // 0.4 / 0.7
            var set = new CouncilAdviceSet(new KnowledgeSnapshotId("snap-1"), new[]
            {
                Advice("恰在低界", tuning.LowCeiling),    // == 0.4 → 中（非严格小于低界）
                Advice("恰在高界", tuning.HighCeiling),   // == 0.7 → 高（非严格小于高界）
            });

            var view = CampaignCouncilView.FromSet(set, new KnowledgeSnapshotId("snap-1"), tuning);

            Assert.That(view.Advice[0].ConfidenceLabel, Is.EqualTo("中"), "恰在低/中分界归中。");
            Assert.That(view.Advice[1].ConfidenceLabel, Is.EqualTo("高"), "恰在中/高分界归高。");
        }

        // ---- AC-1：全文无成功率/胜率/百分号/唯一推荐/最优 ----

        [Test]
        public void test_council_view_contains_no_success_rate_or_recommendation()
        {
            var set = new CouncilAdviceSet(new KnowledgeSnapshotId("snap-1"), new[]
            {
                Advice("断粮疲敌", FixedPoint.FromFraction(1, 2)),
                Advice("守城待变", FixedPoint.FromFraction(4, 5)),
            });
            var view = CampaignCouncilView.FromSet(set, new KnowledgeSnapshotId("snap-1"), CouncilIntelTuning.Default);

            string all = string.Join("\n", RenderedCouncilText(view));
            foreach (var banned in new[] { "成功率", "胜率", "%", "推荐方案", "推荐", "最优" })
                Assert.That(all.Contains(banned), Is.False, $"军议全文不得含「{banned}」。");
        }

        // ---- AC-3：敌情时效「N 段前」；超 ttl 高亮过时，未超不过时 ----

        [Test]
        public void test_enemy_intel_shows_age_label_and_staleness_against_ttl()
        {
            const int ttl = 8;
            var observedAt = new WorldTime(0, DaySegment.Dawn);
            var now = observedAt.Advance(3);    // 3 段前 ≤ ttl → 未过时
            var later = observedAt.Advance(10); // 10 段前 > ttl → 过时

            var fresh = CampaignEnemyIntelPanelView.FromProjection(ProjectionAt(1000, observedAt), now, ttl);
            var stale = CampaignEnemyIntelPanelView.FromProjection(ProjectionAt(1000, observedAt), later, ttl);

            Assert.That(fresh.Entries[0].AgeSegments, Is.EqualTo(3));
            Assert.That(fresh.Entries[0].ObservedAgoLabel, Is.EqualTo("3 段前"));
            Assert.That(fresh.Entries[0].IsStale, Is.False, "3 段 ≤ ttl(8) → 未过时。");
            Assert.That(fresh.Entries[0].EstimatedStrength, Is.EqualTo(1000), "呈现报告估计值（非真值）。");

            Assert.That(stale.Entries[0].ObservedAgoLabel, Is.EqualTo("10 段前"));
            Assert.That(stale.Entries[0].IsStale, Is.True, "10 段 > ttl(8) → 过时。");
        }

        // ---- AC-4：军议绑定召开时快照——知识变化后旧建议标过时，内容不静默变化 ----

        [Test]
        public void test_council_marked_stale_after_snapshot_change_without_silent_update()
        {
            var atConvene = new KnowledgeSnapshotId("snap-at-convene");
            var set = new CouncilAdviceSet(atConvene, new[] { Advice("断粮疲敌", FixedPoint.FromFraction(1, 2)) });

            var current = CampaignCouncilView.FromSet(set, atConvene, CouncilIntelTuning.Default);
            var afterScout = CampaignCouncilView.FromSet(set, new KnowledgeSnapshotId("snap-after-scout"), CouncilIntelTuning.Default);

            Assert.That(current.IsStale, Is.False);
            Assert.That(current.StaleNotice, Is.Empty);
            Assert.That(afterScout.IsStale, Is.True, "知识快照变化后标过时。");
            Assert.That(afterScout.StaleNotice, Is.Not.Empty, "过时给出重开提示（不禁用旧建议）。");

            // 内容与召开时一致（未静默重算）。
            Assert.That(afterScout.Advice.Count, Is.EqualTo(current.Advice.Count));
            Assert.That(afterScout.Advice[0].CandidateLabel, Is.EqualTo(current.Advice[0].CandidateLabel));
            Assert.That(afterScout.Advice[0].Observation, Is.EqualTo(current.Advice[0].Observation));
            Assert.That(afterScout.Advice[0].ConfidenceLabel, Is.EqualTo(current.Advice[0].ConfidenceLabel));
        }

        // ---- AC-6：同投影两次渲染逐字段相等（确定性） ----

        [Test]
        public void test_view_models_are_deterministic_for_same_inputs()
        {
            var set = new CouncilAdviceSet(new KnowledgeSnapshotId("snap-1"),
                new[] { Advice("断粮疲敌", FixedPoint.FromFraction(1, 2)) });
            var snap = new KnowledgeSnapshotId("snap-1");

            var c1 = CampaignCouncilView.FromSet(set, snap, CouncilIntelTuning.Default);
            var c2 = CampaignCouncilView.FromSet(set, snap, CouncilIntelTuning.Default);
            Assert.That(c2.IsStale, Is.EqualTo(c1.IsStale));
            Assert.That(c2.Advice[0].ConfidenceLabel, Is.EqualTo(c1.Advice[0].ConfidenceLabel));
            Assert.That(c2.Advice[0].Observation, Is.EqualTo(c1.Advice[0].Observation));

            var observedAt = new WorldTime(1, DaySegment.Day);
            var now = observedAt.Advance(2);
            var e1 = CampaignEnemyIntelPanelView.FromProjection(ProjectionAt(900, observedAt), now, 8);
            var e2 = CampaignEnemyIntelPanelView.FromProjection(ProjectionAt(900, observedAt), now, 8);
            Assert.That(e2.Entries[0].ObservedAgoLabel, Is.EqualTo(e1.Entries[0].ObservedAgoLabel));
            Assert.That(e2.Entries[0].IsStale, Is.EqualTo(e1.Entries[0].IsStale));
            Assert.That(e2.Entries[0].EstimatedStrength, Is.EqualTo(e1.Entries[0].EstimatedStrength));
        }

        // ---- AC-5：类型层反全知 + 无最优/成功率字段（结构性，反射固化） ----

        [Test]
        public void test_enemy_view_has_no_truth_fields()
        {
            AssertNoPropContains(typeof(CampaignEnemyIntelView), "truth", "actual", "real");
            Assert.That(PropNames(typeof(CampaignEnemyIntelView)), Does.Contain("estimatedstrength"),
                "正向：只暴露报告估计值。");
        }

        [Test]
        public void test_council_view_has_no_ranking_or_success_rate_fields()
        {
            AssertNoPropContains(typeof(CampaignAdviceView),
                "best", "optimal", "recommended", "rank", "ranking", "score", "success", "probability", "percent", "winrate");
            AssertNoPropContains(typeof(CampaignCouncilView), "best", "optimal", "recommended", "rank", "score");
            Assert.That(PropNames(typeof(CampaignAdviceView)), Does.Contain("confidencelabel"),
                "正向：置信为定性标签字段。");
        }

        // ---- 帮助 ----

        private static IEnumerable<string> RenderedCouncilText(CampaignCouncilView view)
        {
            yield return view.StaleNotice;
            foreach (var a in view.Advice)
            {
                yield return a.CandidateLabel;
                yield return a.Observation;
                yield return a.Assumption;
                yield return a.ConfidenceLabel;
                foreach (var c in a.RequiredConditions) yield return c;
                foreach (var r in a.Risks) yield return r;
                foreach (var m in a.MissingIntel) yield return m;
            }
        }

        private static string[] PropNames(Type t) =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name.ToLowerInvariant()).ToArray();

        private static void AssertNoPropContains(Type t, params string[] forbiddenSubstrings)
        {
            foreach (var name in PropNames(t))
                foreach (var bad in forbiddenSubstrings)
                    Assert.That(name.Contains(bad), Is.False,
                        $"{t.Name} 不得含属性「{name}」（设计锁禁止 '{bad}'）。");
        }
    }
}
