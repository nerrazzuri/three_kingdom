# Play Mode 测试

在真实游戏场景中运行的集成测试。用于跨系统交互、场景生命周期与协程。

- 需要 Assembly Definition：`tests/PlayMode/PlayModeTests.asmdef`
- 命名：文件 `[system]_[feature]_test.cs`，方法 `Test_Scenario_Expected`。

## 本项目优先 Play Mode 覆盖的内容

- 存档读档 round-trip 状态一致性（ADR-0005）
- Presentation → Command → Application → Domain → Event → Projection 完整贯通用例（ADR-0002）
- Vertical Slice 最短闭环（小城防御场景）的跨系统集成
