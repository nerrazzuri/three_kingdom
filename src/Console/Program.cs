using System;

namespace ThreeKingdom.Console
{
    /// <summary>
    /// 完整游戏文本控制台入口（仅 I/O 薄壳）。渲染/命令分派在 <see cref="GameConsole"/>（纯逻辑，可脚本回放·确定性）。
    /// 用法：<c>dotnet run --project src/Console</c>。<c>--script "gov city-chenliu|w|w|mission"</c> 非交互回放（竖线分隔）。
    /// 整个游戏在此可玩——无 UI，纯文本（选年选城→经营→君命→出征→施计→晋升/自立→一生→被灭续局/传承）。
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            var game = new GameConsole();

            int scriptIdx = Array.IndexOf(args, "--script");
            if (scriptIdx >= 0 && scriptIdx + 1 < args.Length)
            {
                foreach (string token in args[scriptIdx + 1].Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    System.Console.WriteLine(game.StatusText());
                    System.Console.WriteLine($"> {token.Trim()}");
                    System.Console.WriteLine(game.Dispatch(token.Trim()));
                    if (game.Quit) break;
                }
                System.Console.WriteLine(game.StatusText());
                return 0;
            }

            System.Console.WriteLine("【三国·空降者】完整文本控制台 — 整个游戏可玩（无 UI）。默认「汜水关太守」开局。");
            System.Console.WriteLine(GameConsole.Menu());
            while (!game.Quit)
            {
                System.Console.WriteLine();
                System.Console.WriteLine(game.StatusText());
                System.Console.Write("> ");
                string? line = System.Console.ReadLine();
                if (line is null) break;
                string feedback = game.Dispatch(line);
                if (feedback.Length > 0) System.Console.WriteLine(feedback);
            }
            return 0;
        }
    }
}
