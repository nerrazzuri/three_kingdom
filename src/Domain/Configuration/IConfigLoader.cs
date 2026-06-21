namespace ThreeKingdom.Domain.Configuration
{
    /// <summary>
    /// 配置加载端口（ADR-0002：Domain 定义端口，Infrastructure 实现；ADR-0003 §Key Interfaces）。
    /// 实现方负责解析（SO 编辑期产物 / JSON）、默认值展开，并经 <see cref="ConfigValidator"/> 校验，
    /// 返回不可变 <see cref="ValidatedConfig"/> 或聚合错误（失败无部分写入）。
    /// Domain/Application 仅依赖本接口，不依赖任何 Unity 类型。
    /// </summary>
    public interface IConfigLoader
    {
        /// <summary>
        /// 加载并校验一个配置集：解析 + 默认值展开 + 完整校验。
        /// 成功返回不可变配置；任一非法范围/缺失引用导致整体拒绝且无部分加载状态。
        /// </summary>
        /// <param name="id">配置集标识。</param>
        Result<ValidatedConfig> Load(ConfigSetId id);

        /// <summary>
        /// 计算已校验配置的确定性指纹（供 ADR-0004 状态哈希 / ADR-0005 存档兼容判定）。
        /// </summary>
        /// <param name="config">已校验配置。</param>
        ConfigFingerprint Fingerprint(ValidatedConfig config);
    }
}
