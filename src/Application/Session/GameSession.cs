using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Intel;
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

        internal GameSession(SliceScenario scenario)
        {
            _scenario = scenario;
            _clock = new WorldClock(scenario.Start);

            _city = scenario.InitialCity;
            _logistics = scenario.InitialLogistics;

            _playerIntel = new FactionIntel(scenario.PlayerFaction);
            _truth.Set(new TruthRecord(scenario.EnemySubject, scenario.EnemyInitialStrength, scenario.EnemyFaction));
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

            return result.DayBoundaries.Count;
        }

        /// <summary>
        /// 侦察敌方：读世界真值生成观察→报告→并入玩家阵营知识（GDD_007 单向四层流转，绝不写回真值）。
        /// 报告以当前世界时间为观察基准（时效）。
        /// </summary>
        internal void Scout()
        {
            Observation observation = _intel.Observe(_truth, _scenario.EnemySubject, _scenario.PlayerFaction, _clock.Current);
            IntelReport report = _intel.ToReport(observation, _scenario.PlayerFaction, IntelSource.Scouting);
            _playerIntel.ApplyReport(report);
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
    }
}
