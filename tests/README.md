# 测试基础设施

**引擎**：Unity 6.3 LTS
**测试框架**：Unity Test Framework（NUnit）
**CI**：`.github/workflows/tests.yml`
**搭建日期**：2026-06-21

## 目录布局

```
tests/
  unit/           # 隔离单元测试（公式、状态机、Domain 规则）— 纯 C#，无 UnityEngine 依赖
  integration/    # 跨系统测试与存档 round-trip
  smoke/          # /smoke-check 读取的关键路径冒烟清单
  evidence/       # 截图日志与人工测试签核记录
  EditMode/       # Edit Mode 测试（纯逻辑，不进 Play Mode）
  PlayMode/       # Play Mode 测试（跨系统/场景集成）
```

## 运行测试

**Unity 编辑器**：Window → General → Test Runner → 选择 EditMode 或 PlayMode → Run All

**命令行（CI 同款）**：
```
Unity -batchmode -runTests -projectPath . -testPlatform EditMode -testResults results-editmode.xml
Unity -batchmode -runTests -projectPath . -testPlatform PlayMode -testResults results-playmode.xml
```

## 测试命名

- **文件**：`[system]_[feature]_test.cs`
- **函数**：`test_[scenario]_[expected]`（或 NUnit `[Test]` 方法名 `Test_Scenario_Expected`）
- **示例**：`battle_resolution_test.cs` → `Test_SameSeed_ProducesSameStateHash()`

## 分层测试约定（本项目）

- **Domain 层**：纯 C# 单元测试，放 `tests/unit/[system]/`，**禁止** `UnityEngine.*` 依赖，
  可独立 `dotnet test`（见 ADR-0002）。
- **Application 层**：用例测试，放 `tests/unit/[system]/` 或 `tests/integration/`。
- **EditMode**：不进 Play Mode 的纯逻辑测试（公式、配置校验、状态机）。
- **PlayMode**：需真实场景的集成测试（跨系统、协程）。

### 必测项（technical-preferences.md）

- 时间推进确定性（同一种子 → 同一结果）— 见 ADR-0004
- 战役条件链结算（各条件独立可验证）
- 存档读档 round-trip 状态一致性 — 见 ADR-0005
- 配置校验（非法范围与缺失引用被拒绝）— 见 ADR-0003
- Command 前置校验（失败返回稳定错误码，无部分写入）— 见 ADR-0002

## Story 类型 → 测试证据

| Story 类型 | 必需证据 | 位置 | 门槛 |
|---|---|---|---|
| Logic（公式/AI/状态机） | 自动化单元测试——须通过 | `tests/unit/[system]/` | BLOCKING |
| Integration（跨系统） | 集成测试 OR 记录的 playtest | `tests/integration/[system]/` | BLOCKING |
| Visual/Feel（动画/VFX/手感） | 截图 + lead 签核 | `production/qa/evidence/` | ADVISORY |
| UI（菜单/HUD/界面） | 人工走查文档 OR 交互测试 | `production/qa/evidence/` | ADVISORY |
| Config/Data（平衡调参） | 冒烟检查通过 | `production/qa/smoke-*.md` | ADVISORY |

> 注：可视/UI/手感类证据按 coding-standards 归档于 `production/qa/evidence/`；
> `tests/evidence/` 用于测试运行附带的截图与签核草稿。

## 启用 Unity Test Framework

```
Window → General → Test Runner
（Unity Test Framework 在 Unity 2019+ 默认包含）
```

EditMode/PlayMode 测试各需一个 Assembly Definition（`.asmdef`），
Domain 单元测试程序集不得引用 UnityEngine（保证可独立编译测试）。

## CI

测试在每次 push 到 `main` 与每个 PR 上自动运行。测试套件失败将阻止合并。

> **首次 CI 前须配置**：GitHub 仓库 Settings → Secrets → 添加 `UNITY_LICENSE`。
