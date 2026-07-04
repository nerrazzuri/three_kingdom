using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Conquest;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Talent;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Application.Talent
{
    /// <summary>一次招揽尝试的结果（判定 + 新态 + 入伙为将，或无效原因）。不可变。</summary>
    public sealed class TalentRecruitAttempt
    {
        /// <summary>调用是否有效（登场 + 已知晓 + 未入伙才有效）。</summary>
        public bool Valid { get; }
        /// <summary>无效原因（Valid=false 时）。</summary>
        public string Reason { get; }
        /// <summary>招揽判定（有效时）。</summary>
        public RecruitmentVerdict Verdict { get; }
        /// <summary>更新后的人才态（无效时=原态）。</summary>
        public TalentState State { get; }
        /// <summary>入伙人才为将（出仕时非空）。</summary>
        public OffensiveGeneral? Joined { get; }

        internal TalentRecruitAttempt(bool valid, string reason, RecruitmentVerdict verdict, TalentState state, OffensiveGeneral? joined)
        {
            Valid = valid;
            Reason = reason;
            Verdict = verdict;
            State = state;
            Joined = joined;
        }
    }

    /// <summary>
    /// 人才招揽编排（Application / GDD_020）：把 Domain 人才服务连成循环——知晓（反全知）→ 可见（登场∩知晓）→
    /// 招揽（登场/知晓校验 + 种子组装 + 条件种子判定 + 态更新 + 入伙为将）。只编排不拥规则（判定在 Domain）。
    /// </summary>
    public sealed class TalentService
    {
        private readonly TalentRecruitmentService _recruit = new TalentRecruitmentService();
        private readonly TalentAwarenessService _aware = new TalentAwarenessService();

        /// <summary>经渠道知晓某人才（反全知：此后才进入视野）。</summary>
        public TalentState Reveal(TalentState state, TalentId id, TalentChannel channel) => state.Reveal(id, channel);

        /// <summary>玩家可见人才（已登场 ∩ 已知晓）。</summary>
        public IReadOnlyList<TalentProfile> Visible(TalentRoster roster, TalentState state, WorldTime worldTime)
            => _aware.Visible(roster, state, worldTime);

        /// <summary>
        /// 发起招揽：须已登场 + 已知晓（反全知）+ 未入伙。以 Hash(baseSeed,worldTick,talentId,faction,attemptIndex)
        /// 组装种子（各次尝试独立确定性），条件种子判定 → 更新态（尝试+1，出仕则入伙 + 返回为将）。
        /// </summary>
        public TalentRecruitAttempt AttemptRecruit(
            TalentRoster roster, TalentState state, TalentId id, WorldTime worldTime,
            RecruitmentOffer offer, ulong baseSeed, FactionId playerFaction, TalentRecruitmentConfig config)
        {
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (state == null) throw new ArgumentNullException(nameof(state));

            TalentProfile? profile = roster.Find(id);
            if (profile == null) return Invalid("查无此人。", state);
            if (!profile.AppearedAt(worldTime)) return Invalid("此人尚未出世（未到登场之时）。", state);
            if (!state.Knows(id)) return Invalid("尚未听闻此人（须先经侦察/事件知晓）。", state);      // 反全知
            if (state.IsRecruited(id)) return Invalid("此人已在麾下。", state);

            int attempt = state.Attempts(id);
            ulong seed = ComposeSeed(baseSeed, worldTime, id, playerFaction, attempt);
            TalentRecruitmentResult result = _recruit.Resolve(profile, offer, seed, config);

            TalentState next = state.RecordAttempt(id);
            OffensiveGeneral? joined = null;
            if (result.Verdict == RecruitmentVerdict.Joined)
            {
                next = next.Recruit(id);
                joined = profile.ToGeneral();
            }
            return new TalentRecruitAttempt(true, string.Empty, result.Verdict, next, joined);
        }

        private static TalentRecruitAttempt Invalid(string reason, TalentState state)
            => new TalentRecruitAttempt(false, reason, RecruitmentVerdict.Declined, state, null);

        private static ulong ComposeSeed(ulong baseSeed, WorldTime t, TalentId id, FactionId faction, int attempt)
        {
            var h = new StateHasher();
            h.Append(baseSeed).Append(t.AbsoluteIndex);
            AppendString(h, id.Value);
            AppendString(h, faction.Value);
            h.Append(attempt);
            return h.ToHash().Value;
        }

        private static void AppendString(StateHasher h, string s)
        {
            h.Append(s?.Length ?? 0);
            if (s != null) foreach (char c in s) h.Append((int)c);
        }
    }
}
