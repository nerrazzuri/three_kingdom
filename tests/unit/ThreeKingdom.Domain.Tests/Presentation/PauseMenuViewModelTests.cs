using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Outcome;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Screens;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-004：暂停菜单状态机 + 草稿处置 + 失败延续（可测逻辑，BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0005。覆盖 AC-4 保存失败错误态、AC-5 草稿 P9 门控、AC-7 失败延续「继续」。
    /// </summary>
    [TestFixture]
    public class PauseMenuViewModelTests
    {
        private static readonly ISaveSerializer Serializer = new CanonicalSaveSerializer();
        private const string Slot = "campaign";

        private static SaveSnapshot Snapshot()
            => new SaveSnapshot(new SaveVersion(1, 0), new ThreeKingdom.Domain.Configuration.ConfigFingerprint(1UL),
                new RngStreamState[0],
                new Dictionary<string, long> { ["troops"] = 100 },
                new Dictionary<string, long>());

        // ---- AC-4: 保存失败错误态 ----

        [Test]
        public void test_save_failure_maps_to_error_state_with_reason()
        {
            var medium = new InMemorySaveMedium();
            medium.FailWriteOn.Add(Slot + ".tmp"); // 模拟磁盘写满
            var repo = new SaveRepository(medium, Serializer);
            var result = repo.Save(Slot, Snapshot());
            Assert.That(result.Succeeded, Is.False);

            var vm = PauseMenuViewModel.Open(false).BeginSave().OnSaveResult(result);

            Assert.That(vm.State, Is.EqualTo(PauseMenuState.SaveError));
            Assert.That(vm.ErrorCode, Is.EqualTo(SaveErrorCode.TempWriteFailed));
            Assert.That(vm.ErrorReason, Is.Not.Empty);
        }

        [Test]
        public void test_save_success_returns_to_resting()
        {
            var repo = new SaveRepository(new InMemorySaveMedium(), Serializer);
            var result = repo.Save(Slot, Snapshot());

            var vm = PauseMenuViewModel.Open(false).BeginSave().OnSaveResult(result);
            Assert.That(vm.State, Is.EqualTo(PauseMenuState.Default));
        }

        // ---- AC-5: 草稿 P9 门控 ----

        [Test]
        public void test_exit_with_draft_requires_p9_disposition()
        {
            var vm = PauseMenuViewModel.Open(hasDraft: true).RequestExitOrLoad();
            Assert.That(vm.RequiresDraftDisposition, Is.True, "有草稿 → 先经 P9 处置。");
            Assert.That(vm.MayProceed, Is.False, "未处置不放行。");
        }

        [Test]
        public void test_exit_without_draft_proceeds()
        {
            var vm = PauseMenuViewModel.Open(hasDraft: false).RequestExitOrLoad();
            Assert.That(vm.RequiresDraftDisposition, Is.False);
            Assert.That(vm.MayProceed, Is.True);
        }

        [Test]
        public void test_dispose_draft_keep_cancels_proceed()
        {
            var vm = PauseMenuViewModel.Open(hasDraft: true).RequestExitOrLoad().DisposeDraft(keepDraft: true);
            Assert.That(vm.MayProceed, Is.False, "保留草稿 → 取消退出。");
            Assert.That(vm.HasDraft, Is.True);
        }

        // ---- AC-7: 失败延续「继续」契约 ----

        [Test]
        public void test_defeat_continuation_prompt_is_playable_with_options()
        {
            var faction = new FactionId("faction-liu");
            var world = OutcomeWorld.Empty
                .WithReputation(faction, 100)
                .WithCity(new CityEconomyState(new CityId("city-x"), 1000, 0, 50, 50, 60, 100));
            var config = new OutcomeConsequenceConfig(20, 8, 35, 15, 20, 25, 600);
            var continuation = new FailureContinuationService()
                .Resolve(world, OutcomeBranch.CityLost, new OutcomeContext(faction, new CityId("city-x")), config);

            var view = ContinuationPromptView.From(continuation);

            Assert.That(view.IsPlayable, Is.True, "败局仍可继续。");
            Assert.That(view.Options.Count, Is.GreaterThanOrEqualTo(1), "至少一条合法可继续命令。");
        }
    }
}
