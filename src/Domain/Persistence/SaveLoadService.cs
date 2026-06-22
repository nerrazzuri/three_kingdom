using System;
using ThreeKingdom.Domain.Configuration;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 存档加载校验服务（ADR-0005 / TR-save-003 + TR-intel-003）。
    /// 加载<b>先校验后载入</b>，任一校验不过即拒绝且<b>零部分载入当前会话</b>（纯函数：失败只返回错误，不改任何状态）：
    /// <list type="number">
    ///   <item><b>结构完整性</b>：读取并反序列化；损坏/截断（含必需分段缺失）→ <see cref="LoadErrorCode.Corrupted"/>。</item>
    ///   <item><b>版本兼容</b>：存档版本高于当前 → <see cref="LoadErrorCode.IncompatibleNewer"/>（不静默降级）。</item>
    ///   <item><b>配置指纹</b>：与当前不符 → <see cref="LoadErrorCode.FingerprintMismatch"/>。</item>
    ///   <item><b>迁移</b>：旧档经迁移链升级到当前；失败 → <see cref="LoadErrorCode.MigrationFailed"/>。</item>
    /// </list>
    /// 真值段与知识段在反序列化时各归各位、<b>不交叉污染</b>（TR-intel-003）；知识段缺失走结构完整性拒绝而非用真值回填。
    /// </summary>
    public sealed class SaveLoadService
    {
        private readonly ISaveMedium _medium;
        private readonly ISaveSerializer _serializer;
        private readonly SaveMigrator _migrator;

        public SaveLoadService(ISaveMedium medium, ISaveSerializer serializer, SaveMigrator migrator)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _migrator = migrator ?? throw new ArgumentNullException(nameof(migrator));
        }

        /// <summary>校验并加载一个槽到当前 (<paramref name="current"/>, <paramref name="currentFingerprint"/>) 运行环境。</summary>
        public LoadResult Load(string slot, SaveVersion current, ConfigFingerprint currentFingerprint)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));

            string? text = _medium.Read(slot);
            if (text == null)
                return LoadResult.Failure(LoadErrorCode.SlotEmpty, $"槽「{slot}」无存档。");

            // 1) 结构完整性（含必需分段缺失 → 截断异常）。
            SaveSnapshot snapshot;
            try
            {
                snapshot = _serializer.Deserialize(text);
            }
            catch (SaveFormatException ex)
            {
                return LoadResult.Failure(LoadErrorCode.Corrupted, $"存档损坏：{ex.Message}");
            }

            // 2) 版本兼容（高于当前不部分载入、不静默降级）。
            var compatibility = snapshot.Version.ClassifyForLoad(current);
            if (compatibility == SaveCompatibility.IncompatibleNewer)
                return LoadResult.Failure(LoadErrorCode.IncompatibleNewer,
                    $"存档版本 {snapshot.Version} 高于当前 {current}，请升级游戏后再读取（不会改动当前进度）。");

            // 3) 配置指纹比对（配置已变，加载不安全）。
            if (snapshot.Fingerprint != currentFingerprint)
                return LoadResult.Failure(LoadErrorCode.FingerprintMismatch,
                    $"存档配置指纹 {snapshot.Fingerprint} 与当前 {currentFingerprint} 不符（配置已变更）。");

            // 4) 迁移到当前版本（兼容则空操作）。
            if (compatibility == SaveCompatibility.Migratable)
            {
                MigrationResult migration = _migrator.Migrate(snapshot, current);
                if (!migration.Succeeded)
                    return LoadResult.Failure(LoadErrorCode.MigrationFailed,
                        $"存档迁移失败（{migration.Error}）：{migration.Detail}");
                snapshot = migration.Result!;
            }

            return LoadResult.Success(snapshot);
        }
    }
}
