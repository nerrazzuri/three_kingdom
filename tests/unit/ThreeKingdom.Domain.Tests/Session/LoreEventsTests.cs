using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;
using ThreeKingdom.Domain.Characters;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// 演义事件引擎（GDD_027 R6 / ADR-0016）：触发门 + 效果层（登场/移籍/斩杀）+ 覆盖态确定性重放。纯 C# 无场景依赖。
    /// </summary>
    [TestFixture]
    public class LoreEventsTests
    {
        private static CharacterId C(string id) => new CharacterId(id);
        private static bool FiresId(LoreContext ctx, string id)
        {
            foreach (LoreEvent e in LoreEvents.FiredAt(ctx)) if (e.Id == id) return true;
            return false;
        }

        // ---- 触发门 ----
        [Test]
        public void test_lore_taoyuan_fires_only_for_liubei_before_190()
        {
            // Arrange / Act / Assert
            Assert.That(FiresId(new LoreContext(190, 190, PlayableCampaign.LiuBei), "event-taoyuan"), Is.True, "刘备·讨董之世 → 桃园结义。");
            Assert.That(FiresId(new LoreContext(190, 190, PlayableCampaign.Cao), "event-taoyuan"), Is.False, "非刘备不触发桃园。");
            Assert.That(FiresId(new LoreContext(200, 200, PlayableCampaign.LiuBei), "event-taoyuan"), Is.False, "200 已非结义之时。");
        }

        [Test]
        public void test_lore_sangu_introduces_zhugeliang_for_liubei_after_207()
        {
            // Arrange
            var ctx = new LoreContext(200, 208, PlayableCampaign.LiuBei);
            // Act
            LoreOverrides ov = LoreEvents.OverridesAt(ctx);
            // Assert
            Assert.That(FiresId(new LoreContext(208, 208, PlayableCampaign.LiuBei), "event-sangu"), Is.True, "刘备·隆中对之世 → 三顾茅庐触发。");
            Assert.That(ov.IsIntroduced(C("char-zhugeliang")), Is.True, "三顾后卧龙登场（可发觉·可招）。");
            Assert.That(LoreEvents.OverridesAt(new LoreContext(200, 208, PlayableCampaign.Cao)).IsIntroduced(C("char-zhugeliang")), Is.False, "非刘备不触发三顾。");
        }

        // ---- 斩杀效果 ----
        [Test]
        public void test_lore_slay_removes_general_from_affiliation()
        {
            // Arrange：温酒斩华雄（刘备·190）→ 华雄陨落。
            var ctx = new LoreContext(190, 190, PlayableCampaign.LiuBei);
            // Act
            LoreOverrides ov = LoreEvents.OverridesAt(ctx);
            Affiliation before = GeneralAffiliations.AffiliationOf(C("char-huaxiong"), 190);
            Affiliation after = GeneralAffiliations.AffiliationOf(C("char-huaxiong"), 190, ov);
            // Assert
            Assert.That(ov.IsSlain(C("char-huaxiong")), Is.True, "温酒斩华雄 → 华雄入斩杀集。");
            Assert.That(before.Status, Is.Not.EqualTo(AffiliationStatus.Absent), "未加覆盖前华雄在盘。");
            Assert.That(after.Status, Is.EqualTo(AffiliationStatus.Absent), "斩杀覆盖 → 归属 Absent（不入册）。");
        }

        [Test]
        public void test_lore_baimenlou_slays_lubu_faction_and_reassigns_zhangliao()
        {
            // Arrange：白门楼（世事·≥199），吕布覆亡。
            var ctx = new LoreContext(194, 200, PlayableCampaign.Cao);
            // Act
            LoreOverrides ov = LoreEvents.OverridesAt(ctx);
            // Assert
            Assert.That(ov.IsSlain(C("char-lubu")), Is.True, "白门楼缢吕布。");
            Assert.That(ov.IsSlain(C("char-gaoshun")), Is.True, "高顺就戮。");
            Assert.That(ov.IsSlain(C("char-chengong")), Is.True, "陈宫就戮。");
            Assert.That(ov.TryReassigned(C("char-zhangliao"), out var f), Is.True, "张辽移籍。");
            Assert.That(f.HasValue && f.Value.Equals(PlayableCampaign.Cao), Is.True, "文远归曹。");
        }

        // ---- 移籍效果（真实 delta：baseFaction 袁绍 → 覆盖曹魏）----
        [Test]
        public void test_lore_reassign_changes_faction_from_base()
        {
            // Arrange：许攸夜奔（世事·≥200）。许攸本属袁绍。
            var ctx = new LoreContext(200, 200, PlayableCampaign.Cao);
            Affiliation baseline = GeneralAffiliations.AffiliationOf(C("char-xuyou"), 200);
            // Act
            LoreOverrides ov = LoreEvents.OverridesAt(ctx);
            Affiliation moved = GeneralAffiliations.AffiliationOf(C("char-xuyou"), 200, ov);
            // Assert
            Assert.That(baseline.Faction.Equals(PlayableCampaign.YuanShao), Is.True, "覆盖前许攸本属袁绍。");
            Assert.That(moved.Status, Is.EqualTo(AffiliationStatus.InService), "移籍后仍在职。");
            Assert.That(moved.Faction.Equals(PlayableCampaign.Cao), Is.True, "许攸夜奔 → 归属改曹魏（真实 delta）。");
        }

        // ---- 确定性重放（可推演不入档的核心保证）----
        [Test]
        public void test_lore_overrides_replay_is_deterministic()
        {
            // Arrange：同一上下文两次求值。
            var ctx = new LoreContext(190, 234, PlayableCampaign.LiuBei);
            // Act
            LoreOverrides a = LoreEvents.OverridesAt(ctx);
            LoreOverrides b = LoreEvents.OverridesAt(ctx);
            // Assert：读档重算即复原——两次陨落/移籍/登场集合一致。
            Assert.That(a.Slain.Count, Is.EqualTo(b.Slain.Count), "重放陨落数确定。");
            Assert.That(a.Reassigned.Count, Is.EqualTo(b.Reassigned.Count), "重放移籍数确定。");
            Assert.That(a.Introduced.Count, Is.EqualTo(b.Introduced.Count), "重放登场数确定。");
            // 190→234 全程累积：华雄/吕布/关羽/诸葛亮均已陨落。
            Assert.That(a.IsSlain(C("char-huaxiong")) && a.IsSlain(C("char-lubu"))
                && a.IsSlain(C("char-guanyu")) && a.IsSlain(C("char-zhugeliang")), Is.True, "跨纪元累积斩杀齐备。");
        }

        [Test]
        public void test_lore_overrides_empty_when_no_event_fired()
        {
            // Arrange：曹操·190 开局当年，无世事类事件（均 ≥199）触发。
            var ctx = new LoreContext(190, 190, PlayableCampaign.Cao);
            // Act
            LoreOverrides ov = LoreEvents.OverridesAt(ctx);
            // Assert
            Assert.That(ov.IsEmpty, Is.True, "无触发 → 覆盖态空。");
        }

        // ---- 覆盖层机制单测 ----
        [Test]
        public void test_lore_overrides_apply_records_each_effect_kind()
        {
            // Arrange
            var ov = new LoreOverrides();
            // Act
            ov.Apply(LoreEffect.Slay(C("char-a")));
            ov.Apply(LoreEffect.Reassign(C("char-b"), PlayableCampaign.Cao));
            ov.Apply(LoreEffect.Reassign(C("char-c"), null));
            ov.Apply(LoreEffect.Introduce(C("char-d")));
            // Assert
            Assert.That(ov.IsSlain(C("char-a")), Is.True);
            Assert.That(ov.TryReassigned(C("char-b"), out var fb) && fb.HasValue && fb.Value.Equals(PlayableCampaign.Cao), Is.True, "移籍目标势力。");
            Assert.That(ov.TryReassigned(C("char-c"), out var fc) && !fc.HasValue, Is.True, "移籍 null = 转在野。");
            Assert.That(ov.IsIntroduced(C("char-d")), Is.True);
            Assert.That(LoreOverrides.Empty.IsEmpty, Is.True, "Empty 恒空。");
        }
    }
}
