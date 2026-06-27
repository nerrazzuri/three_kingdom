using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城池控制权权威的内存实现（ADR-0008：GDD_004 唯一权威写入点）。
    /// 持有 city.owner / city.garrison 权威态；<see cref="RequestControlChange"/> 为唯一写路径——校验后写并发布
    /// <see cref="CityControlChanged"/>。生涯/世界/战役层只发起请求或订阅，绝不直接写归属。
    /// <para>确定性：变更与发布无随机；订阅者按注册顺序通知。</para>
    /// </summary>
    public sealed class CityControlAuthority : ICityControlAuthority
    {
        private readonly Dictionary<CityId, (FactionId Owner, Garrison Garrison)> _control
            = new Dictionary<CityId, (FactionId, Garrison)>();
        private readonly List<Action<CityControlChanged>> _subscribers = new List<Action<CityControlChanged>>();

        /// <summary>登记开局城池归属（权威初始态）。重复登记同城抛。</summary>
        public void RegisterInitial(CityId city, FactionId owner, Garrison garrison)
        {
            if (_control.ContainsKey(city))
                throw new InvalidOperationException($"城池已登记，不可重复初始化：{city}。");
            _control[city] = (owner, garrison);
        }

        public FactionId? OwnerOf(CityId city)
            => _control.TryGetValue(city, out (FactionId Owner, Garrison Garrison) v) ? v.Owner : (FactionId?)null;

        public Garrison? GarrisonOf(CityId city)
            => _control.TryGetValue(city, out (FactionId Owner, Garrison Garrison) v) ? v.Garrison : (Garrison?)null;

        public void RequestControlChange(CityId city, FactionId newOwner, Garrison garrison, ChangeCause cause)
        {
            if (!_control.ContainsKey(city))
                throw new InvalidOperationException($"城池未登记，无法变更控制权（须先 RegisterInitial）：{city}。");

            _control[city] = (newOwner, garrison);   // 唯一权威写
            var evt = new CityControlChanged(city, newOwner, garrison, cause);
            foreach (Action<CityControlChanged> sub in _subscribers) sub(evt);  // 发布
        }

        public void Subscribe(Action<CityControlChanged> handler)
            => _subscribers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
    }
}
