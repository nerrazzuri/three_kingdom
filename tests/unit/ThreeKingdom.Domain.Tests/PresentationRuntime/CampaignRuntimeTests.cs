using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Tests.Persistence;
using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.PresentationRuntime
{
    /// <summary>
    /// 会话接缝 <see cref="CampaignRuntime"/>（epic-028 story-001 / TR-ux-005）。
    /// 验证：新局/推进最小生命周期经 CampaignSessionService（AC-1/2）；统一信封存读档 round-trip
    /// 保状态哈希 + 恢复后推进确定性（AC-3）；损坏信封不部分载入、当前会话不变（AC-3 edge）；
    /// 渲染纯函数恒等且不改会话态（AC-4/5）；原子写回失败保留上一份有效存档（ADR-0005 guardrail）。
    /// 治理 ADR：ADR-0009（primary）+ ADR-0002 + ADR-0004。介质用纯内存替身（test-standards：无文件 I/O）。
    /// </summary>
    [TestFixture]
    public class CampaignRuntimeTests
    {
        private InMemorySaveMedium _medium = null!;
        private CampaignRuntime _runtime = null!;

        [SetUp]
        public void SetUp()
        {
            // Arrange（公共）：内存介质 + 共享「汜水关太守」场景源（缺省注入，单一来源）。
            _medium = new InMemorySaveMedium();
            _runtime = new CampaignRuntime(_medium);
        }

        [Test]
        public void test_newgame_then_advance_progresses_one_segment()
        {
            // Act
            WorldStatusView start = _runtime.NewGame();
            WorldStatusView after = _runtime.Advance(1);

            // Assert：开局第 1 日黎明（0 基日 +1），推进一段到白昼，未跨日。
            Assert.That(start.TimeLabel, Is.EqualTo("第 1 日 · 黎明"));
            Assert.That(after.TimeLabel, Is.EqualTo("第 1 日 · 白昼"));
            Assert.That(after.CrossedDay, Is.False);
        }

        [Test]
        public void test_advance_without_newgame_autostarts_without_throwing()
        {
            // Act：未显式开局即推进（HUD 场景单独打开的路径）——自动开局，不抛裸异常。
            WorldStatusView view = _runtime.Advance(1);

            // Assert
            Assert.That(view.TimeLabel, Is.EqualTo("第 1 日 · 白昼"));
        }

        [Test]
        public void test_save_load_roundtrip_preserves_state_hash()
        {
            // Arrange：开局 + 治理命令 1 次 + 推进 2 段（QA 用例 AC-3 指定序列）。
            var service = new CampaignSessionService();
            _runtime.NewGame();
            CampaignCommandResult governed = service.RequisitionFood(_runtime.Session, 10);
            Assert.That(governed.Applied, Is.True, "前置：治理命令应成功。");
            _runtime.Advance(2);
            var hashBefore = _runtime.Session.ComputeHash();

            // Act
            bool saved = _runtime.Save();
            bool loaded = _runtime.Load(out string reason);

            // Assert：round-trip 后状态哈希逐位一致（TR-session-003 / AC-6）。
            Assert.That(saved, Is.True);
            Assert.That(loaded, Is.True, reason);
            Assert.That(_runtime.Session.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_restored_sessions_advance_deterministically()
        {
            // Arrange：存一份含治理+推进的会话，两个独立运行期从同一信封恢复。
            var service = new CampaignSessionService();
            _runtime.NewGame();
            service.RequisitionFood(_runtime.Session, 10);
            _runtime.Advance(1);
            Assert.That(_runtime.Save(), Is.True);

            var runtimeA = new CampaignRuntime(_medium);
            var runtimeB = new CampaignRuntime(_medium);
            Assert.That(runtimeA.Load(out _), Is.True);
            Assert.That(runtimeB.Load(out _), Is.True);

            // Act：恢复后各自推进同样时段。
            runtimeA.Advance(1);
            runtimeB.Advance(1);

            // Assert：同信封 + 同命令流 → 同状态哈希（ADR-0004 确定性延续）。
            Assert.That(runtimeA.Session.ComputeHash(), Is.EqualTo(runtimeB.Session.ComputeHash()));
        }

        [Test]
        public void test_load_corrupt_envelope_fails_without_touching_session()
        {
            // Arrange：当前会话已推进；槽内是损坏内容。
            _runtime.NewGame();
            _runtime.Advance(1);
            var hashBefore = _runtime.Session.ComputeHash();
            _medium.Write(CampaignRuntime.DefaultSlot, "这不是一份合法的会话存档");

            // Act
            bool loaded = _runtime.Load(out string reason);

            // Assert：失败返回原因；当前会话不变（不部分载入，TR-session-003）。
            Assert.That(loaded, Is.False);
            Assert.That(reason, Is.Not.Empty);
            Assert.That(_runtime.Session.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_status_rendering_is_pure_and_does_not_mutate_session()
        {
            // Arrange
            _runtime.NewGame();
            _runtime.Advance(1);
            var hashBefore = _runtime.Session.ComputeHash();

            // Act：同会话态连续两次取投影。
            WorldStatusView first = _runtime.Status();
            WorldStatusView second = _runtime.Status();

            // Assert：渲染恒等（AC-6/TR-ux-005）且不改会话态。
            Assert.That(second.TimeLabel, Is.EqualTo(first.TimeLabel));
            Assert.That(second.DayLabel, Is.EqualTo(first.DayLabel));
            Assert.That(second.CrossDayNotice, Is.EqualTo(first.CrossDayNotice));
            Assert.That(_runtime.Session.ComputeHash(), Is.EqualTo(hashBefore));
        }

        [Test]
        public void test_hassave_reflects_persisted_slot()
        {
            // Arrange
            _runtime.NewGame();

            // Act / Assert
            Assert.That(_runtime.HasSave(), Is.False);
            Assert.That(_runtime.Save(), Is.True);
            Assert.That(_runtime.HasSave(), Is.True);
        }

        [Test]
        public void test_save_failure_keeps_previous_valid_save()
        {
            // Arrange：先存一份有效存档，然后让临时槽写入失败（模拟磁盘写满）。
            _runtime.NewGame();
            Assert.That(_runtime.Save(), Is.True);
            string? firstEnvelope = _medium.Peek(CampaignRuntime.DefaultSlot);
            _runtime.Advance(1);
            _medium.FailWriteOn.Add(CampaignRuntime.DefaultSlot + ".tmp");

            // Act
            bool saved = _runtime.Save();

            // Assert：失败返回 false；正式槽保留上一份有效存档（ADR-0005 guardrail）。
            Assert.That(saved, Is.False);
            Assert.That(_medium.Peek(CampaignRuntime.DefaultSlot), Is.EqualTo(firstEnvelope));
        }
    }
}
