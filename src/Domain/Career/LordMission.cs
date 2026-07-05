using System.Collections.Generic;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Career
{
    /// <summary>君主任务类型（GDD_014 §Main Rules）：君主主动派发，完成累积功绩通往晋升，逾期/失败损名望。</summary>
    public enum MissionType
    {
        /// <summary>讨伐：攻取指定敌城（与出征授权同轴）。</summary>
        Subjugate = 0,
        /// <summary>守土：守住治所至期限（失守即败）。</summary>
        Defend = 1,
        /// <summary>献纳：向君主上缴军粮若干。</summary>
        Tribute = 2,
    }

    /// <summary>君主任务进度。</summary>
    public enum MissionProgress { Pending = 0, Completed = 1, Failed = 2 }

    /// <summary>评估任务进度所需的当前情势（由运行期从世界/城态取）。</summary>
    public readonly struct MissionContext
    {
        /// <summary>讨伐：已占目标城 / 守土：仍据本城。</summary>
        public bool OwnsTargetCity { get; }
        /// <summary>献纳：累计已上缴军粮。</summary>
        public long GrainDelivered { get; }

        public MissionContext(bool ownsTargetCity, long grainDelivered)
        {
            OwnsTargetCity = ownsTargetCity;
            GrainDelivered = grainDelivered;
        }
    }

    /// <summary>一道君主任务（GDD_014，不可变）：类型 + 目标 + 期限（公元年）+ 奖惩。</summary>
    public sealed class LordMission
    {
        public string Id { get; }
        public MissionType Type { get; }
        /// <summary>讨伐目标城 / 守土之城；献纳则为 null（无目标城）。</summary>
        public CityId? TargetCity { get; }
        /// <summary>献纳所需军粮（Tribute）。</summary>
        public long TributeGrain { get; }
        public int IssuedYear { get; }
        public int DeadlineYear { get; }
        public int RewardMerit { get; }
        public int RewardRenown { get; }
        /// <summary>失败/逾期损名望。</summary>
        public int PenaltyRenown { get; }

        public LordMission(string id, MissionType type, CityId? targetCity, long tributeGrain,
            int issuedYear, int deadlineYear, int rewardMerit, int rewardRenown, int penaltyRenown)
        {
            Id = id;
            Type = type;
            TargetCity = targetCity;
            TributeGrain = tributeGrain;
            IssuedYear = issuedYear;
            DeadlineYear = deadlineYear;
            RewardMerit = rewardMerit;
            RewardRenown = rewardRenown;
            PenaltyRenown = penaltyRenown;
        }
    }

    /// <summary>君主任务配置（GDD_014 §Balancing，可版本化）。</summary>
    public sealed class LordMissionConfig
    {
        public int DeadlineYears { get; }        // 期限年数
        public int BaseMerit { get; }
        public int MeritPerRank { get; }
        public int BaseRenown { get; }
        public int PenaltyRenown { get; }
        public long TributeBaseGrain { get; }

        public LordMissionConfig(int deadlineYears, int baseMerit, int meritPerRank, int baseRenown, int penaltyRenown, long tributeBaseGrain)
        {
            DeadlineYears = deadlineYears;
            BaseMerit = baseMerit;
            MeritPerRank = meritPerRank;
            BaseRenown = baseRenown;
            PenaltyRenown = penaltyRenown;
            TributeBaseGrain = tributeBaseGrain;
        }

        /// <summary>默认：期限 3 年、基酬 60 功绩/40 名望（+10 功绩/阶）、逾期损 20 名望、献纳基数 80 石。</summary>
        public static LordMissionConfig Default { get; } = new LordMissionConfig(
            deadlineYears: 3, baseMerit: 60, meritPerRank: 10, baseRenown: 40, penaltyRenown: 20, tributeBaseGrain: 80);
    }

    /// <summary>
    /// 君主任务派发 + 评估（GDD_014，<b>确定性纯函数</b>，ADR-0004/0006）：君主按情势种子化派发一道任务，
    /// 运行期据世界情势评估其进度。奖励随官阶递增（官越高任越重、酬越丰）。不写任何权威态——生成/评估皆纯函数。
    /// </summary>
    public sealed class LordMissionService
    {
        /// <summary>
        /// 派发一道任务：有可攻目标则可能派"讨伐"，否则"守土/献纳"；期限=当前年+配置年数、奖励随官阶。种子化确定性。
        /// </summary>
        public LordMission Generate(
            Rank rank, int year, IReadOnlyList<CityId> attackTargets, CityId ownCapital, ulong seed, LordMissionConfig config)
        {
            LordMissionConfig cfg = config ?? LordMissionConfig.Default;
            var rng = new DeterministicRandom(seed);
            int rankLevel = (int)rank;

            // 类型选择（种子化）：有可攻目标 → 三类均可；否则守土/献纳。
            bool canSubjugate = attackTargets != null && attackTargets.Count > 0;
            int pick = (int)(rng.NextUnit() * FixedPoint.FromInt(3)).RoundToInt();
            MissionType type = pick switch
            {
                0 when canSubjugate => MissionType.Subjugate,
                1 => MissionType.Defend,
                _ => MissionType.Tribute,
            };

            int reward = cfg.BaseMerit + rankLevel * cfg.MeritPerRank;
            int deadline = year + cfg.DeadlineYears;
            string id = $"mission-{type}-{year}-{seed % 100000UL}";

            switch (type)
            {
                case MissionType.Subjugate:
                {
                    int idx = (int)(rng.NextUnit() * FixedPoint.FromInt(attackTargets!.Count)).RoundToInt();
                    if (idx >= attackTargets.Count) idx = attackTargets.Count - 1;
                    return new LordMission(id, type, attackTargets[idx], 0, year, deadline,
                        reward + 20, cfg.BaseRenown + 10, cfg.PenaltyRenown);   // 讨伐酬更丰
                }
                case MissionType.Defend:
                    return new LordMission(id, type, ownCapital, 0, year, deadline,
                        reward, cfg.BaseRenown, cfg.PenaltyRenown);
                default: // Tribute
                {
                    long grain = cfg.TributeBaseGrain + rankLevel * (cfg.TributeBaseGrain / 4);
                    return new LordMission(id, type, null, grain, year, deadline,
                        reward - 15, cfg.BaseRenown - 10, cfg.PenaltyRenown);   // 献纳较轻
                }
            }
        }

        /// <summary>据当前情势评估任务进度（讨伐=已占目标城；守土=守到期限且未失城；献纳=已缴足）。逾期未成 → 失败。</summary>
        public MissionProgress Evaluate(LordMission mission, int currentYear, MissionContext ctx)
        {
            switch (mission.Type)
            {
                case MissionType.Subjugate:
                    if (ctx.OwnsTargetCity) return MissionProgress.Completed;
                    return currentYear > mission.DeadlineYear ? MissionProgress.Failed : MissionProgress.Pending;

                case MissionType.Defend:
                    if (!ctx.OwnsTargetCity) return MissionProgress.Failed;              // 失守即败
                    return currentYear >= mission.DeadlineYear ? MissionProgress.Completed : MissionProgress.Pending;

                default: // Tribute
                    if (ctx.GrainDelivered >= mission.TributeGrain) return MissionProgress.Completed;
                    return currentYear > mission.DeadlineYear ? MissionProgress.Failed : MissionProgress.Pending;
            }
        }
    }
}
