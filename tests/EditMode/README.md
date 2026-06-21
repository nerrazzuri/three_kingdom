# Edit Mode 测试

不进入 Play Mode 即可运行的测试。用于纯逻辑：公式、状态机、数据校验、Domain 规则。

- 需要 Assembly Definition：`tests/EditMode/EditModeTests.asmdef`
- **Domain 层测试程序集不得引用 `UnityEngine`**（ADR-0002：Domain 可独立 `dotnet test`）。
- 命名：文件 `[system]_[feature]_test.cs`，方法 `Test_Scenario_Expected`。

## 本项目优先 Edit Mode 覆盖的内容

- 时间推进确定性（同种子→同结果，ADR-0004）
- 配置校验：非法范围/缺失交叉引用被拒（ADR-0003）
- Command 前置校验：失败稳定错误码、无部分写入（ADR-0002）
- 资源守恒（城市/后勤库存转移）
