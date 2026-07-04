using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.World
{
    /// <summary>
    /// 心里话规则（GDD_015 事件分级，数据驱动）：某"够不着"事件结局 → 主角一句心里话，
    /// <b>口吻随主角人设而异</b>（同一事件，雄心者动心、忠义者不齿）。纯为丰富体验/代入，<b>非机械种子</b>。不可变。
    /// </summary>
    public sealed class MonologueRule
    {
        private readonly Dictionary<ProtagonistPersona, string> _byPersona;

        /// <summary>匹配的事件结局标签（<see cref="HistoricalOutcome.Label"/>）。</summary>
        public string OutcomeLabel { get; }
        /// <summary>缺人设专属台词时的通用心里话（非空）。</summary>
        public string DefaultLine { get; }

        public MonologueRule(string outcomeLabel, string defaultLine, IReadOnlyDictionary<ProtagonistPersona, string>? byPersona = null)
        {
            if (string.IsNullOrWhiteSpace(outcomeLabel)) throw new ArgumentException("结局标签不可为空。", nameof(outcomeLabel));
            if (string.IsNullOrWhiteSpace(defaultLine)) throw new ArgumentException("通用心里话不可为空。", nameof(defaultLine));
            OutcomeLabel = outcomeLabel;
            DefaultLine = defaultLine;
            _byPersona = new Dictionary<ProtagonistPersona, string>();
            if (byPersona != null)
                foreach (KeyValuePair<ProtagonistPersona, string> kv in byPersona)
                    if (!string.IsNullOrWhiteSpace(kv.Value)) _byPersona[kv.Key] = kv.Value;
        }

        /// <summary>按主角人设取心里话；无专属则回退通用台词。</summary>
        public string LineFor(ProtagonistPersona persona)
            => _byPersona.TryGetValue(persona, out string line) ? line : DefaultLine;
    }

    /// <summary>
    /// 心里话规则表（GDD_015 事件分级，数据驱动，构建期由配置提供）。按结局标签查规则。不可变。
    /// </summary>
    public sealed class MonologueCatalog
    {
        private readonly Dictionary<string, MonologueRule> _rules = new Dictionary<string, MonologueRule>(StringComparer.Ordinal);

        public MonologueCatalog(IReadOnlyList<MonologueRule> rules)
        {
            if (rules is null) throw new ArgumentNullException(nameof(rules));
            foreach (MonologueRule r in rules)
            {
                if (r is null) throw new ArgumentException("规则不可含 null。", nameof(rules));
                _rules[r.OutcomeLabel] = r;   // 同标签后者覆盖（构建期容错）
            }
        }

        /// <summary>查某结局标签的心里话规则；无则 null（→ 背景事件）。</summary>
        public MonologueRule? Find(string outcomeLabel)
            => outcomeLabel != null && _rules.TryGetValue(outcomeLabel, out MonologueRule r) ? r : null;

        /// <summary>空表（一切够不着事件皆背景）。</summary>
        public static MonologueCatalog Empty { get; } = new MonologueCatalog(Array.Empty<MonologueRule>());

        /// <summary>演义代表条目（示例：同一事件按人设给不同口吻的心里话）。内容层逐步扩充（GDD_015 §全演义事件网）。</summary>
        public static MonologueCatalog Default { get; } = new MonologueCatalog(new[]
        {
            new MonologueRule("yuanshu-declares-emperor",
                "袁术竟也敢称帝，这天下当真要变了。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "袁术这般人物竟也敢称帝……那我，是不是也可以？",
                    [ProtagonistPersona.Loyalist]  = "袁术僭号称帝，人神共愤！食君之禄者，岂能坐视此等悖逆。",
                    [ProtagonistPersona.Pragmatist] = "称帝？木秀于林风必摧之。袁术怕是命不久矣，且看着。",
                    [ProtagonistPersona.Cautious]  = "僭越称帝，取祸之道。乱世凶险，还是稳住自家一亩三分地为上。",
                }),
            new MonologueRule("warlord-secedes",
                "又一路诸侯自立门户，天下愈发四分五裂了。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "有兵有城便可为王——这乱世，正是英雄用武之地。",
                    [ProtagonistPersona.Loyalist]  = "背主自立，终究名不正言不顺，难成大器。",
                }),
            new MonologueRule("dong-zhuo-burns-luoyang",
                "洛阳付之一炬，苍生何辜。这乱世，忠义又能保我几时？",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "旧秩序既已焚毁，新天下正待人取——乱世予我机也。",
                    [ProtagonistPersona.Loyalist]  = "董卓焚都弑君，罪不容诛！我辈当扶汉室于将倾。",
                    [ProtagonistPersona.Cautious]  = "京师尚且成灰，边城更难自保。深沟高垒，谨守为上。",
                }),
            new MonologueRule("guan-yu-loses-jingzhou",
                "关云长竟败走麦城……名将尚有此劫，何况你我。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "关羽既失荆州，天下棋局再乱——正是我落子之时。",
                    [ProtagonistPersona.Pragmatist] = "大意失荆州，骄兵必败。前车之鉴，我当引以为戒。",
                }),
            new MonologueRule("cao-cao-controls-emperor",
                "曹孟德挟天子以令诸侯，好大的手笔。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "挟天子以令诸侯——名正则言顺。此局，我亦想弈。",
                    [ProtagonistPersona.Loyalist]  = "名为汉相，实为汉贼！挟持天子，是可忍孰不可忍。",
                    [ProtagonistPersona.Pragmatist] = "奉天子者得大义之名，曹操这步棋走得极精。",
                }),
            new MonologueRule("liu-bei-recruits-zhuge",
                "刘玄德三顾茅庐，终得卧龙。求贤如此，难怪成事。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "得一大才可安天下——我也当访遍山野，求我的卧龙。",
                    [ProtagonistPersona.Cautious]  = "礼贤下士固好，只是这乱世，人心最难测。",
                }),
            new MonologueRule("taoyuan-oath",
                "桃园三结义，不求同生但求同死。这般义气，乱世难得。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Loyalist]  = "义结金兰、生死与共——大丈夫立身，正当如此。",
                    [ProtagonistPersona.Pragmatist] = "义气固可聚人，然成大事，终究还得看实力与时机。",
                }),
            new MonologueRule("wang-yun-chain-plot",
                "王允一出连环计，借吕布之手除了董卓。美人计、离间计，环环相扣。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "以计除强敌，胜过千军万马——攻心为上，我当习之。",
                    [ProtagonistPersona.Pragmatist] = "连环计精妙，然吕布反复，除一董卓又来一董卓。治标而已。",
                }),
            new MonologueRule("guandu-cao-wins",
                "官渡一战，曹操以弱胜袁绍。烧乌巢粮草，一举定北方。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Ambitious] = "以少胜多，正是英雄本色。断其粮道、攻其必救——用兵之道，我记下了。",
                    [ProtagonistPersona.Loyalist]  = "曹操虽奸，用兵却真有过人之处。北方从此姓曹了。",
                    [ProtagonistPersona.Pragmatist] = "袁绍兵多而败，可见胜负不在众寡，在断粮攻心。",
                }),
            new MonologueRule("lubu-executed",
                "吕布殒命白门楼。天下第一猛将，终究反复无信、身死人手。",
                new Dictionary<ProtagonistPersona, string>
                {
                    [ProtagonistPersona.Loyalist]  = "三姓家奴，背主求荣，有此下场不足惜。忠信二字，重于武勇。",
                    [ProtagonistPersona.Cautious]  = "武勇冠世尚且如此收场，做人还是信义、谨慎为本。",
                }),
        });
    }
}
