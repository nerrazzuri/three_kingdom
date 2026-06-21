using System;
using System.Collections.Generic;
using NUnit.Framework;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.Numerics;
using ThreeKingdom.Domain.Relationships;
using ThreeKingdom.Domain.Time;

namespace ThreeKingdom.Domain.Tests.Relationships
{
    /// <summary>
    /// epic-003 story-003：方向性多维关系与事件幂等。
    /// 治理 ADR：ADR-0004（确定性、幂等）+ ADR-0002（关系不授权）。GDD_006 / TR-relationship-001/002。
    /// 覆盖 AC-1 方向性多维不对称、AC-2 具名事件幂等、AC-3 coop_score 不凭空授权、AC-4 授权有效性、AC-5 多维不合并。
    /// </summary>
    [TestFixture]
    public class DirectionalRelationshipTests
    {
        private static CharacterId C(string s) => new CharacterId(s);

        private static RelationshipEvent Event(string id, CharacterId target, CharacterId[] knowers,
            Dictionary<RelationshipDimension, int> deltas, string reason = "test")
            => new RelationshipEvent(id, target, knowers, deltas, reason);

        // ---- AC-1：方向性多维不对称 ----

        [Test]
        public void Relationship_is_directional_and_asymmetric()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e1", C("B"), new[] { C("A") },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 50 }));

            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Trust), Is.EqualTo(50));
            Assert.That(rel.Get(C("B"), C("A"), RelationshipDimension.Trust), Is.EqualTo(0), "反方向独立，不受影响");
        }

        [Test]
        public void Only_knowers_relationships_change()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("rescue", C("rescuer"), new[] { C("jiap") },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Gratitude] = 20 }));

            Assert.That(rel.Get(C("jiap"), C("rescuer"), RelationshipDimension.Gratitude), Is.EqualTo(20));
            Assert.That(rel.Get(C("yi"), C("rescuer"), RelationshipDimension.Gratitude), Is.EqualTo(0), "不知情者不变");
        }

        [Test]
        public void Dimensions_are_independent_not_merged()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e", C("B"), new[] { C("A") }, new Dictionary<RelationshipDimension, int>
            {
                [RelationshipDimension.Trust] = 70,
                [RelationshipDimension.Respect] = -30, // 信任高但敬重低（GDD §Edge）
            }));

            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Trust), Is.EqualTo(70));
            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Respect), Is.EqualTo(-30));
        }

        [Test]
        public void Values_clamp_to_scale()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e", C("B"), new[] { C("A") },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 150 }));
            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Trust), Is.EqualTo(RelationshipScale.Max));
        }

        // ---- AC-2：事件幂等 ----

        [Test]
        public void Same_event_id_is_idempotent()
        {
            var rel = new RelationshipState();
            var ev = Event("evt-1", C("B"), new[] { C("A") },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 30 });

            Assert.That(rel.ApplyEvent(ev), Is.True);
            Assert.That(rel.ApplyEvent(ev), Is.False, "重复同 ID 被跳过");

            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Trust), Is.EqualTo(30), "不叠加");
            Assert.That(rel.HasApplied("evt-1"), Is.True);
        }

        [Test]
        public void Distinct_event_ids_accumulate()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e1", C("B"), new[] { C("A") }, new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 30 }));
            rel.ApplyEvent(Event("e2", C("B"), new[] { C("A") }, new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 10 }));
            Assert.That(rel.Get(C("A"), C("B"), RelationshipDimension.Trust), Is.EqualTo(40));
        }

        // ---- AC-3：coop_score（不凭空授权）----

        [Test]
        public void CoopScore_matches_gdd_example()
        {
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e", C("req"), new[] { C("exec") }, new Dictionary<RelationshipDimension, int>
            {
                [RelationshipDimension.Trust] = 70,
                [RelationshipDimension.Respect] = 40,
                [RelationshipDimension.Gratitude] = 30,
                [RelationshipDimension.Resentment] = 10,
            }));

            var weights = new Dictionary<RelationshipDimension, FixedPoint>
            {
                [RelationshipDimension.Trust] = FixedPoint.FromFraction(1, 2),
                [RelationshipDimension.Respect] = FixedPoint.FromFraction(3, 10),
                [RelationshipDimension.Gratitude] = FixedPoint.FromFraction(1, 5),
                [RelationshipDimension.Resentment] = FixedPoint.FromFraction(-2, 5),
            };

            // 0.5×70+0.3×40+0.2×30−0.4×10−5 = 44
            var coop = CooperationEvaluator.ComputeCoopScore(rel, C("exec"), C("req"), weights, FixedPoint.FromInt(5));

            Assert.That(coop > FixedPoint.FromFraction(439, 10), Is.True);
            Assert.That(coop < FixedPoint.FromFraction(441, 10), Is.True);
        }

        [Test]
        public void Classify_maps_score_to_structured_response()
        {
            var thresholds = new CooperationThresholds(FixedPoint.FromInt(40), FixedPoint.FromInt(20), FixedPoint.Zero);
            Assert.That(CooperationEvaluator.Classify(FixedPoint.FromInt(44), thresholds), Is.EqualTo(CooperationResponse.Accept));
            Assert.That(CooperationEvaluator.Classify(FixedPoint.FromInt(25), thresholds), Is.EqualTo(CooperationResponse.RequireGuarantee));
            Assert.That(CooperationEvaluator.Classify(FixedPoint.FromInt(5), thresholds), Is.EqualTo(CooperationResponse.Oppose));
            Assert.That(CooperationEvaluator.Classify(FixedPoint.FromInt(-10), thresholds), Is.EqualTo(CooperationResponse.Reject));
        }

        // ---- AC-4：授权有效性（关系不绕过）----

        [Test]
        public void Authority_grant_validity_respects_expiry_revocation_and_granter()
        {
            var grant = new AuthorityGrant(C("lord"), C("general"), CommandType.Dispatch, new WorldTime(5, DaySegment.Dawn));
            var before = new WorldTime(3, DaySegment.Day);
            var after = new WorldTime(6, DaySegment.Dawn);

            Assert.That(grant.IsValid(before, granterStillAuthorized: true), Is.True);
            Assert.That(grant.IsValid(after, granterStillAuthorized: true), Is.False, "过期无效");
            Assert.That(grant.IsValid(before, granterStillAuthorized: false), Is.False, "授予者失权无效");
            Assert.That(grant.AsRevoked().IsValid(before, granterStillAuthorized: true), Is.False, "撤销无效");
        }

        [Test]
        public void High_relationship_does_not_create_authority()
        {
            // 关系满值但无 AuthorityGrant → 无任何授权产生（结构性：授权与关系是独立类型，无从关系生成授权的路径）
            var rel = new RelationshipState();
            rel.ApplyEvent(Event("e", C("req"), new[] { C("exec") },
                new Dictionary<RelationshipDimension, int> { [RelationshipDimension.Trust] = 100 }));

            // 关系只产 coop_score（协作意愿），不产授权——此处无 AuthorityGrant 即无权限
            var weights = new Dictionary<RelationshipDimension, FixedPoint> { [RelationshipDimension.Trust] = FixedPoint.One };
            var coop = CooperationEvaluator.ComputeCoopScore(rel, C("exec"), C("req"), weights, FixedPoint.Zero);
            Assert.That(coop, Is.EqualTo(FixedPoint.FromInt(100)), "关系只转化为协作意愿分");
        }
    }
}
