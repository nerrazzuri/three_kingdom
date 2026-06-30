using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThreeKingdom.Domain.Career;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Preparation;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 完整游戏会话脊梁（ADR-0009）。**Application 装配层**：持有当前会话的 Domain 聚合引用与会话元数据，
    /// <b>只编排、不拥有玩法规则</b>（R-5：不计算玩法公式、不直接写 city.owner/势力存续）。
    /// 生涯/世界/控制权的权威仍在各 Domain 系统；本类型只读暴露其态、经服务路由变更。
    /// <para>
    /// 本 story（001）建骨架与配置驱动开局；日界推进（002）、后果原子写回（003）、统一存档（004）
    /// 由后续 story 在本脊梁上叠加，均经 <see cref="CampaignSessionService"/> 命令路径。
    /// </para>
    /// </summary>
    public sealed class CampaignSession
    {
        private readonly WorldCityProjection _worldProjection;

        /// <summary>会话稳定 id。</summary>
        public string Id { get; }

        /// <summary>场景配置 id（追溯）。</summary>
        public string ScenarioConfigId { get; }

        /// <summary>配置指纹（进入快照、载入校验）。</summary>
        public ConfigFingerprint Fingerprint { get; }

        /// <summary>生涯快照（CareerState + RetinueState）。变更只经 <see cref="CampaignSessionService"/> 命令路径。</summary>
        public CareerSnapshot Career { get; private set; }

        /// <summary>世界态（含订阅 GDD_004 的归属投影）。</summary>
        public WorldState World => _worldProjection.Current;

        /// <summary>当前世界时间。</summary>
        public WorldTime CurrentTime => World.CurrentTime;

        /// <summary>城池控制权权威（GDD_004，唯一写归属点；本会话只读它/经它发起，不直接写）。</summary>
        internal ICityControlAuthority Control { get; }

        /// <summary>
        /// 城市治理态（GDD_004 / M03）。<b>可选</b>：场景未启用城市治理时为 null（无日界城市结算、不入哈希）。
        /// 变更只经 <see cref="CampaignSessionService"/> 编排既有 <see cref="CityDaySettlementService"/> 产出。
        /// </summary>
        public CityEconomyState? CityEconomy { get; private set; }

        /// <summary>后勤持有的军粮总量（征用移交后单一计入此处，城市不再计；守恒 TR-city-001）。</summary>
        public long LogisticsHolding { get; private set; }

        /// <summary>是否启用城市治理循环（城市态存在）。</summary>
        public bool HasCityGovernance => CityEconomy != null;

        /// <summary>城市日结配置（数据驱动，ADR-0003）；仅供服务编排日结，城市治理未启用时为 null。</summary>
        internal CitySettlementConfig? SettlementConfig { get; }

        /// <summary>人口压力系数（GDD_004 §Formula 3，喂给日结民用需求）。</summary>
        internal FixedPoint PopulationPressure { get; }

        /// <summary>治理命令代价/增益配置（数据驱动，ADR-0003）；启用城市治理时必填。</summary>
        internal CityGovernanceConfig? GovernanceConfig { get; }

        // --- 情报态（M04 / GDD_007）。可选；四层分离——真值 internal、只读出口仅阵营知识投影（反全知）---
        private readonly FactionIntel? _playerIntel;

        /// <summary>世界真值（GDD_007 第 1 层）。<b>仅 internal</b>——侦察/存档经此读，<b>绝不</b>进 public 只读出口（反全知）。</summary>
        internal WorldTruthLedger? Truth { get; }

        /// <summary>玩家阵营知识层（internal，供侦察写入/军议读取/存档）。</summary>
        internal FactionIntel? PlayerIntel => _playerIntel;

        /// <summary>情报配置（数据驱动）；启用情报时必填。</summary>
        internal IntelConfig? IntelConfig { get; }

        /// <summary>是否启用情报/军议循环（情报态存在）。</summary>
        public bool HasIntel => _playerIntel != null;

        /// <summary>
        /// 玩家阵营知识只读投影（GDD_007 第 4 层 / TR-intel-001）——<b>显示层唯一可读情报入口</b>，
        /// 结构上不含真值字段，无从泄露 <see cref="Truth"/>（反全知）。未启用情报时为 null。
        /// </summary>
        public IntelProjection? PlayerKnowledge => _playerIntel?.Project();

        /// <summary>会话军议装配配置（M04 / GDD_008）；启用军议时存在。</summary>
        internal SessionCouncilSetup? Council { get; }

        // --- 战役准备态（M05 / GDD_009）。可选；草稿可变，提交经服务原子生成承诺 ---
        private readonly PlanDraft? _draft;

        /// <summary>当前资源池（只读；提交后为扣减锁定后的池。启用准备时存在）。</summary>
        public ResourcePool? Pool { get; private set; }

        /// <summary>计划草稿（internal，供编辑命令/提交/存档）。</summary>
        internal PlanDraft? Draft => _draft;

        /// <summary>准备校验配置（数据驱动）；启用准备时必填。</summary>
        internal PreparationConfig? PrepConfig { get; }

        /// <summary>可达区域（GDD_003，供提交校验）。</summary>
        internal IReadOnlyCollection<RegionId> ReachableRegions { get; }

        /// <summary>已授权命令（GDD_005/006，供提交校验）。</summary>
        internal IReadOnlyCollection<OrderId> AuthorizedOrders { get; }

        /// <summary>是否启用战役准备循环（资源池存在）。</summary>
        public bool HasPreparation => Pool != null;

        /// <summary>当前计划草稿命令只读视图（未启用准备时为 null）。</summary>
        public IReadOnlyList<PreparedOrder>? PlanOrders => _draft?.Orders;

        /// <summary>已承诺计划（提交成功后存在；未提交为 null）。</summary>
        public CommittedPlan? CommittedPlan { get; private set; }

        /// <summary>
        /// 当前知识快照 ID（GDD_008 §Formula 4）：由玩家已知条目（主题+估计值+观察时间）确定性派生。
        /// 侦察改变知识 → 快照变 → 已召开军议建议被标过时（<see cref="CouncilAdviceSet.IsStaleAgainst"/>）。
        /// 未启用情报时为 null。
        /// </summary>
        public KnowledgeSnapshotId? CurrentKnowledgeSnapshotId
        {
            get
            {
                if (_playerIntel == null) return null;
                var entries = new List<IntelKnowledgeEntry>(_playerIntel.Project().Entries);
                entries.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
                var sb = new StringBuilder("k");
                foreach (IntelKnowledgeEntry e in entries)
                    sb.Append('|').Append(e.Subject.Value).Append(':')
                      .Append(e.KnownStrength.ToString(CultureInfo.InvariantCulture)).Append('@')
                      .Append(e.ObservedAt.AbsoluteIndex.ToString(CultureInfo.InvariantCulture));
                return new KnowledgeSnapshotId(sb.ToString());
            }
        }

        internal CampaignSession(
            string id, string scenarioConfigId, ConfigFingerprint fingerprint,
            CareerSnapshot career, WorldCityProjection worldProjection, ICityControlAuthority control,
            CityEconomyState? cityEconomy = null, CitySettlementConfig? settlementConfig = null,
            FixedPoint populationPressure = default, long logisticsHolding = 0,
            CityGovernanceConfig? governanceConfig = null,
            WorldTruthLedger? truth = null, FactionIntel? playerIntel = null, IntelConfig? intelConfig = null,
            SessionCouncilSetup? council = null,
            ResourcePool? pool = null, PlanDraft? draft = null, PreparationConfig? prepConfig = null,
            IReadOnlyCollection<RegionId>? reachableRegions = null, IReadOnlyCollection<OrderId>? authorizedOrders = null,
            CommittedPlan? committedPlan = null)
        {
            if (logisticsHolding < 0) throw new ArgumentOutOfRangeException(nameof(logisticsHolding), "后勤持有量不可为负。");
            if (cityEconomy != null && settlementConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 settlementConfig。", nameof(settlementConfig));
            if (cityEconomy != null && governanceConfig == null)
                throw new ArgumentException("启用城市治理（cityEconomy 非空）时必须提供 governanceConfig。", nameof(governanceConfig));
            if (playerIntel != null && (truth == null || intelConfig == null))
                throw new ArgumentException("启用情报（playerIntel 非空）时必须提供 truth 与 intelConfig。", nameof(playerIntel));
            if (pool != null && prepConfig == null)
                throw new ArgumentException("启用准备（pool 非空）时必须提供 prepConfig。", nameof(prepConfig));

            Id = id;
            ScenarioConfigId = scenarioConfigId;
            Fingerprint = fingerprint;
            Career = career ?? throw new ArgumentNullException(nameof(career));
            _worldProjection = worldProjection ?? throw new ArgumentNullException(nameof(worldProjection));
            Control = control ?? throw new ArgumentNullException(nameof(control));
            CityEconomy = cityEconomy;
            SettlementConfig = settlementConfig;
            PopulationPressure = populationPressure;
            LogisticsHolding = logisticsHolding;
            GovernanceConfig = governanceConfig;
            Truth = truth;
            _playerIntel = playerIntel;
            IntelConfig = intelConfig;
            Council = council;
            Pool = pool;
            _draft = draft ?? (pool != null ? new PlanDraft() : null);
            PrepConfig = prepConfig;
            ReachableRegions = reachableRegions ?? Array.Empty<RegionId>();
            AuthorizedOrders = authorizedOrders ?? Array.Empty<OrderId>();
            CommittedPlan = committedPlan;
        }

        /// <summary>提交成功后写回承诺计划与扣减后资源池（仅供 <see cref="CampaignSessionService"/> 编排）。</summary>
        internal void ApplyCommittedPlan(CommittedPlan plan, ResourcePool resultingPool)
        {
            CommittedPlan = plan ?? throw new ArgumentNullException(nameof(plan));
            Pool = resultingPool ?? throw new ArgumentNullException(nameof(resultingPool));
        }

        /// <summary>日界推进世界时间（仅供 <see cref="CampaignSessionService"/> 按全局结算顺序编排调用）。</summary>
        internal void AdvanceWorld(int segments) => _worldProjection.AdvanceTime(segments);

        /// <summary>
        /// 应用一次城市日界结算结果（仅供 <see cref="CampaignSessionService"/> 编排 <see cref="CityDaySettlementService"/> 后写回）。
        /// 装配层不算公式，只写回 Domain 服务产出的新态与后勤持有量。
        /// </summary>
        internal void ApplyCitySettlement(CityEconomyState endState, long endLogisticsHolding)
        {
            CityEconomy = endState ?? throw new ArgumentNullException(nameof(endState));
            LogisticsHolding = endLogisticsHolding;
        }

        /// <summary>替换城市治理态（仅供治理命令编排，M03 story-002）。</summary>
        internal void SetCityEconomy(CityEconomyState state)
            => CityEconomy = state ?? throw new ArgumentNullException(nameof(state));

        // --- 仅供 ConsequenceTransaction 原子写回 / 回滚使用（R-6）---
        internal void SetCareer(CareerSnapshot career) => Career = career ?? throw new ArgumentNullException(nameof(career));
        internal void CreateFaction(Domain.World.FactionRecord faction) => _worldProjection.CreateFaction(faction);
        internal void RestoreWorld(WorldState world) => _worldProjection.RestoreTo(world);

        /// <summary>会话权威态的确定性哈希（生涯 ⊕ 世界 ⊕ 城市治理）——支撑确定性回归与存档校验。</summary>
        public StateHash ComputeHash()
        {
            var hasher = new StateHasher();
            Career.AppendTo(hasher);
            World.AppendTo(hasher);
            if (CityEconomy != null)
            {
                CityEconomy.AppendTo(hasher);
                hasher.Append(LogisticsHolding);
            }
            if (_playerIntel != null && Truth != null)
            {
                AppendIntel(hasher);
            }
            if (Pool != null)
            {
                AppendPreparation(hasher);
            }
            return hasher.ToHash();
        }

        /// <summary>准备态确定性哈希：资源池 ⊕ 草稿命令 ⊕ 已承诺计划（ADR-0004）。</summary>
        private void AppendPreparation(StateHasher hasher)
        {
            AppendResources(hasher, Pool!.AsAvailable());

            IReadOnlyList<PreparedOrder> draftOrders = _draft!.Orders;
            hasher.Append(draftOrders.Count);
            foreach (PreparedOrder o in OrderById(draftOrders)) AppendOrder(hasher, o);

            if (CommittedPlan != null)
            {
                hasher.Append(1);
                IReadOnlyList<PreparedOrder> committed = CommittedPlan.Orders;
                hasher.Append(committed.Count);
                foreach (PreparedOrder o in OrderById(committed)) AppendOrder(hasher, o);
                AppendResources(hasher, CommittedPlan.CommittedResources);
            }
            else hasher.Append(0);
        }

        private static IEnumerable<PreparedOrder> OrderById(IReadOnlyList<PreparedOrder> orders)
        {
            var sorted = new List<PreparedOrder>(orders);
            sorted.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));
            return sorted;
        }

        private static void AppendOrder(StateHasher hasher, PreparedOrder o)
        {
            AppendString(hasher, o.Id.Value);
            AppendString(hasher, o.Executor.Value);
            AppendString(hasher, o.Target.Value);
            hasher.Append(o.Window.Start);
            hasher.Append(o.Window.End);
            AppendResources(hasher, o.ResourceNeeds);
            var deps = new List<OrderId>(o.Dependencies);
            deps.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            hasher.Append(deps.Count);
            foreach (OrderId d in deps) AppendString(hasher, d.Value);
        }

        private static void AppendResources(StateHasher hasher, IReadOnlyDictionary<ResourceKey, long> res)
        {
            var keys = new List<ResourceKey>(res.Keys);
            keys.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            hasher.Append(keys.Count);
            foreach (ResourceKey k in keys)
            {
                AppendString(hasher, k.Value);
                hasher.Append(res[k]);
            }
        }

        /// <summary>情报态确定性哈希：世界真值 ⊕ 玩家知识，各按 subject 排序（ADR-0004）。</summary>
        private void AppendIntel(StateHasher hasher)
        {
            var truth = new List<TruthRecord>(Truth!.Records);
            truth.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
            hasher.Append(truth.Count);
            foreach (TruthRecord r in truth)
            {
                AppendString(hasher, r.Subject.Value);
                hasher.Append(r.ActualStrength);
                AppendString(hasher, r.Owner.Value);
            }

            var entries = new List<IntelKnowledgeEntry>(_playerIntel!.Project().Entries);
            entries.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
            hasher.Append(entries.Count);
            foreach (IntelKnowledgeEntry e in entries)
            {
                AppendString(hasher, e.Subject.Value);
                hasher.Append(e.KnownStrength);
                hasher.Append((int)e.Source);
                hasher.Append(e.ObservedAt.AbsoluteIndex);
            }
        }

        private static void AppendString(StateHasher hasher, string value)
        {
            hasher.Append(value.Length);
            foreach (char ch in value) hasher.Append((int)ch);
        }
    }
}
