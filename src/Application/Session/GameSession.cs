using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 一局游戏会话的 Application 聚合（ADR-0002）：编排 Domain 权威状态、对外只暴露<b>只读投影</b>。
    /// 当前 slice 阶段会话仅持有世界时钟（<see cref="WorldClock"/>）作为最小可玩状态——后续可扩展持有
    /// 城市/补给/情报等聚合。<b>可变状态封装于此</b>，仅经 <see cref="SessionService"/> 推进；
    /// Presentation 永不直接持有或修改本聚合内的 Domain 对象（红线：UI/MonoBehaviour 不碰可变 Domain 状态）。
    /// </summary>
    public sealed class GameSession
    {
        private readonly WorldClock _clock;

        internal GameSession(WorldTime start) => _clock = new WorldClock(start);

        /// <summary>当前权威世界时间（只读）。</summary>
        public WorldTime CurrentTime => _clock.Current;

        /// <summary>推进时钟（仅 Application 层经 <see cref="SessionService"/> 调用）。返回起止与穿越日界。</summary>
        internal AdvanceResult Advance(int segments) => _clock.Apply(new AdvanceTimeCommand(segments));
    }
}
