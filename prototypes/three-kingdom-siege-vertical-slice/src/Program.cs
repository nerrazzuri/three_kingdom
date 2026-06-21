// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: headless 控制台驱动真实 Domain，验证「赢因创造条件而非点技能」
// Date: 2026-06-21

using System;
using TkSlice.Application;
using TkSlice.Domain.Battle;
using TkSlice.Domain.Diplomacy;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Siege;
using TkSlice.Harness;

namespace TkSlice
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "demo";

            if (mode == "play")
            {
                new InteractiveSession().Run();
                return 0;
            }

            Console.WriteLine("=== 三国演义：兵法沙盒 — Vertical Slice（汜水小城守御）===");
            Console.WriteLine("（脚本对照演示三条兵法条件链；交互模式：dotnet run play）\n");
            RunBaseline();
            Console.WriteLine();
            RunCutSupplyChain();
            Console.WriteLine();
            RunFeignedRetreatChain();
            Console.WriteLine();
            RunHoldAndAwaitChain();
            return 0;
        }

        // 对照组：不创造条件，正面硬守 → 2 倍兵力下城破
        static void RunBaseline()
        {
            Console.WriteLine("── 对照组 A：正面硬守（不创造任何条件）──");
            var svc = new SiegeService(SiegeScenario.CreateXishuiSiege());
            PrintOpening(svc.State);
            var result = svc.ResolveAssault(new ResolveAssaultCommand());
            PrintBattle(result);
            Console.WriteLine($"  结论：{SiegeState.Outcome(result.Outcome)} — 正面对抗 2 倍敌军，败局已定。");
        }

        // 链 1：断粮疲敌（双边博弈 + 情报雾）
        static void RunCutSupplyChain()
        {
            Console.WriteLine("── 链 1：断粮疲敌（双边博弈 + 情报雾）──");

            // A) 投入不足：敌护粮 + 补给车队反复回补，断粮失败（徒劳）
            var weak = new SiegeService(SiegeScenario.CreateXishuiSiege());
            weak.CommitRaid(new CommitRaidCommand(1));
            weak.Advance(new AdvanceSegmentCommand(8));
            weak.State.ScoutEnemy();
            Console.WriteLine($"  [投 1 支·8 段后侦察] {weak.State.Intel.Describe(weak.State.Clock.TotalSegments)}");
            Console.WriteLine($"      （真值补给 {weak.State.Attacker.SupplyState}）→ 袭扰太弱未压过护粮，敌补给车队反复回补，断粮徒劳。");

            // B) 充分投入：袭扰压过护卫，边打边侦察确认，赶在敌援军（第14段）前拖垮
            Console.WriteLine("  [投 3 支·边打边侦察]");
            var svc = new SiegeService(SiegeScenario.CreateXishuiSiege());
            PrintOpening(svc.State);
            Expect(svc.CommitRaid(new CommitRaidCommand(3)), "投 3 支袭扰队");
            for (int i = 0; i < 3; i++)
            {
                Expect(svc.Advance(new AdvanceSegmentCommand(3)), "推进");
                Expect(svc.Scout(new ScoutCommand()), "侦察");   // 侦察确认是否真断粮
            }
            var result = svc.ResolveAssault(new ResolveAssaultCommand());
            PrintLog(svc.State);
            PrintBattle(result);
            Console.WriteLine($"  结论：{SiegeState.Outcome(result.Outcome)} — 投入压过敌护粮、侦察确认真断粮、赶在敌援军前拖垮，才取胜。");
        }

        // 链 2：假退伏击
        static void RunFeignedRetreatChain()
        {
            Console.WriteLine("── 链 2：假退伏击 ──");
            var svc = new SiegeService(SiegeScenario.CreateXishuiSiege());
            PrintOpening(svc.State);
            Console.WriteLine($"  敌将鲁莽度 {svc.State.EnemyCommander.Recklessness}（易中诱敌之计）");
            var r = svc.FeignedRetreat(new FeignedRetreatCommand(DecoyTroops: 150, AmbushTroops: 350), out var chk);
            if (!chk.Ok) { Console.WriteLine($"  [命令失败] {chk.Code}"); return; }
            PrintLog(svc.State);
            if (r.Ambush != null) PrintBattle(r.Ambush);
            Console.WriteLine($"  结论：敌将贪功冒进、伏兵未暴露——是我用军纪与判断设的局，非技能。");
            Console.WriteLine($"        敌先锋经此重创（余 {svc.State.Attacker.Troops}，士气 {svc.State.Attacker.UnitMorale}）。");
        }

        // 链 3：守城待变（外交受控入口 GDD_012 §8）
        static void RunHoldAndAwaitChain()
        {
            Console.WriteLine("── 链 3：守城待变（外交受控入口）──");
            var svc = new SiegeService(SiegeScenario.CreateXishuiSiege());
            PrintOpening(svc.State);
            var (chk, pledge) = svc.RequestDiplomacy(new RequestDiplomacyCommand(PledgeType.Relief, PledgeCostPct: 60));
            if (!chk.Ok) { Console.WriteLine($"  [命令失败] {chk.Code}"); return; }
            // 边守边等：援军延迟交付（绝非即到）
            Expect(svc.Advance(new AdvanceSegmentCommand(6)), "守城待援 6 段");
            var result = svc.ResolveAssault(new ResolveAssaultCommand());
            PrintLog(svc.State);
            PrintBattle(result);
            Console.WriteLine($"  结论：{SiegeState.Outcome(result.Outcome)} — 外援延迟、有代价、可背约；");
            Console.WriteLine($"        是我扛住围城压力、等到援军抵达，才稳住战线（守军余 {svc.State.Defender.Troops}）。");
        }

        // ───────── 输出工具 ─────────
        static void PrintOpening(SiegeState s)
        {
            Console.WriteLine($"  开局 {s.Clock}·{Domain.Environment.Weather.Name(s.CurrentWeather)}：守军 {s.Defender.Troops}（工事 {s.Fortification}） vs 敌先锋 {s.Attacker.Troops}");
            Console.WriteLine($"  敌军初始：士气 {s.Attacker.UnitMorale} 疲劳 {s.Attacker.Fatigue} 补给 {s.Attacker.SupplyState}");
        }
        static void PrintLog(SiegeState s)
        {
            Console.WriteLine("  因果链（来源 → 修正 → 结果）：");
            foreach (var line in s.CausalLog) Console.WriteLine("    " + line);
        }
        static void PrintBattle(BattleResult r)
        {
            Console.WriteLine($"  战力：攻 {r.AttackerPower} vs 守 {r.DefenderPower}（比 {r.PowerRatio}）");
            Console.WriteLine("  决定性因素（≤5）：");
            foreach (var f in r.Factors) Console.WriteLine($"    · {f.Name}：{f.Detail}");
            Console.WriteLine($"  伤亡：攻方 -{r.AttackerCasualties}，守方 -{r.DefenderCasualties}");
        }
        static void Expect(CommandResult r, string what)
        {
            if (!r.Ok) Console.WriteLine($"  [命令失败] {what}：{r.Code}");
        }
    }
}
