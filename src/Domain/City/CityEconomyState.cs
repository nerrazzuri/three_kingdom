using System;

namespace ThreeKingdom.Domain.City
{
    /// <summary>
    /// 城市经济权威状态（GDD_004 §Data Model：CityState / TR-city-001 / ADR-0002）。
    /// 不可变聚合：粮食库存、已承诺保留量、城市民心、治安、工事；构造时校验不变量，
    /// 失败即抛、无部分写入。日界结算经 <see cref="CityDaySettlementService"/> 产出新实例 + 账本，
    /// <b>不</b>就地修改（确定性可复盘，ADR-0004）。
    /// <para>
    /// 粮量为权威整数（粒/单位），不使用 float（ADR-0004 §权威路径禁 float）。
    /// 民用与军用粮食<b>同一权威库存</b>（<see cref="Stock"/>）；拨给军队的部分经
    /// 「承诺→移交后勤」转移所有权，城市不再计入（守恒，无双计，TR-city-001）。
    /// </para>
    /// </summary>
    public sealed class CityEconomyState
    {
        /// <summary>城市 ID。</summary>
        public CityId Id { get; }

        /// <summary>当前粮食库存（权威整数，≥0）。民用与军用同源。</summary>
        public long Stock { get; }

        /// <summary>已承诺保留量（已批准未执行的军粮征用，0..<see cref="Stock"/>）。日界「承诺」阶段移交后勤。</summary>
        public long Reserved { get; }

        /// <summary>城市民心（居民支持/怨恨，≥0；与 GDD_011 部队士气是不同状态，命名消歧 civ_morale）。</summary>
        public int CivMorale { get; }

        /// <summary>治安（秩序与执行能力，≥0；与民心是不同状态，不得合并）。</summary>
        public int Security { get; }

        /// <summary>工事当前值（0..<see cref="FortificationMax"/>）。</summary>
        public int FortificationCurrent { get; }

        /// <summary>工事最大值（≥0）。</summary>
        public int FortificationMax { get; }

        public CityEconomyState(
            CityId id,
            long stock,
            long reserved,
            int civMorale,
            int security,
            int fortificationCurrent,
            int fortificationMax)
        {
            if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock), "库存不可为负。");
            if (reserved < 0) throw new ArgumentOutOfRangeException(nameof(reserved), "保留量不可为负。");
            if (reserved > stock) throw new ArgumentOutOfRangeException(nameof(reserved), "保留量不可超过库存（reserved ≤ stock）。");
            if (civMorale < 0) throw new ArgumentOutOfRangeException(nameof(civMorale), "民心不可为负。");
            if (security < 0) throw new ArgumentOutOfRangeException(nameof(security), "治安不可为负。");
            if (fortificationMax < 0) throw new ArgumentOutOfRangeException(nameof(fortificationMax), "工事最大值不可为负。");
            if (fortificationCurrent < 0 || fortificationCurrent > fortificationMax)
                throw new ArgumentOutOfRangeException(nameof(fortificationCurrent), "工事当前值须在 [0, 工事最大值]。");

            Id = id;
            Stock = stock;
            Reserved = reserved;
            CivMorale = civMorale;
            Security = security;
            FortificationCurrent = fortificationCurrent;
            FortificationMax = fortificationMax;
        }

        /// <summary>可自由分配量（派生：stock − reserved，恒 ≥0）。</summary>
        public long Available => Stock - Reserved;

        /// <summary>返回替换指定字段后的新实例（不可变更新；其余字段不变）。</summary>
        public CityEconomyState With(
            long? stock = null,
            long? reserved = null,
            int? civMorale = null,
            int? security = null,
            int? fortificationCurrent = null)
            => new CityEconomyState(
                Id,
                stock ?? Stock,
                reserved ?? Reserved,
                civMorale ?? CivMorale,
                security ?? Security,
                fortificationCurrent ?? FortificationCurrent,
                FortificationMax);
    }
}
