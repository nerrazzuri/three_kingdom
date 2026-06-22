using System;
using System.Collections.Generic;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>因果链一步（hud.md §5 战果态：来源→修正→结果）。</summary>
    public sealed class CausalStep
    {
        /// <summary>步骤标签（来源/修正说明）。</summary>
        public string Label { get; }
        /// <summary>该步对结果的修正量（整数，权威；展示用）。</summary>
        public long Delta { get; }

        public CausalStep(string label, long delta)
        {
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("标签不可为空。", nameof(label));
            Label = label;
            Delta = delta;
        }
    }

    /// <summary>
    /// 战果因果链展示模型（hud.md §12 / P12 / ADR-0004）。
    /// 可<b>逐步展开</b>也可<b>整体跳过</b>，<b>终值不变</b>（确定性）：<see cref="FinalValue"/> 恒为
    /// 基值 + 全部步骤修正之和，与展开进度无关。不可变转移（每次 reveal/skip 产出新实例）。
    /// </summary>
    public sealed class CausalChainView
    {
        private readonly long _baseValue;
        private readonly IReadOnlyList<CausalStep> _steps;

        /// <summary>已揭示的步骤数。</summary>
        public int RevealedCount { get; }

        /// <summary>步骤总数。</summary>
        public int StepCount => _steps.Count;

        /// <summary>是否已完全揭示。</summary>
        public bool IsFullyRevealed => RevealedCount >= _steps.Count;

        private CausalChainView(long baseValue, IReadOnlyList<CausalStep> steps, int revealedCount)
        {
            _baseValue = baseValue;
            _steps = steps;
            RevealedCount = revealedCount;
        }

        /// <summary>从基值与步骤构造（初始未揭示）。</summary>
        public static CausalChainView From(long baseValue, IEnumerable<CausalStep> steps)
        {
            if (steps == null) throw new ArgumentNullException(nameof(steps));
            return new CausalChainView(baseValue, new List<CausalStep>(steps), 0);
        }

        /// <summary>终值（基值 + 全部步骤修正之和；与展开进度无关——跳过/逐步一致）。</summary>
        public long FinalValue
        {
            get
            {
                long v = _baseValue;
                foreach (var s in _steps) v += s.Delta;
                return v;
            }
        }

        /// <summary>当前已揭示部分的运行值（基值 + 前 RevealedCount 步之和）。</summary>
        public long RevealedRunningValue
        {
            get
            {
                long v = _baseValue;
                for (int i = 0; i < RevealedCount; i++) v += _steps[i].Delta;
                return v;
            }
        }

        /// <summary>揭示下一步（已全部揭示则返回自身）。</summary>
        public CausalChainView RevealNext()
            => IsFullyRevealed ? this : new CausalChainView(_baseValue, _steps, RevealedCount + 1);

        /// <summary>整体跳过：直接全部揭示（终值不变）。</summary>
        public CausalChainView SkipToEnd()
            => new CausalChainView(_baseValue, _steps, _steps.Count);
    }
}
