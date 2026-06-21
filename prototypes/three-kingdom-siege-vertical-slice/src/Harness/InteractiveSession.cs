// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 玩家无引导自由选链、逐段承诺，亲身体验「创造条件取胜」
// Date: 2026-06-21

using System;
using TkSlice.Application;
using TkSlice.Domain.Advisory;
using TkSlice.Domain.Battle;
using TkSlice.Domain.Diplomacy;
using TkSlice.Domain.Environment;
using TkSlice.Domain.Siege;

namespace TkSlice.Harness
{
    /// <summary>控制台交互会话：状态面板 → 玩家命令 → 因果反馈 → 结算。文本占位 UI。</summary>
    public sealed class InteractiveSession
    {
        private SiegeService _svc = null!;
        private int _logShown;
        private BattleOutcome? _lastAssault;
        private int _actionsTaken;
        private const int OnboardingTurns = 3;   // 前期自动显示军师建议，帮生疏玩家上手

        public void Run()
        {
            _svc = new SiegeService(SiegeScenario.CreateXishuiSiege());
            _logShown = 0;

            Console.WriteLine("=== 汜水小城守御 — 交互模式 ===");
            Console.WriteLine("你是守将。敌先锋 2 倍于你，正面必败。靠侦察、准备与承诺创造取胜条件。\n");

            bool running = true;
            while (running)
            {
                PrintPanel();
                if (_actionsTaken < OnboardingTurns) PrintAdvice();   // 前期自动军师建议
                PrintMenu();
                string? line = Console.ReadLine();
                if (line == null) break;        // 管道/EOF
                line = line.Trim();
                running = Dispatch(line);
                FlushNewLog();
                if (running) running = !CheckEnd();
            }
            Console.WriteLine("\n（会话结束）");
        }

        private void PrintPanel()
        {
            var s = _svc.State;
            Console.WriteLine("────────────────────────────────────────");
            Console.WriteLine($" {s.Clock}·{Weather.Name(s.CurrentWeather)}   外交压力 {s.DiplPressure}");
            Console.WriteLine($" 守军 {s.Defender.Troops}  工事 {s.Fortification}  士气 {s.Defender.UnitMorale}  军纪 {s.Defender.Discipline}  城粮 {s.CityFood}（己方已知）");
            Console.WriteLine($" 敌军（探报）：{s.Intel.Describe(s.Clock.TotalSegments)}");
            if (s.EnemyRaidCutActive) Console.WriteLine($" ▸ 袭扰队断粮中（{s.RaidUnits} 支）——成效须侦察确认");
            if (s.PendingPledge != null) Console.WriteLine($" ▸ 外援在途：{DiplomaticPledge.TypeName(s.PendingPledge.Type)}，约 {s.PendingPledge.ArrivalT} 抵达");
        }

        private static void PrintMenu()
        {
            Console.WriteLine("命令：[1]投袭扰断粮 [2]佯退伏击 [3]遣使外交 [4]推进时段 [5]撤袭扰 [6]迎战决战 [7]侦察敌情 [?]军师建议 [0]退出");
            Console.Write("> ");
        }

        /// <summary>显示军师建议（GDD_008）：观察 + 候选路线 + 风险，但不替玩家定计。</summary>
        private void PrintAdvice()
        {
            var adv = Advisor.Advise(_svc.State);
            Console.WriteLine("┌─ 军师进言 ───────────────────────────");
            foreach (var o in adv.Observations) Console.WriteLine("│ 观察：" + o);
            Console.WriteLine("│ 可行兵法路线（自行权衡，未排优劣）：");
            foreach (var r in adv.Routes)
            {
                string tag = r.Available ? "○可行" : "×暂不可行";
                Console.WriteLine($"│   【{r.Name}·{tag}】");
                Console.WriteLine($"│     所需：{r.Needs}");
                Console.WriteLine($"│     风险：{r.Risk}");
                Console.WriteLine($"│     操作：{r.How}");
            }
            foreach (var m in adv.MissingIntel) Console.WriteLine("│ 缺情：" + m);
            Console.WriteLine($"│ 军师置信：{adv.Confidence}");
            Console.WriteLine($"│ {adv.Disclaimer}");
            Console.WriteLine("└──────────────────────────────────────");
        }

        private bool Dispatch(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts.Length > 0 ? parts[0] : "";
            switch (cmd)
            {
                case "1":
                    {
                        int units = ArgInt(parts, 1, 2);
                        Report(_svc.CommitRaid(new CommitRaidCommand(units)));
                        break;
                    }
                case "2":
                    {
                        int decoy = ArgInt(parts, 1, 150);
                        int ambush = ArgInt(parts, 2, 350);
                        var r = _svc.FeignedRetreat(new FeignedRetreatCommand(decoy, ambush), out var chk);
                        if (!chk.Ok) Console.WriteLine($"  ✗ {chk.Code}");
                        else if (r.Ambush != null) PrintBattle(r.Ambush);
                        break;
                    }
                case "3":
                    {
                        PledgeType type = (parts.Length > 1 ? parts[1] : "a") switch
                        {
                            "b" => PledgeType.Supply,
                            "c" => PledgeType.Deadline,
                            _ => PledgeType.Relief,
                        };
                        int cost = ArgInt(parts, 2, 60);
                        var (chk, _) = _svc.RequestDiplomacy(new RequestDiplomacyCommand(type, cost));
                        if (!chk.Ok) Console.WriteLine($"  ✗ {chk.Code}");
                        break;
                    }
                case "4":
                    {
                        int n = ArgInt(parts, 1, 1);
                        Report(_svc.Advance(new AdvanceSegmentCommand(n)));
                        break;
                    }
                case "5":
                    _svc.State.StopRaid();
                    break;
                case "7":
                    Report(_svc.Scout(new ScoutCommand()));
                    break;
                case "6":
                    {
                        var result = _svc.ResolveAssault(new ResolveAssaultCommand());
                        _lastAssault = result.Outcome;
                        PrintBattle(result);
                        break;
                    }
                case "?":
                    PrintAdvice();
                    return true;        // 请教军师不消耗时段，也不计入上手回合
                case "0":
                    return false;
                default:
                    Console.WriteLine("  （无效命令）");
                    return true;
            }
            _actionsTaken++;
            return true;
        }

        private bool CheckEnd()
        {
            var s = _svc.State;
            if (s.Defender.Troops <= 0 || _lastAssault == BattleOutcome.AttackerDecisive)
            {
                Console.WriteLine("\n★ 城破。失败——但你可撤退、求和、东山再起（后果延续）。");
                return true;
            }
            if (s.Attacker.Troops <= 150 || _lastAssault == BattleOutcome.AttackerRepelled)
            {
                Console.WriteLine("\n★ 守城成功！你以弱胜强守住汜水——靠的是创造的条件，不是技能。");
                return true;
            }
            _lastAssault = null;   // 僵持可继续
            return false;
        }

        // ───────── 工具 ─────────
        private void FlushNewLog()
        {
            var log = _svc.State.CausalLog;
            for (int i = _logShown; i < log.Count; i++) Console.WriteLine("    · " + log[i]);
            _logShown = log.Count;
        }
        private static void Report(CommandResult r)
        {
            if (!r.Ok) Console.WriteLine($"  ✗ {r.Code}");
        }
        private static int ArgInt(string[] parts, int idx, int dflt)
            => parts.Length > idx && int.TryParse(parts[idx], out int v) ? v : dflt;
        private static void PrintBattle(BattleResult r)
        {
            Console.WriteLine($"  战力 攻 {r.AttackerPower} vs 守 {r.DefenderPower}（比 {r.PowerRatio}）→ {SiegeState.Outcome(r.Outcome)}");
            foreach (var f in r.Factors) Console.WriteLine($"      · {f.Name}：{f.Detail}");
            Console.WriteLine($"  伤亡 攻 -{r.AttackerCasualties} / 守 -{r.DefenderCasualties}");
        }
    }
}
