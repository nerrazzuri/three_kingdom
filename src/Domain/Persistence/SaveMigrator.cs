using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>迁移错误码（稳定枚举）。</summary>
    public enum MigrationErrorCode
    {
        /// <summary>无错误。</summary>
        None = 0,

        /// <summary>链中缺少从某版本继续的迁移步骤（断链）。</summary>
        NoMigrationPath = 1,

        /// <summary>某迁移步骤执行抛错；原存档保留未变。</summary>
        StepFailed = 2,

        /// <summary>存档版本高于当前（不可向下迁移；属不兼容拒绝，TR-save-003）。</summary>
        IncompatibleNewer = 3,
    }

    /// <summary>迁移结果（不可变）。失败时 <see cref="Result"/> 为 null，调用方应继续持有原存档（未变）。</summary>
    public sealed class MigrationResult
    {
        /// <summary>是否成功迁移到当前版本。</summary>
        public bool Succeeded { get; }

        /// <summary>升级后的快照（成功非空）。</summary>
        public SaveSnapshot? Result { get; }

        /// <summary>错误码。</summary>
        public MigrationErrorCode Error { get; }

        /// <summary>可读细节。</summary>
        public string Detail { get; }

        private MigrationResult(bool ok, SaveSnapshot? result, MigrationErrorCode error, string detail)
        {
            Succeeded = ok;
            Result = result;
            Error = error;
            Detail = detail ?? string.Empty;
        }

        internal static MigrationResult Success(SaveSnapshot s) => new MigrationResult(true, s, MigrationErrorCode.None, string.Empty);
        internal static MigrationResult Failure(MigrationErrorCode e, string d) => new MigrationResult(false, null, e, d);
    }

    /// <summary>
    /// 逐版迁移链执行器（ADR-0005 / TR-save-001）。把旧版本快照沿注册的单步迁移升级到当前版本：
    /// <list type="bullet">
    ///   <item>只操作<b>副本</b>（快照不可变，逐步产出新实例）；任一步失败 → 返回错误，<b>原存档保留未变</b>。</item>
    ///   <item>断链（缺少从当前工作版本继续的步骤）→ <see cref="MigrationErrorCode.NoMigrationPath"/>。</item>
    ///   <item>存档版本高于当前 → <see cref="MigrationErrorCode.IncompatibleNewer"/>（不向下迁移）。</item>
    /// </list>
    /// 确定性：每个 <c>From</c> 至多一个迁移步骤（注册重复抛错），故链路唯一。
    /// </summary>
    public sealed class SaveMigrator
    {
        private readonly Dictionary<SaveVersion, ISaveMigration> _byFrom = new Dictionary<SaveVersion, ISaveMigration>();

        /// <summary>以一组单步迁移构造；同一 From 出现多次抛 <see cref="ArgumentException"/>（链路须确定）。</summary>
        public SaveMigrator(IEnumerable<ISaveMigration> migrations)
        {
            if (migrations == null) throw new ArgumentNullException(nameof(migrations));
            foreach (var m in migrations)
            {
                if (m == null) throw new ArgumentException("迁移集合不可含 null。", nameof(migrations));
                if (m.To <= m.From) throw new ArgumentException($"迁移须递增版本（{m.From}→{m.To}）。", nameof(migrations));
                if (_byFrom.ContainsKey(m.From)) throw new ArgumentException($"版本 {m.From} 存在重复迁移步骤（链路须唯一）。", nameof(migrations));
                _byFrom[m.From] = m;
            }
        }

        /// <summary>把快照迁移到 <paramref name="current"/> 当前版本。</summary>
        public MigrationResult Migrate(SaveSnapshot snapshot, SaveVersion current)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            SaveSnapshot working = snapshot; // 不可变 → 持有原引用即「原档保留」
            int guard = 0;
            int maxSteps = _byFrom.Count + 1;

            while (working.Version != current)
            {
                if (working.Version > current)
                    return MigrationResult.Failure(MigrationErrorCode.IncompatibleNewer,
                        $"存档版本 {working.Version} 高于当前 {current}，不可向下迁移。");

                if (!_byFrom.TryGetValue(working.Version, out var step))
                    return MigrationResult.Failure(MigrationErrorCode.NoMigrationPath,
                        $"缺少从版本 {working.Version} 继续的迁移步骤（断链）。");

                if (++guard > maxSteps)
                    return MigrationResult.Failure(MigrationErrorCode.NoMigrationPath, "迁移步数异常（疑似环）。");

                SaveSnapshot next;
                try
                {
                    next = step.Apply(working); // 对副本应用；working/snapshot 不被修改
                }
                catch (Exception ex)
                {
                    return MigrationResult.Failure(MigrationErrorCode.StepFailed,
                        $"迁移步骤 {step.From}→{step.To} 失败：{ex.Message}（原存档保留）。");
                }

                if (next == null || next.Version != step.To)
                    return MigrationResult.Failure(MigrationErrorCode.StepFailed,
                        $"迁移步骤 {step.From}→{step.To} 未产出正确目标版本。");

                working = next;
            }

            return MigrationResult.Success(working);
        }
    }
}
