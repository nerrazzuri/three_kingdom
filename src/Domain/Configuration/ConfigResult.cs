using System;
using System.Collections.Generic;

namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 配置校验失败的稳定错误码（ADR-0003 §2：失败「报稳定错误码」）。
    /// 数值显式固定，跨版本稳定，可用于日志/测试断言；新增码须追加不得复用旧值。
    /// </summary>
    public enum ConfigErrorCode
    {
        /// <summary>数值字段超出 schema 声明的合法闭区间。</summary>
        ValueOutOfRange = 1,

        /// <summary>交叉引用指向不存在的 stable_id。</summary>
        MissingReference = 2,

        /// <summary>schema 声明的必填字段缺失。</summary>
        MissingRequiredField = 3,

        /// <summary>条目 schema 版本与目标 schema 不符。</summary>
        SchemaVersionMismatch = 4,

        /// <summary>同一配置集内出现重复 stable_id。</summary>
        DuplicateStableId = 5,

        /// <summary>条目含 schema 未声明的字段（schema 为权威，拒绝未知字段）。</summary>
        UnknownField = 6,
    }

    /// <summary>
    /// 单条配置校验错误。<see cref="EntryId"/>/<see cref="Field"/> 指出定位（可空——非字段级错误时为 null）。
    /// </summary>
    public sealed class ConfigError
    {
        /// <summary>稳定错误码。</summary>
        public ConfigErrorCode Code { get; }

        /// <summary>人类可读说明（仅诊断用，非稳定契约）。</summary>
        public string Message { get; }

        /// <summary>出错条目的 stable_id（若适用）。</summary>
        public string? EntryId { get; }

        /// <summary>出错字段名（若适用）。</summary>
        public string? Field { get; }

        public ConfigError(ConfigErrorCode code, string message, string? entryId = null, string? field = null)
        {
            Code = code;
            Message = message ?? string.Empty;
            EntryId = entryId;
            Field = field;
        }

        public override string ToString() => $"[{(int)Code} {Code}] {EntryId}.{Field}: {Message}";
    }

    /// <summary>
    /// 配置加载/校验结果（ADR-0003：失败不产生部分加载状态）。
    /// 成功时携带值且 <see cref="Errors"/> 为空；失败时无 <see cref="Value"/>（访问抛异常），携聚合错误。
    /// </summary>
    /// <typeparam name="T">成功值类型。</typeparam>
    public readonly struct Result<T>
    {
        private static readonly IReadOnlyList<ConfigError> NoErrors = Array.Empty<ConfigError>();

        private readonly T _value;

        /// <summary>是否成功。</summary>
        public bool IsSuccess { get; }

        /// <summary>聚合错误（成功时为空列表，恒非 null）。</summary>
        public IReadOnlyList<ConfigError> Errors { get; }

        private Result(bool success, T value, IReadOnlyList<ConfigError> errors)
        {
            IsSuccess = success;
            _value = value;
            Errors = errors;
        }

        /// <summary>构造成功结果。</summary>
        public static Result<T> Success(T value) => new Result<T>(true, value, NoErrors);

        /// <summary>构造失败结果；错误列表不可为空（至少一条）。</summary>
        public static Result<T> Failure(IReadOnlyList<ConfigError> errors)
        {
            if (errors == null || errors.Count == 0)
                throw new ArgumentException("失败结果必须至少含一个错误。", nameof(errors));
            var copy = new ConfigError[errors.Count];
            for (int i = 0; i < errors.Count; i++) copy[i] = errors[i];
            return new Result<T>(false, default!, copy);
        }

        /// <summary>成功值；失败时抛 <see cref="InvalidOperationException"/>（无部分写入，禁止读取）。</summary>
        public T Value =>
            IsSuccess ? _value : throw new InvalidOperationException("Result 失败时无 Value（ADR-0003：无部分写入）。");
    }
}
