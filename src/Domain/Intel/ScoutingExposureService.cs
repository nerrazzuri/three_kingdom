using System;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Intel
{
    /// <summary>侦察暴露判定结果（GDD_007 §Formula 5）。不可变值。</summary>
    public readonly struct ExposureResult
    {
        /// <summary>暴露概率 P(exposed)（[0,1]）。</summary>
        public FixedPoint Probability { get; }

        /// <summary>是否暴露（r &lt; P）。</summary>
        public bool Exposed { get; }

        public ExposureResult(FixedPoint probability, bool exposed)
        {
            Probability = probability;
            Exposed = exposed;
        }
    }

    /// <summary>
    /// 侦察暴露服务（GDD_007 §Formula 5 / TR-intel-002 / ADR-0004）。
    /// 由注入的确定性随机流判定暴露：P=clamp(base_expose + k_alert×alert − k_skill×exec_cap)，
    /// exposed = (r &lt; P)。<b>同流位置 + 同输入 → 同结果</b>（可重放，random.Position 可存档）。
    /// 禁隐式全局随机源（control-manifest 禁则）。
    /// </summary>
    public sealed class ScoutingExposureService
    {
        /// <summary>判定一次侦察是否暴露（消费一次随机流）。</summary>
        /// <param name="random">注入的确定性随机流。</param>
        /// <param name="alert">敌方警戒强度（[0,1]）。</param>
        /// <param name="executorCapability">执行者相关能力 exec_cap（[0,1]）。</param>
        /// <param name="config">版本化配置（暴露曲线）。</param>
        public ExposureResult Resolve(IDeterministicRandom random, FixedPoint alert, FixedPoint executorCapability, IntelConfig config)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (alert < FixedPoint.Zero || alert > FixedPoint.One) throw new ArgumentOutOfRangeException(nameof(alert), "警戒须在 [0,1]。");
            if (executorCapability < FixedPoint.Zero || executorCapability > FixedPoint.One) throw new ArgumentOutOfRangeException(nameof(executorCapability), "能力须在 [0,1]。");

            FixedPoint probability =
                (config.BaseExposure
                 + config.ExposureAlertWeight * alert
                 - config.ExposureSkillWeight * executorCapability)
                .Clamp(FixedPoint.Zero, FixedPoint.One);

            FixedPoint r = random.NextUnit();
            return new ExposureResult(probability, r < probability);
        }
    }
}
