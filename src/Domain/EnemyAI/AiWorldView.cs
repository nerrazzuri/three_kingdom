using System;
using ThreeKingdom.Domain.Environment;
using ThreeKingdom.Domain.Intel;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.EnemyAI
{
    /// <summary>
    /// 己方兵力快照（敌方 AI 视角的"我方"——即 AI 阵营自身的真实态，AI 知道自己的兵力）。
    /// 不可变。兵力为整数（ADR-0004），士气/纪律为定点 [0,1]。
    /// </summary>
    public readonly struct OwnForceSnapshot
    {
        /// <summary>己方兵力（≥0）。</summary>
        public int Force { get; }

        /// <summary>己方士气 [0,1]。</summary>
        public FixedPoint Morale { get; }

        /// <summary>己方纪律 [0,1]。</summary>
        public FixedPoint Discipline { get; }

        public OwnForceSnapshot(int force, FixedPoint morale, FixedPoint discipline)
        {
            if (force < 0) throw new ArgumentOutOfRangeException(nameof(force), "兵力不可为负。");
            Force = force;
            Morale = morale;
            Discipline = discipline;
        }
    }

    /// <summary>
    /// 目标压力（GDD_016）：AI 当前目标的紧迫度与性质，影响动作效用。
    /// 不可变。<see cref="Urgency"/> [0,1]；<see cref="MustDefend"/> 表是否守土目标（抬高坚守）。
    /// </summary>
    public readonly struct ObjectivePressure
    {
        /// <summary>目标紧迫度 [0,1]（越高越倾向决断动作）。</summary>
        public FixedPoint Urgency { get; }

        /// <summary>是否守土目标（true 抬高坚守效用、压低撤退）。</summary>
        public bool MustDefend { get; }

        public ObjectivePressure(FixedPoint urgency, bool mustDefend)
        {
            Urgency = urgency;
            MustDefend = mustDefend;
        }
    }

    /// <summary>
    /// 敌方 AI 的<b>唯一敌情入口</b>（ADR-0006 §2 反全知锁 / TR-ai-001）。
    /// <para>
    /// <b>结构级反全知</b>：构造签名<b>只</b>接受阵营知识投影（<see cref="IntelProjection"/>）+ 敌情评估
    /// （<see cref="IntelAssessment"/>）+ 己方态 + 环境 + 目标压力；<b>绝不</b>接受 MapTruth/WorldTruth/TruthRecord
    /// 等真值类型——编译期即杜绝偷看。AI 的敌情误判由"按错误情报行动 vs 真值不同"天然涌现，非约定。
    /// </para>
    /// 不可变快照（ADR-0004）。
    /// </summary>
    public sealed class AiWorldView
    {
        /// <summary>己方阵营知识投影（GDD_007 第 4 层；AI 对敌情的唯一合法认知来源）。</summary>
        public IntelProjection OwnIntel { get; }

        /// <summary>敌情评估（置信 + 估计区间；AI 感知的敌军估计，可能与真值不同）。</summary>
        public IntelAssessment EnemyAssessment { get; }

        /// <summary>己方兵力态（AI 知道自己的真实兵力）。</summary>
        public OwnForceSnapshot Own { get; }

        /// <summary>环境修正（天气/地形派生具名修正）。</summary>
        public EnvironmentModifierSet Environment { get; }

        /// <summary>目标压力。</summary>
        public ObjectivePressure Objective { get; }

        /// <summary>AI 感知的敌军兵力估计（来自情报评估的区间中心，<b>非</b>真值）。</summary>
        public int PerceivedEnemyForce => EnemyAssessment.Interval.Center;

        public AiWorldView(
            IntelProjection ownIntel, IntelAssessment enemyAssessment,
            OwnForceSnapshot own, EnvironmentModifierSet environment, ObjectivePressure objective)
        {
            OwnIntel = ownIntel ?? throw new ArgumentNullException(nameof(ownIntel));
            EnemyAssessment = enemyAssessment;
            Own = own;
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            Objective = objective;
        }
    }
}
