# 单元测试

隔离的单元测试（公式、状态机、Domain 规则）。每个系统一个子目录，例如 `tests/unit/battle/`、
`tests/unit/save/`。Domain 层测试为纯 C#，**不得**依赖 `UnityEngine`，可独立 `dotnet test`（ADR-0002）。
