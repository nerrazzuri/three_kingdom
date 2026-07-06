using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 三国武将档案目录（GDD_025 内容，公版三国人物 + <b>自定的原创标签/心</b>，无任何数值 stat、不取自商业游戏）。
    /// 按稳定 id 查档案；未登记者由调用方回退。供人心杠杆（隐秘心）、战斗（气质标签）、羁绊等消费。
    /// 当前谱系 ~500 员（七批扩充：名将/名士/女性/南蛮羌/魏晋末/汉末朝堂/群雄州牧/后期文武/建安七子/五关六将等），非穷尽全谱——可持续扩充（GDD_025 §Future）。
    /// </summary>
    public static class GeneralDossiers
    {
        private static readonly IReadOnlyDictionary<string, GeneralDossier> ById = Build();
        private static readonly IReadOnlyList<GeneralDossier> Roster = BuildRoster();

        /// <summary>查某武将档案；未登记则 null。</summary>
        public static GeneralDossier? Find(CharacterId id)
            => id.Value != null && ById.TryGetValue(id.Value, out GeneralDossier? d) ? d : null;

        /// <summary>全体已登记武将档案（稳定序：按 id 规范序）——供武将目录（#2）遍历。</summary>
        public static IReadOnlyList<GeneralDossier> All => Roster;

        private static IReadOnlyList<GeneralDossier> BuildRoster()
        {
            var list = new List<GeneralDossier>(ById.Values);
            list.Sort((a, b) => string.CompareOrdinal(a.Id.Value, b.Id.Value));
            return list;
        }

        // ---- 生卒年（GDD_026 / ADR-0015 D4）：史载近似，供在世/在职与 EraStage 判定。GeneralDossiers 仍为武将数据唯一权威。----
        // 空降者（char-player-lord）非历史武将，不入此表（不受生卒约束）。
        private static readonly IReadOnlyDictionary<string, (int Birth, int Death)> LifeYears =
            new Dictionary<string, (int, int)>(StringComparer.Ordinal)
            {
                // 君主
                ["char-caocao"] = (155, 220), ["char-liubei"] = (161, 223), ["char-sunce"] = (175, 200),
                ["char-sunquan"] = (182, 252), ["char-yuanshao"] = (154, 202), ["char-yuan"] = (155, 199),
                ["char-lubu"] = (161, 199), ["char-liubiao"] = (142, 208), ["char-liuzhang"] = (162, 219),
                ["char-mateng"] = (156, 212), ["char-zhanglu"] = (160, 216), ["char-gongsun"] = (151, 199),
                ["char-lijue"] = (150, 198), ["char-zhangxiu"] = (160, 207), ["char-kongrong"] = (153, 208),
                ["char-hansui"] = (140, 215), ["char-shixie"] = (137, 226),
                // 蜀
                ["char-guanyu"] = (160, 220), ["char-zhangfei"] = (165, 221), ["char-zhaoyun"] = (168, 229),
                ["char-zhugeliang"] = (181, 234), ["char-machao"] = (176, 222), ["char-huangzhong"] = (148, 220),
                ["char-weiyan"] = (170, 234), ["char-pangtong"] = (179, 214), ["char-jiangwei"] = (202, 264),
                // 魏
                ["char-xiahoudun"] = (157, 220), ["char-zhangliao"] = (169, 222), ["char-xuchu"] = (170, 230),
                ["char-dianwei"] = (160, 197), ["char-guojia"] = (170, 207), ["char-xunyu"] = (163, 212),
                ["char-simayi"] = (179, 251), ["char-caoren"] = (168, 223), ["char-jiaxu"] = (147, 223),
                // 吴
                ["char-zhouyu"] = (175, 210), ["char-lusu"] = (172, 217), ["char-lvmeng"] = (178, 220),
                ["char-luxun"] = (183, 245), ["char-taishici"] = (166, 206), ["char-ganning"] = (175, 215),
                ["char-huanggai"] = (145, 215),
                // 群雄部将
                ["char-yanliang"] = (160, 200), ["char-wenchou"] = (160, 200), ["char-gaoshun"] = (160, 198),
                ["char-chengong"] = (160, 198), ["char-huaxiong"] = (160, 191), ["char-haozhao"] = (180, 229),
                // 扩充批 1 — 魏
                ["char-xiahouyuan"] = (160, 219), ["char-zhanghe"] = (167, 231), ["char-xuhuang"] = (169, 227),
                ["char-yujin"] = (155, 221), ["char-yuejin"] = (160, 218), ["char-lidian"] = (174, 209),
                ["char-caohong"] = (160, 232), ["char-caozhang"] = (189, 223), ["char-caopi"] = (187, 226),
                ["char-caozhi"] = (192, 232), ["char-dengai"] = (197, 264), ["char-zhonghui"] = (225, 264),
                ["char-guohuai"] = (170, 255), ["char-caozhen"] = (175, 231), ["char-pangde"] = (170, 219),
                ["char-wenpin"] = (160, 228), ["char-chengyu"] = (141, 220), ["char-simashi"] = (208, 255),
                ["char-simazhao"] = (211, 265), ["char-huaxin"] = (157, 232), ["char-manchong"] = (160, 242),
                // 扩充批 1 — 蜀
                ["char-guanping"] = (178, 220), ["char-zhoucang"] = (160, 220), ["char-liaohua"] = (170, 264),
                ["char-madai"] = (170, 240), ["char-wangping"] = (180, 248), ["char-guanxing"] = (192, 234),
                ["char-zhangbao"] = (196, 222), ["char-yanyan"] = (140, 220), ["char-fazheng"] = (176, 220),
                ["char-masu"] = (190, 228), ["char-jiangwan"] = (175, 246), ["char-feiyi"] = (180, 253),
                ["char-liyan"] = (170, 234), ["char-liufeng"] = (180, 220),
                // 扩充批 1 — 吴
                ["char-chengpu"] = (145, 215), ["char-handang"] = (155, 227), ["char-zhoutai"] = (165, 225),
                ["char-jiangqin"] = (160, 220), ["char-chenwu"] = (178, 215), ["char-lingtong"] = (189, 237),
                ["char-xusheng"] = (175, 228), ["char-dingfeng"] = (185, 271), ["char-panzhang"] = (165, 234),
                ["char-zhuran"] = (182, 249), ["char-zhugejin"] = (174, 241), ["char-zhangzhao"] = (156, 236),
                ["char-zhugeke"] = (203, 253), ["char-sunshangxiang"] = (185, 250),
                // 扩充批 1 — 群雄/董卓/黄巾
                ["char-dongzhuo"] = (139, 192), ["char-liru"] = (150, 192), ["char-zhangjiao"] = (140, 184),
                ["char-jiling"] = (150, 199), ["char-tianfeng"] = (150, 200), ["char-jushou"] = (155, 200),
                ["char-shenpei"] = (155, 204), ["char-gaolan"] = (160, 200), ["char-caimao"] = (155, 208),
                ["char-huangzu"] = (150, 208), ["char-zangba"] = (160, 230),
                // 扩充批 2 — 蜀
                ["char-sunqian"] = (160, 215), ["char-mizhu"] = (165, 221), ["char-mifang"] = (168, 230),
                ["char-jianyong"] = (160, 223), ["char-yiji"] = (160, 214), ["char-maliang"] = (187, 222),
                ["char-dengzhi"] = (178, 251), ["char-zhangyi"] = (180, 264), ["char-zhangni"] = (185, 254),
                ["char-wuyi"] = (170, 237), ["char-huojun"] = (178, 217), ["char-mengda"] = (170, 228),
                ["char-guansuo"] = (190, 235), ["char-zhugezhan"] = (227, 263), ["char-fuqian"] = (225, 263),
                // 扩充批 2 — 魏
                ["char-caoxiu"] = (170, 228), ["char-caoshuang"] = (195, 249), ["char-wangshuang"] = (185, 229),
                ["char-tianyu"] = (171, 252), ["char-qianzhao"] = (170, 231), ["char-chentai"] = (200, 260),
                ["char-wangji"] = (190, 261), ["char-jiachong"] = (217, 282), ["char-wangjun"] = (206, 286),
                ["char-yanghu"] = (221, 278), ["char-duyu"] = (222, 285), ["char-jiangji"] = (168, 249),
                ["char-liuye"] = (175, 234), ["char-chenqun"] = (170, 237), ["char-zhongyao"] = (151, 230),
                ["char-caorui"] = (205, 239),
                // 扩充批 2 — 吴
                ["char-sunjian"] = (155, 191), ["char-dongxi"] = (165, 213), ["char-machongwu"] = (175, 240),
                ["char-zhuhuan"] = (177, 238), ["char-quancong"] = (198, 249), ["char-buzhi"] = (170, 247),
                ["char-guyong"] = (168, 243), ["char-zhanghong"] = (153, 212), ["char-kanze"] = (170, 243),
                ["char-lukang"] = (226, 274),
                // 扩充批 2 — 群雄/南蛮/方士
                ["char-huatuo"] = (145, 208), ["char-zuoci"] = (156, 250), ["char-yuji"] = (120, 200),
                ["char-zhangbao-turban"] = (150, 184), ["char-zhangliang-turban"] = (150, 184), ["char-hejin"] = (135, 189),
                ["char-menghuo"] = (180, 250), ["char-zhurong"] = (185, 250), ["char-shamoke"] = (180, 222),
                ["char-zhangren"] = (160, 213), ["char-hanxuan"] = (150, 209), ["char-gongsunkang"] = (180, 238),
                ["char-tadun"] = (160, 207), ["char-kebineng"] = (170, 235), ["char-yanbaihu"] = (160, 196),
                // 扩充批 3 — 名士/女性/南蛮/魏晋末/西凉/副将
                ["char-xushu"] = (168, 234), ["char-xunyou"] = (157, 214), ["char-simahui"] = (145, 208),
                ["char-cuizhouping"] = (160, 220), ["char-zhangsong"] = (160, 212), ["char-lihui"] = (170, 231),
                ["char-qiaozhou"] = (201, 270), ["char-kuailiang"] = (150, 208), ["char-kuaiyue"] = (150, 214),
                ["char-diaochan"] = (172, 200), ["char-daqiao"] = (175, 215), ["char-xiaoqiao"] = (177, 220),
                ["char-zhenji"] = (183, 221), ["char-caiwenji"] = (177, 249), ["char-huangyueying"] = (190, 234),
                ["char-wangyuanji"] = (217, 268), ["char-zhangchunhua"] = (189, 247),
                ["char-wutugu"] = (180, 225), ["char-mulu"] = (180, 225), ["char-duosi"] = (180, 225),
                ["char-dongtuna"] = (180, 225),
                ["char-wenyang"] = (238, 291), ["char-wenqin"] = (200, 258), ["char-zhugedan"] = (200, 258),
                ["char-guanqiujian"] = (190, 255), ["char-wangling"] = (172, 251), ["char-weiguan"] = (220, 291),
                ["char-dengzhong"] = (235, 264),
                ["char-chengyi"] = (160, 211), ["char-liangxing"] = (165, 211), ["char-houxuan"] = (165, 215),
                ["char-yangqiu"] = (165, 215), ["char-mawan"] = (165, 211),
                ["char-panfeng"] = (155, 191), ["char-wuanguo"] = (160, 195), ["char-xingdaorong"] = (170, 209),
                ["char-leitong"] = (175, 218), ["char-wulan"] = (175, 218),
                // 扩充批 4 — 汉末朝堂/讨董名臣
                ["char-wangyun"] = (137, 192), ["char-luzhi"] = (139, 192), ["char-huangfusong"] = (137, 195),
                ["char-zhujun"] = (140, 195), ["char-caiyong"] = (133, 192), ["char-dongcheng"] = (150, 200),
                // 扩充批 4 — 魏晋文武
                ["char-caoang"] = (177, 197), ["char-yangxiu"] = (175, 219), ["char-cuiyan"] = (163, 216),
                ["char-maojie"] = (160, 216), ["char-dongzhao"] = (156, 236), ["char-wanglang"] = (156, 228),
                ["char-chenlin"] = (160, 217), ["char-miheng"] = (173, 198), ["char-lvqian"] = (160, 228),
                ["char-hanhao"] = (160, 216), ["char-simafu"] = (180, 272), ["char-huanfan"] = (170, 249),
                ["char-jianggan"] = (165, 215), ["char-qinlang"] = (190, 238), ["char-guanning"] = (158, 241),
                // 扩充批 4 — 蜀汉后进
                ["char-chendao"] = (165, 230), ["char-dongyun"] = (180, 246), ["char-yangyi"] = (170, 235),
                ["char-xiangchong"] = (170, 240), ["char-liaoli"] = (175, 230), ["char-fengxi"] = (175, 222),
                ["char-futong"] = (175, 222), ["char-wuban"] = (170, 235), ["char-gaoxiang"] = (170, 235),
                ["char-lvkai"] = (175, 225), ["char-wangfu"] = (165, 222),
                // 扩充批 4 — 东吴中生代
                ["char-lvfan"] = (155, 228), ["char-yufan"] = (164, 233), ["char-zhoufang"] = (175, 235),
                ["char-heqi"] = (170, 227), ["char-luotong"] = (193, 228), ["char-zhuyi"] = (205, 257),
                ["char-sunhuan"] = (198, 223), ["char-sunyi"] = (184, 204), ["char-panjun"] = (176, 239),
                ["char-sunjun"] = (219, 256), ["char-sunchen"] = (231, 259),
                // 扩充批 4 — 群雄/汉末诸侯
                ["char-chendeng"] = (163, 201), ["char-chengui"] = (145, 208), ["char-hanfu"] = (150, 191),
                ["char-zhangmiao"] = (155, 195), ["char-zhangyang"] = (160, 198), ["char-baoxin"] = (151, 192),
                ["char-liudai"] = (155, 192), ["char-yuantan"] = (170, 205), ["char-yuanshang"] = (178, 207),
                ["char-zhangyan"] = (160, 205), ["char-guanhai"] = (155, 195), ["char-caiyang"] = (155, 201),
                ["char-chezhou"] = (155, 199),
                // 扩充批 4 — 巾帼
                ["char-bianfuren"] = (159, 230), ["char-ganfuren"] = (168, 209), ["char-mifuren"] = (170, 208),
                ["char-wuguotai"] = (150, 210),
                // 扩充批 5 — 汉末群雄/凉州/黄巾/李郭
                ["char-liuyan"] = (152, 194), ["char-liuyu"] = (142, 193), ["char-taoqian"] = (132, 194),
                ["char-gongsundu"] = (150, 204), ["char-qiaorui"] = (150, 190), ["char-kongzhou"] = (150, 190),
                ["char-zhangchao"] = (155, 195), ["char-bocai"] = (150, 184), ["char-zhangmancheng"] = (150, 184),
                ["char-biansang"] = (140, 185), ["char-lisu"] = (155, 192), ["char-yangfeng"] = (155, 197),
                ["char-hanxian"] = (155, 197), ["char-zhangji"] = (150, 196), ["char-fanchou"] = (155, 195),
                ["char-guosi"] = (155, 197), ["char-huchier"] = (165, 210),
                // 扩充批 5 — 魏
                ["char-jiakui"] = (174, 228), ["char-tianchou"] = (169, 214), ["char-liangxi"] = (160, 230),
                ["char-gaorou"] = (174, 263), ["char-xinpi"] = (165, 235), ["char-luyu"] = (183, 257),
                ["char-xiahouba"] = (180, 259), ["char-wangchang"] = (185, 259), ["char-shiba"] = (190, 273),
                ["char-zhugexu"] = (190, 260), ["char-hujun"] = (180, 256), ["char-caoyu"] = (190, 278),
                // 扩充批 5 — 蜀
                ["char-liushan"] = (207, 271), ["char-mazhong"] = (185, 249), ["char-huoyi"] = (190, 264),
                ["char-luoxian"] = (218, 270), ["char-zhaotong"] = (195, 255), ["char-zhaoguang"] = (198, 263),
                ["char-dongjue"] = (190, 265), ["char-fanjian"] = (195, 270), ["char-jiangshu"] = (210, 270),
                ["char-liuchen"] = (240, 263),
                // 扩充批 5 — 吴
                ["char-sunhao"] = (242, 284), ["char-sunxiu"] = (235, 264), ["char-sunliang"] = (243, 260),
                ["char-zhuju"] = (194, 250), ["char-lvju"] = (200, 256), ["char-liuzan"] = (183, 255),
                ["char-tengyin"] = (190, 256), ["char-tangzi"] = (200, 260),
                // 扩充批 5 — 汉臣/荆州/巾帼
                ["char-yangbiao"] = (142, 225), ["char-liuqi"] = (170, 209), ["char-liucong"] = (185, 209),
                ["char-xiahoushi"] = (180, 240), ["char-fuhou"] = (180, 214),
                // 扩充批 6 — 魏晋近臣/宿将
                ["char-niujin"] = (175, 240), ["char-shihuan"] = (160, 209), ["char-wangbi"] = (160, 218),
                ["char-zhaoyan"] = (171, 245), ["char-dumu"] = (165, 240), ["char-zhangji-wei"] = (165, 223),
                ["char-suze"] = (170, 223), ["char-hanji"] = (159, 238), ["char-gaotanglong"] = (175, 237),
                ["char-liufang"] = (170, 250), ["char-sunzi"] = (170, 251), ["char-zhongyu"] = (210, 263),
                ["char-xunyi"] = (205, 274), ["char-wangsu"] = (195, 256), ["char-dingmi"] = (190, 249),
                ["char-biyang"] = (190, 249), ["char-lisheng"] = (195, 249), ["char-wangguan"] = (170, 260),
                ["char-chenjiao"] = (165, 237), ["char-dianman"] = (185, 240), ["char-xuyi"] = (200, 263),
                ["char-panghui"] = (210, 270),
                // 扩充批 6 — 蜀汉文武
                ["char-huangquan"] = (170, 240), ["char-liuba"] = (185, 222), ["char-zongyu"] = (190, 264),
                ["char-xujing"] = (150, 222), ["char-mengguang"] = (170, 250), ["char-yinmo"] = (165, 240),
                ["char-laijiang"] = (165, 261), ["char-yangxi"] = (190, 261), ["char-liuyong"] = (210, 270),
                ["char-liuli"] = (214, 244), ["char-chenshi"] = (170, 235),
                // 扩充批 6 — 东吴宗室/文武
                ["char-zhangwen"] = (193, 230), ["char-yanjun"] = (165, 240), ["char-zhaozi"] = (165, 235),
                ["char-lukai"] = (198, 269), ["char-zhuzhi"] = (156, 224), ["char-shiji"] = (210, 270),
                ["char-sunben"] = (165, 210), ["char-sunjiao"] = (180, 219), ["char-sunyu"] = (177, 215),
                ["char-sundeng"] = (209, 241), ["char-sunhe"] = (224, 253), ["char-xuegong"] = (176, 243),
                ["char-zhouchu"] = (236, 297),
                // 扩充批 6 — 巾帼
                ["char-bufuren"] = (190, 238), ["char-panshu"] = (210, 252),
                // 扩充批 6 — 董卓/吕布残部
                ["char-niufu"] = (155, 192), ["char-dongmin"] = (155, 192), ["char-songxian"] = (165, 200),
                ["char-weixu"] = (165, 205), ["char-houcheng"] = (165, 200), ["char-caoxing"] = (165, 198),
                ["char-haomeng"] = (165, 196), ["char-yanxing"] = (170, 215),
                // 扩充批 6 — 袁绍集团
                ["char-chunyuqiong"] = (150, 200), ["char-guotu"] = (155, 205), ["char-fengji"] = (155, 202),
                ["char-xinping"] = (155, 205), ["char-gaogan"] = (165, 206), ["char-yuanxi"] = (178, 207),
                ["char-lvkuang"] = (165, 205), ["char-lvxiang"] = (165, 205),
                // 扩充批 6 — 刘表/刘璋部将
                ["char-liupan"] = (170, 215), ["char-zhangyun"] = (165, 208), ["char-songzhong"] = (155, 208),
                ["char-wangwei"] = (165, 208), ["char-lengbao"] = (165, 213), ["char-dengxian"] = (165, 213),
                ["char-gaopei"] = (165, 212), ["char-yanghuai"] = (165, 212), ["char-zhengdu"] = (165, 235),
                ["char-wanglei"] = (155, 211), ["char-liugui"] = (165, 213),
                // 扩充批 6 — 黄巾/异族/辽东
                ["char-liupi"] = (155, 201), ["char-gongdu"] = (155, 201), ["char-peiyuanshao"] = (160, 200),
                ["char-budugen"] = (170, 233), ["char-gongsunyuan"] = (200, 238), ["char-maxiu"] = (190, 212),
                ["char-machie"] = (192, 212),
                // 扩充批 6 — 汉末名士/方外
                ["char-zhengxuan"] = (127, 200), ["char-zhangzhongjing"] = (150, 219), ["char-pangdegong"] = (130, 200),
                ["char-xushao"] = (150, 195), ["char-huangwan"] = (141, 192), ["char-fuwan"] = (150, 209),
                ["char-zhaoqi"] = (108, 201), ["char-liuyao"] = (156, 197), ["char-wangkuang"] = (155, 192),
                ["char-huzhao"] = (161, 250), ["char-guanlu"] = (209, 256),
                // 扩充批 7 — 魏晋
                ["char-xiahouhui"] = (200, 235), ["char-xiahourong"] = (209, 219), ["char-xiahouwei"] = (195, 260),
                ["char-caoxi"] = (200, 249), ["char-linghuyu"] = (190, 249), ["char-changdiao"] = (175, 222),
                ["char-simawang"] = (205, 271), ["char-simazhou"] = (227, 283), ["char-simagan"] = (232, 311),
                ["char-wangchen"] = (210, 266), ["char-caofang"] = (231, 274), ["char-caomao"] = (241, 260),
                ["char-caohuan"] = (246, 302), ["char-wangjing"] = (200, 260), ["char-hufen"] = (205, 288),
                ["char-simayan"] = (236, 290), ["char-xunxu"] = (220, 289), ["char-caochun"] = (170, 210),
                ["char-hulie"] = (220, 270), ["char-qianhong"] = (210, 271),
                // 扩充批 7 — 蜀
                ["char-zhugeshang"] = (244, 263), ["char-huangchengyan"] = (145, 215), ["char-pengyang"] = (178, 214),
                ["char-xianglang"] = (167, 247), ["char-donghe"] = (160, 221), ["char-huanghao"] = (210, 264),
                ["char-chenzhi"] = (200, 258), ["char-zhangshao"] = (210, 270), ["char-yanghong"] = (168, 228),
                ["char-lvyi"] = (178, 251), ["char-duqiong"] = (170, 250), ["char-huji"] = (190, 260),
                // 扩充批 7 — 吴
                ["char-lvdai"] = (161, 256), ["char-quanji"] = (220, 258), ["char-hulzong"] = (183, 243),
                ["char-shiyi"] = (178, 250), ["char-luyi"] = (200, 260), ["char-luji"] = (187, 219),
                ["char-sunkuang"] = (185, 208), ["char-sunshao"] = (188, 241), ["char-wufan"] = (170, 226),
                ["char-liulve"] = (205, 260), ["char-buchan"] = (220, 272), ["char-quanshang"] = (200, 258),
                ["char-sunlang"] = (190, 230), ["char-weizhao"] = (204, 273), ["char-huagai"] = (219, 278),
                // 扩充批 7 — 群雄/凉州
                ["char-quyi"] = (160, 199), ["char-caobao"] = (155, 196), ["char-hanmeng"] = (165, 205),
                ["char-jiangyiqu"] = (160, 205), ["char-hujzhen"] = (155, 192), ["char-yangding"] = (155, 195),
                ["char-duanwei"] = (150, 209), ["char-liyue"] = (155, 200), ["char-hucai"] = (155, 196),
                ["char-guanjing"] = (160, 199), ["char-zoudan"] = (160, 193), ["char-jiaochu"] = (165, 207),
                ["char-yuanyao"] = (155, 192), ["char-liuxun"] = (160, 215), ["char-leibo"] = (165, 199),
                ["char-chenlan"] = (165, 209), ["char-songjian"] = (150, 214),
                // 扩充批 7 — 过五关六将/吕布八健将
                ["char-kongxiu"] = (165, 200), ["char-hanfuluo"] = (165, 200), ["char-mengtan"] = (165, 200),
                ["char-bianxi"] = (165, 200), ["char-wangzhi"] = (165, 200), ["char-qinqi"] = (165, 200),
                ["char-chenglian"] = (165, 198),
                // 扩充批 7 — 巾帼
                ["char-guozhao"] = (184, 235), ["char-xinxianying"] = (191, 269), ["char-caojie"] = (196, 260),
                ["char-wangyi"] = (180, 230),
                // 扩充批 7 — 羌蛮/南中
                ["char-yueji"] = (180, 229), ["char-cheliji"] = (180, 229), ["char-ahuinan"] = (180, 225),
                ["char-jinhuansanjie"] = (180, 225), ["char-mangyachang"] = (180, 225), ["char-yongkai"] = (180, 225),
                ["char-gaoding"] = (180, 225), ["char-zhubao"] = (180, 225),
                // 扩充批 7 — 建安七子/荆南太守/谋士
                ["char-wangcan"] = (177, 217), ["char-ruanyu"] = (165, 212), ["char-liuzhen"] = (180, 217),
                ["char-liudu"] = (165, 215), ["char-zhaofan"] = (165, 215), ["char-jinxuan"] = (165, 209),
                ["char-fushiren"] = (170, 222), ["char-xunchen"] = (160, 200), ["char-xuyou"] = (150, 204),
            };

        // ---- 190 讨董布防（GDD_026 D4）：部将 → 任职城（须属该城 190 归属势力）。君主不入（本身即势力之主）。----
        // 未及冠/未生者（machao/zhugeliang/simayi/luxun/lvmeng/jiangwei/pangtong/zhouyu/ganning…）此年不布防，留待后续锚点年。
        private static readonly IReadOnlyDictionary<string, string> Placement190 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-guanyu"] = "city-xiaopei", ["char-zhangfei"] = "city-xiaopei",   // 刘备·小沛
                ["char-zhaoyun"] = "city-beiping",                                       // 公孙瓒·北平
                ["char-huangzhong"] = "city-xiangyang", ["char-weiyan"] = "city-xiangyang", // 刘表·襄阳
                ["char-xiahoudun"] = "city-chenliu", ["char-caoren"] = "city-chenliu",   // 曹操·陈留
                ["char-dianwei"] = "city-chenliu", ["char-xunyu"] = "city-chenliu",
                ["char-guojia"] = "city-chenliu", ["char-chengong"] = "city-chenliu",
                ["char-zhangliao"] = "city-changan", ["char-jiaxu"] = "city-changan",     // 李傕·长安（董卓系）
                ["char-yanliang"] = "city-ye", ["char-wenchou"] = "city-ye",             // 袁绍·邺城
                ["char-gaoshun"] = "city-xiapi",                                          // 吕布·下邳
                ["char-huaxiong"] = "city-hulao",                                         // 袁术·虎牢关
                ["char-taishici"] = "city-beihai",                                        // 孔融·北海
                ["char-lusu"] = "city-jianye", ["char-huanggai"] = "city-jianye",        // 孙氏·建业
                // 扩充批 1 布防
                ["char-zhanghe"] = "city-ye", ["char-tianfeng"] = "city-ye",             // 袁绍·邺城
                ["char-jushou"] = "city-ye", ["char-shenpei"] = "city-ye", ["char-gaolan"] = "city-ye",
                ["char-dongzhuo"] = "city-luoyang", ["char-liru"] = "city-luoyang",       // 董卓系·洛阳
                ["char-chengpu"] = "city-jianye", ["char-handang"] = "city-jianye", ["char-zhoutai"] = "city-jianye",
                ["char-jiling"] = "city-runan",                                           // 袁术·汝南
                ["char-yanyan"] = "city-jiangzhou",                                       // 刘璋·江州
                ["char-huangzu"] = "city-jiangxia", ["char-caimao"] = "city-jiangling",   // 刘表
                ["char-zangba"] = "city-xuzhou",                                          // 吕布·徐州
                ["char-pangde"] = "city-xiliang", ["char-madai"] = "city-xiliang",        // 马腾·西凉
                // 加密（190 各州具名布防）
                ["char-zhangren"] = "city-chengdu", ["char-liugui"] = "city-chengdu",     // 刘焉·益州
                ["char-jiling"] = "city-shouchun",                                        // 袁术·寿春
                ["char-liupan"] = "city-changsha",                                        // 刘表·长沙
                ["char-kuailiang"] = "city-xiangyang", ["char-kuaiyue"] = "city-xiangyang", ["char-songzhong"] = "city-xiangyang", // 刘表谋士
                ["char-quyi"] = "city-ye", ["char-chunyuqiong"] = "city-ye",              // 袁绍·邺城
                ["char-guanjing"] = "city-beiping", ["char-zoudan"] = "city-beiping",     // 公孙瓒·北平
                ["char-hanmeng"] = "city-nanpi",                                          // 袁绍·南皮
            };

        // ---- 200 官渡布防（ADR-0015 离散快照）：曹袁对峙，刘备寄汝南，孙氏据江东。----
        private static readonly IReadOnlyDictionary<string, string> Placement200 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-xunyu"] = "city-xuchang", ["char-guojia"] = "city-xuchang", ["char-xuchu"] = "city-xuchang", // 曹操·许昌
                ["char-zhangliao"] = "city-xiapi", ["char-caoren"] = "city-xuchang",
                ["char-yanliang"] = "city-ye", ["char-wenchou"] = "city-ye", ["char-tianfeng"] = "city-ye",         // 袁绍·邺城
                ["char-jushou"] = "city-ye", ["char-shenpei"] = "city-ye", ["char-zhanghe"] = "city-ye", ["char-gaolan"] = "city-ye",
                ["char-guanyu"] = "city-runan", ["char-zhangfei"] = "city-runan", ["char-zhaoyun"] = "city-runan",  // 刘备·汝南
                ["char-zhouyu"] = "city-jianye", ["char-taishici"] = "city-jianye", ["char-huanggai"] = "city-jianye", // 孙氏·建业
            };

        // ---- 208 赤壁布防（ADR-0015）：曹操并北取荆，孙权据江东联刘，孔明关张随刘备据荆南。----
        private static readonly IReadOnlyDictionary<string, string> Placement208 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-xunyu"] = "city-xuchang", ["char-jiaxu"] = "city-xuchang",                                   // 曹操·许昌
                ["char-caoren"] = "city-jiangling", ["char-xuhuang"] = "city-xiangyang", ["char-yuejin"] = "city-xiangyang", // 曹操·新得荆州
                ["char-zhangliao"] = "city-ye", ["char-zhanghe"] = "city-ye",
                ["char-zhouyu"] = "city-jianye", ["char-lusu"] = "city-jianye", ["char-lvmeng"] = "city-jianye",   // 孙权·建业
                ["char-ganning"] = "city-jiangxia", ["char-huanggai"] = "city-jiangxia",                           // 孙权·夏口
                ["char-guanyu"] = "city-changsha", ["char-zhangfei"] = "city-changsha",                            // 刘备·荆南
                ["char-zhaoyun"] = "city-changsha", ["char-zhugeliang"] = "city-changsha",
                ["char-zhangren"] = "city-chengdu", ["char-fazheng"] = "city-chengdu",                             // 刘璋·成都
                // 加密
                ["char-zhanghe"] = "city-ye", ["char-yujin"] = "city-ye",                                          // 曹操·河北
                ["char-wenpin"] = "city-xiangyang",                                                                // 曹操·荆州降将
                ["char-lingtong"] = "city-jianye", ["char-xusheng"] = "city-jianye",                               // 孙权·江东
            };

        // ---- 220 三国鼎立布防（ADR-0015）：魏据中原，蜀跨益汉中，吴有江东荆南。----
        private static readonly IReadOnlyDictionary<string, string> Placement220 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-simayi"] = "city-xuchang", ["char-caoren"] = "city-xuchang", ["char-xuhuang"] = "city-xiangyang", // 魏·许昌/襄阳
                ["char-zhanghe"] = "city-changan", ["char-guohuai"] = "city-changan", ["char-caozhen"] = "city-changan", // 魏·关中
                ["char-zhugeliang"] = "city-chengdu", ["char-zhaoyun"] = "city-chengdu", ["char-jiangwan"] = "city-chengdu", // 蜀·成都
                ["char-weiyan"] = "city-hanzhong", ["char-wangping"] = "city-hanzhong", ["char-madai"] = "city-hanzhong",    // 蜀·汉中
                ["char-luxun"] = "city-jianye", ["char-zhuran"] = "city-jianye", ["char-zhugejin"] = "city-jianye",     // 吴·建业
                ["char-handang"] = "city-jiangling", ["char-panzhang"] = "city-jiangling",                             // 吴·江陵（219 夺荆）
                // 加密
                ["char-zhangliao"] = "city-shouchun", ["char-manchong"] = "city-shouchun",                             // 魏·淮南拒吴
                ["char-caohong"] = "city-luoyang", ["char-xuchu"] = "city-luoyang",                                    // 魏·洛阳宿卫
                ["char-jiaxu"] = "city-xuchang",                                                                       // 魏·许昌
            };

        // ---- 184 黄巾之乱布防（ADR-0015）：汉庭三将平乱，黄巾据河北，孙坚讨贼。----
        private static readonly IReadOnlyDictionary<string, string> Placement184 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-huangfusong"] = "city-luoyang", ["char-luzhi"] = "city-luoyang", ["char-caocao"] = "city-luoyang", // 汉庭·讨黄巾三将
                ["char-zhujun"] = "city-luoyang",
                ["char-dongzhuo"] = "city-changan", ["char-jiaxu"] = "city-changan",                                    // 凉州董卓系
                ["char-zhangjiao"] = "city-ye", ["char-zhangbao-turban"] = "city-ye", ["char-zhangliang-turban"] = "city-ye", // 黄巾·冀州
                ["char-bocai"] = "city-runan", ["char-zhangmancheng"] = "city-nanpi",                                   // 黄巾渠帅
                ["char-gongsun"] = "city-beiping",                                                                      // 幽州公孙瓒
                ["char-sunjian"] = "city-jianye", ["char-chengpu"] = "city-jianye", ["char-huanggai"] = "city-jianye",  // 孙坚讨贼
                ["char-hansui"] = "city-xiliang", ["char-biansang"] = "city-xiliang", ["char-mateng"] = "city-xiliang", // 凉州之乱
                ["char-liuyan"] = "city-chengdu", ["char-zhangren"] = "city-chengdu",                                   // 益州刘焉
            };

        // ---- 234 五丈原布防（ADR-0015）：诸葛北伐驻汉中，司马拒关中，陆逊镇江东。----
        private static readonly IReadOnlyDictionary<string, string> Placement234 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-zhugeliang"] = "city-hanzhong", ["char-weiyan"] = "city-hanzhong", ["char-wangping"] = "city-hanzhong", // 蜀·北伐前线
                ["char-jiangwei"] = "city-hanzhong",
                ["char-liushan"] = "city-chengdu", ["char-jiangwan"] = "city-chengdu", ["char-feiyi"] = "city-chengdu",   // 蜀·成都
                ["char-mazhong"] = "city-chengdu",
                ["char-simayi"] = "city-changan", ["char-guohuai"] = "city-changan", ["char-xiahouba"] = "city-changan", // 魏·拒关中
                ["char-caorui"] = "city-xuchang", ["char-manchong"] = "city-xuchang", ["char-jiangji"] = "city-xuchang", // 魏·许昌
                ["char-luxun"] = "city-jianye", ["char-zhuran"] = "city-jianye", ["char-quancong"] = "city-jianye",     // 吴·建业
            };

        // ---- 194 兖州之战布防（ADR-0015）：曹操困守鄄城，吕布陈宫据濮阳，刘备保徐州。----
        private static readonly IReadOnlyDictionary<string, string> Placement194 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-xunyu"] = "city-juancheng", ["char-guojia"] = "city-juancheng", ["char-xiahoudun"] = "city-juancheng", // 曹操·鄄城
                ["char-dianwei"] = "city-juancheng", ["char-caoren"] = "city-juancheng",
                ["char-lubu"] = "city-puyang", ["char-chengong"] = "city-puyang", ["char-gaoshun"] = "city-puyang",  // 吕布·濮阳
                ["char-zhangliao"] = "city-puyang", ["char-zangba"] = "city-puyang",
                ["char-guanyu"] = "city-xiapi", ["char-zhangfei"] = "city-xiapi",                                    // 刘备·徐州
                ["char-yanliang"] = "city-ye", ["char-wenchou"] = "city-ye", ["char-quyi"] = "city-ye",             // 袁绍·邺城
                ["char-jushou"] = "city-ye", ["char-tianfeng"] = "city-ye",
                ["char-sunce"] = "city-jianye", ["char-zhouyu"] = "city-jianye", ["char-huanggai"] = "city-jianye", // 孙策·江东
                ["char-lijue"] = "city-changan", ["char-guosi"] = "city-changan", ["char-jiaxu"] = "city-changan",  // 李傕·长安挟天子
                ["char-jiling"] = "city-shouchun",                                                                  // 袁术·寿春
            };

        // ---- 219 襄樊之战布防（ADR-0015）：关羽围樊北伐，曹仁守樊、徐晃救援，吕蒙陆逊图荆。----
        private static readonly IReadOnlyDictionary<string, string> Placement219 =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["char-guanyu"] = "city-jiangling", ["char-guanping"] = "city-jiangling", ["char-zhoucang"] = "city-jiangling", // 关羽·江陵
                ["char-liaohua"] = "city-jiangling",
                ["char-zhangfei"] = "city-hanzhong", ["char-machao"] = "city-hanzhong", ["char-huangzhong"] = "city-hanzhong",  // 蜀·新取汉中
                ["char-weiyan"] = "city-hanzhong",
                ["char-zhugeliang"] = "city-chengdu", ["char-fazheng"] = "city-chengdu", ["char-zhaoyun"] = "city-chengdu",     // 蜀·成都
                ["char-caoren"] = "city-xiangyang", ["char-manchong"] = "city-xiangyang",                                       // 曹操·樊城
                ["char-xuhuang"] = "city-xuchang", ["char-simayi"] = "city-xuchang", ["char-zhangliao"] = "city-xuchang",       // 曹操·许都（徐晃救樊）
                ["char-lvmeng"] = "city-jianye", ["char-luxun"] = "city-jianye", ["char-lusu"] = "city-jianye",                // 孙权·白衣渡江
            };

        /// <summary>某锚点年的布防表（ADR-0015 离散快照）；未登记年返回 null。</summary>
        private static IReadOnlyDictionary<string, string>? PlacementFor(int anchorYear) => anchorYear switch
        {
            184 => Placement184,
            190 => Placement190,
            194 => Placement194,
            200 => Placement200,
            208 => Placement208,
            219 => Placement219,
            220 => Placement220,
            234 => Placement234,
            _ => null,
        };

        /// <summary>某武将生卒年（公元）；未登记则 null（不受生卒约束，视为常在）。</summary>
        public static (int Birth, int Death)? LifeOf(CharacterId id)
            => id.Value != null && LifeYears.TryGetValue(id.Value, out (int, int) y) ? y : ((int, int)?)null;

        /// <summary>某武将在某公元年是否在世且已及冠出仕（GDD_026 F4）；无生卒登记者视为常在。</summary>
        public static bool AvailableAt(CharacterId id, int year, int serviceMinAge = 16)
        {
            (int Birth, int Death)? life = LifeOf(id);
            if (life == null) return true;
            return life.Value.Birth + serviceMinAge <= year && year <= life.Value.Death;
        }

        /// <summary>某锚点年全部布防对（部将→任职城；未过滤在世，供地图投影自行按当前年过滤）。190/200/208/220 有数据。</summary>
        public static IReadOnlyList<(CharacterId General, CityId City)> AllPlacements(int anchorYear)
        {
            var result = new List<(CharacterId, CityId)>();
            IReadOnlyDictionary<string, string>? table = PlacementFor(anchorYear);
            if (table == null) return result;
            foreach (KeyValuePair<string, string> kv in table)
                result.Add((new CharacterId(kv.Key), new CityId(kv.Value)));
            result.Sort((a, b) => string.CompareOrdinal(a.Item1.Value, b.Item1.Value));
            return result;
        }

        /// <summary>某锚点年、某城在职的部将（GDD_026 R4；反全知外壳另投影）。190/200/208/220 有布防数据，余年返回空。</summary>
        public static IReadOnlyList<CharacterId> GeneralsAt(CityId city, int anchorYear)
        {
            var result = new List<CharacterId>();
            IReadOnlyDictionary<string, string>? table = PlacementFor(anchorYear);
            if (table == null || city.Value == null) return result;
            foreach (KeyValuePair<string, string> kv in table)
            {
                if (kv.Value != city.Value) continue;
                var id = new CharacterId(kv.Key);
                if (AvailableAt(id, anchorYear)) result.Add(id);
            }
            result.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));
            return result;
        }

        private static IReadOnlyDictionary<string, GeneralDossier> Build()
        {
            var list = new List<GeneralDossier>();
            void D(string id, CombatTier tier, StrategyTier strat, LoyaltyLeaning loy, Ambition amb, params GeneralTag[] tags)
                => list.Add(new GeneralDossier(new CharacterId(id), tags, loy, amb, tier, strat));

            // ---- 君主（忠于己，野心分方面/问鼎）----
            D("char-caocao", CombatTier.Sturdy, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Strategist, GeneralTag.Bloodthirsty);
            D("char-liubei", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Benevolent);
            D("char-sunce", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Reckless, GeneralTag.Cavalry);
            D("char-sunquan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);
            D("char-yuanshao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Stubborn);
            D("char-yuan", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Arrogant, GeneralTag.Hesitant);
            D("char-lubu", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless, GeneralTag.Fickle, GeneralTag.Cavalry);
            D("char-liubiao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant, GeneralTag.Benevolent);
            D("char-liuzhang", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);
            D("char-mateng", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-zhanglu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Defender);
            D("char-gongsun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-lijue", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Bloodthirsty, GeneralTag.Fickle);
            D("char-zhangxiu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-kongrong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent, GeneralTag.Arrogant);
            D("char-hansui", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-shixie", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-player-lord", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Defender);

            // ---- 蜀汉 ----
            D("char-guanyu", CombatTier.Peerless, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Awe, GeneralTag.IronBones, GeneralTag.Arrogant);
            D("char-zhangfei", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless, GeneralTag.Bloodthirsty);
            D("char-zhaoyun", CombatTier.Peerless, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-zhugeliang", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-machao", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry, GeneralTag.Bloodthirsty);
            D("char-huangzhong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-weiyan", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-pangtong", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-jiangwei", CombatTier.Valiant, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist);

            // ---- 曹魏 ----
            D("char-xiahoudun", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-zhangliao", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.NightRaider, GeneralTag.Defender);
            D("char-xuchu", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-dianwei", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-guojia", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning, GeneralTag.Strategist);
            D("char-xunyu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-simayi", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Strategist, GeneralTag.Wolflook);
            D("char-caoren", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-jiaxu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);

            // ---- 孙吴 ----
            D("char-zhouyu", CombatTier.Sturdy, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-lusu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-lvmeng", CombatTier.Valiant, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.Cunning);
            D("char-luxun", CombatTier.Sturdy, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval, GeneralTag.Strategist);
            D("char-taishici", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-ganning", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Naval, GeneralTag.NightRaider);
            D("char-huanggai", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);

            // ---- 群雄部将 ----
            D("char-yanliang", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-wenchou", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-gaoshun", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-chengong", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-huaxiong", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-haozhao", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);

            // ==== 演义名将扩充批 1（2026-07-05）：公版人物 + 原创标签/档，无任何商业数值。====
            // ---- 曹魏 ----
            D("char-xiahouyuan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry, GeneralTag.Reckless);
            D("char-zhanghe", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cavalry, GeneralTag.Cunning);
            D("char-xuhuang", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-yujin", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Defender);
            D("char-yuejin", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-lidian", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-caohong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-caozhang", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cavalry, GeneralTag.Reckless);
            D("char-caopi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Wolflook);
            D("char-caozhi", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-dengai", CombatTier.Valiant, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Strategist, GeneralTag.Defender);
            D("char-zhonghui", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Wolflook);
            D("char-guohuai", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-caozhen", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-pangde", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor, GeneralTag.Cavalry);
            D("char-wenpin", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender, GeneralTag.Naval);
            D("char-chengyu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning, GeneralTag.Strategist);
            D("char-simashi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Strategist, GeneralTag.Wolflook);
            D("char-simazhao", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Strategist, GeneralTag.Wolflook);
            D("char-huaxin", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);
            D("char-manchong", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender, GeneralTag.Cunning);

            // ---- 蜀汉 ----
            D("char-guanping", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.IronBones);
            D("char-zhoucang", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-liaohua", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-madai", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);
            D("char-wangping", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-guanxing", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.IronBones);
            D("char-zhangbao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-yanyan", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.IronBones, GeneralTag.Defender);
            D("char-fazheng", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-masu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Strategist, GeneralTag.Arrogant);
            D("char-jiangwan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-feiyi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-liyan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Defender);
            D("char-liufeng", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);

            // ---- 孙吴 ----
            D("char-chengpu", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-handang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-zhoutai", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor, GeneralTag.IronBones);
            D("char-jiangqin", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-chenwu", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-lingtong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor, GeneralTag.Naval);
            D("char-xusheng", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender, GeneralTag.Naval);
            D("char-dingfeng", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.NightRaider);
            D("char-panzhang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Bloodthirsty);
            D("char-zhuran", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender, GeneralTag.Naval);
            D("char-zhugejin", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-zhangzhao", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Stubborn, GeneralTag.Strategist);
            D("char-zhugeke", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Arrogant);
            D("char-sunshangxiang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);

            // ---- 群雄 / 董卓系 / 黄巾 ----
            D("char-dongzhuo", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Bloodthirsty, GeneralTag.Arrogant);
            D("char-liru", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            D("char-zhangjiao", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Cunning);
            D("char-jiling", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);
            D("char-tianfeng", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Stubborn);
            D("char-jushou", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-shenpei", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Stubborn, GeneralTag.Defender);
            D("char-gaolan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cavalry);
            D("char-caimao", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Naval, GeneralTag.Fickle);
            D("char-huangzu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Naval);
            D("char-zangba", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);

            // ==== 演义名将扩充批 2（2026-07-05）====
            // ---- 蜀汉（文吏/后进）----
            D("char-sunqian", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-mizhu", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-mifang", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Fickle);
            D("char-jianyong", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-yiji", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            D("char-maliang", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-dengzhi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            D("char-zhangyi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-zhangni", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-wuyi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);
            D("char-huojun", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-mengda", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-guansuo", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.IronBones);
            D("char-zhugezhan", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.IronBones);
            D("char-fuqian", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.IronBones);

            // ---- 曹魏（后期）----
            D("char-caoxiu", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);
            D("char-caoshuang", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Arrogant, GeneralTag.Hesitant);
            D("char-wangshuang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-tianyu", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-qianzhao", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-chentai", CombatTier.Valiant, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-wangji", CombatTier.Sturdy, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-jiachong", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Wolflook);
            D("char-wangjun", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Naval);
            D("char-yanghu", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent);
            D("char-duyu", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-jiangji", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            D("char-liuye", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning, GeneralTag.Strategist);
            D("char-chenqun", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-zhongyao", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-caorui", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Cunning);

            // ---- 孙吴 ----
            D("char-sunjian", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Reckless, GeneralTag.Cavalry);
            D("char-dongxi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-machongwu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-zhuhuan", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-quancong", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Naval);
            D("char-buzhi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-guyong", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent, GeneralTag.Strategist);
            D("char-zhanghong", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);
            D("char-kanze", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            D("char-lukang", CombatTier.Valiant, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Defender);

            // ---- 群雄 / 南蛮 / 方士 ----
            D("char-huatuo", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-zuoci", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Disloyal, Ambition.None, GeneralTag.Cunning);
            D("char-yuji", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-zhangbao-turban", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Cunning);
            D("char-zhangliang-turban", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Reckless);
            D("char-hejin", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.Grand, GeneralTag.Arrogant, GeneralTag.Hesitant);
            D("char-menghuo", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-zhurong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cavalry);
            D("char-shamoke", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Reckless, GeneralTag.Bloodthirsty);
            D("char-zhangren", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-hanxuan", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);
            D("char-gongsunkang", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-tadun", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-kebineng", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-yanbaihu", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);

            // ==== 演义名将扩充批 3（2026-07-05）：名士谋臣/女性/南蛮/魏晋末/西凉/讨董副将 ====
            // ---- 名士·谋臣 ----
            D("char-xushu", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);   // 徐庶·走马荐诸葛
            D("char-xunyou", CombatTier.Ordinary, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist, GeneralTag.Cunning); // 荀攸·曹操谋主
            D("char-simahui", CombatTier.Feeble, StrategyTier.Master, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Strategist, GeneralTag.Benevolent); // 水镜先生
            D("char-cuizhouping", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Strategist);
            D("char-zhangsong", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cunning);   // 献益州图
            D("char-lihui", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);           // 说降马超
            D("char-qiaozhou", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Hesitant);      // 劝降
            D("char-kuailiang", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Strategist);
            D("char-kuaiyue", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);
            // ---- 女性 ----
            D("char-diaochan", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cunning);       // 连环计
            D("char-daqiao", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-xiaoqiao", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-zhenji", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);
            D("char-caiwenji", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);     // 才女
            D("char-huangyueying", CombatTier.Feeble, StrategyTier.Master, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);  // 善机巧
            D("char-wangyuanji", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);
            D("char-zhangchunhua", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);
            // ---- 南蛮 / 异族 ----
            D("char-wutugu", CombatTier.Peerless, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.IronBones);      // 藤甲兵
            D("char-mulu", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Reckless);          // 驱兽
            D("char-duosi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cunning);          // 毒泉
            D("char-dongtuna", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cavalry);
            // ---- 魏晋末 ----
            D("char-wenyang", CombatTier.Peerless, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor, GeneralTag.Reckless); // 单骑退雄兵
            D("char-wenqin", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-zhugedan", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Stubborn);
            D("char-guanqiujian", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Defender);
            D("char-wangling", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-weiguan", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Wolfish, GeneralTag.Cunning, GeneralTag.Wolflook);
            D("char-dengzhong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);        // 邓艾子
            // ---- 西凉（韩遂部）----
            D("char-chengyi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-liangxing", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-houxuan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-yangqiu", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-mawan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            // ---- 讨董/群雄副将 ----
            D("char-panfeng", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);          // 上将潘凤
            D("char-wuanguo", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-xingdaorong", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Reckless);
            D("char-leitong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-wulan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);

            // ---- 扩充批 4 · 汉末朝堂/讨董名臣 ----
            D("char-wangyun", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cunning);       // 连环计诛董
            D("char-luzhi", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);           // 名将名儒·刘备之师
            D("char-huangfusong", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);      // 平黄巾名将
            D("char-zhujun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-caiyong", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);       // 文豪
            D("char-dongcheng", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring);                        // 衣带诏

            // ---- 扩充批 4 · 魏晋文武 ----
            D("char-caoang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                                 // 曹操长子·宛城殁
            D("char-yangxiu", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Arrogant);     // 恃才
            D("char-cuiyan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Stubborn);          // 刚正
            D("char-maojie", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);        // 清廉
            D("char-dongzhao", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-wanglang", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);
            D("char-chenlin", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                              // 建安七子·檄文
            D("char-miheng", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Arrogant);     // 击鼓骂曹
            D("char-lvqian", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-hanhao", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-simafu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Strategist);      // 晋室元老
            D("char-huanfan", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);          // 智囊
            D("char-jianggan", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);                             // 盗书中计
            D("char-qinlang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);
            D("char-guanning", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);      // 割席隐士

            // ---- 扩充批 4 · 蜀汉后进 ----
            D("char-chendao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);          // 白毦兵
            D("char-dongyun", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);       // 四相之一
            D("char-yangyi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Arrogant);   // 狭隘
            D("char-xiangchong", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);        // 出师表所荐
            D("char-liaoli", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Arrogant);   // 自负废徙
            D("char-fengxi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                                 // 夷陵殁
            D("char-futong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);          // 断后死战
            D("char-wuban", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-gaoxiang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-lvkai", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);           // 永昌拒叛
            D("char-wangfu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);

            // ---- 扩充批 4 · 东吴中生代 ----
            D("char-lvfan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-yufan", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);         // 狂直
            D("char-zhoufang", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);         // 断发赚曹休
            D("char-heqi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-luotong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-zhuyi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-sunhuan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                               // 少年安东将军
            D("char-sunyi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Reckless);        // 骁悍类兄
            D("char-panjun", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-sunjun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Bloodthirsty, GeneralTag.Fickle);  // 权臣
            D("char-sunchen", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Bloodthirsty, GeneralTag.Fickle);

            // ---- 扩充批 4 · 群雄/汉末诸侯 ----
            D("char-chendeng", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Cunning);     // 智擒吕布
            D("char-chengui", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);
            D("char-hanfu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);         // 让冀州
            D("char-zhangmiao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);  // 叛迎吕布
            D("char-zhangyang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);
            D("char-baoxin", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                                 // 首识曹操
            D("char-liudai", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);
            D("char-yuantan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Grand, GeneralTag.Stubborn);       // 袁绍长子
            D("char-yuanshang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Grand);                           // 袁绍幼子
            D("char-zhangyan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.NightRaider); // 黑山军
            D("char-guanhai", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);    // 黄巾围北海
            D("char-caiyang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);           // 追关羽被斩
            D("char-chezhou", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);                            // 徐州刺史·殁于关羽

            // ---- 扩充批 4 · 巾帼 ----
            D("char-bianfuren", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);       // 曹操继室·母仪
            D("char-ganfuren", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                               // 刘备夫人·阿斗生母
            D("char-mifuren", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                                // 长坂投井
            D("char-wuguotai", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);      // 甘露寺相婿

            // ---- 扩充批 5 · 汉末群雄/凉州/黄巾/李郭 ----
            D("char-liuyan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Grand, GeneralTag.Cunning);        // 益州牧·图自立
            D("char-liuyu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);         // 幽州牧·仁政
            D("char-taoqian", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);       // 徐州牧·三让
            D("char-gongsundu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Grand, GeneralTag.Defender);      // 辽东自守
            D("char-qiaorui", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                       // 讨董诸侯
            D("char-kongzhou", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-zhangchao", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                       // 张邈弟
            D("char-bocai", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);      // 黄巾渠帅
            D("char-zhangmancheng", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-biansang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cavalry);    // 凉州之乱
            D("char-lisu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);      // 说吕布杀丁原
            D("char-yangfeng", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-hanxian", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-zhangji", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);     // 张绣叔
            D("char-fanchou", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cavalry);     // 李郭同党
            D("char-guosi", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Bloodthirsty, GeneralTag.Fickle); // 李郭之乱
            D("char-huchier", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Reckless);        // 盗典韦戟

            // ---- 扩充批 5 · 魏（后期） ----
            D("char-jiakui", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);           // 魏之良史
            D("char-tianchou", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);     // 义士·导曹破乌桓
            D("char-liangxi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                             // 治并州
            D("char-gaorou", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                              // 执法平恕
            D("char-xinpi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);           // 持节制司马
            D("char-luyu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-xiahouba", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);  // 后降蜀
            D("char-wangchang", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-shiba", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Aspiring);                           // 晋室开国
            D("char-zhugexu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);      // 阴平失机
            D("char-hujun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);
            D("char-caoyu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                               // 曹操子·燕王

            // ---- 扩充批 5 · 蜀（后期） ----
            D("char-liushan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);      // 后主·乐不思蜀
            D("char-mazhong", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);         // 蜀·平南名将
            D("char-huoyi", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);            // 守南中
            D("char-luoxian", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);         // 孤守永安拒吴
            D("char-zhaotong", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 赵云长子
            D("char-zhaoguang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);      // 赵云次子·沓中战殁
            D("char-dongjue", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-fanjian", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-jiangshu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle);  // 开关降魏
            D("char-liuchen", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);        // 北地王·哭庙殉国

            // ---- 扩充批 5 · 吴（后期） ----
            D("char-sunhao", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Bloodthirsty, GeneralTag.Arrogant); // 吴末暴君
            D("char-sunxiu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Aspiring);                        // 诛孙綝
            D("char-sunliang", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-zhuju", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-lvju", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-liuzan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);
            D("char-tengyin", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-tangzi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                         // 降魏叛将

            // ---- 扩充批 5 · 汉臣/荆州/巾帼 ----
            D("char-yangbiao", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);       // 汉太尉·守节
            D("char-liuqi", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Hesitant);    // 刘表长子
            D("char-liucong", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);       // 刘表幼子·举州降曹
            D("char-xiahoushi", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 张飞妻·夏侯氏
            D("char-fuhou", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                                 // 伏皇后·殁于曹

            // ---- 扩充批 6 · 魏晋近臣/宿将 ----
            D("char-niujin", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                              // 曹仁骁将·殁于司马
            D("char-shihuan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 中军校尉
            D("char-wangbi", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                            // 曹操长史
            D("char-zhaoyan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-dumu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-zhangji-wei", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);   // 雍凉刺史·张既
            D("char-suze", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-hanji", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-gaotanglong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                       // 直谏之臣
            D("char-liufang", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning); // 魏中枢
            D("char-sunzi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-zhongyu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                           // 钟繇子
            D("char-xunyi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-wangsu", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);       // 经学大家
            D("char-dingmi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning); // 曹爽党·台中三狗
            D("char-biyang", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);
            D("char-lisheng", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);
            D("char-wangguan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-chenjiao", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-dianman", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None);                             // 典韦子
            D("char-xuyi", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Loyal, Ambition.None);                               // 许褚子·殁于钟会
            D("char-panghui", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);       // 庞德子·灭关氏

            // ---- 扩充批 6 · 蜀汉文武 ----
            D("char-huangquan", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);  // 智谋·后不得已降魏
            D("char-liuba", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);       // 蜀财计
            D("char-zongyu", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-xujing", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                            // 名士·虚誉
            D("char-mengguang", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);
            D("char-yinmo", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-laijiang", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);
            D("char-yangxi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                            // 季汉辅臣赞作者
            D("char-liuyong", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                           // 刘备子·鲁王
            D("char-liuli", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                            // 刘备子·梁王
            D("char-chenshi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);

            // ---- 扩充批 6 · 东吴宗室/文武 ----
            D("char-zhangwen", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);   // 才辩·遭忌废
            D("char-yanjun", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-zhaozi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                            // 使魏不辱命
            D("char-lukai", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);      // 吴末直臣
            D("char-zhuzhi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 孙氏元从
            D("char-shiji", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);            // 朱然子
            D("char-sunben", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 孙氏宗室
            D("char-sunjiao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Naval);
            D("char-sunyu", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-sundeng", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);    // 宣太子·早逝
            D("char-sunhe", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                          // 废太子
            D("char-xuegong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-zhouchu", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);       // 除三害·改过

            // ---- 扩充批 6 · 巾帼 ----
            D("char-bufuren", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);                          // 孙权步夫人
            D("char-panshu", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);   // 孙权潘后·遇弑

            // ---- 扩充批 6 · 董卓/吕布残部 ----
            D("char-niufu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                        // 董卓婿
            D("char-dongmin", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                            // 董卓弟
            D("char-songxian", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle); // 缚吕布降曹
            D("char-weixu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-houcheng", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-caoxing", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.NightRaider); // 射伤夏侯惇
            D("char-haomeng", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-yanxing", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry); // 韩遂骁将·几杀马超

            // ---- 扩充批 6 · 袁绍集团 ----
            D("char-chunyuqiong", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Reckless);  // 醉失乌巢
            D("char-guotu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);  // 谗谋误袁
            D("char-fengji", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-xinping", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-gaogan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Grand, GeneralTag.Defender);     // 袁绍甥·据并州
            D("char-yuanxi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Grand);                          // 袁绍次子
            D("char-lvkuang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);   // 降曹
            D("char-lvxiang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);

            // ---- 扩充批 6 · 刘表/刘璋部将 ----
            D("char-liupan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);         // 刘表侄·骁勇
            D("char-zhangyun", CombatTier.Ordinary, StrategyTier.Dull, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Naval);       // 荆州水军·殁于反间
            D("char-songzhong", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-wangwei", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                            // 劝袭曹之谋
            D("char-lengbao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);
            D("char-dengxian", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);
            D("char-gaopei", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);
            D("char-yanghuai", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cunning);
            D("char-zhengdu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);     // 坚壁清野之谋
            D("char-wanglei", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                          // 倒吊死谏
            D("char-liugui", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);

            // ---- 扩充批 6 · 黄巾/异族/辽东 ----
            D("char-liupi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                        // 汝南黄巾·附刘备
            D("char-gongdu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);
            D("char-peiyuanshao", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-budugen", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);  // 鲜卑
            D("char-gongsunyuan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Fickle); // 辽东僭燕
            D("char-maxiu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                              // 马腾子
            D("char-machie", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);

            // ---- 扩充批 6 · 汉末名士/方外 ----
            D("char-zhengxuan", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent);  // 经神
            D("char-zhangzhongjing", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Benevolent); // 医圣
            D("char-pangdegong", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Strategist); // 鹿门隐士·识卧龙凤雏
            D("char-xushao", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);       // 月旦评
            D("char-huangwan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-fuwan", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                              // 伏皇后父
            D("char-zhaoqi", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-liuyao", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                         // 扬州刺史·拒孙策
            D("char-wangkuang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring);                     // 讨董诸侯
            D("char-huzhao", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                           // 隐士
            D("char-guanlu", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);       // 卜筮神算

            // ---- 扩充批 7 · 魏晋 ----
            D("char-xiahouhui", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-xiahourong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);   // 定军山殉父
            D("char-xiahouwei", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-caoxi", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                      // 曹爽弟
            D("char-linghuyu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring);                   // 谋废立
            D("char-changdiao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-simawang", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);
            D("char-simazhou", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Aspiring);
            D("char-simagan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);
            D("char-wangchen", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning); // 卖曹髦
            D("char-caofang", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);    // 魏帝·被废
            D("char-caomao", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.Aspiring, GeneralTag.Reckless);    // 司马昭之心·殉
            D("char-caohuan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);    // 魏末帝·禅晋
            D("char-wangjing", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);   // 忠汉·殉曹髦
            D("char-hufen", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.Aspiring);
            D("char-simayan", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.Wolfish, GeneralTag.Wolflook); // 晋武帝·代魏
            D("char-xunxu", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.Aspiring, GeneralTag.Cunning);
            D("char-caochun", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cavalry);        // 虎豹骑督
            D("char-hulie", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-qianhong", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                          // 牵招子

            // ---- 扩充批 7 · 蜀 ----
            D("char-zhugeshang", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.LoneValor);   // 绵竹殉国
            D("char-huangchengyan", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                     // 卧龙岳丈
            D("char-pengyang", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Arrogant); // 恃才谋反被诛
            D("char-xianglang", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-donghe", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);     // 董允父·清廉
            D("char-huanghao", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Fickle);   // 蜀宦·乱政亡国
            D("char-chenzhi", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning); // 结黄皓
            D("char-zhangshao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                        // 张飞子·奉降表
            D("char-yanghong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-lvyi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-duqiong", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                          // 谶纬之学
            D("char-huji", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);

            // ---- 扩充批 7 · 吴 ----
            D("char-lvdai", CombatTier.Sturdy, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);         // 平交州·寿九十
            D("char-quanji", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-hulzong", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                          // 文檄
            D("char-shiyi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);     // 清介
            D("char-luyi", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                            // 陆逊族·直谏
            D("char-luji", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Strategist);        // 怀橘·天文历算
            D("char-sunkuang", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                          // 孙坚幼子
            D("char-sunshao", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Defender);      // 守广陵拒魏
            D("char-wufan", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Cunning);        // 术数占候
            D("char-liulve", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-buchan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish, GeneralTag.Fickle);  // 步骘子·叛降晋
            D("char-quanshang", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);
            D("char-sunlang", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);
            D("char-weizhao", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);     // 吴书·直笔
            D("char-huagai", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                          // 谏孙皓

            // ---- 扩充批 7 · 群雄/凉州 ----
            D("char-quyi", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);   // 先登破公孙·功高被诛
            D("char-caobao", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);                          // 徐州·激张飞
            D("char-hanmeng", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-jiangyiqu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-hujzhen", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                    // 董卓大督护
            D("char-yangding", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);
            D("char-duanwei", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Defender);      // 华阴自守
            D("char-liyue", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);      // 白波
            D("char-hucai", CombatTier.Sturdy, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);
            D("char-guanjing", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                        // 公孙瓒谋士·殉主
            D("char-zoudan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-jiaochu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Fickle);   // 降曹
            D("char-yuanyao", CombatTier.Ordinary, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                        // 讨董·袁氏族
            D("char-liuxun", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                      // 庐江·败于孙策
            D("char-leibo", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);  // 叛袁术为寇
            D("char-chenlan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-songjian", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Wolfish);                     // 河首平汉王·僭号

            // ---- 扩充批 7 · 过五关六将/吕布八健将 ----
            D("char-kongxiu", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                           // 东岭关
            D("char-hanfuluo", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                          // 洛阳
            D("char-mengtan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);
            D("char-bianxi", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cunning);     // 镇国寺伏杀
            D("char-wangzhi", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None, GeneralTag.Cunning);
            D("char-qinqi", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                             // 黄河渡
            D("char-chenglian", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None);                        // 吕布健将

            // ---- 扩充批 7 · 巾帼 ----
            D("char-guozhao", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning);  // 郭女王·魏文德后
            D("char-xinxianying", CombatTier.Feeble, StrategyTier.Adept, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Cunning);     // 辛毗女·料事如神
            D("char-caojie", CombatTier.Feeble, StrategyTier.Plain, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Benevolent);      // 献帝曹后·护汉
            D("char-wangyi", CombatTier.Valiant, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None, GeneralTag.Reckless);        // 赵昂妻·守城拒马超

            // ---- 扩充批 7 · 羌蛮/南中 ----
            D("char-yueji", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);   // 羌·铁车兵
            D("char-cheliji", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cavalry);  // 羌王
            D("char-ahuinan", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.Aspiring);                    // 南蛮
            D("char-jinhuansanjie", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-mangyachang", CombatTier.Valiant, StrategyTier.Dull, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Reckless);
            D("char-yongkai", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring);                   // 南中首叛
            D("char-gaoding", CombatTier.Valiant, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring);
            D("char-zhubao", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring);

            // ---- 扩充批 7 · 建安七子/荆南太守/谋士 ----
            D("char-wangcan", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Loyal, Ambition.None);                            // 七子之冠冕
            D("char-ruanyu", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None);                          // 建安七子·檄
            D("char-liuzhen", CombatTier.Feeble, StrategyTier.Sharp, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Arrogant);     // 建安七子
            D("char-liudu", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None, GeneralTag.Hesitant);     // 零陵·降刘备
            D("char-zhaofan", CombatTier.Ordinary, StrategyTier.Plain, LoyaltyLeaning.Wavering, Ambition.None);                       // 桂阳
            D("char-jinxuan", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Content, Ambition.None);                         // 武陵
            D("char-fushiren", CombatTier.Sturdy, StrategyTier.Plain, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Fickle);  // 士仁·叛降吴陷关羽
            D("char-xunchen", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Wavering, Ambition.Aspiring, GeneralTag.Cunning); // 荀彧兄·说韩馥让冀州
            D("char-xuyou", CombatTier.Ordinary, StrategyTier.Adept, LoyaltyLeaning.Disloyal, Ambition.Aspiring, GeneralTag.Cunning, GeneralTag.Arrogant); // 官渡叛袁献计·恃功被杀

            var dict = new Dictionary<string, GeneralDossier>(StringComparer.Ordinal);
            foreach (GeneralDossier d in list) dict[d.Id.Value] = d;
            return dict;
        }
    }
}
