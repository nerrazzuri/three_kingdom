using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 敌AI世界观投影（GDD_021 R6/R9 / ADR-0013 D2 反全知锁）：AI 只见<b>自身</b>支队 + <b>可见</b>敌情。
    /// 隐蔽伏兵（某区蓄势中 AmbushCharge&gt;0 且 AI 不在该区）<b>不可见</b>——AI 不会预判未知伏兵。
    /// <b>结构上拒绝真值</b>：本视图由构造时过滤，AI 决策只读本视图，不读 <see cref="ZoneBattleState"/> 敌方真实兵力。
    /// </summary>
    public sealed class AiWorldView
    {
        private readonly Dictionary<string, int> _visibleEnemy;
        private readonly Dictionary<string, int> _own;

        /// <summary>AI 阵营。</summary>
        public BattleSide AiSide { get; }

        private AiWorldView(BattleSide aiSide, Dictionary<string, int> visibleEnemy, Dictionary<string, int> own)
        {
            AiSide = aiSide;
            _visibleEnemy = visibleEnemy;
            _own = own;
        }

        /// <summary>某区可见敌兵力（隐蔽伏兵排除）。</summary>
        public int VisibleEnemyIn(ZoneId zone) => _visibleEnemy.TryGetValue(zone.Value ?? "", out int v) ? v : 0;

        /// <summary>某区己方在场兵力。</summary>
        public int OwnIn(ZoneId zone) => _own.TryGetValue(zone.Value ?? "", out int v) ? v : 0;

        /// <summary>可见敌情映射（供更新记忆）。</summary>
        public IReadOnlyDictionary<string, int> VisibleEnemyMap => _visibleEnemy;

        /// <summary>从战斗态为 <paramref name="aiSide"/> 构造反全知投影。</summary>
        public static AiWorldView BuildFor(ZoneBattleState state, BattleSide aiSide)
        {
            var visible = new Dictionary<string, int>(StringComparer.Ordinal);
            var own = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (Zone z in state.Field.Zones)
            {
                int ownStrength = 0, enemyStrength = 0;
                bool aiPresent = false;
                foreach (Detachment d in state.DetachmentsIn(z.Id))
                {
                    if (d.InTransit || d.IsBroken) continue;
                    if (d.Side == aiSide) { ownStrength += d.Strength; aiPresent = true; }
                    else enemyStrength += d.Strength;
                }
                // 反全知：该区蓄势中且 AI 不在场 → 敌隐蔽，不可见。
                bool hidden = state.EngagementOf(z.Id).AmbushCharge > 0 && !aiPresent;
                own[z.Id.Value] = ownStrength;
                visible[z.Id.Value] = hidden ? 0 : enemyStrength;
            }
            return new AiWorldView(aiSide, visible, own);
        }
    }

    /// <summary>敌AI效用权重配置（GDD_021 §11 / ADR-0013 D4，数据驱动，整数）。不可变。</summary>
    public sealed class EnemyAiConfig
    {
        /// <summary>各区战略价值（key=zone id；缺省 <see cref="DefaultZoneValue"/>）。</summary>
        public IReadOnlyDictionary<string, int> ZoneValue { get; }
        /// <summary>缺省区域价值。</summary>
        public int DefaultZoneValue { get; }
        /// <summary>威胁权重（可见敌兵力每 50 → +此值，向受威胁区集中）。</summary>
        public int ThreatWeight { get; }
        /// <summary>兵力劣势加成（可见敌 &gt; 己方 → +此值，增援薄弱区）。</summary>
        public int DeficitBonus { get; }
        /// <summary>意图趋势加成（可见敌较上回合增加 → +此值）。</summary>
        public int TrendBonus { get; }
        /// <summary>调动代价（移动候选 −此值，偏好稳守）。</summary>
        public int MoveCost { get; }
        /// <summary>选择锐度（≥1；权重指数，高=更倾向高效用，realizes 种子softmax 温度反比）。</summary>
        public int Sharpness { get; }
        /// <summary>攻方进攻目标（正面）推进加成（攻方角色向破城目标施压）。</summary>
        public int ObjectivePush { get; }
        /// <summary>优势加成（该区己方 &gt; 可见敌 → +此值：攻方乘虚、守方巩固）。</summary>
        public int OpportunityBonus { get; }
        /// <summary>低士气退避阈值（支队士气低于此则偏好向低威胁区退避保全）。</summary>
        public FixedPoint RetreatMoraleThreshold { get; }
        /// <summary>退避加成（低士气支队移向低威胁区 +此值）。</summary>
        public int PreserveBonus { get; }

        public EnemyAiConfig(
            IReadOnlyDictionary<string, int>? zoneValue, int defaultZoneValue, int threatWeight,
            int deficitBonus, int trendBonus, int moveCost, int sharpness,
            int objectivePush = 50, int opportunityBonus = 30,
            FixedPoint? retreatMoraleThreshold = null, int preserveBonus = 35)
        {
            if (sharpness < 1) throw new ArgumentOutOfRangeException(nameof(sharpness));
            var zv = new Dictionary<string, int>(StringComparer.Ordinal);
            if (zoneValue != null) foreach (KeyValuePair<string, int> kv in zoneValue) zv[kv.Key] = kv.Value;
            ZoneValue = zv;
            DefaultZoneValue = defaultZoneValue;
            ThreatWeight = threatWeight;
            DeficitBonus = deficitBonus;
            TrendBonus = trendBonus;
            MoveCost = moveCost;
            Sharpness = sharpness;
            ObjectivePush = objectivePush;
            OpportunityBonus = opportunityBonus;
            RetreatMoraleThreshold = retreatMoraleThreshold ?? FixedPoint.FromFraction(3, 10);
            PreserveBonus = preserveBonus;
        }

        /// <summary>某区战略价值。</summary>
        public int ValueOf(ZoneId zone) => ZoneValue.TryGetValue(zone.Value ?? "", out int v) ? v : DefaultZoneValue;

        /// <summary>默认（正面/关城价值最高——守城之要；敌粮道次之）。</summary>
        public static EnemyAiConfig Default { get; } = new EnemyAiConfig(
            zoneValue: new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [BattleField.Front.Value] = 100,
                [BattleField.Supply.Value] = 60,
                [BattleField.Flank.Value] = 40,
                [BattleField.Cover.Value] = 30,
                [BattleField.Reserve.Value] = 20,
            },
            defaultZoneValue: 30, threatWeight: 20, deficitBonus: 40, trendBonus: 25, moveCost: 15, sharpness: 2);
    }
}
