using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>演义事件求值上下文（GDD_027 P8）：纪元盘年 + 当前公元年 + 玩家势力。（区别于 GDD_015 条件历史 HistoricalEvent。）</summary>
    public readonly struct LoreContext
    {
        public int AnchorYear { get; }
        public int CurrentYear { get; }
        public FactionId PlayerFaction { get; }
        public LoreContext(int anchorYear, int currentYear, FactionId playerFaction)
        {
            AnchorYear = anchorYear; CurrentYear = currentYear; PlayerFaction = playerFaction;
        }
    }

    /// <summary>演义事件的效果种类（GDD_027 R6）：登场/移籍/斩杀。</summary>
    public enum LoreEffectKind
    {
        /// <summary>登场——使一名在野武将被玩家发觉·可招（三顾茅庐）。</summary>
        Introduce = 0,
        /// <summary>移籍——覆盖一名武将的归属势力（<see cref="LoreEffect.ToFaction"/> 为 null 表转在野/流亡）。</summary>
        Reassign = 1,
        /// <summary>斩杀——从世间除名（白门楼/走麦城），此后归属为 Absent，不入任何册。</summary>
        Slay = 2,
    }

    /// <summary>
    /// 一桩演义事件产出的效果（GDD_027 R6，ADR-0016 覆盖层）：不可变数据，纯描述「对谁做什么」。
    /// 由 <see cref="LoreEvents.OverridesAt"/> 确定性重放累积成 <see cref="LoreOverrides"/>，被 <see cref="GeneralAffiliations.AffiliationOf"/> 优先消费。
    /// </summary>
    public readonly struct LoreEffect
    {
        public LoreEffectKind Kind { get; }
        public CharacterId General { get; }
        /// <summary>Reassign 有效：移籍目标势力；null 表转在野。</summary>
        public FactionId? ToFaction { get; }

        private LoreEffect(LoreEffectKind kind, CharacterId general, FactionId? toFaction)
        {
            Kind = kind; General = general; ToFaction = toFaction;
        }

        public static LoreEffect Introduce(CharacterId general) => new LoreEffect(LoreEffectKind.Introduce, general, null);
        public static LoreEffect Reassign(CharacterId general, FactionId? toFaction) => new LoreEffect(LoreEffectKind.Reassign, general, toFaction);
        public static LoreEffect Slay(CharacterId general) => new LoreEffect(LoreEffectKind.Slay, general, null);

        /// <summary>中文效果描述；<paramref name="name"/> 把武将 id 映射成显示名（Presentation 层传入，Domain 不识名表）。</summary>
        public string Describe(Func<CharacterId, string>? name)
        {
            string who = name != null ? name(General) : General.Value;
            switch (Kind)
            {
                case LoreEffectKind.Introduce: return $"{who} 登场（可发觉·可招）";
                case LoreEffectKind.Reassign: return ToFaction == null ? $"{who} 弃职流亡（转在野）" : $"{who} 移籍 → {ToFaction.Value.Value}";
                case LoreEffectKind.Slay: return $"{who} 陨落（除名）";
                default: return who;
            }
        }
    }

    /// <summary>
    /// 演义事件累积后的归属覆盖态（GDD_027 R6 / ADR-0016）：由 <see cref="LoreEvents.OverridesAt"/> 从持久化态确定性重放得出，<b>不入存档</b>。
    /// 优先序高于 baseFaction 派生：斩杀 → Absent；移籍 → 换势力；登场 → 标记可发觉。
    /// </summary>
    public sealed class LoreOverrides
    {
        private readonly Dictionary<string, FactionId?> _reassign = new Dictionary<string, FactionId?>(StringComparer.Ordinal);
        private readonly HashSet<string> _slain = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _introduced = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>空覆盖态（无任何演义事件生效）。</summary>
        public static LoreOverrides Empty { get; } = new LoreOverrides();

        /// <summary>累积一桩效果（常规构建走 <see cref="LoreEvents.OverridesAt"/>；公开以便运行期/测试直接施加）。幂等：集合去重，移籍后写覆盖前。</summary>
        public void Apply(LoreEffect eff)
        {
            string id = eff.General.Value;
            if (id == null) return;
            switch (eff.Kind)
            {
                case LoreEffectKind.Slay: _slain.Add(id); break;
                case LoreEffectKind.Reassign: _reassign[id] = eff.ToFaction; break;
                case LoreEffectKind.Introduce: _introduced.Add(id); break;
            }
        }

        /// <summary>该将是否被演义事件斩杀（此后 Absent）。</summary>
        public bool IsSlain(CharacterId g) => g.Value != null && _slain.Contains(g.Value);

        /// <summary>该将是否被演义事件登场（玩家可发觉·可招）。</summary>
        public bool IsIntroduced(CharacterId g) => g.Value != null && _introduced.Contains(g.Value);

        /// <summary>该将是否被移籍；命中则 <paramref name="faction"/> 为覆盖势力（null 表转在野）。</summary>
        public bool TryReassigned(CharacterId g, out FactionId? faction)
        {
            if (g.Value != null && _reassign.TryGetValue(g.Value, out FactionId? f)) { faction = f; return true; }
            faction = null; return false;
        }

        /// <summary>是否无任何生效覆盖（供 console/UI 判空）。</summary>
        public bool IsEmpty => _slain.Count == 0 && _reassign.Count == 0 && _introduced.Count == 0;

        /// <summary>已陨落者 id（稳定用于展示/断言）。</summary>
        public IReadOnlyCollection<string> Slain => _slain;
        /// <summary>已移籍者 id → 目标势力。</summary>
        public IReadOnlyDictionary<string, FactionId?> Reassigned => _reassign;
        /// <summary>已登场者 id。</summary>
        public IReadOnlyCollection<string> Introduced => _introduced;
    }

    /// <summary>一桩演义事件（GDD_027 R6）：锚定具名武将，按在世/归属/纪元触发，产出登场/移籍/斩杀效果。</summary>
    public sealed class LoreEvent
    {
        public string Id { get; }
        public string Name { get; }
        public string Narrative { get; }
        public IReadOnlyList<LoreEffect> Effects { get; }
        private readonly Func<LoreContext, bool> _trigger;

        public LoreEvent(string id, string name, string narrative, Func<LoreContext, bool> trigger, IReadOnlyList<LoreEffect>? effects = null)
        {
            Id = id; Name = name; Narrative = narrative; _trigger = trigger;
            Effects = effects ?? Array.Empty<LoreEffect>();
        }

        public bool Fires(LoreContext ctx) => _trigger(ctx);
    }

    /// <summary>
    /// 演义事件引擎（GDD_027 P8 / ADR-0016）：登记招牌事件表，按上下文求值当轮触发者并<b>产生归属效果</b>（登场/移籍/斩杀）。
    /// 效果层<b>可推演不入档</b>：触发只依赖已持久化态（纪元盘/当前年/玩家势力/在世门），故 <see cref="OverridesAt"/> 读档时确定性重放即复原，无需新存档字段（沿用发觉门「派生自持久化态」范式）。
    /// 与 GDD_015 条件历史世界模型（Domain.World.HistoricalEvent，因果四元组）分工：此为<b>演义脚本事件</b>，那为世界因果推演。纯函数，无场景依赖。
    /// </summary>
    public static class LoreEvents
    {
        // 势力速记（可读）。
        private static readonly FactionId Shu = PlayableCampaign.LiuBei;
        private static readonly FactionId Wei = PlayableCampaign.Cao;
        private static readonly FactionId Wu = PlayableCampaign.Sun;

        private static bool Alive(string id, int year) => GeneralDossiers.AvailableAt(new CharacterId(id), year);
        private static CharacterId C(string id) => new CharacterId(id);
        private static LoreEffect[] FX(params LoreEffect[] fx) => fx;

        /// <summary>
        /// 招牌事件表（GDD_027 R6，系统化）：10 桩演义关目，触发条件锚定具名武将 + 纪元/玩家势力，产出登场/移籍/斩杀效果。
        /// 「世事」类（白门楼/官渡/走麦城/五丈原）不限玩家势力；「主角亲历」类（桃园/三顾/温酒/三英/水淹）限玩家为对应势力。
        /// </summary>
        public static readonly IReadOnlyList<LoreEvent> All = new[]
        {
            // 桃园结义：玩家扮刘备、讨董之世(≤190)、刘关张俱在 → 义结金兰（凝聚 hook，无归属突变）。
            new LoreEvent(
                id: "event-taoyuan", name: "桃园结义",
                narrative: "念刘备、关羽、张飞，虽然异姓，既结为兄弟，则同心协力，救困扶危；上报国家，下安黎庶。",
                trigger: ctx => ctx.PlayerFaction == Shu && ctx.CurrentYear <= 190
                    && Alive("char-liubei", ctx.CurrentYear) && Alive("char-guanyu", ctx.CurrentYear) && Alive("char-zhangfei", ctx.CurrentYear)),

            // 温酒斩华雄：讨董联军之刘备、≤191、华雄与关羽俱在 → 斩华雄。
            new LoreEvent(
                id: "event-wenjiu", name: "温酒斩华雄",
                narrative: "关某去便来。酒且斟下，某去便来——鸾铃响处，马到中军，云长提华雄之头，掷于地上；其酒尚温。",
                trigger: ctx => ctx.PlayerFaction == Shu && ctx.CurrentYear <= 191
                    && Alive("char-huaxiong", ctx.CurrentYear) && Alive("char-guanyu", ctx.CurrentYear),
                effects: FX(LoreEffect.Slay(C("char-huaxiong")))),

            // 三英战吕布：刘备、≤191、刘关张与吕布俱在 → 虎牢一战（无归属突变，威名叙事）。
            new LoreEvent(
                id: "event-sanying", name: "三英战吕布",
                narrative: "虎牢关前，玄德、云长、翼德三马一心，围裹吕布；画戟纷纷，转灯儿般厮杀——天下英雄，于此并辔。",
                trigger: ctx => ctx.PlayerFaction == Shu && ctx.CurrentYear <= 191
                    && Alive("char-lubu", ctx.CurrentYear) && Alive("char-guanyu", ctx.CurrentYear) && Alive("char-zhangfei", ctx.CurrentYear)),

            // 三顾茅庐：刘备、隆中对之时(≥207)、诸葛亮在世 → 卧龙登场（可发觉·可招）。
            new LoreEvent(
                id: "event-sangu", name: "三顾茅庐",
                narrative: "由是先主遂诣亮，凡三往，乃见。将军既帝室之胄，信义著于四海——则霸业可成，汉室可兴矣。",
                trigger: ctx => ctx.PlayerFaction == Shu && ctx.CurrentYear >= 207
                    && Alive("char-liubei", ctx.CurrentYear) && Alive("char-zhugeliang", ctx.CurrentYear),
                effects: FX(LoreEffect.Introduce(C("char-zhugeliang")))),

            // 官渡·许攸夜奔（世事）：≥200、许攸在世 → 弃袁投曹（移籍：袁绍→曹魏）。
            new LoreEvent(
                id: "event-guandu-xuyou", name: "许攸夜奔",
                narrative: "许攸不告而别，径投曹营。公闻攸至，跣足出迎，抚掌欢笑——乌巢粮尽，袁军自此土崩。",
                trigger: ctx => ctx.CurrentYear >= 200 && ctx.CurrentYear <= 202 && Alive("char-xuyou", ctx.CurrentYear),
                effects: FX(LoreEffect.Reassign(C("char-xuyou"), Wei))),

            // 白门楼（世事）：≥199、吕布覆亡前夕仍在 → 缢吕布、斩高顺陈宫、张辽归曹（移籍）。
            new LoreEvent(
                id: "event-baimenlou", name: "白门楼",
                narrative: "缚一虎，不得不急。吕布授首白门楼下；高顺陈宫从容就戮，唯文远骂贼不屈——曹操义之，亲释其缚，引之上宾。",
                trigger: ctx => ctx.CurrentYear >= 199 && ctx.CurrentYear <= 201 && Alive("char-lubu", 198),
                effects: FX(
                    LoreEffect.Slay(C("char-lubu")), LoreEffect.Slay(C("char-gaoshun")), LoreEffect.Slay(C("char-chengong")),
                    LoreEffect.Reassign(C("char-zhangliao"), Wei))),

            // 赤壁鏖兵：玩家为孙或刘、≥208、周瑜孔明俱在 → 火烧连环（威名叙事，无归属突变）。
            new LoreEvent(
                id: "event-chibi", name: "赤壁鏖兵",
                narrative: "东风既起，火逐风飞；操之舟舰，一时尽着。烟焰张天，人马烧溺死者甚众——樯橹灰飞烟灭。",
                trigger: ctx => (ctx.PlayerFaction == Wu || ctx.PlayerFaction == Shu) && ctx.CurrentYear >= 208 && ctx.CurrentYear <= 209
                    && Alive("char-zhouyu", ctx.CurrentYear)),

            // 水淹七军：玩家扮刘备、≥219、关羽于禁俱在 → 于禁降关（移籍：曹魏→刘备）。
            new LoreEvent(
                id: "event-shuiyan", name: "水淹七军",
                narrative: "秋七月，大霖雨，汉水溢；关羽乘船临围，七军皆没。于禁降，庞德愤詈受戮——威震华夏。",
                trigger: ctx => ctx.PlayerFaction == Shu && ctx.CurrentYear >= 219 && ctx.CurrentYear <= 220
                    && Alive("char-guanyu", ctx.CurrentYear) && Alive("char-yujin", ctx.CurrentYear),
                effects: FX(LoreEffect.Reassign(C("char-yujin"), Shu))),

            // 败走麦城（世事）：≥220、关羽于前岁仍在 → 关羽父子陨落（斩杀）。
            new LoreEvent(
                id: "event-maicheng", name: "败走麦城",
                narrative: "羽率数十骑走麦城，西保临沮，为潘璋部所获。玉可碎不可改其白，竹可焚不可毁其节——身虽殒，名可垂于竹帛。",
                trigger: ctx => ctx.CurrentYear >= 220 && Alive("char-guanyu", 219),
                effects: FX(LoreEffect.Slay(C("char-guanyu")))),

            // 五丈原（世事）：≥234、诸葛亮于前岁仍在 → 秋风陨大星（斩杀）。
            new LoreEvent(
                id: "event-wuzhang", name: "秋风五丈原",
                narrative: "亮疾病，卒于军，时年五十四。是夜，一星赤而有芒，自东北落于蜀营——出师未捷身先死，长使英雄泪满襟。",
                trigger: ctx => ctx.CurrentYear >= 234 && Alive("char-zhugeliang", 233),
                effects: FX(LoreEffect.Slay(C("char-zhugeliang")))),
        };

        /// <summary>某上下文当轮（在 <see cref="LoreContext.CurrentYear"/>）触发的招牌事件（稳定序，供叙事播报）。</summary>
        public static IReadOnlyList<LoreEvent> FiredAt(LoreContext ctx)
        {
            var fired = new List<LoreEvent>();
            foreach (LoreEvent e in All) if (e.Fires(ctx)) fired.Add(e);
            return fired;
        }

        /// <summary>
        /// 从纪元盘开局至当前年，确定性重放所有已触发事件累积成的归属覆盖态（GDD_027 R6 / ADR-0016）。
        /// 逐年扫描 [AnchorYear, CurrentYear]，凡触发即累积其效果；纯函数、幂等，读档重算即复原，<b>不入存档</b>。
        /// </summary>
        public static LoreOverrides OverridesAt(LoreContext ctx)
        {
            var ov = new LoreOverrides();
            int from = Math.Min(ctx.AnchorYear, ctx.CurrentYear);
            for (int y = from; y <= ctx.CurrentYear; y++)
            {
                var stepped = new LoreContext(ctx.AnchorYear, y, ctx.PlayerFaction);
                foreach (LoreEvent e in All)
                    if (e.Fires(stepped))
                        foreach (LoreEffect eff in e.Effects) ov.Apply(eff);
            }
            return ov;
        }
    }
}
