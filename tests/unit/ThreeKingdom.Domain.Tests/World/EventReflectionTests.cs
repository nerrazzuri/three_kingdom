using System;
using NUnit.Framework;
using ThreeKingdom.Domain.Time;
using ThreeKingdom.Domain.World;

namespace ThreeKingdom.Domain.Tests.World
{
    /// <summary>
    /// 事件分级通报 + 心里话（GDD_015 事件分级）：够得着走完整事件（Personal）；够不着的按心里话表
    /// 为可述通报（Notable，口吻<b>随主角人设</b>）或背景（Background）。未触发无通报。人设开局种子化随机。
    /// 心里话纯为丰富体验，不机械改状态。
    /// </summary>
    [TestFixture]
    public class EventReflectionTests
    {
        private readonly EventReflectionService _svc = new EventReflectionService();

        private static WorldState World()
            => new WorldState(new WorldTime(0, DaySegment.Dawn),
                Array.Empty<FactionRecord>(), Array.Empty<CityOwnership>(), Array.Empty<string>(), Array.Empty<string>());

        private static HistoryAdvanceResult Fired(FireReason reason, string label)
            => new HistoryAdvanceResult(fired: true, diverged: reason == FireReason.Diverged, reason,
                new HistoricalOutcome(label), World());

        [Test]
        public void test_reachable_precondition_event_is_personal_no_monologue()
        {
            EventReflection? r = _svc.Reflect(Fired(FireReason.NormalPreconditionsHeld, "battle-of-guandu"),
                MonologueCatalog.Default, ProtagonistPersona.Ambitious);
            Assert.That(r, Is.Not.Null);
            Assert.That(r!.Tier, Is.EqualTo(NoticeTier.Personal), "够得着 → 完整事件。");
            Assert.That(r.HasMonologue, Is.False, "切身事件走完整结算，不用心里话通报。");
        }

        [Test]
        public void test_diverged_event_is_personal()
        {
            EventReflection r = _svc.Reflect(Fired(FireReason.Diverged, "chibi-averted"),
                MonologueCatalog.Default, ProtagonistPersona.Loyalist)!;
            Assert.That(r.Tier, Is.EqualTo(NoticeTier.Personal), "已分叉（玩家改了前置）→ 完整事件。");
        }

        [Test]
        public void test_unreachable_notable_event_is_notable_with_monologue()
        {
            EventReflection r = _svc.Reflect(Fired(FireReason.NormalUnreachable, "yuanshu-declares-emperor"),
                MonologueCatalog.Default, ProtagonistPersona.Ambitious)!;
            Assert.That(r.Tier, Is.EqualTo(NoticeTier.Notable), "够不着但值得一提 → 通报。");
            Assert.That(r.HasMonologue, Is.True, "带主角心里话。");
        }

        [Test]
        public void test_monologue_voice_varies_by_persona()
        {
            HistoryAdvanceResult ev = Fired(FireReason.NormalUnreachable, "yuanshu-declares-emperor");
            string ambitious = _svc.Reflect(ev, MonologueCatalog.Default, ProtagonistPersona.Ambitious)!.Monologue;
            string loyalist = _svc.Reflect(ev, MonologueCatalog.Default, ProtagonistPersona.Loyalist)!.Monologue;
            Assert.That(ambitious, Is.Not.EqualTo(loyalist), "同一事件，不同人设 → 不同口吻的心里话（丰富体验）。");
            Assert.That(ambitious, Does.Contain("我"), "雄心者动了自立的念头。");
            Assert.That(loyalist, Does.Contain("僭"), "忠义者鄙弃僭越。");
        }

        [Test]
        public void test_persona_without_specific_line_falls_back_to_default()
        {
            // "guan-yu-loses-jingzhou" 仅有雄心/务实专属台词 → 忠义/谨慎皆回退通用心里话。
            HistoryAdvanceResult ev = Fired(FireReason.NormalUnreachable, "guan-yu-loses-jingzhou");
            string a = _svc.Reflect(ev, MonologueCatalog.Default, ProtagonistPersona.Loyalist)!.Monologue;
            string b = _svc.Reflect(ev, MonologueCatalog.Default, ProtagonistPersona.Cautious)!.Monologue;
            Assert.That(a, Is.EqualTo(b), "无专属台词的人设 → 回退同一通用心里话。");
            Assert.That(a.Length, Is.GreaterThan(0));
        }

        [Test]
        public void test_unreachable_unknown_event_is_background_no_monologue()
        {
            EventReflection r = _svc.Reflect(Fired(FireReason.NormalUnreachable, "some-minor-skirmish"),
                MonologueCatalog.Default, ProtagonistPersona.Pragmatist)!;
            Assert.That(r.Tier, Is.EqualTo(NoticeTier.Background), "够不着且琐碎 → 仅世界事实，不打扰玩家。");
            Assert.That(r.HasMonologue, Is.False);
        }

        [Test]
        public void test_not_fired_yields_no_reflection()
        {
            var notFired = new HistoryAdvanceResult(false, false, FireReason.AlreadyTriggered, null, World());
            Assert.That(_svc.Reflect(notFired, MonologueCatalog.Default, ProtagonistPersona.Ambitious), Is.Null, "未触发 → 无通报。");
        }

        [Test]
        public void test_persona_roll_is_deterministic()
        {
            Assert.That(ProtagonistPersonas.Roll(42UL), Is.EqualTo(ProtagonistPersonas.Roll(42UL)), "同种子同人设（可复现）。");
        }
    }
}
