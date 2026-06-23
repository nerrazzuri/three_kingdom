using System;
using ThreeKingdom.Domain.Configuration;
using ThreeKingdom.Domain.Persistence;

namespace ThreeKingdom.Application.Session
{
    /// <summary>会话加载结果（Application 封装 Domain <see cref="LoadResult"/>，成功携带恢复出的会话）。不可变。</summary>
    public sealed class SessionLoadResult
    {
        /// <summary>是否成功。</summary>
        public bool Succeeded { get; }
        /// <summary>恢复出的会话（成功非空）。</summary>
        public GameSession? Session { get; }
        /// <summary>错误码（稳定枚举，供 UI 错误态）。</summary>
        public LoadErrorCode Error { get; }
        /// <summary>可行动原因。</summary>
        public string Reason { get; }

        private SessionLoadResult(bool ok, GameSession? session, LoadErrorCode error, string reason)
        {
            Succeeded = ok;
            Session = session;
            Error = error;
            Reason = reason ?? string.Empty;
        }

        internal static SessionLoadResult Success(GameSession session) => new SessionLoadResult(true, session, LoadErrorCode.None, string.Empty);
        internal static SessionLoadResult Failure(LoadErrorCode error, string reason) => new SessionLoadResult(false, null, error, reason);
    }

    /// <summary>
    /// 会话存档/读档协调器（ADR-0005 用例编排）：以注入的 <see cref="ISaveMedium"/> 经真实
    /// <see cref="SaveRepository"/>（原子写）与 <see cref="SaveLoadService"/>（先校验后载入）落地存档。
    /// 当前运行版本/配置指纹为 slice 常量；存档 ↔ 会话映射委托 <see cref="SaveMapper"/>。
    /// 读档成功返回<b>恢复出的新会话</b>，失败<b>不</b>影响当前会话（零部分载入，TR-save-003）。
    /// </summary>
    public sealed class SaveCoordinator
    {
        /// <summary>slice 当前存档 schema 版本。</summary>
        public static readonly SaveVersion CurrentVersion = new SaveVersion(1, 0);

        /// <summary>slice 当前配置指纹（固定常量；量产期由配置管线计算，ADR-0003）。</summary>
        public static readonly ConfigFingerprint CurrentFingerprint = new ConfigFingerprint(0x51CE_C0F1_2026_0001UL);

        private readonly SaveRepository _repository;
        private readonly SaveLoadService _loader;

        public SaveCoordinator(ISaveMedium medium)
        {
            if (medium == null) throw new ArgumentNullException(nameof(medium));
            var serializer = new CanonicalSaveSerializer();
            _repository = new SaveRepository(medium, serializer);
            _loader = new SaveLoadService(medium, serializer, new SaveMigrator(Array.Empty<ISaveMigration>()));
        }

        /// <summary>原子保存会话到槽（失败保留上一份有效存档，TR-save-001）。</summary>
        public SaveResult Save(string slot, GameSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            SaveSnapshot snapshot = SaveMapper.ToSnapshot(session, CurrentVersion, CurrentFingerprint);
            return _repository.Save(slot, snapshot);
        }

        /// <summary>校验并加载槽，恢复为新会话；失败返回错误码 + 可行动原因（不动当前会话）。</summary>
        public SessionLoadResult Load(string slot)
        {
            LoadResult result = _loader.Load(slot, CurrentVersion, CurrentFingerprint);
            if (!result.Succeeded)
                return SessionLoadResult.Failure(result.Error, result.Reason);
            return SessionLoadResult.Success(SaveMapper.FromSnapshot(result.Snapshot!, SliceScenario.Default()));
        }
    }
}
