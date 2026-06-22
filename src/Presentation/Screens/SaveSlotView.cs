using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Presentation.Screens
{
    /// <summary>
    /// 存档槽的展示投影（ADR-0002：UI 只读投影）。供主菜单/暂停判断「继续/读档」可用性。
    /// 不含权威状态写路径；版本标签仅供显示。不可变。
    /// </summary>
    public sealed class SaveSlotView
    {
        /// <summary>槽名。</summary>
        public string Slot { get; }

        /// <summary>该槽是否有存档。</summary>
        public bool HasSave { get; }

        /// <summary>版本标签（无存档为空串；仅显示用）。</summary>
        public string VersionLabel { get; }

        private SaveSlotView(string slot, bool hasSave, string versionLabel)
        {
            Slot = slot;
            HasSave = hasSave;
            VersionLabel = versionLabel;
        }

        /// <summary>从一份（可空）已读快照构造槽投影：null → 无存档。</summary>
        public static SaveSlotView Present(string slot, SaveSnapshot? snapshot)
            => snapshot == null
                ? new SaveSlotView(slot, false, string.Empty)
                : new SaveSlotView(slot, true, snapshot.Version.ToString());

        /// <summary>显式构造一个无存档槽。</summary>
        public static SaveSlotView Empty(string slot) => new SaveSlotView(slot, false, string.Empty);
    }
}
