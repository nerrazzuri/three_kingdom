using ThreeKingdom.Domain.Numerics;

namespace ThreeKingdom.Domain.Life
{
    /// <summary>空降者人生阶段（GDD_026 F5/§11：只给定性档，不给精确倒计时——反全知延伸至己身）。</summary>
    public enum LifePhase
    {
        /// <summary>春秋鼎盛：盛年当打。</summary>
        Vigorous = 0,
        /// <summary>年事渐高：精力渐衰，当思身后。</summary>
        Aging = 1,
        /// <summary>风烛残年：大限将近。</summary>
        Twilight = 2,
    }

    /// <summary>空降者寿命配置（GDD_026 §12，可版本化）。默认：弱冠入场、寿约 40–55 活跃年。</summary>
    public sealed class ArrivalLifeConfig
    {
        /// <summary>入场年龄（默认 20，弱冠）。</summary>
        public int EntryAge { get; }
        /// <summary>寿命基准年数（默认 48）。</summary>
        public int LifespanBase { get; }
        /// <summary>寿命上下浮动年数（默认 7 → 寿命 ∈ [41,55]）。</summary>
        public int LifespanSpread { get; }
        /// <summary>「年事渐高」起于大限前几年（默认 15）。</summary>
        public int AgingLeadYears { get; }
        /// <summary>「风烛残年」起于大限前几年（默认 5）。</summary>
        public int TwilightLeadYears { get; }

        public ArrivalLifeConfig(int entryAge = 20, int lifespanBase = 48, int lifespanSpread = 7,
            int agingLeadYears = 15, int twilightLeadYears = 5)
        {
            EntryAge = entryAge;
            LifespanBase = lifespanBase;
            LifespanSpread = lifespanSpread;
            AgingLeadYears = agingLeadYears;
            TwilightLeadYears = twilightLeadYears;
        }

        public static ArrivalLifeConfig Default { get; } = new ArrivalLifeConfig();
    }

    /// <summary>
    /// 空降者的一生（GDD_026 R5 / ADR-0015 D5）：入场年龄 + 一段<b>确定性寿命</b>（种子化派生自会话，非纯随机）。
    /// 寿终（当前年 ≥ 卒年）为一世的<b>自然落幕</b>——非 game-over，走结算+传承（GDD_014）。选年因此有取舍：
    /// 早入乱世活得久、历经更多大势；晚入则跑道短。<b>不可变</b>、可由会话 id 复现（存读档一致，ADR-0004）。
    /// </summary>
    public sealed class ArrivalLife
    {
        /// <summary>入场年龄。</summary>
        public int EntryAge { get; }
        /// <summary>寿命（活跃年数）。</summary>
        public int Lifespan { get; }
        /// <summary>入场公元年（= 锚点年）。</summary>
        public int StartYear { get; }
        private readonly ArrivalLifeConfig _cfg;

        /// <summary>卒年（公元）。</summary>
        public int DeathYear => StartYear + Lifespan;
        /// <summary>寿终年龄。</summary>
        public int DeathAge => EntryAge + Lifespan;

        public ArrivalLife(int entryAge, int lifespan, int startYear, ArrivalLifeConfig config)
        {
            EntryAge = entryAge;
            Lifespan = lifespan;
            StartYear = startYear;
            _cfg = config ?? ArrivalLifeConfig.Default;
        }

        /// <summary>某公元年时的年龄。</summary>
        public int AgeAt(int year) => EntryAge + (year - StartYear);

        /// <summary>是否已寿终（当前年 ≥ 卒年）。</summary>
        public bool IsOver(int year) => year >= DeathYear;

        /// <summary>某公元年时的人生阶段（定性；越近大限越晚阶段）。</summary>
        public LifePhase PhaseAt(int year)
        {
            int remaining = DeathYear - year;
            if (remaining <= _cfg.TwilightLeadYears) return LifePhase.Twilight;
            if (remaining <= _cfg.AgingLeadYears) return LifePhase.Aging;
            return LifePhase.Vigorous;
        }

        /// <summary>
        /// 由种子确定性派生一生（GDD_026 F3）：<c>寿命 = 基准 + round((2u−1)·浮动)</c>，
        /// <c>u = DeterministicRandom(seed).NextUnit()</c>。注入式确定性流（复用 ADR-0004 随机基座），非掷骰、可复现。
        /// </summary>
        public static ArrivalLife Roll(ulong seed, int startYear, ArrivalLifeConfig config)
        {
            ArrivalLifeConfig cfg = config ?? ArrivalLifeConfig.Default;
            var rng = new DeterministicRandom(seed);
            FixedPoint u = rng.NextUnit();                                   // [0,1)
            FixedPoint centered = u * FixedPoint.FromInt(2) - FixedPoint.One; // [-1,1)
            int spread = (centered * FixedPoint.FromInt(cfg.LifespanSpread)).RoundToInt();
            int lifespan = cfg.LifespanBase + spread;
            return new ArrivalLife(cfg.EntryAge, lifespan, startYear, cfg);
        }
    }
}
