using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>武将在某纪元的归属状态（GDD_027 R1）。</summary>
    public enum AffiliationStatus
    {
        /// <summary>未及冠或已故——不入任何册（生卒门，GDD_026）。</summary>
        Absent = 0,
        /// <summary>在野——未仕，可被发觉、可招揽（本势力此纪元不存续，或无归属登记）。</summary>
        Wandering = 1,
        /// <summary>在职——事奉某势力、驻某城、担某役。</summary>
        InService = 2,
    }

    /// <summary>武将担当的角色（GDD_027 R3，从气质标签/档派生，无新增数据）。</summary>
    public enum GeneralRole
    {
        Administrator = 0, // 内政
        Defender = 1,      // 守将
        Vanguard = 2,      // 先锋
        Strategist = 3,    // 谋士
        Scout = 4,         // 斥候
        Naval = 5,         // 水军
    }

    /// <summary>一名武将某纪元的归属投影（不可变值）。</summary>
    public readonly struct Affiliation
    {
        public AffiliationStatus Status { get; }
        public FactionId Faction { get; }   // InService 有效
        public CityId City { get; }         // InService 有效（驻城/治所）
        public GeneralRole Role { get; }    // InService 有效

        private Affiliation(AffiliationStatus status, FactionId faction, CityId city, GeneralRole role)
        {
            Status = status; Faction = faction; City = city; Role = role;
        }

        public static Affiliation Absent { get; } = new Affiliation(AffiliationStatus.Absent, default, default, default);
        public static Affiliation Wandering { get; } = new Affiliation(AffiliationStatus.Wandering, default, default, default);
        public static Affiliation Serving(FactionId f, CityId c, GeneralRole r) => new Affiliation(AffiliationStatus.InService, f, c, r);
    }

    /// <summary>
    /// 武将归属层（GDD_027 / ADR-0016）：把 500 武将解耦地融入全局——归属/角色/城册均<b>纯函数派生</b>，
    /// 由 baseFaction（一将一势力标签）+ 生卒（GDD_026）+ 纪元盘（<see cref="PlayableCampaign.WorldAt"/> via
    /// FactionExistsAt/Capital）合成，不依赖任何场景。六消费方（招揽/内政/军师/关系/战斗/事件）各自按需查询。
    /// </summary>
    public static class GeneralAffiliations
    {
        /// <summary>城武将册上限（GDD_027 R2，可版本化调）。</summary>
        public const int RosterCap = 20;

        private static readonly IReadOnlyDictionary<string, FactionId> Base = BuildBase();

        /// <summary>某将的本属势力（GDD_027 R1）；未登记（名士/隐士/女性/方士/异族散/散雄）→ null（默认在野）。</summary>
        public static FactionId? BaseFactionOf(CharacterId general)
            => general.Value != null && Base.TryGetValue(general.Value, out FactionId f) ? f : (FactionId?)null;

        private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<int, FactionId?>> EraOverrides = BuildEraOverrides();

        /// <summary>
        /// (将,纪元)→势力 覆盖（GDD_027 R1 史准细化）：某将在加入其 baseFaction 前的实际归属（值 null=该纪元在野）。
        /// 命中返 true 且 <paramref name="faction"/> 为覆盖势力。数据活、可扩，修「跳槽者在早纪元混入终属势力册」（如黄权 190 混入刘备小沛册）。
        /// </summary>
        public static bool TryEraFaction(CharacterId general, int anchorYear, out FactionId? faction)
        {
            faction = null;
            return general.Value != null
                && EraOverrides.TryGetValue(general.Value, out IReadOnlyDictionary<int, FactionId?>? byYear)
                && byYear.TryGetValue(anchorYear, out faction);
        }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<int, FactionId?>> BuildEraOverrides()
        {
            var m = new Dictionary<string, IReadOnlyDictionary<int, FactionId?>>(StringComparer.Ordinal);
            void O(string id, params (int Year, FactionId? Faction)[] entries)
            {
                var d = new Dictionary<int, FactionId?>();
                foreach ((int y, FactionId? f) in entries) d[y] = f;
                m[id] = d;
            }
            FactionId? LZ = PlayableCampaign.LiuZhang;   // 刘璋/刘焉·益州
            FactionId? MT = PlayableCampaign.MaTeng;     // 西凉马氏
            FactionId? WEI = PlayableCampaign.Cao;       // 曹魏
            FactionId? WILD = null;                      // 在野

            // 益州系（加入刘备 214 前属刘璋/刘焉）：
            O("char-huangquan", (190, LZ), (194, LZ), (200, LZ), (208, LZ));
            O("char-donghe", (190, LZ), (194, LZ), (200, LZ), (208, LZ));
            O("char-fazheng", (194, LZ), (200, LZ), (208, LZ));   // 190 未及冠（生176）
            // 西凉马氏（马超加入刘备 214 前从父马腾）：
            O("char-machao", (194, MT), (200, MT), (208, MT));    // 190 未及冠（生176）
            // 后归者早年在野（杨仪加入刘备 ~209 前辗转）：
            O("char-yangyi", (190, WILD), (194, WILD), (200, WILD), (208, WILD));
            // 姜维降蜀 228 前属曹魏天水：
            O("char-jiangwei", (219, WEI));               // 202 生，190/208 未出；219 属魏

            return m;
        }

        /// <summary>
        /// 某将在某纪元的归属（GDD_027 R1）：生卒门 → 本属势力存续则在职（驻布防城，无则治所；角色派生），否则在野。
        /// 城/势力键于纪元盘（anchorYear）；在世与否由调用方按当前年 <see cref="GeneralDossiers.AvailableAt"/> 另门（地图已如此）。
        /// </summary>
        public static Affiliation AffiliationOf(CharacterId general, int anchorYear, LoreOverrides? overrides = null)
        {
            // 演义事件覆盖层（GDD_027 R6 / ADR-0016）优先于 baseFaction 派生：斩杀 → Absent；移籍 → 换势力。
            if (overrides != null && overrides.IsSlain(general)) return Affiliation.Absent;
            if (!GeneralDossiers.AvailableAt(general, anchorYear)) return Affiliation.Absent;
            FactionId? bf = BaseFactionOf(general);
            // 纪元覆盖（GDD_027 R1 史准细化）：跳槽者加入 baseFaction 前的实际归属（null=在野）。数据活，非引擎改。
            if (TryEraFaction(general, anchorYear, out FactionId? eraF)) bf = eraF;
            // 演义事件移籍（动态，最高优先，覆盖静态纪元）。
            if (overrides != null && overrides.TryReassigned(general, out FactionId? moved)) bf = moved;
            if (bf == null || !PlayableCampaign.FactionExistsAt(bf.Value, anchorYear)) return Affiliation.Wandering;
            CityId? station = GeneralDossiers.StationOf(general, anchorYear) ?? PlayableCampaign.FactionCapitalAt(bf.Value, anchorYear);
            if (station == null) return Affiliation.Wandering;
            return Affiliation.Serving(bf.Value, station.Value, RoleOf(general));
        }

        /// <summary>角色派生（GDD_027 R3）：按气质标签/档取主役；一将可兼数役，取最强项。</summary>
        public static GeneralRole RoleOf(CharacterId general)
        {
            GeneralDossier? d = GeneralDossiers.Find(general);
            if (d == null) return GeneralRole.Administrator;
            if (d.HasTag(GeneralTag.Naval)) return GeneralRole.Naval;
            if (d.Strategy >= StrategyTier.Adept || d.HasTag(GeneralTag.Strategist)) return GeneralRole.Strategist;
            if (d.HasTag(GeneralTag.NightRaider)) return GeneralRole.Scout;
            if (d.Prowess >= CombatTier.Valiant || d.HasTag(GeneralTag.Cavalry) || d.HasTag(GeneralTag.Reckless)) return GeneralRole.Vanguard;
            if (d.HasTag(GeneralTag.Defender) || d.HasTag(GeneralTag.IronBones)) return GeneralRole.Defender;
            if (d.HasTag(GeneralTag.Benevolent) || d.Strategy >= StrategyTier.Sharp) return GeneralRole.Administrator;
            return GeneralRole.Defender;
        }

        /// <summary>
        /// 某城某纪元的武将册（GDD_027 R2，≤ <see cref="RosterCap"/>）：全体在职于该城之将，按 max(战阵档,谋略档) 降序 + id 稳定序裁剪。
        /// NPC 城由此派生；玩家城由太守调拨（任用态另存，P3）。
        /// </summary>
        public static IReadOnlyList<CharacterId> RosterOf(CityId city, int anchorYear, LoreOverrides? overrides = null)
        {
            var members = new List<CharacterId>();
            if (city.Value == null) return members;
            foreach (GeneralDossier d in GeneralDossiers.All)
            {
                Affiliation a = AffiliationOf(d.Id, anchorYear, overrides);
                if (a.Status == AffiliationStatus.InService && a.City.Value == city.Value) members.Add(d.Id);
            }
            members.Sort((x, y) =>
            {
                int rx = RankKey(x), ry = RankKey(y);
                if (rx != ry) return ry.CompareTo(rx);                       // 档高在前
                return string.CompareOrdinal(x.Value, y.Value);             // 稳定
            });
            if (members.Count > RosterCap) members.RemoveRange(RosterCap, members.Count - RosterCap);
            return members;
        }

        private static int RankKey(CharacterId g)
        {
            GeneralDossier? d = GeneralDossiers.Find(g);
            if (d == null) return 0;
            return Math.Max((int)d.Prowess, (int)d.Strategy);
        }

        private static IReadOnlyDictionary<string, FactionId> BuildBase()
        {
            var m = new Dictionary<string, FactionId>(StringComparer.Ordinal);
            void F(FactionId faction, params string[] ids) { foreach (string id in ids) m[id] = faction; }

            // ---- 蜀汉（刘备阵营）----
            F(PlayableCampaign.LiuBei,
                "char-liubei", "char-guanyu", "char-zhangfei", "char-zhaoyun", "char-zhugeliang", "char-machao", "char-huangzhong",
                "char-weiyan", "char-pangtong", "char-jiangwei", "char-mizhu", "char-mifang", "char-sunqian", "char-jianyong", "char-yiji",
                "char-maliang", "char-masu", "char-fazheng", "char-fengxi", "char-futong", "char-wuban", "char-gaoxiang", "char-lvkai",
                "char-wangfu", "char-chendao", "char-dongyun", "char-yangyi", "char-xiangchong", "char-liaoli", "char-jiangwan", "char-feiyi",
                "char-liyan", "char-liufeng", "char-guanping", "char-guansuo", "char-guanxing", "char-zhangbao", "char-madai", "char-wangping",
                "char-zhoucang", "char-liaohua", "char-yanyan", "char-huojun", "char-mengda", "char-zhugezhan", "char-fuqian", "char-dengzhi",
                "char-zhangyi", "char-zhangni", "char-wuyi", "char-lihui", "char-qiaozhou", "char-huangquan", "char-liuba", "char-zongyu",
                "char-xujing", "char-mengguang", "char-yinmo", "char-laijiang", "char-yangxi", "char-liuyong", "char-liuli", "char-chenshi",
                "char-zhugeshang", "char-huangchengyan", "char-huangyueying", "char-pengyang", "char-xianglang", "char-donghe", "char-huanghao",
                "char-chenzhi", "char-zhangshao", "char-yanghong", "char-lvyi", "char-duqiong", "char-huji", "char-mazhong", "char-huoyi",
                "char-luoxian", "char-zhaotong", "char-zhaoguang", "char-dongjue", "char-fanjian", "char-jiangshu", "char-liushan", "char-liuchen",
                "char-ganfuren", "char-mifuren", "char-zhangsong", "char-sunshangxiang");

            // ---- 曹魏（曹操阵营；含晋室诸臣）----
            F(PlayableCampaign.Cao,
                "char-caocao", "char-xiahoudun", "char-xiahouyuan", "char-zhangliao", "char-xuchu", "char-dianwei", "char-guojia", "char-xunyu",
                "char-xunyou", "char-simayi", "char-caoren", "char-caohong", "char-jiaxu", "char-zhanghe", "char-xuhuang", "char-yujin",
                "char-yuejin", "char-lidian", "char-caozhang", "char-caopi", "char-caozhi", "char-dengai", "char-zhonghui", "char-guohuai",
                "char-caozhen", "char-pangde", "char-wenpin", "char-chengyu", "char-simashi", "char-simazhao", "char-huaxin", "char-manchong",
                "char-caoxiu", "char-caorui", "char-caoshuang", "char-chenqun", "char-chentai", "char-wangji", "char-wangjun", "char-weiguan",
                "char-wangling", "char-wangshuang", "char-dengzhong", "char-guanqiujian", "char-wenqin", "char-wenyang", "char-zhugedan",
                "char-zhongyao", "char-jiachong", "char-jiangji", "char-tianyu", "char-qianzhao", "char-duyu", "char-yanghu", "char-xiahouba",
                "char-caoang", "char-yangxiu", "char-cuiyan", "char-maojie", "char-dongzhao", "char-wanglang", "char-chenlin", "char-lvqian",
                "char-hanhao", "char-simafu", "char-huanfan", "char-jianggan", "char-qinlang", "char-niujin", "char-shihuan", "char-wangbi",
                "char-zhaoyan", "char-dumu", "char-zhangji-wei", "char-suze", "char-hanji", "char-gaotanglong", "char-liufang", "char-sunzi",
                "char-zhongyu", "char-xunyi", "char-wangsu", "char-dingmi", "char-biyang", "char-lisheng", "char-wangguan", "char-chenjiao",
                "char-dianman", "char-xuyi", "char-panghui", "char-xiahouhui", "char-xiahourong", "char-xiahouwei", "char-caoxi", "char-linghuyu",
                "char-changdiao", "char-simawang", "char-simazhou", "char-simagan", "char-wangchen", "char-caofang", "char-caomao", "char-caohuan",
                "char-wangjing", "char-hufen", "char-simayan", "char-xunxu", "char-caochun", "char-hulie", "char-qianhong", "char-zhenji",
                "char-guozhao", "char-xinxianying", "char-caiwenji", "char-wangyi");

            // ---- 孙吴（孙氏阵营）----
            F(PlayableCampaign.Sun,
                "char-sunce", "char-sunquan", "char-sunjian", "char-zhouyu", "char-lusu", "char-lvmeng", "char-luxun", "char-taishici",
                "char-ganning", "char-huanggai", "char-chengpu", "char-handang", "char-zhoutai", "char-jiangqin", "char-chenwu", "char-lingtong",
                "char-xusheng", "char-dingfeng", "char-panzhang", "char-zhuran", "char-zhugejin", "char-zhangzhao", "char-zhugeke", "char-daqiao",
                "char-xiaoqiao", "char-guyong", "char-kanze", "char-lukang", "char-machongwu", "char-quancong", "char-zhuhuan", "char-buzhi",
                "char-zhanghong", "char-dongxi", "char-sunhuan", "char-sunyi", "char-sunjun", "char-sunchen", "char-sunhao", "char-sunxiu",
                "char-sunliang", "char-zhuju", "char-lvju", "char-liuzan", "char-tengyin", "char-tangzi", "char-panjun", "char-zhangwen",
                "char-yanjun", "char-zhaozi", "char-lukai", "char-zhuzhi", "char-shiji", "char-sunben", "char-sunjiao", "char-sunyu",
                "char-sundeng", "char-sunhe", "char-xuegong", "char-zhouchu", "char-lvdai", "char-quanji", "char-hulzong", "char-shiyi",
                "char-luyi", "char-luji", "char-sunkuang", "char-sunshao", "char-wufan", "char-liulve", "char-buchan", "char-quanshang",
                "char-sunlang", "char-weizhao", "char-huagai", "char-bufuren", "char-panshu", "char-wuguotai");

            // ---- 袁绍 ----
            F(PlayableCampaign.YuanShao,
                "char-yuanshao", "char-yanliang", "char-wenchou", "char-tianfeng", "char-jushou", "char-shenpei", "char-gaolan",
                "char-chunyuqiong", "char-guotu", "char-fengji", "char-xinping", "char-gaogan", "char-yuanxi", "char-yuantan", "char-yuanshang",
                "char-lvkuang", "char-lvxiang", "char-hanmeng", "char-jiangyiqu", "char-quyi", "char-xuyou", "char-xunchen", "char-jiaochu");

            // ---- 吕布 ----
            F(PlayableCampaign.LuBu,
                "char-lubu", "char-gaoshun", "char-chengong", "char-zangba", "char-songxian", "char-weixu", "char-houcheng",
                "char-caoxing", "char-haomeng", "char-chenglian");

            // ---- 刘表（荆州）----
            F(PlayableCampaign.LiuBiao,
                "char-liubiao", "char-huangzu", "char-caimao", "char-kuailiang", "char-kuaiyue", "char-liupan", "char-zhangyun",
                "char-songzhong", "char-wangwei", "char-liuqi", "char-liucong", "char-hanxuan");

            // ---- 刘璋/刘焉（益州）----
            F(PlayableCampaign.LiuZhang,
                "char-liuzhang", "char-liuyan", "char-zhangren", "char-lengbao", "char-dengxian", "char-gaopei", "char-yanghuai",
                "char-zhengdu", "char-wanglei", "char-liugui");

            // ---- 马腾（西凉马氏）----
            F(PlayableCampaign.MaTeng, "char-mateng", "char-maxiu", "char-machie");

            // ---- 韩遂（凉州）----
            F(PlayableCampaign.HanSui,
                "char-hansui", "char-chengyi", "char-liangxing", "char-houxuan", "char-yangqiu", "char-mawan", "char-biansang",
                "char-yanxing", "char-songjian");

            // ---- 公孙瓒（幽州）----
            F(PlayableCampaign.GongSun, "char-gongsun", "char-guanjing", "char-zoudan");

            // ---- 董卓/李傕（凉州军阀）----
            F(PlayableCampaign.LiJue,
                "char-dongzhuo", "char-lijue", "char-guosi", "char-fanchou", "char-huaxiong", "char-liru", "char-lisu", "char-huchier",
                "char-niufu", "char-dongmin", "char-hujzhen", "char-yangding", "char-duanwei", "char-liyue", "char-hucai", "char-zhangji");

            // ---- 袁术 ----
            F(PlayableCampaign.Enemy, "char-yuan", "char-jiling", "char-leibo", "char-chenlan", "char-liuxun");

            // ---- 单城诸侯 ----
            F(PlayableCampaign.ZhangXiu, "char-zhangxiu");
            F(PlayableCampaign.KongRong, "char-kongrong");
            F(PlayableCampaign.ZhangLu, "char-zhanglu");
            F(PlayableCampaign.ShiXie, "char-shixie");

            // ---- 汉庭（184/朝堂）----
            F(PlayableCampaign.Han,
                "char-hejin", "char-wangyun", "char-luzhi", "char-huangfusong", "char-zhujun", "char-caiyong", "char-dongcheng",
                "char-yangbiao", "char-fuwan", "char-fuhou", "char-caojie", "char-huangwan", "char-zhaoqi");

            // ---- 黄巾 ----
            F(PlayableCampaign.Huangjin,
                "char-zhangjiao", "char-zhangbao-turban", "char-zhangliang-turban", "char-bocai", "char-zhangmancheng",
                "char-guanhai", "char-liupi", "char-gongdu", "char-peiyuanshao");

            // 未登记者（名士/隐士/女性中立/方士/异族散/散雄/空降者）→ 默认在野（BaseFactionOf 返 null）。
            return m;
        }
    }
}
