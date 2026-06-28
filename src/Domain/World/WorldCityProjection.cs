using System;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 世界模型的城池归属只读投影同步器（GDD_015 §城池归属 / TR-world-003 / ADR-0008）。
    /// 订阅 GDD_004 的 <see cref="CityControlChanged"/> 事件，把战略尺度归属反映同步进 <see cref="Current"/>。
    /// <para>
    /// <b>世界模型不独立写归属</b>：本投影<b>无</b>对外写 city.owner 的 API——唯一更新路径是订阅的控制权变更事件
    /// （ADR-0008 归属唯一权威在 GDD_004）。历史结局的 owner_change 须经
    /// <see cref="ICityControlAuthority.RequestControlChange"/> 发起，再由事件回流至此。
    /// </para>
    /// </summary>
    public sealed class WorldCityProjection
    {
        /// <summary>当前世界态（含已同步的归属投影）。</summary>
        public WorldState Current { get; private set; }

        /// <summary>以初始世界态绑定，并订阅控制权变更事件。</summary>
        public WorldCityProjection(WorldState initial, ICityControlAuthority authority)
        {
            Current = initial ?? throw new ArgumentNullException(nameof(initial));
            if (authority is null) throw new ArgumentNullException(nameof(authority));
            authority.Subscribe(OnCityControlChanged);
        }

        private void OnCityControlChanged(CityControlChanged e)
        {
            // 唯一更新路径：订阅事件 → 同步只读投影（不独立写归属）。
            Current = Current.WithCityOwnership(e.City, e.NewOwner, e.Garrison.Value);
        }
    }
}
