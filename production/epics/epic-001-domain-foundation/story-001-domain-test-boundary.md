# Story 001: 建立纯 C# Domain 与测试边界

> **Epic**: 项目与 Domain 基础
> **Status**: Complete
> **Layer**: Foundation
> **Type**: Integration
> **Estimate**: M（4h）
> **Manifest Version**: 1 (2026-06-21)
> **Last Updated**: 2026-06-21

## Context

**GDD**: 横切（systems-index「命令与因果链」「权威状态与系统契约」）
**Requirement**: 前置能力 — Domain 程序集边界（服务全部下游 TR）

**ADR Governing Implementation**: ADR-0002 架构分层
**ADR Decision Summary**: Domain/Application/Infrastructure/Presentation 四层 + 单向依赖；Domain 为纯 C#，不依赖 UnityEngine。

**Engine**: Unity 6.3 LTS + C# | **Risk**: HIGH
**Engine Notes**: 用 asmdef 隔离 Domain 程序集，禁止引用 UnityEngine 程序集；Unity 6.3 程序集定义引用须显式（post-cutoff 行为，落地时验证 EditMode/PlayMode 测试程序集装配）。

**Control Manifest Rules (Foundation)**:
- Required: gameplay state 只由 Domain 经 Application Command 路径修改
- Forbidden: Domain 层出现 MonoBehaviour 或任何 UnityEngine.* 类型
- Guardrail: Domain 单元测试不依赖帧率、文件 I/O、外部 API

---

## Acceptance Criteria

- [x] 存在独立 Domain 程序集（`src/Domain/ThreeKingdom.Domain.csproj`，netstandard2.1），不引用 UnityEngine — 由 `DomainBoundaryTests.Domain_assembly_references_no_unity_assembly` 反射断言
- [x] 存在独立测试程序集（`tests/unit/ThreeKingdom.Domain.Tests`，NUnit），可对 Domain public API 编写正常/边界/失败用例 — `BuildInfoTests` + `DomainBoundaryTests` 即为证
- [x] `dotnet test` 可在无 Unity 运行时下执行 Domain 单元测试（net10.0 测试引用 netstandard2.1 库；CI `domain-tests` job 旁路 UNITY_LICENSE）
- [x] 一个示例 Domain 类型 + 其示例测试通过（`BuildInfo` + `BuildInfoTests`，确认框架可用——满足进 Production 硬前置之示例测试项）

---

## Implementation Notes

*Derived from ADR-0002:*
- Domain 程序集仅依赖 BCL；Application 依赖 Domain + 抽象端口；Infrastructure 实现端口；Presentation 依赖 Application API。
- 用 asmdef 的 "No Engine References" / 不勾选 UnityEngine 引用强制边界。
- 测试用纯 C#（slice 已验证 .NET 控制台可驱动真实 Domain）；CI 经 game-ci/unity-test-runner（需先配 UNITY_LICENSE secret）。

---

## Out of Scope

- Story 002: 定点数值/随机流底座
- Story 003: 配置加载
- Story 004: SaveVersion

---

## QA Test Cases

- **AC-1**: Domain 程序集不引用 UnityEngine
  - Given: 已构建的 Domain 程序集
  - When: 检查其程序集引用 / 反射 ReferencedAssemblies
  - Then: 不含 UnityEngine.* 任何引用
  - Edge cases: 间接经第三方库引入 UnityEngine 也须失败
- **AC-2**: 示例 Domain 测试在无 Unity 运行时下通过
  - Given: 测试程序集 + 一个示例 Domain 类型
  - When: 运行 `dotnet test` / EditMode runner
  - Then: 示例测试通过，进程无 Unity 运行时依赖
  - Edge cases: 测试不读文件/不依赖时钟

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- `tests/unit/ThreeKingdom.Domain.Tests/DomainBoundaryTests.cs` — 3 测（无 Unity 引用 / 无 Unity 命名空间 public 类型 / UnityEngine 查找为空）
- `tests/unit/ThreeKingdom.Domain.Tests/BuildInfoTests.cs` — 2 测（框架确认：DomainMarker 稳定 / Echo 确定性）
- 工程：`src/Domain/ThreeKingdom.Domain.csproj`(netstandard2.1) + `tests/unit/ThreeKingdom.Domain.Tests/*.csproj`(net10.0+NUnit) + `ThreeKingdom.slnx`
> 注：原拟路径 `tests/integration/foundation/domain_test_boundary_test.cs` 调整为统一测试程序集内（`ThreeKingdom.Domain.Tests`），单一 .NET 测试工程更易 CI 装配。
**Status**: [x] 已创建并通过 — `dotnet test ThreeKingdom.slnx` → 5/5 绿（2026-06-21，本地）

---

## Dependencies

- Depends on: None（首个 story）
- Unlocks: Story 002, 003, 004（全部 Domain 工作）

## Completion Notes
**Completed**: 2026-06-21
**Criteria**: 4/4 passing（无 deferred）
**Deviations**: ADVISORY — 测试落于统一工程 `tests/unit/ThreeKingdom.Domain.Tests`（非原拟 `tests/integration/foundation/`）；实现 inline（非子代理）。均功能等价、无阻断。
**Test Evidence**: `tests/unit/ThreeKingdom.Domain.Tests/DomainBoundaryTests.cs`(3) + `BuildInfoTests.cs`(2) + 工程 `src/Domain` + `ThreeKingdom.slnx` — `dotnet test` 5/5 绿（2026-06-21 本地）
**Code Review**: Complete — `/code-review` APPROVED（2026-06-21，inline 审查）
**回归基线**: DomainBoundaryTests 为后续 Domain 程序集「禁 UnityEngine」永久回归门。
