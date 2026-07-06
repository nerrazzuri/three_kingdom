using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Application.Scenarios
{
    /// <summary>
    /// 三国武将羁绊谱（GDD_025 R4 内容）：血脉/师徒/知己/仇怨。同场触发协同或互扣（BondEffectService）。
    /// 公版人物 + 原创机制。可持续扩充；无羁绊者互不影响。
    /// </summary>
    public static class GeneralBonds
    {
        private static Bond B(string a, string b, BondType t)
            => new Bond(new CharacterId(a), new CharacterId(b), t);

        /// <summary>全体羁绊。</summary>
        public static IReadOnlyList<Bond> All { get; } = new[]
        {
            // 知己·生死之交
            B("char-liubei", "char-guanyu", BondType.Kindred),   // 桃园
            B("char-liubei", "char-zhangfei", BondType.Kindred),
            B("char-guanyu", "char-zhangfei", BondType.Kindred),
            B("char-sunce", "char-zhouyu", BondType.Kindred),    // 总角之好
            // 血脉·宗族
            B("char-caocao", "char-xiahoudun", BondType.Blood),  // 曹夏侯本家
            B("char-caocao", "char-caoren", BondType.Blood),
            B("char-caocao", "char-caohong", BondType.Blood),
            B("char-xiahoudun", "char-xiahouyuan", BondType.Blood),
            B("char-caocao", "char-caopi", BondType.Blood),
            B("char-caocao", "char-caozhi", BondType.Blood),
            B("char-sunce", "char-sunquan", BondType.Blood),
            B("char-sunjian", "char-sunce", BondType.Blood),      // 孙坚父子
            B("char-sunjian", "char-sunquan", BondType.Blood),
            B("char-guanyu", "char-guanping", BondType.Blood),
            B("char-guanyu", "char-guansuo", BondType.Blood),     // 关羽父子（演义）
            B("char-zhugeliang", "char-zhugezhan", BondType.Blood), // 诸葛父子
            B("char-zhugeliang", "char-huangyueying", BondType.Kindred), // 诸葛夫妇
            B("char-zhugeliang", "char-xushu", BondType.Kindred),   // 同窗知己
            B("char-zhouyu", "char-xiaoqiao", BondType.Kindred),    // 周瑜小乔
            B("char-sunce", "char-daqiao", BondType.Kindred),       // 孙策大乔
            B("char-dengai", "char-dengzhong", BondType.Blood),     // 邓艾父子
            B("char-wenqin", "char-wenyang", BondType.Blood),       // 文钦父子
            // 师徒·传承
            B("char-liubei", "char-zhugeliang", BondType.Mentor),   // 三顾·如鱼得水
            B("char-zhugeliang", "char-jiangwei", BondType.Mentor), // 衣钵
            // 仇怨·宿敌
            B("char-lubu", "char-dongzhuo", BondType.Feud),      // 反复弑主
            B("char-guanyu", "char-lvmeng", BondType.Feud),      // 白衣渡江
            B("char-machao", "char-caocao", BondType.Feud),      // 潼关灭族之恨
            // 扩充批 4 · 血脉
            B("char-yuanshao", "char-yuantan", BondType.Blood),  // 袁绍父子
            B("char-yuanshao", "char-yuanshang", BondType.Blood),
            B("char-caocao", "char-caoang", BondType.Blood),     // 曹操长子
            B("char-chengui", "char-chendeng", BondType.Blood),  // 陈珪陈登父子
            B("char-sunjian", "char-sunyi", BondType.Blood),     // 孙坚三子
            B("char-sunce", "char-sunyi", BondType.Blood),
            B("char-caiyong", "char-caiwenji", BondType.Blood),  // 蔡邕蔡文姬父女
            B("char-sunjun", "char-sunchen", BondType.Blood),    // 孙峻孙綝从兄弟
            // 扩充批 4 · 知己/眷属
            B("char-caocao", "char-bianfuren", BondType.Kindred),// 曹操继室
            B("char-liubei", "char-ganfuren", BondType.Kindred), // 刘备夫人
            B("char-wangyun", "char-diaochan", BondType.Kindred),// 王允义女·连环计
            // 扩充批 4 · 师徒
            B("char-luzhi", "char-liubei", BondType.Mentor),     // 卢植门下
            B("char-luzhi", "char-gongsun", BondType.Mentor),
            // 扩充批 4 · 仇怨
            B("char-yuantan", "char-yuanshang", BondType.Feud),  // 兄弟阋墙·自相图
        };

        /// <summary>取只涉及给定在场武将集的羁绊（两端皆在场）。</summary>
        public static IReadOnlyList<Bond> Among(IReadOnlyCollection<CharacterId> present)
        {
            var result = new List<Bond>();
            if (present == null || present.Count < 2) return result;
            var set = new HashSet<string>();
            foreach (CharacterId c in present) if (c.Value != null) set.Add(c.Value);
            foreach (Bond b in All)
                if (set.Contains(b.A.Value) && set.Contains(b.B.Value)) result.Add(b);
            return result;
        }
    }
}
