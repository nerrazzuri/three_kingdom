using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 区域战斗屏进程内路由（Unity 侧薄静态壳，GDD_021 §12 / epic-031 S7）。按<b>模式</b>把独立战斗场景接到
    /// campaign 真战斗（出征/守城，经 <see cref="SessionRuntime"/>）或演示局。生命周期/规则全在纯 C#（dotnet 已单测）。
    /// 玩家可<b>亲自打</b>（逐回合）或<b>挂 AI 代打</b>（<see cref="AutoResolve"/>，代打不作弊、不保证赢）。
    /// </summary>
    public static class ZoneBattleSession
    {
        public enum Mode { Demo, Offensive, Defense }

        /// <summary>当前战斗模式（进战斗场景前由 HUD 设定）。</summary>
        public static Mode Current { get; private set; } = Mode.Demo;

        /// <summary>结算后返回的场景名（进战斗前捕获来源场景）。</summary>
        public static string ReturnScene { get; private set; } = "Hud";

        private static ZoneBattleRuntime _demo;

        /// <summary>进出征战斗（HUD 发起出征后调用，随后加载 ZoneBattle 场景）。</summary>
        public static void EnterOffensive(string returnScene) { Current = Mode.Offensive; ReturnScene = returnScene; }

        /// <summary>进守城战斗。</summary>
        public static void EnterDefense(string returnScene) { Current = Mode.Defense; ReturnScene = returnScene; }

        /// <summary>演示战斗（无 campaign 时自我演示）。</summary>
        public static void StartDemo() { Current = Mode.Demo; _demo = ZoneBattleRuntime.Demo(); }

        /// <summary>当前战斗投影（未开演示则先起演示局）。</summary>
        public static ZoneBattleView View() => Current switch
        {
            Mode.Offensive => SessionRuntime.OffensiveBattleView(),
            Mode.Defense => SessionRuntime.DefenseBattleView(),
            _ => Demo().View(),
        };

        /// <summary>玩家亲自打：推进一回合（敌AI + 结算）。</summary>
        public static ZoneBattleView ResolveRound() => Current switch
        {
            Mode.Offensive => SessionRuntime.OffensiveBattleResolveRound(),
            Mode.Defense => SessionRuntime.DefenseBattleResolveRound(),
            _ => Demo().ResolveRound(),
        };

        /// <summary>挂 AI 代打至终局（双方皆 AI，胜负由部署/对阵决定，不保证赢）。</summary>
        public static ZoneBattleView AutoResolve() => Current switch
        {
            Mode.Offensive => SessionRuntime.OffensiveBattleAutoResolve(),
            Mode.Defense => SessionRuntime.DefenseBattleAutoResolve(),
            _ => Demo().AutoResolve(),
        };

        /// <summary>战中调动己方支队到相邻区（排兵布阵）。</summary>
        public static bool Move(string detachmentId, string zoneId) => Current switch
        {
            Mode.Offensive => SessionRuntime.OffensiveBattleMove(detachmentId, zoneId),
            Mode.Defense => SessionRuntime.DefenseBattleMove(detachmentId, zoneId),
            _ => Demo().MoveDetachment(detachmentId, zoneId).Applied,
        };

        /// <summary>战斗是否已终局。</summary>
        public static bool IsOver => Current switch
        {
            Mode.Offensive => SessionRuntime.OffensiveBattleOver,
            Mode.Defense => SessionRuntime.DefenseBattleOver,
            _ => _demo == null || _demo.IsOver,
        };

        /// <summary>结算战果（出征→占城归属/退兵，权威写回；守城→守土成败），返回结论文案。</summary>
        public static string Conclude()
        {
            switch (Current)
            {
                case Mode.Offensive:
                    OffensiveResultView r = SessionRuntime.ConcludeOffensive();
                    return r.ConclusionLabel + (string.IsNullOrEmpty(r.OwnershipLabel) ? string.Empty : "　" + r.OwnershipLabel);
                case Mode.Defense:
                    return SessionRuntime.DefenseHeld ? "守土成功，退敌！" : "城破——守御失利。";
                default:
                    return string.Empty;
            }
        }

        private static ZoneBattleRuntime Demo()
        {
            if (_demo == null) StartDemo();
            return _demo;
        }
    }
}
