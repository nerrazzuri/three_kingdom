using System;
using NUnit.Framework;
using ThreeKingdom.Presentation.Intents;

namespace ThreeKingdom.Domain.Tests.Presentation
{
    /// <summary>
    /// epic-010 story-001：UI 意图 → Application 命令映射。
    /// 治理 ADR：ADR-0002（Presentation 构造意图 Command 交 Application；不执行规则、不触达 Domain）。
    /// 覆盖 AC-2 显式映射产出对应命令载荷 + 确定性 + 不支持意图稳定报错。
    /// </summary>
    [TestFixture]
    public class IntentTranslationTests
    {
        private static readonly IntentTranslator Translator = new IntentTranslator();

        [Test]
        public void test_new_game_intent_maps_to_start_command()
        {
            var cmd = Translator.Translate(new NewGameIntent());
            Assert.That(cmd, Is.InstanceOf<StartNewGameCommand>());
        }

        [Test]
        public void test_load_intent_carries_slot_to_command()
        {
            var cmd = Translator.Translate(new LoadGameIntent("campaign"));
            Assert.That(cmd, Is.InstanceOf<LoadGameCommand>());
            Assert.That(((LoadGameCommand)cmd).Slot, Is.EqualTo("campaign"));
        }

        [Test]
        public void test_scout_and_submit_plan_intents_map_with_payload()
        {
            var scout = Translator.Translate(new ScoutIntent("enemy-main-host"));
            Assert.That(((ScoutCommand)scout).Subject, Is.EqualTo("enemy-main-host"));

            var plan = Translator.Translate(new SubmitPlanIntent("plan-7"));
            Assert.That(((SubmitPlanCommand)plan).PlanId, Is.EqualTo("plan-7"));
        }

        [Test]
        public void test_translation_is_deterministic()
        {
            var a = Translator.Translate(new SaveGameIntent("slot-a"));
            var b = Translator.Translate(new SaveGameIntent("slot-a"));
            Assert.That(a.GetType(), Is.EqualTo(b.GetType()));
            Assert.That(((SaveGameCommand)a).Slot, Is.EqualTo(((SaveGameCommand)b).Slot));
        }

        [Test]
        public void test_null_intent_throws()
        {
            Assert.Throws<ArgumentNullException>(() => Translator.Translate(null!));
        }
    }
}
