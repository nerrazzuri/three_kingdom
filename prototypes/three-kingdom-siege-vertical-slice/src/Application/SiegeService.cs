// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 玩家意图经 Command→Service→Domain 路径进入状态，UI 不直接改 Domain（架构锁）
// Date: 2026-06-21

using TkSlice.Domain.Battle;
using TkSlice.Domain.Diplomacy;
using TkSlice.Domain.Numerics;
using TkSlice.Domain.Siege;

namespace TkSlice.Application
{
    // --- Commands（玩家意图）---
    public readonly record struct CommitRaidCommand(int RaidUnits);
    public readonly record struct AdvanceSegmentCommand(int Segments);
    public readonly record struct ResolveAssaultCommand();
    public readonly record struct FeignedRetreatCommand(int DecoyTroops, int AmbushTroops);
    public readonly record struct RequestDiplomacyCommand(PledgeType Type, int PledgeCostPct);
    public readonly record struct ScoutCommand();

    /// <summary>命令结果：稳定错误码或成功，无部分写入。</summary>
    public readonly struct CommandResult
    {
        public readonly bool Ok;
        public readonly string Code;   // 成功为 "OK"，失败为稳定错误码
        public CommandResult(bool ok, string code) { Ok = ok; Code = code; }
        public static readonly CommandResult Success = new(true, "OK");
        public static CommandResult Fail(string code) => new(false, code);
    }

    /// <summary>
    /// Application 服务：校验前置条件、编排 Domain 操作。
    /// Presentation 只能经此提交意图，不能直接修改 Domain 状态。
    /// </summary>
    public sealed class SiegeService
    {
        private readonly SiegeState _state;
        public SiegeState State => _state;

        public SiegeService(SiegeState state) => _state = state;

        public CommandResult CommitRaid(CommitRaidCommand cmd)
        {
            if (cmd.RaidUnits <= 0) return CommandResult.Fail("ERR_RAID_UNITS_NONPOSITIVE");
            if (_state.EnemyRaidCutActive) return CommandResult.Fail("ERR_RAID_ALREADY_ACTIVE");
            _state.CommitRaidOnSupplyLine(cmd.RaidUnits);
            return CommandResult.Success;
        }

        public CommandResult Advance(AdvanceSegmentCommand cmd)
        {
            if (cmd.Segments <= 0) return CommandResult.Fail("ERR_SEGMENTS_NONPOSITIVE");
            for (int i = 0; i < cmd.Segments; i++) _state.AdvanceSegment();
            return CommandResult.Success;
        }

        public BattleResult ResolveAssault(ResolveAssaultCommand _) => _state.ResolveEnemyAssault();

        /// <summary>侦察：耗一时段（世界照常推进），在新时刻观察敌情、刷新知识投影。</summary>
        public CommandResult Scout(ScoutCommand _)
        {
            _state.AdvanceSegment();
            _state.ScoutEnemy();
            return CommandResult.Success;
        }

        public FeignedRetreatResult FeignedRetreat(FeignedRetreatCommand cmd, out CommandResult check)
        {
            int total = cmd.DecoyTroops + cmd.AmbushTroops;
            if (cmd.DecoyTroops <= 0 || cmd.AmbushTroops <= 0)
                { check = CommandResult.Fail("ERR_FEIGN_FORCE_NONPOSITIVE"); return Empty(); }
            if (total > _state.Defender.Troops)
                { check = CommandResult.Fail("ERR_FEIGN_FORCE_EXCEEDS_GARRISON"); return Empty(); }
            check = CommandResult.Success;
            return _state.CommitFeignedRetreat(cmd.DecoyTroops, cmd.AmbushTroops);

            static FeignedRetreatResult Empty() => new() { Outcome = FeignOutcome.NotPursued, Note = "" };
        }

        public (CommandResult, DiplomaticPledge?) RequestDiplomacy(RequestDiplomacyCommand cmd)
        {
            if (cmd.PledgeCostPct < 0 || cmd.PledgeCostPct > 100)
                return (CommandResult.Fail("ERR_PLEDGE_COST_OUT_OF_RANGE"), null);
            var pledge = _state.RequestDiplomacy(cmd.Type, Fixed.FromFraction(cmd.PledgeCostPct, 100));
            return (CommandResult.Success, pledge);
        }
    }
}
