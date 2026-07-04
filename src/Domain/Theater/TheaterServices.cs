using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Theater
{
    /// <summary>
    /// 委任下属可做的动作（GDD M12：<b>仅本地治理</b>，不越权做战略）。枚举<b>结构上不含</b>出征/宣战/战区令等战略动作
    /// （负向不变量：委任 AI 不越权）。
    /// </summary>
    public enum DelegateAction
    {
        Idle = 0,          // 无为（态势平稳）
        Requisition = 1,   // 征用军粮（本地）
        Repair = 2,        // 修工事（本地）
        Appease = 3,       // 安抚民心（本地）
    }

    /// <summary>委任治理阈值（数据驱动）。不可变。</summary>
    public sealed class DelegateGovernanceConfig
    {
        public int LowMorale { get; }
        public int LowFortification { get; }
        public long AmpleStock { get; }

        public DelegateGovernanceConfig(int lowMorale, int lowFortification, long ampleStock)
        {
            LowMorale = lowMorale;
            LowFortification = lowFortification;
            AmpleStock = ampleStock;
        }

        public static DelegateGovernanceConfig Default { get; } = new DelegateGovernanceConfig(lowMorale: 40, lowFortification: 40, ampleStock: 150);
    }

    /// <summary>
    /// 委任下属自理（GDD M12 story-002 / ADR-0006 精神，确定性）：受任城由下属按本地态势择<b>本地治理</b>动作，
    /// <b>绝不越权做战略</b>（动作空间结构上仅本地，无出征/宣战/战区令）。纯函数、无随机（规则化优先级）。
    /// </summary>
    public sealed class DelegateGovernanceService
    {
        /// <summary>下属按本地态势择动作：民心低→安抚；工事低→修；粮丰→征；否则无为。确定性优先级。</summary>
        public DelegateAction ChooseAction(long stock, int morale, int fortification, DelegateGovernanceConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (morale < config.LowMorale) return DelegateAction.Appease;
            if (fortification < config.LowFortification) return DelegateAction.Repair;
            if (stock >= config.AmpleStock) return DelegateAction.Requisition;
            return DelegateAction.Idle;
        }
    }

    /// <summary>
    /// 多城资源账（GDD M12 story-003：跨城资源/补给<b>守恒</b>——无凭空产出/丢失，调配经命令路径）。不可变。
    /// </summary>
    public sealed class TheaterResources
    {
        private readonly Dictionary<string, long> _stock;   // cityId → 粮

        public TheaterResources(IReadOnlyDictionary<string, long>? stock)
        {
            _stock = new Dictionary<string, long>(StringComparer.Ordinal);
            if (stock != null)
                foreach (KeyValuePair<string, long> kv in stock)
                {
                    if (kv.Value < 0) throw new ArgumentOutOfRangeException(nameof(stock), "城粮不可为负。");
                    _stock[kv.Key] = kv.Value;
                }
        }

        public long StockOf(CityId city) => _stock.TryGetValue(city.Value ?? "", out long v) ? v : 0;

        /// <summary>全战区总粮（守恒不变量的检验量）。</summary>
        public long Total
        {
            get { long t = 0; foreach (long v in _stock.Values) t = checked(t + v); return t; }
        }

        /// <summary>
        /// 跨城调粮：<paramref name="from"/> → <paramref name="to"/> <paramref name="amount"/>（守恒：总量不变）。
        /// 源粮不足或量非法 → 抛（无部分写入）。
        /// </summary>
        public TheaterResources Transfer(CityId from, CityId to, long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "调粮量须为正。");
            if (from == to) throw new ArgumentException("源与目的同城。", nameof(to));
            long src = StockOf(from);
            if (src < amount) throw new InvalidOperationException("源城粮不足，调配被拒。");
            var next = new Dictionary<string, long>(_stock, StringComparer.Ordinal);
            next[from.Value] = src - amount;
            next[to.Value] = StockOf(to) + amount;
            return new TheaterResources(next);
        }

        internal void AppendTo(StateHasher h)
        {
            var keys = new List<string>(_stock.Keys);
            keys.Sort(StringComparer.Ordinal);
            h.Append(keys.Count);
            foreach (string k in keys)
            {
                h.Append(k.Length);
                foreach (char c in k) h.Append((int)c);
                h.Append(_stock[k]);
            }
        }
    }

    /// <summary>一座城的战区报告（GDD M12 story-004 反全知）：亲管城=即时准确；委任城=<b>下属汇报</b>（可滞后，非即时全知）。不可变。</summary>
    public sealed class TheaterCityReport
    {
        public CityId City { get; }
        public GovernanceMode Mode { get; }
        /// <summary>是否即时准确（亲管=真；委任=假，经下属汇报，反全知）。</summary>
        public bool Fresh { get; }
        /// <summary>报告的粮估值（委任城为下属汇报值，可能滞后）。</summary>
        public long ReportedStock { get; }

        internal TheaterCityReport(CityId city, GovernanceMode mode, bool fresh, long reportedStock)
        {
            City = city;
            Mode = mode;
            Fresh = fresh;
            ReportedStock = reportedStock;
        }
    }

    /// <summary>
    /// 战区报告投影（GDD M12 story-004 反全知）：玩家<b>不获全知势力面板</b>——亲管城即时准确，委任城经下属汇报
    /// （标 <see cref="TheaterCityReport.Fresh"/>=false）。纯函数。
    /// </summary>
    public sealed class TheaterReportService
    {
        /// <summary>由多城态 + 各城粮（委任城传入的是"上次汇报值"）构造报告；委任城标非即时（反全知）。</summary>
        public IReadOnlyList<TheaterCityReport> Build(TheaterState state, TheaterResources reported)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (reported == null) throw new ArgumentNullException(nameof(reported));
            var list = new List<TheaterCityReport>();
            foreach (CityHolding h in state.Holdings)
            {
                bool fresh = h.Mode == GovernanceMode.SelfGoverned;
                list.Add(new TheaterCityReport(h.City, h.Mode, fresh, reported.StockOf(h.City)));
            }
            return list;
        }
    }
}
