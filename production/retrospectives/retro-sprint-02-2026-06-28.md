# Retrospective: Sprint 02（Meta 层 Domain 内核）
Period: 2026-06-24 -- 2026-06-28（计划 06-24~07-07）
Generated: 2026-06-28

Goal：epic-011 战役与生涯 + epic-012 条件历史世界模型的 Domain 内核与标志性机制。

### Metrics

| Metric | Planned | Actual | Delta |
|--------|---------|--------|-------|
| Stories | 11（Must 5 + Should 3 + Nice 3） | 11 | 0 |
| Completion Rate | -- | 100% | -- |
| Effort (estimate_days 合计) | ~7.5d | 完成（单批会话推进） | -- |
| Bugs Found | -- | 0 | -- |
| Unplanned Tasks | -- | 2（补缺 ICityControlAuthority / LordMissionLog，均为已知 ADR 接缝） | -- |
| Story Commits | -- | 11（cf00edb…dc2db30） | -- |
| Tests Added | -- | ~150（451→556 全绿） | -- |

### Velocity Trend

| Sprint | Planned | Completed | Rate |
|--------|---------|-----------|------|
| Vertical Slice（pre-prod） | 3 链 | 3 链 | 100%（PROCEED 签核） |
| Sprint 01（Foundation） | 随竖切 | Complete | 100%（无 carryover） |
| Sprint 02（current） | 11 story | 11 | 100% |

**Trend**: Stable-high。Meta 层全清，Must+Should+Nice 一次性完成。

### What Went Well
- **100% 完成且零缺陷**：11 story 全过，全套 dotnet 556/556 绿、-warnaserror 0；smoke PASS、team-qa APPROVED。
- **纯 Domain + dotnet/NUnit 旁路 Unity 许可**奏效：确定性可测、CI 友好，全程无引擎许可阻塞。
- **ADR 驱动落地顺滑**：8 份 Accepted ADR 直接指导实现（0007 触发门、0008 控制权契约尤其清晰），实现与设计零漂移。
- **组合式而非侵入式扩展**：11-2/11-3 在 11-1 命令路径之上叠加（未改其已提交 switch）；存档段独立 codec（生涯/世界同构）——降低回归风险。
- **确定性纪律保持**：零 float 入权威路径、零旁路随机（抽象结算仅注入 IDeterministicRandom）、规范字节序哈希、好感快照隔离——全部有测试守门。
- **技术债清零**：src 中 0 TODO/FIXME/HACK。

### What Went Poorly
- **跨 epic 接缝在实现期才补**：ADR-0008 的 `ICityControlAuthority`/`CityControlChanged` 在 epic-004 标 Complete 后才由 ADR-0008 定义、本 sprint 11-4 才落地；`LordMissionLog` 同理在 11-5 才建。虽属"已知接缝、若缺则补"，但更早识别可避免 story 内临时补缺。影响小（均一次成型、有测试）。
- **测试路径系统性偏离 story header**：所有 story 实际落 `ThreeKingdom.Domain.Tests/{Career,World}/`，与 header 拟的 `tests/unit|integration/...` 不符（沿 epic-001 起的工程约定）。已逐一在 Completion Notes 注明，但 story 模板路径与项目实际约定长期不一致，属文档债。
- **存档三段未物理统一**：生涯段（11-5）、世界段（12-6）各自 codec，尚未与 epic-009 信封 + 原子写整合为单一 GDD_013 存档。MVP 可接受，但留整合项。
- **命名空间碰撞**：新 `Outcome` 类与既有 `ThreeKingdom.Domain.Outcome` 命名空间冲突，实现期改名 `HistoricalOutcome`。registry 命名校验本可更早拦截。

### Blockers Encountered

| Blocker | Duration | Resolution | Prevention |
|---------|----------|------------|------------|
| ICityControlAuthority 接口缺失（11-4） | 分钟级 | 按 story 指示补最小实现于 Domain.City | epic-004 回填 ADR-0008 契约，或建独立接缝 story |
| Outcome 命名空间冲突（12-2） | 分钟级 | 改名 HistoricalOutcome | registry 跨系统命名校验前置 |

### Estimation Accuracy

| Story | Estimated | Actual | Variance | Likely Cause |
|------|-----------|--------|----------|--------------|
| 11-4 太守开局守城 | 1d | 偏高（含补 ADR-0008 契约） | 约 +0.3d | Integration + 跨 epic 接缝补缺 |
| 12-2 历史事件触发门 | 1d | 准 | ~0 | ADR-0007 算法明确，直译 |
| 其余 9 story | 0.5–1d | 准 | ~0 | 骨架/逻辑边界清晰 |

**Overall**：估算准确（绝大多数 ±20% 内）。Logic story 估算可靠；Integration story 因跨 epic 接缝有轻微低估倾向。

### Carryover Analysis

无 carryover——11/11 全完成。

### Technical Debt Status
- TODO: 0（previous: n/a）
- FIXME: 0
- HACK: 0
- Trend: 清洁。Domain 层零 debt 标记。
- 关注项（非 debt 标记，已登记 ADVISORY）：① story 模板测试路径 vs 工程实际约定不一致；② 存档三段物理整合待 epic-009；③ CitySeed 配置占位待世界模型权威源。

### Previous Action Items Follow-Up

无（Sprint 01 未产 retrospective 文件）。

### Action Items for Next Iteration

| # | Action | Owner | Priority | Deadline |
|---|--------|-------|----------|----------|
| 1 | 统一 story 模板测试路径约定为 `tests/unit/ThreeKingdom.Domain.Tests/[System]/`（改 create-stories 模板或文档），消除逐 story 注明 | producer | Med | 下个 sprint 规划前 |
| 2 | 建存档信封整合 story：三段（生涯/世界/战役）合一 + 原子写 + schema/指纹统一校验（GDD_013，epic-009 范围） | technical-director | Med | epic-012 收尾后 |
| 3 | epic-004 回填 ADR-0008 控制权契约（或登记本 sprint 已落地的最小实现为正式归属） | producer | Low | 下个 sprint |
| 4 | registry 跨系统命名校验前置到 dev-story（防 Outcome 类命名空间碰撞复发） | technical-director | Low | 持续 |

### Process Improvements
- **接缝先识别**：sprint 规划时显式列出跨 epic 软依赖的"待补最小实现"为独立任务，而非藏在 Integration story 内临时补。
- **保持组合式扩展模式**：本 sprint "叠加于已提交命令路径、不改其 switch" 的做法降低回归——固化为约定。

### Summary
Sprint 02 是一次干净高效的 Meta 层落地：11/11 完成、556/556 绿、零缺陷、零技术债标记，确定性纪律全程有测试守门。最重要的单点改进：**把跨 epic 接缝（ADR 契约/缺失类型）在规划期识别为独立任务**，避免实现期临时补缺；其次统一测试路径约定消除文档债。epic-011（5/5）与 epic-012（6/6）均可判关闭。
