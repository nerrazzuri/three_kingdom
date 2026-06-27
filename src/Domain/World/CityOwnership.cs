using System;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 城池归属的<b>只读投影</b>（GDD_015 §城池归属 / TR-world-003 / ADR-0008）。
    /// 世界模型在战略尺度只读反映「城归属势力 + 守备」，<b>不独立写</b> <see cref="Owner"/>——
    /// 归属权威在 GDD_004，更新经其控制权变更事件订阅（story-004 接入）。
    /// <para>
    /// 本类型<b>无任何变更 API</b>：构造后不可改，故 Presentation/世界推进路径无法直接写归属（AC-5 编译级阻止）。
    /// </para>
    /// </summary>
    public sealed class CityOwnership
    {
        /// <summary>城池 ID。</summary>
        public CityId City { get; }

        /// <summary>归属势力（无主城为 null）。只读反映，写权威在 GDD_004。</summary>
        public FactionId? Owner { get; }

        /// <summary>守备值（≥0，战略尺度抽象兵备）。</summary>
        public int Garrison { get; }

        /// <summary>构造只读归属投影。守备为负即抛，无部分写入。</summary>
        public CityOwnership(CityId city, FactionId? owner, int garrison)
        {
            if (garrison < 0) throw new ArgumentOutOfRangeException(nameof(garrison), "守备不可为负。");
            City = city;
            Owner = owner;
            Garrison = garrison;
        }
    }
}
