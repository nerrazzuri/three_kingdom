using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.ZoneBattle
{
    /// <summary>战中调整命令稳定错误码（GDD_021 R4 / ADR-0012 D3：前置校验、无部分写入）。</summary>
    public enum ZoneCommandError
    {
        None = 0,
        DetachmentNotFound = 1,
        NotOwner = 2,
        ZoneNotFound = 3,
        NotAdjacent = 4,
        AlreadyInTransit = 5,
        DetachmentBroken = 6,
        InvalidTransit = 7,
        SameZone = 8,
    }

    /// <summary>战中调整命令结果（成功携新态；失败=原态不变 + 稳定错误码，无部分写入）。不可变。</summary>
    public sealed class ZoneCommandResult
    {
        public bool Applied { get; }
        public ZoneCommandError Error { get; }
        public string Detail { get; }
        public ZoneBattleState State { get; }

        private ZoneCommandResult(bool applied, ZoneCommandError error, string detail, ZoneBattleState state)
        {
            Applied = applied;
            Error = error;
            Detail = detail;
            State = state;
        }

        public static ZoneCommandResult Success(ZoneBattleState state) => new ZoneCommandResult(true, ZoneCommandError.None, "", state);
        public static ZoneCommandResult Failure(ZoneCommandError error, ZoneBattleState original, string detail = "")
            => new ZoneCommandResult(false, error, detail, original);
    }

    /// <summary>
    /// 战中调整命令（GDD_021 R4「核心好玩点」/ ADR-0012 D3）：调动（相邻+在途）/ 改姿态。
    /// <b>玩家与敌AI受同一套约束</b>（不作弊，ADR-0013）：调动仅限相邻区、在途 N 回合（失位代价）、在途/被打散不可再调。
    /// 纯函数：成功产出新态，失败原态不变 + 稳定错误码（无部分写入）。
    /// </summary>
    public sealed class ZoneCommandService
    {
        /// <summary>默认调动在途回合（GDD_021 §11，默认 1）。</summary>
        public const int DefaultTransitRounds = 1;

        /// <summary>调动某支队到相邻区（在途 <paramref name="transitRounds"/> 回合）。投预备=从预备区调出，同此命令。</summary>
        public ZoneCommandResult MoveDetachment(
            ZoneBattleState state, BattleSide commandingSide, DetachmentId id, ZoneId target, int transitRounds = DefaultTransitRounds)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (transitRounds < 1) return ZoneCommandResult.Failure(ZoneCommandError.InvalidTransit, state, "在途回合须 ≥1。");

            Detachment? d = state.TryGet(id);
            if (d == null) return ZoneCommandResult.Failure(ZoneCommandError.DetachmentNotFound, state, id.ToString());
            if (d.Side != commandingSide) return ZoneCommandResult.Failure(ZoneCommandError.NotOwner, state, "只可调己方支队。");
            if (d.IsBroken) return ZoneCommandResult.Failure(ZoneCommandError.DetachmentBroken, state, "被打散支队不可调动。");
            if (d.InTransit) return ZoneCommandResult.Failure(ZoneCommandError.AlreadyInTransit, state, "在途支队不可再调。");
            if (!state.Field.Contains(target)) return ZoneCommandResult.Failure(ZoneCommandError.ZoneNotFound, state, target.ToString());
            if (target == d.Location) return ZoneCommandResult.Failure(ZoneCommandError.SameZone, state, "已在该区。");
            if (!state.Field.AreAdjacent(d.Location, target))
                return ZoneCommandResult.Failure(ZoneCommandError.NotAdjacent, state, $"{d.Location}→{target} 非相邻。");

            return ZoneCommandResult.Success(Replace(state, d.MoveTo(target, transitRounds)));
        }

        /// <summary>改某支队姿态（主攻/佯攻/守）。在途/被打散仍可改姿态（不涉移动）。</summary>
        public ZoneCommandResult SetPosture(ZoneBattleState state, BattleSide commandingSide, DetachmentId id, Posture posture)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            Detachment? d = state.TryGet(id);
            if (d == null) return ZoneCommandResult.Failure(ZoneCommandError.DetachmentNotFound, state, id.ToString());
            if (d.Side != commandingSide) return ZoneCommandResult.Failure(ZoneCommandError.NotOwner, state, "只可改己方支队姿态。");
            if (d.IsBroken) return ZoneCommandResult.Failure(ZoneCommandError.DetachmentBroken, state, "被打散支队无姿态。");
            if (d.Posture == posture) return ZoneCommandResult.Success(state);   // 幂等
            return ZoneCommandResult.Success(Replace(state, d.WithPosture(posture)));
        }

        private static ZoneBattleState Replace(ZoneBattleState state, Detachment updated)
        {
            var list = new List<Detachment>();
            foreach (Detachment d in state.Detachments)
                list.Add(d.Id == updated.Id ? updated : d);
            return state.WithDetachments(list);
        }
    }
}
