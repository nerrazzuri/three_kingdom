using System.IO;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Console;

namespace ThreeKingdom.Domain.Tests.Console
{
    /// <summary>
    /// M15 交互控制台 harness（PlayableCampaign / CampaignTextView / CampaignDriver）。
    /// 验证：默认场景全循环启用且可开局；渲染纯函数确定性；脚本回放确定性；全循环路径可达；
    /// 存读档 round-trip 保哈希；失败战果可继续；未知命令不抛。
    /// 治理 ADR：ADR-0002（只经用例）+ ADR-0004（确定性）+ ADR-0009（配置驱动开局）。
    /// </summary>
    [TestFixture]
    public class CampaignConsoleTests
    {
        private string _save = null!;

        [SetUp]
        public void SetUp() => _save = Path.Combine(Path.GetTempPath(), "tk-console-test-" + System.Guid.NewGuid().ToString("N") + ".save");

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_save)) File.Delete(_save);
        }

        // ---- 默认场景：全循环启用且可开局 ----

        [Test]
        public void test_playable_campaign_starts_with_all_loops_enabled()
        {
            PlayableCampaign scenario = PlayableCampaign.Default();
            CampaignStartResult result = new CampaignSessionService().StartCampaign(scenario.StartConfig);

            Assert.That(result.Started, Is.True, "默认场景应可开局");
            CampaignSession s = result.Session!;
            Assert.That(s.HasCityGovernance, Is.True, "城市治理循环");
            Assert.That(s.HasIntel, Is.True, "情报循环");
            Assert.That(s.HasPreparation, Is.True, "战役准备循环");
            Assert.That(s.HasHistory, Is.True, "历史世界循环");
        }

        // ---- 渲染：纯函数确定性 + 含核心字段 ----

        [Test]
        public void test_status_render_is_deterministic()
        {
            var d = new CampaignDriver(_save);
            string a = CampaignTextView.Status(d.Session);
            string b = CampaignTextView.Status(d.Session);

            Assert.That(a, Is.EqualTo(b), "同会话态渲染恒等（纯函数）");
            Assert.That(a, Does.Contain("生涯").And.Contain("城池").And.Contain("情报"));
        }

        // ---- 脚本回放：确定性（同输入序列 → 同状态哈希）----

        [Test]
        public void test_scripted_play_is_deterministic()
        {
            string[] script = { "5", "6", "7", "8", "m", "m", "m", "9", "t", "o", "c", "h", "1" };
            var d1 = new CampaignDriver(_save + ".1");
            var d2 = new CampaignDriver(_save + ".2");
            foreach (string k in script) { d1.Step(k); d2.Step(k); }

            Assert.That(d1.Session.ComputeHash(), Is.EqualTo(d2.Session.ComputeHash()),
                "同开局 + 同输入序列 → 同会话哈希（ADR-0004 确定性）");
        }

        // ---- 全循环路径可达 ----

        [Test]
        public void test_full_loop_script_reaches_all_systems()
        {
            var d = new CampaignDriver(_save);
            foreach (string k in new[] { "5", "7", "8", "m", "m", "m", "9", "t", "o", "c", "1", "h" })
                d.Step(k);

            Assert.That(d.Session.HasBattle, Is.True, "已开战");
            Assert.That(d.Session.HasOutcome, Is.True, "已结算战果");
            Assert.That(d.Session.Career.Career.Merit, Is.GreaterThan(0), "已累积战功");
            Assert.That(d.Session.World.TriggeredEvents.Count, Is.GreaterThan(0), "历史事件已触发");
        }

        // ---- 存读档 round-trip 保哈希（跨实例）----

        [Test]
        public void test_save_load_round_trip_preserves_hash()
        {
            var saver = new CampaignDriver(_save);
            foreach (string k in new[] { "5", "7", "8" }) saver.Step(k);
            var before = saver.Session.ComputeHash();
            saver.Step("s");

            var loader = new CampaignDriver(_save);
            string feedback = loader.Step("l");

            Assert.That(feedback, Does.StartWith("✓"), "读档应成功");
            Assert.That(loader.Session.ComputeHash(), Is.EqualTo(before), "读档恢复 → 哈希一致（无丢失）");
        }

        // ---- 失败战果可继续（control-manifest 强制设计锁）----

        [Test]
        public void test_defeat_outcome_is_continuable()
        {
            var d = new CampaignDriver(_save);
            string feedback = d.Step("p");   // 结算战果·失城

            Assert.That(d.Session.HasOutcome, Is.True);
            Assert.That(d.Session.LastContinuationOptions.Count, Is.GreaterThan(0), "失利亦有续局选项（失败不删档）");
            Assert.That(feedback, Does.Contain("可继续"));
        }

        // ---- 未知命令：友好反馈不抛 ----

        [Test]
        public void test_unknown_command_returns_friendly_feedback_without_throwing()
        {
            var d = new CampaignDriver(_save);
            string feedback = d.Step("zzz");

            Assert.That(feedback, Does.Contain("未知命令"));
            Assert.That(d.Quit, Is.False);
        }

        // ---- 退出 ----

        [Test]
        public void test_quit_sets_quit_flag()
        {
            var d = new CampaignDriver(_save);
            d.Step("q");
            Assert.That(d.Quit, Is.True);
        }
    }
}
