using System;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>主菜单状态（main-menu §6/§14：有/无存档/读档中/失败/退出确认）。</summary>
    public enum MainMenuState
    {
        /// <summary>无存档：仅「新游戏/退出」。</summary>
        NoSave = 0,

        /// <summary>有存档：「继续」可用。</summary>
        HasSave = 1,

        /// <summary>读档中（瞬时）。</summary>
        Loading = 2,

        /// <summary>读档失败：显示可行动原因。</summary>
        LoadError = 3,

        /// <summary>退出确认。</summary>
        QuitConfirm = 4,
    }

    /// <summary>
    /// 主菜单展示状态机（main-menu §14 / ADR-0002 + ADR-0005）。
    /// <b>不可变</b>：每次转移产出新实例。本屏<b>不</b>直接写权威状态——读档经 Application、错误态消费
    /// epic-009 <see cref="LoadResult"/>（不兼容/损坏 → 可行动原因，零部分加载）。
    /// </summary>
    public sealed class MainMenuViewModel
    {
        private readonly SaveSlotView _slot;

        /// <summary>当前状态。</summary>
        public MainMenuState State { get; }

        /// <summary>「继续」是否可用（仅有存档时）。</summary>
        public bool ContinueAvailable => _slot.HasSave;

        /// <summary>存档版本标签（显示用）。</summary>
        public string SlotVersionLabel => _slot.VersionLabel;

        /// <summary>读档失败原因（仅 <see cref="MainMenuState.LoadError"/> 非空）。</summary>
        public string? ErrorReason { get; }

        /// <summary>读档失败错误码（仅 LoadError 非空）。</summary>
        public LoadErrorCode? ErrorCode { get; }

        /// <summary>读档是否成功（成功后由调用方切场景；本屏不进游戏）。</summary>
        public bool LoadSucceeded { get; }

        private MainMenuViewModel(SaveSlotView slot, MainMenuState state, string? errorReason, LoadErrorCode? errorCode, bool loadSucceeded)
        {
            _slot = slot;
            State = state;
            ErrorReason = errorReason;
            ErrorCode = errorCode;
            LoadSucceeded = loadSucceeded;
        }

        /// <summary>从槽投影构造初始状态（有存档 → HasSave；否则 NoSave）。</summary>
        public static MainMenuViewModel FromSlot(SaveSlotView slot)
        {
            if (slot == null) throw new ArgumentNullException(nameof(slot));
            return new MainMenuViewModel(slot, slot.HasSave ? MainMenuState.HasSave : MainMenuState.NoSave, null, null, false);
        }

        private MainMenuState RestingState => _slot.HasSave ? MainMenuState.HasSave : MainMenuState.NoSave;

        /// <summary>开始读档（进入瞬时 Loading）。</summary>
        public MainMenuViewModel BeginLoad()
            => new MainMenuViewModel(_slot, MainMenuState.Loading, null, null, false);

        /// <summary>
        /// 应用读档结果：成功 → 回到 HasSave 且 <see cref="LoadSucceeded"/>=true（调用方切场景）；
        /// 失败 → <see cref="MainMenuState.LoadError"/> 携带可行动原因，<b>不</b>进入游戏（零部分加载）。
        /// </summary>
        public MainMenuViewModel OnLoadResult(LoadResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.Succeeded
                ? new MainMenuViewModel(_slot, MainMenuState.HasSave, null, null, true)
                : new MainMenuViewModel(_slot, MainMenuState.LoadError, result.Reason, result.Error, false);
        }

        /// <summary>从错误态返回静止态（关闭错误提示）。</summary>
        public MainMenuViewModel DismissError()
            => new MainMenuViewModel(_slot, RestingState, null, null, false);

        /// <summary>请求退出 → 退出确认（根屏 Esc 不退游戏由 UXML 层保证；此处只建模确认态）。</summary>
        public MainMenuViewModel RequestQuit()
            => new MainMenuViewModel(_slot, MainMenuState.QuitConfirm, null, null, false);

        /// <summary>取消退出 → 返回静止态。</summary>
        public MainMenuViewModel CancelQuit()
            => new MainMenuViewModel(_slot, RestingState, null, null, false);
    }
}
