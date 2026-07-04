using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>
    /// 敌方区域AI（GDD_021 R6 / ADR-0013，首次落地 GDD_016）：每回合为 AI 阵营各支队择区域动作——
    /// 按<b>反全知视野</b>（<see cref="AiWorldView"/>）做<b>数据驱动效用评分</b> + <b>种子化整数加权选择</b>
    /// （realizes ADR-0006 种子softmax意图，无 float、确定性可复现）+ <b>渐进记忆</b>。
    /// <b>受与玩家同一命令契约</b>（经 <see cref="ZoneCommandService"/>：相邻+在途，<b>不作弊</b>，ADR-0013 D3）。
    /// LLM 隔离：本服务无任何 LLM 依赖（结算纯确定性）。
    /// </summary>
    public sealed class EnemyZoneAiService
    {
        private readonly ZoneCommandService _commands = new ZoneCommandService();

        /// <summary>为 <paramref name="aiSide"/> 决策并应用本回合区域动作；返回新态（含更新的记忆）。</summary>
        public ZoneBattleState Decide(ZoneBattleState state, BattleSide aiSide, ZoneBattleConfig config, EnemyAiConfig ai)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (ai == null) throw new ArgumentNullException(nameof(ai));

            AiWorldView view = AiWorldView.BuildFor(state, aiSide);       // 反全知投影（决策只读此）
            var rng = new DeterministicRandom(PerRoundSeed(state, aiSide));

            ZoneBattleState working = state;
            // 快照 AI 支队（规范序）；逐支队独立决策，决策据同一初始投影（确定性）。
            var aiDetachments = new List<Detachment>(state.DetachmentsOf(aiSide));
            foreach (Detachment det in aiDetachments)
            {
                if (det.InTransit || det.IsBroken) continue;

                // 候选动作：留守 + 调往各相邻区。
                var targets = new List<ZoneId> { det.Location };
                foreach (ZoneId n in state.Field.Neighbors(det.Location)) targets.Add(n);

                // 评分。
                var scores = new int[targets.Count];
                int min = int.MaxValue;
                for (int i = 0; i < targets.Count; i++)
                {
                    bool isMove = targets[i] != det.Location;
                    scores[i] = Score(targets[i], isMove, view, state.Memory, ai);
                    if (scores[i] < min) min = scores[i];
                }

                // 整数加权（权重 = (score - min + 1)^sharpness ≥ 1；高效用更可能，非必然）。
                var weights = new long[targets.Count];
                long total = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    weights[i] = Pow(scores[i] - min + 1, ai.Sharpness);
                    total += weights[i];
                }

                int pick = WeightedPick(rng, weights, total);
                if (targets[pick] != det.Location)
                {
                    ZoneCommandResult r = _commands.MoveDetachment(working, aiSide, det.Id, targets[pick]);
                    if (r.Applied) working = r.State;   // 命令契约把关；失败则留守（不作弊、不强改）
                }
            }

            // 更新记忆（只记可见敌情，反全知）。
            return working.WithMemory(new EnemyAiMemory(view.VisibleEnemyMap));
        }

        private static int Score(ZoneId target, bool isMove, AiWorldView view, EnemyAiMemory memory, EnemyAiConfig ai)
        {
            int visibleEnemy = view.VisibleEnemyIn(target);
            int score = ai.ValueOf(target);
            score += (visibleEnemy / 50) * ai.ThreatWeight;                     // 向受威胁区集中
            if (visibleEnemy > view.OwnIn(target)) score += ai.DeficitBonus;    // 增援劣势区
            if (visibleEnemy > memory.LastVisible(target)) score += ai.TrendBonus; // 玩家增兵趋势
            if (isMove) score -= ai.MoveCost;                                   // 调动代价
            return score;
        }

        private static int WeightedPick(DeterministicRandom rng, long[] weights, long total)
        {
            if (total <= 0) return 0;
            long r = (long)(rng.NextBits() % (ulong)total);
            long acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (r < acc) return i;
            }
            return weights.Length - 1;
        }

        /// <summary>饱和幂（超上限即封顶，避免溢出；保留相对优势且确定性）。</summary>
        private static long Pow(long b, int e)
        {
            const long Cap = long.MaxValue / 1024;   // 留余量供候选求和不溢出
            long r = 1;
            for (int i = 0; i < e; i++)
            {
                if (b != 0 && r > Cap / b) return Cap;
                r *= b;
            }
            return r;
        }

        /// <summary>回合种子（ADR-0013 D5：seed=Hash(worldSeed, round, side)）。</summary>
        private static ulong PerRoundSeed(ZoneBattleState state, BattleSide aiSide)
        {
            var h = new StateHasher();
            h.Append(state.Seed).Append(state.Clock.Round).Append((int)aiSide);
            return h.ToHash().Value;
        }
    }
}
