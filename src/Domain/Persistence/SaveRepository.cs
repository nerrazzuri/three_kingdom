using System;

namespace ThreeKingdom.Domain.Persistence
{
    /// <summary>
    /// 存档仓库（ADR-0005 / TR-save-001）：编排<b>临时文件 + 原子改名</b>的原子写回。
    /// <list type="number">
    ///   <item>序列化快照为版本化 DTO 文本（经 <see cref="ISaveSerializer"/>）。</item>
    ///   <item>写入临时槽 <c>{slot}.tmp</c>。失败 → 清理临时槽，正式存档<b>未触碰</b>，返回稳定错误。</item>
    ///   <item>原子改名 <c>{slot}.tmp</c> → <c>{slot}</c>。改名失败 → 正式存档保持上一份有效内容。</item>
    /// </list>
    /// 任何失败路径都<b>不破坏</b>现有有效存档（guardrail：写失败保留上一份）。纯编排，介质经端口注入，可纯内存单测。
    /// </summary>
    public sealed class SaveRepository
    {
        private readonly ISaveMedium _medium;
        private readonly ISaveSerializer _serializer;

        public SaveRepository(ISaveMedium medium, ISaveSerializer serializer)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <summary>原子保存一个快照到 <paramref name="slot"/>（失败保留上一份有效存档）。</summary>
        public SaveResult Save(string slot, SaveSnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("槽名不可为空。", nameof(slot));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            string tmp = slot + ".tmp";
            string content = _serializer.Serialize(snapshot);

            try
            {
                _medium.Write(tmp, content); // 仅写临时槽——正式槽此刻未触碰
            }
            catch (Exception ex)
            {
                TryCleanup(tmp);
                return SaveResult.Failure(SaveErrorCode.TempWriteFailed, ex.Message);
            }

            try
            {
                _medium.Move(tmp, slot); // 原子改名（要么成功，要么正式槽保持原值）
            }
            catch (Exception ex)
            {
                TryCleanup(tmp);
                return SaveResult.Failure(SaveErrorCode.CommitFailed, ex.Message);
            }

            return SaveResult.Success();
        }

        /// <summary>读取并反序列化一个槽的原始快照（不做版本/指纹校验——那是加载服务的职责）。</summary>
        public SaveSnapshot? ReadRaw(string slot)
        {
            string? content = _medium.Read(slot);
            return content == null ? null : _serializer.Deserialize(content);
        }

        private void TryCleanup(string tmp)
        {
            try { _medium.Delete(tmp); } catch { /* 清理失败不掩盖原始错误 */ }
        }
    }
}
