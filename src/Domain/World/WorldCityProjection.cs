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

        /// <summary>
        /// 确定性推进世界时间（GDD_015 / ADR-0004，story epic-013-002）。复用 <see cref="WorldProgressionService"/>，
        /// 供 CampaignSession 日界推进编排调用——归属仍只经订阅事件更新，时间是另一独立确定性驱动。
        /// </summary>
        public void AdvanceTime(int segments)
        {
            Current = _progression.Advance(Current, segments);
        }

        /// <summary>
        /// 创建新势力（GDD_015 为势力存续唯一权威，R-3）。供 CampaignSession 自立后果写回编排调用——
        /// GDD_014 发起请求、015 创建 FactionRecord；非"写归属"（归属仍只经 004 事件）。
        /// </summary>
        public void CreateFaction(FactionRecord faction)
        {
            Current = Current.WithFaction(faction);
        }

        /// <summary>恢复到指定世界态（仅供 ConsequenceTransaction 回滚使用，R-6）。</summary>
        public void RestoreTo(WorldState world)
        {
            Current = world ?? throw new ArgumentNullException(nameof(world));
        }

        private readonly WorldProgressionService _progression = new WorldProgressionService();
    }
}
