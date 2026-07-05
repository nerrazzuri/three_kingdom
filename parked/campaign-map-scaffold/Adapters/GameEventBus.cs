using System;
using System.Collections.Generic;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// 简易类型化事件总线（scaffold IGameEventBus 实现）：一张 Type→委托列表 的字典。
    /// Application 侧状态变更后 Publish&lt;T&gt;，Presentation 侧 Subscribe&lt;T&gt; 响应而不知领域内部。★需 Unity 编辑器验证。
    /// </summary>
    public sealed class GameEventBus : IGameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (!_handlers.TryGetValue(typeof(T), out var list)) { list = new List<Delegate>(); _handlers[typeof(T)] = list; }
            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (_handlers.TryGetValue(typeof(T), out var list)) list.Remove(handler);
        }

        public void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // 复制一份再遍历，容忍回调内 (取消)订阅。
            foreach (Delegate d in list.ToArray())
                (d as Action<T>)?.Invoke(evt);
        }
    }
}
