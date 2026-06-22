using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-002：主菜单状态机 + 读档错误态（可测逻辑，BLOCKING）。
    /// 治理 ADR：ADR-0002 + ADR-0005。覆盖 AC-1 状态派生（有/无存档/读档中/失败/退出确认）、
    /// AC-4 读档错误态映射（不兼容/损坏 → 可行动原因，零部分加载）。
    /// </summary>
    [TestFixture]
    public class MainMenuViewModelTests
    {
        private static readonly ISaveSerializer Serializer = new CanonicalSaveSerializer();
        private static readonly SaveVersion Current = new SaveVersion(1, 0);
        private static readonly ConfigFingerprint Fp = new ConfigFingerprint(0xABCUL);
        private const string Slot = "campaign";

        private static SaveSnapshot Snapshot(SaveVersion version)
            => new SaveSnapshot(version, Fp,
                new[] { new RngStreamState("battle", 1UL, 2UL) },
                new Dictionary<string, long> { ["troops"] = 3000 },
                new Dictionary<string, long> { ["enemy.estimate"] = 1000 });

        private static LoadResult Load(SaveVersion saveVersion, SaveVersion current)
        {
            var medium = new InMemorySaveMedium();
            medium.Write(Slot, Serializer.Serialize(Snapshot(saveVersion)));
            var service = new SaveLoadService(medium, Serializer, new SaveMigrator(new ISaveMigration[0]));
            return service.Load(Slot, current, Fp);
        }

        // ---- AC-1: 状态派生 ----

        [Test]
        public void test_no_save_yields_no_save_state()
        {
            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Empty(Slot));
            Assert.That(vm.State, Is.EqualTo(MainMenuState.NoSave));
            Assert.That(vm.ContinueAvailable, Is.False);
        }

        [Test]
        public void test_existing_save_yields_has_save_state_with_continue()
        {
            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Present(Slot, Snapshot(Current)));
            Assert.That(vm.State, Is.EqualTo(MainMenuState.HasSave));
            Assert.That(vm.ContinueAvailable, Is.True);
            Assert.That(vm.SlotVersionLabel, Is.EqualTo("1.0"));
        }

        [Test]
        public void test_quit_confirm_and_cancel_round_trip()
        {
            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Present(Slot, Snapshot(Current)));
            var confirming = vm.RequestQuit();
            Assert.That(confirming.State, Is.EqualTo(MainMenuState.QuitConfirm));
            Assert.That(confirming.CancelQuit().State, Is.EqualTo(MainMenuState.HasSave), "取消退出回到静止态。");
        }

        // ---- AC-4: 读档错误态映射 ----

        [Test]
        public void test_incompatible_save_maps_to_load_error_without_entering_game()
        {
            var result = Load(new SaveVersion(2, 0), Current); // 存档高于当前 → 不兼容
            Assert.That(result.Succeeded, Is.False);

            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Present(Slot, Snapshot(Current)))
                .BeginLoad()
                .OnLoadResult(result);

            Assert.That(vm.State, Is.EqualTo(MainMenuState.LoadError));
            Assert.That(vm.ErrorCode, Is.EqualTo(LoadErrorCode.IncompatibleNewer));
            Assert.That(vm.ErrorReason, Is.Not.Empty, "显示可行动原因。");
            Assert.That(vm.LoadSucceeded, Is.False, "零部分加载——不进入游戏。");
        }

        [Test]
        public void test_load_error_can_be_dismissed_back_to_resting()
        {
            var result = Load(new SaveVersion(2, 0), Current);
            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Present(Slot, Snapshot(Current)))
                .OnLoadResult(result)
                .DismissError();
            Assert.That(vm.State, Is.EqualTo(MainMenuState.HasSave));
        }

        [Test]
        public void test_successful_load_flags_ready_to_enter()
        {
            var result = Load(Current, Current); // 兼容 → 成功
            Assert.That(result.Succeeded, Is.True);

            var vm = MainMenuViewModel.FromSlot(SaveSlotView.Present(Slot, Snapshot(Current)))
                .BeginLoad()
                .OnLoadResult(result);

            Assert.That(vm.LoadSucceeded, Is.True);
            Assert.That(vm.State, Is.EqualTo(MainMenuState.HasSave));
        }
    }
}
