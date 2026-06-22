using System;
using System.Collections.Generic;
using ThreeKingdom.Domain.Characters;
using ThreeKingdom.Domain.City;
using ThreeKingdom.Domain.Map;
using ThreeKingdom.Domain.Relationships;

namespace ThreeKingdom.Domain.Outcome
{
    /// <summary>
    /// 跨系统后果写回服务（gdd-010 §后果 / systems-index「后果结算」契约 / ADR-0004 + ADR-0002）。
    /// <b>先生成变更计划并全量校验，再原子写回，不产生半结算态</b>：
    /// <list type="number">
    ///   <item>校验阶段聚合检查每条变更（目标存在、写回后不变量、守恒分组净额为 0）。</item>
    ///   <item>任一目标不合法 → <b>整批回滚</b>，返回原快照未变 + 稳定错误码（零部分写入，全有或全无）。</item>
    ///   <item>全部通过 → 一次性构造写回后的新 <see cref="OutcomeWorld"/>，各权威系统独占写自身状态。</item>
    /// </list>
    /// 纯函数：不就地修改输入快照；同一战果 → 同一变更集 → 同一最终状态哈希（确定性）。
    /// </summary>
    public sealed class OutcomeWritebackService
    {
        /// <summary>校验并原子写回一个变更集。</summary>
        public OutcomeWritebackResult Apply(OutcomeWorld world, ConsequenceSet set)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (set == null) throw new ArgumentNullException(nameof(set));

            var errors = new List<OutcomeError>();

            // 工作副本（仅在全部通过后才成为新世界；失败时丢弃，原快照不受影响）。
            var cities = world.CitiesCopy();
            var reputation = world.ReputationCopy();
            var vitality = world.CharacterVitalityCopy();
            var relationships = world.RelationshipsCopy();

            // 城市字段按 (城市, 字段) 累计净 delta（多条同字段变更先聚合再校验最终值）。
            var cityDelta = new Dictionary<(CityId, CityField), long>();
            var conservation = new Dictionary<string, long>(StringComparer.Ordinal);

            foreach (var ch in set.Changes)
            {
                if (ch.ConservationKey != null)
                    conservation[ch.ConservationKey] =
                        (conservation.TryGetValue(ch.ConservationKey, out long s) ? s : 0L) + ch.Delta;

                switch (ch.Kind)
                {
                    case OutcomeTargetKind.City:
                        if (!world.HasCity(ch.City))
                        {
                            errors.Add(new OutcomeError(OutcomeErrorCode.UnknownTarget, $"城市「{ch.City}」不存在。"));
                            break;
                        }
                        var ckey = (ch.City, ch.Field);
                        cityDelta[ckey] = (cityDelta.TryGetValue(ckey, out long cd) ? cd : 0L) + ch.Delta;
                        break;

                    case OutcomeTargetKind.Reputation:
                        if (!world.HasReputation(ch.Faction))
                        {
                            errors.Add(new OutcomeError(OutcomeErrorCode.UnknownTarget, $"阵营「{ch.Faction}」名声未登记。"));
                            break;
                        }
                        reputation[ch.Faction] = reputation[ch.Faction] + ch.Delta; // 名声带符号，无非负不变量
                        break;

                    case OutcomeTargetKind.Character:
                        if (!world.HasCharacter(ch.Character))
                        {
                            errors.Add(new OutcomeError(OutcomeErrorCode.UnknownTarget, $"人物「{ch.Character}」计量未登记。"));
                            break;
                        }
                        vitality[ch.Character] = vitality[ch.Character] + ch.Delta;
                        break;

                    case OutcomeTargetKind.Relationship:
                        var rk = new RelationshipKey(ch.RelFrom, ch.RelTo, ch.RelDim);
                        int cur = relationships.TryGetValue(rk, out int rv) ? rv : RelationshipScale.Neutral;
                        relationships[rk] = RelationshipScale.Clamp((long)cur + ch.Delta); // 关系按刻度 clamp，永不越界
                        break;
                }
            }

            // 城市最终值校验 + 落入工作副本。
            foreach (var kv in cityDelta)
            {
                var (cityId, field) = kv.Key;
                var c = cities[cityId];
                long projected = FieldValue(c, field) + kv.Value;
                switch (field)
                {
                    case CityField.Stock:
                        if (projected < c.Reserved)
                        { errors.Add(new OutcomeError(OutcomeErrorCode.NegativeResult, $"城市「{cityId}」库存写回后将低于已保留量。")); continue; }
                        cities[cityId] = c.With(stock: projected);
                        break;
                    case CityField.CivMorale:
                        if (projected < 0)
                        { errors.Add(new OutcomeError(OutcomeErrorCode.NegativeResult, $"城市「{cityId}」民心写回后为负。")); continue; }
                        cities[cityId] = c.With(civMorale: checked((int)projected));
                        break;
                    case CityField.Security:
                        if (projected < 0)
                        { errors.Add(new OutcomeError(OutcomeErrorCode.NegativeResult, $"城市「{cityId}」治安写回后为负。")); continue; }
                        cities[cityId] = c.With(security: checked((int)projected));
                        break;
                    case CityField.Fortification:
                        if (projected < 0 || projected > c.FortificationMax)
                        { errors.Add(new OutcomeError(OutcomeErrorCode.FortificationOutOfRange, $"城市「{cityId}」工事写回后越界 [0,{c.FortificationMax}]。")); continue; }
                        cities[cityId] = c.With(fortificationCurrent: checked((int)projected));
                        break;
                }
            }

            // 人物非负不变量校验。
            foreach (var kv in vitality)
                if (kv.Value < 0)
                    errors.Add(new OutcomeError(OutcomeErrorCode.NegativeResult, $"人物「{kv.Key}」计量写回后为负。"));

            // 守恒分组净额校验（同键之和须为 0）。
            foreach (var kv in conservation)
                if (kv.Value != 0)
                    errors.Add(new OutcomeError(OutcomeErrorCode.ConservationViolation, $"守恒分组「{kv.Key}」净额 {kv.Value} ≠ 0（凭空增减）。"));

            if (errors.Count > 0)
                return OutcomeWritebackResult.Failure(world, errors); // 整批回滚，原快照未变

            var newWorld = new OutcomeWorld(cities, reputation, vitality, relationships);
            return OutcomeWritebackResult.Success(newWorld);
        }

        private static long FieldValue(CityEconomyState c, CityField field) => field switch
        {
            CityField.Stock => c.Stock,
            CityField.CivMorale => c.CivMorale,
            CityField.Security => c.Security,
            CityField.Fortification => c.FortificationCurrent,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
    }
}
