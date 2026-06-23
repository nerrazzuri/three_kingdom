using System.Collections.Generic;
using System.Globalization;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Council;
using ThreeKingdom.Domain.Diplomacy;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Persistence;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Session
{
    /// <summary>
    /// 一局游戏会话的 Application 聚合（ADR-0002）：编排 Domain 权威状态、对外只暴露<b>只读投影</b>。
    /// slice 阶段会话持有：世界时钟（<see cref="WorldClock"/>）、己方城市经济（GDD_004）、
    /// 敌方世界真值 + 玩家阵营知识（GDD_007 四层之真值/知识）。<b>可变状态封装于此</b>，
    /// 推进/侦察等行为为 internal——仅 <see cref="SessionService"/> 调用；Presentation 永不直接持有或修改
    /// 内部 Domain 对象（红线：UI/MonoBehaviour 不碰可变 Domain 状态），只经服务拿不可变投影。
    /// 确定性：同一推进/侦察序列产生同一状态（ADR-0004）。
    /// </summary>
    public sealed class GameSession
    {
        private readonly SliceScenario _scenario;
        private readonly WorldClock _clock;

        // 城市（己方账本）
        private readonly CityDaySettlementService _citySettlement = new CityDaySettlementService();
        private CityEconomyState _city;
        private long _logistics;
        private long _lastDayShortage;
        private bool _highUnrestRisk;

        // 情报（敌情）
        private readonly IntelService _intel = new IntelService();
        private readonly WorldTruthLedger _truth = new WorldTruthLedger();
        private readonly FactionIntel _playerIntel;

        // 军议（军师条件化建议，GDD_008）——瞬时，不入存档（读档后重新召开）
        private readonly WarCouncilService _council = new WarCouncilService();
        private CouncilAdviceSet? _lastAdvice; // null = 未召开

        // 侦察（派出→在途→返报；非即时暴露，GDD_007）
        private long _pendingScoutIndex = -1; // 侦察队返报的绝对时段索引；-1 = 无在途

        // 袭扰敌补给（断粮疲敌，第二取胜路线；派出→在途→见效）
        private DeterministicRandom _raidRng;
        private long _pendingRaidIndex = -1;  // 袭扰队见效的绝对时段索引；-1 = 无在途
        private bool _lastRaidExposed;        // 最近一次「已结算」袭扰是否暴露
        private bool _hasRaidResult;          // 是否有可展示的已结算袭扰结果
        private bool _lastDispatchPerformed;  // 最近一次派出操作是否真正派出（用于 UI 反馈）

        // 假退伏击（第三取胜路线；一次性高风险决战赌注）
        private DeterministicRandom _ambushRng;
        private bool _ambushUsed;
        private long _pendingAmbushIndex = -1; // 伏击发动的绝对时段索引；-1 = 无在途
        private bool _ambushResolved;
        private bool _ambushSucceeded;

        // 外交（求粮受控入口，GDD_012 §8）
        private readonly DiplomacyService _diplomacy = new DiplomacyService();
        private DeterministicRandom _diploRng;
        private bool _diplomacyUsed;
        private DiplomaticResponse _diploResponse;
        private bool _diploFulfilledRoll;
        private long _pendingDeliveryIndex = -1; // -1 = 无在途交付
        private long _pendingDeliveryAmount;
        private long _diploDeliveredAmount;       // >0 = 援粮已抵达入城

        internal GameSession(SliceScenario scenario)
        {
            _scenario = scenario;
            _clock = new WorldClock(scenario.Start);

            _city = scenario.InitialCity;
            _logistics = scenario.InitialLogistics;

            _playerIntel = new FactionIntel(scenario.PlayerFaction);
            _truth.Set(new TruthRecord(scenario.EnemySubject, scenario.EnemyInitialStrength, scenario.EnemyFaction));

            _diploRng = new DeterministicRandom(scenario.DiplomacyRngSeed);
            _raidRng = new DeterministicRandom(scenario.RaidRngSeed);
            _ambushRng = new DeterministicRandom(scenario.AmbushRngSeed);
        }

        /// <summary>
        /// 从存档恢复会话（<see cref="SaveMapper"/> 用）：以显式状态重建时钟/城市/敌方真值/玩家知识。
        /// 知识经合成报告并入（与正常侦察同路径），保证恢复后行为与存档前一致。
        /// </summary>
        internal GameSession(
            SliceScenario scenario,
            WorldTime time,
            CityEconomyState city,
            long logistics,
            long lastDayShortage,
            bool highUnrestRisk,
            int enemyTruthStrength,
            bool hasKnownEnemy,
            int knownEnemyStrength,
            IntelSource knownEnemySource,
            WorldTime knownEnemyObservedAt)
        {
            _scenario = scenario;
            _clock = new WorldClock(time);
            _city = city;
            _logistics = logistics;
            _lastDayShortage = lastDayShortage;
            _highUnrestRisk = highUnrestRisk;

            _playerIntel = new FactionIntel(scenario.PlayerFaction);
            _truth.Set(new TruthRecord(scenario.EnemySubject, enemyTruthStrength, scenario.EnemyFaction));
            if (hasKnownEnemy)
                _playerIntel.ApplyReport(new IntelReport(
                    scenario.EnemySubject, scenario.PlayerFaction, knownEnemyStrength, knownEnemySource, knownEnemyObservedAt));

            _diploRng = new DeterministicRandom(scenario.DiplomacyRngSeed); // 位置由 RestoreDiplomacy 覆盖
            _raidRng = new DeterministicRandom(scenario.RaidRngSeed);       // 位置由 RestoreRaid 覆盖
            _ambushRng = new DeterministicRandom(scenario.AmbushRngSeed);   // 位置由 RestoreAmbush 覆盖
        }

        /// <summary>当前权威世界时间（只读）。</summary>
        public WorldTime CurrentTime => _clock.Current;

        // ---- 行为（internal：仅经 SessionService）----

        /// <summary>
        /// 推进时钟 <paramref name="segments"/> 个时段；每跨入新一日触发城市日界结算 + 敌方真值漂移。
        /// 返回本次推进穿越的日界数（用于跨日提示）。
        /// </summary>
        internal int Advance(int segments)
        {
            AdvanceResult result = _clock.Apply(new AdvanceTimeCommand(segments));

            foreach (var _ in result.DayBoundaries)
            {
                // 己方城市日界产耗结算（GDD_004，确定性纯函数；数值来自注入配置）。
                CitySettlementResult settled = _citySettlement.Settle(
                    _city, _logistics, _scenario.CityConfig, _scenario.PopulationPressure);
                _city = settled.EndState;
                _logistics = settled.EndLogisticsHolding;
                _lastDayShortage = settled.Shortage;
                _highUnrestRisk = settled.HighUnrestRisk;

                // 敌方真值随日漂移（slice 场景设定：每日增援，数值来自配置）——
                // 玩家若不再侦察，所持情报随时间过时（GDD_007 时效）。
                int grown = _truth.Get(_scenario.EnemySubject).ActualStrength + _scenario.EnemyReinforcePerDay;
                _truth.SetStrength(_scenario.EnemySubject, grown);
            }

            // 外交援粮到达：到达时段后入城粮草（延迟交付，非点击即到）。
            if (_pendingDeliveryIndex >= 0 && _clock.Current.AbsoluteIndex >= _pendingDeliveryIndex)
            {
                _city = _city.With(stock: checked(_city.Stock + _pendingDeliveryAmount));
                _diploDeliveredAmount = _pendingDeliveryAmount;
                _pendingDeliveryIndex = -1;
                _pendingDeliveryAmount = 0;
            }

            ResolveScout();
            ResolveRaid();
            ResolveAmbush();

            return result.DayBoundaries.Count;
        }

        /// <summary>侦察队返报：抵达时段后读真值生成观察→报告→并入知识（GDD_007，观察时间=返报时刻）。</summary>
        private void ResolveScout()
        {
            if (_pendingScoutIndex < 0 || _clock.Current.AbsoluteIndex < _pendingScoutIndex) return;
            Observation observation = _intel.Observe(_truth, _scenario.EnemySubject, _scenario.PlayerFaction, _clock.Current);
            IntelReport report = _intel.ToReport(observation, _scenario.PlayerFaction, IntelSource.Scouting);
            _playerIntel.ApplyReport(report);
            _pendingScoutIndex = -1;
        }

        /// <summary>
        /// 伏击发动：抵达发动时刻后判定（消费伏击随机流）。成立 = 敌将性烈（非战斗前提）且未暴露
        /// （r ≥ 失败概率 base−skill×cap）。得手则重创敌真实兵力 + 复原工事 + 提振民心；
        /// 失败则示弱失策、重挫民心、工事维持降低（高风险高回报）。
        /// </summary>
        private void ResolveAmbush()
        {
            if (_pendingAmbushIndex < 0 || _clock.Current.AbsoluteIndex < _pendingAmbushIndex) return;
            _pendingAmbushIndex = -1;
            _ambushResolved = true;

            FixedPoint failProb = (_scenario.AmbushExposureBase - _scenario.AmbushSkillWeight * _scenario.AmbushCapability)
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint r = _ambushRng.NextUnit();
            // 非战斗前提：敌将不性烈则诱敌必败（条件不完整不自动补齐）。
            _ambushSucceeded = _scenario.EnemyGeneralRash && r >= failProb;

            if (_ambushSucceeded)
            {
                int reduced = _truth.Get(_scenario.EnemySubject).ActualStrength - _scenario.AmbushSuccessDamage;
                if (reduced < 0) reduced = 0;
                _truth.SetStrength(_scenario.EnemySubject, reduced);
                int fort = System.Math.Min(_city.FortificationMax, _city.FortificationCurrent + _scenario.AmbushFortCost); // 复原示弱开口
                int morale = System.Math.Min(_scenario.CityConfig.CivMoraleMax, _city.CivMorale + _scenario.AmbushSuccessMoraleBonus);
                _city = _city.With(fortificationCurrent: fort, civMorale: morale);
            }
            else
            {
                int morale = _city.CivMorale - _scenario.AmbushFailMoralePenalty;
                if (morale < 0) morale = 0;
                _city = _city.With(civMorale: morale); // 工事维持降低（开口未复原），民心重挫
            }
        }

        /// <summary>袭扰见效：抵达时段后判定暴露（消费袭扰随机流）。未暴露削敌真实兵力，暴露损民心。</summary>
        private void ResolveRaid()
        {
            if (_pendingRaidIndex < 0 || _clock.Current.AbsoluteIndex < _pendingRaidIndex) return;
            _pendingRaidIndex = -1;
            _hasRaidResult = true;

            FixedPoint prob = (_scenario.RaidExposureBase - _scenario.RaidSkillWeight * _scenario.RaidCapability)
                .Clamp(FixedPoint.Zero, FixedPoint.One);
            FixedPoint r = _raidRng.NextUnit();
            _lastRaidExposed = r < prob;

            if (_lastRaidExposed)
            {
                int morale = _city.CivMorale - _scenario.RaidExposureMoralePenalty;
                if (morale < 0) morale = 0;
                _city = _city.With(civMorale: morale);
            }
            else
            {
                int reduced = _truth.Get(_scenario.EnemySubject).ActualStrength - _scenario.RaidEnemyDamage;
                if (reduced < 0) reduced = 0;
                _truth.SetStrength(_scenario.EnemySubject, reduced);
            }
        }

        /// <summary>
        /// 求粮（受控外交入口，一局一次）：评估响应 + 兑现判定（消费外交随机流）。接受且兑现则安排
        /// <see cref="SliceScenario.DiplomacyConfig"/> 交付时段后入城（延迟、可背约、代价已付不返还，GDD_012 §8）。
        /// </summary>
        internal void RequestAid()
        {
            if (_diplomacyUsed) return; // 受控入口：一局一次
            _diplomacyUsed = true;

            var request = new DiplomaticRequest(
                DiplomaticRequestType.Supply, _scenario.DiplomacyPower,
                _scenario.DiplomacyPledgeCost, _scenario.DiplomacySupplyAmount,
                _scenario.DiplomacyStanding, _scenario.DiplomacyPressure);

            DiplomaticPledge pledge = _diplomacy.Evaluate(request, _clock.Current, _scenario.DiplomacyConfig);
            _diploResponse = pledge.Response;

            DiplomaticOutcome outcome = _diplomacy.Resolve(pledge, _diploRng, true, false, _scenario.DiplomacyConfig);
            _diploFulfilledRoll = outcome.Fulfilled;
            if (outcome.Fulfilled && pledge.ArrivalTime.HasValue)
            {
                _pendingDeliveryIndex = pledge.ArrivalTime.Value.AbsoluteIndex;
                _pendingDeliveryAmount = pledge.DeliveredAmount;
            }
        }

        /// <summary>是否有侦察队在途（已派出未返报）。</summary>
        internal bool ScoutInFlight => _pendingScoutIndex >= 0;

        /// <summary>当前是否可派出侦察（无在途侦察 + 局未终）。</summary>
        internal bool CanDispatchScout => Outcome == GameOutcome.Ongoing && !ScoutInFlight;

        /// <summary>侦察队预计返报的世界日（0 基；-1 = 无在途）。</summary>
        internal int ScoutArrivalDay => _pendingScoutIndex < 0 ? -1 : (int)(_pendingScoutIndex / WorldTime.SegmentsPerDay);

        /// <summary>
        /// 派出侦察（GDD_007）：非即时——侦察队往返 <see cref="SliceScenario.ScoutLeadSegments"/> 个时段后，
        /// 在 <see cref="Advance"/> 抵达返报时刻时读真值生成报告并入知识。在途期间不可重复派出。
        /// </summary>
        internal void DispatchScout()
        {
            if (!CanDispatchScout) { _lastDispatchPerformed = false; return; }
            _pendingScoutIndex = _clock.Current.Advance(_scenario.ScoutLeadSegments).AbsoluteIndex;
            _lastDispatchPerformed = true;
        }

        /// <summary>是否有袭扰队在途（已派出未见效）。</summary>
        internal bool RaidInFlight => _pendingRaidIndex >= 0;

        /// <summary>当前是否可派出袭扰（无在途袭扰 + 粮草足够 + 局未终）。</summary>
        internal bool CanDispatchRaid =>
            Outcome == GameOutcome.Ongoing && !RaidInFlight && _city.Stock - _scenario.RaidStockCost >= 0;

        /// <summary>袭扰队预计见效的世界日（0 基；-1 = 无在途）。</summary>
        internal int RaidArrivalDay => _pendingRaidIndex < 0 ? -1 : (int)(_pendingRaidIndex / WorldTime.SegmentsPerDay);

        /// <summary>
        /// 派出袭扰（断粮疲敌）：派出即兑付粮草代价（投送辎重/兵力），袭扰队往返
        /// <see cref="SliceScenario.RaidLeadSegments"/> 个时段后于 <see cref="Advance"/> 见效（暴露判定）。
        /// 非即时；在途期间不可重复派出。
        /// </summary>
        internal void DispatchRaid()
        {
            if (!CanDispatchRaid) { _lastDispatchPerformed = false; return; }
            _city = _city.With(stock: checked(_city.Stock - _scenario.RaidStockCost)); // 派出即兑付代价
            _pendingRaidIndex = _clock.Current.Advance(_scenario.RaidLeadSegments).AbsoluteIndex;
            _lastDispatchPerformed = true;
        }

        internal bool LastDispatchPerformed => _lastDispatchPerformed;
        internal bool HasRaidResult => _hasRaidResult;
        internal bool LastRaidExposed => _lastRaidExposed;

        /// <summary>是否有伏击在途（已设伏诱敌未发动）。</summary>
        internal bool AmbushInFlight => _pendingAmbushIndex >= 0;

        /// <summary>是否可设伏诱敌（一局一次 + 无在途 + 工事足够示弱 + 局未终）。</summary>
        internal bool CanDispatchAmbush =>
            Outcome == GameOutcome.Ongoing && !_ambushUsed && !AmbushInFlight
            && _city.FortificationCurrent - _scenario.AmbushFortCost >= 0;

        /// <summary>伏击预计发动的世界日（0 基；-1 = 无在途）。</summary>
        internal int AmbushArrivalDay => _pendingAmbushIndex < 0 ? -1 : (int)(_pendingAmbushIndex / WorldTime.SegmentsPerDay);

        internal bool AmbushUsed => _ambushUsed;
        internal bool AmbushResolved => _ambushResolved;
        internal bool AmbushSucceeded => _ambushSucceeded;

        /// <summary>
        /// 设伏诱敌（假退伏击，一局一次）：示弱即降工事（开口诱敌），伏击队往返
        /// <see cref="SliceScenario.AmbushLeadSegments"/> 个时段后于 <see cref="Advance"/> 发动判定。非即时；
        /// 成立须敌将性烈（非战斗前提，花名册可见）。
        /// </summary>
        internal void DispatchAmbush()
        {
            if (!CanDispatchAmbush) { _lastDispatchPerformed = false; return; }
            _ambushUsed = true;
            _city = _city.With(fortificationCurrent: _city.FortificationCurrent - _scenario.AmbushFortCost); // 示弱开口
            _pendingAmbushIndex = _clock.Current.Advance(_scenario.AmbushLeadSegments).AbsoluteIndex;
            _lastDispatchPerformed = true;
        }

        /// <summary>
        /// 召开军议（GDD_008）：读当前只读知识投影，整理为条件化建议集（并列、无最优解）。
        /// 建议绑定当前知识快照 ID；之后侦察改变知识则建议过时（不静默更新）。
        /// </summary>
        internal void Convene()
        {
            var confidences = new Dictionary<IntelSubjectId, FixedPoint>();
            IntelProjection knowledge = _playerIntel.Project();
            foreach (var entry in knowledge.Entries)
                confidences[entry.Subject] = _scenario.KnownClaimConfidence;

            _lastAdvice = _council.Convene(
                CurrentKnowledgeSnapshotId, knowledge, confidences,
                _scenario.Advisor, _scenario.AdviceTemplates, _scenario.CouncilConfig);
        }

        /// <summary>最近一次军议建议集（null = 未召开）。</summary>
        internal CouncilAdviceSet? LastAdvice => _lastAdvice;

        /// <summary>
        /// 当前知识快照 ID（GDD_008 §Formula 4）：由已知条目（主题+估计值+观察时间）确定性派生。
        /// 侦察改变知识 → 快照变 → 已召开建议被标过时。
        /// </summary>
        internal KnowledgeSnapshotId CurrentKnowledgeSnapshotId
        {
            get
            {
                var entries = new List<IntelKnowledgeEntry>(_playerIntel.Project().Entries);
                entries.Sort((a, b) => string.CompareOrdinal(a.Subject.Value, b.Subject.Value));
                var sb = new System.Text.StringBuilder("k");
                foreach (var e in entries)
                    sb.Append('|').Append(e.Subject.Value).Append(':')
                      .Append(e.KnownStrength.ToString(CultureInfo.InvariantCulture)).Append('@')
                      .Append(e.ObservedAt.AbsoluteIndex.ToString(CultureInfo.InvariantCulture));
                return new KnowledgeSnapshotId(sb.ToString());
            }
        }

        // ---- 只读快照（internal：供 SessionService 构造投影）----

        internal CityEconomyState City => _city;
        internal long Logistics => _logistics;
        internal long LastDayShortage => _lastDayShortage;
        internal bool HighUnrestRisk => _highUnrestRisk;

        /// <summary>敌方真值兵力（仅供存档映射读取；绝不进入显示投影）。</summary>
        internal int EnemyTruthStrength => _truth.Get(_scenario.EnemySubject).ActualStrength;

        /// <summary>会话场景（供存档映射读取主题/阵营等稳定标识）。</summary>
        internal SliceScenario Scenario => _scenario;

        /// <summary>玩家阵营知识的只读投影（GDD_007；结构上不含真值）。</summary>
        internal IntelProjection IntelProjection => _playerIntel.Project();

        /// <summary>
        /// 当前胜负态：民心崩溃（≤0）即失城为败（优先）；否则<b>两条取胜路线</b>任一达成即胜——
        /// 守至援军抵达日（守城待变）或敌兵力降至退兵阈值（断粮疲敌）；其余进行中。
        /// </summary>
        internal GameOutcome Outcome
        {
            get
            {
                if (_city.CivMorale <= 0) return GameOutcome.Defeat;
                if (EnemyWithdrew || _clock.Current.Day >= _scenario.ReliefDay) return GameOutcome.Victory;
                return GameOutcome.Ongoing;
            }
        }

        /// <summary>敌兵力是否已降至退兵阈值（断粮疲敌取胜条件；读真值，仅系统判定，不进显示投影）。</summary>
        private bool EnemyWithdrew => _truth.Get(_scenario.EnemySubject).ActualStrength <= _scenario.EnemyWithdrawThreshold;

        /// <summary>败因（仅 <see cref="GameOutcome.Defeat"/> 非空）。</summary>
        internal string DefeatReason => _city.CivMorale <= 0 ? "民心崩溃，城池陷落。" : string.Empty;

        /// <summary>胜利方式（仅 <see cref="GameOutcome.Victory"/> 非空）：区分伏击大破 / 断粮退兵 / 守至援军。</summary>
        internal string VictoryReason =>
            Outcome != GameOutcome.Victory ? string.Empty
            : (_ambushSucceeded && EnemyWithdrew) ? "假退伏击得手，敌军溃乱败退——大破之。"
            : EnemyWithdrew ? "敌军粮道断绝、师老兵疲，已退兵——汜水关解围。"
            : "已守至援军抵达——汜水关守住了。";

        // ---- 外交只读快照 + 存档（internal）----

        internal bool DiplomacyUsed => _diplomacyUsed;
        internal DiplomaticResponse DiplomacyResponse => _diploResponse;
        internal bool DiplomacyFulfilledRoll => _diploFulfilledRoll;
        internal long PendingDeliveryIndex => _pendingDeliveryIndex;
        internal long PendingDeliveryAmount => _pendingDeliveryAmount;
        internal long DiplomacyDeliveredAmount => _diploDeliveredAmount;

        /// <summary>抓取外交随机流位置（存档；读档据 (seed,position) 重建续判，不重抽）。</summary>
        internal RngStreamState CaptureDiplomacyRng() => RngStreamState.Capture("diplomacy", _diploRng);

        /// <summary>抓取袭扰随机流位置（存档）。</summary>
        internal RngStreamState CaptureRaidRng() => RngStreamState.Capture("raid", _raidRng);

        /// <summary>抓取伏击随机流位置（存档）。</summary>
        internal RngStreamState CaptureAmbushRng() => RngStreamState.Capture("ambush", _ambushRng);

        internal long PendingScoutIndex => _pendingScoutIndex;
        internal long PendingRaidIndex => _pendingRaidIndex;
        internal long PendingAmbushIndex => _pendingAmbushIndex;

        /// <summary>恢复伏击状态（<see cref="SaveMapper"/> 用）：一局一次标记 + 在途发动时刻 + 已结算结果 + 随机流位置。</summary>
        internal void RestoreAmbush(bool used, long pendingIndex, bool resolved, bool succeeded, RngStreamState rng)
        {
            _ambushUsed = used;
            _pendingAmbushIndex = pendingIndex;
            _ambushResolved = resolved;
            _ambushSucceeded = succeeded;
            _ambushRng = rng.Rebuild();
        }

        /// <summary>恢复侦察在途（<see cref="SaveMapper"/> 用）。</summary>
        internal void RestoreScout(long pendingScoutIndex) => _pendingScoutIndex = pendingScoutIndex;

        /// <summary>恢复袭扰状态（<see cref="SaveMapper"/> 用）：在途见效时刻 + 已结算结果 + 随机流位置。</summary>
        internal void RestoreRaid(long pendingRaidIndex, bool hasResult, bool lastExposed, RngStreamState rng)
        {
            _pendingRaidIndex = pendingRaidIndex;
            _hasRaidResult = hasResult;
            _lastRaidExposed = lastExposed;
            _lastDispatchPerformed = false;
            _raidRng = rng.Rebuild();
        }

        /// <summary>恢复外交状态（<see cref="SaveMapper"/> 用）：含在途交付与随机流位置。</summary>
        internal void RestoreDiplomacy(
            bool used, DiplomaticResponse response, bool fulfilledRoll,
            long pendingIndex, long pendingAmount, long deliveredAmount, RngStreamState rng)
        {
            _diplomacyUsed = used;
            _diploResponse = response;
            _diploFulfilledRoll = fulfilledRoll;
            _pendingDeliveryIndex = pendingIndex;
            _pendingDeliveryAmount = pendingAmount;
            _diploDeliveredAmount = deliveredAmount;
            _diploRng = rng.Rebuild();
        }
    }
}
