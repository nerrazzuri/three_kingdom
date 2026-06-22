using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Battle
{
    /// <summary>
    /// 兵法复盘识别器（GDD_010 §Formula 4/7/8/9/10 / TR-battle-002 / 强制设计锁）。
    /// <b>只在事件后</b>对复盘上下文做模式匹配并打标签——<b>不存在</b>「执行假退/伏击」等
    /// 无条件按钮，识别<b>不影响结算</b>，仅事后解释。缺任一必要条件则对应兵法不涌现、不打标签。
    /// 纯函数、确定性（按链定义顺序）。
    /// </summary>
    public sealed class TacticRecognizer
    {
        /// <summary>
        /// 识别复盘上下文中涌现的兵法（GDD_010）。仅当某链<b>全部</b>必要条件成立才纳入。
        /// </summary>
        public IReadOnlyList<RecognizedTactic> Recognize(RetrospectiveContext context, TacticChainConfig config)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var recognized = new List<RecognizedTactic>();
            foreach (TacticChainDefinition chain in config.Chains)
            {
                bool allMet = true;
                foreach (TacticCondition required in chain.Required)
                    if (!context.Has(required)) { allMet = false; break; }

                if (allMet)
                    recognized.Add(new RecognizedTactic(chain.Tag, chain.Required));
            }
            return recognized;
        }
    }
}
