using System;

namespace ThreeKingdom.Domain.Supply
{
    /// <summary>
    /// 补给链权威状态（GDD_012 §Formula 1 全局守恒 / TR-supply-001 / ADR-0004）。
    /// 不可变快照：粮食分布于三类权威持有者（城市/营地、在途运输批次、单位携行），
    /// 另记已消耗与已损耗以维持<b>全局守恒不变量</b>。所有转移为「一减一加」原子事务，
    /// 同一批粮<b>不能同时留城与在途</b>（无双计）。补给量为权威整数（无 float，ADR-0004）。
    /// <para>
    /// 守恒不变量：<see cref="GrandTotal"/> = 城市 + 在途 + 携行 + 已消耗 + 已损耗，
    /// 跨任意转移/结算操作恒定（faucet/sink 仅经显式的消耗/损耗计入，绝不凭空增减）。
    /// </para>
    /// </summary>
    public sealed class SupplyChainState
    {
        /// <summary>城市/营地库存（≥0）。</summary>
        public long CityStock { get; }

        /// <summary>在途运输批次载量（≥0）。</summary>
        public long ConvoyLoad { get; }

        /// <summary>单位携行库存（≥0）。</summary>
        public long UnitCarried { get; }

        /// <summary>累计已消耗（≥0；单位需求的 sink）。</summary>
        public long Consumed { get; }

        /// <summary>累计已损耗（≥0；在途天气/道路损耗的 sink）。</summary>
        public long Lost { get; }

        /// <summary>单位连续短缺累计时段（≥0；GDD_012 §Formula 5）。</summary>
        public int ShortageSegments { get; }

        public SupplyChainState(
            long cityStock,
            long convoyLoad,
            long unitCarried,
            long consumed,
            long lost,
            int shortageSegments)
        {
            if (cityStock < 0) throw new ArgumentOutOfRangeException(nameof(cityStock), "城市库存不可为负。");
            if (convoyLoad < 0) throw new ArgumentOutOfRangeException(nameof(convoyLoad), "在途载量不可为负。");
            if (unitCarried < 0) throw new ArgumentOutOfRangeException(nameof(unitCarried), "携行量不可为负。");
            if (consumed < 0) throw new ArgumentOutOfRangeException(nameof(consumed), "已消耗不可为负。");
            if (lost < 0) throw new ArgumentOutOfRangeException(nameof(lost), "已损耗不可为负。");
            if (shortageSegments < 0) throw new ArgumentOutOfRangeException(nameof(shortageSegments), "短缺累计时段不可为负。");

            CityStock = cityStock;
            ConvoyLoad = convoyLoad;
            UnitCarried = unitCarried;
            Consumed = consumed;
            Lost = lost;
            ShortageSegments = shortageSegments;
        }

        /// <summary>守恒总量（城市 + 在途 + 携行 + 已消耗 + 已损耗），跨操作恒定。</summary>
        public long GrandTotal => checked(CityStock + ConvoyLoad + UnitCarried + Consumed + Lost);

        /// <summary>当前仍在系统内流动的活粮（城市 + 在途 + 携行）。</summary>
        public long LiveTotal => checked(CityStock + ConvoyLoad + UnitCarried);

        /// <summary>返回替换指定字段后的新实例（不可变更新；其余字段不变）。</summary>
        public SupplyChainState With(
            long? cityStock = null,
            long? convoyLoad = null,
            long? unitCarried = null,
            long? consumed = null,
            long? lost = null,
            int? shortageSegments = null)
            => new SupplyChainState(
                cityStock ?? CityStock,
                convoyLoad ?? ConvoyLoad,
                unitCarried ?? UnitCarried,
                consumed ?? Consumed,
                lost ?? Lost,
                shortageSegments ?? ShortageSegments);
    }
}
