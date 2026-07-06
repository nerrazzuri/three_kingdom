using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Presentation.Screens;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// 武将目录（#2 / GDD_025 R1 无数值面板）：目录 infra 遍历全体档案；反全知目录卡只呈中文名 + 气质性情文字，
    /// 绝不投影统率/武力/智略之数，更不露隐藏的战阵档/谋略档与隐秘的心。玩家凭名声与性情识人。
    /// </summary>
    [TestFixture]
    public class GeneralRosterTests
    {
        [Test]
        public void test_roster_enumerates_all_dossiers_in_stable_order()
        {
            var all = GeneralDossiers.All;
            Assert.That(all.Count, Is.GreaterThanOrEqualTo(48), "名将集 ≥48。");
            // 稳定序（按 id 规范序）：可复现、便于渲染/测试。
            for (int i = 1; i < all.Count; i++)
                Assert.That(string.CompareOrdinal(all[i - 1].Id.Value, all[i].Id.Value), Is.LessThan(0), "按 id 升序、无重复。");
            Assert.That(GeneralDossiers.Find(new CharacterId("char-guanyu")), Is.Not.Null, "含关羽。");
        }

        [Test]
        public void test_roster_card_shows_traits_not_numbers()
        {
            GeneralRosterView view = GeneralRosterView.Build();
            Assert.That(view.Cards.Count, Is.EqualTo(GeneralDossiers.All.Count));

            GeneralCardView guanyu = null!;
            foreach (GeneralCardView c in view.Cards) if (c.Id == "char-guanyu") guanyu = c;
            Assert.That(guanyu, Is.Not.Null);
            Assert.That(guanyu.Name, Is.EqualTo("关羽"), "中文名经 DisplayNames。");
            // 关羽气质：威压/傲骨/傲物（GDD_025 档案）——以性情文字呈现，非数值。
            Assert.That(guanyu.Traits, Does.Contain("威压"));
            Assert.That(guanyu.Traits, Does.Contain("傲骨"));
            Assert.That(guanyu.Traits, Does.Contain("傲物"));
        }

        [Test]
        public void test_roster_expanded_with_major_generals()
        {
            Assert.That(GeneralDossiers.All.Count, Is.GreaterThanOrEqualTo(490), "名将谱已大幅扩充（≥490，七批，近 500 员）。");
            // 抽查各扩充批：档案 + 中文名 + 生卒在世判定皆到位。
            foreach (string id in new[] { "char-zhanghe", "char-masu", "char-sunjian", "char-huatuo", "char-menghuo", "char-xushu", "char-diaochan", "char-wenyang",
                                          "char-wangyun", "char-yangyi", "char-zhoufang", "char-chendeng", "char-yuantan", "char-bianfuren",
                                          "char-liushan", "char-xiahouba", "char-sunhao", "char-taoqian", "char-guosi", "char-liuqi",
                                          "char-xuyou", "char-caochun", "char-wangcan", "char-huangquan", "char-lvdai", "char-guozhao", "char-huanghao" })
                Assert.That(GeneralDossiers.Find(new CharacterId(id)), Is.Not.Null, $"{id} 已入谱。");
            // 批 4–7 中文名经 DisplayNames（不漏原始 id）。
            foreach (var (id, name) in new[] { ("char-wangyun", "王允"), ("char-yangyi", "杨仪"), ("char-yuantan", "袁谭"),
                                               ("char-liushan", "刘禅"), ("char-xiahouba", "夏侯霸"), ("char-guosi", "郭汜"),
                                               ("char-xuyou", "许攸"), ("char-caochun", "曹纯"), ("char-huanghao", "黄皓") })
                Assert.That(DisplayNames.Of(id), Is.EqualTo(name), $"{id} → {name}。");
            var roster = GeneralRosterView.Build();
            GeneralCardView masu = null!;
            foreach (GeneralCardView c in roster.Cards) if (c.Id == "char-masu") masu = c;
            Assert.That(masu, Is.Not.Null);
            Assert.That(masu.Name, Is.EqualTo("马谡"));
            // 董卓 139–192：190 在世，200 已亡（生卒驱动登场/退场）。
            Assert.That(GeneralDossiers.AvailableAt(new CharacterId("char-dongzhuo"), 190), Is.True);
            Assert.That(GeneralDossiers.AvailableAt(new CharacterId("char-dongzhuo"), 200), Is.False);
            // 邓艾 197 生：190 尚未出（后世名将）。
            Assert.That(GeneralDossiers.AvailableAt(new CharacterId("char-dengai"), 190), Is.False);
        }

        [Test]
        public void test_tag_text_maps_every_tag_to_chinese()
        {
            // 每个气质标签都有中文短语（无落空到枚举名）。
            foreach (GeneralTag t in System.Enum.GetValues(typeof(GeneralTag)))
                Assert.That(GeneralTagText.Of(t), Is.Not.EqualTo(t.ToString()), $"{t} 应有中文性情短语。");
        }
    }
}
