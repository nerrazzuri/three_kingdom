using System;

namespace ThreeKingdom.Presentation.Accessibility
{
    /// <summary>无障碍设置持久端口（ADR-0002）：跨会话加载/保存。</summary>
    public interface ISettingsStore
    {
        /// <summary>加载设置；缺失或损坏时优雅回落默认（不抛、不砸档）。</summary>
        SettingsLoadResult Load();

        /// <summary>原子保存设置（失败保留上一份有效设置，返回稳定错误）。</summary>
        SettingsSaveResult Save(AccessibilitySettings settings);
    }

    /// <summary>加载结果：始终带可用设置；<see cref="WasReset"/> 标记是否回落默认。</summary>
    public sealed class SettingsLoadResult
    {
        /// <summary>可用设置（成功=持久值；回落=默认值）。</summary>
        public AccessibilitySettings Settings { get; }
        /// <summary>是否回落到默认（缺失或损坏）。</summary>
        public bool WasReset { get; }
        /// <summary>回落原因（成功时为 null）。</summary>
        public string? Reason { get; }

        private SettingsLoadResult(AccessibilitySettings settings, bool wasReset, string? reason)
        {
            Settings = settings;
            WasReset = wasReset;
            Reason = reason;
        }

        /// <summary>成功加载持久值。</summary>
        public static SettingsLoadResult Loaded(AccessibilitySettings settings)
            => new SettingsLoadResult(settings, false, null);

        /// <summary>回落默认（缺失或损坏）。</summary>
        public static SettingsLoadResult Reset(string reason)
            => new SettingsLoadResult(AccessibilitySettings.Default, true, reason);
    }

    /// <summary>保存结果（成功 / 稳定错误码）。</summary>
    public sealed class SettingsSaveResult
    {
        /// <summary>是否成功。</summary>
        public bool Success { get; }
        /// <summary>失败原因（成功时为 null）。</summary>
        public string? Reason { get; }

        private SettingsSaveResult(bool success, string? reason)
        {
            Success = success;
            Reason = reason;
        }

        /// <summary>成功。</summary>
        public static SettingsSaveResult Ok() => new SettingsSaveResult(true, null);
        /// <summary>失败（写或改名异常）。</summary>
        public static SettingsSaveResult Failure(string reason) => new SettingsSaveResult(false, reason);
    }

    /// <summary>
    /// 无障碍设置仓库：编排<b>临时键 + 原子改名</b>的原子写回（镜像 epic-009 <c>SaveRepository</c>）。
    /// <list type="number">
    ///   <item>序列化为持久文本（<see cref="AccessibilitySettings.Serialize"/>）。</item>
    ///   <item>写入临时键 <c>{key}.tmp</c>。失败 → 清理临时键，正式键<b>未触碰</b>，返回稳定错误。</item>
    ///   <item>原子改名 <c>{key}.tmp</c> → <c>{key}</c>。改名失败 → 正式键保持上一份有效内容。</item>
    /// </list>
    /// 加载时损坏文本回落默认（设置损坏不应砸档，区别于存档的拒绝语义）。纯编排，介质经端口注入，可纯内存单测。
    /// </summary>
    public sealed class SettingsStore : ISettingsStore
    {
        /// <summary>默认设置键名。</summary>
        public const string DefaultKey = "accessibility";

        private readonly ISettingsMedium _medium;
        private readonly string _key;

        public SettingsStore(ISettingsMedium medium, string key = DefaultKey)
        {
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("设置键名不可为空。", nameof(key));
            _key = key;
        }

        /// <inheritdoc/>
        public SettingsLoadResult Load()
        {
            string? content = _medium.Read(_key);
            if (content == null) return SettingsLoadResult.Reset("无持久设置，使用默认。");

            try
            {
                return SettingsLoadResult.Loaded(AccessibilitySettings.Parse(content));
            }
            catch (FormatException ex)
            {
                // 损坏的设置文件回落默认（而非砸档）——guardrail：可调项跨会话持久但不致命。
                return SettingsLoadResult.Reset($"设置已损坏，已重置为默认：{ex.Message}");
            }
        }

        /// <inheritdoc/>
        public SettingsSaveResult Save(AccessibilitySettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            string tmp = _key + ".tmp";
            string content = settings.Serialize();

            try
            {
                _medium.Write(tmp, content); // 仅写临时键——正式键此刻未触碰
            }
            catch (Exception ex)
            {
                TryCleanup(tmp);
                return SettingsSaveResult.Failure($"临时键写入失败：{ex.Message}");
            }

            try
            {
                _medium.Move(tmp, _key); // 原子改名（要么成功，要么正式键保持原值）
            }
            catch (Exception ex)
            {
                TryCleanup(tmp);
                return SettingsSaveResult.Failure($"原子改名失败：{ex.Message}");
            }

            return SettingsSaveResult.Ok();
        }

        private void TryCleanup(string tmp)
        {
            try { _medium.Delete(tmp); } catch { /* 清理失败不掩盖原始错误 */ }
        }
    }
}
