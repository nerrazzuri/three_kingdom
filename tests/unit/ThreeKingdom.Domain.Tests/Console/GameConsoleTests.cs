using NUnit.Framework;
using ThreeKingdom.Console;

namespace ThreeKingdom.Domain.Tests.Console
{
    /// <summary>
    /// 完整游戏文本控制台（GameConsole）端到端可玩性：整个游戏在无 UI 下能从头玩到尾——
    /// 选城开局 → 推进(纪元/天下大势) → 君主任务 → 出征全流程 → 地图/武将录 → 施计/自立。确定性、不崩。
    /// 这是"整个游戏可玩,只是没 UI"的活证。
    /// </summary>
    [TestFixture]
    public class GameConsoleTests
    {
        [Test]
        public void test_full_playthrough_runs_end_to_end()
        {
            var g = new GameConsole();

            // 选城开局：陈留太守，掌该城 6 部将。
            Assert.That(g.Dispatch("gov city-chenliu"), Does.Contain("陈留"));
            Assert.That(g.Runtime.DeputyRoster.Count, Is.EqualTo(6), "该年该城武将归你。");

            // 领受君命（开局年即派发，期限=当年+3；真实游玩每回合都显状态，故此处先看一眼锁定于当前年）。
            Assert.That(g.Dispatch("mission"), Does.Contain("君命"));

            // 按年推进——天下大势按公元年铺开、君主任务(守土)可完成。
            g.Dispatch("year"); g.Dispatch("year"); g.Dispatch("year");
            Assert.That(g.Runtime.CurrentYear, Is.GreaterThanOrEqualTo(193));
            Assert.That(g.Dispatch("checkmission"), Does.Contain("Completed"), "守土至期限 → 君命完成。");

            // 出征全流程：授权 → 目标 → 组装 → 发起 → AI代打 → 结算。
            Assert.That(g.Dispatch("authorize"), Does.Contain("授权"));
            Assert.That(g.Dispatch("targets"), Does.Contain("虎牢关"));
            g.Dispatch("offensive"); g.Dispatch("launch"); g.Dispatch("auto");
            Assert.That(g.Dispatch("conclude"), Is.Not.Empty);

            // 视图：战略地图 + 武将录（203 员）。
            Assert.That(g.Dispatch("map"), Does.Contain("曹操"), "地图列势力领城。");
            Assert.That(g.Dispatch("roster 0"), Does.Contain("武将录"));

            // 施计 + 自立判定 皆可达、不崩。
            Assert.That(g.Dispatch("subvert city-hulao 1"), Is.Not.Empty);
            Assert.That(g.Dispatch("rebel"), Is.Not.Empty);

            Assert.That(g.StatusText(), Does.Contain("公元"), "状态渲染正常。");
        }

        [Test]
        public void test_life_flows_to_succession_over_decades()
        {
            var g = new GameConsole();
            // 跳时数十年 → 寿终 → 传承子嗣续局（一生闭环）。
            for (int i = 0; i < 70; i++) g.Dispatch("year");
            Assert.That(g.Runtime.IsLifeOver, Is.True, "数十年后寿终。");
            Assert.That(g.Dispatch("heir"), Does.Contain("承业"), "子嗣续局。");
            Assert.That(g.Runtime.Generation, Is.EqualTo(1));
            Assert.That(g.Runtime.IsLifeOver, Is.False, "新一世方始。");
        }

        [Test]
        public void test_menu_and_unknown_command()
        {
            var g = new GameConsole();
            Assert.That(GameConsole.Menu(), Does.Contain("出征"));
            Assert.That(g.Dispatch("nonsense"), Does.Contain("未知命令"));
        }
    }
}
