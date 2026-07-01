using System;

namespace ThreeKingdom.Console
{
    /// <summary>
    /// M15 交互控制台入口（仅 I/O 薄壳）。渲染（<see cref="CampaignTextView"/>）与命令分派
    /// （<see cref="CampaignDriver"/>）的纯逻辑在别处、可单测；本类只读输入、写输出。
    /// 用法：<c>dotnet run --project src/Console</c>。支持 <c>--script "1 5 6 7 8 ..."</c> 非交互回放（确定性自检）。
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            var driver = new CampaignDriver();

            // 非交互脚本模式：把空格分隔的输入符逐个回放（供 CI/手动确定性自检）。
            int scriptIdx = Array.IndexOf(args, "--script");
            if (scriptIdx >= 0 && scriptIdx + 1 < args.Length)
            {
                foreach (string token in args[scriptIdx + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    System.Console.WriteLine(CampaignTextView.Status(driver.Session));
                    System.Console.WriteLine($"> {token}");
                    System.Console.WriteLine(driver.Step(token));
                    if (driver.Quit) break;
                }
                System.Console.WriteLine(CampaignTextView.Status(driver.Session));
                return 0;
            }

            // 交互模式。
            System.Console.WriteLine("【汜水关太守】M15 交互控制台 — 完整 CampaignSession 全循环可玩。");
            System.Console.WriteLine(CampaignTextView.Menu());
            while (!driver.Quit)
            {
                System.Console.WriteLine();
                System.Console.Write(CampaignTextView.Status(driver.Session));
                System.Console.Write("> ");
                string? line = System.Console.ReadLine();
                if (line is null) break;   // EOF（管道结束）→ 退出
                string feedback = driver.Step(line);
                if (feedback.Length > 0) System.Console.WriteLine(feedback);
            }
            return 0;
        }
    }
}
