using ThreeKingdom.Presentation.Runtime;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Unity.UI
{
    /// <summary>
    /// 区域战斗屏进程内单一来源（Unity 侧薄静态壳，GDD_021 §12 / epic-031 S7）。
    /// 把 Unity 战斗屏接到纯 C# <see cref="ZoneBattleRuntime"/>（dotnet 已单测）——生命周期/规则全在 Domain/Application，
    /// 本类只转发。不持可变 Domain 对象；UI 只拿只读 <see cref="ZoneBattleView"/>（ADR-0002）。
    /// </summary>
    public static class ZoneBattleSession
    {
        private static ZoneBattleRuntime _runtime;

        /// <summary>是否已有进行中的战斗。</summary>
        public static bool Active => _runtime != null;

        /// <summary>开始演示战斗（假退诱敌攻虎牢关；与 console/dotnet 同一引擎）。</summary>
        public static void StartDemo() => _runtime = ZoneBattleRuntime.Demo();

        /// <summary>当前战斗投影（未开战则先开演示局）。</summary>
        public static ZoneBattleView View()
        {
            if (_runtime == null) StartDemo();
            return _runtime.View();
        }

        /// <summary>推进一回合（敌AI + 结算），返回战后投影。</summary>
        public static ZoneBattleView ResolveRound()
        {
            if (_runtime == null) StartDemo();
            return _runtime.ResolveRound();
        }

        /// <summary>调动己方支队到相邻区（排兵布阵）；返回是否成功。</summary>
        public static bool Move(string detachmentId, string zoneId)
            => _runtime != null && _runtime.MoveDetachment(detachmentId, zoneId).Applied;

        /// <summary>战斗是否已终局。</summary>
        public static bool IsOver => _runtime != null && _runtime.IsOver;
    }
}
