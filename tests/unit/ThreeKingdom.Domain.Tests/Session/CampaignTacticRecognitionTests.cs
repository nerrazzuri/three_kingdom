using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ThreeKingdom.Application.Session;
using ThreeKingdom.Domain.Battle;
using B = ThreeKingdom.Domain.Tests.Session.CampaignBattleStateTests;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>
    /// epic-019 story-003：兵法事后识别（FeintAmbush 机动招式 + 涌现无按钮）— CD 硬退出门（Integration / Assembly）。
    /// 治理 ADR：ADR-0004（确定性）+ ADR-0009（装配，复用 TacticRecognizer）。TR-battle-002。
    /// 覆盖：FeintAmbush 全条件成立识别；条件不全不识别（无无条件按钮）；确定性；多链独立。
    /// </summary>
    [TestFixture]
    public class CampaignTacticRecognitionTests
    {
        private static readonly CampaignSessionService Service = B.Service;

        private static CampaignSession StartedBattle()
        {
            CampaignSession s = B.SessionWithCommittedPlan();
            Service.StartBattle(s, B.Units(), B.BattleCfg(), 42, B.TacticChains());
            return s;
        }

        // ---- AC-1: FeintAmbush 全条件成立时识别（CD 硬退出门核心）----

        [Test]
        public void test_feint_ambush_recognized_when_all_conditions_met()
        {
            CampaignSession s = StartedBattle();
            Service.MarkTacticCondition(s, TacticCondition.ControlledRetreatKeptFormation);
            Service.MarkTacticCondition(s, TacticCondition.EnemyPursued);
            Service.MarkTacticCondition(s, TacticCondition.AmbushSurprise);

            IReadOnlyList<RecognizedTactic> tactics = Service.RecognizeTactics(s);

            Assert.That(tactics.Any(t => t.Tag == TacticTag.FeintAmbush), Is.True, "假退伏击机动招式被识别");
        }

        // ---- AC-2: 条件不全不识别（无无条件按钮，TR-battle-002）----

        [Test]
        public void test_feint_ambush_not_recognized_when_condition_missing()
        {
            CampaignSession s = StartedBattle();
            Service.MarkTacticCondition(s, TacticCondition.ControlledRetreatKeptFormation);
            Service.MarkTacticCondition(s, TacticCondition.EnemyPursued);
            // 缺 AmbushSurprise

            IReadOnlyList<RecognizedTactic> tactics = Service.RecognizeTactics(s);

            Assert.That(tactics.Any(t => t.Tag == TacticTag.FeintAmbush), Is.False, "条件不全不识别（涌现非按钮）");
        }

        [Test]
        public void test_no_conditions_yields_no_tactics()
        {
            CampaignSession s = StartedBattle();
            IReadOnlyList<RecognizedTactic> tactics = Service.RecognizeTactics(s);
            Assert.That(tactics.Count, Is.EqualTo(0));
        }

        // ---- AC-3: 识别确定性 ----

        [Test]
        public void test_recognition_is_deterministic()
        {
            CampaignSession a = StartedBattle();
            CampaignSession b = StartedBattle();
            foreach (CampaignSession s in new[] { a, b })
            {
                Service.MarkTacticCondition(s, TacticCondition.ControlledRetreatKeptFormation);
                Service.MarkTacticCondition(s, TacticCondition.EnemyPursued);
                Service.MarkTacticCondition(s, TacticCondition.AmbushSurprise);
            }

            IReadOnlyList<RecognizedTactic> ta = Service.RecognizeTactics(a);
            IReadOnlyList<RecognizedTactic> tb = Service.RecognizeTactics(b);

            Assert.That(ta.Select(t => t.Tag), Is.EqualTo(tb.Select(t => t.Tag)), "同条件集 → 同识别");
        }

        // ---- AC-4: 多兵法链独立识别 ----

        [Test]
        public void test_multiple_tactic_chains_recognized_independently()
        {
            CampaignSession s = StartedBattle();
            // FeintAmbush 三条件
            Service.MarkTacticCondition(s, TacticCondition.ControlledRetreatKeptFormation);
            Service.MarkTacticCondition(s, TacticCondition.EnemyPursued);
            Service.MarkTacticCondition(s, TacticCondition.AmbushSurprise);
            // NightRaid 四条件
            Service.MarkTacticCondition(s, TacticCondition.IsNight);
            Service.MarkTacticCondition(s, TacticCondition.StealthSuccess);
            Service.MarkTacticCondition(s, TacticCondition.DefenderUnaware);
            Service.MarkTacticCondition(s, TacticCondition.RaiderDisciplineMet);

            IReadOnlyList<RecognizedTactic> tactics = Service.RecognizeTactics(s);

            Assert.That(tactics.Any(t => t.Tag == TacticTag.FeintAmbush), Is.True);
            Assert.That(tactics.Any(t => t.Tag == TacticTag.NightRaid), Is.True, "两兵法独立识别");
        }
    }
}
