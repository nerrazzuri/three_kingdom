using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>人才知晓阶梯（GDD_027 #2 / GDD_020）：反全知——未闻名者玩家不可见，须经渠道逐级发觉方可接触招揽。</summary>
    public enum TalentKnowledge
    {
        /// <summary>未闻名：玩家全然不知此人（不入任何可见列表）。</summary>
        Unknown = 0,
        /// <summary>听闻：知有其人（入可见池，但情报浅，未知所在）。</summary>
        Heard = 1,
        /// <summary>已定位：知其所在，可遣使。</summary>
        Located = 2,
        /// <summary>已接触：可发起招揽。</summary>
        Contacted = 3,
    }

    /// <summary>发觉渠道（GDD_020）：不同渠道发觉深度不同。</summary>
    public enum RecruitChannel
    {
        /// <summary>侦察斥候：知有其人（→听闻）。</summary>
        Scout = 0,
        /// <summary>军师举荐：知其所在（→已定位）。</summary>
        Council = 1,
        /// <summary>部曲人脉/羁绊：引见（→已接触）。</summary>
        Bond = 2,
        /// <summary>历史事件：登场引见（→已接触）。</summary>
        Event = 3,
        /// <summary>亲往访贤：登门（→已接触）。</summary>
        Visit = 4,
    }

    /// <summary>招揽结果（GDD_020）。</summary>
    public enum RecruitOutcome
    {
        None = 0,
        /// <summary>入伙：加入僚属。</summary>
        Joined = 1,
        /// <summary>婉拒：可再图（冷却后）。</summary>
        Declined = 2,
        /// <summary>结怨：屡辱其志，短期不再受招且损名。</summary>
        Resented = 3,
        /// <summary>投敌：野心者转投他家（离场）。</summary>
        Defected = 4,
    }

    /// <summary>
    /// 人才知晓与招揽进度簿（GDD_027 #2 / ADR-0016）：per-将记录发觉阶梯 + 招揽尝试次数 + 末次结果。
    /// 玩家侧权威进度态——为持久化预留（<see cref="Entries"/> 可入存档 DTO；本阶段会话内，读档重发觉，同发觉门范式）。
    /// </summary>
    public sealed class TalentKnowledgeBook
    {
        public struct Entry
        {
            public TalentKnowledge Discovery;
            public int Attempts;
            public RecruitOutcome LastOutcome;
        }

        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

        /// <summary>某将发觉阶梯（未记录＝未闻名）。</summary>
        public TalentKnowledge DiscoveryOf(CharacterId g)
            => g.Value != null && _entries.TryGetValue(g.Value, out Entry e) ? e.Discovery : TalentKnowledge.Unknown;

        /// <summary>某将已招揽尝试次数。</summary>
        public int AttemptsOf(CharacterId g)
            => g.Value != null && _entries.TryGetValue(g.Value, out Entry e) ? e.Attempts : 0;

        /// <summary>某将末次招揽结果。</summary>
        public RecruitOutcome OutcomeOf(CharacterId g)
            => g.Value != null && _entries.TryGetValue(g.Value, out Entry e) ? e.LastOutcome : RecruitOutcome.None;

        /// <summary>是否已闻名（入可见池的反全知门）。</summary>
        public bool IsKnown(CharacterId g) => DiscoveryOf(g) >= TalentKnowledge.Heard;

        /// <summary>是否可发起招揽（已接触且未终局）。</summary>
        public bool CanAttempt(CharacterId g)
        {
            RecruitOutcome o = OutcomeOf(g);
            return DiscoveryOf(g) >= TalentKnowledge.Contacted && o != RecruitOutcome.Joined && o != RecruitOutcome.Defected && o != RecruitOutcome.Resented;
        }

        internal void RaiseDiscovery(CharacterId g, TalentKnowledge to)
        {
            if (g.Value == null) return;
            _entries.TryGetValue(g.Value, out Entry e);
            if (to > e.Discovery) e.Discovery = to;
            _entries[g.Value] = e;
        }

        internal void RecordAttempt(CharacterId g, RecruitOutcome outcome)
        {
            if (g.Value == null) return;
            _entries.TryGetValue(g.Value, out Entry e);
            e.Attempts += 1;
            e.LastOutcome = outcome;
            _entries[g.Value] = e;
        }

        /// <summary>持久化入口（供存档 DTO 读写；键=将 id）。</summary>
        public IReadOnlyDictionary<string, Entry> Entries => _entries;
    }

    /// <summary>某已知人才的招揽视图（反全知：仅已闻名者可见；难度呈定性档，无数字）。</summary>
    public readonly struct KnownTalent
    {
        public string GeneralId { get; }
        public TalentKnowledge Discovery { get; }
        public string DifficultyLabel { get; }
        public bool CanAttempt { get; }
        public RecruitOutcome LastOutcome { get; }
        public KnownTalent(string id, TalentKnowledge disc, string diff, bool canAttempt, RecruitOutcome last)
        {
            GeneralId = id; Discovery = disc; DifficultyLabel = diff; CanAttempt = canAttempt; LastOutcome = last;
        }
    }

    /// <summary>一次招揽尝试的结果（含结局 + 说明）。</summary>
    public readonly struct RecruitAttemptResult
    {
        public bool Accepted { get; }        // 是否受理（须已接触）
        public RecruitOutcome Outcome { get; }
        public string Message { get; }
        public RecruitAttemptResult(bool accepted, RecruitOutcome outcome, string message)
        {
            Accepted = accepted; Outcome = outcome; Message = message;
        }
    }

    /// <summary>
    /// 人才招揽状态机（GDD_027 #2 / GDD_020）：发觉渠道逐级揭示（反全知门）+ 确定性招揽结算（难度−待遇+屡拒生怨）。
    /// <see cref="KnownPool"/> 只呈已闻名者——修复"UI 直调 PoolAt 露全部在野将破反全知"。纯函数（结算注入种子）。
    /// </summary>
    public static class TalentRecruitment
    {
        /// <summary>渠道发觉深度（渠道能把知晓抬到的下限）。</summary>
        private static TalentKnowledge FloorOf(RecruitChannel ch) => ch switch
        {
            RecruitChannel.Scout => TalentKnowledge.Heard,
            RecruitChannel.Council => TalentKnowledge.Located,
            RecruitChannel.Bond => TalentKnowledge.Contacted,
            RecruitChannel.Event => TalentKnowledge.Contacted,
            RecruitChannel.Visit => TalentKnowledge.Contacted,
            _ => TalentKnowledge.Heard,
        };

        /// <summary>经某渠道发觉某在野人才（只升不降；非在野者不入招揽视野，忽略）。</summary>
        public static void Reveal(TalentKnowledgeBook book, CharacterId general, RecruitChannel channel, int anchorYear)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (GeneralAffiliations.AffiliationOf(general, anchorYear).Status != AffiliationStatus.Wandering) return;
            book.RaiseDiscovery(general, FloorOf(channel));
        }

        /// <summary>
        /// 已知人才可招池（GDD_027 #2 反全知门）：仅返回<b>已闻名（≥Heard）且在野在世</b>者——未闻名者玩家看不到。
        /// 替代裸 <see cref="GeneralRecruitment.PoolAt"/> 作为玩家可见招揽列表来源。
        /// </summary>
        public static IReadOnlyList<KnownTalent> KnownPool(int anchorYear, TalentKnowledgeBook book, int renownTier = 0)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            var pool = new List<KnownTalent>();
            foreach (GeneralDossier d in GeneralDossiers.All)
            {
                if (!book.IsKnown(d.Id)) continue;   // 反全知门：未闻名不呈
                if (GeneralAffiliations.AffiliationOf(d.Id, anchorYear).Status != AffiliationStatus.Wandering) continue;
                pool.Add(new KnownTalent(d.Id.Value, book.DiscoveryOf(d.Id),
                    GeneralRecruitment.DifficultyOf(d.Id, renownTier), book.CanAttempt(d.Id), book.OutcomeOf(d.Id)));
            }
            pool.Sort((a, b) => string.CompareOrdinal(a.GeneralId, b.GeneralId));
            return pool;
        }

        /// <summary>
        /// 发起招揽（GDD_020）：须已接触（否则不受理）。确定性结算——阻力 = 难度 − 待遇档 + 屡拒累积；种子化抽取定成败。
        /// 成功→入伙(Joined)；失败→婉拒(Declined，可冷却再图)；屡拒且阻力高→结怨(Resented，狼顾者转投敌 Defected)。
        /// </summary>
        public static RecruitAttemptResult Attempt(
            TalentKnowledgeBook book, CharacterId general, int renownTier, int offerTier, ulong seed)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (!book.CanAttempt(general))
                return new RecruitAttemptResult(false, book.OutcomeOf(general), "尚不可招（未接触，或已入伙/结怨/离去）。");

            int attempt = book.AttemptsOf(general);
            int difficulty = GeneralRecruitment.DifficultyScore(general, renownTier);
            int resistance = difficulty - offerTier + attempt;          // 待遇降阻，屡拒升阻

            int threshold = Clamp(35 + resistance * 15, 5, 95);         // 成功阈（越高越难）
            int roll = Roll100(seed, general, attempt);
            bool success = roll >= threshold;

            RecruitOutcome outcome;
            string msg;
            if (success)
            {
                outcome = RecruitOutcome.Joined;
                msg = $"{DisplayId(general)} 感其诚，愿附骥尾——入僚属。";
            }
            else if (attempt >= 1 && resistance >= 3)
            {
                bool wolfish = IsWolfish(general);
                outcome = wolfish ? RecruitOutcome.Defected : RecruitOutcome.Resented;
                msg = wolfish ? $"{DisplayId(general)} 拂袖而去，转投他家。" : $"{DisplayId(general)} 深以为忤，绝意不仕（结怨·损名）。";
            }
            else
            {
                outcome = RecruitOutcome.Declined;
                msg = $"{DisplayId(general)} 婉言相谢，时机未至（可再图）。";
            }

            book.RecordAttempt(general, outcome);
            return new RecruitAttemptResult(true, outcome, msg);
        }

        private static bool IsWolfish(CharacterId g)
        {
            GeneralDossier? d = GeneralDossiers.Find(g);
            return d != null && d.Ambition == Ambition.Wolfish;
        }

        private static int Roll100(ulong seed, CharacterId g, int attempt)
        {
            ulong salt = FnV(g.Value ?? "") ^ ((ulong)(attempt + 1) * 2654435761UL);
            FixedPoint u = new DeterministicRandom(seed, salt).NextUnit();
            return (u * FixedPoint.FromInt(100)).RoundToInt();
        }

        private static ulong FnV(string s)
        {
            ulong h = 14695981039346656037UL;
            foreach (char c in s) { h ^= c; h *= 1099511628211UL; }
            return h;
        }

        private static string DisplayId(CharacterId g) => g.Value ?? "?";
        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
