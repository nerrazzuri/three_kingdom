using System;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>暂停菜单状态（pause-menu §6/§14：默认/有草稿/保存中/保存失败/退出确认）。</summary>
    public enum PauseMenuState
    {
        /// <summary>默认。</summary>
        Default = 0,
        /// <summary>有未提交草稿。</summary>
        HasDraft = 1,
        /// <summary>保存中。</summary>
        Saving = 2,
        /// <summary>保存失败。</summary>
        SaveError = 3,
        /// <summary>退出确认。</summary>
        QuitConfirm = 4,
    }

    /// <summary>
    /// 暂停菜单展示状态机（pause-menu §14 / ADR-0002 + ADR-0005）。<b>不可变</b>转移。
    /// 本屏<b>不</b>直接写权威状态——保存经端口（epic-009 <see cref="SaveResult"/>，原子写失败给可行动原因）；
    /// 有未提交草稿时退出/读取先经 P9 处置（<see cref="RequiresDraftDisposition"/>）。
    /// </summary>
    public sealed class PauseMenuViewModel
    {
        /// <summary>当前状态。</summary>
        public PauseMenuState State { get; }

        /// <summary>是否有未提交草稿。</summary>
        public bool HasDraft { get; }

        /// <summary>是否正等待 P9 草稿处置（退出/读取被草稿拦截）。</summary>
        public bool RequiresDraftDisposition { get; }

        /// <summary>是否获准继续退出/读取（无草稿或已处置）。</summary>
        public bool MayProceed { get; }

        /// <summary>保存失败原因（仅 <see cref="PauseMenuState.SaveError"/> 非空）。</summary>
        public string? ErrorReason { get; }

        /// <summary>保存失败错误码（仅 SaveError 非空）。</summary>
        public SaveErrorCode? ErrorCode { get; }

        private PauseMenuViewModel(PauseMenuState state, bool hasDraft, bool requiresDisposition, bool mayProceed,
            string? errorReason, SaveErrorCode? errorCode)
        {
            State = state;
            HasDraft = hasDraft;
            RequiresDraftDisposition = requiresDisposition;
            MayProceed = mayProceed;
            ErrorReason = errorReason;
            ErrorCode = errorCode;
        }

        /// <summary>打开暂停菜单（有草稿 → HasDraft）。</summary>
        public static PauseMenuViewModel Open(bool hasDraft)
            => new PauseMenuViewModel(hasDraft ? PauseMenuState.HasDraft : PauseMenuState.Default, hasDraft, false, false, null, null);

        private PauseMenuState RestingState => HasDraft ? PauseMenuState.HasDraft : PauseMenuState.Default;

        /// <summary>开始保存（进入 Saving）。</summary>
        public PauseMenuViewModel BeginSave()
            => new PauseMenuViewModel(PauseMenuState.Saving, HasDraft, false, false, null, null);

        /// <summary>
        /// 应用保存结果：成功 → 回到静止态；失败 → <see cref="PauseMenuState.SaveError"/> 携带可行动原因
        /// （原子写失败不留部分状态，guardrail）。
        /// </summary>
        public PauseMenuViewModel OnSaveResult(SaveResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.Succeeded
                ? new PauseMenuViewModel(RestingState, HasDraft, false, false, null, null)
                : new PauseMenuViewModel(PauseMenuState.SaveError, HasDraft, false, false, result.Detail, result.Error);
        }

        /// <summary>关闭保存失败提示。</summary>
        public PauseMenuViewModel DismissSaveError()
            => new PauseMenuViewModel(RestingState, HasDraft, false, false, null, null);

        /// <summary>
        /// 请求退出/读取：有草稿 → 拦截要求 P9 处置（<see cref="RequiresDraftDisposition"/>=true，不立即放行）；
        /// 无草稿 → <see cref="MayProceed"/>=true。
        /// </summary>
        public PauseMenuViewModel RequestExitOrLoad()
            => HasDraft
                ? new PauseMenuViewModel(RestingState, true, true, false, null, null)
                : new PauseMenuViewModel(RestingState, false, false, true, null, null);

        /// <summary>P9 草稿处置：keepDraft=false 丢弃草稿后放行；keepDraft=true 保留草稿、取消退出/读取。</summary>
        public PauseMenuViewModel DisposeDraft(bool keepDraft)
            => keepDraft
                ? new PauseMenuViewModel(PauseMenuState.HasDraft, true, false, false, null, null)
                : new PauseMenuViewModel(PauseMenuState.Default, false, false, true, null, null);
    }
}
